# House Graph Integration Spec

## Document objective
Define how the generated house shell, room-node templates, room volumes, thresholds, stairs, and populated room instances become a single authoritative house graph that gameplay, visibility, navigation, mutation, and multiplayer systems can all trust. This spec exists to prevent the project from drifting into multiple incompatible truths about what the house is [file:12][file:13][file:14][file:15].

## Why this spec exists
The project now has multiple environment-facing layers: the shell builder, room-node authoring, room identity/legibility guidance, and room population rules. Without a graph integration layer, each system could make its own assumptions about rooms and connections. That would make observation rules, route logic, anomaly mutation, visibility culling, AI pursuit, and co-op synchronization much harder to reason about or debug [file:12][file:13][file:14].

## Core question answered
How do all house-authoring outputs resolve into one structural truth that the runtime can inspect, mutate, render, and synchronize [file:12][file:13]?

## Design thesis
There must be exactly one authoritative topology model for the house at runtime. Visual geometry, room dressing, debug labels, AI movement, observation locks, and anomaly transforms should all reference that model rather than rediscovering space from scene objects ad hoc [file:12][file:13][file:15].

## Scope
This spec covers:
- graph authority,
- node and edge mapping,
- integration of shell outputs,
- integration of room nodes and room volumes,
- threshold/portal mapping,
- stair and vertical linkage,
- graph instance generation,
- graph update boundaries,
- and runtime/debug consumers of the graph.

## Out of scope
This spec does **not** define:
- the full anomaly rule set,
- final rendering implementation,
- detailed multiplayer netcode,
- enemy decision logic,
- or low-level pathfinding algorithms.

It defines the authoritative graph contract those systems rely on.

## Graph authority rule
The house graph is the authoritative runtime topology.

### This means
- room adjacency is graph truth,
- threshold identity is graph truth,
- stair linkage is graph truth,
- layer/world membership is graph truth,
- and any spatial mutation must update or replace graph truth before dependent systems react.

### This does not mean
- every mesh triangle belongs in the graph,
- every small prop becomes graph data,
- or every visual detail gets encoded topologically.

The graph should be structurally meaningful, not bloated.

## Integration layers
The graph should integrate outputs from several earlier systems.

### Source layers
1. House Builder Pipeline provides shell regions, thresholds, stairs, and graph seeds.
2. Room Node Authoring provides semantic room templates, connector contracts, and room metadata.
3. Room Population Rules provide anchors and occupied-but-navigable room expression.
4. Runtime scene components provide live references such as `RoomVolume`, interactable doors, and anchor objects [file:13][file:14].

### Integration result
These layers resolve into a single `HouseGraphInstance` used by runtime systems.

## Core graph entities
The graph should be built from a small set of stable entity types.

### 1. Graph node
A node represents a coherent navigable space unit.

### 2. Graph edge
An edge represents traversable adjacency between nodes.

### 3. Portal/threshold
A portal is the explicit structural connector between nodes, often mapped from a doorway, archway, open passage, or stair connector.

### 4. Vertical link
A vertical link represents same-graph movement across floors or height strata.

### 5. Layer membership
Each node and portal may belong to one or more world layers/overworld states, even if only one is used initially.

## What should count as a node
Version 1 should prefer semantic room-scale nodes, not arbitrary geometric chunks.

### Good node candidates
- bedroom,
- bathroom,
- kitchen,
- stair hall,
- landing,
- living room,
- utility room,
- closet,
- hall segment when the hall is meaningfully segmented.

### Rule
A node should represent a space that players can name, reason about, or traverse as a meaningful unit. This aligns with the project emphasis on room semantics and readability [file:12][file:13].

## Hall segmentation rule
Halls are a special case.

### Guidance
- A short hall with no meaningful branch may be one node.
- A long hall with multiple decision points should be segmented into multiple nodes.
- A hall with a strong bend, landing, or visibility break should usually split.

### Why
This prevents corridor-heavy spaces from becoming topologically vague and improves mutation control later.

## Node identity
Every graph node should have stable identity separate from its current presentation.

### Suggested node fields
- `nodeId`
- `displayName`
- `nodeType`
- `floorIndex`
- `roomDefinitionId` if present
- `roomNodeTemplateId` if present
- `roomVolumeRef`
- `bounds`
- `layerMask`
- `tags`
- `navigationIdentityNotes`
- `debugColor`

