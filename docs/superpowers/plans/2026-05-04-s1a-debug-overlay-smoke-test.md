# S1A Session 3: Debug Overlay + Smoke Test

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add debug overlay (IMGUI HUD + scene gizmos) and player-to-node tracking so the S1A graph can be verified in play mode, completing the sprint's acceptance criteria.

**Architecture:** PlayerNodeTracker (MonoBehaviour on PF_Player) uses CharacterController's OnTriggerEnter/Exit with room volume triggers to track current node. SpatialDebugOverlay (IMGUI) reads from GraphRuntimeHost + PlayerNodeTracker. SpatialDebugGizmos draws node/edge/anchor visualization in scene view. All new code lives in the Desync.World.Graph asmdef.

**Tech Stack:** Unity 6, C#, IMGUI (OnGUI), Gizmos (OnDrawGizmos), CharacterController trigger detection

**TDD scope:** PlayerNodeTracker state logic is testable in EditMode (pure Enter/Exit state management). IMGUI and Gizmos rendering are verified visually via screenshot. The sprint smoke test is a manual Play mode verification.

---

## File Map

### To create
- `Assets/_Project/Scripts/World/Graph/Runtime/PlayerNodeTracker.cs` — tracks which graph node the player occupies
- `Assets/_Project/Scripts/World/Graph/Debug/SpatialDebugOverlay.cs` — IMGUI HUD (F3 toggle)
- `Assets/_Project/Scripts/World/Graph/Debug/SpatialDebugGizmos.cs` — scene view node/edge/anchor drawing
- `Assets/_Project/Tests/EditMode/PlayerNodeTrackerTests.cs` — EditMode tests for state logic

### To modify
- PF_Player prefab — add PlayerNodeTracker component
- House_Prototype scene — add SpatialDebugOverlay + SpatialDebugGizmos GameObjects

---

### Task 1: PlayerNodeTracker — TDD

**Files:**
- Create: `unity-DESYNC/Assets/_Project/Scripts/World/Graph/Runtime/PlayerNodeTracker.cs`
- Test: `unity-DESYNC/Assets/_Project/Tests/EditMode/PlayerNodeTrackerTests.cs`

**Why TDD:** The enter/exit state machine has edge cases (rapid room transitions, overlapping volumes, exit-without-enter). Pure state methods are fully testable in EditMode.

- [ ] **Step 1: Write failing tests**

```csharp
using NUnit.Framework;
using UnityEngine;
using Desync.World.Graph.Runtime;

namespace Desync.Tests.EditMode
{
    public class PlayerNodeTrackerTests
    {
        private GameObject _go;
        private PlayerNodeTracker _tracker;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("Tracker_Test");
            _tracker = _go.AddComponent<PlayerNodeTracker>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void CurrentNodeId_InitiallyNull()
        {
            Assert.IsNull(_tracker.CurrentNodeId);
        }

        [Test]
        public void PreviousNodeId_InitiallyNull()
        {
            Assert.IsNull(_tracker.PreviousNodeId);
        }

        [Test]
        public void EnterNode_SetsCurrentNodeId()
        {
            _tracker.EnterNode("v_entry");
            Assert.AreEqual("v_entry", _tracker.CurrentNodeId);
        }

        [Test]
        public void EnterNode_SetsPreviousToOldCurrent()
        {
            _tracker.EnterNode("v_entry");
            _tracker.EnterNode("v_hall_a");
            Assert.AreEqual("v_entry", _tracker.PreviousNodeId);
            Assert.AreEqual("v_hall_a", _tracker.CurrentNodeId);
        }

        [Test]
        public void ExitNode_ClearsCurrentIfMatching()
        {
            _tracker.EnterNode("v_entry");
            _tracker.ExitNode("v_entry");
            Assert.IsNull(_tracker.CurrentNodeId);
        }

        [Test]
        public void ExitNode_DoesNotClearIfDifferent()
        {
            _tracker.EnterNode("v_entry");
            _tracker.EnterNode("v_hall_a");
            _tracker.ExitNode("v_entry");
            Assert.AreEqual("v_hall_a", _tracker.CurrentNodeId);
        }

        [Test]
        public void ExitNode_WhenNoCurrentNode_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _tracker.ExitNode("v_entry"));
        }

        [Test]
        public void EnterNode_NullId_Ignored()
        {
            _tracker.EnterNode(null);
            Assert.IsNull(_tracker.CurrentNodeId);
        }

        [Test]
        public void EnterNode_EmptyId_Ignored()
        {
            _tracker.EnterNode("");
            Assert.IsNull(_tracker.CurrentNodeId);
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run via Unity MCP: `run_tests(mode="EditMode", test_names=["Desync.Tests.EditMode.PlayerNodeTrackerTests"])`.
Expected: compile error — `PlayerNodeTracker` type not found.

- [ ] **Step 3: Write minimal implementation**

```csharp
using UnityEngine;
using Desync.World.Graph.Authoring;

