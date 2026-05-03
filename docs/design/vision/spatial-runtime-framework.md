# Spatial Runtime Framework

## Purpose
This document defines the core technical framework that integrates all spatial-horror systems in the project. Its goal is to give development a single authoritative contracts document for the house graph, portal resolution, node activation, observation gating, mutation systems, and debug tooling so implementation stays modular instead of drifting into scene-specific Unity behaviors [file:12][file:13][file:14].

This framework exists because the earlier project planning repeatedly identified the same architectural truths: runtime state must remain separate from static definitions, scene objects should stay thin, systems should own narrow responsibilities, and hidden-state logic must be inspectable early if tuning is going to stay tractable [file:12][file:13][file:14][file:15].

## Framework goals
- Establish what exists.
- Establish what owns what.
- Establish the contracts between systems.
- Establish how a graph-defined house becomes an active rendered runtime.
- Establish how future mutations plug in without major refactors.

## Design principles

### 1. Runtime vs definition separation
Static content definitions must be distinct from runtime state [file:12][file:13].

Examples for this project:
- `HouseGraphDefinition` != `SpatialGraphRuntime`
- `RoomNodeDefinition` != `RuntimeNodeState`
- `PortalEdgeDefinition` != `RuntimeEdgeState`
- `GraphMutationRuleDefinition` != active mutation event

### 2. Scene objects stay thin
Scene-authored objects define locations, bounds, anchors, and visual references, but reusable logic lives in systems [file:12][file:13].

### 3. New content should mostly mean new data
A new mutation family should ideally mean new definitions plus a modular evaluator/resolver, not rewriting the graph runtime [file:12].

### 4. Debug-first development
Every hidden spatial rule must be visible in play mode, or later tuning becomes guesswork [file:12][file:13][file:14].

### 5. Narrow system ownership
Avoid giant manager classes. Each system should own one domain clearly, consistent with prior project guidance [file:14][file:15].

## Top-level architecture

### Static layer
Design-time assets and scene authoring references.

### Runtime layer
Loaded graph, active routing, occupancy, visibility, mutation eligibility, and match state.

### Presentation layer
Node activation, portal visibility, visual swaps, audio sends, and debug overlays.

### Gameplay layer
Anchors, player tools, item logic, and later entity interaction with the spatial runtime.

## Namespace recommendation
Consistent namespaces reduce collisions and make AI-assisted generation easier to control, echoing the earlier implementation guidance [file:13][file:14].

```csharp
ProjectName.Core
ProjectName.Player
ProjectName.Interaction
ProjectName.World
ProjectName.Spatial.Definitions
ProjectName.Spatial.Authoring
ProjectName.Spatial.Runtime
ProjectName.Spatial.Visibility
ProjectName.Spatial.Mutations
ProjectName.Match
ProjectName.UI.Debug
ProjectName.Audio
ProjectName.Editor
```

## Folder recommendation
Adapt the earlier predictable Unity structure because it gives every system a stable landing place and reduces project sprawl [file:13][file:14].

```text
Assets/
  _Project/
    Data/
      Spatial/
      Mutations/
      Match/
      UI/
    Prefabs/
      Environment/
      Spatial/
      Debug/
      UI/
    Scenes/
      Bootstrap/
      Test/
      House_Graybox/
      House_Prototype/
    Scripts/
      Core/
      Player/
      Interaction/
      World/
      Spatial/
        Definitions/
        Authoring/
        Runtime/
        Visibility/
        Mutations/
      Match/
      UI/
        HUD/
        Debug/
      Audio/
      Editor/
```

## Graph model
The canonical graph is represented as:

\[
G^* = (V^*, E^*)
\]

where \(V^*\) is the set of currently available room-nodes and \(E^*\) is the set of currently active portal-edges. The runtime may be composed from baseline graph \(G_0\), shadow graph \(G_s\), and inserted subgraphs \(G_t\) as future systems come online.

## Core system map

| System | Namespace | Owns | Does not own |
|---|---|---|---|
| `HouseGraphDefinition` | `Spatial.Definitions` | Static node/edge/portal data | Runtime state |
| `RoomNodeAuthoring` | `Spatial.Authoring` | Scene references, node ids, presentation roots | Global graph logic |
| `SpatialGraphRuntime` | `Spatial.Runtime` | Runtime node/edge tables, occupancy, composed graph state | UI, visual effects |
| `PortalResolver` | `Spatial.Runtime` | Active destination lookup for portals | Mutation strategy |
| `NodeStreamingController` | `Spatial.Visibility` | Which node presentations are active | Graph legality |
| `PortalVisibilityController` | `Spatial.Visibility` | Which destination nodes are visible through portals | Mutation choice |
| `ObservationLockSystem` | `Spatial.Mutations` | Mutation locks based on observation/occupancy | Mutation selection |
| `AnomalyDirector` | `Spatial.Mutations` | Picking and applying legal graph transforms | Rendering |
| `AnchorManager` | `Match` or `World` | Anchor spawn/state/destruction flow | Core graph runtime |
| `SpatialDebugOverlay` | `UI.Debug` | Live explanation of spatial state | Gameplay decisions |

## Static definitions
Recommended static assets:
- `HouseGraphDefinition`
- `RoomNodeDefinition`
- `PortalAnchorDefinition`
- `PortalEdgeDefinition`
- `SubgraphDefinition`
- `ShadowGraphDefinition`
- `GraphMutationRuleDefinition`
- `NodePresentationDefinition`
- `MatchRulesDefinition`

## Runtime state types
Recommended runtime types:
- `RuntimeNodeState`
- `RuntimeEdgeState`
- `RuntimeOccupancyState`
- `RuntimeVisibilityState`
- `RuntimeMutationEvent`
- `RuntimeGraphComposition`

