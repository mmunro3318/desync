# Perplexity Deep Research Seed Prompt + Research Checklist

## Purpose
This document is a seed prompt for a **fresh Perplexity Deep Research session**. Its goal is to produce a research-backed best-practices guide that sharpens Claude’s Unity and C# implementation quality for the impossible-house prototype.

The research should combine:
- official Unity documentation,
- Unity multiplayer documentation,
- Unity best-practice guides,
- URP/lighting/performance guidance,
- and community-informed practice from experienced Unity developers where community consensus has meaningfully diverged from the docs or adds practical nuance [web:70][web:71][web:72][web:76][web:78][web:80][web:81][web:82][web:84].

## What this research is for
The output should help create **clear design and implementation guidelines** for:
- C# architecture and code style,
- Unity project structure,
- ScriptableObject/data-driven patterns,
- graphics and URP configuration,
- lighting and light-leak avoidance,
- graybox level creation and deployment,
- multiplayer architecture and networking workflow,
- co-op testing/debugging,
- performance profiling,
- and practical guardrails for AI-assisted Unity development [web:70][web:71][web:76][web:78][web:80][web:82][web:84].

## Project context to provide Perplexity
The game is a Unity-based first-person co-op spatial-horror prototype with:
- an impossible house / graph-based spatial runtime,
- portals and shifting geometry,
- multiplayer authority and reconciliation,
- observation locks,
- graybox-first development,
- debug-first hidden-state systems,
- and a small-team / AI-assisted implementation workflow.

This is **not** a general-purpose Unity tutorial request. The research should stay tightly relevant to a programmer-led co-op horror prototype in Unity.

## Legacy docs that are still useful
The new repo has newer docs, but some legacy files still matter because they define architectural philosophy that remains valid.

Surface these as useful background context:
- `Clone-MVP-Vision.md` — useful for architecture philosophy: modularity, runtime-vs-definition separation, debug-first, graybox-first.
- `3-Starter-Design-Doc.md` — useful for starter implementation structure, ownership boundaries, folder/scene discipline.
- `2-Project-Structure.md` — useful for folder layout, scene organization, class map discipline.
- `1-Phasmo-Clone-MVP.md` — useful for milestone-slice sequencing and “build one thin playable question at a time.”

Also surface current docs when relevant:
- `repo-docs-index-claude-file-map.md`
- `spatial-horror-gdd.md`
- `spatial-runtime-framework.md`
- `networked-house-runtime-interfaces-contracts.md`
- `networked-house-vertical-slice-integration-checklist.md`
- `claude-code-implementation-prompt-pack-vertical-slice.md`

## Seed prompt for Perplexity
Use the following as the main Deep Research prompt.

---

Produce a research-backed implementation guide for a Unity 6 / C# co-op first-person spatial-horror prototype.

I want this guide to improve how Claude Code writes Unity and C# for this project. The guide should be grounded first in official Unity docs and then supplemented with strong community best practices where practical reality diverges from or extends the docs.

Research and synthesize best practices for these areas:
1. Unity project architecture for small-team programmer-led games.
2. C# code organization, naming, file structure, and maintainability.
3. ScriptableObject usage patterns, including where they help and where they are overused.
4. Scene architecture, bootstrap/composition patterns, and avoiding manager sprawl.
5. Prefab strategy and scene/prefab hygiene.
6. First-person controller, interaction, and held-item systems in Unity.
7. URP graphics setup for a horror game prototype.
8. Lighting setup, shadow settings, and practical debugging of issues like light leaks between floors.
9. Graybox level creation workflows and level-authoring discipline for modular environments.
10. Multiplayer architecture with Unity Netcode for GameObjects vs adjacent Unity multiplayer tooling, focusing on 2–4 player co-op prototypes.
11. Multiplayer testing/debugging workflow, including local multi-instance, host/client authority validation, and useful networking tools.
12. Performance profiling and optimization priorities for early and mid prototype phases.
13. Debug UI / observability best practices for hidden-state systems.
14. AI-assisted Unity development guardrails: how to scope tasks for Claude, how to avoid giant overengineered scripts, how to structure acceptance criteria.
15. Community best practices or common anti-patterns that are not emphasized enough in official docs.

For each topic, I want:
- official Unity guidance,
- practical community guidance,
- what to do,
- what to avoid,
- and a recommendation tailored to this project.

Also include a section that explicitly calls out where the official docs are enough on their own, and where dev community experience adds important implementation nuance.

The final output should be structured as a practical research guide for feeding Claude Code and for making engineering decisions in the repo.

---

