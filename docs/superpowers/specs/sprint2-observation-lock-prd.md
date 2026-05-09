## Problem Statement

The house graph runtime (S1A-S1C) can activate and deactivate rooms, but there are no rules governing *when* spatial mutations are legal. Without observation-based mutation gating, any future anomaly system would change geometry arbitrarily -- breaking the player covenant that "the house changes only when certainty collapses." Sprint 2 builds the substrate that answers: "Is this node or edge safe to mutate right now?"

## Solution

An observation lock system that tracks which nodes and edges are currently observed (occupied, portal-visible, or in a grace cooldown) and exposes a query interface for mutation eligibility. The system is a pure C# evaluator hosted by the existing graph runtime host, with a debug overlay so developers can explain why any node is or is not mutable from runtime data alone.

**Scope framing:** Sprint 2 uses per-node and per-edge lock state as its internal observation substrate for the current 5-room slice. Mutation regions remain the intended higher-level abstraction for anomaly systems (S3+), and later mutation-facing systems may aggregate node/edge lock state into region eligibility. This sprint builds the substrate, not the final anomaly-facing abstraction.

## User Stories

1. As a player, I want the room I'm standing in to never mutate while I'm inside, so that I feel grounded and safe in my immediate space.
2. As a player, I want rooms I can see through doorways to stay stable, so that I never witness the house changing in my line of sight.
3. As a player, I want a brief grace period after I look away from a room, so that spatial changes don't feel like uncanny flicker.
4. As a player, I want edges (connections between rooms) to lock when I'm in an adjacent room, so that the path I just walked doesn't rearrange behind me immediately.
5. As a developer, I want a pure C# lock system with a `Tick(deltaTime)` method, so that I can test all lock logic in EditMode without Play mode or scene dependencies.
6. As a developer, I want the lock system to consume observation facts through an input interface (`IObservationInputSource`), so that I can swap single-player and multiplayer input sources without changing lock logic.
7. As a developer, I want a `LocalObservationInputSource` that polls `PlayerNodeTracker` and `PortalVisibilityController`, so that the lock system works for single-player graybox testing now.
8. As a developer, I want `IObservationLockQuery` to expose lock state, lock reasons, grace remaining, and mutation eligibility per node and edge, so that future mutation systems and debug tools have a clean query surface.
9. As a developer, I want an `ObservationRulesDefinition` ScriptableObject for grace durations and refresh intervals, so that I can tune observation behavior without code changes.
10. As a developer, I want a debug overlay showing occupied nodes, visible nodes/edges, lock reasons, grace countdowns, and eligible mutation targets, so that I can explain why any node is or is not mutable from overlay data alone.
11. As a developer, I want debug override capability (force lock/unlock) in the overlay, so that I can test downstream systems without waiting for real observation state changes.
12. As a developer, I want scene-view gizmos color-coding nodes/edges by lock state, so that I can see the spatial lock map at a glance.
13. As a developer, I want a `Reset()` method on the lock system that clears all state, so that round restart can return to a clean slate.
14. As a developer, I want `NodeStreamingController` to wire real `PortalProbeData` from portal anchor transforms (TD0018 fix), so that `PortalVisibilityController.EvaluatePortals()` returns real results and visibility-dependent systems work against truth.
15. As a developer, I want enumeration methods (`GetAllNodeStates`, `GetAllEdgeStates`) on the query interface, so that the debug overlay can iterate all lock state without maintaining its own node list.
16. As a tester, I want ~34 EditMode tests covering lock logic, grace timers, state types, input source derivation, and probe wiring, so that regressions are caught before Play mode.
17. As a tester, I want 7 manual smoke test items (occupancy lock, edge lock, visibility lock, grace countdown, eligibility transition, debug override, restart reset), so that the system is verified end-to-end in Play mode.

## Implementation Decisions

### Module 1: Portal Probe Wiring (Phase 0, TD0018 fix)

Resolves TD0018: `NodeStreamingController.GetPortalResults()` currently passes `Array.Empty<PortalProbeData>()` to `PortalVisibilityController.EvaluatePortals()`. The fix builds real `PortalProbeData` from `PortalAnchorAuthoring` transforms registered in the scene.

This is a **hard gate**. All visibility-dependent work (visibility lock, visibility overlay acceptance) depends on probe truth being wired first. Do not parallelize visibility work ahead of this.

### Module 2: Observation State Types (Phase 1)

New types, all under the `Desync.World.Graph` assembly:

