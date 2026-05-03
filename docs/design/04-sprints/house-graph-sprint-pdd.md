# House Graph Sprint / PDD

## Sprint title
House Graph Sprint — Authoritative Runtime Topology

## Document objective
Convert the house graph integration model into concrete implementation work: runtime classes, interfaces, ownership boundaries, graph queries, portal and stair handling, debug tooling, and milestone tasks that can turn graph-seed output from the house-builder sprint into a functioning authoritative runtime graph [file:12][file:13][file:14][file:15].

## Why this sprint exists
The previous house-builder sprint establishes canonical layout input, validation, artifact generation, debug visualization, and graph-seed export. The next required step is to turn those graph seeds into a runtime topology layer that other systems can trust. Without that layer, room systems, visibility, mutation, AI, and future multiplayer would each risk inventing their own idea of what the house is [file:12][file:13][file:14].

## Sprint thesis
This sprint should prove that the project can materialize one authoritative `HouseGraphInstance` from graph-seed data, expose stable node/edge/portal identities, answer topology queries, and provide enough debug visibility that later observation-lock and mutation systems can build on it confidently [file:12][file:13].

## Questions this sprint answers
- Can graph-seed output become a real runtime graph object?
- Can nodes, edges, portals, and stairs have stable ids and narrow ownership?
- Can room volumes and runtime scene references bind to graph truth without competing with it?
- Can developers inspect the graph during graybox play and trust what they see [file:12][file:13][file:15]?

## Out of scope
This sprint does **not** include:
- observation-lock logic,
- live spatial mutation rules,
- final render streaming,
- AI pathfinding implementation,
- multiplayer replication,
- or final player-facing navigation aids.

The sprint focuses on authoritative runtime topology, not every consumer that will eventually use it.

## Sprint goals
By the end of this sprint, the project should support:
- creating a `HouseGraphInstance` from graph-seed data,
- representing nodes, edges, portals, and stair links explicitly,
- binding runtime scene references like `RoomVolume` and doors into graph truth,
- querying adjacency and connectivity from one place,
- exposing graph debug views in play mode,
- and proving the graph is stable enough for the next observation/mutation sprint [file:12][file:13][file:14].

## Architectural rules carried into this sprint
This work should preserve the project’s core rules.

- Runtime state must remain separate from imported/baseline definitions [file:12][file:13][file:14].
- Scene objects stay thin and reference graph identity instead of inventing topology [file:12][file:13].
- Hidden systems must expose debug information early [file:12][file:13][file:15].
- Narrow ownership beats giant all-knowing managers [file:13][file:15].

## Relationship to previous sprint
This sprint starts where HB-1 and HB-2 end.

### Inputs from previous sprint
- validated `GeneratedHouseArtifact`
- graph-seed records
- portal records
- stair records
- room and floor indices
- structural debug overlays [file:12][file:13][file:14]

### Outputs of this sprint
- authoritative graph definition/runtime split
- runtime graph builder
- graph query services
- scene binding layer
- graph debug overlay
- and milestone-ready topology APIs for later systems

## Recommended folder targets
This sprint should fit the established project layout rather than create a parallel architecture [file:13][file:14].

### Suggested folders
- `Assets/_Project/Data/Rooms/Graphs/`
- `Assets/_Project/Scripts/World/Graph/`
- `Assets/_Project/Scripts/World/Rooms/`
- `Assets/_Project/Scripts/World/Rooms/Debug/`
- `Assets/_Project/Scripts/Editor/`
- `Assets/_Project/Prefabs/Debug/`

## Target deliverables
The sprint should produce:
- graph definition models,
- graph runtime state models,
- a graph builder service,
- graph query interfaces,
- room-volume binding support,
- portal and stair binding support,
- debug visualization for graph state,
- and tests proving deterministic graph construction [file:12][file:13][file:14].

## Recommended implementation slices
The work should be executed in narrow slices, each with explicit acceptance criteria.

### Recommended order
1. graph contract and ownership types,
2. graph builder,
3. graph query API,
4. scene binding layer,
5. debug overlays,
6. validation and smoke tests,
7. milestone integration scene.

## Core runtime model
This sprint should formalize the following split.

