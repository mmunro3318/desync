# Claude Code Implementation Prompt Pack for the Vertical Slice

## Document objective
Provide a bounded set of Claude Code prompts for implementing the first networked impossible-house vertical slice in Unity. Each prompt is designed to be narrow, testable, and aligned with the project’s established architecture rules: runtime vs definition separation, thin scene objects, debug-first hidden-state development, graybox-first validation, and small milestone slices [file:12][file:13][file:14][file:15].

## Why this document exists
The project now has enough design and systems documentation that broad prompts like “build the networked house” would create unnecessary drift. Earlier planning already showed that AI-assisted development works much better when context is stable, ownership is clear, and acceptance criteria are explicit. This pack converts the vertical slice into Claude-sized tasks [file:12][file:13][file:15].

## How to use this pack
Use one prompt at a time. Give Claude only the docs directly relevant to the prompt plus the current codebase context. After each prompt, verify the acceptance criteria in Unity before moving to the next one. Do **not** merge multiple prompts into one request unless they are truly tiny and share the same system owner [file:12][file:13][file:15].

## Prompt design rules
Each prompt below is structured to reduce implementation thrash.

### Rules
- Keep one system owner per prompt.
- Ask for concrete files/classes, not vague behaviors.
- Require debug visibility for hidden state.
- Require no placeholder TODOs.
- Require a short implementation summary from Claude at the end.
- Require a manual verification checklist or play-mode validation steps.

## Recommended doc bundle for Claude
Before prompting Claude, provide an explicit file map because the old `[file:##]` references are not preserved as repo-native links. The key project docs should be referred to by filename/path in your repo or docs folder, not by temporary chat ids [file:12][file:13][file:14][file:15].

## Prompt order
This pack is sequenced so each prompt answers one specific integration question.

| Order | Prompt focus | Main question answered |
|---|---|---|
| 1 | Composition root and scene bootstrap | can the slice start cleanly? |
| 2 | House graph definitions and runtime load | can the house exist as authoritative graph data? |
| 3 | Player current-node tracking | can players be located inside graph truth? |
| 4 | Observation contribution service | can player observation become structured runtime data? |
| 5 | Aggregated protection ledger | can shared protected state be derived correctly? |
| 6 | Threshold crossing hooks | can doorway traversal create safety windows? |
| 7 | Deterministic mutation evaluator | can legal/illegal mutations be decided reproducibly? |
| 8 | Portal authority service | can thresholds resolve to authoritative destinations? |
| 9 | Network snapshot sync | can clients receive and display shared house truth? |
| 10 | Reconciliation path | can stale clients recover? |
| 11 | Debug overlay summary + authority panels | can truth be inspected quickly? |
| 12 | Debug observation/mutation/portal panels | can reasons be inspected deeply? |
| 13 | World gizmo renderer | can spatial debug align with panel truth? |
| 14 | Test harness + reset controls | can the slice repeatedly run graybox scenarios? |

## Prompt 1 — Composition root and bootstrap
### Goal
Set up the vertical slice startup path and runtime composition root.

### Prompt
Create the runtime composition root for the networked house graybox vertical slice in Unity.

Constraints:
- Use the project’s existing `_Project` structure and keep ownership narrow.
- Do not create giant manager gods.
- Scene objects should stay thin.
- The composition root should register only the services needed for the vertical slice: graph runtime, observation, mutation, portal authority, reconciliation, and debug providers.
- Assume a dedicated `NetworkedHouse_Graybox` test scene.
- Add a clean startup path from `Bootstrap` or an equivalent startup entry.
- Prevent duplicate initialization on scene reload.
- Add concise debug logs showing startup order.

Deliverables:
- New or updated bootstrap/composition classes.
- Clear namespace placement.
- Brief summary of created files and responsibilities.
- Manual verification steps for entering/re-entering the scene.

Acceptance criteria:
- The scene loads through the intended startup path.
- Services initialize in a deterministic order.
- Re-entering the scene does not duplicate service instances.

## Prompt 2 — House graph definition + runtime load
### Goal
Load a deterministic house graph into runtime state.

### Prompt
Implement the first deterministic house graph definition and runtime loader for the networked impossible-house vertical slice.

Constraints:
- Keep design-time graph definition separate from runtime graph state.
- Use stable ids for nodes, edges, and portals.
- Support a tiny test graph: Entry, HallA, HallB, SideRoom, LoopReturn, ExitAnchor.
- Provide adjacency queries and portal destination queries.
- Do not rely on scene hierarchy names as runtime truth.
- Add debug-readable snapshot data for houseVersion, topologyVersion, portalVersion, node count, and portal count.

Deliverables:
- Graph definition asset/classes.
- Runtime graph state classes/services.
- Deterministic load path from the composition root.
- Manual verification steps.

Acceptance criteria:
- The graph loads on play.
- Stable ids exist.
- Runtime graph queries return expected results.
- Versions and counts are visible to debug consumers.

