# Sprint 1A — House Graph Authoring and Runtime Shell

## Sprint objective
Define and implement the first minimal house graph as data, then load it into a runtime shell that can be queried, debugged, and reset. Sprint 1A is successful when a small house slice exists as stable node/edge data rather than just scene arrangement, and the runtime can report active portal destinations for that slice [file:12][file:13][file:14].

## Sprint question
Can the team formalize the house as a graph and stand up a first runtime model without immediately getting lost in rendering complexity or mutation logic [file:13][file:15]?

## In scope
- Stable graph notation and naming conventions.
- Minimal authored graph slice in `House_Prototype`.
- Node ids, edge ids, and portal anchor ids.
- Static data definitions for nodes and edges.
- Runtime graph shell that loads definitions and exposes queries.
- Debug overlay for graph inspection.
- Reset/restart support.

## Out of scope
- Portal-screen rendering.
- Streaming/culling optimization beyond simple activation placeholders.
- Loop mutations.
- Room substitutions.
- Tardis insertion.
- Shadow-house dual graph runtime.
- Anchor gameplay.
- Entity logic.

## Why this sprint exists
The earlier project docs were strongest when they defined what exists, what owns what, and what done looks like before implementation expanded into multiple systems [file:13]. This sprint applies that same discipline to the spatial runtime by proving that the house is a formal graph first, not just a collection of meshes and doors [file:12][file:13].

## Test slice
Create one minimal authored slice in `House_Prototype`:
- `Entry`
- `Hall_A`
- `Living`
- `Kitchen`
- `Corridor_B` or equivalent connector hall

This should be enough to prove node authoring, edge authoring, and runtime loading without overbuilding the scene.

## Graph notation for the sprint
Use this naming convention in docs, debug, and code comments:
- nodes: `v_entry`, `v_hall_a`, `v_living`, `v_kitchen`, `v_corridor_b`
- edges: `e_entry_hall`, `e_hall_living`, `e_hall_kitchen`, `e_living_corridor`
- portal anchors: `p_entry_hall_a`, `p_hall_entry_a`, etc.

Use `v_*` for nodes, `e_*` for edges, and `p_*` for portal anchors so Claude tasks and debug screens can speak a shared language.

## Suggested file set
Following the earlier predictable folder strategy reduces project sprawl and makes AI-assisted generation easier to constrain [file:13][file:14].

### Data / authoring
- `Scripts/Spatial/Definitions/HouseGraphDefinition.cs`
- `Scripts/Spatial/Definitions/RoomNodeDefinition.cs`
- `Scripts/Spatial/Definitions/PortalEdgeDefinition.cs`
- `Scripts/Spatial/Definitions/PortalAnchorDefinition.cs`
- `Scripts/Spatial/Authoring/RoomNodeAuthoring.cs`
- `Scripts/Spatial/Authoring/PortalAnchorAuthoring.cs`

### Runtime
- `Scripts/Spatial/Runtime/SpatialGraphRuntime.cs`
- `Scripts/Spatial/Runtime/RuntimeNodeState.cs`
- `Scripts/Spatial/Runtime/RuntimeEdgeState.cs`
- `Scripts/Spatial/Runtime/PortalResolver.cs`

### Debug
- `Scripts/UI/Debug/SpatialDebugOverlay.cs`
- `Scripts/UI/Debug/SpatialDebugGizmos.cs`

### Match/reset
- `Scripts/Match/Flow/MatchState.cs`
- `Scripts/Match/Flow/MatchManager.cs`

## Ownership boundaries

### `HouseGraphDefinition`
Owns static graph data:
- list of node definitions,
- list of edge definitions,
- portal anchor references,
- authoring-time semantic tags.

Does not own runtime occupancy, mutation state, or render activation.

### `RoomNodeAuthoring`
Owns scene references from authored geometry to a stable node id and portal anchor set.

Does not decide graph legality.

### `SpatialGraphRuntime`
Owns runtime-instantiated dictionaries/maps for:
- nodes by id,
- edges by id,
- active adjacency queries,
- and local portal resolution.

Does not own mutation strategy or UI.

### `PortalResolver`
Owns destination lookup for a given active portal edge.

Does not own transform legality or mutation policy.

### `SpatialDebugOverlay`
Owns live display of:
- current node,
- known nodes,
- known edges,
- active destinations,
- and restart state.

Does not own runtime graph decisions.

## Tasks

