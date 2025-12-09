---
title: Installation
---

## Prerequisites

- **CS2 Dedicated Server**
- **CounterStrikeSharp** installed and working  
  See the official docs: <https://github.com/roflmuffin/CounterStrikeSharp>
- **.NET 8.0 runtime** on the server (for running the plugin’s compiled DLL).

If you are using **[CS2 Server Manager](https://github.com/sivert-io/cs2-server-manager)**, it can set all of this up for you automatically and includes **Matchzy Enhanced** (this enhanced MatchZy fork) by default.

## Option 1: Install via CS2 Server Manager (recommended)

If you are using MatchZy together with **MatchZy Auto Tournament**, this is the easiest path:

1. Follow the CS2 Server Manager guide:  
   <https://mat.sivert.io/guides/cs2-server-manager/>
2. The manager will:
   - Deploy one or more CS2 servers.
   - Install CounterStrikeSharp.
   - Install the **Matchzy Enhanced** plugin.
   - Configure the basic MatchZy convars and configs.
3. Point MatchZy’s `matchzy_loadmatch_url` at your **MatchZy Auto Tournament** instance and you’re ready to go.

The rest of this page focuses on **manual installation** if you are not using CS2 Server Manager.

## Option 2: Manual installation

### 1. Download a release

1. Go to the GitHub releases for this repo:  
   <https://github.com/sivert-io/MatchZy/releases>
2. Download the latest `MatchZy-<version>.zip` artifact.

### 2. Extract into your CS2 server

Extract the archive into your CS2 server’s `game/csgo/` directory, so that you end up with:

- `game/csgo/addons/counterstrikesharp/plugins/MatchZy/MatchZy.dll`
- `game/csgo/addons/counterstrikesharp/plugins/MatchZy/` (other runtime DLLs)
- `game/csgo/cfg/MatchZy/` (MatchZy config files)

If you are building from source instead of using a release, see the **From source** section below.

### 3. Verify CounterStrikeSharp is loading the plugin

1. Start your CS2 dedicated server.
2. Check the server console for CounterStrikeSharp plugin load messages.
3. You should see lines indicating that `MatchZy` has been loaded and its version (e.g. `0.8.27`).

If the plugin does not load:

- Verify that the DLL is under  
  `addons/counterstrikesharp/plugins/MatchZy/MatchZy.dll`.
- Check that your server is using the **same .NET major version** as the plugin target (`net8.0`).

## Installing alongside MatchZy Auto Tournament

When using this plugin with **MatchZy Auto Tournament**:

- Make sure your CS2 server is reachable from the Auto Tournament API (network / firewall).
- Configure the relevant MatchZy convars (see **Configuration**):
  - `matchzy_loadmatch_url`
  - `matchzy_loadmatch_header_key`, `matchzy_loadmatch_header_value` (if you secure the API).
  - `matchzy_match_report_url`, `matchzy_match_report_header_*` (if you enable match reports).
  - `matchzy_demo_upload_url`, `matchzy_demo_upload_header_*` (if you enable demo uploads).

Auto Tournament will:

- Expose `GET /matches/:slug.json` that MatchZy calls via `matchzy_loadmatch_url`.
- Use MatchZy’s **events** and **reports** to keep its UI and brackets in sync with the server.

See **Integration with MatchZy Auto Tournament** for an end‑to‑end view of that flow.

## Installing from source

If you want to tweak the plugin and build it yourself:

1. Clone this repo:

   ```bash
   git clone https://github.com/sivert-io/MatchZy.git
   cd MatchZy
   ```

2. Build the plugin DLL:

   ```bash
   dotnet build
   ```

   The compiled output will be under `build/Debug/net8.0/` (or `build/Release/net8.0/`).

3. Copy the binaries to your server:

   - From:
     - `build/Debug/net8.0/` or `build/Release/net8.0/`
   - To:
     - `game/csgo/addons/counterstrikesharp/plugins/MatchZy/`

4. Copy (or update) the config files:

   - From this repo’s `cfg/MatchZy/`
   - To your server’s `game/csgo/cfg/MatchZy/`

After that, restart the server and confirm the plugin loads as described above.


