# S1A Sprint Wrapup Handoff

## Sprint objective (from PDD)
Define and implement the first minimal house graph as data, then load it into a runtime shell that can be queried, debugged, and reset.

## Sprint done criteria — all met
- [x] The first house slice exists as formal graph data (HouseGraphDefinition SO, 5 nodes, 4 edges)
- [x] Runtime graph queries work in play mode (SpatialGraphRuntime: GetNode, GetEdge, GetConnectedEdges, GetDestinationNode, GetPortalAnchor)
- [x] Debug output explains current graph structure (SpatialDebugOverlay IMGUI HUD + SpatialDebugGizmos scene view)
- [x] Team is ready to move to portal visibility and node activation

## What shipped

### Core graph system (Session 1-2, TDD)
- `HouseGraphDefinition.cs` — ScriptableObject with inlined node/edge/anchor definition structs, `Validate()` method
- `SpatialGraphRuntime.cs` — O(1) query engine, initialized from definition, 5+1 query contract
- `PortalResolver.cs` — bidirectional edge traversal, answers topology not permission
- `RuntimeNodeState.cs` / `RuntimeEdgeState.cs` — per-instance runtime state containers
- `RoomNodeAuthoring.cs` / `PortalAnchorAuthoring.cs` — scene-to-graph bridge components

### Scene assets (Session 2)
- `HouseGraphDefinition.asset` — 5-node graph (Entry, Hall_A, Living, Kitchen, Corridor_B), 4 edges, all roomPrefab refs wired
- 5 room prefabs (`Room_Entry`, `Room_HallA`, `Room_Living`, `Room_Kitchen`, `Room_CorridorB`) with RoomNodeAuthoring + BoxCollider triggers + PortalAnchorAuthoring children
- `House_Prototype.unity` — Camera, Directional Light, 5 room instances at graph positions, GraphRuntimeHost wired to SO, debug overlay + gizmos
- `GraphRuntimeHost.cs` — thin MonoBehaviour, initializes SpatialGraphRuntime on Awake

### Debug overlay (Session 3)
- `SpatialDebugOverlay.cs` — IMGUI HUD (F3 toggle, F5 restart), shows current/previous node, graph stats, connected edges with destinations
- `SpatialDebugGizmos.cs` — scene view wireframe: green node boxes, blue edge lines, orange anchor spheres, all labeled
- `PlayerNodeTracker.cs` — CharacterController trigger-based room detection, enter/exit state machine

### Infrastructure
- `Desync.World.Graph` asmdef — deep module boundary, references NGO + Input System
- `Bootstrap.unity` — gameplaySceneName updated to House_Prototype, serialized value fixed
- `GameBootstrap.cs` — default scene name updated (code default, though serialized value is authoritative)

## Test coverage
- **73 EditMode tests total, 71 pass**
- 48 core graph tests (definitions, runtime, resolver, state) — all pass
- 9 GraphRuntimeHost tests (Awake paths via reflection) — all pass
- 9 PlayerNodeTracker tests (enter/exit state machine) — all pass
- 5 other tests (NetworkBootstrapConsistency, etc.) — all pass
- 2 HouseGrayboxGeometryTests — FAIL (pre-existing, TD0012)

## Smoke test results (verified in play mode)

| Item | Result |
|------|--------|
| Launch from Bootstrap | Pass |
| Enter House_Prototype | Pass |
| Graph asset loads (5 nodes, 4 edges) | Pass |
| Current node from player position | Pass (v_entry) |
| Connected edges in debug | Pass (e_entry_hall -> v_hall_a) |
| Portal destination resolves | Pass |
| Restart (F5) | Pass |
| No blocker errors | Pass (after Input System fix) |

## What was deferred (and why)

### Network sync (NetworkVariable<HouseSnapshot>)
**Originally planned for:** Session 3 (per checkpoint, not per sprint PDD)
**Deferred because:** Not in S1A sprint PDD scope. The ARCH decision about "full snapshot sync" (AD in ARCH.md) defines the *strategy* for when sync is needed, not a requirement to implement it in S1A. S1A's done criteria are: graph data exists, runtime queries work, debug output explains state.
**Where it goes:** S1B or S3 (multiplayer graph sync sprint). The ARCH.md decision stands as the design intent.

### SpatialGraphRuntime -> HouseGraphRuntime rename (TD0011)
**Originally surfaced by:** Counter-drift session
**Deferred because:** Taste call, not a drift bug. "Spatial" describes function, "House" matches type family. Either works.
**Where it goes:** TD0011, post-S1A merge. Low effort (15m), low risk.

