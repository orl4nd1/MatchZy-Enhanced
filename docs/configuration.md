---
title: Configuration
---

## Overview

MatchZy reads configuration from:

- **ConVars** (CounterStrikeSharp / server cvars) – control plugin behavior.
- **Config files** under `cfg/MatchZy/` – executed at specific lifecycle moments.
- **JSON match configs** – loaded from a URL (typically MatchZy Auto Tournament).

This page focuses on the parts that matter most when integrating with  
**[MatchZy Auto Tournament](https://mat.sivert.io)**.

For detailed timing of when each config file is executed, see  
`Config loading behavior` (which documents the exact flow).

## Core config files (`cfg/MatchZy/`)

By default, the plugin ships with:

- `config.cfg` – main MatchZy plugin convars.
- `live.cfg` / `live_wingman.cfg` – game settings used when a match goes live.
- `warmup.cfg` – game settings used during warmup.
- `knife.cfg` – game settings used during knife rounds.
- `sleep.cfg`, `prac.cfg`, `dryrun.cfg`, etc. – for other modes.

These are executed via `Server.ExecuteCommand("exec ...")` at specific times:

- On plugin load: `execifexists MatchZy/config.cfg`
- On match going live: `exec MatchZy/live.cfg` (or wingman equivalent)
- On warmup/practice/sleep: corresponding configs as needed.

You can safely edit these files to tune server settings for your environment.

## Important convars for Auto Tournament integration

The exact names and defaults are defined in `ConfigConvars.cs`, but these are the key ones for platform integration:

- **Match loading**
  - `matchzy_loadmatch_url`  
    Base URL used when you run `!match <slug>` (or equivalent command).  
    MatchZy will request `GET {matchzy_loadmatch_url}/{slug}.json`.
  - `matchzy_loadmatch_header_key`, `matchzy_loadmatch_header_value`  
    Optional extra header for authenticating with your Auto Tournament API (e.g. `Authorization: Bearer ...`).

- **Match report**
  - `matchzy_match_report_url`  
    If set, MatchZy will POST match reports here at the end of maps/series.
  - `matchzy_match_report_header_key`, `matchzy_match_report_header_value`  
    Optional custom header to secure the report endpoint.

- **Demo upload**
  - `matchzy_demo_upload_url`  
    If set, MatchZy will upload `.dem` files here after each map.  
    See `Demo upload API` and `Demo upload guide` for full details.
  - `matchzy_demo_upload_header_key`, `matchzy_demo_upload_header_value`  
    Optional custom header for the demo upload API.

- **Tournament status (used by the safe auto‑updater)**
  - `matchzy_tournament_status`  
    String status that MatchZy keeps up to date as the server runs:
    - `idle`, `loading`, `warmup`, `knife`, `live`, `paused`, `halftime`, `postgame`, `error`.
  - `matchzy_tournament_match`  
    Slug/identifier for the active match (e.g. `r1m1`, `bo3_final_2`).
  - `matchzy_tournament_updated`  
    Unix timestamp (as a string) for when the tournament status was last changed.

  These are primarily useful for **external automation** and for the built‑in safe auto‑updater:
  - The updater **never checks for updates or restarts** while status is one of:
    `loading`, `warmup`, `knife`, `live`, `paused`, `halftime`.
  - Once the server is `idle`, `postgame` or `error`, it:
    - Checks Steam’s `UpToDateCheck` API to see if a new CS2 build is required.
    - Logs a marker when an update is available:  
      `[MATCHZY_UPDATE_AVAILABLE] required_version=<number>`
    - Later, when it is safe to restart, it kicks human players and logs:  
      `[MATCHZY_UPDATE_SHUTDOWN] required_version=<number>`  
      before executing `quit` so your process manager can restart the server.

Consult the shipped `cfg/MatchZy/config.cfg` for the up‑to‑date list and defaults.

## JSON match config (from Auto Tournament)

When used with MatchZy Auto Tournament, MatchZy expects to load matches from a JSON document that looks roughly like:

```json
{
  "matchid": 12345,
  "num_maps": 3,
  "maplist": ["de_inferno", "de_mirage", "de_nuke"],
  "team1": {
    "id": "team-1",
    "name": "Team One",
    "players": {
      "STEAM_1:1:111": "Player One",
      "STEAM_1:1:222": "Player Two"
    }
  },
  "team2": {
    "id": "team-2",
    "name": "Team Two",
    "players": {
      "STEAM_1:1:333": "Player Three",
      "STEAM_1:1:444": "Player Four"
    }
  },
  "simulation": false
}
```

The plugin validates the structure and then maps it into an internal `MatchConfig` object.

### Important JSON fields

- `matchid` (`match_id` in internal config)  
  Numeric identifier used in logs, events, reports and demo upload headers.

- `num_maps`  
  The series length (e.g. `1`, `3`, `5`).

- `maplist` (internally `MapsPool`)  
  Array of map names. The enhanced fork **does not use in‑game veto**; instead:
  - It takes the first `num_maps` entries from `maplist` as the final `Maplist`.
  - It also prepares `MapSides` based on `match_side_type` (see below).

- `team1`, `team2`:
  - `id`: Platform‑level team id (string).
  - `name`: Team display name.
  - `players`: Object mapping `SteamID` → `name`.  
    This is especially important in **simulation mode**, where each player is represented by a bot.

- `wingman`  
  Boolean; if `true`, MatchZy sets up the match in wingman mode (2v2).

- `simulation`  
  Boolean; when `true`, enables **simulation mode** (see the separate `Simulation mode` page).

- `maxRounds` / `overtimeMode` / `overtimeSegments`  
  High-level overtime / regulation configuration, typically provided by the tournament backend:
  - `maxRounds` (number, optional)  
    - When present and > 0, MatchZy maps this directly to `mp_maxrounds` when the match goes live.
  - `overtimeMode` (`"enabled"` \| `"disabled"`, optional)  
    - `"enabled"` → MatchZy sets `mp_overtime_enable 1`.  
    - `"disabled"` → MatchZy sets `mp_overtime_enable 0` (no overtime; regulation only).
  - `overtimeSegments` (number, optional)  
    - Parsed and stored on the match; we **do not** currently enforce a hard “max OT segments” cutoff (CS2 continues OT as normal).  
    - Used to control how **ties are resolved** at the end of a map:
      - If `overtimeMode: "disabled"` and `overtimeSegments: 0`: no OT is played, and a tied final score is broken by comparing **total team damage** (winner = higher damage; if damage is also tied, the result is a true draw).
      - If `overtimeMode` is not `"disabled"` and `overtimeSegments > 0`: OT may still run as usual, but if the map ever ends tied, the same **damage-based tiebreak** is applied instead of reporting a draw.
      - In all other cases (no `overtimeSegments` provided, or negative), tied final scores are treated as **draws**.

- `match_side_type`  
  Controls how sides are determined:
  - `"standard"` / `"always_knife"` – sides decided via knife (or simulated knife in simulation mode).
  - `"random"` – random side assignment per map.
  - Other values default to `"team1_ct"` (team1 starts CT).

### Deprecated veto‑related fields

For compatibility with older MatchZy / Get5‑style configs, the internal `MatchConfig` still has:

- `maps_left_in_veto_pool`
- `map_ban_order` (`veto_mode` in older JSON)
- `skip_veto`

However, this enhanced fork has **removed in‑game veto entirely**:

- Maps are **fully controlled by the JSON** provided by Auto Tournament.
- These fields are **parsed but ignored**; you can stop sending them from the platform.

## Simulation‑specific configuration

When `"simulation": true`:

- MatchZy will:
  - Treat the match as fully real from the plugin’s perspective.
  - Spawn bots that represent the configured players.
  - Use the SteamIDs and names from the JSON in **all events, reports and stats**.
  - Drive a time‑based ready flow and simulated knife/side selection.
  - Mark match reports as `simulated: true`.

- Optional: `simulation_timescale`  
  - A numeric multiplier (e.g. `1.0`, `1.5`, `2.0`) that controls the CS2 `host_timescale` while the simulation is running.
  - Only applied when `simulation: true`. Normal (human) matches always run with `host_timescale 1` and `sv_cheats 0`.
  - Values are clamped between `0.1` and `4.0` internally to prevent extreme settings.
  - When a simulated match starts, MatchZy will:
    - Set `sv_cheats 1` and `host_timescale` to the configured `simulation_timescale`.
    - Re‑enforce `sv_cheats 1` and the configured `host_timescale` at the **start of every round** so map changes or external configs cannot drop the match out of simulation speed.
    - Reset back to `host_timescale 1` and `sv_cheats 0` automatically when the simulated series ends, before the server returns to idle.

- Normal (non‑simulation, non‑practice) matches:
  - At the start of warmup and at the beginning of each round, MatchZy explicitly enforces `sv_cheats 0` and `host_timescale 1`.
  - This guarantees that cheats/timescale from previous simulation or practice sessions do **not** leak into real matches (including multi‑map series like BO3/BO5).

- The JSON **must** provide players for at least one of `team1.players` or `team2.players`.  
  If both are missing/empty, MatchZy will log an error and refuse to start the match.

See **Simulation mode** for a full walkthrough and examples.

## Summary

- Use `cfg/MatchZy/config.cfg` to tune plugin‑level options and integration URLs.
- Use the JSON match config (typically produced by MatchZy Auto Tournament) to define:
  - Teams, players, maps, series length, side rules, and whether to run in simulation mode.
- Veto is no longer used; the **platform is the source of truth** for maps and sides.


