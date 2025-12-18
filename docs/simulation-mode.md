---
title: Simulation mode
---

## Why simulation mode?

Simulation mode lets you **run full matches with bots** while:

- Driving the normal MatchZy lifecycle (warmup → knife/side → live → results).
- Emitting the **same events and reports** you would see with real players.
- Using **canonical player identities** (SteamIDs + names) taken from the JSON match config.

From the perspective of **[MatchZy Auto Tournament](https://mat.sivert.io)** and any external systems, the match looks like a real player match, except for an optional `simulated` flag in reports.

This is useful for:

- Testing platform features (brackets, reports, demo handling) without players.
- Demoing your platform to others.
- Load‑testing and validating flows end‑to‑end.

## Enabling simulation mode

Simulation is controlled **per match** via the JSON match config:

```json
{
  "matchid": 12345,
  "num_maps": 1,
  "maplist": ["de_inferno"],
  "team1": {
    "id": "team-1",
    "name": "Team One",
    "players": {
      "STEAM_1:1:111": "Player One",
      "STEAM_1:1:222": "Player Two",
      "STEAM_1:1:333": "Player Three",
      "STEAM_1:1:444": "Player Four",
      "STEAM_1:1:555": "Player Five"
    }
  },
  "team2": {
    "id": "team-2",
    "name": "Team Two",
    "players": {
      "STEAM_1:1:666": "Player Six",
      "STEAM_1:1:777": "Player Seven",
      "STEAM_1:1:888": "Player Eight",
      "STEAM_1:1:999": "Player Nine",
      "STEAM_1:1:1010": "Player Ten"
    }
  },
  "simulation": true
}
```

- `simulation: true` – turns on simulation mode for that match.
- If `simulation` is **missing or false**, MatchZy behaves exactly like the non‑simulation plugin.

### Requirements

- At least one of `team1.players` or `team2.players` must be non‑empty.
  - If both are empty/missing, MatchZy logs an error and refuses to start the match.
- It’s best if the number of players per team matches:
  - `players_per_team` in the JSON (or default in config).
  - The actual game mode (5v5, 2v2 wingman, etc.).

## How simulation mode works

When a match with `simulation: true` is loaded:

1. **Match config is parsed** as usual into internal `MatchConfig`.
2. MatchZy sets an internal flag: `isSimulationMode = true`.
3. It builds an internal list of **simulation identities** from the JSON:
   - For each player in `team1.players` and `team2.players`:
     - `ConfigSteamId`, `ConfigName`, `TeamSlot` (`"team1"` / `"team2"`).
4. MatchZy starts warmup and then calls the simulation orchestrator:
   - `BuildSimulationConfigPlayers()` – records all configured players.
   - `SpawnSimulationBots()` – spawns bots to represent those players.
   - After a short delay, `StartSimulationReadyFlow()` – makes them “ready up”.

### Bot spawning

For each configured player, MatchZy:

- Kicks any existing bots and relaxes team limits:
  - `bot_kick`
  - `mp_autoteambalance 0; mp_limitteams 0; mp_autokick 0;`
- Determines the **desired side** based on:
  - `TeamSlot` (`team1`/`team2`)
  - The current `teamSides` mapping (CT/T assignments for team1/team2)
- Executes commands:
  - If team should start CT: `bot_join_team CT; bot_add_ct`
  - If team should start T: `bot_join_team T; bot_add_t`

As bots connect, MatchZy:

- Recognizes them in the player connect handler.
- Assigns each bot a `SimulationPlayerIdentity` (SteamID, name, team slot).
- Stores a mapping from CS2 `userId` → simulation identity.

### Staggered ready flow

Once bots have connected and been mapped:

1. MatchZy collects all simulated player `userId`s and sorts them.
2. It starts a timer‑driven ready flow:
   - Every **0.5 seconds** it:
     - Looks up the player by `userId`.
     - If still valid, calls the internal `OnPlayerReady(...)` handler.
   - This:
     - Marks the player as ready.
     - Sends the usual `player_ready` event to your API.
3. After all players have been “readied”:
   - Sets `teamReadyOverride[CT] = true` and `teamReadyOverride[T] = true`.
   - Calls `CheckAndSendTeamReadyEvent()` to emit team‑level ready events and allow the match to go live.

From the outside, this looks like each player typed `!ready` at slightly different times.

### Knife and side selection

In simulation mode, the plugin **does not actually play a knife round**:

- It ends warmup and emits `warmup_ended`.
- Emits `knife_round_started`.
- Randomly decides which team gets which side for the first map, based on `match_side_type`.
- Emits `knife_round_ended` with a winner (`team1`/`team2`/`none`).
- Immediately starts live (`StartLive()`).

This keeps events and side logic consistent while avoiding a real knife round.

## Identity mapping & external visibility

The key idea in simulation mode is that **all external consumers see the configured players**, not the bots:

- When building `MatchZyPlayerInfo` for events:
  - If `isSimulationMode` and a mapping exists:
    - Use `ConfigSteamId` and `ConfigName` from the simulation identity.
    - Use `TeamSlot` as the team label (`"team1"`/`"team2"`).
  - Otherwise fall back to the real player’s steam ID and name.

This is applied consistently in:

- **Player events**:
  - `player_connected`, `player_disconnected`
  - `player_ready`, `player_unready`
  - Pause/unpause events that reference a player.
- **Match reports**:
  - Player entries use the configured SteamID and name.
  - The match metadata includes `simulated: true` when simulation mode is on.
- **Stats**:
  - Player stats dictionaries are keyed by `ConfigSteamId` and reported with the configured name.

As a result, MatchZy Auto Tournament and any other consumer:

- See the same canonical player identities they would see for a real match.
- Can store and display stats as usual.

## Error handling & edge cases

Simulation mode includes some basic safeguards:

- **No configured players**

  - If `simulation: true` but both `team1.players` and `team2.players` are empty:
    - MatchZy logs an error.
    - Updates the tournament status to `"error"`.
    - Aborts loading the match.

- **No bots mapped**

  - If after spawning bots, `simulationPlayersByUserId` is empty when starting the ready flow:
    - Logs a warning.
    - Updates tournament status to `"error"`.
    - Disables `isSimulationMode` for that match.

- **Partial mapping**
  - If fewer bots are mapped than configured players:
    - Logs a warning with `X of Y` mapped.
    - Continues with the players that were successfully mapped.

## How this interacts with MatchZy Auto Tournament

From the Auto Tournament side:

- You **set `simulation: true`** in the JSON you serve for a match.
- Everything else (teams, players, maps, format) is configured exactly the same as for real matches.
- Auto Tournament:
  - Receives events and reports it already understands.
  - Can show these matches in its UI as normal, optionally marking them as simulated.

If you want to treat simulated matches differently in the UI:

- Check the `simulated` flag in the match report payload.
- Optionally add your own metadata in the match JSON that MatchZy will echo back/custom‑forward.

## Summary

- Turn on simulation by adding `"simulation": true` to the JSON match config.
- MatchZy:
  - Spawns bots to represent each configured player.
  - Drives a staggered ready flow and simulated knife decision.
  - Rewrites events, reports and stats to use the **configured** SteamIDs and names.
  - Marks reports as `simulated: true`.
- To the tournament platform, everything looks like a normal match, just without real players.
