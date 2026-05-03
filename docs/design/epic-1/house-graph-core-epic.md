# House Graph Core — Epic

## Purpose
This epic defines the canonical spatial runtime for the project: the house as a graph of nodes and portal-edges that can be inspected, transformed, and rendered without collapsing into scene-specific hacks. Its purpose is to make every later spatial horror mechanic—loops, node substitution, Tardis insertions, and shadow-house bridges—operate through one formal structure rather than bespoke one-off code paths [file:12][file:13][file:14].

The epic exists because the architecture must be built for mutation, not just completion, and because hidden-state systems in this kind of game must be debuggable from the beginning if they are going to remain tunable as complexity grows [file:12][file:13].

## Epic question
Can the project represent an authored house as a stable, queryable runtime graph that supports spatial mutation, interior-only extra-normal rendering, and repeatable debugging without major refactors later [file:12][file:13]?

## Design intent
The player experiences a house. The runtime experiences a graph. That separation matters because the player-facing fiction depends on domestic legibility, while the simulation depends on nodes, edges, visibility sets, routing tables, and graph transforms.

The house graph should support two truths at once:
- the baseline exterior remains the only valid outside-facing structure,
- while interior traversal can compose impossible continuity by redirecting portal edges and inserting extra-normal nodes or subgraphs.

## Academic notation
This project should use a stable graph notation in docs and implementation comments.

### Core graph
Let the baseline house graph be:

\[
G_0 = (V_0, E_0)
\]

where:
- \(V_0\) is the set of baseline room-nodes,
- \(E_0\) is the set of baseline portal-edges.

A room-node \(v \in V\) represents a bounded traversable spatial unit, such as a room, corridor segment, stair landing, or other interior traversal cell. A portal-edge \(e \in E\) represents a directed or bidirectional traversable connection between two nodes via a doorway, archway, threshold, hall mouth, or other portal construct.

### Shadow graph
Let the shadow-house graph be:

\[
G_s = (V_s, E_s)
\]