## Authoring contracts

### `RoomNodeAuthoring`
Minimal responsibilities:
- stable node id,
- presentation root,
- portal anchor references,
- bounds or occupancy volume,
- optional semantic tags.

### `PortalAnchorAuthoring`
Minimal responsibilities:
- stable anchor id,
- owning node id,
- portal facing direction,
- aperture bounds or reference plane,
- link to authored door/threshold object if needed.

These authoring components should remain descriptive, not procedural, which matches the prior scene-object guidance [file:12][file:13].

## Runtime contracts

### `ISpatialGraphQuery`
```csharp
public interface ISpatialGraphQuery
{
    bool HasNode(string nodeId);
    bool HasEdge(string edgeId);
    RuntimeNodeState GetNode(string nodeId);
    RuntimeEdgeState GetEdge(string edgeId);
    IReadOnlyList<RuntimeEdgeState> GetConnectedEdges(string nodeId);
    string GetDestinationNodeId(string edgeId);
}
```

### `INodeActivationQuery`
```csharp
public interface INodeActivationQuery
{
    bool IsNodeActive(string nodeId);
    IReadOnlyList<string> GetActiveNodeIds();
    IReadOnlyList<string> GetActivationReasons(string nodeId);
}
```

### `IObservationLockQuery`
```csharp
public interface IObservationLockQuery
{
    bool IsNodeLocked(string nodeId);
    bool IsEdgeLocked(string edgeId);
    string GetNodeLockReason(string nodeId);
    string GetEdgeLockReason(string edgeId);
}
```

### `IGraphMutationService`
```csharp
public interface IGraphMutationService
{
    bool CanApplyTransform(string transformId);
    bool TryApplyTransform(string transformId);
    RuntimeMutationEvent GetLastMutationEvent();
}
```

These interfaces intentionally keep the surface area narrow so systems remain composable and Claude tasks can stay scoped, consistent with earlier anti-sprawl guidance [file:14][file:15].

## System flow

### 1. Boot
`Bootstrap` loads `House_Prototype` and initializes match/runtime services, following the earlier recommendation to keep startup flow out of the gameplay scene itself [file:13][file:14].

### 2. Graph load
`HouseGraphDefinition` and scene authoring references are loaded into `SpatialGraphRuntime`.

### 3. Portal resolution
`PortalResolver` provides active destination lookup for each portal-edge.

### 4. Node activation
`NodeStreamingController` computes active nodes from occupancy, adjacency, and portal visibility.

### 5. Portal visibility
`PortalVisibilityController` computes which destination nodes are currently visible through local portals.

### 6. Observation gating
`ObservationLockSystem` determines which nodes/edges are mutation-locked.

### 7. Mutation
`AnomalyDirector` chooses and applies legal graph transforms through the mutation service.

### 8. Gameplay hooks
Anchor, item, and later entity systems consume graph/runtime state but do not own graph legality.

### 9. Debug
`SpatialDebugOverlay` and gizmos surface runtime truth continuously.

## Match ownership
A narrow match owner is still useful here, just as it was in the earlier planning docs, because round flow and reset behavior should not be smeared across spatial systems [file:4][file:13][file:14].

Recommended `MatchManager` responsibilities:
- boot state,
- active exploration state,
- anchor progression state,
- win/loss transitions,
- restart/reset orchestration.

`MatchManager` should not own graph legality, visibility tests, or mutation policy.

## Rendering and streaming rules
The framework should adopt these rules globally:
- baseline exterior shell is the only exterior truth,
- non-baseline nodes are interior-only,
- node presentation activation is local and demand-driven,
- future shadow and Tardis nodes should be activated only when occupied, adjacent, or visible through an active portal,
- and graph/runtime state should be valid even if a presentation is currently inactive.

This separation between simulation truth and presentation state is critical if the project is going to support impossible interiors without showing impossible exteriors.

## Debug specification
The debug layer is not optional. The prior docs were explicit that hidden-state systems become miserable to tune without observability [file:12][file:13][file:15].

Minimum spatial debug surface:
- current player node id,
- current graph domain (`G_0`, `G_s`, subgraph id),
- current active node set,
- active edge mappings,
- current visible portal destinations,
- observation locks,
- mutation candidates,
- last mutation event,
- anchor state later,
- and any debug override toggles.

## Claude usage guidance
This document should travel with every sprint PDD that touches spatial systems because earlier planning showed that AI-assisted work improves when naming, ownership, and boundaries are stable [file:12][file:13].

When assigning tasks to Claude:
- provide this framework doc plus one sprint doc,
- assign one system at a time,
- state non-goals explicitly,
- and require acceptance tests for every task.

Good task shape:
- objective,
- files to create/edit,
- interfaces touched,
- runtime dependencies,
- acceptance tests,
- non-goals.

## Phased implementation order
Following the earlier milestone philosophy, development should prove one major question at a time rather than building broad complexity all at once [file:12][file:15].

Recommended order:
1. Graph authoring and runtime shell.
2. Portal visibility and node activation.
3. Observation lock system.
4. One loop mutation vertical slice.
5. One node substitution vertical slice.
6. One Tardis insertion vertical slice.
7. Shadow-house bridge slice.
8. Anchor loop integration.
9. Entity integration.

## Framework acceptance criteria
This framework is considered established when:
- spatial systems have stable names and responsibilities,
- graph definitions and runtime state are clearly separated [file:12][file:13],
- portal resolution and node activation have explicit ownership,
- mutation gating has a reserved contract even before all mutations exist,
- debug requirements are documented for every hidden subsystem [file:12][file:13][file:14],
- and sprint docs can reference this framework instead of redefining architecture every time.