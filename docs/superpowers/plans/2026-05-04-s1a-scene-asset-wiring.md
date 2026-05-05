# S1A Session 2: Scene Assets and Runtime Wiring

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Create 5 room prefabs, a House_Prototype scene, and wire the graph runtime so the game loads and runs against the S1A house graph.

**Architecture:** Room prefabs are graybox cubes with RoomNodeAuthoring + PortalAnchorAuthoring components. GraphRuntimeHost (thin MonoBehaviour) initializes SpatialGraphRuntime from the HouseGraphDefinition SO on Awake. GameBootstrap loads House_Prototype instead of House_Graybox.

**Tech Stack:** Unity 6, URP 17.4, C#, Unity MCP (editor orchestration)

**Execution constraint:** Tasks touching Unity Editor via MCP must run sequentially in the main context (single MCP connection). File-only edits can be dispatched to subagents in worktrees.

---

## File Map

### Already created (this session)
- `Assets/_Project/Data/HouseGraphDefinition.asset` — SO with 5 nodes, 4 edges, validated
- `Assets/_Project/Scripts/World/Graph/GraphRuntimeHost.cs` — thin scene host MonoBehaviour

### To create (this plan)
- `Assets/_Project/Prefabs/Rooms/Room_Entry.prefab`
- `Assets/_Project/Prefabs/Rooms/Room_HallA.prefab`
- `Assets/_Project/Prefabs/Rooms/Room_Living.prefab`
- `Assets/_Project/Prefabs/Rooms/Room_Kitchen.prefab`
- `Assets/_Project/Prefabs/Rooms/Room_CorridorB.prefab`
- `Assets/_Project/Scenes/House_Prototype.unity`

### To modify
- `Assets/_Project/Data/HouseGraphDefinition.asset` — wire roomPrefab references
- `Assets/_Project/Scripts/Core/GameBootstrap.cs:9` — change scene name default

## Graph Topology Reference

```
Entry --(e_entry_hall)--> Hall_A --(e_hall_living)--> Living --(e_living_corridor)--> Corridor_B
                            |
                     (e_hall_kitchen)
                            |
                          Kitchen
```

Room positions (world-space, center of 5x5m floor):
| Node | Position | Portal anchors (local) |
|------|----------|----------------------|
| v_entry | (0, 0, 0) | p_entry_hall_a: (+2.5, 0, 0) |
| v_hall_a | (6, 0, 0) | p_hall_entry_a: (-2.5, 0, 0), p_hall_living_a: (0, 0, +2.5), p_hall_kitchen_a: (+2.5, 0, 0) |
| v_living | (6, 0, 6) | p_living_hall_a: (0, 0, -2.5), p_living_corridor_a: (0, 0, +2.5) |
| v_kitchen | (12, 0, 0) | p_kitchen_hall_a: (-2.5, 0, 0) |
| v_corridor_b | (6, 0, 12) | p_corridor_living_a: (0, 0, -2.5) |

Room graybox dimensions: 5m wide x 3m tall x 5m deep. Floor at y=0, walls are thin cubes.

---

### Task 1: Verify GraphRuntimeHost compilation

**Execution:** Main context (Unity MCP)

- [ ] **Step 1: Poll editor state until compilation finishes**

Read `mcpforunity://editor/state`, confirm `is_compiling == false`.

- [ ] **Step 2: Check console for errors**

```
read_console(types=["error"], count=10)
```

Expected: no new errors (2 pre-existing geometry test warnings are OK).

- [ ] **Step 3: Mark compilation verified**

If errors, fix GraphRuntimeHost.cs. If clean, proceed.

---

### Task 2: Create 5 room prefabs via execute_code

**Execution:** Main context (Unity MCP) — single execute_code call creates all 5 prefabs programmatically

Each room prefab structure:
```
Room_[Name]              (empty GO, RoomNodeAuthoring, BoxCollider trigger size 5x3x5 center 0,1.5,0)
  Portal_[anchorId]      (child GO, PortalAnchorAuthoring, BoxCollider trigger size 1x2.5x0.5)
```

- [ ] **Step 1: Create all 5 room prefabs via execute_code**