### Definition-side
- `HouseGraphDefinition`
- `NodeDefinition`
- `EdgeDefinition`
- `PortalDefinition` or graph-safe renamed equivalent
- `VerticalLinkDefinition`

### Runtime-side
- `HouseGraphInstance`
- `NodeRuntimeState`
- `EdgeRuntimeState`
- `PortalRuntimeState`
- `VerticalLinkRuntimeState`

### Rule
Definitions describe baseline graph truth derived from artifact data. Runtime state tracks active session conditions such as enabled state, occupancy, temporary locks, and future mutation hooks [file:12][file:13].

## Suggested key classes

| Class | Responsibility |
|---|---|
| `HouseGraphBuilder` | Builds graph definition/runtime objects from graph-seed data |
| `HouseGraphInstance` | Owns authoritative runtime topology |
| `HouseGraphQueryService` | Exposes query methods over the graph |
| `RoomGraphBinder` | Binds `RoomVolume` and room scene objects to node ids |
| `PortalGraphBinder` | Binds door/threshold scene objects to portal ids |
| `StairGraphBinder` | Binds stair scene objects or markers to vertical links |
| `HouseGraphDebugOverlay` | Visualizes nodes, edges, portals, stairs, and selection state |
| `GraphSelectionPanel` | Inspector-style debug details for selected graph elements |

This preserves focused system ownership consistent with the rest of the architecture [file:13][file:14].

## Suggested interfaces
The exact signatures can evolve, but the sprint should lock in narrow graph contracts instead of ad hoc direct access [file:13][file:14].

```csharp
public interface IHouseGraphBuilder
{
    HouseGraphInstance Build(GeneratedHouseArtifact artifact);
}

public interface IHouseGraphQueryService
{
    bool TryGetNode(string nodeId, out NodeRuntimeState node);
    IReadOnlyList<NodeRuntimeState> GetNeighbors(string nodeId);
    bool AreConnected(string fromNodeId, string toNodeId);
}

public interface IGraphBindable
{
    string GraphId { get; }
}
```

## Suggested ids and identity rules
Stable ids are non-negotiable because the graph will eventually support mutation, debugging, and likely multiplayer synchronization [file:12][file:13].

### Rules
- node ids must remain deterministic for the same baseline artifact,
- portal ids must survive binding from artifact to scene,
- edge ids must not depend on transient scene instance order,
- stair/vertical-link ids must preserve cross-floor identity.

## Milestone framing
This sprint is split into two concrete milestones.

### HG-1
Build the runtime graph core from artifact/graph-seed data.

### HG-2
Bind the runtime graph to scene objects and expose it through debug tooling and query surfaces.

## HG-1 overview
HG-1 is about materializing authoritative graph truth.

### HG-1 answers
Can the project produce a trustworthy runtime graph structure from the output of the house-builder pipeline [file:12][file:13]?

## HG-1 tasks

### HG1-T1 — Create graph definition models
Implement serializable baseline graph definition classes.

#### Required types
- `HouseGraphDefinition`
- `NodeDefinition`
- `EdgeDefinition`
- `GraphPortalDefinition` if naming collision avoidance is needed
- `VerticalLinkDefinition`

#### Acceptance criteria
- Types compile.
- Ownership is definition-only, not runtime state.
- Fields align with graph integration rules: ids, adjacency, floor relation, layer membership, and portal linkage [file:12][file:13][file:14].

### HG1-T2 — Create runtime graph state models
Implement runtime graph state types.

#### Required types
- `HouseGraphInstance`
- `NodeRuntimeState`
- `EdgeRuntimeState`
- `PortalRuntimeState`
- `VerticalLinkRuntimeState`

#### Runtime fields may include
- active/inactive state,
- occupancy summary,
- debug flags,
- future lock hooks,
- and presentation hooks.

#### Acceptance criteria
- Runtime types remain separate from baseline definition models.
- No importer/build-layer data is mutated in place.

### HG1-T3 — Build graph-seed translator
Create a service that translates `GeneratedHouseArtifact` graph-seed output into graph definition records.

#### Responsibilities
- convert room candidates into nodes,
- convert portal records into explicit portal definitions,
- convert adjacency into edges,
- convert stair relations into vertical links,
- and attach floor/layer metadata.

