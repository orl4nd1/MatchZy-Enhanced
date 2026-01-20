---
title: Commands & ConVars
---

## Overview

This page is a **quick reference** for the most important MatchZy commands and configuration knobs.

- **Chat commands** (start with `.`) are typed in CS2 chat.
- **Console commands** (start with `css_`, `matchzy_` or `get5_`) are run from the CS2 server console or RCON.
- Many features have **both** a player‑facing chat command and an admin‑only console command.

For integration details (webhooks, demo upload, JSON config), see **Getting Started → Configuration**.

## ✨ New in v1.3.0: Enhanced Features

MatchZy Enhanced introduces several new commands and improvements for better match management:

- **Auto-Ready System**: Players can be automatically marked as ready on join (`matchzy_autoready_enabled`)
- **Enhanced Pause Controls**: New `.p` and `.up` aliases, pause limits, timeout controls
- **Side Selection Timer**: Time-limited side selection after knife round
- **`.gg` Command**: Team forfeit voting system
- **FFW System**: Automatic forfeit handling when teams disconnect

See sections below for command details and configuration options.

## Player chat commands (`.xxx`)

### Ready system & match lifecycle

| Command                           | Who can use it    | Description                                                                                                                                    |
| --------------------------------- | ----------------- | ---------------------------------------------------------------------------------------------------------------------------------------------- |
| `.ready`, `.r`                    | Players on a team | Mark yourself ready before the match starts. Triggers `player_ready` events. **Note:** With `matchzy_autoready_enabled`, players start ready. |
| `.unready`, `.notready`, `.ur`    | Players on a team | Mark yourself **not ready** again. Triggers `player_unready` events. Works even with auto-ready enabled.                                      |
| `.start`, `.force`, `.forcestart` | Admins only       | Force‑start the match immediately (skips waiting for all players to ready).                                                                   |
| `.restart`, `.rr`                 | Admins only       | Fully restart the current match/series.                                                                                                       |
| `.endmatch`, `.forceend`          | Admins only       | Force‑end and reset the current match.                                                                                                        |
| `.gg`                             | Players on a team | Vote to forfeit the match. Requires team consensus (default 80%). Opposing team wins via MatchZy’s normal match‑end flow. Can be gated behind a minimum score difference using `matchzy_gg_min_score_diff`. **New in v1.3.0**                                       |

### Pause & technical timeouts

| Command                 | Who can use it                  | Description                                                                                                                                                 |
| ----------------------- | ------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `.tech`                 | Players / admins (configurable) | Request a **technical pause** via MatchZy’s pause system (for hardware/network/config issues). Does **not** consume a CS2 tactical timeout.                 |
| `.pause`, `.p`          | Players / admins (configurable) | General MatchZy pause. **Defaults to a plugin‑managed pause** using `mp_pause_match`. If `matchzy_use_pause_command_for_tactical_pause 1` is set, it instead behaves like a tactical timeout and can consume native CS2 timeouts. Counts towards `matchzy_max_pauses_per_team` when issued by players.     |
| `.unpause`, `.up`       | Players / admins                | Request to unpause the match. Both teams must confirm unless an admin unpauses (via `.fp` / `.fup` or console), to prevent accidental or one‑sided resumes. If a native CS2 tactical timeout is active, `.unpause` immediately resumes the match. |
| `.tac`                  | Players on a team               | Start a **tactical timeout** using the **native CS2 timeout system** (shows as a tactical timeout in‑game, consumes that team’s tactical timeout budget). Also counts towards `matchzy_max_pauses_per_team` for that team when limits are enabled.   |
| `.fp`, `.forcepause`    | Admins only                     | Force‑pause the match as an admin, regardless of team votes.                                                                                                |
| `.fup`, `.forceunpause` | Admins only                     | Force‑unpause the match as an admin, immediately resuming play.                                                                                             |

#### When to use which pause

