# Sprint 1B — Portal Visibility and Node Activation Harness

## Sprint objective
Implement the first render/runtime harness that decides which room-nodes are active, which target spaces are visible through active portals, and how the baseline exterior remains canonical while extra-normal interior content stays hidden unless locally relevant. Sprint 1B is successful when a player can stand inside the authored test slice, look through a doorway, and the correct adjacent node presentation is active without exposing impossible geometry from the outside [file:12][file:13][file:14].

## Sprint question
Can the project make graph composition and rendering cooperate at a basic playable level before adding mutation logic or portal-polish effects [file:12][file:13]?

## Why this sprint exists
Sprint 1A turns the house into a formal graph and runtime shell. The next technical bottleneck is visibility: a graph alone does not solve what should be loaded, what should be culled, and what a player can see through a threshold. This sprint exists to define and implement that bridge between graph truth and scene presentation while keeping responsibilities narrow and debuggable, which follows the earlier architecture principle that scene objects stay thin and hidden systems expose their state early [file:12][file:13][file:14].

## In scope
- Node activation and deactivation rules.
- Portal visibility rules for adjacent spaces.
- Local active-node set resolution.
- Interior-only handling for future shadow and extra-normal nodes.
- Debug readout for why nodes are active, visible, or inactive.
- Simple baseline implementation in `House_Prototype`.

## Out of scope
- Fancy portal shaders or recursive portal rendering.
- Loop mutations.
- Room substitutions.
- Shadow-house runtime bridging.
- Tardis subgraph insertion.
- Observation locks.
- Anchor gameplay.
- Entity behavior.

## Design rules
This sprint should preserve the project-wide architecture constraints already established in prior docs:
- runtime state is separate from authored content definitions [file:12][file:13],
- scene objects stay thin and describe locations/presentation rather than global logic [file:12][file:13],
- systems should have narrow responsibilities rather than collapse into a giant manager [file:14][file:15],
- and hidden runtime state must be visible in debug from the beginning [file:12][file:13][file:14].

## Core rendering constraints
The project must maintain two truths at the same time:
- the outside of the house remains the baseline exterior truth,
- while interior traversal can resolve into different active node compositions.

For this sprint, that means:
- baseline exterior shell stays always canonical,
- baseline interior nodes can activate when occupied, adjacent, or portal-visible,
- future shadow-house and extra-normal nodes are treated as interior-only classes even if not fully implemented yet,
- and nodes that are not locally relevant should not remain active by default.

## Activation model
Use a simple three-set model for the first pass.

### 1. Occupied set
Nodes currently containing at least one player.

### 2. Adjacent set
Nodes directly reachable through an active portal-edge from an occupied node.

### 3. Portal-visible set
Nodes not currently occupied but visible through an active doorway or threshold from the player's current camera context.

The current active node set for Sprint 1B is:
- occupied nodes,
- adjacent nodes,
- and portal-visible nodes.

Everything else is a candidate for deactivation unless a temporary debug override is enabled.

## Visibility model
For this sprint, "portal-visible" should be implemented in the simplest reliable way possible.

Recommended first-pass rule:
- if a portal from the current occupied node is within camera-facing tolerance and not blocked by the local doorway plane or a cheap occlusion test, the destination node is marked portal-visible.

This does not need to be a perfect simulation of final line-of-sight. It needs to be deterministic, explainable, and easy to debug.

## Responsibilities

### `NodeStreamingController`
Owns:
- deciding which node presentations should be active,
- activating/deactivating node GameObjects or presentation roots,
- maintaining the current active-node set,
- exposing reasons for activation.

Does not own:
- graph legality,
- mutation choice,
- or camera rendering tricks.

### `PortalVisibilityController`
Owns:
- determining whether a given portal should expose its destination node as visible,
- evaluating the player's local camera-facing relationship to the portal,
- and reporting visibility results to `NodeStreamingController`.

Does not own:
- graph mutation,
- traversal resolution,
- or scene reset flow.

### `SpatialGraphRuntime`
Owns:
- graph topology,
- node and edge state,
- portal destination resolution.

Does not own:
- activation or visibility presentation policy.

### `SpatialDebugOverlay`
Owns:
- live explanation of active nodes,
- live explanation of portal-visible nodes,
- reason labels such as `Occupied`, `Adjacent`, `PortalVisible`, `DebugForced`.

Does not own gameplay decisions.

## Suggested files

### Runtime / presentation
- `Scripts/World/Graph/Runtime/NodeStreamingController.cs`
- `Scripts/World/Graph/Runtime/NodePresentationHandle.cs`
- `Scripts/World/Graph/Runtime/PortalVisibilityController.cs`
- `Scripts/World/Graph/Runtime/PortalViewProbe.cs`

### Debug
- `Scripts/UI/Debug/SpatialVisibilityDebugOverlay.cs`
- `Scripts/UI/Debug/SpatialVisibilityDebugGizmos.cs`