## Prompt 3 — Player current-node tracking
### Goal
Make player presence real inside graph truth.

### Prompt
Implement player current-node tracking for the networked house vertical slice.

Constraints:
- Reuse existing first-person controller foundations where possible.
- Each player must report a current node id based on graph occupancy rules.
- Keep the tracking logic outside the raw scene objects where possible.
- Expose current-node data through a read-only debug snapshot.
- Add lightweight logs for node transitions.

Deliverables:
- Player node tracking component/service.
- Hookup between player movement and graph occupancy.
- Debug snapshot fields for current node per player.
- Manual verification steps for walking across node boundaries.

Acceptance criteria:
- Each player receives a valid current node id.
- Crossing between regions updates the id reliably.
- Debug can display current node per player.

## Prompt 4 — Observation contribution service
### Goal
Turn per-player observation into structured runtime data.

### Prompt
Implement the observation contribution service for the networked house vertical slice.

Constraints:
- Observation contribution should be per-player.
- Output should identify protected node ids and protected portal ids contributed by that player.
- Keep this distinct from aggregated shared protection.
- Expose contribution reasons or source info where useful.
- Use read-only snapshot models for debug.

Deliverables:
- Observation contribution service.
- Per-player observation snapshot model.
- Update flow tied to player camera/state.
- Manual verification steps with two players looking at different regions.

Acceptance criteria:
- Each player contribution is inspectable independently.
- Observation results update predictably as players move/look.
- No aggregated union logic is mixed directly into this service.

## Prompt 5 — Aggregated protection ledger
### Goal
Derive shared protected state from valid contributions.

### Prompt
Implement the aggregated protection ledger for the networked house vertical slice.

Constraints:
- Consume per-player observation contributions.
- Produce shared protected node and portal sets.
- Preserve source attribution where possible for debug.
- Support stale/expired contribution clearing according to policy.
- Expose reasons and counts through debug snapshots.

Deliverables:
- Protection ledger service.
- Aggregated snapshot model.
- Clear update rules.
- Manual verification steps.

Acceptance criteria:
- Shared protection is the union of valid contributions.
- Expired contributions do not remain forever.
- Debug can show both aggregate and source detail.

## Prompt 6 — Threshold crossing hooks
### Goal
Add a temporary protection model for unsafe remap prevention during crossings.

### Prompt
Implement threshold crossing detection and temporary crossing-protection hooks for portal traversal.

Constraints:
- Detect when a player is actively crossing a tracked portal/threshold.
- Raise temporary protection or lock state associated with that crossing.
- Ensure the state expires correctly after crossing/grace time.
- Expose the crossing-protection reason in debug.
- Avoid hardcoding one-off door logic directly inside unrelated systems.

Deliverables:
- Threshold crossing tracker.
- Protection/lock integration hooks.
- Debug snapshot fields and logs.
- Manual verification steps for crossing and expiry.

Acceptance criteria:
- Crossing protection activates during traversal.
- Protection expires predictably.
- Repeated crossing does not create stuck permanent state.

## Prompt 7 — Deterministic mutation evaluator
### Goal
Support at least one legal and one rejected mutation path.

### Prompt
Implement a deterministic mutation evaluator for the vertical slice.

Constraints:
- Support a seeded or explicit trigger path rather than fully random mutation.
- Support at least one legal mutation in an unprotected region.
- Support at least one rejected mutation when a protected node or portal is affected.
- Record reason-rich decision traces.
- Increment topology version only on committed mutation.
- Expose recent mutation history for debug.

Deliverables:
- Mutation candidate/evaluator classes.
- Decision trace data model.
- Trigger path for test scenarios.
- Manual verification steps for legal and rejected cases.

Acceptance criteria:
- Legal mutation can commit.
- Illegal mutation can be rejected with explicit reason.
- Debug can show the last decision and recent history.

## Prompt 8 — Portal authority service
### Goal
Make thresholds resolve from runtime truth instead of scene assumptions.

### Prompt
Implement the portal authority service for the networked house vertical slice.

Constraints:
- Each portal has a stable id.
- The service owns authoritative destination lookup.
- Local rendered destination and authoritative destination should be separable for debug.
- Portal state should respect threshold crossing protection/locks.
- Portal version updates should be visible in snapshots.

Deliverables:
- Portal authority service.
- Portal snapshot model.
- Integration with mutation results.
- Manual verification steps for destination lookup and remap.

Acceptance criteria:
- Portals resolve destinations through authoritative runtime state.
- Unsafe remaps during crossing are blocked.
- Debug can display authority vs local render state.

## Prompt 9 — Network snapshot sync
### Goal
Get authoritative house truth from host to client.

### Prompt
Implement the minimum network snapshot sync layer for the vertical slice.

Constraints:
- Host/server is the authority for house version, topology version, portal version, and other required shared snapshots.
- Clients consume authoritative state rather than inventing local truth.
- Keep the implementation minimal and slice-oriented.
- Expose current snapshot freshness in debug.
- Do not overbuild production networking features.

