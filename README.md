<div align="center">

  <img src="docs/icon.svg" alt="Matchzy Enhanced" width="140" height="140">

# Matchzy Enhanced

⚡ **Enhanced CS2 match management plugin tailored for tournament automation**

  <p>Enhanced fork of MatchZy tailored for the automatic tournament platform. Adds more events and enables external tools to setup, control, and track matches in real-time.</p>

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![C#](https://img.shields.io/badge/C%23-239120?logo=c-sharp&logoColor=white)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)

**🔗 [MatchZy Auto Tournament](https://github.com/sivert-io/matchzy-auto-tournament)** • **[CS2 Server Manager](https://github.com/sivert-io/cs2-server-manager)**

</div>

---

## 🚀 Quick Start

### Automatic Installation (Recommended)

**[CS2 Server Manager](https://github.com/sivert-io/cs2-server-manager)** includes **Matchzy Enhanced** (this enhanced MatchZy fork) and sets up servers automatically configured for MatchZy Auto Tournament:

```bash

wget https://raw.githubusercontent.com/sivert-io/cs2-server-manager/master/install.sh

bash install.sh

```

### Manual Installation

1. **Download** the latest release from [Releases](https://github.com/sivert-io/MatchZy/releases)

2. **Extract** to your CS2 server's `game/csgo/` directory

3. **Configure** files in `cfg/MatchZy/`

4. **Restart** your server

👉 **[Documentation](https://mat.sivert.io/)** • **[Original MatchZy Docs](https://shobhit-pathak.github.io/MatchZy/)** (for reference)

---

## ✨ Enhanced Features

This fork is tailored for tournament automation and competitive play:

### 🎯 Tournament Automation Features

✨ **Match Report API** — Structured JSON reports for match state

📡 **Additional Events** — More events for external tools to track match progress in real-time

🔧 **External Tool Integration** — Enables external tools to setup, control, and track matches in real-time

🔄 **Thread-Safe Operations** — Fixed non-main thread issues for reliable HTTP operations

### ⚡ Player Experience Features (New!)

**🚀 Auto-Ready System** — Players automatically marked as ready on join (optional)
- Configurable auto-ready on connect
- Players can still opt-out with `.unready`
- Perfect for fast-paced tournaments

**⏸️ Enhanced Pause System** — Improved pause controls
- Both teams must `.unpause` to resume (configurable)
- Per-team pause limits (e.g., 2 pauses per team)
- Configurable pause duration with auto-timeout
- New aliases: `.p` for pause, `.up` for unpause

**⏱️ Side Selection Timer** — No more endless waiting after knife
- Configurable timer for side selection (default: 60s)
- Commands: `.ct`, `.t`, `.stay`, `.swap`
- Random side selection if time expires

**🏳️ Early Match Termination (`.gg`)** — Quick forfeit system
- Type `.gg` to vote for early match end
- Requires 80% team consensus (configurable)
- Opposing team wins automatically
- Votes reset each round

**🚫 FFW (Forfeit/Walkover) System** — Handles team disconnects
- 4-minute timer when entire team leaves
- Minute-by-minute warnings
- Auto-cancels if team returns
- Fair handling of connection issues

---

## 🏆 MatchZy Auto Tournament

**This enhanced fork was created specifically for [MatchZy Auto Tournament](https://github.com/sivert-io/matchzy-auto-tournament)** — a complete tournament automation platform.

The additional events and APIs enable external tools to setup matches, control match flow, and track progress in real-time, making it perfect for automated tournament management.

👉 **[Get Started with MatchZy Auto Tournament](https://github.com/sivert-io/matchzy-auto-tournament)**

---

## 🖥️ CS2 Server Manager

**[CS2 Server Manager](https://github.com/sivert-io/cs2-server-manager)** is a multi-server management tool that includes **Matchzy Enhanced** (this enhanced MatchZy fork) and automatically configures servers for use with MatchZy Auto Tournament.

- 🚀 Deploys 3–5 dedicated servers in minutes

- 🔧 Includes MatchZy enhanced fork and all required plugins

- ⚙️ Pre-configured for tournament automation

- 🎯 Zero manual configuration required

👉 **[CS2 Server Manager Guide](https://csm.sivert.io/)**

---

## ⚙️ Requirements

- **CounterStrikeSharp** (latest version) — [Installation Guide](https://github.com/roflmuffin/CounterStrikeSharp)

- **CS2 Dedicated Server**

- **.NET 8.0 Runtime** (for running the plugin)

---

## 📖 Configuration

All new features are disabled by default for safety. Enable them in `cfg/MatchZy/config.cfg`:

```cfg
// Auto-Ready System
matchzy_autoready_enabled "1"

// Enhanced Pause System  
matchzy_both_teams_unpause_required "1"
matchzy_max_pauses_per_team "2"  // 2 pauses per team
matchzy_pause_duration "300"     // 5 minute timeout

// Side Selection Timer
matchzy_side_selection_enabled "1"
matchzy_side_selection_time "60"  // 60 seconds

// Early Match Termination
matchzy_gg_enabled "1"
matchzy_gg_threshold "0.8"  // 80% team consensus

// FFW System
matchzy_ffw_enabled "1"
matchzy_ffw_time "240"  // 4 minutes
```

👉 **[Full Configuration Guide](docs/configuration.md)** • **[Commands Reference](docs/commands.md)**

---

## 🔗 Links

- [MatchZy Auto Tournament](https://github.com/sivert-io/matchzy-auto-tournament)

- [CS2 Server Manager](https://github.com/sivert-io/cs2-server-manager)

- [Original MatchZy](https://github.com/shobhit-pathak/MatchZy)

- [CounterStrikeSharp Docs](https://docs.cssharp.dev/)

---

## 🧱 Project Structure

The C# source files are organized under a `src` directory to keep the repository root focused on configuration, docs, and packaging:

```text
MatchZy/
  MatchZy.csproj
  src/
    MatchZy.cs
    Utility.cs
    MatchManagement.cs
    SimulationMode.cs
    Events.cs
    ConfigConvars.cs
    ConsoleCommands.cs
    PracticeMode.cs
    ... (other plugin partials and support classes)
  cfg/
  lang/
  spawns/
  docs/
  release.sh
  README.md
```

The `.csproj` remains at the root; `dotnet build` and `dotnet publish` still work as before, but all plugin code now lives in `src/` instead of cluttering the top-level directory.

## 🙏 Credits & Thanks

**Original MatchZy:**

- **[shobhit-pathak/MatchZy](https://github.com/shobhit-pathak/MatchZy)** — Original MatchZy plugin by WD-

**Enhanced Fork (Matchzy Enhanced):**

- **Matchzy Enhanced** (this fork) is maintained by [sivert-io](https://github.com/sivert-io) for [MatchZy Auto Tournament](https://github.com/sivert-io/matchzy-auto-tournament)

- Tailored for tournament automation with additional events and APIs for external tools to setup, control, and track matches in real-time

**Original Credits:**

- **[Get5](https://github.com/splewis/get5)** — Referenced for many functionalities and match management concepts

- **[G5V](https://github.com/PhlexPlexico/G5V)** and **[G5API](https://github.com/PhlexPlexico/G5API)** — Amazing work with the web panel

- **[eBot](https://github.com/deStrO/eBot-CSGO)** — Great panel and logic references

- **[CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp/)** — The platform that made this plugin possible

- **[AlliedModders](https://alliedmods.net/)** — Community and inspiration

- **[LOTGaming](https://lotgaming.xyz/)** — Initial testing and server support

- **[CHR15cs](https://github.com/CHR15cs)** — Practice mode contributions

- **[K4ryuu](https://github.com/K4ryuu)** — Damage report feature

- **[DEAFPS](https://github.com/DEAFPS)** — Player whitelisting and practice mode contributions

---

<div align="center">

<strong>Made with ❤️ for the CS2 community</strong>

</div>
