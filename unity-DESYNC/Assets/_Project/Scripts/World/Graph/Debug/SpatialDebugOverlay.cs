using UnityEngine;
using Desync.World.Graph.Runtime;

namespace Desync.World.Graph.Debug
{
    // IMGUI HUD: live spatial graph state. F3 toggles; F5 restarts runtime.
    public class SpatialDebugOverlay : MonoBehaviour
    {
        [SerializeField] private GraphRuntimeHost graphHost;

        private bool _visible = true;
        private PlayerNodeTracker _playerTracker;
        private GUIStyle _labelStyle;
        private Texture2D _darkBg;
        private Texture2D _greenBg;
        private bool _stylesInitialized;

        private void Update()
        {
            if (_playerTracker == null) // Lazy-find: player spawns dynamically via NGO
                _playerTracker = FindAnyObjectByType<PlayerNodeTracker>();
        }

        private void RestartRuntime()
        {
            if (graphHost == null || graphHost.Runtime == null)
            {
                UnityEngine.Debug.LogWarning("[SpatialDebugOverlay] Cannot restart — graphHost or Runtime is null.");
                return;
            }
            graphHost.Runtime.Reset();
            graphHost.Runtime.Initialize(graphHost.Definition);
            UnityEngine.Debug.Log("[SpatialDebugOverlay] Graph runtime restarted via F5.");
        }

        private void OnGUI()
        {
            // Input handling via IMGUI — avoids Input System null-device crash
            if (Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.F3) _visible = !_visible;
                if (Event.current.keyCode == KeyCode.F5) RestartRuntime();
            }

            if (!_visible) return;
            EnsureStyles();

            const float panelX = 8f, panelY = 8f, panelW = 320f, lineH = 20f;
            var runtime = graphHost != null ? graphHost.Runtime : null;
            string curId = _playerTracker != null ? _playerTracker.CurrentNodeId ?? "—" : "—";
            string prevId = _playerTracker != null ? _playerTracker.PreviousNodeId ?? "—" : "—";
            int nodeCount = runtime != null ? runtime.NodeCount : 0;
            int edgeCount = runtime != null ? runtime.EdgeCount : 0;

            var connEdges = (runtime != null && curId != "—") ? runtime.GetConnectedEdges(curId) : null;
            int edgeRows = connEdges != null ? connEdges.Count : 0;
            float totalH = lineH * (5 + edgeRows) + 12f;

            GUI.DrawTexture(new Rect(panelX, panelY, panelW, totalH), _darkBg);
            GUI.DrawTexture(new Rect(panelX, panelY, panelW, lineH + 4f), _greenBg);
            GUI.Label(new Rect(panelX + 4, panelY + 2, panelW - 8, lineH),
                "<b>SPATIAL GRAPH DEBUG</b>  [F3] [F5 restart]", _labelStyle);

            float y = panelY + lineH + 6f;
            GUI.Label(new Rect(panelX + 4, y, panelW - 8, lineH), $"<b>Current Node:</b>  {curId}", _labelStyle); y += lineH;
            GUI.Label(new Rect(panelX + 4, y, panelW - 8, lineH), $"Previous Node:  {prevId}", _labelStyle); y += lineH;
            GUI.Label(new Rect(panelX + 4, y, panelW - 8, lineH), $"Nodes: {nodeCount}   Edges: {edgeCount}", _labelStyle); y += lineH;

            if (connEdges != null && connEdges.Count > 0)
            {
                GUI.Label(new Rect(panelX + 4, y, panelW - 8, lineH), "Connected edges:", _labelStyle); y += lineH;
                foreach (var edge in connEdges)
                {
                    runtime.GetDestinationNode(edge.edgeId, curId, out string destId);
                    GUI.Label(new Rect(panelX + 12, y, panelW - 16, lineH),
                        $"  {edge.edgeId}  →  {destId ?? "?"}", _labelStyle);
                    y += lineH;
                }
            }
            else
            {
                GUI.Label(new Rect(panelX + 4, y, panelW - 8, lineH), "No connected edges", _labelStyle);
            }
        }

        private void EnsureStyles()
        {
            if (_stylesInitialized) return;
            _darkBg = MakeTex(1, 1, new Color(0f, 0f, 0f, 0.7f));
            _greenBg = MakeTex(1, 1, new Color(0.2f, 0.4f, 0.2f, 0.85f));
            _labelStyle = new GUIStyle(GUI.skin.label) { richText = true, fontSize = 12 };
            _labelStyle.normal.textColor = Color.white;
            _stylesInitialized = true;
        }

        private void OnDestroy()
        {
            if (_darkBg != null) Destroy(_darkBg);
            if (_greenBg != null) Destroy(_greenBg);
        }

        private static Texture2D MakeTex(int w, int h, Color col)
        {
            var tex = new Texture2D(w, h);
            tex.SetPixel(0, 0, col);
            tex.Apply();
            return tex;
        }
    }
}