Use Unity Editor scripting to:
1. Create `Assets/_Project/Prefabs/Rooms/` folder
2. For each room: create root GameObject, add BoxCollider (trigger, size 5x3x5, center 0,1.5,0), add RoomNodeAuthoring
3. For each portal anchor: create child GO at localPosition, add BoxCollider (trigger, size 1x2.5x0.5), add PortalAnchorAuthoring
4. Use `SerializedObject` to set private [SerializeField] fields (nodeId, roomVolume, anchorId, crossingTrigger)
5. Save as prefab via `PrefabUtility.SaveAsPrefabAsset`
6. Destroy temp scene objects

```csharp
// Key pattern for setting serialized private fields:
var so = new UnityEditor.SerializedObject(component);
so.FindProperty("nodeId").stringValue = "v_entry";
so.FindProperty("roomVolume").objectReferenceValue = boxCollider;
so.ApplyModifiedProperties();
```

- [ ] **Step 2: Verify prefabs exist**

```
manage_asset(action="search", search_term="Room_", search_in="Assets/_Project/Prefabs/Rooms")
```

Expected: 5 prefab assets returned.

- [ ] **Step 3: Check console for errors**

```
read_console(types=["error"], count=10)
```

---

### Task 3: Wire prefab references into HouseGraphDefinition SO

**Execution:** Main context (Unity MCP)

- [ ] **Step 1: Load SO and assign roomPrefab fields via execute_code**

```csharp
var def = UnityEditor.AssetDatabase.LoadAssetAtPath<Desync.World.Graph.Definitions.HouseGraphDefinition>("Assets/_Project/Data/HouseGraphDefinition.asset");
var so = new UnityEditor.SerializedObject(def);
var nodesProp = so.FindProperty("nodes");

// Map nodeId -> prefab path
var prefabMap = new Dictionary<string, string> {
    {"v_entry", "Assets/_Project/Prefabs/Rooms/Room_Entry.prefab"},
    {"v_hall_a", "Assets/_Project/Prefabs/Rooms/Room_HallA.prefab"},
    {"v_living", "Assets/_Project/Prefabs/Rooms/Room_Living.prefab"},
    {"v_kitchen", "Assets/_Project/Prefabs/Rooms/Room_Kitchen.prefab"},
    {"v_corridor_b", "Assets/_Project/Prefabs/Rooms/Room_CorridorB.prefab"}
};

for (int i = 0; i < nodesProp.arraySize; i++)
{
    var node = nodesProp.GetArrayElementAtIndex(i);
    var nodeId = node.FindPropertyRelative("nodeId").stringValue;
    if (prefabMap.TryGetValue(nodeId, out var path))
    {
        var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
        node.FindPropertyRelative("roomPrefab").objectReferenceValue = prefab;
    }
}
so.ApplyModifiedProperties();
UnityEditor.EditorUtility.SetDirty(def);
UnityEditor.AssetDatabase.SaveAssets();
```

- [ ] **Step 2: Verify references**

Re-read the SO asset and confirm all 5 roomPrefab fields are non-null.

---

### Task 4: Create House_Prototype scene

**Execution:** Main context (Unity MCP)

- [ ] **Step 1: Create new scene**

```
manage_scene(action="create", scene_name="House_Prototype", save_path="Assets/_Project/Scenes/House_Prototype.unity", template="empty")
```

- [ ] **Step 2: Add Camera and Directional Light**

```
batch_execute(commands=[
    {"tool": "manage_gameobject", "params": {"action": "create", "name": "Main Camera", "tag": "MainCamera", "position": [6, 8, -5], "rotation": [45, 0, 0], "components_to_add": ["Camera", "AudioListener"]}},
    {"tool": "manage_gameobject", "params": {"action": "create", "name": "Directional Light", "position": [0, 10, 0], "rotation": [50, -30, 0], "components_to_add": ["Light"]}}
])
```

- [ ] **Step 3: Instantiate 5 room prefabs at graph-defined positions**

