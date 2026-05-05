# Deep Module Architecture Review — DESYNC Codebase

**Date:** 2025-05-05  
**Scope:** High-level structural analysis, no code edits  
**Status:** Research report for Mike's review

---

## Executive Summary

The codebase has **one well-isolated deep module** (`World.Graph`) and **five unguarded modules** (Core, Player, Items, Audio, UI) that compile into Unity's default assembly with no enforced boundaries. The deep-module philosophy is highly compatible with Unity/C# — assembly definitions (`.asmdef`) are the natural seam enforcement mechanism. The nascent state of the repo makes this the ideal time to establish boundaries before cross-module coupling calcifies.

---

## Current State

### Module Map (6 modules, 1 boundary)

| Module | Folder | Namespace | asmdef | Depth |
|--------|--------|-----------|--------|-------|
| **World.Graph** | `Scripts/World/Graph/` | `Desync.World.Graph.*` | `Desync.World.Graph.asmdef` | **Deep** — 15+ classes behind a small query surface |
| Core | `Scripts/Core/` | `Desync.Core` | *none* | Shallow — 2 classes, both public |
| Player | `Scripts/Player/` | `Desync.Player` | *none* | Medium — 3 classes, event-coupled |
| Items | `Scripts/Items/` | `Desync.Items` | *none* | Shallow — 1 class |
| Audio | `Scripts/Audio/` | `Desync.Audio` | *none* | Shallow — 2 classes |
| UI | `Scripts/UI/` | `Desync.UI` | *none* | Shallow — 1 class |

### What's Working Well

- **World.Graph is a model deep module.** Clear definition/runtime/authoring split. Pure C# query engine (`SpatialGraphRuntime`, `PortalResolver`) with zero external deps beyond Netcode. Internal state classes (`RuntimeNodeState`, `RuntimeEdgeState`) hidden behind public query methods. Assembly boundary enforced.
- **ScriptableObject definition vs. runtime state** pattern is clean (`HouseGraphDefinition` vs `SpatialGraphRuntime`; `GameplaySettings` as pure config SO).
- **Test convention is consistent.** 1:1 class-to-test-file mapping, `{ClassName}Tests.cs`, NUnit, EditMode. 73 tests across 8 files. (This is why Option A is correct for TD0014.)
- **Namespace hierarchy** (`Desync.*`) is well-chosen and consistent even where asmdef enforcement is absent.

### What Needs Attention

**The five unguarded modules have no compile-time boundary enforcement.** Without `.asmdef` files, any script can `using Desync.Player;` from anywhere. This is the #1 structural risk as the codebase grows.

---

## Deepening Opportunities

### Candidate 1: Assembly Boundary Enforcement (Priority: High)

**Files:** All modules except World.Graph  
**Problem:** Five modules share the default assembly. Cross-module imports are invisible to the compiler. A `using Desync.Audio;` inside a Player script compiles silently — no seam, no adapter, no locality. As new systems land (observation, mutation, portals, entity AI), undeclared dependencies will accumulate.  
**Solution:** Add `.asmdef` files for each module, declaring explicit dependency edges. Start with the leaf modules (Audio, Items, UI — fewest inbound deps) and work inward.  
**Benefits:**  
- *Locality*: compile errors immediately surface cross-module violations  
- *Leverage*: each module's public surface becomes the only way in, enabling independent testing  
- *AI navigability*: agents can scope context to one assembly  

**Effort:** Low per module (~10 min each). Risk: discovering undeclared dependencies that need resolution.

### Candidate 2: Extract Shared Test Utilities (Priority: Low-Medium)

**Files:** All 8 test files under `Tests/EditMode/`  
**Problem:** Test setup patterns are duplicated — `ScriptableObject.CreateInstance<HouseGraphDefinition>()`, graph builder helpers, `GameObject.AddComponent<>()` teardown. Each test file reinvents the same 5-node test graph. No shared fixtures.  
**Solution:** Extract a `TestGraphBuilder` helper (fluent API) and shared setup utilities into a `Tests/EditMode/Helpers/` folder. Not a separate asmdef — just co-located utilities.  
**Benefits:**  
- *Locality*: test graph construction logic lives in one place; changes to definition structs propagate through one builder, not 8 files  
- *Leverage*: new test files get a consistent, valid graph in one line  
- Reduces test LoC by ~30% across the suite  

**Threshold check:** With 8 test files and growing, this is past the "two adapters = real seam" threshold for test helpers.

### Candidate 3: Interface Extraction for Query Contracts (Priority: Medium, Timing: Pre-S2)