- **Use `.pause`** when your team needs a **short, coordinated break** (e.g. quick bio break, brief reset between rounds) and you want MatchZy to manage the pause flow and unpause voting.
- **Use `.tech`** when there is a **technical problem** (PC crash, network issues, config problems, HUD bugs, etc.) and you need time to fix it **without spending a CS2 tactical timeout**.
- **Use `.tac`** when you want a **strategic timeout** that:
  - uses the **game’s native tactical timeout system**,
  - is subject to the game’s tactical timeout limits per team/map,
  - and should clearly show up to viewers/players as a “tactical timeout” on the CS2 UI/scoreboard.

#### How and when the game actually pauses

- **MatchZy pauses (`.pause`, `.tech`, `.fp`)**

  - Internally call `mp_pause_match`, which is handled by the CS2 server:
    - If triggered **during a live round**, that round is allowed to **finish normally**, and the game pauses in the **next freezetime** at the start of the following round.
    - If triggered **while already in freezetime/halftime**, the pause takes effect **immediately** and the round timer does not start until the pause is cleared.
  - No pause command rewinds or freezes the current round mid‑fight; it always respects the engine’s “pause at a safe point” behavior.

- **Tactical timeouts (`.tac`)**
  - Use the native CS2 timeout commands: `timeout_terrorist_start` / `timeout_ct_start`.
  - If triggered **during a live round**, the requested tactical timeout starts in the **next freezetime** (between rounds).
  - If triggered **while already in freezetime**, the tactical timeout begins **immediately**, consuming one of that team’s available tactical timeouts and showing up in the CS2 UI.

### Knife and side selection

| Command              | Who can use it          | Description                                                                                                                    |
| -------------------- | ----------------------- | ------------------------------------------------------------------------------------------------------------------------------ |
| `.roundknife`, `.rk` | Admins only             | Toggle whether a knife round is required for this match.                                                                       |
| `.stay`              | Knife‑round winner team | After winning knife, choose to **stay** on your current side. **Timer:** With `matchzy_side_selection_enabled`, time limited. |
| `.switch`, `.swap`   | Knife‑round winner team | After winning knife, **swap sides** with the other team. **Timer:** With `matchzy_side_selection_enabled`, time limited.      |
| `.ct`                | Knife‑round winner team | After winning knife, choose to play as **CT**. Equivalent to `.stay` if already CT, `.switch` otherwise.                      |
| `.t`                 | Knife‑round winner team | After winning knife, choose to play as **T**. Equivalent to `.stay` if already T, `.switch` otherwise.                        |

### Practice / tactics utilities

| Command                                                                                                                   | Who can use it  | Description                                                                                          |
| ------------------------------------------------------------------------------------------------------------------------- | --------------- | ---------------------------------------------------------------------------------------------------- |
| `.prac`, `.tactics`                                                                                                       | Admins only     | Enter practice mode (lineup training, bots, utilities).                                              |
| `.exitprac`                                                                                                               | Admins only     | Exit practice mode and return to match mode.                                                         |
| `.bot`, `.cbot`, `.crouchbot`, `.boost`, `.crouchboost`                                                                   | Players in prac | Spawn helper bots (standing/crouched/boost) at your position for practicing nades and boosts.       |
| `.nobots`                                                                                                                 | Players in prac | Remove all practice bots spawned by MatchZy.                                                         |
| `.spawn`, `.ctspawn`, `.tspawn`                                                                                           | Players in prac | Teleport to a recorded spawn by round index for your team / CT / T spawns.                          |
| `.bestspawn`, `.worstspawn`, `.bestctspawn`, `.worstctspawn`, `.besttspawn`, `.worsttspawn`                               | Players in prac | Teleport to the closest / furthest competitive spawn relative to your current position.             |
| `.showspawns`, `.hidespawns`                                                                                              | Players in prac | Visualize all competitive spawns with beams (`.showspawns`) or hide them again (`.hidespawns`).     |
| `.impacts`, `.traj`, `.pip`                                                                                               | Players in prac | Toggle CS2’s `sv_showimpacts` and `sv_grenade_trajectory_prac_pipreview` helpers.                    |
| `.noflash`, `.noblind`                                                                                                    | Players in prac | Toggle “no flash” for yourself while practicing.                                                     |
| `.ff`, `.fastforward`                                                                                                     | Players in prac | Fast‑forward the round in practice mode.                                                             |
| `.clear`                                                                                                                  | Players in prac | Clear grenades / helpers in practice mode.                                                           |
| `.savepos`, `.loadpos`                                                                                                    | Players in prac | Save your current position/angles and later teleport back to that saved position.                   |
| `.last`, `.back`, `.throwindex`, `.lastindex`, `.delay`                                                                   | Players in prac | Navigate nade‑throw history: jump to last throw, go back by index, rethrow by index, add a delay.   |
| `.throw`, `.rethrow`, `.throwsmoke`, `.rethrowsmoke`, `.throwflash`, `.rethrowflash`, `.throwgrenade`, `.rethrowgrenade`, `.throwmolotov`, `.rethrowmolotov`, `.throwdecoy`, `.rethrowdecoy` | Players in prac | Re‑execute your last grenade of the given type from the same position/angles.                        |
| `.timer`                                                                                                                  | Players in prac | Start/stop a per‑player practice timer and show the result (e.g. for nade lineups or movement).     |
| `.god`                                                                                                                    | Players in prac | Toggle “god” HP in practice (gives very high HP so you don’t die while testing lineups).            |
| `.break`                                                                                                                  | Players in prac | Break all breakable entities (e.g. windows) on the map.                                              |
| `.t`, `.ct`, `.spec`, `.fas`, `.watchme`                                                                                  | Players in prac | Switch to T/CT/spec, or move everyone else to spectator to watch your POV (`.fas` / `.watchme`).    |