### 1. Define notation and ids
- [ ] Write a short naming convention section into the project docs.
- [ ] Define stable ids for the first 5-node slice.
- [ ] Define stable ids for each portal anchor and edge.

#### Acceptance tests
- [ ] Every authored node has a unique id.
- [ ] Every authored edge has a unique id.
- [ ] Portal anchors can be referred to unambiguously in debug output.

### 2. Build authoring components
- [ ] Create `RoomNodeAuthoring.cs`.
- [ ] Create `PortalAnchorAuthoring.cs`.
- [ ] Tag the test slice with node and portal anchor components.
- [ ] Add validation warnings for duplicate ids or missing portal anchors.

#### Acceptance tests
- [ ] Duplicate ids are caught in editor or play-mode validation.
- [ ] A node without required anchors reports a clear error.
- [ ] The scene can be scanned into graph data without null-reference failure.

### 3. Build graph definitions
- [ ] Create `HouseGraphDefinition.cs`.
- [ ] Create `RoomNodeDefinition.cs`.
- [ ] Create `PortalEdgeDefinition.cs`.
- [ ] Create `PortalAnchorDefinition.cs`.
- [ ] Populate the first graph asset for the test slice.

#### Acceptance tests
- [ ] The graph asset stores all 5 nodes.
- [ ] The graph asset stores all intended edges.
- [ ] Node-to-edge references are internally consistent.

### 4. Build runtime graph shell
- [ ] Create `RuntimeNodeState.cs`.
- [ ] Create `RuntimeEdgeState.cs`.
- [ ] Create `SpatialGraphRuntime.cs`.
- [ ] Create `PortalResolver.cs`.
- [ ] Load graph asset at round start through a simple match/bootstrap path.

#### Acceptance tests
- [ ] Runtime dictionaries initialize successfully.
- [ ] A node can be queried by id.
- [ ] An edge can be queried by id.
- [ ] The destination node for a portal edge can be resolved in play mode.
- [ ] Restart resets runtime state cleanly.

### 5. Add debug visibility
- [ ] Create `SpatialDebugOverlay.cs`.
- [ ] Create `SpatialDebugGizmos.cs`.
- [ ] Show node count, edge count, current node id, and local active destinations.
- [ ] Add a toggle key for debug.
- [ ] Add a restart shortcut for quick iteration.

#### Acceptance tests
- [ ] Debug can be toggled in play mode.
- [ ] The overlay lists current node and connected destinations.
- [ ] Gizmos label nodes and portal anchors in-scene.
- [ ] A developer can verify the authored graph from the overlay alone.

## Suggested runtime queries
These are the only queries Sprint 1A really needs:
- `GetNode(nodeId)`
- `GetEdge(edgeId)`
- `GetConnectedEdges(nodeId)`
- `GetDestinationNode(edgeId)`
- `GetPortalAnchor(anchorId)`

Keeping the query surface narrow matches the earlier narrow-systems philosophy and reduces the chance of overdesign [file:14][file:15].

## Scene setup guidance
Use the earlier scene discipline that kept `Bootstrap`, `Test`, and `House_Prototype` separate, because it avoids pushing every experiment directly into the main play scene [file:13][file:14].

For this sprint:
- `Bootstrap` should load `House_Prototype`.
- `House_Prototype` contains the first 5-node authored slice.
- `Test` can contain isolated graph validation objects if needed.

## Smoke test
Run this before marking Sprint 1A complete:
- [ ] Launch from `Bootstrap`.
- [ ] Enter `House_Prototype`.
- [ ] Confirm graph asset loads.
- [ ] Confirm runtime graph reports 5 authored nodes.
- [ ] Confirm current node can be identified from player position.
- [ ] Confirm connected edges for current node appear in debug.
- [ ] Confirm at least one portal destination resolves correctly.
- [ ] Trigger restart.
- [ ] Confirm graph runtime resets cleanly.
- [ ] Confirm no blocker console errors occur.

## Deferred from Sprint 1A
- Portal rendering.
- Streaming/culling.
- Visibility lock system.
- All mutation logic.
- Shadow-house support.
- Tardis graphs.
- Anchor loop.

## Sprint done
Mark complete when:
- [ ] the first house slice exists as formal graph data,
- [ ] runtime graph queries work in play mode,
- [ ] debug output explains current graph structure,
- [ ] and the team is ready to move to portal visibility and node activation in the next sprint.