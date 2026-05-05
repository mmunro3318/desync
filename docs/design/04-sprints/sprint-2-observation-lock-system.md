# Sprint 2 — Observation Lock System

## Sprint objective
Implement the first mutation-gating system that decides when room-nodes and portal-edges are considered observed, temporarily protected, or eligible for spatial change. Sprint 2 is successful when the project can explain, in real time, why a node or edge is mutable or not mutable based on player occupancy, portal visibility, and recent observation history [file:12][file:13][file:14][file:15].

## Sprint question
Can the game formalize the rule that impossible space may only change when it is meaningfully unobserved, while keeping that rule simple enough to debug and reliable enough to build future mutations on top of it [file:12][file:13]?

## Why this sprint exists
The spatial concept depends on a clear covenant with the player: space is unstable, but not arbitrary. Earlier docs already established that hidden systems must surface debug state early, runtime behavior must stay separate from design definitions, and development should prove one major question at a time instead of overbuilding broad complexity first [file:12][file:13][file:14][file:15]. This sprint exists to make the house’s central horror rule real before loop mutations, room substitutions, or Tardis expansions are added.

## In scope
- Occupancy-based locking.
- Visibility-based locking.
- Recent-observation grace timers.
- Node and edge mutation eligibility queries.
- Debug overlay and gizmos for lock state.
- Integration points for future `AnomalyDirector` use.

## Out of scope
- Applying actual graph mutations.
- Loop remaps.
- Shadow-house bridging.
- Tardis insertion.
- Artifact/anchor gameplay.
- Entity AI.
- Multiplayer synchronization beyond code shape that does not block it later.

## Design intent
The player fantasy is not “the house cheats constantly.” The fantasy is “the house changes when certainty collapses.” For Sprint 2, observation should be modeled as a pragmatic gameplay contract rather than a physically perfect visibility simulation. The system should privilege trust, predictability, and debuggability over cleverness.

## Design rules
This sprint inherits the standing architecture rules already captured in the earlier planning set:
- runtime state is separate from content definitions [file:12][file:13],
- scene objects stay thin and mostly descriptive [file:12][file:13],
- systems should keep narrow roles rather than collapse into manager sprawl [file:14][file:15],
- and hidden systems must expose clear debug data from the beginning [file:12][file:13][file:14].

## Observation model
Observation is represented through a layered lock model rather than one binary flag.

### 1. Occupancy lock
A node is occupancy-locked when one or more players are inside it. Any edge directly participating in the player’s immediate traversal context should also be treated as locked while occupancy persists.

### 2. Direct visibility lock
A node or edge is visibility-locked when it is currently visible through local camera context and portal evaluation. This uses the portal visibility harness from Sprint 1B rather than a whole-world omniscient line-of-sight simulation.

### 3. Grace lock
A node or edge remains locked for a short configurable period after it stops being occupied or visible. This avoids uncanny flicker and prevents the house from visibly changing on the exact frame a player looks away.

## First-pass truth model
A node or edge is mutation-eligible only if all of the following are true:
- it is not occupancy-locked,
- it is not directly visibility-locked,
- its grace timer has expired,
- and no higher-priority rule marks it protected.

This should be the only eligibility rule in Sprint 2. Complexity belongs later.

## Lock ownership

### `ObservationLockSystem`
Owns:
- collecting lock inputs,
- computing lock state per node and edge,
- maintaining grace timers,
- exposing eligibility queries,
- and surfacing lock reasons for debug.

Does not own:
- choosing mutations,
- applying graph transforms,
- or presentation activation.

### `NodeStreamingController`
Owns active presentation state only.
It may inform observation indirectly, but it does not decide lock legality.

### `PortalVisibilityController`
Owns visibility probing inputs for observation.
It reports candidate visible destinations/edges to the lock system.

### `AnomalyDirector`
Future consumer only in this sprint.
It should ask the lock system whether a candidate node or edge is eligible, but not decide the meaning of observation itself.

