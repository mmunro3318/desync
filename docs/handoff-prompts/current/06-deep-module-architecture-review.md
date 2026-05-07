# Deep Module Architecture Review — DESYNC Codebase

**Date:** 2025-05-05  
**Scope:** High-level structural analysis + design guide for future development  
**Status:** Living reference for Mike and AI agents

---

## Executive Summary

The codebase has **one well-isolated deep module** (`World.Graph`) and **five unguarded modules** (Core, Player, Items, Audio, UI) that compile into Unity's default assembly with no enforced boundaries. The deep-module philosophy is highly compatible with Unity/C# — assembly definitions (`.asmdef`) are the natural seam enforcement mechanism. The nascent state of the repo makes this the ideal time to establish boundaries before cross-module coupling calcifies.

---

## Part 1: Current State

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

- **World.Graph is a model deep module.** Clear definition/runtime/authoring split. Pure C# query engine with zero external deps beyond Netcode. Internal state classes hidden behind public query methods. Assembly boundary enforced.
- **ScriptableObject definition vs. runtime state** pattern is clean.
- **Test convention is consistent.** 1:1 class-to-test-file mapping, NUnit, EditMode. 75 tests across 8 files.
- **Namespace hierarchy** (`Desync.*`) is well-chosen and consistent even where asmdef enforcement is absent.

### What Needs Attention

The five unguarded modules have no compile-time boundary enforcement. Without `.asmdef` files, any script can `using Desync.Player;` from anywhere. This is the #1 structural risk as the codebase grows.

---

## Part 2: Target Repo Structure

### Recommended Layout (with asmdef boundaries marked)

```
unity-DESYNC/Assets/_Project/
  Scripts/
    Core/                                    # [asmdef: Desync.Core]
      Desync.Core.asmdef
      GameBootstrap.cs
      GameplaySettings.cs                    # ScriptableObject — pure config
      
    Player/                                  # [asmdef: Desync.Player]
      Desync.Player.asmdef                   # refs: Desync.Core, Unity.InputSystem, Unity.Netcode.Runtime
      PlayerInputRouter.cs
      PlayerLook.cs
      PlayerMotor.cs
      
    Items/                                   # [asmdef: Desync.Items]
      Desync.Items.asmdef                    # refs: Desync.Core, Unity.Netcode.Runtime
      FlashlightController.cs
      
    Audio/                                   # [asmdef: Desync.Audio]
      Desync.Audio.asmdef                    # refs: Desync.Core
      AmbientAudioManager.cs
      FootstepAudio.cs
      
    UI/                                      # [asmdef: Desync.UI]
      Desync.UI.asmdef                       # refs: Desync.Core
      LobbyUI.cs
      
    World/
      Graph/                                 # [asmdef: Desync.World.Graph] ← already exists
        Desync.World.Graph.asmdef
        Definitions/
          HouseGraphDefinition.cs            # ScriptableObject + struct definitions
        Runtime/
          SpatialGraphRuntime.cs             # O(1) query engine
          PortalResolver.cs                  # Pure traversal logic
          PlayerNodeTracker.cs               # Room state machine
          RuntimeNodeState.cs                # Internal mutable state
          RuntimeEdgeState.cs                # Internal mutable state
        Authoring/
          RoomNodeAuthoring.cs               # Scene-to-graph binding
          PortalAnchorAuthoring.cs           # Portal entry/exit points
        GraphRuntimeHost.cs                  # MonoBehaviour host/initializer
        Debug/
          SpatialDebugOverlay.cs
          SpatialDebugGizmos.cs

      Observation/                           # [asmdef: Desync.World.Observation] ← future S2
        Desync.World.Observation.asmdef      # refs: Desync.World.Graph
        IObservationLedgerService.cs         # Interface — public contract
        ...
        
      Mutation/                              # [asmdef: Desync.World.Mutation] ← future S3
        Desync.World.Mutation.asmdef         # refs: Desync.World.Graph, Desync.World.Observation
        IMutationAuthorityService.cs         # Interface — public contract
        ...

  Tests/
    EditMode/                                # [asmdef: Desync.Tests.EditMode]
      Desync.Tests.EditMode.asmdef           # refs: all production asmdefs + NUnit
      Helpers/                               # Shared test utilities (future)
        TestGraphBuilder.cs
      GraphRuntimeHostTests.cs
      HouseGraphDefinitionTests.cs
      HouseGrayboxGeometryTests.cs
      NetworkBootstrapConsistencyTests.cs
      PlayerNodeTrackerTests.cs
      PortalResolverTests.cs
      RuntimeStateTests.cs
      SpatialGraphRuntimeTests.cs
```

