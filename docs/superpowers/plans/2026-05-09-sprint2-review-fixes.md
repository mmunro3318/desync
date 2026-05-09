# Sprint 2 Pre-Ship Review Fixes (v2 — post-Codex review)

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix 4 findings from code review before shipping Sprint 2 PR.

**Architecture:** Fix the Update-order race between `NodeStreamingController` and `GraphRuntimeHost` using `[DefaultExecutionOrder]` attributes (declarative, no code changes, no dead paths). Add real test coverage for `BindObservationTracker` and `ResetObservation` that creates actual lock state before asserting reset. Add edge override tests.

**Tech Stack:** Unity 6, NUnit, EditMode tests.

**Current state:** 181 tests pass, branch `feat/sprint2-observation-lock`, 5 commits.

**Codex review changes (v1 → v2):**
- Task 1: Replaced `TickObservation()` push/latch approach with `[DefaultExecutionOrder]` attributes. Codex correctly identified 3 dead paths (forceAllActive, no player, empty nodeId) where observation would stop ticking entirely. Attributes keep both `Update()` methods unchanged — zero dead paths, zero coupling change.
- Tasks 2/3: Rewrote tests to create real lock state via `FakeObservationInputSource` injection before asserting reset. Codex correctly called out "fake coverage" — old tests passed even if methods were empty.
- Task 6: Dropped entirely. Codex identified that `#if`-guarding methods on a runtime class creates build-target-dependent API and compile breakage in the overlay. For a jam project, the risk of debug overrides leaking is near-zero. Deferred to C5 in sprint-2-concerns.md.

---

### Task 1: Fix Update execution order with `[DefaultExecutionOrder]`

The bug: `NodeStreamingController.Update()` feeds portal results to `GraphRuntimeHost.ObservationInput.SetPortalResults()`. `GraphRuntimeHost.Update()` calls `_observationLock.Tick()`. Unity does not guarantee which `Update()` runs first. If the host ticks first, the lock system sees stale/empty portal data for one frame.

The fix: Add `[DefaultExecutionOrder]` attributes. `NodeStreamingController` gets `-10` (runs first, feeds portal data). `GraphRuntimeHost` gets `0` (default, ticks lock system after data is fed). Attributes are declarative and travel with the class — no Script Execution Order settings to drift, no code changes, no dead paths.

**Files:**
- Modify: `unity-DESYNC/Assets/_Project/Scripts/World/Graph/Runtime/NodeStreamingController.cs:8` (add attribute)
- Modify: `unity-DESYNC/Assets/_Project/Scripts/World/Graph/GraphRuntimeHost.cs:12` (add attribute)

- [ ] **Step 1: Add `[DefaultExecutionOrder(-10)]` to `NodeStreamingController`**

In `NodeStreamingController.cs`, add the attribute before the class declaration:

```csharp
[DefaultExecutionOrder(-10)]
public class NodeStreamingController : MonoBehaviour
```

- [ ] **Step 2: Add `[DefaultExecutionOrder(0)]` to `GraphRuntimeHost`**

In `GraphRuntimeHost.cs`, add the attribute before the class declaration. While `0` is the default, making it explicit documents the ordering contract:

```csharp
[DefaultExecutionOrder(0)]
public class GraphRuntimeHost : MonoBehaviour
```

- [ ] **Step 3: Run all tests to verify no regressions**

Run: Unity MCP `run_tests` with `assembly_names: ["Desync.Tests.EditMode"]`
Expected: 181 pass, 0 fail

- [ ] **Step 4: Commit**

```bash
git add unity-DESYNC/Assets/_Project/Scripts/World/Graph/GraphRuntimeHost.cs \
       unity-DESYNC/Assets/_Project/Scripts/World/Graph/Runtime/NodeStreamingController.cs
git commit -m "fix: guarantee Update order — NodeStreamingController before GraphRuntimeHost

Add [DefaultExecutionOrder] attributes to ensure portal results are
fed before the observation lock system ticks. NodeStreamingController
runs at -10, GraphRuntimeHost at 0 (explicit default)."
```

---