namespace Desync.World.Graph.Runtime
{
    /// <summary>
    /// Tracks which graph node the player currently occupies.
    /// Uses CharacterController trigger detection with room volume colliders.
    /// Place on the player GameObject alongside CharacterController.
    /// </summary>
    public class PlayerNodeTracker : MonoBehaviour
    {
        private string _currentNodeId;
        private string _previousNodeId;

        public string CurrentNodeId => _currentNodeId;
        public string PreviousNodeId => _previousNodeId;

        /// <summary>
        /// Called when entering a room node. Public for testability
        /// and manual override. OnTriggerEnter delegates to this.
        /// </summary>
        public void EnterNode(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId))
                return;

            _previousNodeId = _currentNodeId;
            _currentNodeId = nodeId;
        }

        /// <summary>
        /// Called when exiting a room node. Only clears current if
        /// the exited node matches (handles overlapping volumes).
        /// </summary>
        public void ExitNode(string nodeId)
        {
            if (_currentNodeId == nodeId)
                _currentNodeId = null;
        }

        private void OnTriggerEnter(Collider other)
        {
            var room = other.GetComponent<RoomNodeAuthoring>();
            if (room != null)
                EnterNode(room.NodeId);
        }

        private void OnTriggerExit(Collider other)
        {
            var room = other.GetComponent<RoomNodeAuthoring>();
            if (room != null)
                ExitNode(room.NodeId);
        }
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run via Unity MCP: `run_tests(mode="EditMode", test_names=["Desync.Tests.EditMode.PlayerNodeTrackerTests"])`.
Expected: 9/9 PASS.

- [ ] **Step 5: Commit**

```bash
git add unity-DESYNC/Assets/_Project/Scripts/World/Graph/Runtime/PlayerNodeTracker.cs \
       unity-DESYNC/Assets/_Project/Tests/EditMode/PlayerNodeTrackerTests.cs
git commit -m "feat: PlayerNodeTracker with TDD — enter/exit node state machine (9 tests)"
```

---

### Task 2: SpatialDebugOverlay — IMGUI HUD

**Files:**
- Create: `unity-DESYNC/Assets/_Project/Scripts/World/Graph/Debug/SpatialDebugOverlay.cs`

**Not TDD:** IMGUI rendering code. Verified visually.

- [ ] **Step 1: Create the overlay script**

