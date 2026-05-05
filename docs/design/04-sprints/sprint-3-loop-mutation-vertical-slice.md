# Sprint 3 — Loop Mutation Vertical Slice

## Sprint objective
Implement the first real spatial anomaly: a legal loop mutation that remaps one portal path so the player can traverse a corridor or threshold and return to an unexpected but coherent continuation of the house graph. Sprint 3 is successful when one loop anomaly can be triggered, experienced in play, debugged clearly, and reset cleanly without breaking graph integrity [file:12][file:13][file:14][file:15].

## Sprint question
Can the project express one impossible-space trick as a clean graph transform, constrained by observation rules and visible through the current portal/activation harness, without collapsing into scene-specific hacks [file:12][file:13][file:15]?

## Why this sprint exists
Sprints 1A, 1B, and 2 establish the substrate: authored graph, visibility harness, and mutation legality through observation locks. The next question should be narrow and playable, matching the earlier milestone philosophy of proving one major question at a time before widening scope [file:12][file:15]. This sprint exists to prove that the project can deliver a genuine spatial-horror beat through system architecture rather than by faking a single set-piece in scene logic.

## In scope
- One loop mutation family only.
- One or two authored legal remap patterns.
- Triggering mutation only when observation rules allow it.
- Debug visualization of pre-state, post-state, and reasons for legality.
- Clean reset and replay path.
- Playable proof inside `House_Prototype`.

## Out of scope
- Multiple anomaly families.
- Room substitution systems.
- Tardis insertion.
- Shadow-house bridging.
- Artifact/anchor progression.
- Creature stalking behavior.
- Multiplayer sync beyond future-safe code shape.
- Final FX polish.

## Design intent
The first anomaly should feel unmistakable but legible. The player should be able to say, “the hall changed when I wasn’t observing it,” not “the game teleported me randomly.” This sprint prioritizes readability, trust, and replayable correctness over maximal weirdness.

## Loop mutation definition
A loop mutation is a graph transform that remaps one or more active portal edges so a traversal path resolves to a different destination than the baseline route while preserving a coherent local walk experience.

At a high level:
- baseline graph route says `A -> B -> C`,
- mutated route may temporarily resolve as `A -> B -> A'` or `A -> B -> C'`,
- where the remap is legal only if the affected nodes and edges are mutation-eligible under Sprint 2 rules.

## First-pass mutation pattern
For Sprint 3, use only one mutation family:
- **Corridor loop remap**

Recommended test shape:
- `EntryHall -> LongHall -> EndDoor -> ReturnHall`
- mutate the `EndDoor` destination so traversing through it resolves back into a variant or repeat of a previous hall node sequence.

This is deliberately constrained because earlier planning emphasized narrow slices and “one major question at a time” instead of broad content expansion [file:12][file:15].

## System ownership

### `AnomalyDirector`
Owns:
- requesting a candidate mutation,
- evaluating whether a legal remap pattern can be applied,
- applying the transform through mutation services,
- recording the active mutation event.

Does not own:
- observation legality rules,
- portal visibility computation,
- or node activation.

### `ObservationLockSystem`
Owns legality information only.
It answers whether nodes and edges are eligible.

### `SpatialGraphRuntime`
Owns runtime topology state.
It applies the resulting edge remap and exposes current destinations.

### `PortalResolver`
Owns active destination lookup after mutation.

### `NodeStreamingController` and `PortalVisibilityController`
Consume the new topology but do not decide whether mutation should occur.

### `SpatialDebugOverlay`
Owns mutation trace visibility for developers.

This ownership split follows the same narrow-role guidance that earlier project docs established as important for maintainability and AI-assisted implementation quality [file:13][file:14][file:15].

## Data and authoring
Recommended static assets:
- `LoopMutationDefinition`
- `LoopPatternDefinition`
- optional `MutationTestScenarioDefinition`

Each loop pattern should define:
- stable pattern id,
- candidate source edge ids,
- remapped destination edge ids or destination node ids,
- required preconditions,
- affected node set,
- affected edge set,
- and reset behavior.

