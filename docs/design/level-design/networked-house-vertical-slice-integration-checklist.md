# Networked House Vertical Slice Integration Checklist

## Document objective
Define the concrete integration checklist required to assemble the first playable networked impossible-house vertical slice in Unity. This checklist bridges architecture, runtime contracts, graybox scene wiring, debug surfaces, and test readiness so the project can move from good documents to a functioning two-player prototype slice [file:12][file:13][file:14][file:15].

## Why this document exists
The project now has system docs, sprint docs, contracts, a debug overlay spec, and a graybox test plan. The next risk is not lack of ideas but integration drift: systems exist on paper or in partial code, but do not connect into one reliable playable slice. Earlier planning repeatedly emphasized narrow milestones, graybox-first validation, reusable runtime systems, and debug visibility. This checklist applies that philosophy to the point where the first co-op impossible-house slice actually runs end to end [file:12][file:13][file:14][file:15].

## Definition of the target slice
This vertical slice is successful when two players can enter the same graybox house, occupy a shared authoritative graph, contribute observation/protection state, cause or prevent at least one topology mutation, inspect the runtime with debug tools, and run the core graybox test scenarios without relying on hand-waved editor magic [file:12][file:13][file:15].

## Scope of this checklist
This document covers:
- folder and scene readiness,
- core runtime services,
- graph and portal integration,
- network authority wiring,
- player/controller hooks,
- debug integration,
- deterministic mutation harness,
- graybox test readiness,
- and “done enough to test” acceptance gates [file:12][file:13][file:14][file:15].

## Out of scope
This checklist does **not** cover:
- final art,
- creature AI production behavior,
- final expedition loop,
- audio polish,
- menu flow,
- matchmaking/lobby UX,
- or content scale beyond the first deterministic house slice.

## Integration principles
The checklist follows the same principles already established in the project docs [file:12][file:13][file:14][file:15].

- Runtime state must stay distinct from definitions [file:12][file:13][file:15].
- Scene objects should stay thin; systems own rules [file:12][file:13][file:14].
- Graybox-first is correct [file:12][file:13][file:15].
- Debug-first is mandatory for hidden-state gameplay [file:12][file:13][file:15].
- Each integration step should answer a specific playable/testable question rather than dumping in whole subsystems at once [file:12][file:15].

## Integration target map
The slice should only require a small group of cooperating systems.

| Area | Must exist for the slice |
|---|---|
| Bootstrap | loads the graybox scene and initializes services |
| Graph core | authoritative node/edge/portal model |
| Observation | per-player contributions and aggregated protections |
| Mutation | at least one legal mutation path and one rejection path |
| Portals | authoritative destination state + local traversal hooks |
| Networking | host/client sync for versions and snapshot truth |
| Player | spawn, move, look, occupy nodes, cross thresholds |
| Debug | summary + deep panels + world gizmos |
| Test harness | deterministic triggers and reset flow |

## Section 1 — Project structure readiness
These items ensure the slice lands in predictable places consistent with the earlier project structure guidance [file:13][file:14].

### Checklist
- [ ] Confirm the vertical slice lives inside the existing `_Project` structure rather than a throwaway parallel hierarchy [file:13][file:14].
- [ ] Create or confirm dedicated folders for graph/runtime/debug/networked-house code under `Scripts/` in a way that preserves narrow ownership [file:13][file:14].
- [ ] Create or confirm a dedicated graybox scene location under `Scenes/Test/` or equivalent test-safe location [file:13][file:14].
- [ ] Create or confirm `Prefabs/Debug/` entries for runtime debug presenters and gizmo helpers [file:13][file:14].
- [ ] Create or confirm data asset folders for graph definitions, node metadata, and deterministic test presets, keeping content separate from runtime state [file:12][file:13][file:14].

## Section 2 — Scene readiness
The scene should be boring, explicit, and easy to reset.

### Checklist
- [ ] Create the dedicated `NetworkedHouse_Graybox` scene or equivalent test slice scene [file:13][file:14].
- [ ] Add a single obvious root hierarchy: `Systems`, `Players`, `Environment`, `GraphAuthoring`, `Portals`, `Debug`, `SpawnPoints` [file:13][file:14].
- [ ] Keep authored geometry extremely simple; do not hide integration problems behind art or layout complexity [file:12][file:13][file:15].
- [ ] Place deterministic spawn points for host and client.
- [ ] Add explicit node/portal authoring markers or components in the scene only if they are required for definition import or visual debugging.
- [ ] Verify the scene can be fully reset without manual hierarchy repair.