```csharp
using UnityEngine;
using Desync.World.Graph.Definitions;
using Desync.World.Graph.Runtime;

namespace Desync.World.Graph.Debug
{
    /// <summary>
    /// IMGUI debug HUD for the spatial graph. Shows current node,
    /// graph stats, connected edges, and portal destinations.
    /// Toggle with F3. Restart graph with F5.
    /// </summary>
    public class SpatialDebugOverlay : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GraphRuntimeHost graphHost;

        private bool _visible;
        private PlayerNodeTracker _playerTracker;
        private GUIStyle _boxStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _labelStyle;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F3))
                _visible = !_visible;

            if (Input.GetKeyDown(KeyCode.F5) && graphHost != null && graphHost.Runtime != null)
            {
                graphHost.Runtime.Reset();
                graphHost.Runtime.Initialize(graphHost.Definition);
                UnityEngine.Debug.Log("[SpatialDebugOverlay] Graph runtime restarted.");
            }

            if (_playerTracker == null)
                _playerTracker = FindFirstObjectByType<PlayerNodeTracker>();
        }

        private void OnGUI()
        {
            if (!_visible || graphHost == null || graphHost.Runtime == null)
                return;

            InitStyles();

            var runtime = graphHost.Runtime;
            var x = 10f;
            var y = 10f;
            var w = 320f;

            GUI.Box(new Rect(x, y, w, 24f), "SPATIAL GRAPH DEBUG [F3] [F5 restart]", _headerStyle);
            y += 28f;

            var currentNode = _playerTracker != null ? _playerTracker.CurrentNodeId : "(no tracker)";
            var prevNode = _playerTracker != null ? _playerTracker.PreviousNodeId : "-";

            GUI.Box(new Rect(x, y, w, 60f), "", _boxStyle);
            GUI.Label(new Rect(x + 8, y + 4, w - 16, 20f),
                $"Current Node: <b>{currentNode ?? "none"}</b>", _labelStyle);
            GUI.Label(new Rect(x + 8, y + 22, w - 16, 20f),
                $"Previous Node: {prevNode ?? "-"}", _labelStyle);
            GUI.Label(new Rect(x + 8, y + 40, w - 16, 20f),
                $"Nodes: {runtime.NodeCount}  Edges: {runtime.EdgeCount}", _labelStyle);
            y += 64f;

            if (currentNode != null && runtime.GetNode(currentNode, out _))
            {
                var edges = runtime.GetConnectedEdges(currentNode);
                var edgeH = Mathf.Max(24f, edges.Count * 20f + 8f);
                GUI.Box(new Rect(x, y, w, edgeH), "", _boxStyle);
                for (int i = 0; i < edges.Count; i++)
                {
                    var edge = edges[i];
                    runtime.GetDestinationNode(edge.edgeId, currentNode, out var destId);
                    GUI.Label(new Rect(x + 8, y + 4 + i * 20f, w - 16, 20f),
                        $"  {edge.edgeId} -> <b>{destId}</b>", _labelStyle);
                }
            }
        }

        private void InitStyles()
        {
            if (_boxStyle != null) return;

            _boxStyle = new GUIStyle(GUI.skin.box);
            _boxStyle.normal.background = MakeTex(1, 1, new Color(0f, 0f, 0f, 0.7f));

            _headerStyle = new GUIStyle(GUI.skin.box);
            _headerStyle.normal.background = MakeTex(1, 1, new Color(0.2f, 0.4f, 0.2f, 0.85f));
            _headerStyle.normal.textColor = Color.white;
            _headerStyle.fontStyle = FontStyle.Bold;
            _headerStyle.alignment = TextAnchor.MiddleLeft;
            _headerStyle.padding = new RectOffset(8, 8, 2, 2);

            _labelStyle = new GUIStyle(GUI.skin.label);
            _labelStyle.normal.textColor = Color.white;
            _labelStyle.richText = true;
            _labelStyle.fontSize = 12;
        }

        private static Texture2D MakeTex(int w, int h, Color col)
        {
            var tex = new Texture2D(w, h);
            var pixels = new Color[w * h];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = col;
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }
    }
}
```

- [ ] **Step 2: Check console for compile errors**

Run: `refresh_unity` then `read_console(types=["error"])`.
Expected: no new compilation errors.

- [ ] **Step 3: Commit**

```bash
git add unity-DESYNC/Assets/_Project/Scripts/World/Graph/Debug/SpatialDebugOverlay.cs
git commit -m "feat: SpatialDebugOverlay — IMGUI HUD with F3 toggle, F5 restart"
```

---

### Task 3: SpatialDebugGizmos — Scene View Visualization

**Files:**
- Create: `unity-DESYNC/Assets/_Project/Scripts/World/Graph/Debug/SpatialDebugGizmos.cs`

**Not TDD:** Editor gizmo drawing. Verified visually.

- [ ] **Step 1: Create the gizmos script**