This keeps the project aligned with the rule that new content should mostly mean new data rather than giant new engine code branches [file:12][file:13].

## Suggested files

### Runtime / mutation
- `Scripts/World/Graph/Mutations/AnomalyDirector.cs`
- `Scripts/World/Graph/Mutations/LoopMutationDefinition.cs`
- `Scripts/World/Graph/Mutations/LoopPatternDefinition.cs`
- `Scripts/World/Graph/Mutations/GraphMutationService.cs`
- `Scripts/World/Graph/Mutations/RuntimeMutationEvent.cs`

### Debug
- `Scripts/UI/Debug/MutationDebugOverlay.cs`
- `Scripts/UI/Debug/MutationDebugGizmos.cs`

### Existing dependencies touched
- `ObservationLockSystem.cs`
- `SpatialGraphRuntime.cs`
- `PortalResolver.cs`
- `NodeStreamingController.cs`
- `PortalVisibilityController.cs`
- `MatchManager.cs`

## Contracts

### `IGraphMutationService`
```csharp
public interface IGraphMutationService
{
    bool CanApplyLoopPattern(string patternId);
    bool TryApplyLoopPattern(string patternId);
    bool ResetActiveMutation();
    RuntimeMutationEvent GetActiveMutation();
}
```

### `IMutationEligibilityQuery`
```csharp
public interface IMutationEligibilityQuery
{
    bool AreNodesEligible(IReadOnlyList<string> nodeIds);
    bool AreEdgesEligible(IReadOnlyList<string> edgeIds);
    IReadOnlyList<string> GetBlockingReasonsForNodes(IReadOnlyList<string> nodeIds);
    IReadOnlyList<string> GetBlockingReasonsForEdges(IReadOnlyList<string> edgeIds);
}
```

### `IPortalRouteQuery`
```csharp
public interface IPortalRouteQuery
{
    string GetResolvedDestinationNodeId(string edgeId);
    string GetBaselineDestinationNodeId(string edgeId);
    bool IsEdgeRemapped(string edgeId);
}
```

The API should remain deliberately narrow so future anomaly families can plug into the same framework without bloating core systems, which mirrors the earlier anti-sprawl guidance [file:14][file:15].

## Core rules

### Rule 1 — Mutation must be legal before it is spooky
Do not apply the remap unless all affected nodes and edges are eligible under Sprint 2 observation rules.

### Rule 2 — Mutations operate on graph routing, not scene teleports
The anomaly should resolve through runtime edge remapping and portal destination queries, not by arbitrarily moving the player transform.

### Rule 3 — Mutation must be explainable in debug
At any moment, a developer should be able to inspect:
- active pattern id,
- affected nodes,
- affected edges,
- current remapped routes,
- and blocking reasons if a pattern could not fire.

### Rule 4 — Only one active loop mutation in Sprint 3
Do not stack anomalies yet.

### Rule 5 — Reset must be cheap and trustworthy
A quick reset is mandatory so the anomaly can be replay-tested repeatedly, consistent with earlier acceptance-test-driven planning [file:1][file:15].

## Tasks

### 1. Define the first loop pattern asset
- [ ] Create `LoopMutationDefinition.cs` or `LoopPatternDefinition.cs`.
- [ ] Author one corridor-loop pattern for the 5-node test slice or its nearest expanded slice.
- [ ] Record affected nodes and edges explicitly.

#### Acceptance tests
- [ ] Pattern asset exists and is editable.
- [ ] Pattern clearly lists required nodes and edges.
- [ ] Pattern can be referenced by id at runtime.

### 2. Build mutation service
- [ ] Create `GraphMutationService.cs`.
- [ ] Implement baseline route lookup and remapped route application.
- [ ] Support single active mutation only.
- [ ] Support full reset to baseline graph routing.

#### Acceptance tests
- [ ] Service can apply one remap pattern.
- [ ] Resolved destination changes after mutation.
- [ ] Reset returns all remapped edges to baseline.
- [ ] No stale remap remains after restart.