## Research checklist
Use this checklist to evaluate whether the Deep Research session covered the right ground.

### A. Architecture and code organization
- [ ] Does the research cover Unity-recommended project architecture patterns for maintainable codebases? [web:70][web:71][web:73][web:84]
- [ ] Does it discuss ScriptableObject-based modular architecture with caution about overuse? [web:71][web:73][web:77]
- [ ] Does it give guidance on separating runtime state from definitions/data? [web:71][web:73]
- [ ] Does it cover code style, naming, file organization, and maintainability practices? [web:74]
- [ ] Does it address composition roots / bootstrap scenes / service initialization patterns relevant to Unity scenes? [web:70][web:71]

### B. Scene, prefab, and level-building discipline
- [ ] Does the research cover scene ownership and how to avoid manager sprawl? [web:70][web:71]
- [ ] Does it discuss prefab strategy and prefab hygiene in practice?
- [ ] Does it include guidance on modular graybox level creation for iteration-heavy development?
- [ ] Does it cover how to keep scene-authored geometry from becoming the source of hidden runtime truth?
- [ ] Does it include community advice for keeping Unity scenes/prefabs from turning brittle?

### C. Graphics, URP, and horror-friendly rendering
- [ ] Does the research cover Unity 6/URP graphics guidance relevant to a horror prototype? [web:72][web:82]
- [ ] Does it address light count, shadows, lighting tradeoffs, and URP performance settings? [web:76][web:79][web:82]
- [ ] Does it include practical guidance for lighting indoors and debugging artifacts like floor-to-floor light leaks? [web:76][web:82]
- [ ] Does it distinguish prototype-friendly visual practices from shipping-level polish work? [web:72][web:76]

### D. Multiplayer and networking
- [ ] Does the research explain Unity’s current multiplayer stack options clearly, especially NGO for small co-op games? [web:75][web:78][web:80][web:84]
- [ ] Does it compare official multiplayer guidance with community views on when NGO is a good fit? [web:78][web:80][web:81]
- [ ] Does it discuss authority, synchronization boundaries, and pitfalls for 2–4 player co-op prototypes? [web:78][web:80][web:84]
- [ ] Does it cover test/debug tooling such as multiplayer tools, network simulation, or multi-instance workflows? [web:78]
- [ ] Does it surface common mistakes that make “works locally” fail across computers?

### E. Performance and profiling
- [ ] Does the research identify what to profile early versus later in Unity prototypes? [web:71][web:76]
- [ ] Does it recommend sensible performance budgets or priorities for CPU, GPU, lighting, and networking?
- [ ] Does it explain common early over-optimization traps?

### F. Debugging and observability
- [ ] Does the research cover debug tooling practices for hidden-state or multiplayer systems?
- [ ] Does it include ideas for runtime overlays, logs, version/state tracing, and test harnesses?
- [ ] Does it distinguish player-facing UI from developer-facing debug UI?

### G. AI-assisted development workflow
- [ ] Does the research provide actionable guidance for prompting Claude effectively on Unity tasks?
- [ ] Does it recommend bounded tasks, clear contracts, and explicit acceptance criteria?
- [ ] Does it warn against giant scripts, overengineered abstractions, and clone-by-memory coding?
- [ ] Does it help convert best practices into reusable repo-level implementation rules?

## Deliverable request for Perplexity
Ask Perplexity to produce:

1. A primary research report in structured markdown.
2. A short “recommended defaults for this project” section.
3. A “what Claude should always do” checklist.
4. A “what Claude should avoid” checklist.
5. A “where docs vs community differ” section.
6. A prioritized reading list of Unity official docs and 5–10 high-signal community resources.

## What the final research should help us create later
This research should later feed into:
- a Unity/C# engineering standards doc,
- a graphics and lighting standards doc,
- a multiplayer implementation standards doc,
- and a Claude coding guardrails doc for this repo.

## Notes on source weighting
Prefer sources in this order:
1. official Unity docs/manual/guides,
2. official Unity multiplayer docs,
3. official Unity how-to / best-practice content,
4. high-signal experienced community discussions,
5. tutorial-style sources only when they add concrete practical clarity.

Avoid over-weighting random tutorials when official docs or stronger community consensus exists [web:70][web:71][web:78][web:84].

## Why this matters
The project already has strong design docs. The missing piece is a research-backed implementation guidance layer that helps Claude produce tighter Unity/C# code, better scene/prefab discipline, safer multiplayer assumptions, and more reliable graphics/lighting decisions for the vertical slice [web:70][web:71][web:72][web:76][web:78][web:80][web:82][web:84].