This supports the broader runtime-vs-definition separation already established in the project [file:12][file:13][file:14].

## Edge identity
An edge should describe adjacency as a first-class runtime entity.

### Suggested edge fields
- `edgeId`
- `fromNodeId`
- `toNodeId`
- `portalId`
- `traversalType`
- `isBidirectional`
- `floorRelation`
- `layerMask`
- `isCurrentlyActive`
- `debugState`

## Portal identity
Thresholds and portals should remain explicit, not implied by node adjacency alone.

### Suggested portal fields
- `portalId`
- `portalType`
- `worldTransform`
- `orientation`
- `widthClass`
- `sourceNodeId`
- `targetNodeId`
- `doorRef` if any
- `thresholdAnchorRef`
- `visibilityWindowPolicy`
- `layerMask`
- `isLockedByObservation`
- `isActive`

This matches your project’s growing emphasis on thresholds as meaningful world entities rather than just wall holes [file:13].

## Integration from builder seeds
The House Builder Pipeline should emit graph-seed artifacts rather than forcing the graph layer to infer everything from geometry later.

### Builder should provide
- provisional room regions,
- threshold records,
- stair records,
- floor membership,
- and any region/connector ids that can survive into runtime.

### Rule
The graph layer may refine or normalize these seeds, but it should not ignore them and start from scratch.

## Integration from room nodes
Room-node templates should attach semantic meaning to provisional shell regions.

### Room-node integration should contribute
- semantic room category,
- connector contracts,
- transform information,
- anomaly compatibility tags,
- navigation identity notes,
- and expected zone structure.

### Rule
When shell and room-node data disagree, the integration phase should fail or emit a strong validation warning rather than silently blending incompatible truths.

## Integration from room volumes
Runtime `RoomVolume` components and `RoomRegistry` should map cleanly onto graph nodes rather than competing with them [file:13][file:14].

### Rule of ownership
- `RoomVolume` defines physical world bounds and local scene references.
- `HouseGraphInstance` defines authoritative topology.
- `RoomRegistry` is a query service for instantiated room volumes.

### Therefore
`RoomVolume` should reference its `nodeId`, not invent adjacency or room identity independently.

## Integration from population outputs
Population adds room expression and gameplay affordances, but should not rewrite topology.

### Population may contribute
- anchor references,
- landmark references,
- interactable clusters,
- local cover/occlusion metadata,
- and visibility-support hints.

### Population may not do
- create hidden traversable edges without portal registration,
- merge nodes implicitly through decoration,
- or block mandatory graph traversal without explicit graph-state updates.

## Vertical linkage
The graph must support multi-floor structures explicitly.

### Vertical connectors include
- stairs,
- split-level transitions,
- ladders if ever added,
- open-to-below adjacency for visibility only,
- and floor-over-floor relation for rendering/awareness purposes.

### Rule
Traversal linkage and visibility linkage should be separate concepts. Two nodes may be visually related without being directly traversable.

## Graph construction pipeline
The graph integration process should occur in ordered steps.

### Recommended steps
1. Read builder seed artifacts.
2. Resolve provisional regions to candidate nodes.
3. Attach room-node template semantics.
4. Bind runtime `RoomVolume` references.
5. Bind threshold/portal references.
6. Bind stair and vertical-link references.
7. Bind population anchor references.
8. Validate graph completeness and consistency.
9. Produce `HouseGraphInstance`.
10. Publish graph to runtime consumers.

## Validation rules
The graph build should fail when:
- a node is missing a stable id,
- a portal references a nonexistent node,
- a `RoomVolume` maps to multiple conflicting nodes,
- stair endpoints cannot be resolved,
- connector contracts are violated,
- active traversable routes become one-way accidentally,
- or layer membership becomes internally inconsistent.

## Runtime consumers
Many systems should read from the house graph, but not own it.

### Primary consumers
- player navigation/debug systems,
- observation and geometry-lock systems,
- anomaly director,
- visibility/activation streaming,
- AI navigation coordination,
- anchor lookup systems,
- room-based audio logic,
- evidence/event logic,
- and multiplayer state synchronization.