### Miscellaneous

| Command                             | Who can use it  | Description                                                                  |
| ----------------------------------- | --------------- | ---------------------------------------------------------------------------- |
| `.whitelist`                        | Admins only     | Toggle the match whitelist (only configured players may stay on the server). |
| `.globalnades`                      | Admins only     | Toggle whether saved lineups are global or per‑player.                       |
| `.settings`                         | Admins only     | Show current match settings (knife, ready required, playout).                |
| `.playout`                          | Admins only     | Toggle whether the match plays out all regulation rounds once clinched.      |
| `.reloadmap`                        | Admins only     | Reload the current map via `changelevel` while keeping MatchZy state sane.   |
| `.reload_admins`                    | Admins only     | Reload MatchZy’s admin list from disk.                                       |
| `.reload_config`                    | Admins only     | Reload MatchZy’s core plugin config (`cfg/MatchZy/config.cfg`) from disk. Blocked while a match is live for safety. |
| `.help`                             | Anyone          | Show a help message listing common MatchZy commands.                         |
| `.asay`                             | Admins only     | Say a message with the admin chat prefix.                                    |
| `.match`                            | Admins only     | Start match mode manually.                                                   |
| `.uncoach`                          | Coaches only    | Stop coaching and return to normal player state (match mode only).           |
| `.version`, `.matchzyversion`       | Anyone          | Print the running MatchZy version in chat.                                   |
| `.te`, `.testevent`                 | Admins only     | Send a `test_event` to the configured remote log URL for diagnostics.        |

## Server console commands (`css_...`)

Almost every chat command has a **console equivalent** for admins or scripts. The pattern is:

- Chat `.ready` ⇢ Console `css_ready`
- Chat `.pause` ⇢ Console `css_pause`
- Chat `.prac` ⇢ Console `css_prac` (via dedicated handlers)

Some of the most important console commands:

### Match control

- `css_match` – start match mode.
- `css_ready`, `css_unready` – mark a specific player ready/unready (when invoked as that player).
- `css_start` – force‑start the match.
- `css_endmatch`, `get5_endmatch`, `css_forceend` – end and reset the current match.
- `css_restart`, `css_rr` – restart the current match/series.
- `css_map <mapname>` – change to a specific map.
- `css_rmap` – reload the current map.
 - `matchzy_reload_config` – reload `cfg/MatchZy/config.cfg` from disk. Only allowed when no match is currently live.

### Pause & timeout

- `css_tech` – technical pause.
- `css_pause` – standard pause (or tactical, depending on settings).
- `css_fp`, `css_forcepause`, `sm_pause` – admin force‑pause.
- `css_fup`, `css_forceunpause`, `sm_unpause` – admin force‑unpause.
- `css_tac` – tactical timeout using the game’s native timeout system.