```
batch_execute(commands=[
    {"tool": "manage_gameobject", "params": {"action": "create", "name": "Room_Entry", "prefab_path": "Assets/_Project/Prefabs/Rooms/Room_Entry.prefab", "position": [0, 0, 0]}},
    {"tool": "manage_gameobject", "params": {"action": "create", "name": "Room_HallA", "prefab_path": "Assets/_Project/Prefabs/Rooms/Room_HallA.prefab", "position": [6, 0, 0]}},
    {"tool": "manage_gameobject", "params": {"action": "create", "name": "Room_Living", "prefab_path": "Assets/_Project/Prefabs/Rooms/Room_Living.prefab", "position": [6, 0, 6]}},
    {"tool": "manage_gameobject", "params": {"action": "create", "name": "Room_Kitchen", "prefab_path": "Assets/_Project/Prefabs/Rooms/Room_Kitchen.prefab", "position": [12, 0, 0]}},
    {"tool": "manage_gameobject", "params": {"action": "create", "name": "Room_CorridorB", "prefab_path": "Assets/_Project/Prefabs/Rooms/Room_CorridorB.prefab", "position": [6, 0, 12]}}
])
```

- [ ] **Step 4: Add GraphRuntimeHost GO**

```
manage_gameobject(action="create", name="GraphRuntimeHost", components_to_add=["Desync.World.Graph.GraphRuntimeHost"])
```

Then wire the SO reference via execute_code:
```csharp
var host = GameObject.Find("GraphRuntimeHost").GetComponent<Desync.World.Graph.GraphRuntimeHost>();
var so = new UnityEditor.SerializedObject(host);
var defAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<Desync.World.Graph.Definitions.HouseGraphDefinition>("Assets/_Project/Data/HouseGraphDefinition.asset");
so.FindProperty("graphDefinition").objectReferenceValue = defAsset;
so.ApplyModifiedProperties();
```

- [ ] **Step 5: Save scene**

```
manage_scene(action="save")
```

- [ ] **Step 6: Take screenshot to verify layout**

```
manage_camera(action="screenshot", include_image=True, max_resolution=512)
```

---

### Task 5: Update GameBootstrap and build settings

**Execution:** File edit (subagent-safe for GameBootstrap.cs) + Unity MCP (build settings)

- [ ] **Step 1: Update GameBootstrap.cs default scene name**

File: `unity-DESYNC/Assets/_Project/Scripts/Core/GameBootstrap.cs:9`

Change:
```csharp
[SerializeField] private string gameplaySceneName = "House_Graybox";
```
To:
```csharp
[SerializeField] private string gameplaySceneName = "House_Prototype";
```

- [ ] **Step 2: Add House_Prototype to build settings**

```csharp
// via execute_code
var scenes = new System.Collections.Generic.List<UnityEditor.EditorBuildSettingsScene>(UnityEditor.EditorBuildSettings.scenes);
var protoPath = "Assets/_Project/Scenes/House_Prototype.unity";
if (!scenes.Exists(s => s.path == protoPath))
{
    scenes.Add(new UnityEditor.EditorBuildSettingsScene(protoPath, true));
    UnityEditor.EditorBuildSettings.scenes = scenes.ToArray();
}
return "Build settings updated: " + string.Join(", ", System.Array.ConvertAll(UnityEditor.EditorBuildSettings.scenes, s => s.path));
```

- [ ] **Step 3: Wait for compilation, check console**

---

### Task 6: Verify everything

**Execution:** Main context (Unity MCP + test runner)

- [ ] **Step 1: Run all EditMode tests**

```
run_tests(mode="EditMode")
```

Expected: all tests pass (48 existing + new GraphRuntimeHost tests from background agent).

- [ ] **Step 2: Enter Play mode from Bootstrap scene**

Load Bootstrap scene, enter Play, check console for GraphRuntimeHost initialization log.

- [ ] **Step 3: Screenshot in play mode**

Verify the scene loads, camera shows the room layout.

- [ ] **Step 4: Exit Play mode**

- [ ] **Step 5: Commit**

```bash
git add unity-DESYNC/Assets/_Project/Data/ unity-DESYNC/Assets/_Project/Prefabs/Rooms/ unity-DESYNC/Assets/_Project/Scenes/House_Prototype.unity unity-DESYNC/Assets/_Project/Scripts/World/Graph/GraphRuntimeHost.cs unity-DESYNC/Assets/_Project/Scripts/Core/GameBootstrap.cs unity-DESYNC/Assets/_Project/Tests/EditMode/GraphRuntimeHostTests.cs
git commit -m "feat: S1A scene assets — room prefabs, House_Prototype, GraphRuntimeHost wiring"
```
