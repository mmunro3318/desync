# Networked House Debug Overlay Spec

## Document objective
Define the debug HUD, panels, traces, gizmos, filters, and required runtime read models for the networked impossible-house prototype. The overlay must make hidden multiplayer house state inspectable enough to tune graph truth, observation protection, mutation legality, portal authority, reconciliation, and player-local presentation without guesswork [file:12][file:13][file:14][file:15].

## Why this document exists
The project’s earlier docs repeatedly establish a core rule: hidden-state systems must expose debug data early or they become miserable to tune. That rule applied to ghosts, evidence, and round state in the clone planning, and it applies even more strongly now that the house itself has mutable topology, co-op observation semantics, portal truth, and authority/reconciliation behavior [file:12][file:13][file:15].

## Core thesis
The impossible house is not debuggable through scene view intuition alone. The project needs a first-class debug overlay that reveals shared truth, local truth, and the reasons decisions were made. If the overlay cannot answer “what is true right now, for whom, and why,” then multiplayer spatial-horror iteration will be too slow and too error-prone [file:12][file:13][file:15].

## Design goals
The overlay should optimize for:
- fast diagnosis,
- low friction during playtests,
- clear separation of shared versus local state,
- reason-rich mutation decisions,
- and support for two-player graybox testing first [file:12][file:13][file:15].

## Questions the overlay must answer
At minimum, the overlay must be able to answer:
- What node is each player in?
- What topology version is authoritative right now?
- Which nodes and portals are protected, and by whom?
- Why was a mutation allowed or rejected?
- What portal destination is authoritative versus merely rendered locally?
- Is a client stale, reconciled, or diverged?
- Is threshold crossing protection active?
- What hidden system is currently blocking the behavior I expected?

## Out of scope
This overlay spec does **not** cover:
- final user-facing UI,
- polished accessibility settings for shipping builds,
- production telemetry pipelines,
- or spectator tooling.

This is a developer-focused runtime observability layer for prototype and graybox phases.

## Architectural rules carried forward
This spec extends the same architecture logic already established elsewhere [file:12][file:13][file:14][file:15].

- Debug must consume public snapshots/contracts rather than private field scraping where possible [file:12][file:13][file:15].
- Scene objects stay thin; debug should not depend on scene-authoring accidents [file:12][file:13][file:14].
- Hidden-state systems must expose reason-rich outputs, not just booleans [file:12][file:13][file:15].
- The debug layer should help preserve narrow ownership by showing which system made which decision [file:12][file:13][file:15].

## Overlay philosophy
The overlay should behave like a set of developer lenses.

### Lens model
- **Global lens** shows authoritative shared truth.
- **Per-player lens** shows local contribution and local presentation.
- **Decision lens** shows why a change happened or was blocked.
- **Spatial lens** shows geometry-linked data in-world.

This is better than one giant wall of text because multiplayer spatial systems fail in different ways, and each failure mode needs a focused lens.

## Presentation form
The overlay should exist in three complementary forms.

### 1. HUD panel
A compact on-screen overlay for live play.

### 2. Expandable debug window/panels
A richer set of grouped panels for deep inspection.

### 3. World gizmos
In-world node, portal, edge, and protection visuals for scene/game view correlation.

All three are needed. HUD alone is too shallow. Gizmos alone are too spatially noisy. Panels alone are too slow during active playtests.

## Toggle model
The overlay should be toggled in layers rather than all at once.

### Minimum toggles
- master debug on/off,
- HUD summary on/off,
- global authority panel,
- per-player panels,
- mutation trace panel,
- portal panel,
- reconciliation panel,
- world gizmos on/off,
- text labels on/off,
- spam/log verbosity mode.

## Input expectations
The current starter project already assumes a debug panel and toggleable hidden-state UI, so this overlay should extend that same workflow rather than invent a new one [file:13][file:15].

### Suggested controls
- backquote or F1 toggles master overlay,
- number keys or function keys cycle panel groups,
- bracket keys cycle tracked player lens,
- semicolon toggles gizmo labels,
- quote toggles mutation traces.

Exact bindings can change, but fast keyboard access matters.

## Minimum top-level panel groups
The overlay should have six primary panel groups.

| Panel | Purpose |
|---|---|
| Summary | one-screen health check for the networked house |
| Authority | shared truth, versions, active topology state |
| Observation | per-player contributions and aggregated protections |
| Mutation | last decisions, current candidates, block reasons |
| Portals | authoritative portal truth vs local presentation |
| Reconciliation | client freshness and correction state |

## 1. Summary panel
The Summary panel is the “something is wrong, where do I start?” surface.

