---
title: Matchzy Enhanced
---

## MatchZy Enhanced

Automated **CS2 match management plugin** for tournaments. MatchZy Enhanced runs inside your CS2 servers to handle configs, match flow, events, stats, and demo uploads, while your tournament platform decides what to play.

Designed to work hand‑in‑hand with:

- **[MatchZy Auto Tournament](https://mat.sivert.io)** – web UI and API for automated CS2 tournaments.
- **[CS2 Server Manager](https://csm.sivert.io/)** – multi‑server CS2 deployment that ships with MatchZy Enhanced preinstalled.

## What it does

- **In‑server match automation**: Warmup, knife/live, pauses, side swaps, BO1/BO3 series, and clean match resets.
- **Tournament integration**: Stable HTTP **events** and **match reports** that external tools (like MatchZy Auto Tournament) consume in real time.
- **Demo upload pipeline**: Automatically captures GOTV demos and uploads `.dem` files to your API with rich headers.
- **Simulation mode**: Runs fully automated matches with bots mapped to real player identities for validation, testing, and “instant brackets”.
- **Production‑ready layout**: Clear separation of source (`src/`), runtime content (`cfg/`, `lang/`, `spawns/`), and build artifacts (`build/`).

## Quick Start

For most users running tournaments, this is all you need:

```bash
wget https://raw.githubusercontent.com/sivert-io/cs2-server-manager/master/install.sh
bash install.sh
```

This uses **CS2 Server Manager** to:

- **Deploy CS2 servers** (one or many).
- **Install CounterStrikeSharp** and **MatchZy Enhanced**.
- **Configure basic MatchZy settings** ready for MatchZy Auto Tournament.

For manual installs or existing servers, see **Getting Started → Installation**.

## Project layout

- **`src/`**: C# plugin source (match logic, events, reports, simulation mode, etc.).
- **`cfg/MatchZy/`**: Match configs for warmup, knife, live, practice, wingman, sleep mode, and more.
- **`lang/`**: Localized strings for in‑game messages.
- **`spawns/`**: Optional spawn definitions (e.g. coach positions).
- **`build/`**: Compiled DLLs and publish artifacts (ignored by git).
- **`docs/`**: This documentation site.

## Where to go next

- **Getting Started → Overview**: High‑level architecture and when to use this fork.
- **Getting Started → Installation**: Install via CS2 Server Manager or manually on an existing CS2 server.
- **Getting Started → Configuration**: MatchZy convars and config files in `cfg/MatchZy/`.
- **Getting Started → Simulation mode**: How to run API‑driven, fully simulated matches.
- **Getting Started → Integration with MatchZy Auto Tournament**: End‑to‑end flow between server and tournament platform.
- **Plugin Docs**: Demo upload API, config loading behavior, and server allocation status.
- **Changelog**: Release history and notable changes.
- **Related**: Links to MatchZy Auto Tournament, CS2 Server Manager, and the original MatchZy plugin.

## Support

- [GitHub Issues](https://github.com/sivert-io/matchzy-auto-tournament/issues) – report bugs or request features.
- [Discussions](https://github.com/sivert-io/matchzy-auto-tournament/discussions) – ask questions and share ideas.
- [Discord Community](https://discord.gg/n7gHYau7aW) – real-time support and chat with other tournament hosts.

## Related projects

- [CS2 Server Manager](https://sivert-io.github.io/cs2-server-manager/) – multi-server CS2 deployment and management.
- [MatchZy Enhanced](https://me.sivert.io) – enhanced MatchZy plugin for in-server automation.

## License & credits

- **License**: [MIT License](https://github.com/sivert-io/MatchZy/blob/main/LICENSE).
- **Original plugin**: [shobhit-pathak/MatchZy](https://github.com/shobhit-pathak/MatchZy) by WD‑.
- **Enhanced fork**: Maintained by [sivert-io](https://github.com/sivert-io) for tight integration with MatchZy Auto Tournament.

<div align="center" markdown>

MIT License • Built on MatchZy • Made with :material-heart: for the CS2 community

</div>
