# Sprint 4A — Substitution Anomaly Vertical Slice

## Sprint objective
Implement the first substitution anomaly slice: a familiar doorway or room resolves into a different authored room variant while preserving coherent local traversal and graph legality. Sprint 4A is successful when one baseline node or threshold can legally substitute to an alternate variant, be experienced in play as a meaningful “this place is wrong now” event, and reset cleanly without breaking graph identity or routing [file:12][file:13][file:14][file:15].

## Sprint question
Can the project support one real room/space substitution through the graph runtime and presentation systems, rather than by faking a one-off scene swap, while keeping the effect readable and debuggable [file:12][file:13][file:15]?

## Why this sprint exists
The anomaly taxonomy marks substitution anomalies as an MVP family because they produce strong psychological impact without requiring the full complexity of domain overlays or large inserted subgraphs. This sprint exists to prove that a familiar space can become a different version of itself through modular runtime rules, which aligns with the broader project philosophy of proving one major question at a time in narrow, testable slices [file:12][file:15].

## In scope
- One substitution anomaly family only.
- One authored baseline node and one alternate compatible variant.
- One legal trigger path gated by observation rules.
- Variant activation through runtime node/domain substitution.
- Debug visibility into baseline state versus active variant state.
- Reset and replay path.
- Playable proof in `House_Prototype`.

## Out of scope
- Multiple simultaneous substitutions.
- Procedural variant generation.
- Full shadow-house domain swaps.
- Tardis expansion.
- Artifact/anchor integration.
- Entity interactions with substituted rooms.
- Final FX/audio polish.

## Design intent
The first substitution anomaly should feel like betrayal of familiarity, not merely decoration. The player should recognize the place, sense that it is “supposed” to be the same room, and then realize key spatial, visual, or route facts are wrong. The effect should be strong enough to create dread, but clear enough that the player can still understand where they are in the run.

## Substitution anomaly definition
A substitution anomaly is a graph/runtime event in which a baseline node resolves into a different authored node variant or active presentation/domain while preserving a compatible connection contract with neighboring graph elements.

In practical terms, substitution can mean one of two tightly related implementations:
- **node presentation substitution**: the logical node id remains stable, but its active authored variant changes,
- **compatible node swap**: the baseline node temporarily resolves to another compatible node definition with matching edge contract.

For Sprint 4A, prefer the first approach if possible because it keeps identity clearer and lowers routing complexity.

## Recommended first slice
Use one room that is easy for the player to remember and revisit, such as:
- small bedroom,
- study,
- bathroom,
- nursery,
- or side hallway.

Recommended variation dimensions:
- changed prop layout,
- altered wall materials or damage,
- missing or added doorway dressing,
- altered lighting or color temperature,
- one changed route affordance that is still compatible with graph contract.

Keep the variant semantically similar enough that the player recognizes the room, but different enough that the substitution feels unmistakable.

## Core constraints

### 1. Graph compatibility must be preserved
If a node substitutes to another variant, its edge contract must remain valid for the current test slice. Do not let substitution silently destroy required connections.

### 2. Observation legality still applies
The room may only substitute when the affected node and related edges are mutation-eligible under the observation lock system.

### 3. Substitution should privilege stable identity in Sprint 4A
Prefer “same logical room, different active variant” over “entirely different logical node id” unless the latter becomes necessary. This keeps debug, saving, and later systems easier to reason about.

### 4. One substitution at a time
Keep only one active substitution event in this sprint.

## System ownership

### `AnomalyDirector`
Owns:
- selecting or triggering the substitution pattern,
- checking legality,
- applying and clearing the active substitution event,
- recording mutation history.

Does not own:
- node activation,
- portal visibility,
- or room presentation rendering details.

### `GraphMutationService`
Owns:
- applying the substitution pattern,
- rebinding active node variant or active compatible node mapping,
- restoring baseline variant on reset.

### `SpatialGraphRuntime`
Owns:
- current runtime node state,
- active variant references,
- and any compatibility checks needed for safe substitution.

### `NodeStreamingController`
Owns:
- ensuring only the active room presentation variant is streamed/rendered.

### `ObservationLockSystem`
Owns:
- legality gating for substitution.

### `SpatialDebugOverlay`
Owns:
- exposing baseline node variant, active node variant, and blocking reasons.