- **`LockReason`** enum: `Occupied`, `AdjacentOccupiedEdge`, `PortalVisible`, `GracePeriod`, `DebugForced`, `ProtectedByRule`, `None`. `ProtectedByRule` is a dormant seam for non-observation protection (stable anchors, higher-priority rules) -- it exists in the enum and is handled in eligibility queries, but no Sprint 2 code path sets it. This preserves an additive extension point so future protection rules can be injected without changing the core observation model.
- **`NodeObservationState`** struct: active lock reasons, grace timer, last-reason-cleared timestamp.
- **`EdgeObservationState`** struct: same shape, separate type for semantic clarity and independent tunables.
- **`IObservationInputSource`** interface: `GetOccupiedNodeIds()`, `GetVisibleNodeIds()`, `GetVisibleEdgeIds()`. Returns `IReadOnlyList<string>`.
- **`IObservationLockQuery`** interface: `IsNodeLocked`, `IsEdgeLocked`, `IsNodeMutationEligible`, `IsEdgeMutationEligible`, `GetNodeLockReasons`, `GetEdgeLockReasons`, `GetNodeGraceRemaining`, `GetEdgeGraceRemaining`, `GetAllNodeStates`, `GetAllEdgeStates`.
- **`ObservationRulesDefinition`** ScriptableObject: `nodeGraceSeconds` (default 2.0), `edgeGraceSeconds` (default 1.5), `visibilityRefreshInterval` (default 0f = every frame), `lockDebugVerbose` (bool). No `portalVisibilityDotThreshold` -- that belongs to `PortalVisibilityController`.

### Module 3: ObservationLockSystem (Phases 2-3, deep module)

Pure C# class. This is the deep module: complex internals, narrow public surface.

- Consumes `IObservationInputSource` for observation facts.
- Exposes `IObservationLockQuery` for downstream consumers.
- `Tick(deltaTime)` drives the evaluation loop.
- Occupancy lock: occupied node is locked, adjacent edges are locked.
- Visibility lock: portal-visible nodes and edges are locked.
- Grace timers: per-target (one timer per node/edge), starts when the LAST active reason clears, configurable per-type.
- Visibility refresh interval: accumulator pattern, `<=0` means every-frame evaluation.
- Debug override: force-lock or force-unlock individual targets.
- `Reset()`: clears all dictionaries, timers, and overrides.

**Architectural guardrail:** `ObservationLockSystem` remains a pure lock-evaluation and query service over externally supplied observation facts. Perception gathering, mutation scheduling, stable-anchor policy, networking concerns, and perception policy stay outside it even if the first local adapter lives nearby. Later work must not accrete these responsibilities into this class.

### Module 4: LocalObservationInputSource (Phase 4)

Concrete `IObservationInputSource` implementation for single-player graybox.

- Polls `PlayerNodeTracker.CurrentNodeId` for occupied nodes.
- Polls `PortalVisibilityController.EvaluatePortals()` for visible nodes.
- Derives visible edge IDs via `SpatialGraphRuntime.GetConnectedEdges()` filtered by visible destination node IDs.
- Guards against null `CurrentNodeId` (TD0013 trigger overlap race).

**Scope framing:** `LocalObservationInputSource` is a single-player local adapter for Sprint 2. In co-op, observation truth is expected to move to an authority-owned contribution/aggregation model, and this interface exists to make that replacement possible without rewriting `ObservationLockSystem`.

**Edge visibility approximation:** Edge visibility derivation in Sprint 2 is a graph-query approximation over the current portal results contract. Future portal/threshold authority work may replace this with explicit portal/edge identity reporting.

### Module 5: GraphRuntimeHost Wiring (Phase 4)

Add an Update loop to `GraphRuntimeHost` that calls `ObservationLockSystem.Tick(deltaTime)`. Wire dependencies (input source, rules definition) via serialized fields or initialization.

### Module 6: Debug Overlay + Gizmos (Phase 5)

- **`ObservationDebugOverlay`**: IMGUI overlay (follows `SpatialDebugOverlay` / `SpatialVisibilityDebugOverlay` pattern). Shows occupied nodes, visible nodes/edges, lock reasons per target, grace countdowns, eligible mutation target count. Toggle key. Debug override controls.
- **`ObservationDebugGizmos`**: scene-view gizmos color-coding nodes/edges by lock state (locked/grace/eligible).
- A developer must be able to explain why any node is not mutable from overlay data alone.

### Module 7: Reset + Polish (Phase 6)

- `ObservationLockSystem.Reset()` clears all state.
- Current repo has a debug reset path (`SpatialDebugOverlay` F5 key calls `Runtime.Reset()` then `Runtime.Initialize()`). There is no formal round-reset orchestration service today. Sprint 2 wires observation reset into the existing debug reset path. Formal round-restart ownership is a future concern and should not be assumed to exist.

### Module Placement

Sprint 2 remains under the existing `World/Graph` module layout (`Scripts/World/Graph/Runtime/`, `Scripts/World/Graph/Definitions/`, `Scripts/World/Graph/Debug/`) for consistency with implemented code. Design docs describe a more mutation-oriented namespace future; that is deferred until mutation systems actually land.

