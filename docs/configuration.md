---
title: Configuration
---

## Overview

MatchZy reads configuration from:

- **ConVars** (CounterStrikeSharp / server cvars) – control plugin behavior.
- **Config files** under `cfg/MatchZy/` – executed at specific lifecycle moments.
- **JSON match configs** – loaded from a URL (typically MatchZy Auto Tournament).

This page focuses on the parts that matter most when integrating with  
**[MatchZy Auto Tournament](https://github.com/sivert-io/matchzy-auto-tournament)**.

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

- The JSON **must** provide players for at least one of `team1.players` or `team2.players`.  
  If both are missing/empty, MatchZy will log an error and refuse to start the match.

See **Simulation mode** for a full walkthrough and examples.

## Summary

- Use `cfg/MatchZy/config.cfg` to tune plugin‑level options and integration URLs.
- Use the JSON match config (typically produced by MatchZy Auto Tournament) to define:
  - Teams, players, maps, series length, side rules, and whether to run in simulation mode.
- Veto is no longer used; the **platform is the source of truth** for maps and sides.