## Data and tunables
Recommended ScriptableObject or config holder:
- `ObservationRulesDefinition`

Recommended tunables:
- `nodeGraceSeconds`
- `edgeGraceSeconds`
- `portalVisibilityDotThreshold`
- `visibilityRefreshInterval`
- `lockDebugVerbose`

These should be simple config values, consistent with the earlier philosophy that tuning should live in data or config rather than scattered engine code [file:12][file:13].

## State types
Recommended runtime types:
- `NodeObservationState`
- `EdgeObservationState`
- `ObservationSnapshot`
- `LockReason`

### Example `LockReason`
```csharp
public enum LockReason
{
    None,
    Occupied,
    AdjacentOccupiedEdge,
    PortalVisible,
    GracePeriod,
    DebugForced,
    ProtectedByRule
}
```

## Suggested files

### Runtime
- `Scripts/World/Graph/Runtime/ObservationLockSystem.cs`
- `Scripts/World/Graph/Runtime/NodeObservationState.cs`
- `Scripts/World/Graph/Runtime/EdgeObservationState.cs`
- `Scripts/World/Graph/Definitions/ObservationRulesDefinition.cs`

### Debug
- `Scripts/UI/Debug/ObservationDebugOverlay.cs`
- `Scripts/UI/Debug/ObservationDebugGizmos.cs`

### Existing dependencies touched
- `SpatialGraphRuntime.cs`
- `NodeStreamingController.cs`
- `PortalVisibilityController.cs`
- `MatchManager.cs`

## Contracts

### `IObservationLockQuery`
```csharp
public interface IObservationLockQuery
{
    bool IsNodeLocked(string nodeId);
    bool IsEdgeLocked(string edgeId);
    bool IsNodeMutationEligible(string nodeId);
    bool IsEdgeMutationEligible(string edgeId);
    IReadOnlyList<LockReason> GetNodeLockReasons(string nodeId);
    IReadOnlyList<LockReason> GetEdgeLockReasons(string edgeId);
    float GetNodeGraceRemaining(string nodeId);
    float GetEdgeGraceRemaining(string edgeId);
}
```

### `IObservationInputSource`
```csharp
public interface IObservationInputSource
{
    IReadOnlyList<string> GetOccupiedNodeIds();
    IReadOnlyList<string> GetVisibleNodeIds();
    IReadOnlyList<string> GetVisibleEdgeIds();
}
```

These interfaces keep the system queryable and swappable, which matches the earlier emphasis on narrow reusable systems and clean contracts [file:13][file:14][file:15].

## Core rules

### Rule 1 — Occupied nodes cannot mutate
If a player is inside a node, that node is locked.

### Rule 2 — Traversal edges near occupied players cannot mutate
If an edge belongs to the immediate traversal context of an occupied node, it is locked. In Sprint 2, “immediate traversal context” can be approximated as edges connected to an occupied node.

### Rule 3 — Portal-visible nodes and edges cannot mutate
If a destination node or portal-edge is currently visible through the local visibility harness, it is locked.

### Rule 4 — Recently unobserved spaces remain protected briefly
After occupancy or visibility ends, start a grace timer. During this period the node or edge is still locked.

### Rule 5 — Debug override may force lock or unlock for testing
This must exist for development convenience, but should be clearly labeled in the overlay.

## Tasks

### 1. Create observation state types
- [ ] Create `LockReason.cs` or equivalent enum.
- [ ] Create `NodeObservationState.cs`.
- [ ] Create `EdgeObservationState.cs`.
- [ ] Ensure state stores current reasons and grace timer data.

#### Acceptance tests
- [ ] Every authored runtime node can have a node observation state.
- [ ] Every active runtime edge can have an edge observation state.
- [ ] State can represent multiple simultaneous lock reasons.

### 2. Create observation rules config
- [ ] Create `ObservationRulesDefinition.cs`.
- [ ] Add grace durations and refresh settings.
- [ ] Expose values in inspector.