```csharp
using UnityEngine;
using Desync.World.Graph.Definitions;

namespace Desync.World.Graph.Debug
{
    /// <summary>
    /// Draws graph topology in the scene view: node boxes with labels,
    /// edge lines between connected nodes, portal anchor markers.
    /// </summary>
    public class SpatialDebugGizmos : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GraphRuntimeHost graphHost;

        [Header("Colors")]
        [SerializeField] private Color nodeColor = new Color(0.2f, 0.8f, 0.3f, 0.3f);
        [SerializeField] private Color edgeColor = new Color(0.3f, 0.6f, 1f, 0.8f);
        [SerializeField] private Color anchorColor = new Color(1f, 0.5f, 0.2f, 0.8f);

        private void OnDrawGizmos()
        {
            if (graphHost == null || graphHost.Definition == null)
                return;

            var def = graphHost.Definition;
            DrawNodes(def);
            DrawEdges(def);
            DrawAnchors(def);
        }

        private void DrawNodes(HouseGraphDefinition def)
        {
            Gizmos.color = nodeColor;
            foreach (var node in def.nodes)
            {
                var pos = node.worldPosition + Vector3.up * 1.5f;
                Gizmos.DrawWireCube(pos, new Vector3(5f, 3f, 5f));

#if UNITY_EDITOR
                var labelPos = node.worldPosition + Vector3.up * 3.2f;
                UnityEditor.Handles.Label(labelPos,
                    $"{node.nodeId}\n{node.displayName}",
                    new GUIStyle
                    {
                        normal = { textColor = Color.green },
                        fontStyle = FontStyle.Bold,
                        fontSize = 11,
                        alignment = TextAnchor.MiddleCenter
                    });
#endif
            }
        }

        private void DrawEdges(HouseGraphDefinition def)
        {
            Gizmos.color = edgeColor;
            foreach (var edge in def.edges)
            {
                Vector3 srcPos = Vector3.zero, tgtPos = Vector3.zero;
                bool foundSrc = false, foundTgt = false;

                foreach (var node in def.nodes)
                {
                    if (node.nodeId == edge.sourceNodeId) { srcPos = node.worldPosition + Vector3.up * 1.5f; foundSrc = true; }
                    if (node.nodeId == edge.targetNodeId) { tgtPos = node.worldPosition + Vector3.up * 1.5f; foundTgt = true; }
                }

                if (foundSrc && foundTgt)
                {
                    Gizmos.DrawLine(srcPos, tgtPos);

#if UNITY_EDITOR
                    var mid = (srcPos + tgtPos) * 0.5f + Vector3.up * 0.3f;
                    UnityEditor.Handles.Label(mid, edge.edgeId,
                        new GUIStyle
                        {
                            normal = { textColor = new Color(0.5f, 0.8f, 1f) },
                            fontSize = 10,
                            alignment = TextAnchor.MiddleCenter
                        });
#endif
                }
            }
        }

        private void DrawAnchors(HouseGraphDefinition def)
        {
            Gizmos.color = anchorColor;
            foreach (var node in def.nodes)
            {
                if (node.portalAnchors == null) continue;
                foreach (var anchor in node.portalAnchors)
                {
                    var worldPos = node.worldPosition + anchor.localPosition + Vector3.up * 1.25f;
                    Gizmos.DrawWireSphere(worldPos, 0.3f);

#if UNITY_EDITOR
                    UnityEditor.Handles.Label(worldPos + Vector3.up * 0.5f,
                        anchor.anchorId,
                        new GUIStyle
                        {
                            normal = { textColor = new Color(1f, 0.6f, 0.3f) },
                            fontSize = 9
                        });
#endif
                }
            }
        }
    }
}
```

- [ ] **Step 2: Check console for compile errors**

Run: `refresh_unity` then `read_console(types=["error"])`.
Expected: no new compilation errors.

- [ ] **Step 3: Commit**

```bash
git add unity-DESYNC/Assets/_Project/Scripts/World/Graph/Debug/SpatialDebugGizmos.cs
git commit -m "feat: SpatialDebugGizmos — scene view node/edge/anchor visualization"
```

---

### Task 4: Wire into scene and PF_Player prefab

**Execution:** Unity MCP (main context only)

- [ ] **Step 1: Add PlayerNodeTracker to PF_Player prefab**