### Team & whitelist

- `css_team1 <name>`, `css_team2 <name>` – set display names for team1/team2.
- `css_whitelist`, `css_wl` – toggle whitelist.
- `css_save_nades_as_global`, `css_globalnades` – toggle global vs per‑player saved lineups.

### RCON passthrough & admin tools

- `css_rcon <command>` – run an arbitrary server command (RCON‑like) via MatchZy.
- `css_help` – show a help message with commands.
- `matchzy_version`, `css_matchzy_version`, `css_version` – print the current MatchZy version.
- `reload_admins` – reload MatchZy’s admin list from disk.

## Integration & automation commands

These are the **most important knobs** when using MatchZy with an external controller like MatchZy Auto Tournament.

### Remote log / webhook events

- `matchzy_remote_log_url <url>` / `get5_remote_log_url <url>`  
  If set, MatchZy will POST JSON events (match start, ready events, round events, disconnects, etc.) to this URL.
- `matchzy_remote_log_header_key <name>` / `matchzy_remote_log_header_value <value>`  
  Optional custom header (for example `X-MatchZy-Token`) to authenticate event POSTs.
- `css_te`, `css_testevent`  
  Send a `test_event` payload to the current `matchzy_remote_log_url` to verify connectivity.

### Demo upload

- `matchzy_demo_upload_url <url>` / `get5_demo_upload_url <url>`  
  If set, MatchZy uploads GOTV demo files to this URL after each map.
- `matchzy_demo_upload_header_key <name>`, `matchzy_demo_upload_header_value <value>`  
  Optional header (for example `X-MatchZy-Token`) used when uploading demos.

### Remote backup upload

- `matchzy_remote_backup_url <url>` / `get5_remote_backup_url <url>`  
  Send periodic JSON backup files (match state snapshots) to this URL. Leave empty to disable.
- `matchzy_remote_backup_header_key <name>`, `matchzy_remote_backup_header_value <value>`  
  Optional custom HTTP header used when posting backup payloads (for example an auth token).

### Match loading (when not fully controlled by Auto Tournament)

- `matchzy_loadmatch <file>`  
  Load a match from a local JSON file under the CS2 `csgo` directory.
- `matchzy_loadmatch_url <url> [header_name] [header_value]` / `get5_loadmatch_url`  
  Load a match JSON from an HTTP endpoint (typically your tournament platform).

### Roster management (advanced)

- `matchzy_addplayer <steam64> <team1|team2|spec> "<name>"` / `get5_addplayer`  
  Add a player or spectator to the in‑memory match config during match setup. Fails if the SteamID
  is invalid, the player is already assigned, or you try to modify the roster during halftime.
- `matchzy_removeplayer <steam64>` / `get5_removeplayer`  
  Remove a player from all teams in the current match config (and kick them from the server if
  they are connected). Only available while a match is set up and not in the halftime phase.

### Status and health checks

- `get5_status`  
  Print a JSON blob describing the current match state, scores, map info and various Get5‑style
  fields. Intended for external controllers and monitoring tools.
- `get5_web_available`  
  Return a small JSON object with the current game state as an integer, for legacy Get5 web
  integrations checking whether the plugin is active.
- `matchzy_check_for_updates`  
  Ask the built‑in auto‑updater to query Steam and report whether this CS2 server is up to date.
  This is informational only and does **not** trigger a restart by itself.

## MatchZy convars (`matchzy_...`)

All MatchZy convars can be set in `cfg/MatchZy/config.cfg` and changed at runtime via RCON.

### Core behavior & quality‑of‑life