**Files:** `SpatialGraphRuntime.cs`, `PortalResolver.cs`, future observation/mutation systems  
**Problem:** The design docs define four core interfaces (`ISpatialGraphQuery`, `INodeActivationQuery`, `IObservationLockQuery`, `IGraphMutationService`) but none are implemented as C# `interface` types yet. `SpatialGraphRuntime` exposes concrete public methods. Consumers bind to the concrete class, not an interface — meaning mock/stub testing of downstream systems will require the real runtime.  
**Solution:** Extract `ISpatialGraphQuery` from `SpatialGraphRuntime`'s existing public methods. Defer the other three interfaces until their systems exist (one adapter = hypothetical seam).  
**Benefits:**  
- *Leverage*: downstream systems (PlayerNodeTracker, future ObservationLockSystem) depend on the interface, not the 700-line runtime  
- *Locality*: runtime internals (dictionary indexing, edge resolution) become implementation details that can change freely  
- *Testability*: mock graph queries in downstream tests without constructing real definitions  

**Why only ISpatialGraphQuery now:** It has two real adapters today (SpatialGraphRuntime + test stubs). The other three have zero implementations — extracting them now is speculative.

### Candidate 4: GameBootstrap Decomposition (Priority: Low, Timing: When Relay/Lobby Lands)

**Files:** `Scripts/Core/GameBootstrap.cs`  
**Problem:** GameBootstrap is the thinnest "manager" in the codebase, but it already handles: NetworkManager configuration, connection approval, transport binding, scene loading orchestration. As Relay/Lobby/NAT traversal land, it will grow into a god class.  
**Solution:** Not yet. Flag for decomposition when a second networking concern (Relay integration) arrives. At that point, split into `NetworkConfig` (transport/approval) and `SessionOrchestrator` (scene loading/flow).  
**Benefits (future):** Prevents the bootstrap from becoming a shallow pass-through for every networking feature.  

**Why not now:** One adapter. The deletion test says: if you deleted GameBootstrap, all its complexity reappears in one place (the scene). It's earning its keep as-is.

### Candidate 5: Singleton Pattern Audit (Priority: Low)

**Files:** `GameBootstrap.cs`, `AmbientAudioManager.cs`  
**Problem:** Two classes use `static Instance` pattern. This is the standard Unity singleton, but it creates a hidden global seam — any code anywhere can call `GameBootstrap.Instance` without declaring a dependency. With asmdef boundaries (Candidate 1), this becomes more visible, but the coupling is still implicit.  
**Solution:** Defer. Singletons are pragmatic for bootstrap/audio in a small project. Revisit when a third singleton appears or when testing requires decoupling.  
**Benefits (future):** Explicit dependency injection would make the dependency graph visible and testable.

---

## Compatibility: Deep Modules + Unity

**Verdict: Highly compatible.** Unity's `.asmdef` system is literally a module boundary mechanism. C# interfaces + assembly definitions give you compile-time seam enforcement. The main friction points:

1. **MonoBehaviour composition** — Unity's component model encourages many small scripts on GameObjects. This is fine *within* a module but dangerous *across* modules. Asmdef boundaries prevent the worst case.
2. **Inspector serialization** — `[SerializeField]` fields are part of the interface (they're visible in the editor). Deep modules should minimize inspector-exposed state on cross-module boundaries.
3. **ScriptableObject channels** — SO assets are a natural adapter pattern (one SO type, many consumers). The project already uses this well with `GameplaySettings` and `HouseGraphDefinition`.

---

## Recommended Sequencing

1. **Now (S1B pre-flight):** Resolve TD0014 with Option A. No structural changes needed.
2. **S2 prep:** Candidate 1 (asmdef boundaries) — low effort, high leverage, prevents coupling before new systems land.
3. **S2 implementation:** Candidate 3 (ISpatialGraphQuery interface) — extract as observation system needs it.
4. **Ongoing:** Candidate 2 (test helpers) — extract incrementally as test count grows past ~100.
5. **Deferred:** Candidates 4 & 5 — revisit when complexity warrants.

---

## TD0014 Verdict (Restated)

**Option A. Tests grouped by class under test.**

Deep-module reasoning: the test surface mirrors the interface surface. `SpatialGraphRuntime` owns `Initialize()` sanitization → test lives in `SpatialGraphRuntimeTests.cs`. `HouseGraphDefinition` owns `Validate()` warnings → test lives in `HouseGraphDefinitionTests.cs`. Quaternion handling is not a module — it has no interface — so it gets no test file.

The threshold for a dedicated `QuaternionSanitizationTests.cs` would be: a `SpatialMath` module with its own interface and at least two consumers. Two guard clauses don't meet that bar.