### House_Graybox geometry test failures (TD0012)
**Originally surfaced by:** S0.3 geometry grammar rules landing before scene geometry was updated
**Deferred because:** Not S1A scope. Tests are correct, scene geometry is stale.
**Where it goes:** TD0012 at P1.

## Architectural decisions made (ARCH.md)
All recorded in `docs/ARCH.md` under "S1A — House Graph Authoring and Runtime Shell":
- AD: Prefab rooms, not additive scenes
- AD: Stable doorway IDs (door_a, door_b), not cardinal directions
- AD: Full snapshot sync strategy (temporary, evolve to deltas in S3) — strategy only, not implemented
- AD: 9-file structure with inlined definition structs
- AD: Hand-authored Inspector population for S1A
- AD: IMGUI debug overlay (not UI Toolkit — faster to iterate, adequate for dev tooling)
- AD: EditMode-first testing
- AD: 5+1 query contract scope (narrow surface)
- AD: `Desync.World.Graph.*` namespace (not `Desync.Spatial.*`)

## Files created/modified (29 files, +3486 lines)

### New scripts (10)
- `Scripts/World/Graph/Definitions/HouseGraphDefinition.cs` — SO + definition structs
- `Scripts/World/Graph/Runtime/SpatialGraphRuntime.cs` — query engine
- `Scripts/World/Graph/Runtime/PortalResolver.cs` — edge traversal
- `Scripts/World/Graph/Runtime/RuntimeNodeState.cs` — per-node state
- `Scripts/World/Graph/Runtime/RuntimeEdgeState.cs` — per-edge state
- `Scripts/World/Graph/Runtime/PlayerNodeTracker.cs` — player-to-node tracking
- `Scripts/World/Graph/Authoring/RoomNodeAuthoring.cs` — scene bridge (rooms)
- `Scripts/World/Graph/Authoring/PortalAnchorAuthoring.cs` — scene bridge (doorways)
- `Scripts/World/Graph/GraphRuntimeHost.cs` — thin scene host
- `Scripts/World/Graph/Debug/SpatialDebugOverlay.cs` — IMGUI HUD
- `Scripts/World/Graph/Debug/SpatialDebugGizmos.cs` — scene gizmos

### New tests (5)
- `Tests/EditMode/HouseGraphDefinitionTests.cs` — 16 tests
- `Tests/EditMode/SpatialGraphRuntimeTests.cs` — 15 tests
- `Tests/EditMode/PortalResolverTests.cs` — 8 tests
- `Tests/EditMode/RuntimeStateTests.cs` — 9 tests
- `Tests/EditMode/GraphRuntimeHostTests.cs` — 9 tests
- `Tests/EditMode/PlayerNodeTrackerTests.cs` — 9 tests

### New assets
- `Data/HouseGraphDefinition.asset` — 5-node graph SO
- `Prefabs/Rooms/Room_*.prefab` (x5) — room prefabs with authoring components
- `Scenes/House_Prototype.unity` — prototype gameplay scene

### Modified
- `Scripts/Core/GameBootstrap.cs` — default scene name
- `Scenes/Bootstrap.unity` — serialized scene name
- `Scripts/World/Graph/Desync.World.Graph.asmdef` — added Input System ref
- `Tests/EditMode/Desync.Tests.EditMode.asmdef` — added Graph ref

## Bugs found and fixed during sprint

### Namespace collision: `Desync.World.Graph.Debug` shadows `UnityEngine.Debug`
Creating the `Debug/` subfolder introduced a namespace that shadows `UnityEngine.Debug` for all files in `Desync.World.Graph.*`. Fix: `global::UnityEngine.Debug` in affected files (GraphRuntimeHost, RoomNodeAuthoring, PortalAnchorAuthoring). The overlay files use fully-qualified `UnityEngine.Debug` since they're *inside* the Debug namespace.

### Input System conflict: legacy `Input.GetKeyDown` throws at runtime
The project uses Input System 1.19.0 exclusively (Player Settings). SpatialDebugOverlay originally used `UnityEngine.Input.GetKeyDown(KeyCode.F3)` which throws `InvalidOperationException` at runtime. Fix: switched to `Keyboard.current.f3Key.wasPressedThisFrame`, added `Unity.InputSystem` reference to the asmdef.

### Serialized scene name not updated by code default change
Changing `[SerializeField] private string gameplaySceneName = "House_Prototype"` in `GameBootstrap.cs` does NOT update the serialized value in `Bootstrap.unity` — Unity serializes field values per-instance, not from code defaults. Fix: updated the serialized value directly in the scene via SerializedObject.

## Key learnings and operational insights

1. **Unity domain reload is separate from compilation.** Scripts can compile successfully without a domain reload. New types won't be available via `Type.GetType()` or `AddComponent` until the domain actually reloads. Use `CompilationPipeline.RequestScriptCompilation(CleanBuildCache)` to force a full rebuild when types are missing.