### Assembly Dependency Graph

```
Desync.Core  (leaf — no project refs)
    ↑
    ├── Desync.Player       (refs Core)
    ├── Desync.Items        (refs Core)
    ├── Desync.Audio        (refs Core)
    ├── Desync.UI           (refs Core)
    └── Desync.World.Graph  (refs nothing internal — only Unity.Netcode.Runtime)
            ↑
            ├── Desync.World.Observation  (refs Graph)
            └── Desync.World.Mutation     (refs Graph, Observation)

Desync.Tests.EditMode  (refs all production assemblies + test frameworks)
```

**Rule: Dependencies flow toward leaves, never sideways.** Player never imports Items. Audio never imports UI. If two modules need to talk, they share through Core (events, SOs) or through an interface defined at the depended-upon module's boundary.

---

## Part 3: What Makes a Good Deep Module

### The Depth Formula

```
Depth = (Implementation Complexity) / (Interface Complexity)
```

A deep module hides a lot behind a little. A shallow module forces callers to understand nearly as much as the implementation itself.

### Concrete Example: World.Graph (Deep)

**Interface (what callers see):**
```csharp
// 7 public methods. That's the entire surface for 15+ internal classes.
public class SpatialGraphRuntime
{
    public int NodeCount { get; }
    public int EdgeCount { get; }
    public void Initialize(HouseGraphDefinition definition)
    public bool GetNode(string nodeId, out HouseNodeDefinition node)
    public bool GetEdge(string edgeId, out HouseEdgeDefinition edge)
    public IReadOnlyList<HouseEdgeDefinition> GetConnectedEdges(string nodeId)
    public bool GetDestinationNode(string edgeId, string currentNodeId, out string destinationNodeId)
    public bool GetPortalAnchor(string nodeId, string anchorId, out PortalAnchorDefinition anchor)
    public void Reset()
}
```

