---
title: Overview
---

## What is Matchzy Enhanced?

Matchzy Enhanced is a **CS2 match management plugin** designed to work hand‑in‑hand with  
**[MatchZy Auto Tournament](https://github.com/sivert-io/matchzy-auto-tournament)**.

The original MatchZy plugin already offered a Get5‑style experience for CS2.  
This fork extends it with:

- **Stable HTTP events and reports** suitable for external automation.
- A **structured match report JSON** consumed by MatchZy Auto Tournament.
- A **demo upload pipeline** that ships `.dem` files to your API with rich headers.
- A fully automated **simulation mode** driven by bots (no real players required).
- Quality‑of‑life fixes and behavior tweaks for large‑scale tournament usage.

## High‑level architecture

- **Matchzy Enhanced (this project)** runs **inside the CS2 server**:
  - Listens to game events and manages the full match lifecycle.
  - Applies server configs (`cfg/MatchZy/*.cfg`) for warmup, knife, live, practice, etc.
  - Emits HTTP **events** (webhooks) and **match reports** to your tournament platform.
  - Optionally uploads **demo files** to a configured HTTP endpoint.

- **MatchZy Auto Tournament (separate project)** runs as a **web service + UI**:
  - Owns the **match schedule**: which teams, maps, format, and settings to play.
  - Exposes a `GET /matches/:slug.json` endpoint that MatchZy uses via `matchzy_loadmatch_url`.
  - Receives events and reports from MatchZy to update brackets, scores, and stats in real time.

In other words:

- **Auto Tournament decides *what* to play.**
- **MatchZy decides *how* to run it on the CS2 server.**

## Key features in this fork

- **Tournament‑oriented events**
  - Extended event set: match setup, ready system, side swaps, map results, match results, demo upload lifecycle, etc.
  - Event schema is documented and stable so Auto Tournament can rely on it.

- **Match report API**
  - After a map or series finishes, MatchZy can upload a **JSON match report**.
  - Includes teams, players, scores, per‑map/per‑round stats and metadata (including a `simulated` flag in simulation mode).

- **Demo upload pipeline**
  - MatchZy records demos via GOTV and uploads `.dem` files to your API endpoint.
  - See:
    - `Demo upload API` for exact headers and request details.
    - `Demo upload guide` for configuration and debugging tips.

- **Simulation mode**
  - Configure `simulation: true` in your match JSON and MatchZy:
    - Spawns bots instead of requiring real players.
    - Maps each bot to a **configured player identity** (SteamID + name) from the JSON.
    - Drives a **staggered ready flow** so events look like human players typing `!ready`.
    - Skips the actual knife round and **simulates side choice** and `knife_round_ended`.
  - Externally (Auto Tournament, webhooks, stats), the match looks like a normal player match, with an optional `simulated` flag for clarity.

- **Layout and build**
  - C# source code in `src/`.
  - Runtime content in `cfg/`, `lang/`, `spawns/`.
  - Docs in `docs/` (this site).
  - Build artifacts in `build/` (ignored by git).

## When should you use this fork?

Use this fork if:

- You use **MatchZy Auto Tournament** (or plan to).
- You want **API‑driven** match control and detailed telemetry from CS2.
- You care about **full automation**, including simulation / validation flows without human players.

If you only need a simple in‑server pug or scrim plugin and don’t intend to automate via APIs or external tooling, the original MatchZy project may be sufficient. This fork is optimized for **tournament platforms and automation**.