### Task 2: Add `BindObservationTracker` tests

`GraphRuntimeHost.BindObservationTracker()` has non-trivial logic: null guard, recreates input source and lock system, resets old lock state. Zero test coverage. Tests must create real lock state before asserting that bind clears it.

**Files:**
- Modify: `unity-DESYNC/Assets/_Project/Tests/EditMode/GraphRuntimeHostTests.cs`

- [ ] **Step 1: Write test — `BindObservationTracker` replaces the lock system instance**

This test proves the lock system is actually recreated (not just that it exists). It captures the pre-bind instance reference and asserts it changes.

Add to `GraphRuntimeHostTests.cs` after the existing tests, before the Helpers region:

```csharp
#region Observation Binding

[Test]
public void BindObservationTracker_ReplacesLockSystemInstance()
{
    var def = BuildMinimalValidDefinition();
    var rules = ScriptableObject.CreateInstance<ObservationRulesDefinition>();
    SetSerializedField(_host, "graphDefinition", def);
    SetSerializedField(_host, "observationRules", rules);
    InvokeAwake(_host);

    var preBind = _host.ObservationLock;
    Assert.IsNotNull(preBind, "Lock system should exist after Awake with rules");

    var trackerGo = new GameObject("Tracker");
    var tracker = trackerGo.AddComponent<PlayerNodeTracker>();

    _host.BindObservationTracker(tracker);

    Assert.IsNotNull(_host.ObservationLock, "Lock system should exist after bind");
    Assert.AreNotSame(preBind, _host.ObservationLock,
        "Bind should create a new lock system instance");

    Object.DestroyImmediate(trackerGo);
    Object.DestroyImmediate(def);
    Object.DestroyImmediate(rules);
}
```

- [ ] **Step 2: Run test to verify it passes**

Run: Unity MCP `run_tests` with `test_names: ["Desync.Tests.EditMode.GraphRuntimeHostTests.BindObservationTracker_ReplacesLockSystemInstance"]`
Expected: PASS

- [ ] **Step 3: Write test — `BindObservationTracker` with null rules does not crash**

```csharp
[Test]
public void BindObservationTracker_NoRules_DoesNotThrow()
{
    var def = BuildMinimalValidDefinition();
    SetSerializedField(_host, "graphDefinition", def);
    LogAssert.Expect(LogType.Warning,
        "[GraphRuntimeHost] No ObservationRulesDefinition assigned — observation lock disabled.");
    InvokeAwake(_host);

    var trackerGo = new GameObject("Tracker");
    var tracker = trackerGo.AddComponent<PlayerNodeTracker>();

    Assert.DoesNotThrow(() => _host.BindObservationTracker(tracker),
        "Should not throw when observation system was not initialized");

    Object.DestroyImmediate(trackerGo);
    Object.DestroyImmediate(def);
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: Unity MCP `run_tests` with `test_names: ["Desync.Tests.EditMode.GraphRuntimeHostTests.BindObservationTracker_NoRules_DoesNotThrow"]`
Expected: PASS

- [ ] **Step 5: Write test — `BindObservationTracker` clears prior lock state**

This test injects a `FakeObservationInputSource` to create real lock state (occupied node), ticks it, verifies state exists, then rebinds and verifies it's gone. Uses `ObservationLockSystem` directly since `GraphRuntimeHost` doesn't expose the concrete type for injection — we verify by constructing a parallel lock system with the same graph, populating it, then checking the host's new instance is clean.

Actually, simpler approach: use `ForceNodeLock` (debug override) to create state on the host's lock system, then rebind and verify it's gone:

```csharp
[Test]
public void BindObservationTracker_ClearsPriorLockState()
{
    var def = BuildMinimalValidDefinition();
    var rules = ScriptableObject.CreateInstance<ObservationRulesDefinition>();
    SetSerializedField(_host, "graphDefinition", def);
    SetSerializedField(_host, "observationRules", rules);
    InvokeAwake(_host);

    // Create real lock state via debug override
    var lockSystem = _host.ObservationLock as ObservationLockSystem;
    Assert.IsNotNull(lockSystem);
    lockSystem.ForceNodeLock("entry");
    Assert.IsTrue(_host.ObservationLock.IsNodeLocked("entry"),
        "Precondition: entry should be locked before rebind");

    var trackerGo = new GameObject("Tracker");
    var tracker = trackerGo.AddComponent<PlayerNodeTracker>();

    _host.BindObservationTracker(tracker);

    Assert.IsFalse(_host.ObservationLock.IsNodeLocked("entry"),
        "Lock state should be cleared after rebind");
    Assert.AreEqual(0, _host.ObservationLock.GetAllNodeStates().Count,
        "All node states should be empty after rebind");

    Object.DestroyImmediate(trackerGo);
    Object.DestroyImmediate(def);
    Object.DestroyImmediate(rules);
}