#### Acceptance tests
- [ ] Rule asset exists and is editable.
- [ ] Grace timings can be changed without code edits.
- [ ] Runtime reads values from the config correctly.

### 3. Build `ObservationLockSystem`
- [ ] Aggregate occupied node ids.
- [ ] Aggregate visible nodes and visible edges from `PortalVisibilityController`.
- [ ] Apply occupancy lock.
- [ ] Apply visibility lock.
- [ ] Start and update grace timers when a lock source disappears.
- [ ] Expose final eligibility queries.

#### Acceptance tests
- [ ] Occupied node reports locked.
- [ ] Adjacent traversal edge from occupied node reports locked.
- [ ] Portal-visible node reports locked.
- [ ] A node becomes eligible after all locks end and grace expires.
- [ ] No stale lock remains after reset.

### 4. Integrate debug overlay and gizmos
- [ ] Create `ObservationDebugOverlay.cs`.
- [ ] Create `ObservationDebugGizmos.cs`.
- [ ] Show lock status, reasons, and grace remaining for current node/edge focus.
- [ ] Provide counts for locked nodes, eligible nodes, locked edges, eligible edges.

#### Acceptance tests
- [ ] Overlay can be toggled during play.
- [ ] A developer can explain why a node is not mutable from overlay data alone.
- [ ] Grace countdown is visible and updates in real time.

### 5. Add mutation-service handoff point
- [ ] Add a narrow query path for future `AnomalyDirector` consumption.
- [ ] Stub one debug button or test command that asks for eligible nodes/edges without actually mutating.

#### Acceptance tests
- [ ] A future mutation caller can ask whether a node or edge is eligible.
- [ ] Query returns stable results during repeated playtest loops.
- [ ] No mutation logic is required for the sprint to pass.

### 6. Add reset and restart handling
- [ ] Reset all observation state on round restart.
- [ ] Clear stale grace timers.
- [ ] Confirm no invalid references remain after restart.

#### Acceptance tests
- [ ] Restart clears all lock state correctly.
- [ ] First frame after restart enters valid state.
- [ ] Repeated restart testing produces no blocker errors.

## Debug expectations
The debug layer should at minimum expose:
- current occupied node ids,
- current visible node ids,
- current visible edge ids,
- node lock state,
- edge lock state,
- lock reasons,
- grace remaining,
- and count of currently eligible mutation targets.

This follows the same debug-first standard earlier docs established for hidden systems, because tuning becomes miserable when system truth is invisible [file:12][file:13][file:15].

## Suggested implementation order
Following the earlier milestone philosophy of proving one question at a time [file:12][file:15]:
1. State types.
2. Rules definition asset.
3. Occupancy locking only.
4. Visibility locking from portal harness.
5. Grace timers.
6. Debug overlay.
7. Eligibility query handoff.
8. Reset stabilization.

## Smoke test
Run this before marking Sprint 2 complete:
- [ ] Launch from `Bootstrap`.
- [ ] Enter `House_Prototype`.
- [ ] Confirm current node is occupancy-locked.
- [ ] Confirm connected traversal edges are locked.
- [ ] Look through at least one portal and confirm destination node becomes visibility-locked.
- [ ] Step away and confirm grace period begins instead of instant unlock.
- [ ] Wait for grace expiry and confirm node/edge becomes eligible if no other locks remain.
- [ ] Trigger debug override and confirm lock reason is shown.
- [ ] Restart round and confirm lock state resets cleanly.
- [ ] Confirm no critical console errors occur.

## Deferred from Sprint 2
- Precise occlusion models beyond local portal visibility.
- Cross-player multiplayer observation reconciliation.
- Rule exceptions for special anchor-protected rooms.
- Audio/FX responses to observation loss.
- Mutation weights and pacing logic.

## Sprint done
Mark complete when:
- [ ] node and edge lock states are computed live,
- [ ] grace timers prevent immediate visible cheating,
- [ ] eligibility queries exist for later anomaly systems,
- [ ] and debug output makes the mutation legality rules understandable during playtesting.