#### Acceptance criteria
- The translator is deterministic for the same artifact.
- It does not re-infer topology from scene geometry.
- It fails loudly on contradictory seed data.

### HG1-T4 — Implement `HouseGraphBuilder`
Build the full graph from artifact to runtime instance.

#### Responsibilities
- create definition objects,
- instantiate runtime state,
- index nodes/edges/portals,
- validate internal graph consistency,
- and publish build summary/debug info.

#### Acceptance criteria
- A valid artifact produces a usable `HouseGraphInstance`.
- Invalid seed relationships produce structured failures or issues, not silent corruption.

### HG1-T5 — Implement graph validation pass
Add runtime graph validation separate from import validation.

#### Checks should include
- duplicate node ids,
- edge references unknown node,
- portal references unknown endpoint,
- vertical link mismatch,
- disconnected required nodes,
- asymmetric adjacency when not intended,
- and impossible floor relation.

#### Acceptance criteria
- Graph validation is explicit and testable.
- Build failures identify the exact graph element that is broken.

### HG1-T6 — Create query API
Implement a graph query layer instead of raw list access.

#### Minimum query support
- get node by id,
- get neighbors,
- get portals for node,
- get nodes on floor,
- get connected component,
- are two nodes connected,
- get vertical links for node.

#### Acceptance criteria
- Consumers do not need to traverse raw collections manually for common operations.
- Query outputs are stable and deterministic.

### HG1-T7 — Add edit-mode/runtime tests
Create tests for graph construction and queries.

#### Suggested tests
- valid artifact builds expected node/edge counts,
- bad portal ref fails validation,
- stair mismatch fails,
- connected component count matches fixture,
- neighbor query correctness,
- deterministic rebuild id consistency.

#### Acceptance criteria
- Graph tests run without full gameplay boot.
- Failures point to graph code, not generic scene errors.

## HG-1 completion definition
HG-1 is complete when a `GeneratedHouseArtifact` can become a validated `HouseGraphInstance` with stable node/edge/portal/vertical-link identity and basic query support [file:12][file:13][file:14].

## HG-2 overview
HG-2 is about scene binding and developer visibility.

### HG-2 answers
Can runtime scene objects and developers both interact with the graph as the one trusted topology source [file:12][file:13]?

## HG-2 tasks

### HG2-T1 — Create `RoomGraphBinder`
Bind room scene objects such as `RoomVolume` to graph node ids.

#### Responsibilities
- map `RoomVolume` to `nodeId`,
- validate one-to-one or declared one-to-many expectations,
- expose debug mismatch info.

#### Acceptance criteria
- `RoomVolume` no longer invents adjacency independently.
- Room-to-node mismatches are visible and fail loudly when necessary [file:13][file:14].

### HG2-T2 — Create `PortalGraphBinder`
Bind door/threshold scene objects to graph portal ids.

#### Responsibilities
- map scene doors and thresholds to portal records,
- expose unresolved portal bindings,
- support future state hooks like open/closed or observed/unobserved.

#### Acceptance criteria
- Scene threshold objects resolve to authoritative portal ids.
- Missing or duplicate bindings are visible in debug.

### HG2-T3 — Create `StairGraphBinder`
Bind stair markers or stair scene groups to vertical link records.

#### Acceptance criteria
- Cross-floor stair linkage can be inspected in runtime debug.
- Selecting a stair binding reveals both linked endpoints.

### HG2-T4 — Build graph debug overlay
Implement a debug overlay specialized for graph truth.

#### It should show
- node ids,
- edge lines,
- portal ids,
- vertical links,
- floor segmentation,
- active/inactive flags,
- and selection state.

#### Acceptance criteria
- Developers can understand graph topology during graybox play without reading raw object dumps.
- Overlay categories can be toggled independently.

### HG2-T5 — Build graph selection panel
Add selection inspection for graph elements.

#### Node selection should show
- node id,
- type,
- floor,
- connected edges,
- connected portals,
- runtime flags,
- bound room refs.

#### Portal selection should show
- portal id,
- type,
- endpoints,
- bidirectional state,
- bound scene object refs,
- runtime flags.

#### Acceptance criteria
- Most topology debugging can be done from overlay + selection panel alone.

