---
title: Matchzy Enhanced Docs
---

## Matchzy Enhanced – Tournament Automation Focus

This is the documentation hub for **Matchzy Enhanced** (an enhanced MatchZy plugin for CS2), built specifically to integrate with **[MatchZy Auto Tournament](https://github.com/sivert-io/matchzy-auto-tournament)**.

- **Plugin repo**: [sivert-io/MatchZy](https://github.com/sivert-io/MatchZy)
- **Tournament platform**: [MatchZy Auto Tournament](https://github.com/sivert-io/matchzy-auto-tournament)

Matchzy Enhanced handles in-game match management (server configs, match lifecycle, events, stats, demo uploads), while MatchZy Auto Tournament orchestrates **what to play** (matches, maps, formats, teams) and consumes MatchZy’s events and reports.

## Related projects

- [CS2 Server Manager](https://mat.sivert.io/guides/cs2-server-manager/) – multi-server CS2 deployment and management.
- [MatchZy Auto Tournament](https://mat.sivert.io) – web UI and API for automated CS2 tournaments.

### Documentation structure

- **Getting Started**

  - High-level **overview** of what this fork adds.
  - **Installation** instructions for CS2 servers (standalone or via CS2 Server Manager).
  - **Configuration** reference for core convars and JSON match configs.
  - **Simulation mode** guide for running bot-driven matches that still look like real players from the platform’s perspective.
  - How this plugin integrates with **MatchZy Auto Tournament**.

- **Plugin Docs**

  - **Demo upload API** specification.
  - **Demo upload guide** with practical setup and troubleshooting.
  - Detailed **config loading behavior** for `.cfg` files.
  - **Server allocation status** fields used by the tournament platform.

- **Development**

  - Internal **simulation mode design / TODO** notes.
  - **Release process** for building and publishing new versions.

- **Changelog**
  - Historical changes for this enhanced fork.

If you’re using Matchzy Enhanced together with MatchZy Auto Tournament, start with **Getting Started → Overview** and then follow **Installation** and **Integration with MatchZy Auto Tournament**.
