# /init Seed Prompt — Generate `CLAUDE.md` for the DESYNC (spatial-horror) Unity Repo

## Purpose

This prompt seeds a fresh Claude Code session running `/init` (or equivalent first-pass repo analysis) so it produces a `CLAUDE.md` that accurately reflects:

1. what this repo *is* (working title, jam constraint, design intent),
2. what code currently exists (the just-migrated foundation from the prior Phasmo-Clone repo),
3. how Claude should navigate the docs and the Unity project,
4. what coding/architecture rules to enforce,
5. and what *not* to do.

A draft already exists at `CLAUDE-draft.md` — treat it as a strong starting point but **do not assume it is up to date or complete**. Verify against the live repo before reusing any section.

## Repo at a glance

- **Working title:** `DESYNC`
- **Genre:** first-person co-op spatial / liminal horror
- **Premise (short):** a house occupied by a Lovecraftian entity expands its interior into an impossible labyrinth; players hunt 1–3 anchoring artifacts to collapse the anomaly and escape, while a stalking Entity hunts them.
- **Jam constraint:** Pride Jam 2026, due **2026-06-12**, theme **"Asylum"**.
- **Engine:** Unity 6 + URP, Netcode for GameObjects (NGO) 2.11.
- **Repo state at the time of this prompt:** documentation-heavy, with a freshly-bootstrapped Unity project containing a small foundation imported from the prior `phasmo-clone` repo (player + lobby + graybox house + NGO scaffolding). No spatial-horror runtime systems exist yet.

## Reading order for the /init pass

Read these in order, then build the `CLAUDE.md` from what you actually find — not from assumptions.

### 1. Repo orientation

1. `README.md` (top-level) — repo intent statement.
2. `CLAUDE-draft.md` — the existing draft. Treat as reference only.
3. `docs/design/00-index/repo-docs-index-claude-file-map.md` — the canonical doc map. Use this to decide what to load deeper.

### 2. Vision + design (load briefly, then move on)

4. `docs/design/01-vision/spatial-horror-gdd.md` — game design doc.
5. `docs/design/01-vision/player-experience-pillars.md` — emotional / pacing pillars.
6. `docs/design/01-vision/spatial-horror-reference-board.md` — creative anchor + dev intent.

### 3. Architecture and runtime contracts

7. `docs/design/02-architecture/spatial-runtime-framework.md` — high-level runtime framing.
8. `docs/design/02-architecture/networked-house-runtime-interfaces-contracts.md` — interface contracts.
9. Skim epics under `docs/design/03-systems/` (house-graph, observation, mutation, portal, anchor) — do **not** load every spec; just learn the system taxonomy.

### 4. Unity research baseline (these are the source of truth for engine-side conventions)

10. `docs/design/98-unity-research/00-unity-technology-baseline-report.md`
11. `docs/design/98-unity-research/01-project-architecture-and-code-organization-report.md`
12. `docs/design/98-unity-research/03-unity-urp-graphics-lighting-horror-report.md`
13. `docs/design/98-unity-research/04-ngo-multiplayer-architecture-report.md`
14. `docs/design/98-unity-research/06-ai-guardrails-and-unity-antipatterns-report.md` — **directly informs the rules section of `CLAUDE.md`**.

### 5. Migration context (so you understand what code already exists and why)

15. `docs/05-migration/phasmo-clone-archaeology-and-extraction-report.md`
16. `docs/05-migration/phasmo-clone-carry-forward-manifest.md`
17. The original handoff prompts (for context on intent, not as instructions): `docs/handoff-prompts/current/claude-onboarding-archaeology-phasmo-clone-extraction.md` and `phasmo-clone-migration-qa-checklist.md`.

### 6. The actual Unity project

18. `unity-DESYNC/My project/Packages/manifest.json` — current package set.
19. `unity-DESYNC/My project/Assets/_Project/` — every script, scene, prefab, setting, test that currently exists. Read enough to know what is real versus aspirational in the docs.

## What `CLAUDE.md` MUST contain

The generated `CLAUDE.md` must include:

1. **Project identity** — working title, jam constraint, deadline, premise (1 paragraph).
2. **Current code state snapshot** — a short, factual paragraph stating what scripts, scenes, and packages exist *right now* in `unity-DESYNC/My project/`. Reference the migration report for full detail; do not duplicate it.
3. **Folder map** — the actual top-level repo layout, including `unity-DESYNC/`, `docs/design/{00–99}/`, `docs/handoff-prompts/`, `docs/05-migration/`. Verify by listing, do not invent.
4. **Architecture rules** — distilled from the existing `CLAUDE-draft.md` "Coding Principals" + "Deep Module" sections, plus anything load-bearing from `docs/design/98-unity-research/06-ai-guardrails-and-unity-antipatterns-report.md`. Required content:
   - Lean code, single-purpose functions (~50 LoC budget with explicit justification when exceeded).
   - Meaningful naming; comments explain *why*, not *what*.
   - Deep-module / graybox interface discipline.
   - Runtime state vs. definition (ScriptableObject) separation — already practiced in `_Project/Scripts/Core/GameplaySettings.cs`.
   - Thin scene objects; no manager-god classes.
   - Debug-first hidden-state systems (per debug overlay specs).
   - Vertical-slice-first execution.
