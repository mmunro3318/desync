# Sprint 4B — Tardis Anomaly Vertical Slice

## Sprint objective
Implement the first constrained Tardis anomaly slice: a small baseline threshold resolves into an interior-only subgraph that contains more navigable sequence depth than the baseline exterior shell implies. Sprint 4B is successful when one authored doorway, hatch, or closet threshold can legally unfold into an impossible interior branch, be traversed coherently, and reset cleanly without exposing impossible exterior geometry [file:12][file:13][file:14][file:15].

## Sprint question
Can the project support one believable “bigger on the inside” anomaly using the existing graph, visibility, and observation framework, while keeping the exterior shell canonical and the implementation narrow enough to remain debuggable [file:12][file:13][file:15]?

## Why this sprint exists
The anomaly taxonomy identifies Tardis anomalies as one of the project’s strongest spatial-horror payoffs, but also as a higher-complexity family than loop or substitution anomalies. This sprint exists to answer that complexity question with a tightly constrained slice rather than by attempting a full impossible-interior system all at once, which keeps faith with the earlier project principle of proving one major question at a time through narrow, testable slices [file:12][file:15].

## In scope
- One Tardis anomaly family only.
- One authored threshold entry point.
- One small inserted interior subgraph, such as 2–4 nodes.
- Legal activation only when observation rules allow it.
- Interior-only node activation and streaming.
- Debug visibility for subgraph insertion, active entry threshold, and reset state.
- Playable traversal in `House_Prototype`.

## Out of scope
- Multiple Tardis entry points.
- Dynamic procedural Tardis generation.
- Full shadow-house domain support.
- Multiplayer domain separation.
- Artifact/anchor integration.
- Creature pressure tied to Tardis depth.
- Final audiovisual polish.

## Design intent
The goal is not to overwhelm the player with scale. The goal is to make one small ordinary threshold feel newly dangerous because it no longer obeys the size promised by the house exterior. The first Tardis slice should create a strong “that should not fit in here” feeling while remaining locally legible and mechanically trustworthy.

## Tardis anomaly definition
A Tardis anomaly is a graph composition event in which a baseline threshold on the house graph resolves into an inserted interior-only subgraph that contains more navigable depth than the baseline exterior shell can justify.

At the runtime level, this is primarily:
- subgraph insertion,
- threshold remapping into the inserted subgraph,
- interior-only node activation,
- and clean return-path handling back to the baseline graph.

## Recommended first slice
Use one ordinary threshold with low semantic burden, such as:
- linen closet,
- pantry,
- side storage room,
- attic hatch,
- or basement door.

Recommended inserted branch:
- EntryThresholdNode -> ServiceHall_A -> ServiceHall_B -> ReturnNode

Keep the inserted branch small. The purpose is to prove the runtime shape, not maximize content volume.

## Core constraints

### 1. Exterior shell remains canonical
The outside of the house must never visibly imply the inserted volume. The anomaly is permitted only because the inserted branch exists as an interior-only graph composition.

### 2. Tardis content is local, not global
The inserted subgraph should only be active when occupied, adjacent, or visible through the entry threshold or interior portals, consistent with earlier visibility and streaming rules [file:12][file:13].

### 3. Entry must be legal before activation
The threshold should only resolve into the inserted branch when the affected nodes and edges are mutation-eligible under the observation lock system.

### 4. Return path must be intentional
The player must be able to exit the inserted branch cleanly. This can be through the same threshold or a controlled return node mapped back to the baseline graph.

## System ownership

### `AnomalyDirector`
Owns:
- requesting the Tardis pattern,
- checking legality,
- applying the insert/remove event,
- recording active anomaly state.

Does not own:
- node streaming,
- portal visibility,
- or player movement.

### `GraphMutationService`
Owns:
- attaching the interior subgraph,
- remapping the entry threshold to the inserted branch,
- removing the subgraph on reset,
- restoring baseline routing.

### `SpatialGraphRuntime`
Owns:
- runtime graph composition state,
- inserted node and edge registration,
- and active destination resolution support.

### `NodeStreamingController`
Owns:
- activating inserted interior nodes only when locally relevant.

### `PortalVisibilityController`
Owns:
- visibility into the inserted branch through legal active thresholds.

### `ObservationLockSystem`
Owns:
- determining whether entry threshold and affected graph elements are eligible.

### `SpatialDebugOverlay`
Owns:
- exposing active subgraph id, entry threshold, inserted nodes, and baseline/active state.