### 3. Build anomaly director trigger path
- [ ] Create `AnomalyDirector.cs`.
- [ ] Query `ObservationLockSystem` before selecting/applying the loop pattern.
- [ ] Add one explicit trigger mode for testing, such as debug key or inspector button.
- [ ] Optionally add a simple automatic trigger when player leaves the affected area.

#### Acceptance tests
- [ ] Mutation does not apply if any affected node/edge is locked.
- [ ] Mutation applies when all affected parts are eligible.
- [ ] Trigger path is deterministic in test mode.
- [ ] Director records the active mutation event.

### 4. Integrate with portal resolution and activation
- [ ] Route all affected edge resolution through `PortalResolver`.
- [ ] Confirm `NodeStreamingController` and `PortalVisibilityController` respond correctly after remap.
- [ ] Verify active node set remains coherent after traversal into remapped space.

#### Acceptance tests
- [ ] Traversing the mutated portal leads to the remapped destination.
- [ ] Adjacent node activation still behaves correctly.
- [ ] Portal-visible results update against the remapped route.
- [ ] No obvious dead-end or null destination occurs after mutation.

### 5. Add mutation debug tools
- [ ] Create `MutationDebugOverlay.cs`.
- [ ] Create `MutationDebugGizmos.cs`.
- [ ] Show baseline route vs current route for affected edges.
- [ ] Show last attempted pattern and blocking reasons if it failed.
- [ ] Show active mutation timer/state if relevant.

#### Acceptance tests
- [ ] Overlay clearly identifies the active pattern.
- [ ] Overlay shows which edge ids are remapped.
- [ ] Failed mutation attempts expose blocking reasons.
- [ ] A developer can explain the anomaly from debug output alone.

### 6. Add restart and replay stability
- [ ] Ensure restart clears active mutation state.
- [ ] Ensure returning to baseline works even if player crossed the loop.
- [ ] Run repeated trigger/traverse/reset tests.

#### Acceptance tests
- [ ] Repeated mutation testing produces stable results.
- [ ] Reset works from both pre-traversal and post-traversal states.
- [ ] No blocker errors occur during repeated replay loops.

## Debug expectations
Minimum mutation debug surface:
- current active mutation id,
- current remapped edge ids,
- baseline destination vs current destination,
- affected nodes,
- affected edges,
- last failed mutation attempt,
- and observation blocking reasons.

This is consistent with the earlier debug-first principle for all hidden systems [file:12][file:13][file:15].

## Suggested implementation order
Following the earlier milestone approach of proving one question at a time [file:12][file:15]:
1. Pattern asset.
2. Mutation service.
3. Eligibility check integration.
4. Director trigger path.
5. Portal resolver integration.
6. Debug overlay.
7. Reset stabilization.

## Smoke test
Run this before marking Sprint 3 complete:
- [ ] Launch from `Bootstrap`.
- [ ] Enter `House_Prototype`.
- [ ] Confirm baseline route through the target corridor behaves as expected.
- [ ] Move so affected nodes/edges become eligible under observation rules.
- [ ] Trigger the loop mutation.
- [ ] Confirm debug overlay shows active pattern and remapped edges.
- [ ] Traverse the affected threshold and confirm route resolves to the remapped destination.
- [ ] Confirm active-node/portal-visible systems still behave coherently after traversal.
- [ ] Reset the mutation.
- [ ] Confirm baseline route is restored.
- [ ] Restart round and confirm no stale mutation state persists.
- [ ] Confirm no critical console errors occur.

## Deferred from Sprint 3
- Multiple simultaneous loop patterns.
- Automatic pacing logic for anomaly frequency.
- Strong audiovisual mutation presentation.
- Dynamic pattern selection by difficulty.
- Cross-player synchronization.
- Alternate anomaly families.

## Sprint done
Mark complete when:
- [ ] one loop anomaly can be applied legally,
- [ ] the player can experience it through real traversal,
- [ ] the graph/runtime/visibility systems remain coherent after remap,
- [ ] and the anomaly is easy to inspect, replay, and reset during development.
