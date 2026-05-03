# DESYNC

**Working title.** First-person co-op spatial / liminal horror prototype for **Pride Jam 2026** (theme: *Asylum*, due 2026-06-12).

A house occupied by a Lovecraftian entity expands its interior into an impossible labyrinth. 1–3 players hunt anchoring artifacts to collapse the anomaly and escape, while a stalking Entity hunts them.

## Stack

Unity 6, URP 17.4.0, Netcode for GameObjects 2.11.2, Input System 1.19.0. See `unity-DESYNC/Packages/manifest.json` for the full dependency set.

## Repo layout

```
unity-DESYNC/   Unity project (Assets/_Project/ holds all first-party content)
docs/
  design/      vision, architecture, system specs, sprint docs, Unity research
  handoff-prompts/  active session briefs (incl. 01-migration/)
  ARCH.md      key architectural decisions (read before changing load-bearing code)
  DEEP_MODULES_SPEC.md, SKILLS_REFERENCE.md, UNITY_MCP_LESSONS.md, TODO.md
CLAUDE.md      operating manual for Claude Code sessions
```

## Status

**Documentation-heavy with a freshly-migrated Unity foundation.** Player movement, lobby, host/join (local LAN only), flashlight, footstep audio, and a graybox two-floor house are working. **None of the spatial-horror runtime systems** (house graph, observation lock, mutation, portal, anchor) are implemented yet — they live in `docs/design/02-architecture/` and `03-systems/` as specs awaiting build-out.

Cross-machine multiplayer is **not solved**. Local LAN graybox only until a Relay/Lobby integration lands.

## Getting started

1. Open `unity-DESYNC/` in Unity 6 (let packages import).
2. Open `Assets/_Project/Scenes/Bootstrap.unity` and confirm both `Bootstrap` and `House_Graybox` are in **Build Settings → Scenes In Build** (Bootstrap first).
3. Press Play, click **Host**. Verify the player spawns into `House_Graybox`, flashlight toggles, footsteps fire.
4. From a second Editor instance (Multiplayer Play Mode) or build, **Join** at `127.0.0.1` and confirm replication.
5. Run **Test Runner → EditMode → Desync.Tests.EditMode** — `NetworkBootstrapConsistencyTests` must be green.

Full smoke-test detail and known issues (e.g. the floor-to-floor light leak in `House_Graybox.unity`) are in `CLAUDE.md`.

## Where to read next

- **Vision / pillars:** `docs/design/01-vision/spatial-horror-gdd.md`, `player-experience-pillars.md`
- **Architecture:** `docs/design/02-architecture/spatial-runtime-framework.md`, `networked-house-runtime-interfaces-contracts.md`
- **Doc map:** `docs/design/00-index/repo-docs-index-claude-file-map.md`
- **Operating rules for AI agents:** `CLAUDE.md`, `docs/ARCH.md`
- **Migration provenance** (what was carried from the prior Phasmo-Clone repo): `docs/handoff-prompts/current/01-migration/work-done/`