### HG2-T6 — Add scene smoke test workflow
Create one practical workflow to build and inspect the graph in a scene.

#### Could be
- `House_Graybox` startup build path,
- a dedicated graph test scene,
- or an editor window that spawns/inspects the graph.

#### Acceptance criteria
- A developer can load a known fixture and confirm node, portal, and stair bindings in one pass.

### HG2-T7 — Add graph events or change notifications
Even if mutation is not implemented yet, add lightweight signals for graph lifecycle and future consumers.

#### Suggested events
- `OnGraphBuilt`
- `OnGraphBindingFailed`
- `OnNodeSelectedDebug`
- `OnPortalSelectedDebug`

#### Acceptance criteria
- Future systems can respond to graph lifecycle without hard references everywhere.

## HG-2 completion definition
HG-2 is complete when scene room/portal/stair references bind to the graph successfully, graph topology is inspectable in graybox runtime, and debug overlays make graph truth obvious during testing [file:12][file:13][file:14][file:15].

## Suggested ownership map
To keep the sprint Claude-friendly, each major responsibility should stay narrow.

| Area | Owns | Does not own |
|---|---|---|
| Graph builder | definition/runtime graph creation | scene visuals |
| Query service | graph lookups and traversal helpers | scene binding |
| Room binder | room-to-node binding | graph construction policy |
| Portal binder | threshold/door binding | door gameplay logic |
| Stair binder | vertical link binding | stair art or locomotion |
| Debug overlay | graph visualization | graph authority |

## Suggested Claude Code workflow
This sprint is best implemented through narrow prompts with explicit targets and acceptance checks, which matches the broader AI-assisted workflow philosophy in the project docs [file:12][file:13].

### Good prompt shape
- create or modify 1–3 classes,
- specify exact folder targets,
- list required methods/fields,
- include acceptance checks,
- request smoke-test/debug helpers.

### Bad prompt shape
- “Implement the whole graph system.”
- “Make the graph runtime work.”
- “Figure out how to bind rooms and portals.”

## Suggested Claude task sequence
A practical implementation order is:
1. definition models,
2. runtime models,
3. graph-seed translator,
4. graph builder,
5. graph validation,
6. query service,
7. unit/edit-mode tests,
8. room binder,
9. portal binder,
10. stair binder,
11. debug overlay,
12. selection panel,
13. scene smoke workflow.

## Testing strategy
Use three levels of testing.

### 1. Data/graph tests
Confirm deterministic graph construction and validation behavior.

### 2. Binding tests
Confirm scene references bind correctly to graph ids and emit visible failures when they do not.

### 3. Runtime debug sanity checks
Open a graybox scene and visually confirm node, portal, and stair truth through overlays and selection tools.

## Risks and mitigations

### Risk 1
The graph builder re-infers topology from geometry and drifts from artifact truth.

### Mitigation
Make graph-seed translation the only legal topology source for this sprint.

### Risk 2
Room volumes and scene doors keep their own adjacency ideas.

### Mitigation
Force binders to reference graph ids and surface mismatches aggressively [file:13][file:14].

### Risk 3
Runtime and definition data blur together.

### Mitigation
Keep separate classes and avoid mutating artifact-derived definitions in place [file:12][file:13].

### Risk 4
Debug overlays become unreadable.

### Mitigation
Independent toggles first, composite view later.

## Acceptance checklist
This sprint is successful when:
- graph-seed output can be translated into a deterministic graph definition/runtime pair [file:12][file:13][file:14],
- node, edge, portal, and stair/vertical-link identities are explicit and stable,
- graph validation catches broken structural relationships clearly,
- common graph queries are exposed through a dedicated service,
- room volumes and threshold scene objects bind to graph ids instead of inventing topology [file:13][file:14],
- graph topology is visible and understandable in runtime debug overlays,
- and the resulting graph is solid enough to support the next observation-lock and mutation sprint [file:12][file:13][file:15].

## Recommended next document
After this sprint doc, the strongest follow-up is **Observation Lock / Spatial Mutation Rules Spec**, because once the authoritative runtime graph exists, the next key question becomes how and when that graph is allowed to change under player observation and impossible-space rules [file:12][file:13].