5. **NGO + multiplayer guardrails** — summarize the authority/ownership rules from `04-ngo-multiplayer-architecture-report.md`. Note that the carried-forward networking is **graybox-grade only** — local LAN host/join works, cross-machine multiplayer is not solved and requires a relay/lobby integration before it can be claimed as working.
6. **URP / lighting guardrails** — reference the lighting research report and the spatial-horror lighting spec. Mention the **known floor-to-floor light leak** in `_Project/Scenes/House_Graybox.unity` as an open issue to validate before using that scene as ground truth.
7. **Doc routing** — point Claude to `docs/design/00-index/repo-docs-index-claude-file-map.md` as the canonical doc map; *do not duplicate the routing table inside `CLAUDE.md`*.
8. **Skill routing** — preserve the relevant skill-routing block from `CLAUDE-draft.md` (gstack, superpowers, speckit, codex/gemini second opinions). Verify the skill names against the `available-skills` reminder when running `/init`; drop any that do not exist.
9. **Subagent delegation policy** — Opus/Sonnet/Haiku/Agent-Teams routing as in the draft. Keep, do not expand.
10. **What NOT to do** — non-negotiable do-nots:
    - Do not treat the carried Phasmo-Clone code as the architectural template; it is a fixture for the new runtime, not the basis of it.
    - Do not import internal files across modules; work through public interfaces.
    - Do not assume cross-machine multiplayer works.
    - Do not silently expand a function past the LoC budget without justification.
    - Do not introduce new managers/singletons without surfacing the trade-off.
    - Do not edit Unity meta files manually unless explicitly fixing a GUID issue.
11. **First-session smoke test** — list the manual Unity steps to verify the migrated project still works (open project, allow package import, play `Bootstrap.unity`, host, verify spawn into `House_Graybox.unity`, verify flashlight + footstep, run `Desync.Tests.EditMode` suite).

## What `CLAUDE.md` MUST NOT contain

- A wall-of-text duplicate of the GDD, design specs, or research reports. Link out, do not embed.
- A duplicated copy of the `repo-docs-index-claude-file-map.md` routing table.
- Aspirational claims about systems that do not exist yet (no "the house graph runtime is implemented in X" — it isn't).
- Speculative file paths. Verify every path you write by reading or listing it first.
- A history lesson about Phasmo-Clone. The migration report covers that; `CLAUDE.md` should focus on the *current* repo.
- Time estimates or sprint commitments.

## Constraints on the writing pass itself

- Keep `CLAUDE.md` under ~400 lines. If it grows beyond that, you are restating docs that should be linked.
- Use absolute-style repo-relative paths (e.g., `docs/design/02-architecture/spatial-runtime-framework.md`), not just filenames.
- Verify every single path you cite by reading it or listing the parent directory. Do not infer.
- Reuse the `CLAUDE-draft.md` voice (direct, terse, principles-first) but do not copy sections wholesale without re-checking that they still describe the live repo.
- When you finish, list every file you created or modified and the one-line reason for each.

## Suggested structure for the generated `CLAUDE.md`

```
# CLAUDE.md

## Project identity
…

## Current code state (as of <date>)
…  # link to migration report

## Repo layout
…

## Architecture rules
…

## NGO + multiplayer guardrails
…

## URP + lighting guardrails
…

## Doc routing
See `docs/design/00-index/repo-docs-index-claude-file-map.md`.

## Skill routing
…

## Subagent delegation
…

## Don'ts
…

## First-session smoke test
…
```

## Acceptance criteria

The generated `CLAUDE.md` is acceptable if a fresh Claude session, reading only `CLAUDE.md`, can:

1. Correctly state what code exists in the repo today.
2. Find the canonical doc map within one tool call.
3. State the architecture rules without ambiguity.
4. Refuse to claim cross-machine multiplayer works.
5. Find and run the existing EditMode test on first try.
6. Avoid editing internal files of other modules without surfacing the boundary violation first.

If any of those fail, revise `CLAUDE.md` instead of patching downstream.

## Final instruction to Claude running `/init`

You are *not* writing a marketing document or a design doc summary. You are writing a small, accurate, opinionated operating manual for future Claude sessions in this repo. Bias toward *fewer*, *truer* statements over *more*, *aspirational* ones. When in doubt, link to the source-of-truth doc instead of restating it.