Via `manage_prefabs(action="modify_contents")` or `execute_code`:
- Add `PlayerNodeTracker` component to `Assets/_Project/Prefabs/PF_Player.prefab`

- [ ] **Step 2: Add debug GameObjects to House_Prototype scene**

Load House_Prototype scene if not active, then:
```
batch_execute([
    {"tool": "manage_gameobject", "params": {"action": "create", "name": "SpatialDebugOverlay", "components_to_add": ["Desync.World.Graph.Debug.SpatialDebugOverlay"]}},
    {"tool": "manage_gameobject", "params": {"action": "create", "name": "SpatialDebugGizmos", "components_to_add": ["Desync.World.Graph.Debug.SpatialDebugGizmos"]}}
])
```

- [ ] **Step 3: Wire graphHost references on both debug components**

Via `execute_code`: find the GraphRuntimeHost in scene, set it as the `graphHost` field on both SpatialDebugOverlay and SpatialDebugGizmos using SerializedObject.

- [ ] **Step 4: Save scene**

```
manage_scene(action="save")
```

- [ ] **Step 5: Take scene view screenshot to verify gizmos**

```
manage_camera(action="screenshot", capture_source="scene_view", view_target="Room_HallA", include_image=True, max_resolution=512)
```

- [ ] **Step 6: Commit**

```bash
git add unity-DESYNC/Assets/_Project/Scenes/House_Prototype.unity
git commit -m "feat: wire debug overlay + gizmos + PlayerNodeTracker into scene and PF_Player"
```

---

### Task 5: Smoke test from Bootstrap

**Execution:** Unity MCP — Play mode verification

Sprint PDD smoke test checklist:
- [ ] Launch from Bootstrap
- [ ] Enter House_Prototype (host starts, scene loads)
- [ ] Confirm graph asset loads (console log from GraphRuntimeHost)
- [ ] Confirm runtime reports 5 nodes (debug overlay)
- [ ] Confirm current node identified from player position (walk into a room trigger)
- [ ] Confirm connected edges appear in debug for current node
- [ ] Confirm at least one portal destination resolves in debug
- [ ] Trigger restart (F5)
- [ ] Confirm graph runtime resets (console log)
- [ ] Confirm no blocker console errors

- [ ] **Step 1: Load Bootstrap scene**

```
manage_scene(action="load", path="Assets/_Project/Scenes/Bootstrap.unity")
```

- [ ] **Step 2: Enter Play mode**

```
manage_editor(action="play")
```

- [ ] **Step 3: Check console for GraphRuntimeHost initialization**

```
read_console(types=["log"], filter_text="GraphRuntimeHost")
```

Expected: `[GraphRuntimeHost] Initialized graph — 5 nodes, 4 edges`

- [ ] **Step 4: Screenshot with overlay visible (F3)**

Note: Cannot press F3 via MCP. The overlay may need to start visible, OR verify via console logs and scene state. Consider defaulting `_visible = true` for graybox phase, toggled off for production.

- [ ] **Step 5: Check for blocker errors**

```
read_console(types=["error"], count=20)
```

- [ ] **Step 6: Exit Play mode**

```
manage_editor(action="stop")
```

- [ ] **Step 7: Final commit**

```bash
git add -A  # only if scene/prefab changes from Play mode testing
git commit -m "feat: S1A debug overlay + smoke test verified — sprint complete"
```

---

## Self-Review

**Spec coverage:** All 4 sprint PDD Task 5 acceptance tests covered (toggle, current node + destinations, gizmos labels, verify from overlay alone). All 10 smoke test items addressed in Task 5. Network sync intentionally excluded (not in sprint PDD scope).

**Placeholder scan:** No TBD/TODO. All code blocks complete. Task 4 Step 3 uses execute_code pattern established in Session 2.

**Type consistency:** `PlayerNodeTracker.CurrentNodeId` (string), `GraphRuntimeHost.Runtime` (SpatialGraphRuntime), `GraphRuntimeHost.Definition` (HouseGraphDefinition) — all match existing committed code. `EnterNode`/`ExitNode` signatures match between test and implementation.