### Existing dependencies touched
- `SpatialGraphRuntime.cs`
- `PortalResolver.cs`
- `MatchManager.cs`

## Authoring expectations
Each authored room-node in the test slice should expose:
- one stable node id,
- one presentation root GameObject,
- one or more portal anchors,
- one optional activation volume or bounds reference.

Each portal anchor should expose:
- anchor id,
- source node id,
- forward direction,
- optional aperture bounds,
- and destination edge reference.

This keeps the scene side thin and descriptive rather than procedural, matching the earlier scene-object philosophy [file:12][file:13].

## Tasks

### 1. Add presentation handles to authored nodes
- [ ] Create `NodePresentationHandle.cs`.
- [ ] Assign a presentation root for each node in the 5-node test slice.
- [ ] Allow runtime enable/disable per node presentation.

#### Acceptance tests
- [ ] Each authored node has one presentation root.
- [ ] Node presentation can be toggled without breaking player traversal in occupied space.
- [ ] Missing presentation roots report clear validation errors.

### 2. Build node activation controller
- [ ] Create `NodeStreamingController.cs`.
- [ ] Build occupied/adjacent/portal-visible set aggregation.
- [ ] Activate node presentations based on the current active-node set.
- [ ] Add optional debug override to keep all nodes active.

#### Acceptance tests
- [ ] Occupied node remains active at all times.
- [ ] Adjacent nodes activate when connected to the occupied node.
- [ ] Non-local nodes can deactivate cleanly.
- [ ] Debug override can force all nodes active.

### 3. Build portal visibility controller
- [ ] Create `PortalVisibilityController.cs`.
- [ ] Create `PortalViewProbe.cs` or equivalent helper.
- [ ] Evaluate whether a portal is locally visible from the player camera.
- [ ] Feed portal-visible results into `NodeStreamingController`.

#### Acceptance tests
- [ ] Looking at a doorway can activate the destination node as portal-visible.
- [ ] Looking away can remove portal-visible status when no other activation reason exists.
- [ ] Visibility evaluation is deterministic across repeated runs.

### 4. Add debug visibility tools
- [ ] Create `SpatialVisibilityDebugOverlay.cs`.
- [ ] Create `SpatialVisibilityDebugGizmos.cs`.
- [ ] Show current occupied node, active nodes, portal-visible nodes, and activation reasons.
- [ ] Show active portals and current visible destination node ids.

#### Acceptance tests
- [ ] Overlay can be toggled during play.
- [ ] Every active node reports at least one clear activation reason.
- [ ] A developer can explain why a node is active or inactive using only the overlay/gizmos.

### 5. Add reset and stability path
- [ ] Ensure activation state resets correctly on restart.
- [ ] Ensure deactivated nodes do not leave stale runtime references.
- [ ] Run repeated enter/look/turn/restart tests.

#### Acceptance tests
- [ ] Restart clears stale active-node state.
- [ ] No critical console errors occur from repeated toggling.
- [ ] Runtime still resolves graph queries after restart.

## Suggested runtime queries
Sprint 1B should use or expose these narrow queries:
- `GetOccupiedNodeIds()`
- `GetAdjacentNodeIds(nodeId)`
- `GetVisiblePortalDestinations(nodeId)`
- `IsNodeActive(nodeId)`
- `GetNodeActivationReasons(nodeId)`

Keeping the API narrow preserves the anti-sprawl rule from prior docs and makes Claude tasks easier to constrain [file:14][file:15].

## Scene setup guidance
Use the same scene discipline already established in prior planning:
- `Bootstrap` loads `House_Prototype`,
- `House_Prototype` contains the 5-node slice from Sprint 1A,
- `Test` can host isolated visibility experiments if the main slice becomes noisy [file:13][file:14].

## Smoke test
Run this before marking Sprint 1B complete:
- [ ] Launch from `Bootstrap`.
- [ ] Enter `House_Prototype`.
- [ ] Confirm occupied node is active.
- [ ] Confirm adjacent nodes are active.
- [ ] Look directly through at least one doorway and confirm destination node is marked `PortalVisible`.
- [ ] Turn away and confirm portal-visible state updates correctly.
- [ ] Move to a new node and confirm active-node set updates.
- [ ] Toggle debug override and confirm all nodes become active.
- [ ] Restart the round.
- [ ] Confirm activation state resets cleanly with no blocker errors.

## Deferred from Sprint 1B
- Recursive portal rendering.
- Mutation-aware visibility.
- Shadow-house dual rendering.
- Tardis node streaming.
- Exterior window special cases.
- Performance tuning beyond basic correctness.

## Sprint done
Mark complete when:
- [ ] occupied and adjacent node activation works,
- [ ] at least one doorway can expose a destination node as portal-visible,
- [ ] the outside shell remains conceptually canonical,
- [ ] and debug output explains activation state well enough to support the next sprint.