## Section 3 — Bootstrap and runtime composition
The slice needs a clean startup path rather than ad hoc scene state.

### Checklist
- [ ] Ensure `Bootstrap` or equivalent startup flow can load the networked graybox scene cleanly [file:13][file:14].
- [ ] Register the core services needed by the slice: graph service, observation service, mutation evaluator/director, portal authority service, reconciliation service, debug service.
- [ ] Verify services initialize in a deterministic order.
- [ ] Verify there is one obvious composition root for the slice.
- [ ] Confirm scene enter/re-enter does not duplicate managers or leave stale state behind, preserving the “avoid manager sprawl” rule described in earlier docs [file:13][file:14].

## Section 4 — House graph core integration
This is the heart of the slice.

### Checklist
- [ ] Load the deterministic graph definition for the small graybox house.
- [ ] Build runtime node, edge, and portal state from the definition rather than hardcoding scene assumptions [file:12][file:13][file:15].
- [ ] Assign stable ids for nodes, edges, and portals.
- [ ] Expose current house version, topology version, and portal version.
- [ ] Verify the runtime can answer node adjacency queries, portal destination queries, and current-node lookup per player.
- [ ] Confirm graph truth survives scene load and player join without contradictory initialization.

## Section 5 — Player integration
Players must be real runtime participants, not abstract test stubs.

### Checklist
- [ ] Reuse the first-person controller/input/camera foundation already prioritized in the earlier roadmap rather than inventing a separate movement stack for testing [file:13][file:14][file:15].
- [ ] Spawn two players into valid graph nodes.
- [ ] Ensure each player can move, look, and traverse thresholds normally.
- [ ] Ensure node occupancy updates as players move through the house.
- [ ] Ensure current-node state is available to debug consumers.
- [ ] Ensure threshold entry/exit hooks fire reliably enough to support protection and portal-lock logic.

## Section 6 — Observation system integration
This is the first slice-specific logic layer that differentiates the project.

### Checklist
- [ ] Implement or wire per-player observation contribution updates.
- [ ] Aggregate contributions into shared protected node/portal sets.
- [ ] Distinguish ordinary observation protection from threshold-crossing protection.
- [ ] Expose the contribution source per player.
- [ ] Expose protected sets and protection reasons through read-only snapshots.
- [ ] Verify that stale contributions clear according to policy instead of sticking forever.

## Section 7 — Mutation integration
At least one meaningful mutation path must be real.

### Checklist
- [ ] Integrate a deterministic mutation trigger or seeded mutation scheduler.
- [ ] Support one legal mutation affecting an unprotected region.
- [ ] Support one rejected mutation path affecting a protected node or portal.
- [ ] Ensure mutation evaluation consumes protected sets, not scene intuition.
- [ ] Ensure successful mutation increments topology version and records trace data.
- [ ] Ensure rejected mutation records explicit rejection reasons.
- [ ] Confirm mutation history is visible in debug.

## Section 8 — Portal integration
Portals are where the impossible-space illusion becomes concrete.

### Checklist
- [ ] Wire each authored threshold to a stable portal id.
- [ ] Ensure portals resolve destinations from authoritative runtime state.
- [ ] Expose authoritative destination and local rendered destination separately for debug.
- [ ] Ensure threshold crossing can temporarily protect or lock relevant portal state.
- [ ] Ensure portal destination updates after legal mutations.
- [ ] Ensure unsafe portal remaps are blocked during protected crossing windows.

## Section 9 — Networking and authority integration
The slice does not need production netcode scale, but it does need trustworthy shared truth.

### Checklist
- [ ] Decide and implement host/server authority for the house graph.
- [ ] Ensure host owns house/topology/portal version truth.
- [ ] Ensure clients receive authoritative snapshots or equivalent state updates.
- [ ] Ensure clients do not invent alternate graph truth locally.
- [ ] Support stale-state detection.
- [ ] Support a basic reconciliation path.
- [ ] Verify that authority, observation, mutation, and portal state remain interpretable under host/client conditions.

## Section 10 — Debug overlay integration
The slice is not done unless it is inspectable, consistent with the project’s debug-first rule [file:12][file:13][file:15].

### Checklist
- [ ] Integrate the HUD summary strip.
- [ ] Integrate Authority, Observation, Mutation, Portal, and Reconciliation panels.
- [ ] Integrate world gizmos for nodes, portals, protected ids, and player current nodes.
- [ ] Ensure debug uses read-only snapshots/contracts rather than mutating runtime state.
- [ ] Ensure the overlay answers “what is true, for whom, and why?” during live play [file:12][file:13][file:15].
- [ ] Verify debug can be toggled quickly during multiplayer sessions.