This keeps responsibilities narrow and consistent with the project-wide rule that reusable logic belongs in systems while scene objects stay descriptive and thin [file:13][file:14][file:15].

## Data and authoring
Recommended assets:
- `SubstitutionPatternDefinition`
- `NodeVariantDefinition`
- optional `CompatibleNodeContractDefinition`

Each substitution pattern should define:
- pattern id,
- baseline node id,
- baseline variant id,
- substitute variant id,
- required compatible edge contract,
- affected edge ids,
- activation preconditions,
- reset behavior,
- optional art/audio tags.

Each node variant definition should define:
- variant id,
- presentation root or prefab reference,
- optional light profile,
- optional prop set/profile,
- route-affordance tags,
- and semantic labels.

This keeps new anomaly variants data-driven, consistent with the earlier architecture preference for new content = mostly new assets and definitions rather than giant code rewrites [file:12][file:13][file:14].

## Suggested files

### Runtime / mutation
- `Scripts/Spatial/Mutations/SubstitutionPatternDefinition.cs`
- `Scripts/Spatial/Mutations/NodeVariantDefinition.cs`
- `Scripts/Spatial/Mutations/GraphMutationService.cs`
- `Scripts/Spatial/Mutations/RuntimeMutationEvent.cs`
- `Scripts/Spatial/Mutations/AnomalyDirector.cs`

### Authoring / presentation
- `Scripts/Spatial/Authoring/NodeVariantAuthoring.cs`
- `Scripts/Spatial/Runtime/NodeVariantController.cs`

### Debug
- `Scripts/UI/Debug/SubstitutionDebugOverlay.cs`
- `Scripts/UI/Debug/SubstitutionDebugGizmos.cs`

### Existing dependencies touched
- `SpatialGraphRuntime.cs`
- `NodeStreamingController.cs`
- `PortalVisibilityController.cs`
- `ObservationLockSystem.cs`
- `MatchManager.cs`

## Contracts

### `INodeVariantQuery`
```csharp
public interface INodeVariantQuery
{
    string GetBaselineVariantId(string nodeId);
    string GetActiveVariantId(string nodeId);
    bool IsNodeSubstituted(string nodeId);
}
```

### `ISubstitutionMutationService`
```csharp
public interface ISubstitutionMutationService
{
    bool CanApplySubstitution(string patternId);
    bool TryApplySubstitution(string patternId);
    bool ResetSubstitution(string patternId);
    string GetActiveSubstitutionPatternId();
}
```

### `INodeContractQuery`
```csharp
public interface INodeContractQuery
{
    bool IsVariantCompatibleWithNode(string nodeId, string variantId);
    IReadOnlyList<string> GetRequiredEdgeIds(string nodeId);
    IReadOnlyList<string> GetBlockingReasons(string nodeId, string variantId);
}
```

These interfaces deliberately keep the system surface area small and composable, which matches the earlier anti-sprawl guidance and makes AI-scaffolded implementation safer [file:13][file:14][file:15].

## Core rules

### Rule 1 — Substitution is not random art dressing
The change must have gameplay or navigational meaning, even if subtle. It should alter trust in the space, not just swap decorations.

### Rule 2 — Preserve local coherence
The player should still be able to move, enter, and exit the substituted room coherently.

### Rule 3 — Variant identity must be visible in debug
A developer must always be able to see:
- baseline variant id,
- active variant id,
- substitution pattern id,
- legality state,
- and reset state.

### Rule 4 — Do not break edge contracts
If the substituted room changes route affordances, those changes must be intentionally authored and still valid for the current graph context.

### Rule 5 — One active substitution only in Sprint 4A
Do not stack room substitutions yet.

## Tasks

### 1. Author the first room variant pair
- [ ] Create one baseline room variant.
- [ ] Create one substitute room variant.
- [ ] Confirm both satisfy the same edge/entry contract for the chosen node.
- [ ] Author one `SubstitutionPatternDefinition` asset.

#### Acceptance tests
- [ ] Baseline and substitute variants both exist and are editable.
- [ ] Pattern asset lists node id, baseline variant id, and substitute variant id.
- [ ] Required edge contract is documented.

### 2. Build node variant controller
- [ ] Create `NodeVariantController.cs`.
- [ ] Allow a runtime node to switch active presentation variant.
- [ ] Ensure only one variant presentation is active at a time.

