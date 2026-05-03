# V1 PDD — Spatial Core Vertical Slice

## Sprint objective
Build the first proof-of-fun version of the core spatial mechanic: a house with a canonical graph, a shadow-house layer, and one looping traversal anomaly that can only mutate when unobserved. Sprint 1 is successful when the player can walk a short route, see impossible continuity occur, understand that observation constrains mutation, and inspect all relevant hidden state in debug [file:13][file:14].

## Sprint question
Is the loop/shadow-house traversal mechanic fun and readable enough to serve as the foundation of the entire game?

## In scope
- Reuse current player controller, flashlight, and house shell where possible.
- Build or formalize the canonical room/connector graph.
- Implement base-house and shadow-house layer states.
- Implement one loop anomaly family.
- Implement an observation lock system.
- Add debug overlay and gizmos for graph state and mutation legality.
- Support round boot and quick restart.

## Out of scope
- Anchor destruction.
- Stalker AI.
- Procedural house generation.
- Multiple anomaly families.
- Expedition systems.
- Content polish beyond what is needed for readability.
- Cross-machine networking.

## Core user stories
- As a player, I can walk through the house and form an expectation of where a hallway or door should lead.
- As a player, I can re-enter a route and realize I have been folded into a loop.
- As a player, I can learn that spaces behave differently when observed versus unobserved.
- As a developer, I can see exactly why a connector did or did not mutate through debug output.
- As a developer, I can restart the scenario quickly and reproduce the same test conditions.

## Deliverable slice
One graybox test scenario in `House_Prototype` should include:
- Entry room.
- Hallway A.
- Corner or connector node.
- Door or threshold into a shadow-house equivalent.
- One return path that creates impossible continuity.

This scenario does not need to be large. It needs to be undeniable.

## Systems touched

### 1. Match flow
Create or adapt a small match owner for:
- boot,
- active play,
- debug restart,
- and failure-safe reset.

This follows the earlier roadmap principle that match flow should be orchestrated centrally rather than scattered across scene scripts [file:4][file:13].

### 2. Spatial graph runtime
Create runtime ownership for:
- rooms,
- connectors,
- portal identities,
- adjacency,
- active route resolution,
- and current layer bindings.

The goal is to turn the house from authored meshes into a queryable system.

### 3. Layer state controller
Implement two layer states:
- `BaseHouse`
- `ShadowHouse`

The controller resolves which presentation and routing rules are active for a given connector or zone.

### 4. Observation lock system
A connector or zone can only mutate when:
- it is not in the player's validated observation set,
- it is not currently occupied,
- and its cooldown/grace rules allow mutation.

For Sprint 1, simple and explicit beats clever. Prefer deterministic rules over full visual-permanence simulation.

### 5. Loop anomaly resolver
Create one legal mutation type:
- a connector remaps to produce a loop,
- or a threshold resolves to the shadow-house route instead of the expected base route.

This mutation must be inspectable and reversible through restart.

### 6. Debug layer
The debug layer is mandatory, following the earlier hidden-state architecture rule that important invisible systems must expose live state for tuning [file:12][file:13][file:14].

Debug should show:
- current room,
- current layer,
- current connector mapping,
- candidate mutation targets,
- lock reasons blocking mutation,
- last mutation event,
- and restart shortcut state.

## Suggested file map
Adapt the existing predictable project structure because that earlier approach reduces sprawl and gives each system a stable home [file:13][file:14].

### Suggested folders
- `Scripts/Spatial/Definitions`
- `Scripts/Spatial/Runtime`
- `Scripts/Spatial/Observation`
- `Scripts/Spatial/Anomalies`
- `Data/Spatial`
- `Data/Anomalies`
- `Prefabs/Debug`

### Suggested first files
- `MatchState.cs`
- `MatchManager.cs`
- `HouseLayerType.cs`
- `HouseGraphDefinition.cs`
- `RoomNodeAuthoring.cs`
- `ConnectorAuthoring.cs`
- `SpatialGraphRuntime.cs`
- `LayerStateController.cs`
- `ObservationLockSystem.cs`
- `AnomalyDefinition.cs`
- `LoopAnomalyResolver.cs`
- `SpatialDebugOverlay.cs`
- `SpatialDebugGizmos.cs`

## Contracts and responsibilities

### `HouseGraphDefinition`
Owns static graph content:
- room ids,
- connector ids,
- adjacency,
- legal loop targets,
- layer-compatible destinations.

Does not own runtime mutation state.

### `SpatialGraphRuntime`
Owns runtime graph state:
- current active connector mappings,
- active layer bindings,
- occupied nodes,
- mutation history.

Does not own presentation or UI.

### `ObservationLockSystem`
Owns:
- visibility-derived locks,
- occupancy locks,
- grace timers,
- mutation eligibility queries.

Does not own the mutation decision itself.

### `LoopAnomalyResolver`
Owns:
- selecting legal loop remaps,
- validating preconditions,
- applying a remap,
- recording mutation events.

Does not own player movement.