**Implementation (what's hidden):**
- Dictionary indexing for O(1) lookups
- Edge-to-node resolution logic
- Quaternion sanitization on Initialize()
- Bidirectional edge traversal
- Anchor lookup via nested node→anchor indexing
- State management (RuntimeNodeState, RuntimeEdgeState)

**Why it's deep:** Callers say `GetNode("hallway")` and get a struct. They don't know or care about the dictionary internals, the initialization validation, or the bidirectional edge resolution. 7 methods hide 15+ classes and 500+ lines of logic.

### Counter-Example: A Shallow Module (Hypothetical)

```csharp
// BAD: Interface is nearly as complex as the implementation
public class QuaternionSanitizer
{
    public static Quaternion Sanitize(Quaternion q)
    {
        return q == default ? Quaternion.identity : q;
    }
}
```

This fails the deletion test: if you delete it, the one-liner reappears at the call site. No leverage. No locality. The fix is to keep this logic *inside* the module that owns initialization — not to extract it.

### Decision Criteria: When to Create a New Module

| Signal | Action |
|--------|--------|
| 3+ classes working together on one concern | Probably a module |
| 1 class, 1 function, or 1 utility | Keep it inside the owning module |
| 2+ consumers need the same interface | Real seam — extract the interface |
| 1 consumer needs it | Hypothetical seam — defer extraction |
| "Cross-cutting concern" with <3 touch points | Not a module yet — just shared logic in the owner |
| Test file would have <5 tests | Not enough identity for its own test file |

### The Deletion Test

Before creating any new module, file, or abstraction, ask:

> "If I deleted this, would complexity **concentrate** (good — it was earning its keep) or just **move to callers** with no net reduction (bad — it was a pass-through)?"

---

## Part 4: Interface Design — Patterns and Examples

### Pattern 1: Query Interface (Read-Only Contract)

The most common pattern in DESYNC. Systems need to *read* the graph without knowing how it's stored.

```csharp
// Defined in: World/Graph/ISpatialGraphQuery.cs
// Lives at the module boundary — callers depend on this, not the implementation

public interface ISpatialGraphQuery
{
    int NodeCount { get; }
    int EdgeCount { get; }
    bool GetNode(string nodeId, out HouseNodeDefinition node);
    bool GetEdge(string edgeId, out HouseEdgeDefinition edge);
    IReadOnlyList<HouseEdgeDefinition> GetConnectedEdges(string nodeId);
    bool GetDestinationNode(string edgeId, string currentNodeId, out string destinationNodeId);
    bool GetPortalAnchor(string nodeId, string anchorId, out PortalAnchorDefinition anchor);
}

// Implementation (internal to the module):
internal class SpatialGraphRuntime : ISpatialGraphQuery { ... }
```

**Design rules for query interfaces:**
- Return value types (structs) or `IReadOnlyList<T>` — never expose mutable collections
- Use `bool TryGet(key, out value)` pattern — caller handles missing data, no exceptions
- No side effects — calling a query method never changes state
- Stable ID types (`string nodeId`) — not scene references, not indices

### Pattern 2: Authority Interface (Command Contract)

For systems that mutate state. Only the authority can write; everyone else queries.

```csharp
// Future: World/Mutation/IMutationAuthorityService.cs

public interface IMutationAuthorityService
{
    MutationDecision Evaluate(MutationCandidate candidate);
    bool Commit(MutationDecision approved);
}

public readonly struct MutationCandidate
{
    public readonly string targetNodeId;
    public readonly MutationType type;
    public readonly string requestingPlayerId;
}

public readonly struct MutationDecision
{
    public readonly bool isApproved;
    public readonly string reason;
    public readonly MutationCandidate original;
}
```

**Design rules for authority interfaces:**
- Evaluate and Commit are separate steps (inspect before mutate)
- Return decision structs with reasons (debuggable, traceable)
- Input is a value type (candidate) — no mutable references
- Only one implementation should exist at runtime (the authority)

### Pattern 3: Reporter/Aggregator Split

When multiple sources contribute facts that one system aggregates into truth.

```csharp
// Reporter — each player has one, runs locally
public interface IPlayerObservationReporter
{
    ObservationFact BuildObservationFact();  // What can THIS player see right now?
}

// Aggregator — one per session, runs on server
public interface IObservationLedgerService
{
    bool IsNodeObserved(string nodeId);      // Is ANY player observing this node?
    IReadOnlyList<string> GetObservers(string nodeId);
}
```

**Design rules for reporter/aggregator:**
- Reporters report facts — they don't decide truth
- The aggregator owns truth — reporters contribute, aggregator decides
- Separate interfaces even if one class implements both (keeps consumers' dependency narrow)

### Pattern 4: ScriptableObject as Interface Adapter

Unity-specific. SOs bridge the gap between editor-authored data and runtime interfaces.

```csharp
// Definition SO — the "adapter" between editor and runtime
[CreateAssetMenu(menuName = "Desync/House Graph Definition")]
public class HouseGraphDefinition : ScriptableObject
{
    [SerializeField] private HouseNodeDefinition[] nodes;
    [SerializeField] private HouseEdgeDefinition[] edges;
    
    // Public read-only access (the interface)
    public IReadOnlyList<HouseNodeDefinition> Nodes => nodes;
    public IReadOnlyList<HouseEdgeDefinition> Edges => edges;
    
    // Validation (editor-time contract enforcement)
    public List<string> Validate() { ... }
}
```

**Design rules for SO adapters:**
- `[SerializeField] private` for storage, public read-only properties for access
- Validation method returns a list of error strings (not exceptions)
- SOs are definitions — they never hold runtime state
- One SO type per logical domain (not one SO per knob)

---

## Part 5: Workflows for Deep Module Development

### Workflow A: Adding a Feature to an Existing Module

1. **Read the interface** — what does the module already expose?
2. **Does the feature require a new public method?** If yes, design the signature first. If no, it's purely internal — implement freely.
3. **Write the test against the interface** — the test calls the public method with the new scenario.
4. **Implement internally** — change whatever internals you need. Only the test and the interface matter.
5. **Check: did the interface grow?** If you added >2 public members, ask whether the module is trying to do two things.

**Example (TD0014 quaternion fix):**
- Interface: `Initialize(definition)` already exists. No new public method needed.
- Test: Call `Initialize()` with a zero-quaternion anchor → verify it's sanitized to `Quaternion.identity`.
- Implementation: Add the guard clause inside `Initialize()`.
- Interface didn't grow. Done.

### Workflow B: Creating a New Module

1. **Identify the seam** — where are 2+ consumers depending on the same behavior?
2. **Name it** — use domain vocabulary (from CONTEXT.md / GDD). If you can't name it cleanly, it might not be a real module.
3. **Design the interface FIRST** — write the `interface` or public class signature with method names, parameter types, and return types. No implementation.
4. **Write the asmdef** — declare what this module depends on. If it needs >3 other modules, it's too broad.
5. **Write tests against the interface** — these define the contract. Implementation comes last.
6. **Implement** — fill in the internals. Tests pass = you're done.

**Checklist for a new module:**
```
□ Has a clear name from the domain language
□ Has an .asmdef with explicit dependency declarations
□ Has a public interface (C# interface or public class with small surface)
□ Has tests that exercise the interface, not internals
□ Passes the deletion test (removing it would increase caller complexity)
□ Has ≥3 internal classes or ≥50 LoC of implementation
□ Has ≤2 dependency edges to other project modules
```

### Workflow C: Consuming a Module (Depending on Another Module)

1. **Depend on the interface, not the implementation.** If the module exposes `ISpatialGraphQuery`, take that — not `SpatialGraphRuntime`.
2. **Declare the dependency in your asmdef.** If the compiler doesn't let you import it, you have an undeclared dependency.
3. **Never reach into a module's subfolder.** Import from the top-level namespace only.
4. **For testing, mock the interface.** Don't construct the real module just to test yours.

```csharp
// GOOD: depends on the interface
public class PlayerNodeTracker
{
    private readonly ISpatialGraphQuery _graph;
    public PlayerNodeTracker(ISpatialGraphQuery graph) { _graph = graph; }
}

// BAD: depends on the concrete implementation
public class PlayerNodeTracker
{
    private readonly SpatialGraphRuntime _runtime;  // coupled to internals
}
```

### Workflow D: Reviewing/Auditing Module Health

Run this checklist periodically (sprint boundaries, pre-PR):

| Check | Healthy | Unhealthy |
|-------|---------|-----------|
| Public surface | ≤10 methods/properties | 15+ methods, growing every sprint |
| Dependencies declared | All in asmdef | Using `Assembly-CSharp` default |
| Internal classes | `internal` or nested | All `public` "just in case" |
| Tests | Exercise public API | Test private methods via reflection |
| Cross-module coupling | Through interfaces/SOs | Direct class references |
| Singleton usage | 0-1 per module | Static Instance accessed from 3+ modules |

---

## Part 6: File Schema and Naming Conventions

### asmdef File Schema

```json
{
    "name": "Desync.{ModulePath}",
    "rootNamespace": "Desync.{ModulePath}",
    "references": [
        "Desync.Core",
        "Unity.Netcode.Runtime"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

**Naming rule:** `Desync.{Folder.Path}` — e.g., `Desync.World.Graph`, `Desync.World.Observation`, `Desync.Player`.

### Interface File Naming

```
I{Noun}{Verb}Service.cs    — for authority/command interfaces
I{Noun}Query.cs            — for read-only query interfaces
I{Noun}Reporter.cs         — for fact-contribution interfaces
```

Examples: `ISpatialGraphQuery.cs`, `IMutationAuthorityService.cs`, `IPlayerObservationReporter.cs`

### Struct/Data Definition Naming

```
{Domain}{Role}Definition    — editor-authored, serialized (SO fields)
{Domain}{Role}State         — runtime mutable state (internal)
{Domain}{Role}Snapshot      — network-serialized point-in-time capture
{Domain}{Role}Decision      — result of an authority evaluation
{Domain}{Role}Fact          — reporter contribution (pre-aggregation)
```

Examples: `HouseNodeDefinition`, `RuntimeNodeState`, `NetworkHouseSnapshot`, `MutationDecision`, `ObservationFact`

### Test File Naming

```
{ClassUnderTest}Tests.cs    — always mirrors the production class 1:1
```

**Threshold for a new test file:** The class under test exists as a public class. Helper classes, utilities, and internal behaviors get tested through the owning class's test file.

---

## Part 7: Deepening Opportunities (Actionable)

### Candidate 1: Assembly Boundary Enforcement (Priority: High)

**Problem:** Five modules share the default assembly. Cross-module imports are invisible.  
**Solution:** Add `.asmdef` files for each module.  
**Effort:** Low (~10 min each). Risk: discovering undeclared dependencies.  
**Sequencing:** Start with leaves (Audio, Items, UI), then Player, then Core.

### Candidate 2: Extract Shared Test Utilities (Priority: Low-Medium)

**Problem:** Test setup patterns duplicated across 8 files.  
**Solution:** `TestGraphBuilder` fluent helper in `Tests/EditMode/Helpers/`.  
**Threshold:** Extract when test count passes ~100 or when a third test file needs the same 5-node graph setup.

### Candidate 3: ISpatialGraphQuery Interface Extraction (Priority: Medium)

**Problem:** Consumers bind to `SpatialGraphRuntime` concrete class.  
**Solution:** Extract interface from existing public methods. Apply when the observation system (S2) needs to depend on graph queries.  
**Why not now:** One adapter = hypothetical seam. Wait for the second consumer.

### Candidate 4: GameBootstrap Decomposition (Priority: Low)

**Trigger:** When Relay/Lobby integration lands and bootstrap responsibilities double.  
**Solution:** Split into `NetworkConfig` + `SessionOrchestrator`.

### Candidate 5: Singleton Audit (Priority: Low)

**Trigger:** When a third singleton appears or when testing requires decoupling.

---

## Part 8: Recommended Sequencing

1. **Now (S1B):** TD0014 → Option A. No structural changes.
2. **S2 prep:** Candidate 1 (asmdef boundaries) — prevent coupling before new systems land.
3. **S2 implementation:** Candidate 3 (ISpatialGraphQuery) — extract when observation system arrives.
4. **Ongoing:** Candidate 2 (test helpers) — extract incrementally as test count grows.
5. **Deferred:** Candidates 4 & 5 — revisit when complexity warrants.

---

## Part 9: Quick Reference — Deep Module Principles for Unity/C#

| Principle | Unity/C# Mechanism |
|-----------|-------------------|
| Module boundary | `.asmdef` file |
| Interface contract | C# `interface` type |
| Internal hiding | `internal` access modifier (asmdef-scoped) |
| Definition vs runtime | ScriptableObject (definition) vs MonoBehaviour/class (runtime) |
| Test surface | Public interface methods only |
| Dependency declaration | `asmdef` references array |
| Cross-module communication | Interfaces, SO events, C# events — never direct class refs across asmdef boundaries |
| Progressive disclosure | Folder structure mirrors module depth (scan folders → read interfaces → read internals) |

### The Three Questions (Before Any New File/Class)

1. **Does this earn its existence?** (Deletion test — would removing it increase complexity elsewhere?)
2. **Who owns it?** (Which module's interface does this behavior belong to?)
3. **How will it be tested?** (Through what public interface will a test exercise this code?)

If you can't answer all three clearly, the code belongs inside an existing module, not in a new file.

---

## TD0014 Verdict

**Option A. Tests grouped by class under test.**

The test surface mirrors the interface surface. `SpatialGraphRuntime` owns `Initialize()` sanitization → test in `SpatialGraphRuntimeTests.cs`. `HouseGraphDefinition` owns `Validate()` warnings → test in `HouseGraphDefinitionTests.cs`. Quaternion handling has no interface of its own → no test file of its own.
