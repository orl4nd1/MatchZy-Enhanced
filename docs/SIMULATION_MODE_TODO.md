### Simulation Mode – Implementation TODO

This document tracks the work required to implement the **simulation mode** feature described in [MatchZy issue #1](https://github.com/sivert-io/MatchZy/issues/1).  
Goal: allow MatchZy to run a full match lifecycle driven by bots while exposing **canonical player identities from the JSON config** (SteamIDs and names) so that external platforms see the match as if real players played it.

---

### 1. Config & Internal State Extensions

- **Decision: Config flag name**

  - **Use `simulation: boolean`** in the incoming JSON match config.
  - Internal name: `Simulation` on `MatchConfig`, plus a plugin-level `bool isSimulationMode`.
  - **Backward compatibility:** if `simulation` is absent or false, behavior is identical to today.

- **Tasks**
  - **1.1 – Extend `MatchConfig`**
    - Add a new property to `MatchConfig` (`MatchConfig.cs`):
      - `[JsonPropertyName("simulation")] public bool Simulation { get; set; } = false;`
    - This must be purely optional and default to `false`.
  - **1.2 – Parse `simulation` from JSON**
    - In `GetOptionalMatchValues` (`MatchManagement.cs` / `LoadMatchFromJSON` helpers):
      - If `jsonDataObject["simulation"]` exists:
        - Parse as boolean (accept `"true"/"false"` and real JSON booleans).
        - On parse failure, log a clear error and fail `LoadMatchFromJSON` with `UpdateTournamentStatus("error")`.
  - **1.3 – Track simulation state on the plugin**
    - In `MatchZy` partial class (`MatchZy.cs` / `MatchManagement.cs`):
      - Add a field `public bool isSimulationMode = false;`
      - In `LoadMatchFromJSON`, after `matchConfig` is constructed and `GetOptionalMatchValues` is applied:
        - Set `isSimulationMode = matchConfig.Simulation;`
      - Ensure `ResetMatch(...)` and any global cleanup resets `isSimulationMode` to `false`.

---

### 2. Core Simulation Flow Entry Point ✅ (implemented)

High-level behavior: when `simulation: true`, MatchZy should **not wait for real human players**; it should **spawn bots, map them to configured players**, and then drive the normal match lifecycle (warmup → knife/side selection → live → map_result → series_end).

- **Decision: Keep lifecycle identical, but auto-drive it with fake “ready” events**

  - Reuse the existing flow in `LoadMatchFromJSON`:
    - `ExecuteChangedConvars()` → `StartWarmup()` → ready system → `StartKnifeRound()` / `StartLive()`.
  - In simulation mode, **do not require human `.ready` inputs or chat commands**:
    - Programmatically mark each simulated player as ready and emit `player_ready` / `team_ready` / `all_players_ready` events as if they had typed `!ready`.
    - Optionally **stagger** these ready events with a small delay per simulated player (for realism).

- **Status / Implementation notes**
  - `LoadMatchFromJSON` now calls `MaybeStartSimulationFlow()` after `StartWarmup()` and `isMatchSetup = true;` when `matchConfig.Simulation` is `true`.
  - `MaybeStartSimulationFlow` (in `SimulationMode.cs`):
    - Builds the simulation identity pool from `team1.players` / `team2.players`.
    - Calls `SpawnSimulationBots()` (section 3).
    - After a short delay, calls `StartSimulationReadyFlow()` which:
      - Schedules `OnPlayerReady(player, null)` for each simulated player with a staggered delay (0.5s steps).
      - Lets the existing ready system emit `player_ready`, `team_ready`, `all_players_ready` and transition into live as usual.
  - All simulation orchestration is guarded by `if (!isSimulationMode) return;` and non-simulation matches are unaffected.

---

### 3. Bot Spawning & Team Population ✅ (implemented)

Requirement: replace the “wait for players to connect” step with automatic **bot spawning**, assigning bots to `team1` and `team2` according to the JSON config.

- **Decision: Use CS2 server commands, do not require human anchor players**

  - Use console commands (similar to `PracticeMode.cs`) to spawn bots:
    - `bot_kick` to clean up any pre-existing bots.
    - `bot_join_team T` / `bot_join_team CT`.
    - `bot_add_t` / `bot_add_ct`.
  - Avoid practice-specific state (`isPractice`, `pracUsedBots`) for simulation; keep simulation logic separate but reuse patterns/commands where useful.

- **Status / Implementation notes**
  - `SpawnSimulationBots()` (in `SimulationMode.cs`) now:
    - Executes `bot_kick` and sets `mp_autoteambalance 0; mp_limitteams 0; mp_autokick 0; bot_quota_mode normal; bot_quota 0`.
    - Iterates the simulation identity pool and spawns one bot per configured player:
      - Chooses CT/T based on `teamSides[matchzyTeam1]` / `teamSides[matchzyTeam2]` and the identity’s `TeamSlot`.
      - Uses `bot_join_team CT/T` + `bot_add_ct` / `bot_add_t` for each identity.
  - A 3 second `AddTimer` gives bots time to connect and be mapped before the simulated ready flow starts.

---

### 4. Simulation Player Identity Mapping ✅ (implemented)

Requirement: maintain a mapping between **configured players** (from JSON) and the actual in-game **bot entities** (userid / controller) so that all events and reports use the configured SteamIDs and names.

- **Decision: Use an explicit mapping layer**

  - Define a simple internal DTO, e.g.:
    - `record SimulationPlayerIdentity(string ConfigSteamId, string ConfigName, string TeamSlot);`
      - `TeamSlot` is `"team1"` or `"team2"` and ties into existing event/report schemas.
  - Maintain:
    - `Dictionary<int, SimulationPlayerIdentity> simulationPlayersByUserId;` (key: `CCSPlayerController.UserId`).
    - Optionally `Dictionary<string, SimulationPlayerIdentity> simulationPlayersByBotSteamId;` if helpful for lookups.

- **Status / Implementation notes**
  - `SimulationPlayerIdentity` (`SimulationMode.cs`) holds `(ConfigSteamId, ConfigName, TeamSlot)`.
  - `BuildSimulationConfigPlayers()` builds a flat `simulationIdentityPool` from the `team1`/`team2` JSON maps.
  - `AssignSimulationIdentityForBot` is called from `EventPlayerConnectFullHandler` when a simulation bot connects:
    - Skips whitelist kicking for simulation bots.
    - Assigns the next unused identity and populates `simulationPlayersByUserId[userId]`.
  - On disconnect, `EventPlayerDisconnectHandler` removes the mapping for that userId.
  - `ClearSimulationState()` is invoked from `ResetMatch` to clear mappings and flags between matches.
  - **4.4 – Edge cases**
    - If more bots connect than configured players:
      - Extra bots should **not** receive a `SimulationPlayerIdentity`.
      - Events and stats for those extra bots can be suppressed or mapped to generic “anonymous” placeholders if needed in the future (out of scope for first version).
    - If fewer bots than configured players:
      - Log a warning indicating “Not enough bots spawned to cover all configured simulation players”.
      - Still run the match; stats for unassigned players will be zeroed.

---

### 5. Event & Webhook Identity Rewriting ✅ (partially implemented)

Requirement: in simulation mode, **all outgoing events that reference players** must use the **configured SteamID and name** from the JSON, not the underlying bot SteamID or name.

- **Decision: Centralize identity resolution in helper(s)**

  - Introduce a helper method in a shared partial of `MatchZy`, e.g.:
    - `private MatchZyPlayerInfo BuildPlayerInfo(CCSPlayerController player, string teamLabelFallback = "none")`
      - If `!isSimulationMode` or no mapping → use existing behavior:
        - SteamId = `player.SteamID.ToString()`
        - Name = `player.PlayerName`
        - Team = derived from `reverseTeamSides` / `teamLabelFallback`
      - If `isSimulationMode` and `simulationPlayersByUserId` contains this `UserId`:
        - SteamId = `SimulationPlayerIdentity.ConfigSteamId`
        - Name = `SimulationPlayerIdentity.ConfigName`
        - Team = `SimulationPlayerIdentity.TeamSlot` (or human-readable team name if needed).

- **Status / Implementation notes**
  - `BuildPlayerInfo` is implemented and used in:
    - `EventPlayerConnectFullHandler` (`player_connect`).
    - `EventPlayerDisconnectHandler` (`player_disconnect`).
    - `SendPlayerReadyEvent` (`player_ready` / `player_unready`).
  - In simulation mode, these events now expose the configured SteamID/name and team slot for bots.
  - **Remaining**:
    - Audit any other `MatchZyPlayerInfo` usages (e.g. pause-related events) and route them through `BuildPlayerInfo`.
    - If there are player-centric kill/bomb events with explicit SteamID/name fields, consider using the mapping there as well.

---

### 6. Match Report & Stats Alignment ✅ (implemented)

Requirement: match reports and stats must also reflect **configured SteamIDs and names** so that the management platform can treat simulation matches exactly like real ones.

- **Decision: Rewrite connection and stats identities via mapping**

  - Keep the **shape** of the match report as-is (`MatchReportPayload`, `MatchReportTeam`, `MatchReportPlayerConnection`, `StatsPlayer`, etc.).
  - For simulation matches, adapt how we **populate** those objects.

- **Status / Implementation notes**
  - `BuildConnectionSnapshots` now:
    - In sim mode, if a mapping exists for `UserId`, uses `identity.ConfigSteamId` / `identity.ConfigName` and `identity.TeamSlot` for `SteamId`, `Name`, and `Slot`.
    - Keeps `TeamSide` consistent via `ResolvePlayerTeamSide`.
  - `ResolvePlayerSlot` prefers the simulation mapping’s `TeamSlot` when `isSimulationMode` is true, otherwise falls back to existing behavior.
  - `GetPlayerStatsDict` now:
    - In sim mode, prefers `identity.ConfigSteamId`/`ConfigName` and, when parsable, uses `ConfigSteamId` as the primary `ulong` key.
    - Builds `StatsPlayer` instances with configured SteamID and name when mapped, preserving previous behavior for non-sim matches.
  - No schema changes were required for match report uploads – only identifiers in the payload are different in simulation mode.

---

### 7. Optional: Simulation Metadata Flag

Requirement (optional): provide a **non-breaking** way for consumers to distinguish simulation matches from real ones.

- **Decision: Add a `simulated` flag to the match report only**

  - Extend `MatchReportMatch` with:
    - `public bool Simulated { get; init; }`
  - Set `Simulated = isSimulationMode` when building the match section in `BuildMatchReport`.
  - This is optional and should default to `false` for non-simulation matches.

- **Tasks**
  - **7.1 – Extend `MatchReportMatch`**
    - Add `public bool Simulated { get; init; }` with default `false`.
  - **7.2 – Populate from runtime state**
    - In `BuildMatchReport`, set:
      - `Simulated = isSimulationMode;`
  - **7.3 – Backward compatibility**
    - Ensure existing consumers that ignore unknown fields continue to work.
    - No changes to event classes (`MatchZy*Event` types) are required for this metadata flag.

---

### 8. Ready System & Lifecycle Integration ✅ (implemented)

Requirement: in simulation mode, matches should **progress automatically** without human `.ready` commands, but still honor the expected event flow.

- **Decision: Use existing ready system with auto-ready behavior**

  - Do not introduce a separate lifecycle; instead, auto-satisfy ready conditions using current functions.

- **Status / Implementation notes**
  - `StartSimulationReadyFlow`:
    - Collects all mapped simulation userIds, sorts them, and for each schedules `OnPlayerReady(player, null)` at `i * 0.5s`.
    - This reuses the existing `.ready` path, emitting `player_ready` and letting the normal ready logic drive state.
  - After all timers:
    - Sets `teamReadyOverride[CT]` and `teamReadyOverride[T]` to `true`.
    - Calls `CheckAndSendTeamReadyEvent()` so `team_ready` / `all_players_ready` events fire and the existing logic transitions into live.
  - `GetRealPlayersCount` is unchanged; in sim mode it effectively counts bots as the “real players” for the purposes of warmup auto-start, which is acceptable for now.

---

### 9. Edge Cases & Failure Handling ✅ (implemented)

- **Status / Implementation notes**
  - **Missing/malformed config**:
    - `LoadMatchFromJSON` now validates that when `simulation: true`, at least one of `team1.players` / `team2.players` has configured players; otherwise it logs an error, sets tournament status to `"error"`, and returns `false` (match load fails).
  - **Bot spawn/mapping failure**:
    - `StartSimulationReadyFlow` checks `simulationPlayersByUserId.Keys`:
      - If zero, logs that no mapped simulation players were found, sets tournament status to `"error"`, disables `isSimulationMode`, and aborts the simulation flow.
      - If fewer than the configured identities, logs a warning but continues.
  - **Unexpected bot kicks / reconnects**:
    - On disconnect, any simulation mapping for that `userId` is removed; if a new bot joins later it can be assigned a free identity again.
  - Throughout, external identities remain stable for the duration of each bot’s connection, and simulation state is fully cleared on `ResetMatch`.

---

### 10. Validation & Acceptance Checklist

- **Config behavior**
  - [ ] `simulation` flag is parsed and stored correctly on `MatchConfig`.
  - [ ] When `simulation` is absent/false, MatchZy behaves exactly as before.
- **Lifecycle & bots**
  - [ ] In simulation mode, bots are spawned for both teams matching `team1.players` / `team2.players` counts.
  - [ ] Match progresses through warmup → knife (if configured) → live → overtime → map_result → series_end automatically.
- **Events & stats**
  - [ ] All events that include `player`, `attacker`, `victim`, `assister`, etc., expose **configured** SteamIDs and names.
  - [ ] Round stats and map stats (`round_end`, `map_result`) are keyed by configured SteamIDs and names.
  - [ ] Match report (`matchzy_match_report` and automatic uploads) use configured identities in `Connections`, `Teams[teamX].Players`, and any stats sections.
- **Metadata**
  - [ ] Optional `simulated: true` flag is present in match report for simulation matches and `false`/absent for others.
- **Stability**
  - [ ] Bot join/leave edge cases do not crash the plugin.
  - [ ] Simulation mode can be run repeatedly across matches/maps without leaking state.
