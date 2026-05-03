## Purpose

This project begins as a faithful gameplay-focused clone of a cooperative ghost-investigation horror game, built in Unity, with the explicit goal of proving the core loop, validating modular architecture, and creating a stable base for later deviation into original mechanics, ghost logic, assets, and game design concepts. If core value is proven in Unity, we'll move to developing final product in Unreal Engine.

The clone MVP is **not** the final creative destination. It is the shortest serious path to:
- understanding the structure of the original gameplay loop,
- building the right technical architecture for systemic expansion,
- testing whether the loop is fun for us,
- and creating a stable prototype foundation before evolving the game into something more original.

---

## Core product thesis

The clone MVP should recreate the emotional and mechanical structure of the target genre:
- enter a haunted location,
- investigate an unseen ghost through tools and evidence,
- manage uncertainty and danger,
- make a ghost identification decision,
- and either survive with a correct conclusion or fail through incorrect deduction or ghost aggression.

The value of the clone MVP is not imitation for its own sake. The value is that this structure already solves a strong player fantasy:
- cooperative fear,
- procedural deduction,
- uncertain information,
- tool-driven investigation,
- and escalating tension inside an explorable space.

We are using the clone MVP to learn the structure well enough to later mutate it intelligently.

---

## Clone MVP definition

The Clone MVP is complete when the project supports a playable end-to-end loop that feels recognizably like the target ghost-hunting investigation structure, even if content breadth, art polish, and multiplayer quality are still limited.

The clone MVP should include:
- first-person movement and exploration in haunted house maps,
- interactable environment and tool handling,
- ghost runtime behavior,
- evidence generation and detection,
- ghost identity deduction,
- journal/guess flow,
- win/loss state resolution,
- a baseline set of ghost types,
- a baseline set of evidence types,
- and architecture that allows easy tuning and expansion.

The clone MVP does **not** require:
- final art,
- final audio,
- progression/economy polish,
- original post-clone mechanics,
- full production-grade atmosphere,
- or full multiplayer feature completeness (lobbies, matchmaking, voice chat) — though multiplayer-first architecture must be established from day one; no system should assume a single local player.

---

## Design priorities

The project should optimize for these priorities, in order:

1. **Loop clarity**  
   The player must understand what they are doing: explore, investigate, gather evidence, infer ghost identity, survive.

2. **System modularity**  
   Ghosts, evidence, tools, and pacing rules must be data-driven and easy to extend.

3. **Rapid iteration**  
   The architecture must support quick testing, tuning, and playtest feedback.

4. **Tension and readability**  
   The game must feel dangerous and uncertain, but not random or unreadable.

5. **Atmosphere as multiplier**  
   Strong atmosphere matters deeply, but atmosphere should amplify a working loop rather than hide a broken one.

---

## In-scope gameplay pillars

The Clone MVP should faithfully implement the following pillars.

### 1. Exploration

The player moves through an interior haunted space, opens doors, manipulates lights, and navigates rooms that matter as gameplay entities.

### 2. Investigation

The player uses tools to detect ghost activity and collect evidence rather than directly observing all ghost truth.

### 3. Deduction

The player narrows possible ghost identity through evidence, ghost behavior, and observation.

### 4. Risk

The ghost can escalate danger through events, hunts, or equivalent pressure, creating urgency and fear.

### 5. Resolution

The player makes a final identification decision and receives a meaningful outcome.

---

## Out-of-scope for the clone MVP

The following are intentionally out of scope until the clone foundation is complete or nearly complete:

- original custom mechanics beyond small harmless experiments,
- major narrative systems,
- progression economy polish,
- cosmetic customization,
- final production art pass,
- final production sound pass,
- advanced meta-loop systems,
- elaborate menu/lobby/truck simulation beyond what is needed,
- and significant deviations from the clone loop.

This is important because the MVP should answer:
**Can we recreate the structure cleanly and modularly first?**

---

## Clone MVP milestone ladder

The clone MVP should be built through layered milestones.

### Milestone 0 — House sandbox

Goal:
- player controller,
- interaction system,
- doors/lights,
- pickup/drop,
- room logic,
- debug visibility.

Question answered:
- does the player feel good existing in the haunted house space?

### Milestone 1 — Minimal ghost loop

Goal:
- one ghost,
- one evidence type,
- one tool,
- one guess flow,
- one fail state.

Question answered:
- is the minimal investigation loop fun at all?

### Milestone 2 — Investigation loop

Goal:
- three evidence types,
- multiple tools,
- stronger ghost event scheduling,
- more robust journal tracking.

Question answered:
- does evidence-driven deduction create good tension and player reasoning?

### Milestone 3 — Main clone framework

Goal:
- full evidence matrix,
- multiple ghost definitions,
- accurate baseline clone logic,
- stronger round structure.

Question answered:
- can the project represent the main clone cleanly and modularly?

### Milestone 4 — Distinct ghost identities

Goal:
- per-ghost behavioral modifiers,
- ghost-specific tendencies,
- stronger differentiation.