### Rule
Consumers can cache graph queries, but authoritative edits must go through graph-owned update paths.

## Mutation boundary
Spatial anomalies will likely alter adjacency, portal activation, node replacement, or layer membership.

### Rule
Any meaningful spatial mutation must be represented as a graph operation or graph-state transition before the rest of the runtime treats the change as real.

### Examples
- activate dormant edge,
- disable active portal,
- swap node presentation while preserving node id,
- replace a node with a layered variant,
- insert a loop segment,
- redirect a portal target.

This is the core reason the graph must be authoritative.

## Observation lock integration
Your core mechanic depends on observed spaces staying stable.

### Graph implication
Observation should lock graph-relevant entities, not just rendered meshes.

### Examples of lock targets
- node presentation state,
- portal activation state,
- edge redirection eligibility,
- and layer transition eligibility.

### Rule
If at least one player is observing a graph-relevant structure, mutation systems should treat the corresponding node/portal state as non-mutable until the lock clears.

## Visibility integration
Graph topology and render activation are related but not identical.

### Guidance
- The graph says what can connect.
- The visibility system says what should currently render or simulate.
- Portal sightlines and local adjacency should often drive activation.
- Hidden impossible geometry should still obey graph truth even if inactive.

This separation helps prevent render hacks from becoming accidental topology hacks.

## Layer and overworld support
The graph should be ready for multiple coexisting world states even if the first slice uses only one.

### Suggested support
- node-level layer membership,
- portal-level layer eligibility,
- one-way visibility flags,
- per-layer presentation references,
- and cross-layer correspondence ids.

### Rule
A layer shift should not require rebuilding the entire graph if the underlying structural identity persists. Prefer layered state over total reconstruction where possible.

## Multiplayer implications
Even if true multiplayer comes later, the graph design should avoid blocking it [file:13].

### Implications
- node, edge, and portal ids must be stable,
- graph mutations must be representable as replicable events or state diffs,
- observation locks should be attributable to one or more players,
- and clients should not derive conflicting topology from local-only heuristics.

## Data model recommendation
The graph should separate static-ish definition from live runtime state, consistent with the broader architecture [file:12][file:13][file:14].

### Suggested split
- `HouseGraphDefinition`: graph-capable authored/built baseline.
- `HouseGraphInstance`: runtime state for the current session.
- `NodeDefinition` / `NodeRuntimeState`
- `PortalDefinition` / `PortalRuntimeState`
- `EdgeDefinition` / `EdgeRuntimeState`

### Benefit
This keeps session mutations, observation locks, and temporary layer shifts from corrupting the baseline source definition.

## Suggested Unity placement
This spec should fit into the existing project structure cleanly [file:13][file:14].

### Likely folders
- `Assets/_Project/Data/Rooms/Graphs/`
- `Assets/_Project/Scripts/World/Rooms/`
- `Assets/_Project/Scripts/World/Graph/`
- `Assets/_Project/Scripts/Editor/`
- `Assets/_Project/Prefabs/Debug/`

## Debug requirements
The graph must be highly inspectable because hidden-state topology is otherwise impossible to tune [file:12][file:13][file:15].

### Debug should show
- node ids and labels,
- edge lines,
- portal ids and states,
- active vs dormant edges,
- layer membership,
- observation locks,
- vertical links,
- node occupancy by players,
- and mutation history.

### Useful debug modes
- graph-only overlay,
- node-color-by-type,
- portal-state overlay,
- layer view,
- observation-lock view,
- and route trace view.

## Graybox-first recommendation
The graph should be proven in graybox scenes before final art or dense decoration, consistent with the project’s wider development philosophy [file:12][file:15]. If the graph is not understandable in debug blockout form, final art will only hide the problem.

## Acceptance criteria
This spec is successful when:
- the shell builder, room nodes, room volumes, thresholds, stairs, and population outputs resolve into one authoritative graph [file:13][file:14],
- topology can be queried without re-inferring it from meshes or props,
- graph entities have stable ids suitable for debugging and future networking,
- runtime consumers read graph truth rather than inventing private adjacency models,
- observation and mutation systems have explicit graph-level lock/update targets,
- vertical and layer relationships are represented clearly,
- and developers can inspect the active graph easily in graybox playtests [file:12][file:13][file:15].
