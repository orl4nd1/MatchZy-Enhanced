---
title: MatchZy Enhanced
---

# MatchZy Enhanced

Automated CS2 match management plugin for tournaments. Runs inside your CS2 servers to handle configs, match flow, events, stats, and demo uploads while your tournament platform decides what to play.

Designed to work hand-in-hand with:

- **[MatchZy Auto Tournament](https://mat.sivert.io)** – web UI and API for automated CS2 tournaments.
- **[CS2 Server Manager](https://csm.sivert.io/)** – multi-server CS2 deployment that ships with MatchZy Enhanced preinstalled.

## What it does

- **In-server match automation**: Warmup, knife/live, pauses, side swaps, BO1/BO3 series, and clean match resets.
- **Tournament integration**: Stable HTTP events and match reports for external tools like MatchZy Auto Tournament.
- **Demo upload pipeline**: Automatically captures GOTV demos and uploads `.dem` files to your API.
- **Simulation mode**: Bot-driven, fully automated matches mapped to real player identities.

## Quick Start

For most users, this is all you need:

```bash
wget https://raw.githubusercontent.com/sivert-io/cs2-server-manager/master/install.sh
bash install.sh
```

Then follow the **Getting Started** section (Overview & Installation) to point MatchZy at your tournament platform.

## Project layout

- `src/` – C# plugin source (match logic, events, reports, simulation mode).
- `cfg/MatchZy/` – match configs for warmup, knife, live, practice, wingman, sleep, etc.
- `lang/` – localized in-game messages.
- `spawns/` – spawn definitions (e.g. coach positions).

See:

- **Getting Started → Overview** – high-level architecture and when to use this fork.
- **Getting Started → Installation** – install via CS2 Server Manager or manually.
- **Getting Started → Configuration** – MatchZy convars and config files.
- **Getting Started → Simulation mode** – API-driven simulated matches.
- **Getting Started → Integration with MatchZy Auto Tournament** – end-to-end flow.

---

## Support

- [GitHub Issues](https://github.com/sivert-io/matchzy-auto-tournament/issues) – report bugs or request features.
- [Discussions](https://github.com/sivert-io/matchzy-auto-tournament/discussions) – ask questions and share ideas.
- [Discord Community](https://discord.gg/n7gHYau7aW) – real-time support and chat with other tournament hosts.

## Related projects

- [CS2 Server Manager](https://sivert-io.github.io/cs2-server-manager/) – multi-server CS2 deployment and management.
- [MatchZy Auto Tournament](https://mat.sivert.io) – web UI and API for automated CS2 tournaments.

---

## License & credits

<div align="center" markdown>

MIT License • Built on MatchZy • Made with :material-heart: for the CS2 community

</div>