Question answered:
- can the clone support ghost identity beyond evidence combinations?

### Milestone 5 — Clone MVP complete

Goal:
- recognizable complete clone loop,
- stable architecture,
- enough ghost/evidence breadth to count as the full clone baseline.

Question answered:
- do we now have a solid foundation for original evolution?

---

## Architecture philosophy

The architecture must be built for mutation, not just completion.

### Core rule

**New content should mostly mean new data, not new engine-level code.**

That means:
- a new ghost should mostly be a new `GhostDefinition`,
- a new evidence type should mostly be a new `EvidenceDefinition` plus detection path,
- a new tool should mostly be a new `ToolDefinition` plus runtime/tool UI behavior,
- and new pacing should mostly be tunable through rules and config.

### Runtime vs definition separation

The clone MVP must strictly separate:
- static design-time content data,
- runtime per-match state,
- and scene-authored world anchors.

Examples:
- `GhostDefinition` != `GhostRuntimeState`
- `ToolDefinition` != held tool runtime state
- `EvidenceDefinition` != currently active evidence event

This separation is one of the most important structural requirements for long-term maintainability.

### Scene objects stay thin

Scene objects should mostly:
- define locations,
- define physical interactables,
- define anchors/volumes,
- and host lightweight behavior.

The main simulation logic should live in reusable scripts and systems.

### Debug-first development

Because this is a hidden-state game, every important system must be debuggable:
- ghost room,
- ghost state,
- evidence state,
- timers,
- active tool readings,
- current guess,
- and round state.

If the system cannot be inspected, it will be much harder to tune and extend.

---

## Content philosophy

Content should be introduced in narrow slices.

### Ghosts

Ghost identity should begin with:
- evidence permissions,
- baseline timings,
- roam tendencies,
- hunt thresholds,
- and simple modifiers.

Only later should special ghost rules become more bespoke.

### Evidence

Evidence should be:
- manager-driven,
- room-aware,
- tool-readable,
- and constrained by ghost permissions.

Avoid embedding evidence rules directly all over the codebase.

### Tools

Tools should detect:
- evidence categories,
- signals,
- or ghost-related events,

not directly “know” ghost classes.

### Maps

Maps should function first as:
- room topology,
- line-of-sight spaces,
- evidence locations,
- and tension containers.

Art and atmosphere can scale later, but room semantics must be clean from the start.

---

## Technical principles

### Engine choice

Unity is the current clone-development engine because:
- it is faster for programmer-led prototyping,
- it aligns well with AI-assisted coding workflows,
- and it supports the modular, script/data-driven architecture needed for rapid iteration.

A later move to Unreal remains possible if atmosphere and production needs justify it, but the clone MVP itself should be treated as a serious Unity project first.

### AI-assisted development

AI tools should be used to accelerate:
- documentation,
- architecture exploration,
- script scaffolding,
- debugging,
- and system expansion.

But the documentation must give those tools stable context:
- consistent naming,
- phase boundaries,
- system ownership,
- and clear design intent.

That is why this vision doc exists.

### Graybox-first

Use placeholder assets first.
The clone MVP should prove:
- systems,
- pacing,
- and readability

before expensive polish work.

---

## Acceptance criteria for Clone MVP

The Clone MVP is considered complete when all of the following are true:

- The project supports a complete start-to-finish ghost investigation round.
- The player can gather multiple evidence types through tools.
- The project supports a baseline roster of ghost types defined through modular content rules.
- Ghosts differ both by evidence and baseline behavior.
- The player can make a meaningful ghost identification decision.
- Win/loss conditions are stable and understandable.
- The architecture supports adding new ghost types and evidence without major refactors.
- Multiple maps or at least one sufficiently complete map exist for repeated playtesting.
- The game is stable enough that new development can shift from “complete the clone” to “evolve the game.”

---

## Transition after Clone MVP

Once the clone MVP is complete, the project enters a new phase:
**evolution beyond the clone.**

At that point, the questions change from:
- “can we recreate this structure?”

to:
- “how do we change this into something more compelling for us?”

That later evolution can include:
- new ghost behavior models,
- altered evidence logic,
- new map structures,
- changed tension systems,
- different progression,
- and original design concepts.

But those changes should happen **after** the clone architecture is proven.

---

## Guiding question

At every milestone, ask:

**Does this work move us closer to a modular, playable, proof-of-fun clone foundation?**

If yes, it belongs in the clone MVP.
If no, it may belong in post-clone evolution instead.

---

## Related docs

Recommended supporting docs:
- `phase-0-house-sandbox-roadmap.md`
- `phase-0-house-sandbox-implementation-guide.md`
- `phase-0-house-sandbox-checklist.md`
- `phase-1-minimal-ghost-loop-roadmap.md`
- `phase-1-minimal-ghost-loop-implementation-guide.md`
- `phase-1-minimal-ghost-loop-checklist.md`

Future lightweight docs:
- `sprint-2-outline.md`
- `sprint-3-outline.md`