#endregion
```

- [ ] **Step 6: Run test to verify it passes**

Run: Unity MCP `run_tests` with `test_names: ["Desync.Tests.EditMode.GraphRuntimeHostTests.BindObservationTracker_ClearsPriorLockState"]`
Expected: PASS

- [ ] **Step 7: Commit**

```bash
git add unity-DESYNC/Assets/_Project/Tests/EditMode/GraphRuntimeHostTests.cs
git commit -m "test: add BindObservationTracker coverage for GraphRuntimeHost"
```

---

### Task 3: Add `ResetObservation` tests

**Files:**
- Modify: `unity-DESYNC/Assets/_Project/Tests/EditMode/GraphRuntimeHostTests.cs`

- [ ] **Step 1: Write tests**

Add after the Observation Binding region:

```csharp
#region Observation Reset

[Test]
public void ResetObservation_NoRules_DoesNotThrow()
{
    var def = BuildMinimalValidDefinition();
    SetSerializedField(_host, "graphDefinition", def);
    LogAssert.Expect(LogType.Warning,
        "[GraphRuntimeHost] No ObservationRulesDefinition assigned — observation lock disabled.");
    InvokeAwake(_host);

    Assert.DoesNotThrow(() => _host.ResetObservation(),
        "Should not throw when observation system was not initialized");

    Object.DestroyImmediate(def);
}

[Test]
public void ResetObservation_ClearsLockState()
{
    var def = BuildMinimalValidDefinition();
    var rules = ScriptableObject.CreateInstance<ObservationRulesDefinition>();
    SetSerializedField(_host, "graphDefinition", def);
    SetSerializedField(_host, "observationRules", rules);
    InvokeAwake(_host);

    // Create real lock state via debug override
    var lockSystem = _host.ObservationLock as ObservationLockSystem;
    Assert.IsNotNull(lockSystem);
    lockSystem.ForceNodeLock("entry");
    lockSystem.ForceEdgeLock("entry_to_hall");
    Assert.IsTrue(_host.ObservationLock.IsNodeLocked("entry"),
        "Precondition: node should be locked");
    Assert.IsTrue(_host.ObservationLock.IsEdgeLocked("entry_to_hall"),
        "Precondition: edge should be locked");

    _host.ResetObservation();

    Assert.AreEqual(0, _host.ObservationLock.GetAllNodeStates().Count,
        "All node states should be cleared after ResetObservation");
    Assert.AreEqual(0, _host.ObservationLock.GetAllEdgeStates().Count,
        "All edge states should be cleared after ResetObservation");

    Object.DestroyImmediate(def);
    Object.DestroyImmediate(rules);
}

#endregion
```

- [ ] **Step 2: Run tests to verify they pass**

Run: Unity MCP `run_tests` with `assembly_names: ["Desync.Tests.EditMode"]`
Expected: All pass (186+)

- [ ] **Step 3: Commit**

```bash
git add unity-DESYNC/Assets/_Project/Tests/EditMode/GraphRuntimeHostTests.cs
git commit -m "test: add ResetObservation coverage for GraphRuntimeHost"
```

---

### Task 4: Add `ForceEdgeLock`/`ForceEdgeUnlock` tests

The node variants have 5 tests; edge variants have zero. Mirror the key behaviors.

**Files:**
- Modify: `unity-DESYNC/Assets/_Project/Tests/EditMode/ObservationLockSystemTests.cs`

- [ ] **Step 1: Write edge override tests**

Add to the `#region Debug Override` section in `ObservationLockSystemTests.cs`, after `Reset_ClearsDebugOverrides`:

