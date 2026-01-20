---
title: Integration with MatchZy Auto Tournament
---

## Overview

This page explains how the **MatchZy (Enhanced)** plugin integrates with  
**[MatchZy Auto Tournament](https://mat.sivert.io)**.

At a high level:

- **Auto Tournament** decides:
  - Which match to play (teams, maps, format).
  - When to start matches.
  - Where to send events, reports and demos.
- **MatchZy**:
  - Runs on the CS2 server.
  - Pulls match configuration from Auto Tournament as JSON.
  - Drives the in‑game flow and pushes events/reports/demos back.

## Data flow

1. **Admin creates/updates a match** in MatchZy Auto Tournament.
2. The platform exposes that match at:

   ```text
   GET /matches/:slug.json
   ```

3. On the CS2 server, an admin (or automation) runs:

   ```text
   .match <slug>
   ```

   MatchZy:

   - Calls `GET {matchzy_loadmatch_url}/{slug}.json`
   - Parses the JSON into `MatchConfig`.
   - Sets up the match on the server (warmup, team names, map, etc.).

4. As the match progresses, MatchZy:
   - Sends **events** to Auto Tournament (and/or your endpoints).
   - Optionally uploads **demo files** to your API.
   - Optionally uploads a **match report JSON** after maps/series.

## Event reliability

MatchZy includes an automatic event retry system that ensures no match data is lost:

- **Automatic queueing**: Failed event POSTs are saved to the local database
- **Background retry**: Events are retried every 30 seconds with exponential backoff
- **Server tracking**: A `server_configured` event is sent when the webhook URL is set, allowing your API to track active servers
- **Pull API**: Your API can query `matchzy_get_match_stats <matchId>` to retrieve match data directly from the server

This means your tournament platform doesn't need to worry about temporary API downtime or network issues. Events will be delivered automatically when connectivity is restored.

## Required MatchZy configuration

In `cfg/MatchZy/config.cfg`, the key convars for Auto Tournament are:

- **Match loading**
  - `matchzy_loadmatch_url`  
    Base URL for match configs, e.g.:

    ```cfg
    matchzy_loadmatch_url "https://your-tournament-api/matches"
    ```

    When you run `.match abc123`, MatchZy will request:

    ```text
    GET https://your-tournament-api/matches/abc123.json
    ```

  - `matchzy_loadmatch_header_key` / `matchzy_loadmatch_header_value`  
    Optional header for authentication, for example:

    ```cfg
    matchzy_loadmatch_header_key "Authorization"
    matchzy_loadmatch_header_value "Bearer YOUR_SECRET_TOKEN"
    ```

- **Match report (optional but recommended)**
  - `matchzy_match_report_url` – where to POST match report JSON.
  - `matchzy_match_report_header_key` / `matchzy_match_report_header_value` – optional auth header.

- **Demo upload (optional)**
  - `matchzy_demo_upload_url` – where to POST raw `.dem` files.
  - `matchzy_demo_upload_header_key` / `matchzy_demo_upload_header_value` – optional auth header.

See **Configuration** and the `Demo upload` / `Match report` docs for exact request details.

## Expected JSON format from Auto Tournament

Auto Tournament should return a JSON structure compatible with MatchZy’s parser, roughly:

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
  "wingman": false,
  "simulation": false
}
```

Notes:

- `matchid` – numeric match identifier; echoed in events, reports and demo headers.
- `maplist` – full map pool for the series; this enhanced fork takes the first `num_maps` entries as the final series maps (no in‑game veto).
- `team1`/`team2`:
  - `id` – platform‑level identifier used in UIs and for linking.
  - `name` – display name.
  - `players` – SteamID → display name; critical for stats and for simulation mode.
- `simulation` – if `true`, enables **simulation mode** (see separate page).

## Events and reports consumed by Auto Tournament

Auto Tournament typically listens to:

- **Events (webhooks)**
  - Match lifecycle: `match_started`, `match_ended`, `map_result`, `series_result`.
  - Ready system: `player_ready`, `player_unready`, `team_ready`.
  - Knife flow: `knife_round_started`, `knife_round_ended` (simulated in simulation mode).
  - Side swaps: `side_swap`, `halftime_started`.
  - Demo upload lifecycle: `demo_upload_ended`.
  - Player connect/disconnect and pause/unpause events.

  These are designed to be **backwards compatible** and stable so Auto Tournament can rely on them.

- **Match report JSON**
  - Sent to `matchzy_match_report_url` if configured.
  - Contains:
    - Teams, players, scores.
    - Per‑map stats and per‑player stats.
    - A `simulated: true/false` flag indicating whether simulation mode was used.

- **Demo uploads**
  - Raw `.dem` files uploaded to `matchzy_demo_upload_url`.
  - Requests include headers like:
    - `MatchZy-FileName`
    - `MatchZy-MatchId`
    - `MatchZy-MapNumber`
    - `MatchZy-RoundNumber`

See:

- **Demo upload API** – exact header, method and body specification.
- **Demo upload guide** – how to enable, configure and debug demo uploads.
- **Server allocation status** – how Auto Tournament can track which server is running which match.

## Simulation mode in the integration

When you set `"simulation": true` in the JSON:

- Auto Tournament:
  - Configures the match exactly as usual.
  - Optionally marks the match as “simulated” in its own UI.
- MatchZy:
  - Spawns bots instead of requiring players.
  - Maps bots to the configured players.
  - Emits **identical events and reports**, but using the configured SteamIDs and names.
  - Sets `simulated: true` in match reports.

From the platform’s point of view:

- The integration points (events, reports, demo uploads) are unchanged.
- Only semantics differ: the match is known to be simulated.

## Putting it all together

1. Deploy MatchZy Auto Tournament and configure matches there.
2. Install this MatchZy fork on your CS2 servers.
3. Set the MatchZy convars:
   - `matchzy_loadmatch_url` → your Auto Tournament API.
   - Optionally, match report + demo upload URLs.
4. On the CS2 server, run:

   ```text
   .match <slug>
   ```

5. Let MatchZy run the match; Auto Tournament reacts to events, reports and demos.
6. Use simulation mode for automated testing and demo matches without real players.

---

## API Endpoints Reference

### Demo Upload Endpoint

**Configuration:**
```cfg
matchzy_demo_recording_enabled "1"    // Default: 1 (enabled)
matchzy_demo_upload_url "https://your-api/demos/upload"
```

**Smart Restart Delays:**
| Configuration | Match End Delay | Use Case |
|---------------|-----------------|----------|
| Recording **disabled** | ~10s | Fast tournaments, simulations |
| Recording enabled, **no upload URL** | ~25-35s | Local demos only, no upload wait |
| Recording enabled, **upload URL set** | ~60-90s | Full upload, maximum compatibility |

**Benefits:**
- No wasted time waiting for uploads that won't happen
- Fast turnaround when you only need local demo files
- Full compatibility when upload endpoint is configured

**Method:** `POST`  
**Content-Type:** `application/octet-stream`  
**Body:** Raw binary demo file (`.dem`)

**Headers:**
```
MatchZy-FileName: {demo_filename}.dem
MatchZy-MatchId: {match_id}
MatchZy-MapNumber: {map_number}     // Zero-indexed (0, 1, 2...)
MatchZy-RoundNumber: {total_rounds}
```

**Timing:** Uploaded 15 seconds after map ends, one per map in series.

**Example Express.js Handler:**
```javascript
app.post("/api/demos/upload", express.raw({ type: "application/octet-stream", limit: "500mb" }), (req, res) => {
  const filename = req.headers["matchzy-filename"];
  const matchId = req.headers["matchzy-matchid"];
  const mapNumber = req.headers["matchzy-mapnumber"];
  
  fs.writeFile(`./demos/${filename}`, req.body, (err) => {
    if (err) return res.status(500).send("Upload failed");
    res.status(200).send("Upload successful");
  });
});
```

### Match Report Endpoint

**Method:** `POST`  
**Content-Type:** `application/json`

**Body Structure:**
```json
{
  "matchId": "12345",
  "mapNumber": 0,
  "team1": {
    "name": "Team A",
    "score": 16,
    "players": [...]
  },
  "team2": {
    "name": "Team B",
    "score": 14,
    "players": [...]
  },
  "simulated": false
}
```

### Event Webhooks

MatchZy sends real-time events to `remote_log_url`:

**Key Events:**
- `match_started` - Match begins
- `map_result` - Map completed
- `series_result` - Series completed
- `player_connect` / `player_disconnect`
- `player_ready` / `player_unready`
- `round_started` / `round_ended`
- `demo_upload_ended`

---

## Simulation Mode

Set `"simulation": true` in match JSON to run fully automated bot matches:

**Features:**
- 10 bots spawned per team (5v5)
- Bots mapped to configured player Steam IDs
- Identical events and reports as real matches
- Faster execution with `simulation_timescale` (1.0-10.0x speed)
- No human players required

**Use Cases:**
- Tournament bracket simulation
- Match outcome forecasting
- Integration testing
- Stress testing

**Configuration:**
```json
{
  "simulation": true,
  "simulation_timescale": 5.0,  // 5x speed
  "team1": {
    "players": { "STEAM_1:1:111": "Bot Player 1", ... }
  },
  ...
}
```