### Must show
- current house version,
- topology version,
- portal version,
- player count,
- active protected node count,
- active protected portal count,
- last mutation result,
- count of stale clients,
- count of active threshold protections.

### Purpose
A developer should be able to glance at this panel and know whether the current failure is likely an authority issue, observation issue, mutation issue, or reconciliation issue.

## 2. Authority panel
This panel exposes the host/server view of shared truth.

### Must show
- authority role/status,
- authoritative current node graph version,
- authoritative portal version,
- active graph seed or layout id,
- active house snapshot id/version,
- last committed mutation id,
- last published snapshot time or sequence,
- current graph node count and active portal count.

### Optional but useful
- last authority-side snapshot publish reason,
- authority-side queued mutation requests count.

### Why it matters
This is the source-of-truth panel. If this panel is unclear, every other panel becomes harder to interpret.

## 3. Observation panel
This panel must support both aggregated and per-player inspection.

### Aggregated section must show
- protected node ids,
- protected portal ids,
- protection reasons currently active,
- grace-window protections,
- threshold-crossing protections.

### Per-player section must show
- player id,
- current node id,
- current portal crossing state,
- locally contributed protected node ids,
- locally contributed protected portal ids,
- contribution age / last update sequence,
- whether the contribution is considered valid by authority.

### Visual rules
- shared protections should use one color family,
- player A and player B contributions should use distinct colors,
- expired or stale contributions should fade rather than pop instantly.

### Why it matters
In co-op impossible-space logic, many of the ugliest bugs come from disagreement over whether someone “was still observing.” This panel must make that answer obvious.

## 4. Mutation panel
This panel is the reasoning engine view.

### Must show
- last mutation candidate id,
- candidate type,
- candidate affected node ids,
- candidate affected portal ids,
- last evaluation result (approved/rejected),
- explicit rejection reason,
- which protected ids caused rejection,
- topology version before and after commit if committed,
- mutation timestamp/sequence.

### History subpanel
Keep a short rolling history of recent mutation evaluations.

Suggested fields per row:
- sequence,
- candidate id,
- approved/rejected,
- reason,
- affected ids count,
- resulting topology version.

### Why it matters
A spatial-horror mutation system without decision trace visibility is basically untunable. This panel is mandatory.

## 5. Portal panel
This panel explains how doors/thresholds resolve.

### Must show
- portal id,
- authoritative destination node id,
- local rendered destination node id,
- portal version,
- whether portal is protected,
- whether portal is transition-locked,
- whether portal is currently in threshold crossing use,
- last portal authority change sequence.

### Special requirement
If authoritative and local rendered destination differ temporarily, the panel must display that state clearly rather than hiding it.

### Why it matters
Portal weirdness is one of the most likely sources of “the house feels broken” reports. This panel turns that feeling into inspectable facts.

## 6. Reconciliation panel
This panel exposes whether a client is aligned with authority.

### Must show per client
- player/client id,
- latest known house version,
- latest known topology version,
- latest known portal version,
- reconciliation status,
- last reconciliation action taken,
- stale duration if stale,
- last mismatch reason.

### Why it matters
Without this panel, you will waste time confusing stale-client presentation with actual authority bugs.

## HUD summary strip
In addition to panels, the overlay needs a very compact always-available strip.

### Must include
- player current node,
- house version,
- topology version,
- protected set count,
- last mutation status,
- current tracked player lens,
- stale/reconciled badge.

### Constraint
This strip should stay compact enough that it does not block navigation testing in first-person play.

## World gizmo requirements
Text panels are not enough. The graph and protection systems must be visible in world space too.

### Required gizmos
- node centers with node ids,
- edges between neighboring nodes,
- portal markers at thresholds,
- color highlight for protected nodes,
- color highlight for protected portals,
- line or pulse indicating active threshold crossing,
- marker for player current node,
- optional marker for last mutation-affected region.

### Color recommendation
- authority/shared protection: cyan,
- player A contribution: green,
- player B contribution: magenta,
- rejected mutation region: red,
- recently mutated region: amber,
- stale local-only presentation discrepancy: orange.

### Gizmo mode toggles
- node-only,
- node + edge,
- portal-only,
- protection-only,
- full graph,
- last mutation only.

## Required debug read models
The overlay must be backed by public snapshot providers, consistent with the contracts/architecture approach [file:12][file:13][file:15].

### Minimum snapshot models
- `HouseDebugSnapshot`
- `AuthorityDebugSnapshot`
- `ObservationDebugSnapshot`
- `PlayerObservationDebugSnapshot`
- `MutationDecisionTrace`
- `PortalDebugSnapshot`
- `ReconciliationDebugSnapshot`