## Testing Decisions

### What makes a good test

Tests verify external behavior through the public interface, not implementation details. A test should answer "does the system produce the correct output for this input?" not "does the system use this internal data structure?" Tests should be stable across refactors that preserve behavior.

### Modules tested (EditMode)

**Module 1 (Portal Probe Wiring):** ~3 tests verifying that `NodeStreamingController` builds non-empty `PortalProbeData` from registered portal anchors and passes it through to `EvaluatePortals`. Existing evaluator tests already cover probe-to-result evaluation in isolation; new tests cover the wiring gap.

**Module 2 (State Types + Config):** ~6 tests covering state construction, reason tracking, and SO validation/defaults.

**Module 3 (ObservationLockSystem):** ~20 tests covering occupancy lock, edge lock, visibility lock, grace countdown, re-lock resets grace, eligibility transition, debug override, reset clears all state, and enumeration methods. Uses fake `IObservationInputSource` (mock layer).

**Module 4 (LocalObservationInputSource):** ~5 tests with stub `PlayerNodeTracker`, `PortalVisibilityController`, and `SpatialGraphRuntime`. Covers occupied node derivation, visible node derivation, edge ID derivation from graph query, null CurrentNodeId guard, and empty-results handling.

**Estimated total:** ~34 new EditMode tests (bringing repo total to ~161).

### Prior art

Existing test conventions in `Tests/EditMode/`:
- NUnit framework with `[TestFixture]`, `[SetUp]`, `[TearDown]`, `[Test]` attributes.
- Naming: `MethodName_Condition_ExpectedResult`.
- Setup creates test objects, teardown calls `Object.DestroyImmediate()`.
- ScriptableObjects created via `ScriptableObject.CreateInstance<T>()`.
- Arrange/Act/Assert pattern.
- Representative files: `PlayerNodeTrackerTests.cs`, `SpatialGraphRuntimeTests.cs`, `NodeStreamingControllerTests.cs`, `PortalVisibilityEvaluatorTests.cs`.

### Manual smoke test (7 items)

1. Launch Bootstrap > House_Prototype
2. Current node shows occupancy-locked in overlay
3. Connected edges show locked in overlay
4. Look through portal > destination shows visibility-locked
5. Step away > grace countdown visible
6. Wait for expiry > node shows eligible
7. Debug override > lock reason changes
8. Restart round > all state resets

### Review gate

At sprint end, dispatch `/review` agent to confirm Module 1 test coverage is sufficient (probe wiring path was previously untested per TD0018 gap analysis).

## Out of Scope

- **ObservationSnapshot type**: no consumer in Sprint 2; add when sync/anomaly systems need batch state.
- **Multiplayer observation aggregation**: authority model unsettled; `IObservationInputSource` interface supports future swap, implementation deferred.
- **True LOS raycasting**: portal visibility is the Sprint 2 approximation; LOS is endgame. Add TODO to `TODO.md`.
- **Actual graph mutations**: Sprint 3+.
- **Loop remaps, shadow-house, Tardis spaces**: Sprint 3+.
- **Entity AI**: Sprint 5+.
- **Cross-machine multiplayer testing**: LAN-only graybox.
- **Per-reason grace timers**: per-target is sufficient for Sprint 2 debug needs.
- **Region-level lock abstraction**: per-node/edge matches current PDD; region locks are a future optimization.
- **Observer identity / per-player contribution tracking**: deferred to co-op sprint.
- **Expanding PortalVisibilityResult contract**: keep current fields unless Sprint 3 requires portal/edge identity.

## Further Notes

- **TD0013 (PlayerNodeTracker trigger overlap race)**: guard against null `CurrentNodeId` in `LocalObservationInputSource`; grace timer covers brief gaps. Defer fix unless it reproduces under S2 conditions.
- **Failure mode: visibilityRefreshInterval > grace duration**: stale visibility data could outlast grace. Test this edge case; warn in SO validation.
- **Parallelization**: Phase 0 (probe wiring) is a hard gate for visibility work. Phases 1-3 (types + lock system) can proceed in parallel with Phase 0 since they use fake inputs. Phase 4 (wiring) depends on both. Phase 5 (debug) depends on Phase 2 interfaces.
- **LOS endgame**: the `IObservationInputSource` interface is the right seam for future LOS raycasting. A future `LOSObservationInputSource` replaces `LocalObservationInputSource` without touching `ObservationLockSystem`.
- **Key learnings from eng review**: observation-lock-activation-separation (activation is presentation, observation is legality), per-target-grace-not-per-reason (simpler, matches query contract), td0018-empty-portal-probes (Codex caught this blocking dependency that Claude review missed).