This ownership model preserves the earlier rule that systems should have narrow domains and scene logic should not turn into one-off behavior piles [file:13][file:14][file:15].

## Data and authoring
Recommended assets:
- `TardisSubgraphDefinition`
- `TardisPatternDefinition`
- `DomainBridgeDefinition` if needed for return mapping

Each Tardis pattern should define:
- pattern id,
- entry threshold edge id,
- inserted subgraph id,
- inserted node ids,
- inserted edge ids,
- return edge mapping,
- activation preconditions,
- reset behavior,
- optional presentation tags.

This keeps the implementation aligned with the project principle that new content should mostly be data-backed rather than hardcoded [file:12][file:13].

## Suggested files

### Runtime / mutation
- `Scripts/World/Graph/Mutations/TardisSubgraphDefinition.cs`
- `Scripts/World/Graph/Mutations/TardisPatternDefinition.cs`
- `Scripts/World/Graph/Mutations/GraphMutationService.cs`
- `Scripts/World/Graph/Mutations/RuntimeMutationEvent.cs`
- `Scripts/World/Graph/Mutations/AnomalyDirector.cs`

### Authoring
- `Scripts/World/Graph/Authoring/TardisEntryAuthoring.cs`
- `Scripts/World/Graph/Authoring/TardisReturnAuthoring.cs`

### Debug
- `Scripts/UI/Debug/TardisDebugOverlay.cs`
- `Scripts/UI/Debug/TardisDebugGizmos.cs`

### Existing dependencies touched
- `SpatialGraphRuntime.cs`
- `PortalResolver.cs`
- `NodeStreamingController.cs`
- `PortalVisibilityController.cs`
- `ObservationLockSystem.cs`
- `MatchManager.cs`

## Contracts

### `ITardisQuery`
```csharp
public interface ITardisQuery
{
    bool IsTardisPatternActive(string patternId);
    string GetActiveTardisPatternId();
    string GetInsertedSubgraphId();
    IReadOnlyList<string> GetInsertedNodeIds();
    IReadOnlyList<string> GetInsertedEdgeIds();
}
```

### `IGraphCompositionService`
```csharp
public interface IGraphCompositionService
{
    bool CanInsertSubgraph(string patternId);
    bool TryInsertSubgraph(string patternId);
    bool RemoveInsertedSubgraph(string patternId);
    bool HasInsertedSubgraph(string subgraphId);
}
```

### `IThresholdRouteQuery`
```csharp
public interface IThresholdRouteQuery
{
    string GetBaselineDestinationNodeId(string edgeId);
    string GetResolvedDestinationNodeId(string edgeId);
    bool IsThresholdRemapped(string edgeId);
}
```

As with earlier sprint docs, these interfaces stay intentionally narrow so future systems can compose around them without creating god objects [file:14][file:15].

## Core rules

### Rule 1 — Tardis is graph composition, not teleport spectacle
The player should traverse through a legitimate threshold whose routing now resolves into an inserted branch. Avoid arbitrary repositioning tricks as the core mechanic.

### Rule 2 — Inserted subgraphs are interior-only
Do not allow inserted Tardis nodes to be treated as exterior truth.

### Rule 3 — The inserted branch must have a stable return path
A player entering the Tardis branch must be able to return to the baseline graph in a predictable way.

### Rule 4 — Only one active Tardis pattern in Sprint 4B
Keep the system simple until the first slice is proven.

### Rule 5 — Debug must reveal composition truth
A developer must be able to tell exactly which subgraph is active, how the entry threshold is mapped, and what the return route is.

## Tasks

### 1. Author the first Tardis pattern asset
- [ ] Create `TardisSubgraphDefinition.cs`.
- [ ] Create `TardisPatternDefinition.cs`.
- [ ] Author one inserted branch with 2–4 nodes.
- [ ] Bind one baseline threshold edge as the Tardis entry.

#### Acceptance tests
- [ ] Pattern asset exists and is editable.
- [ ] Inserted branch clearly lists node and edge ids.
- [ ] Entry threshold id and return mapping are defined.

### 2. Extend graph composition service for inserted subgraphs
- [ ] Add subgraph insertion support to `GraphMutationService.cs` or dedicated composition service.
- [ ] Register inserted nodes and edges in `SpatialGraphRuntime` when active.
- [ ] Remove them cleanly on reset.

#### Acceptance tests
- [ ] Inserted nodes/edges appear in runtime when pattern is active.
- [ ] Inserted nodes/edges disappear on reset.
- [ ] Baseline graph remains valid after removal.