- `matchzy_smoke_color_enabled` – enable player‑specific smoke colors (mainly for practice/simulation).
- `matchzy_enable_tech_pause` – master toggle for the `.tech` technical pause command.
- `matchzy_tech_pause_flag` – CSSharp permission flag required to use `.tech` (empty = default permissions).
- `matchzy_tech_pause_duration` – default technical pause duration in seconds.
- `matchzy_max_tech_pauses_allowed` – how many technical pauses each team may use.
- `matchzy_everyone_is_admin` – if `true`, treat all players as admins (useful for local testing only).
- `matchzy_show_credits_on_match_start` – show a “MatchZy plugin by …” credit line when a match starts.
- `matchzy_debug_chat` – if `true`, show debug/event logs (webhook success/failure etc.) in in‑game chat.
- `matchzy_debug_console` – if `true` (default), write verbose debug logs (ready system, match start decisions, pauses, `.gg`, FFW, config reload, etc.) to the **server console** for easier issue reporting.
- `matchzy_hostname_format` – template for the server hostname (e.g. `{TEAM1} vs {TEAM2}`).
- `matchzy_enable_damage_report` – enable post‑round damage reports in chat.
- `matchzy_stop_command_no_damage` – if `true`, disable `.stop` once any player has dealt damage that round.
- `matchzy_match_start_message` – custom chat message to broadcast when the match goes live (`$$$` = newline).

### Match defaults & safety

- `matchzy_whitelist_enabled_default` – whether whitelist is enabled by default when the plugin starts.
- `matchzy_knife_enabled_default` – whether a knife round is required by default.
- `matchzy_playout_enabled_default` – whether “play full number of rounds” is enabled by default.
- `matchzy_save_nades_as_global_enabled` – if `true`, saved lineups are global by default instead of per‑player.
- `matchzy_kick_when_no_match_loaded` – if `true`, kick players when no match is loaded and block new joins.
- `matchzy_reset_cvars_on_series_end` – restore cvars modified by the match config when the series ends.
- `matchzy_minimum_ready_required` – default number of ready players required to start a match.
- `matchzy_stop_command_available` – enable or disable `.stop` as a round‑restore tool.
- `matchzy_use_pause_command_for_tactical_pause` – make `.pause` act as a tactical pause instead of a pure tech pause.
- `matchzy_pause_after_restore` – automatically pause the match after a round restore completes.
- `matchzy_autostart_mode` – what to start on map load: `0`=nothing, `1`=match mode, `2`=practice mode.
- `matchzy_allow_force_ready` / `get5_allow_force_ready` – allow use of `.forceready` / `css_readyrequired` helpers.
- `matchzy_max_saved_last_grenades` – per‑player history length for saved grenade throws (0 = disabled).

### Demo recording & upload

- `matchzy_demo_recording_enabled` – automatically start GOTV demo recording when a match goes live.
- `matchzy_demo_path` – relative path (under `csgo/`) where demos should be stored (must end with `/`).
- `matchzy_demo_name_format` – filename template for demos (supports tokens like date, match ID, teams).
- `matchzy_demo_upload_url`, `get5_demo_upload_url` – HTTP endpoint to upload demos after each map.

### Chat & UI

- `matchzy_chat_prefix` – prefix for standard MatchZy chat messages (supports `{Green}`, `{Default}`, etc.).
- `matchzy_admin_chat_prefix` – prefix for admin `.asay` messages.
- `matchzy_chat_messages_timer_delay` – seconds between repeated reminder messages (unready, paused, etc.).

### Backups, reports & automation

- `matchzy_remote_backup_url`, `get5_remote_backup_url` – HTTP endpoint for periodic JSON backup uploads.
- `matchzy_remote_backup_header_key`, `matchzy_remote_backup_header_value` – optional header name/value for backups.
- `matchzy_report_endpoint` – HTTP endpoint to receive MatchZy match reports.
- `matchzy_report_server_id` – server identifier attached to match report uploads.
- `matchzy_report_token` – authentication token sent as an HTTP header for match report uploads.

### Tournament status (read‑mostly)

These are maintained by MatchZy / external controllers and are usually not edited by hand:

- `matchzy_tournament_status` – current server state (`idle`, `loading`, `warmup`, `knife`, `live`, `paused`, `halftime`, `postgame`, `error`).
- `matchzy_tournament_match` – match slug/identifier currently loaded on this server.
- `matchzy_tournament_updated` – Unix timestamp of the last tournament status update.
- `matchzy_tournament_next_match` – slug/identifier of the next match queued for this server.

## Where to go next