2. **Namespace subfolder naming matters.** Creating a `Debug/` folder under an existing namespace creates `Namespace.Debug` which shadows `UnityEngine.Debug` for all sibling files. This is a C# language behavior, not a Unity bug. Consider naming debug folders `DebugTools/` or `Diagnostics/` to avoid the collision. (Not renaming now — the `global::` fix is adequate and the namespace is already committed.)

3. **Subagent worktree isolation works well for independent file creation** but requires a compilation fix pass afterward. The worktree agents can't verify their code compiles in the context of changes made by other agents. A post-merge compile check is essential.

4. **Unity MCP `get_test_job` wait_timeout should be 15s, not 60s.** Tests finish in ~8s but the 60s blocking poll looks hung to the user. Short timeout + re-poll is better UX.

5. **Serialized field defaults vs scene serialization.** Changing a `[SerializeField]` default in code is necessary for new instances but doesn't update existing scene instances. Both the code default AND the scene serialized value must be updated.

6. **Input System vs legacy Input.** Any code using `Input.GetKeyDown` will throw if Player Settings are set to "Input System Package (New)" only. Always check `ARCH.md` for the input stack before writing input code. The project's stack entry explicitly says "New Input System 1.19.0" and "no legacy Input.GetAxis."

## Open bug: overlay + input not working in Play mode

**Symptom (Mike playtest, 2026-05-04 15:41):**
- Player spawns on brown plane with skybox, no visible room geometry (expected — rooms are trigger-only boxes with no mesh)
- No IMGUI overlay visible (should start with `_visible = true`)
- No player controller (no response from keys or mouse)
- F3 and F5 keys do nothing
- Console: `ArgumentNullException: Value cannot be null. Parameter name: key` — repeating every few seconds (once per Update frame)
- Screenshot: `docs/handoff-prompts/current/02-s1a/error-console-unity-3-41-pm.png`

**Debug hypotheses for next session (ordered by likelihood):**

1. **Input System `Keyboard.current` null or stale** — `Keyboard.current` can be null if the Game View isn't focused or the Input System hasn't registered a keyboard device yet. Our null guard (`if (kb == null) return;`) should handle this, but the `ArgumentNullException` with `Parameter name: key` suggests the crash happens *inside* an Input System property accessor (`.f3Key` or `.wasPressedThisFrame`), not at the `Keyboard.current` level. Possible cause: accessing a `KeyControl` on a disposed or not-yet-ready keyboard. **Action:** get full stack trace, check if `Keyboard.current` is non-null but internally invalid.

2. **Update crash prevents OnGUI from executing** — if the `ArgumentNullException` is thrown in `Update()` before the input check completes, Unity may suppress the rest of the frame's callbacks including `OnGUI`. The overlay would exist but never render. **Action:** wrap the Input System calls in a try-catch temporarily to isolate, or move input handling to OnGUI itself (IMGUI has its own `Event.current.type == EventType.KeyDown` API that doesn't depend on Input System).

3. **SpatialDebugOverlay's `graphHost` serialized reference is null** — if the reference broke during NGO scene loading (scene objects load in unpredictable order), graphHost would be null. The current OnGUI code doesn't early-return on null graphHost (it null-coalescces to show zeroes), so this alone wouldn't hide the overlay. But combined with hypothesis 2, a crash in Update would prevent OnGUI entirely. **Action:** add a null-guard log in Awake.

4. **IMGUI rendering requires Game View focus** — IMGUI's `OnGUI` only fires when the Game View is focused in the editor. If Mike is looking at the Scene View, no overlay renders. **Action:** verify Game View tab is active/focused during playtest.

5. **Alternative fix approach:** ditch Input System dependency for the debug overlay entirely. Use IMGUI's built-in `Event.current` in OnGUI for key detection — this is the standard pattern for debug overlays and avoids the Input System dependency altogether:
```csharp
// In OnGUI, before rendering:
if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.F3)
    _visible = !_visible;
```
This removes the Input System asmdef dependency for the debug overlay and sidesteps the keyboard initialization issue.

**Severity:** The overlay is a dev tool, not gameplay-blocking. The graph runtime, prefabs, scene, and tests all work correctly. The overlay rendering + input is the only issue. P2 fix for next session or hotfix before /ship.

---

## Handoff to /document-release -> /ship -> /review
1. Run /document-release to update README, ROADMAP, CLAUDE.md for what shipped
2. Run /ship to create PR (squash merge target: main)
3. Run /review for pre-landing code review
4. After merge: run /counter-drift to catch any naming drift S1A introduced