### `SpatialDebugOverlay`
Owns:
- text/UI presentation of hidden state,
- developer hotkeys,
- current mutation readouts.

Does not drive gameplay.

## Task breakdown

### A. Formalize the play slice
- [ ] Define one minimal test floorplan in `House_Prototype`.
- [ ] Tag room nodes and connectors.
- [ ] Assign stable ids to authored graph elements.
- [ ] Add one intended loop route and one intended shadow transition route.

#### Acceptance tests
- [ ] Every room and connector in the slice has a stable id.
- [ ] The slice can be traversed without anomalies enabled.
- [ ] The intended loop path is authorable in data, not hardcoded into one random script.

### B. Build graph runtime shell
- [ ] Create `HouseLayerType.cs`.
- [ ] Create `HouseGraphDefinition.cs`.
- [ ] Create `SpatialGraphRuntime.cs`.
- [ ] Build startup path that loads graph definition on round start.
- [ ] Expose current node and connector mapping to debug.

#### Acceptance tests
- [ ] Runtime graph initializes without null errors.
- [ ] Connectors can be queried by id.
- [ ] Active destination for a connector can be read at runtime.
- [ ] Restart resets graph state cleanly.

### C. Implement layer switching
- [ ] Create `LayerStateController.cs`.
- [ ] Support `BaseHouse` and `ShadowHouse` route resolution.
- [ ] Add minimal presentation difference for shadow layer, such as material swap, light tint, fog, or audio send.

#### Acceptance tests
- [ ] Player can cross a threshold that resolves into the shadow-house route.
- [ ] Current layer is visible in debug.
- [ ] Shadow-house state is visually distinct enough to read immediately.

### D. Implement observation locks
- [ ] Create `ObservationLockSystem.cs`.
- [ ] Track current room/connector occupancy.
- [ ] Track simple line-of-sight or trigger-based observation eligibility.
- [ ] Add a short grace timer after leaving observation.
- [ ] Expose lock reasons to debug.

#### Acceptance tests
- [ ] Observed connectors cannot mutate.
- [ ] Unobserved connectors can become eligible.
- [ ] Lock reasons are visible and correct in debug.
- [ ] Mutation eligibility changes predictably when the player turns away or exits the area.

### E. Implement one loop anomaly
- [ ] Create `AnomalyDefinition.cs`.
- [ ] Create `LoopAnomalyResolver.cs`.
- [ ] Define one legal remap set for the test slice.
- [ ] Apply remap only when `ObservationLockSystem` permits it.
- [ ] Record the last mutation event.

#### Acceptance tests
- [ ] A hallway or connector can resolve into a loop.
- [ ] The loop does not break collision, traversal, or player orientation.
- [ ] The player can observe the practical effect even if the actual swap occurred off-screen.
- [ ] Restart restores original topology.

### F. Debug and iteration harness
- [ ] Create `SpatialDebugOverlay.cs`.
- [ ] Create `SpatialDebugGizmos.cs`.
- [ ] Add hotkeys for restart, force mutation, freeze mutation, and show ids.
- [ ] Add log panel for last mutation source, target, and blocked reason.

#### Acceptance tests
- [ ] Debug can be toggled on/off in play mode.
- [ ] A developer can force one mutation for testing.
- [ ] A developer can explain any blocked mutation from the overlay alone.
- [ ] No critical console errors appear during repeated test runs.

## Smoke test
Run this before calling Sprint 1 complete:
- [ ] Launch from `Bootstrap`.
- [ ] Enter `House_Prototype` test slice.
- [ ] Traverse baseline route with mutations disabled.
- [ ] Enable mutations and verify one legal loop can occur.
- [ ] Confirm observed connector does not mutate.
- [ ] Turn away or leave area and confirm mutation becomes legal.
- [ ] Traverse into the shadow-house route.
- [ ] Confirm current layer and connector mapping update in debug.
- [ ] Restart round.
- [ ] Confirm graph state and layer state reset cleanly.
- [ ] Confirm no blocker bugs or critical console spam occur.

## Deferred from Sprint 1
- Anchor placement and destruction.
- Additional anomaly types.
- Procedural generation.
- Multiplayer sync.
- Stalker AI.
- Final textures, model polish, advanced sound mix.

## Claude task framing notes
For this sprint, assign Claude tasks in narrow slices with explicit contracts because the earlier docs showed that checklist-style, acceptance-driven work reduces ambiguity and keeps hidden-state systems testable [file:1][file:13].

Use prompts like:
- "Implement `ObservationLockSystem` with these exact responsibilities and debug outputs. Do not handle anomaly selection."
- "Implement `LoopAnomalyResolver` using `SpatialGraphRuntime` and `ObservationLockSystem`. Do not add rendering logic."
- "Create `SpatialDebugOverlay` that reports the following fields..."

## Sprint 1 done
Mark complete when:
- [ ] The loop anomaly is playable.
- [ ] Shadow-house traversal is readable.
- [ ] Observation locks work predictably.
- [ ] Debug visibility is sufficient to explain system behavior.
- [ ] The team agrees the mechanic is fun enough to justify Milestone 2.