- **Getting Started → Configuration** – JSON config format, integration URLs, and simulation mode flags.
- **Plugin Docs → Demo upload API** – exact request/response contract for demo uploads.
- **Plugin Docs → Config loading behavior** – when each config file is executed during the match lifecycle.

## Command details (linkable)

Use these sections when you want to share a **direct link to a specific command**.  
Each heading becomes an anchor, for example: `.../commands/#gg` or `.../commands/#pause--p`.

### .gg

Team forfeit vote:

- **Chat:** `.gg`  
- **Who:** Players on a team  
- **Behavior:**  
  - Counts a forfeit vote for the caller’s team.  
  - When the fraction of connected teammates who have voted reaches `matchzy_gg_threshold`, MatchZy ends the match and awards the win to the opposing team via the normal match‑end flow.  
  - Can be further restricted with `matchzy_gg_min_score_diff` so that `.gg` is only allowed once your team is losing by at least N rounds (for example, `6` for “only at 0–6 or worse”).  

Related convars: `matchzy_gg_enabled`, `matchzy_gg_threshold`, `matchzy_gg_min_score_diff`.

### .pause / .p

General pause command:

- **Chat:** `.pause`, `.p`  
- **Console:** `css_pause`  
- **Who:** Players / admins (configurable via permissions)  
- **Behavior:**  
  - By default uses MatchZy’s pause system and calls `mp_pause_match`.  
  - If `matchzy_use_pause_command_for_tactical_pause 1`, `.pause` instead behaves like a **tactical timeout**, consuming native CS2 timeouts.  
  - When triggered by players, counts toward `matchzy_max_pauses_per_team` (shared limit across pause types).  

Related convars: `matchzy_use_pause_command_for_tactical_pause`, `matchzy_max_pauses_per_team`, `matchzy_pause_duration`, `matchzy_both_teams_unpause_required`.

### .unpause / .up

Unpause request:

- **Chat:** `.unpause`, `.up`  
- **Console:** none (use `css_fup` / `css_forceunpause` for admin force‑unpause)  
- **Who:** Players / admins  
- **Behavior:**  
  - In normal pauses, both teams must confirm if `matchzy_both_teams_unpause_required 1` and the caller is not an admin.  
  - Admins (or force‑unpause commands) immediately resume the match.  
  - If a **native CS2 tactical timeout** is active, `.unpause` immediately clears it even if pause voting would normally be required.  

Related convars: `matchzy_both_teams_unpause_required`, `matchzy_pause_duration`.

### .tech

Technical pause:

- **Chat:** `.tech`  
- **Console:** `css_tech`  
- **Who:** Players / admins (controlled by `matchzy_tech_pause_flag`)  
- **Behavior:**  
  - Uses MatchZy’s pause system for **technical issues** and does **not** spend native CS2 tactical timeouts.  
  - Each team has a limited number of tech pauses via `matchzy_max_tech_pauses_allowed`.  

Related convars: `matchzy_enable_tech_pause`, `matchzy_tech_pause_flag`, `matchzy_tech_pause_duration`, `matchzy_max_tech_pauses_allowed`.

### .tac

Native tactical timeout:

- **Chat:** `.tac`  
- **Console:** `css_tac`  
- **Who:** Players on a team  
- **Behavior:**  
  - Uses CS2’s **built‑in tactical timeout system** (`timeout_terrorist_start` / `timeout_ct_start`).  
  - Subject to native game limits such as `mp_team_timeout_max`.  
  - Also counts against MatchZy’s `matchzy_max_pauses_per_team` when that limit is enabled.  

Related convars: `matchzy_max_pauses_per_team` (plugin‑side), Valve timeout cvars (game‑side).

### .reload_config

Reload MatchZy’s core plugin configuration:

- **Chat:** `.reload_config`  
- **Console:** `matchzy_reload_config`  
- **Who:** Admins only  
- **Behavior:**  
  - Re‑executes `cfg/MatchZy/config.cfg` so you can apply convar changes without restarting the server.  
  - For safety, the command is **blocked while a match is live**; use it between matches or on idle servers.  

Related files: `cfg/MatchZy/config.cfg`.
