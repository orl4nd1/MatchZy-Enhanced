---
title: Commands & ConVars
---

## Overview

This page is a **quick reference** for the most important MatchZy commands and configuration knobs.

- **Chat commands** (start with `.`) are typed in CS2 chat.
- **Console commands** (start with `css_`, `matchzy_` or `get5_`) are run from the CS2 server console or RCON.
- Many features have **both** a player‑facing chat command and an admin‑only console command.

For integration details (webhooks, demo upload, JSON config), see **Getting Started → Configuration**.

## Player chat commands (`.xxx`)

### Ready system & match lifecycle

| Command                           | Who can use it    | Description                                                                  |
| --------------------------------- | ----------------- | ---------------------------------------------------------------------------- |
| `.ready`, `.r`                    | Players on a team | Mark yourself ready before the match starts. Triggers `player_ready` events. |
| `.unready`, `.notready`, `.ur`    | Players on a team | Mark yourself **not ready** again. Triggers `player_unready` events.         |
| `.start`, `.force`, `.forcestart` | Admins only       | Force‑start the match immediately (skips waiting for all players to ready).  |
| `.restart`, `.rr`                 | Admins only       | Fully restart the current match/series.                                      |
| `.endmatch`, `.forceend`          | Admins only       | Force‑end and reset the current match.                                       |

### Pause & technical timeouts

| Command                 | Who can use it                  | Description                                                                                                                                                 |
| ----------------------- | ------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `.tech`                 | Players / admins (configurable) | Request a **technical pause** via MatchZy’s pause system (for hardware/network/config issues). Does **not** consume a CS2 tactical timeout.                 |
| `.pause`, `.p`          | Players / admins (configurable) | General MatchZy pause. Behaves as a normal pause or as a tactical pause depending on `matchzy_use_pause_command_for_tactical` and other match settings.     |
| `.unpause`, `.up`       | Players / admins                | Request to unpause the match. Both teams must confirm unless an admin unpauses (via `.fp` / `.fup` or console), to prevent accidental or one‑sided resumes. |
| `.tac`                  | Players on a team               | Start a **tactical timeout** using the **native CS2 timeout system** (shows as a tactical timeout in‑game, consumes that team’s tactical timeout budget).   |
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

| Command              | Who can use it          | Description                                                   |
| -------------------- | ----------------------- | ------------------------------------------------------------- |
| `.roundknife`, `.rk` | Admins only             | Toggle whether a knife round is required for this match.      |
| `.stay`              | Knife‑round winner team | After winning knife, choose to **stay** on your current side. |
| `.switch`, `.swap`   | Knife‑round winner team | After winning knife, **swap sides** with the other team.      |

### Practice / tactics utilities

| Command                                                 | Who can use it  | Description                                                 |
| ------------------------------------------------------- | --------------- | ----------------------------------------------------------- |
| `.prac`, `.tactics`                                     | Admins only     | Enter practice mode (lineup training, bots, utilities).     |
| `.exitprac`                                             | Admins only     | Exit practice mode and return to match mode.                |
| `.bot`, `.cbot`, `.crouchbot`, `.boost`, `.crouchboost` | Players in prac | Spawn various helper bots for practicing nades/boosts.      |
| `.nobots`                                               | Players in prac | Remove practice bots.                                       |
| `.impacts`, `.traj`, `.pip`                             | Players in prac | Enable grenade impact and trajectory visualization helpers. |
| `.noflash`, `.noblind`                                  | Players in prac | Toggle “no flash” for yourself while practicing.            |
| `.ff`, `.fastforward`                                   | Players in prac | Fast‑forward the round in practice mode.                    |
| `.clear`                                                | Players in prac | Clear grenades / helpers in practice mode.                  |

### Miscellaneous

| Command              | Who can use it  | Description                                                                  |
| -------------------- | --------------- | ---------------------------------------------------------------------------- |
| `.whitelist`         | Admins only     | Toggle the match whitelist (only configured players may stay on the server). |
| `.globalnades`       | Admins only     | Toggle whether saved lineups are global or per‑player.                       |
| `.settings`          | Admins only     | Show current match settings (knife, ready required, playout).                |
| `.help`              | Anyone          | Show a help message listing common MatchZy commands.                         |
| `.t`, `.ct`, `.spec` | Players in prac | Switch team (T, CT, spectator) in practice mode.                             |
| `.asay`              | Admins only     | Say a message with the admin chat prefix.                                    |
| `.match`             | Admins only     | Start match mode manually.                                                   |

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

## Key MatchZy convars (`matchzy_...`)

For a complete list, see `cfg/MatchZy/config.cfg`. Some important ones:

- `matchzy_everyone_is_admin` – if `true`, treat all players as admins (useful for testing).
- `matchzy_match_start_message` – custom message to broadcast when the match starts.
- `matchzy_enable_damage_report` – enable post‑round damage reports.
- `matchzy_kick_when_no_match_loaded` – if `true`, kick players when no match is loaded.
- `matchzy_demo_recording_enabled` – automatically start demo recording when a match goes live.
- `matchzy_demo_path`, `matchzy_demo_name_format` – control where demos are recorded and how they’re named.
- `matchzy_minimum_ready_required` – global default for how many players must be ready per team.
- `matchzy_playout_enabled_default` – control whether playout (play full number of rounds) is enabled by default.

These convars are read from `cfg/MatchZy/config.cfg` on startup and can be overridden at runtime via RCON.

## Where to go next

- **Getting Started → Configuration** – JSON config format, integration URLs, and simulation mode flags.
- **Plugin Docs → Demo upload API** – exact request/response contract for demo uploads.
- **Plugin Docs → Config loading behavior** – when each config file is executed during the match lifecycle.

{
"cells": [],
"metadata": {
"language_info": {
"name": "python"
}
},
"nbformat": 4,
"nbformat_minor": 2
}