#### Acceptance tests
- [ ] Baseline variant is active by default.
- [ ] Substitute variant can be activated at runtime.
- [ ] Switching variants does not leave both active at once.

### 3. Extend mutation service for substitution
- [ ] Add substitution support to `GraphMutationService.cs` or dedicated substitution service.
- [ ] Record active substitution state.
- [ ] Restore baseline variant on reset.

#### Acceptance tests
- [ ] Service can apply one substitution pattern.
- [ ] Active variant changes after substitution.
- [ ] Reset returns node to baseline variant.
- [ ] No stale substitution state remains after restart.

### 4. Integrate legality checks with observation system
- [ ] Query `ObservationLockSystem` before applying substitution.
- [ ] Block substitution when node or affected edges are still locked.
- [ ] Record blocking reasons for debug.

#### Acceptance tests
- [ ] Substitution cannot occur while the room is occupied or visibly locked.
- [ ] Substitution can occur when node and edges are eligible.
- [ ] Failed attempts report blocking reasons clearly.

### 5. Integrate with streaming and portal visibility
- [ ] Ensure substituted variant respects node activation rules.
- [ ] Ensure adjacent visibility through portals reflects the currently active variant.
- [ ] Verify activation/deactivation of the node does not corrupt variant state.

#### Acceptance tests
- [ ] Active variant renders when the node streams in.
- [ ] Looking through a doorway reveals the correct active variant.
- [ ] Variant state persists correctly across node activation changes.

### 6. Add substitution debug tools
- [ ] Create `SubstitutionDebugOverlay.cs`.
- [ ] Create `SubstitutionDebugGizmos.cs`.
- [ ] Show baseline variant id, active variant id, active pattern id, and blocking reasons.
- [ ] Show whether the node is currently mutation-eligible.

#### Acceptance tests
- [ ] Overlay clearly identifies the current room variant.
- [ ] Failed substitution attempts expose why they were blocked.
- [ ] Developer can explain current state from debug output alone.

### 7. Add reset and replay stability
- [ ] Ensure restart clears active substitution state.
- [ ] Ensure player can re-enter the node after reset with no stale state.
- [ ] Run repeated trigger/enter/exit/reset tests.

#### Acceptance tests
- [ ] Restart restores the baseline variant reliably.
- [ ] Repeated substitution tests do not produce blocker errors.
- [ ] No stale active variant remains registered after reset.

## Debug expectations
Minimum substitution debug surface:
- node id,
- baseline variant id,
- active variant id,
- substitution pattern id,
- required edge contract,
- legality result,
- and blocking reasons.

This remains consistent with the project’s earlier debug-first principle for hidden systems [file:12][file:13][file:15].

## Suggested implementation order
Following the earlier one-question-per-sprint philosophy [file:12][file:15]:
1. Room variant authoring.
2. Node variant controller.
3. Mutation service support.
4. Observation legality integration.
5. Streaming/visibility integration.
6. Debug overlay.
7. Reset stabilization.

## Smoke test
Run this before marking Sprint 4A complete:
- [ ] Launch from `Bootstrap`.
- [ ] Enter `House_Prototype`.
- [ ] Confirm the chosen node shows its baseline variant by default.
- [ ] Move so the node becomes eligible under observation rules.
- [ ] Trigger the substitution pattern.
- [ ] Confirm debug overlay shows the active pattern and substitute variant id.
- [ ] Re-enter or view the room through a doorway and confirm the substitute variant is active.
- [ ] Confirm traversal into and out of the room remains coherent.
- [ ] Reset the anomaly.
- [ ] Confirm the room returns to the baseline variant.
- [ ] Restart the round and confirm no stale variant state persists.
- [ ] Confirm no critical console errors occur.

## Deferred from Sprint 4A
- Multiple substitute variants per node.
- Procedural substitution selection.
- Cross-domain substitutions.
- Complex route-contract changes.
- Strong audiovisual substitution reveals.
- Narrative/environmental storytelling layers tied to substitutions.

## Sprint done
Mark complete when:
- [ ] one familiar room can legally resolve into a different authored variant,
- [ ] the player can experience the change as a coherent spatial event,
- [ ] graph compatibility and traversal remain intact,
- [ ] and the substitution state is easy to inspect, replay, and reset during development.