The shadow house mirrors the baseline graph at first, but it is treated as a physically separate house for occupancy and rendering purposes. A player in node \(v_i \in V_0\) and another player in corresponding node \(v'_i \in V_s\) are not co-present and should not see each other unless a later feature explicitly bridges those spaces.

### Tardis subgraph
Let an extra-normal inserted subgraph be:

\[
G_t = (V_t, E_t)
\]

A Tardis insertion is a graph operation that adds an interior-only subgraph behind one or more active portals of a host node in the runtime composition.

### Runtime composed graph
At runtime, the active navigable graph is:

\[
G^* = (V^*, E^*)
\]

where \(G^*\) is composed from baseline, shadow, and inserted subgraphs according to current mutation rules.

## Runtime concepts

### Node
A node is the atomic authored spatial unit.

A node owns:
- a stable node id,
- a node type,
- optional semantic tags,
- one or more portal anchors,
- visibility/culling metadata,
- occupancy state at runtime,
- and prefab or scene-authoring references for visual realization.

A node does not own global routing logic or mutation orchestration, because that belongs in reusable systems rather than scene-authored objects [file:12][file:13].

### Edge
An edge is a portal connection between nodes. It is better to think of it as a portal contract rather than just an adjacency record.

An edge owns or references:
- source node id,
- target node id,
- portal id,
- source aperture transform,
- target spawn or arrival transform,
- traversal direction rules,
- visibility-through-portal policy,
- and mutation eligibility metadata.

### Graph transform
A graph transform is any legal mutation that changes routing or topology while preserving runtime validity.

Core transform families:
- edge remap: reconnect one portal from \(v_j\) to \(v_n\),
- node substitution: replace \(v_i\) with \(v'_i\),
- edge-pruned substitution: replace \(v_i\) and remove one or more incident edges,
- subgraph insertion: attach \(G_t\) behind a portal or node,
- cross-graph bridge: connect \(G_0\) and \(G_s\) through one or more active portal edges.

## What this epic includes
- Canonical graph notation and terminology.
- Authoring model for nodes, portal anchors, and edges.
- Runtime graph composition shell.
- Graph queries and mutation-safe contracts.
- Debug visibility for nodes, edges, active mappings, and mutation legality.
- Render/streaming constraints for interior-only extra-normal spaces.
- Baseline support for shadow-house mirroring as a dual graph.

## What this epic does not include
- Final procedural generation.
- Anchor gameplay loop logic beyond what graph runtime must expose.
- Stalker/entity AI.
- Post-MVP texture drift, floor stacks, or eldritch exterior systems.
- Full portal-screen rendering polish.

## Architecture rules
The epic must preserve the earlier architecture rules that proved useful in the original project planning:
- runtime state must be separate from content definitions [file:12][file:13],
- scene objects should stay thin [file:12][file:13],
- new content should mostly mean new data, not engine-level rewrites [file:12],
- and hidden systems must expose debug state from the start [file:12][file:13][file:14].

Applied to this project, that means:
- `HouseGraphDefinition` is not `SpatialGraphRuntime`,
- authored room prefabs are not mutation policy owners,
- portal rendering hints are data, not hardcoded scene assumptions,
- and every active edge remap must be inspectable in play mode.

## Suggested ownership model

| System | Owns | Does not own |
|---|---|---|
| `HouseGraphDefinition` | Static authored nodes, edges, portal metadata, legal transform references | Runtime mutation state |
| `SpatialGraphRuntime` | Current active nodes, active edge mappings, occupancy, current composed graph | UI and presentation |
| `PortalResolver` | Which destination an active portal currently resolves to | Mutation strategy |
| `GraphMutationPolicy` | Legal transform checks, graph validity rules | Player movement |
| `NodeStreamingController` | Which node visuals are loaded/active | Graph legality rules |
| `SpatialDebugOverlay` | Debug presentation of graph/runtime state | Runtime decision-making |

This follows the earlier narrow-responsibility principle that avoided giant manager classes and kept systems composable [file:14][file:15].

## Data model
Recommended ScriptableObject assets:
- `HouseGraphDefinition`
- `RoomNodeDefinition`
- `PortalAnchorDefinition`
- `PortalEdgeDefinition`
- `SubgraphDefinition`
- `ShadowGraphDefinition`
- `GraphMutationRuleDefinition`
- `NodePresentationDefinition`

Recommended runtime models:
- `RuntimeNodeState`
- `RuntimeEdgeState`
- `RuntimeOccupancyState`
- `RuntimeVisibilityState`
- `RuntimeGraphComposition`

## Scene and render constraints
The baseline exterior of the house must remain the only outside-facing geometry. Extra-normal nodes, shadow-house nodes, and inserted subgraphs should never become visible from the outside shell. That means runtime composition must work with streaming and portal visibility rules instead of simply instantiating all geometry as one always-visible level chunk.

The render/runtime contract should support these rules:
- baseline exterior shell is always canonical,
- non-baseline nodes are only active when occupied, adjacent, or visible through an active portal,
- shadow-house nodes are interior-only,
- inserted subgraphs are interior-only,
- and portal visibility determines whether target-node presentation should be rendered or hidden.

## Debug requirements
Because this project is a hidden-state spatial horror game, graph state must be inspectable early or the system will become impossible to tune later [file:12][file:13][file:14].

The debug layer for this epic must expose:
- current player node,
- current player graph domain (`G_0`, `G_s`, inserted subgraph id),
- active edge mapping for every live portal in the current local area,
- legal mutation candidates,
- blocked mutation reasons,
- active loaded node set,
- and last graph transform event.

## Milestone breakdown

### Milestone HG-1 — Notation and authoring contracts
Goal:
- define graph vocabulary,
- define node/edge ids,
- define authoring contracts,
- and establish the first authored test graph.

Question answered:
- can the house be described as data instead of only scene intuition?

### Milestone HG-2 — Runtime graph shell
Goal:
- load graph data,
- construct runtime node/edge state,
- resolve active portal destinations,
- and reset cleanly on restart.

Question answered:
- can the game own a runtime graph separate from authored geometry [file:12][file:13]?

### Milestone HG-3 — Node activation and portal visibility harness
Goal:
- activate nearby or visible nodes,
- deactivate irrelevant extra-normal nodes,
- preserve exterior truth,
- and support looking through active portals.

Question answered:
- can graph composition and rendering coexist without showing the impossible house from the outside?

### Milestone HG-4 — Mutation-safe graph transforms
Goal:
- support legal edge remaps,
- support node substitution hooks,
- support inserted subgraph attachment points,
- and validate graph consistency after transforms.

Question answered:
- can later anomaly mechanics be added as transforms instead of hacks [file:12]?

### Milestone HG-5 — Shadow-house dual graph support
Goal:
- support a second mirrored graph,
- keep occupancy and rendering distinct,
- and allow bridge edges between baseline and shadow graphs.

Question answered:
- can the runtime support dual-house composition while preserving player separation?

## Epic acceptance criteria
This epic is complete when all of the following are true:
- The project supports an authored baseline graph with stable node and edge ids.
- Runtime graph state is separate from static graph definitions [file:12][file:13].
- Active portal destinations can be queried at runtime.
- The game can stream or activate only the relevant current local node set.
- Extra-normal nodes do not appear from the exterior shell.
- At least one legal graph transform can be applied and inspected.
- Shadow-house dual graph support exists, even if initially simple.
- Debug overlay explains the current graph state well enough to diagnose routing issues [file:12][file:13][file:14].

## Risks
- Treating authored room prefabs as the graph model instead of separating definition from runtime [file:12][file:13].
- Hardcoding portal behavior per scene.
- Letting streaming and graph logic drift into separate incompatible models.
- Building mutations before graph ids and contracts are stable.
- Skipping debug until later.

## Deliverables
This epic should produce:
- one epic doc,
- one architecture/contracts doc later,
- sprint docs for HG-1 onward,
- one authored test graph in `House_Prototype`,
- and one debug harness for graph inspection.