```csharp
[Test]
public void ForceEdgeLock_LocksEdgeNotAdjacentToOccupied()
{
    var system = new ObservationLockSystem(_input, _graph, _rules);
    system.Tick(0f);

    system.ForceEdgeLock("hall_to_living");

    Assert.IsTrue(system.IsEdgeLocked("hall_to_living"));
    var reasons = system.GetEdgeLockReasons("hall_to_living");
    Assert.IsTrue(reasons.Contains(LockReason.DebugForced));
}

[Test]
public void ForceEdgeUnlock_OverridesAdjacentLock()
{
    var system = new ObservationLockSystem(_input, _graph, _rules);
    _input.OccupiedNodeIds.Add("entry");
    system.Tick(0f);
    Assert.IsTrue(system.IsEdgeLocked("entry_to_hall"));

    system.ForceEdgeUnlock("entry_to_hall");
    system.Tick(0f);

    Assert.IsFalse(system.IsEdgeLocked("entry_to_hall"));
    Assert.IsTrue(system.IsEdgeMutationEligible("entry_to_hall"));
}

[Test]
public void ForceEdgeLock_SurvivesTick()
{
    var system = new ObservationLockSystem(_input, _graph, _rules);

    system.ForceEdgeLock("hall_to_living");
    system.Tick(0f);

    Assert.IsTrue(system.IsEdgeLocked("hall_to_living"));
    Assert.IsTrue(system.GetEdgeLockReasons("hall_to_living").Contains(LockReason.DebugForced));
}
```

- [ ] **Step 2: Run tests to verify they pass**

Run: Unity MCP `run_tests` with `assembly_names: ["Desync.Tests.EditMode"]`
Expected: All pass (189+)

- [ ] **Step 3: Commit**

```bash
git add unity-DESYNC/Assets/_Project/Tests/EditMode/ObservationLockSystemTests.cs
git commit -m "test: add ForceEdgeLock/Unlock coverage"
```

---

### Task 5: FormatReasons duplication — SKIP

The two `FormatReasons` methods format differently for different contexts:
- Overlay: `[Occupied, PortalVisible]` (full enum names, comma-separated, bracketed)
- Gizmos: `OCC+VIS` (abbreviated codes, plus-separated, no brackets)

**Decision: No extraction needed.** Not true duplication.

---

### Task 6: Debug override `#if` guards — DEFERRED

**Codex review identified:** `#if`-guarding methods on `ObservationLockSystem` creates a build-target-dependent public API. The overlay's `GetLockSystem()` cast would cause compile errors in release builds. Guarding only some call sites leaves dangling references.

**Decision: Defer.** For a jam project (ship date 2026-06-12), the risk of debug overrides leaking into a release build is near-zero. The methods are no-ops without intentional calls. Tracked as C5 in `sprint-2-concerns.md` for post-jam hardening.

---

## Self-Review

**Spec coverage:**
- [x] P1 execution order race: Task 1 (`[DefaultExecutionOrder]` attributes)
- [x] BindObservationTracker tests: Task 2 (3 tests with real lock state)
- [x] ResetObservation tests: Task 3 (2 tests with real lock state)
- [x] ForceEdgeLock/Unlock tests: Task 4 (3 tests)
- [x] FormatReasons duplication: Task 5 (deliberately skipped — not true duplication)
- [x] Debug override #if guards: Task 6 (deliberately deferred — jam project, tracked as C5)

**Placeholder scan:** No TBDs, TODOs, or "implement later" found.

**Type consistency:** `ForceNodeLock`/`ForceEdgeLock` used for creating test state in Tasks 2/3 — matches existing implementation. `ObservationLockSystem` cast in tests mirrors overlay pattern.

**Test count after all tasks:** ~189 tests (181 current + 3 bind + 2 reset + 3 edge override).