## Section 11 — Deterministic test harness integration
You already have a graybox test plan; this section makes the slice runnable against it.

### Checklist
- [ ] Add a deterministic house preset or test seed.
- [ ] Add simple controls or dev commands to trigger legal and illegal mutation candidates.
- [ ] Add a reset path that restores the graph and players to known starting conditions.
- [ ] Add a way to simulate or force stale-client/reconciliation scenarios.
- [ ] Ensure the slice supports the scenario families in the graybox test plan without custom one-off hacks each time.

## Section 12 — Minimal UX and interaction glue
Even a graybox slice needs enough UX to be usable by humans.

### Checklist
- [ ] Ensure players can start, re-enter, and reset the slice without editor surgery.
- [ ] Ensure a minimal HUD exists for orientation and debug toggles.
- [ ] Ensure obvious failure conditions are visible during testing, such as desync/stale warning badges.
- [ ] Keep all UX strictly prototype-grade; avoid overbuilding production-facing presentation [file:12][file:13][file:15].

## Section 13 — Logging and capture support
Fast bug triage requires lightweight evidence.

### Checklist
- [ ] Add concise structured logs for mutation decisions, portal changes, and reconciliation actions.
- [ ] Ensure major debug panels can be corroborated by logs when needed.
- [ ] Ensure screenshots/video captures can be tied back to visible ids and versions.
- [ ] Avoid log spam so severe that real failures disappear inside noise.

## Section 14 — Manual smoke acceptance
Before deeper scenario testing, the slice should pass a quick manual smoke sequence.

### Smoke checklist
- [ ] Host and client both load into the same graybox scene.
- [ ] Both can move normally.
- [ ] Both receive valid current-node state.
- [ ] Observation protection appears when expected.
- [ ] One legal mutation can succeed.
- [ ] One illegal mutation can be rejected for the correct reason.
- [ ] Portal state changes or blocks correctly.
- [ ] Debug overlay explains the above.
- [ ] Reset works.

## Section 15 — Graybox test plan handoff gate
The slice should not be considered “integrated” until it can genuinely enter the formal test plan.

### Checklist
- [ ] The slice can run baseline authority tests.
- [ ] The slice can run observation aggregation tests.
- [ ] The slice can run threshold crossing tests.
- [ ] The slice can run legal/illegal mutation tests.
- [ ] The slice can run portal authority tests.
- [ ] The slice can run stale-client and reconciliation tests.
- [ ] Any unimplemented scenario is clearly labeled as not yet supported rather than silently failing.

## Section 16 — Claude Code task slicing guidance
This integration checklist should also control how work is delegated to Claude, consistent with the earlier lesson that AI implementation goes better when tasks are bounded and contract-driven [file:12][file:13][file:15].

### Preferred task granularity
Ask Claude for one narrow integration unit at a time, such as:
- bootstrap registration for graph services,
- runtime graph load from definition,
- player current-node tracker,
- observation snapshot provider,
- mutation evaluator integration,
- portal debug presenter,
- stale-client detection panel,
- or reset harness.

### Avoid
- “build the networked house system,”
- “implement multiplayer impossible geometry,”
- or other giant prompts that blur ownership and acceptance criteria.

## Section 17 — Common integration failure modes
This checklist should help catch these likely problems early.

### Watch for
- duplicate service initialization,
- scene-authored assumptions overriding runtime graph truth,
- player movement not updating node occupancy,
- threshold events firing unreliably,
- mutation logic reading stale protection data,
- portal debug showing only local render state rather than authority,
- stale-client conditions being mistaken for logic bugs,
- and debug UI mutating runtime state accidentally.

## Exit criteria
The vertical slice integration is in good shape when all of the following are true:
- the graybox scene launches through the intended startup path [file:13][file:14],
- two players share a coherent authoritative house state,
- the graph, observation, mutation, and portal systems are genuinely wired together,
- debug surfaces explain what the systems are doing [file:12][file:13][file:15],
- the deterministic harness can trigger the important scenario families,
- and the slice is stable enough to enter the formal graybox test plan rather than living as a one-off demo [file:12][file:13][file:15].

## Recommended next document
After this checklist, the most useful next doc is a **Claude Code Implementation Prompt Pack for the Vertical Slice**, a task-by-task set of bounded prompts that map directly to the checklist sections and acceptance gates. That would let you start executing the slice with much less prompt thrash.

