# Sprint 2 Concerns Log

Discoveries during Sprint 2 implementation that may need follow-up as TODOs.

---

## C1: PortalVisible fires for all adjacent nodes regardless of camera direction

**Discovered:** 2026-05-09, Phase 0 (TD0018 probe wiring)

**Observation:** After wiring real `PortalProbeData` from `PortalAnchorAuthoring` transforms, the F4 Node Visibility overlay shows `PortalVisible` for ALL adjacent nodes simultaneously, even when the player camera is facing away from those portals. Expected behavior: only portals within the camera's view cone should evaluate as visible.

**Hypotheses (ranked by likelihood):**

1. **Portal-crossing guard too permissive.** `PortalVisibilityEvaluator.EvaluateSingle()` returns `true` whenever `planeDot < 0` (player is "behind" the portal plane). If `PortalAnchorAuthoring` transforms have forward vectors that don't precisely face outward from the room interior, this guard triggers from inside the room and short-circuits the facing test. This is the most likely cause — the anchors were authored for crossing detection (trigger volumes), not for visibility evaluation.

2. **Dot threshold too wide.** The default `dotThreshold` is 0.5 (~60-degree half-angle cone). In a room like `v_hall_a` with 3 doorways, portals may all fall within a 120-degree forward cone from center-room positions. This would only explain the issue when standing near the center of a room, not when facing a wall.

3. **Anchor forward vectors defaulting to identity.** If `PortalAnchorAuthoring` GameObjects were placed without explicitly setting rotation, `transform.forward` defaults to `Vector3.forward` (0,0,1). If all anchors share the same forward, the evaluator can't distinguish which direction the portal faces.

**Impact:** Currently cosmetic — the activation system already uses `Adjacent` as a reason, so rooms appear regardless. But when the observation lock system (Phase 3) consumes visibility results for mutation gating, false-positive `PortalVisible` will over-lock nodes that should be mutation-eligible. This would make the grace timer meaningless (nodes never become eligible because they're always "visible").

**Severity for Sprint 2:** Medium. Must be investigated before Phase 3 visibility lock testing. If portal visibility always returns true for all adjacent nodes, the visibility lock adds no information beyond what occupancy + adjacency already provide.

**Likely fix directions:**
- Audit `PortalAnchorAuthoring` transform orientations in all 5 room prefabs — forwards should point outward from the room interior through the doorway.
- Consider whether the portal-crossing guard (`planeDot < 0`) should be tightened or removed for visibility evaluation (it was designed for "keep destination visible after player steps through" but may fire too eagerly from inside the room).

---

## C2: FindObjectsByType overload change may exclude inactive handles

**Discovered:** 2026-05-09, Phase 0 (code review)

**Observation:** The deprecation fix changed `FindObjectsByType<NodePresentationHandle>(FindObjectsSortMode.None)` to `FindObjectsByType<NodePresentationHandle>(FindObjectsInactive.Exclude)`. The old overload included inactive objects; the new one excludes them. If a `NodePresentationHandle` is on a deactivated room prefab root before initial activation, it won't be discovered.

**Likely non-issue:** Room roots stay active (only the `Presentation` child toggles). But worth a Play mode check if room discovery ever breaks.

**Severity:** Low. Monitor.

---

## C3: Per-frame allocations in ObservationLockSystem.Tick()

**Discovered:** 2026-05-09, Phase 2 (code review)

**Observation:** Every `Tick()` creates 3x `HashSet<string>` and 2x `List<string>` (key snapshots for safe iteration). Negligible for a 5-node graph. Should pool allocations if the graph scales significantly.

**Severity:** Low. Add pooling if perf budgets tighten per research report 10.

---

## C4: Update execution order race between NodeStreamingController and GraphRuntimeHost

**Discovered:** 2026-05-09, Phase 5 (adversarial code review)

**Observation:** `NodeStreamingController.Update()` feeds portal results to the observation input source. `GraphRuntimeHost.Update()` ticks the lock system. Unity does not guarantee `Update()` call order between MonoBehaviours. If the host ticks first, the lock system evaluates with stale/empty portal data for one frame. In a horror game, this could let a mutation fire on a room the player is looking at through a portal.

**Fix applied:** `[DefaultExecutionOrder(-10)]` on `NodeStreamingController`, `[DefaultExecutionOrder(0)]` on `GraphRuntimeHost`. Attributes are declarative, travel with the class, and create no dead paths. Codex review validated this approach over the alternative (moving `Tick()` into the controller) which would have created 3 dead paths where observation stops ticking.

**Alternative considered and rejected:** `TickObservation()` push model — controller calls host tick explicitly after feeding data. Codex identified that `NodeStreamingController.Update()` has 3 early-return paths (forceAllActive, no player, empty nodeId) that would freeze stale locks and grace timers. A `_hasExternalTicker` latch would also become a one-way trap across despawn/reconnect.

**Severity:** Fixed. No follow-up needed.

---

## C5: Debug override methods available in release builds

**Discovered:** 2026-05-09, Phase 5 (adversarial code review)

**Observation:** `ForceNodeLock`, `ForceNodeUnlock`, `ForceEdgeLock`, `ForceEdgeUnlock`, and `ClearDebugOverrides` on `ObservationLockSystem` are public methods with no preprocessor guards. They exist in release builds.

**Why not fixed now:** Codex review identified that `#if UNITY_EDITOR || DEVELOPMENT_BUILD` guards on `ObservationLockSystem` create a build-target-dependent public API. The overlay's `GetLockSystem()` cast references the concrete type — guarding the methods causes compile errors in release builds. Fixing properly requires either: (a) moving debug methods to a separate `ObservationLockDebugExtensions` class, or (b) guarding the entire overlay with `#if`. Both add complexity for a jam project.

**Risk assessment:** Near-zero for a jam build. The methods are no-ops without intentional calls. No mutation system exists yet to exploit forced unlocks.

**Deferred to:** Post-jam hardening. When the mutation system (M3/M4) ships and debug overrides could actually cause gameplay impact, extract debug methods to a guarded utility class.

---

## C6: Mutable struct + shared List reference in observation state

**Discovered:** 2026-05-09, Phase 5 (adversarial code review)

**Observation:** `NodeObservationState` is a struct containing a `List<LockReason>`. `GetAllNodeStates()` exposes the dictionary. Consumers get struct copies that share the same mutable list. If a consumer holds a reference across tick boundaries, they'll see the list cleared mid-read.

**Current risk:** Safe. The only consumers are IMGUI overlays that read within a single `OnGUI` frame. No cross-frame references exist.

**Deferred to:** Co-op sprint (M4). When multiplayer observation aggregation ships, state may be read across network tick boundaries. At that point, consider making state types immutable or returning snapshots.

**Severity:** Low. Monitor when adding new observation state consumers.