Deliverables:
- Snapshot transport/update flow.
- Host-authoritative state publication.
- Client-side consumption path.
- Manual verification steps with host and one client.

Acceptance criteria:
- Client receives authoritative house state.
- Version values align in normal conditions.
- Debug can display freshness/current snapshot data.

## Prompt 10 — Reconciliation path
### Goal
Recover from stale client state.

### Prompt
Implement a minimal reconciliation path for stale-client recovery in the networked house vertical slice.

Constraints:
- Detect client stale or mismatched versions.
- Provide a clear reconciliation state and last action for debug.
- Reconcile toward authority, not toward a separate local interpretation.
- Keep the path minimal but real enough for graybox test cases.

Deliverables:
- Stale detection logic.
- Reconciliation action flow.
- Reconciliation snapshot/debug fields.
- Manual verification steps.

Acceptance criteria:
- Stale clients can be identified.
- Reconciliation restores client state to authority.
- Debug explains what happened.

## Prompt 11 — Debug overlay summary + authority panels
### Goal
Make basic shared truth inspectable in live play.

### Prompt
Implement the first debug overlay layer for the networked house vertical slice: HUD summary strip plus Authority panel.

Constraints:
- Use read-only snapshot providers.
- Show current player node, houseVersion, topologyVersion, portalVersion, stale/reconciled status, and last mutation status in the summary strip.
- Authority panel should show source-of-truth state clearly.
- Keep the overlay keyboard-toggleable.
- Keep the UI prototype-grade; clarity matters more than polish.

Deliverables:
- HUD summary presenter.
- Authority panel presenter.
- Input toggle hookup.
- Manual verification steps.

Acceptance criteria:
- During play, a developer can quickly identify shared version truth.
- Overlay toggles cleanly.
- No runtime state is mutated by the debug UI.

## Prompt 12 — Debug observation/mutation/portal/reconciliation panels
### Goal
Expose deep reasoning, not just surface numbers.

### Prompt
Implement the deep debug panels for Observation, Mutation, Portal, and Reconciliation.

Constraints:
- Observation must show per-player contribution plus aggregated protection.
- Mutation must show candidate, affected ids, approved/rejected result, and reason.
- Portal must show authority destination vs local rendered destination.
- Reconciliation must show stale state, last action, and current status.
- Keep the panels grouped and readable.

Deliverables:
- Panel presenters and any supporting view models.
- Toggle/cycle controls.
- Manual verification steps using at least one legal and one rejected mutation scenario.

Acceptance criteria:
- A developer can explain why a mutation did or did not happen from the panel data.
- Portal contradictions are visible rather than hidden.
- Stale-client state is inspectable.

## Prompt 13 — World gizmo renderer
### Goal
Correlate spatial truth with panel truth.

### Prompt
Implement world gizmos for the networked house vertical slice.

Constraints:
- Show node ids, portal ids, player current nodes, protected nodes/portals, and optionally last mutation-affected region.
- Keep colors consistent with the debug overlay conventions.
- Support toggles for node-only, portal-only, protection-only, and full view.
- Do not let gizmo rendering own gameplay state.

Deliverables:
- Gizmo renderer(s).
- Toggle controls.
- Manual verification steps.

Acceptance criteria:
- Gizmos align with panel truth.
- Developers can visually inspect protected and mutated regions in-world.

## Prompt 14 — Test harness + reset controls
### Goal
Make the slice easy to rerun against the graybox test plan.

### Prompt
Implement the deterministic graybox test harness controls for the networked house vertical slice.

Constraints:
- Add a deterministic preset/seed or explicit initialization for the small test graph.
- Add simple commands or UI hooks to trigger legal mutation, illegal mutation, and stale/reconciliation scenarios.
- Add a full reset path returning players and graph state to known defaults.
- Keep the harness clearly separated from shipping gameplay code where practical.

Deliverables:
- Test harness entry points.
- Reset logic.
- Minimal dev controls.
- Manual verification steps showing the slice can rerun multiple scenarios without editor surgery.

Acceptance criteria:
- The slice can be reset reliably.
- Core graybox scenarios can be triggered on demand.
- Testers can rerun scenarios quickly.

## Prompt hygiene template
Use this wrapper around any prompt above.

### Template
Before writing code:
- Read only the attached project docs relevant to this task.
- Preserve existing architecture rules: runtime vs definition separation, thin scene objects, debug-first hidden-state support, narrow system ownership.
- Do not introduce placeholder TODO blocks.
- Do not silently rename major concepts without explaining why.
- If a dependency is missing, implement the minimum required local seam and state it clearly.

After writing code:
- Summarize files created/modified.
- Summarize system ownership.
- Provide Unity manual verification steps.
- Call out any assumptions or follow-up tasks.

## Recommended execution rhythm
The earlier project docs emphasized milestone slices and explicit acceptance conditions. Keep that rhythm here too: run one prompt, verify in editor, update docs if contracts shifted, then move to the next prompt [file:12][file:13][file:15].