### Snapshot design rule
Snapshots must be read-only from the overlay’s perspective. The overlay is not allowed to mutate gameplay state directly.

## Suggested data fields per debug snapshot

### `AuthorityDebugSnapshot`
- `houseVersion`
- `topologyVersion`
- `portalVersion`
- `lastCommittedMutationId`
- `lastPublishedSnapshotSequence`
- `activeNodeCount`
- `activePortalCount`

### `ObservationDebugSnapshot`
- `protectedNodeIds`
- `protectedPortalIds`
- `protectionReasons`
- `activeThresholdProtectionCount`
- `playerSnapshots`

### `PlayerObservationDebugSnapshot`
- `playerId`
- `currentNodeId`
- `protectedNodeIds`
- `protectedPortalIds`
- `isThresholdCrossing`
- `lastContributionSequence`
- `isContributionStale`

### `PortalDebugSnapshot`
- `portalId`
- `authoritativeDestinationNodeId`
- `localRenderedDestinationNodeId`
- `portalVersion`
- `isProtected`
- `isTransitionLocked`

### `ReconciliationDebugSnapshot`
- `clientId`
- `knownHouseVersion`
- `knownTopologyVersion`
- `knownPortalVersion`
- `reconciliationStatus`
- `lastAction`
- `staleDuration`

## Reason visibility rules
Booleans are not enough.

### The overlay must expose reasons for
- mutation rejection,
- threshold protection creation,
- contribution invalidation,
- reconciliation action,
- portal authority change,
- and stale-client detection.

### Example
Instead of showing only `MutationBlocked = true`, show:
- `Rejected: affected protected portal P_Hall_02 still protected by PlayerB threshold crossing`.

The exact sentence format can vary, but the principle is mandatory.

## Ordering and grouping rules
The overlay should help the brain parse causality.

### Panel order recommendation
1. Summary
2. Authority
3. Observation
4. Mutation
5. Portals
6. Reconciliation

### Within each panel
Show current state first, then recent history, then raw ids/details.

## History and trace depth
A small rolling history matters more than infinite logs.

### Recommended retained history
- last 10 mutation decisions,
- last 10 portal authority changes,
- last 10 reconciliation actions per client,
- last 10 threshold protection state changes.

This is enough for graybox debugging without becoming unreadable.

## Noise control
A good debug overlay must avoid becoming a firehose.

### Required filters
- tracked player filter,
- tracked portal filter,
- tracked node filter,
- show only stale issues,
- show only rejected mutations,
- hide idle/unchanged values,
- freeze snapshot view.

### Freeze mode
The overlay should support freezing the current debug snapshot while gameplay continues or pauses, so a developer can inspect a fast mutation event after it happens.

## Pause and stepping support
If feasible, the overlay should work well with pause/slow time during graybox testing.

### Useful controls
- pause and inspect current snapshots,
- single-step mutation evaluation if your runtime later supports it,
- clear recent history,
- force refresh snapshot display.

## Performance expectations
This is a debug tool, but it still should not crater iteration performance.

### Rules
- avoid allocating large strings every frame when possible,
- update detailed panels at a throttled cadence if needed,
- separate always-on HUD data from heavy history rendering,
- allow gizmos and traces to be disabled independently.

## Build/config rules
The debug overlay should be easy to include in development and easy to exclude from shipping or non-dev builds.

### Recommended behavior
- enabled by default in editor/dev build,
- disabled or stripped in release configuration,
- panel registration driven by modular providers.

## Claude Code implementation implications
When asking Claude to build any part of this overlay, prompts should be bounded to one of these surfaces: snapshot provider, panel presenter, HUD strip, gizmo renderer, history trace, or toggle/input layer. This matches the earlier project lesson that bounded implementation requests work far better than vague “build debug UI” prompts [file:12][file:13][file:15].

## Acceptance checklist
This overlay spec is fulfilled when:
- a developer can inspect authoritative shared house truth during live play [file:12][file:13][file:15],
- a developer can inspect each player’s local observation contribution separately from aggregated protections,
- mutation decisions expose reasons, not just outcomes,
- portal authority and local rendered portal state can both be inspected,
- stale/reconciliation states are clearly visible per client,
- node/portal/protection data can be visualized both in panels and in world gizmos,
- and the overlay helps answer “what is true, for whom, and why?” quickly enough to support graybox multiplayer iteration [file:12][file:13][file:15].

## Recommended next document
After this spec, the strongest next step is **Networked House Graybox Test Plan**, because once the debug surfaces are defined, the next leverage point is a concrete test matrix of the exact multiplayer anomaly scenarios you want to run repeatedly while building the runtime.