### 3. Remap threshold entry and return path
- [ ] Route the entry threshold into the inserted branch.
- [ ] Define and test the return mapping back to the baseline graph.
- [ ] Ensure `PortalResolver` uses active composition state.

#### Acceptance tests
- [ ] Entering the threshold leads into the inserted branch.
- [ ] Exiting through the return route leads back to the baseline graph.
- [ ] Resolved destination differs from baseline only when pattern is active.

### 4. Integrate legality checks with observation system
- [ ] Query `ObservationLockSystem` before insertion.
- [ ] Block activation if entry threshold or affected graph elements are locked.
- [ ] Record blocking reasons for debug.

#### Acceptance tests
- [ ] Pattern does not activate while blocked by observation rules.
- [ ] Pattern activates when all required elements are eligible.
- [ ] Failed attempts report blocking reasons clearly.

### 5. Integrate node streaming and portal visibility
- [ ] Ensure inserted nodes activate only when occupied, adjacent, or portal-visible.
- [ ] Confirm baseline non-local Tardis content stays inactive when not relevant.
- [ ] Confirm looking back through the entry threshold behaves coherently.

#### Acceptance tests
- [ ] Inserted branch nodes stream in locally.
- [ ] Distant inserted nodes do not remain unnecessarily active.
- [ ] Portal visibility into the branch updates correctly.

### 6. Add Tardis debug tools
- [ ] Create `TardisDebugOverlay.cs`.
- [ ] Create `TardisDebugGizmos.cs`.
- [ ] Show active pattern id, inserted subgraph id, entry threshold, inserted nodes, and return mapping.
- [ ] Show baseline route vs active route for the threshold.

#### Acceptance tests
- [ ] Overlay clearly identifies active Tardis state.
- [ ] Developer can explain current graph composition from debug alone.
- [ ] Failed insertion attempts surface blocking reasons.

### 7. Add reset and replay stability
- [ ] Ensure restart removes active Tardis subgraph cleanly.
- [ ] Ensure exiting the Tardis branch before reset is safe.
- [ ] Run repeated entry/exit/reset tests.

#### Acceptance tests
- [ ] Restart fully restores baseline graph state.
- [ ] Repeated Tardis traversal produces no blocker errors.
- [ ] No stale inserted nodes remain registered after reset.

## Debug expectations
Minimum Tardis debug surface:
- active Tardis pattern id,
- inserted subgraph id,
- inserted node ids,
- inserted edge ids,
- entry threshold id,
- return route mapping,
- baseline destination vs active destination,
- and blocking reasons when insertion is illegal.

This remains consistent with the earlier project-wide requirement that hidden systems must expose state early or tuning becomes miserable [file:12][file:13][file:15].

## Suggested implementation order
Following the earlier narrow-slice roadmap logic [file:12][file:15]:
1. Pattern asset and inserted branch authoring.
2. Graph composition support.
3. Entry threshold remap.
4. Return path mapping.
5. Observation legality integration.
6. Streaming/visibility integration.
7. Debug overlay and reset stabilization.

## Smoke test
Run this before marking Sprint 4B complete:
- [ ] Launch from `Bootstrap`.
- [ ] Enter `House_Prototype`.
- [ ] Confirm baseline threshold route before Tardis activation.
- [ ] Move so the entry threshold becomes eligible under observation rules.
- [ ] Trigger the Tardis pattern.
- [ ] Confirm debug overlay shows active pattern and inserted subgraph.
- [ ] Traverse the threshold and confirm entry into the inserted branch.
- [ ] Move through the inserted branch and confirm local streaming works.
- [ ] Exit through the return route and confirm re-entry to the baseline graph.
- [ ] Reset the anomaly.
- [ ] Confirm the threshold now resolves to baseline behavior.
- [ ] Restart the round and confirm no stale inserted state persists.
- [ ] Confirm no critical console errors occur.

## Deferred from Sprint 4B
- Procedural Tardis branch generation.
- Multiple concurrent Tardis branches.
- Deeper interior expedition structures.
- Cross-player domain divergence.
- Strong audiovisual reveal sequences.
- Tardis branches as anchor-specific spaces.

## Sprint done
Mark complete when:
- [ ] one threshold can legally unfold into an inserted interior-only branch,
- [ ] the player can traverse that branch and return cleanly,
- [ ] the exterior shell remains conceptually canonical,
- [ ] and the active graph composition is easy to inspect, replay, and reset during development.
