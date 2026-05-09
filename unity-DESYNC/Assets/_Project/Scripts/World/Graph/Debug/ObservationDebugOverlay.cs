using System.Collections.Generic;
using UnityEngine;
using Desync.World.Graph.Runtime;

namespace Desync.World.Graph.Debug
{
    /// <summary>
    /// IMGUI HUD showing observation lock state: per-node/edge lock reasons,
    /// grace timers, eligible counts, and keyboard-driven debug overrides.
    /// F6 toggles visibility. While visible: Up/Down to select target,
    /// L to force-lock, U to force-unlock, C to clear all overrides.
    /// </summary>
    public class ObservationDebugOverlay : MonoBehaviour
    {
        [SerializeField] private GraphRuntimeHost graphHost;

        private bool _visible;
        private int _selectedIndex;
        private GUIStyle _labelStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _eligibleStyle;
        private GUIStyle _lockedStyle;
        private GUIStyle _graceStyle;
        private GUIStyle _selectedStyle;
        private bool _stylesInitialized;

        // Snapshot of target IDs for selection stability across frames
        private readonly List<TargetEntry> _targetList = new();

        private const float PanelX = 340f;
        private const float PanelY = 8f;
        private const float PanelW = 340f;
        private const float LineH = 18f;

        private struct TargetEntry
        {
            public string Id;
            public bool IsNode;
        }

        private void OnGUI()
        {
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.F6)
            {
                _visible = !_visible;
                Event.current.Use();
            }

            if (!_visible) return;

            EnsureStyles();

            if (graphHost == null)
            {
                graphHost = FindAnyObjectByType<GraphRuntimeHost>();
                if (graphHost == null) return;
            }

            var lockQuery = graphHost.ObservationLock;
            if (lockQuery == null)
            {
                GUI.Label(new Rect(PanelX, PanelY, PanelW, LineH),
                    "[F6] Observation Lock — not initialized", _headerStyle);
                return;
            }

            // Rebuild target list each frame for accurate state
            RebuildTargetList(lockQuery);
            HandleKeyboardInput();

            float y = PanelY;

            // Header
            GUI.Label(new Rect(PanelX, y, PanelW, LineH),
                "<b>OBSERVATION LOCK</b>  [F6]  ↑↓ select  L lock  U unlock  C clear", _headerStyle);
            y += LineH + 2f;

            // Summary counts
            var nodeStates = lockQuery.GetAllNodeStates();
            var edgeStates = lockQuery.GetAllEdgeStates();
            int lockedNodes = 0, eligibleNodes = 0, graceNodes = 0;
            int lockedEdges = 0, eligibleEdges = 0, graceEdges = 0;

            foreach (var kvp in nodeStates)
            {
                if (kvp.Value.IsLocked) lockedNodes++;
                else if (kvp.Value.GraceRemaining > 0f) graceNodes++;
                if (kvp.Value.IsMutationEligible) eligibleNodes++;
            }
            foreach (var kvp in edgeStates)
            {
                if (kvp.Value.IsLocked) lockedEdges++;
                else if (kvp.Value.GraceRemaining > 0f) graceEdges++;
                if (kvp.Value.IsMutationEligible) eligibleEdges++;
            }

            GUI.Label(new Rect(PanelX + 4, y, PanelW - 8, LineH),
                $"Nodes: {lockedNodes} locked, {graceNodes} grace, {eligibleNodes} eligible", _labelStyle);
            y += LineH;
            GUI.Label(new Rect(PanelX + 4, y, PanelW - 8, LineH),
                $"Edges: {lockedEdges} locked, {graceEdges} grace, {eligibleEdges} eligible", _labelStyle);
            y += LineH + 4f;

            // Per-target rows
            int rowIndex = 0;
            foreach (var kvp in nodeStates)
            {
                bool selected = rowIndex == _selectedIndex;
                DrawTargetRow(kvp.Key, kvp.Value.IsLocked,
                    kvp.Value.ActiveReasons, kvp.Value.GraceRemaining, selected, ref y);
                rowIndex++;
            }

            if (edgeStates.Count > 0)
            {
                y += 2f;
                GUI.Label(new Rect(PanelX + 4, y, PanelW - 8, LineH), "— Edges —", _labelStyle);
                y += LineH;

                foreach (var kvp in edgeStates)
                {
                    bool selected = rowIndex == _selectedIndex;
                    DrawTargetRow(kvp.Key, kvp.Value.IsLocked,
                        kvp.Value.ActiveReasons, kvp.Value.GraceRemaining, selected, ref y);
                    rowIndex++;
                }
            }
        }

        private void RebuildTargetList(IObservationLockQuery lockQuery)
        {
            _targetList.Clear();
            foreach (var kvp in lockQuery.GetAllNodeStates())
                _targetList.Add(new TargetEntry { Id = kvp.Key, IsNode = true });
            foreach (var kvp in lockQuery.GetAllEdgeStates())
                _targetList.Add(new TargetEntry { Id = kvp.Key, IsNode = false });

            if (_targetList.Count > 0)
                _selectedIndex = Mathf.Clamp(_selectedIndex, 0, _targetList.Count - 1);
            else
                _selectedIndex = 0;
        }

        private void HandleKeyboardInput()
        {
            if (Event.current.type != EventType.KeyDown) return;
            if (_targetList.Count == 0) return;

            var lockSystem = GetLockSystem();

            switch (Event.current.keyCode)
            {
                case KeyCode.UpArrow:
                    _selectedIndex = Mathf.Max(0, _selectedIndex - 1);
                    Event.current.Use();
                    break;

                case KeyCode.DownArrow:
                    _selectedIndex = Mathf.Min(_targetList.Count - 1, _selectedIndex + 1);
                    Event.current.Use();
                    break;

                case KeyCode.L:
                    if (lockSystem != null)
                    {
                        var entry = _targetList[_selectedIndex];
                        if (entry.IsNode)
                            lockSystem.ForceNodeLock(entry.Id);
                        else
                            lockSystem.ForceEdgeLock(entry.Id);
                        Event.current.Use();
                    }
                    break;

                case KeyCode.U:
                    if (lockSystem != null)
                    {
                        var entry = _targetList[_selectedIndex];
                        if (entry.IsNode)
                            lockSystem.ForceNodeUnlock(entry.Id);
                        else
                            lockSystem.ForceEdgeUnlock(entry.Id);
                        Event.current.Use();
                    }
                    break;

                case KeyCode.C:
                    if (lockSystem != null)
                    {
                        lockSystem.ClearDebugOverrides();
                        Event.current.Use();
                    }
                    break;
            }
        }

        private void DrawTargetRow(string id, bool isLocked,
            IReadOnlyList<LockReason> reasons, float grace, bool selected, ref float y)
        {
            var style = selected ? _selectedStyle
                : isLocked ? _lockedStyle
                : grace > 0f ? _graceStyle
                : _eligibleStyle;

            string status = isLocked ? "LOCKED" : (grace > 0f ? $"GRACE {grace:F1}s" : "ELIGIBLE");
            string reasonStr = FormatReasons(reasons);
            string marker = selected ? "► " : "  ";

            GUI.Label(new Rect(PanelX + 4, y, PanelW - 8, LineH),
                $"{marker}{id}: {status}  {reasonStr}", style);

            y += LineH;
        }

        private static string FormatReasons(IReadOnlyList<LockReason> reasons)
        {
            if (reasons == null || reasons.Count == 0) return "";
            var sb = new System.Text.StringBuilder(32);
            sb.Append('[');
            for (int i = 0; i < reasons.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(reasons[i]);
            }
            sb.Append(']');
            return sb.ToString();
        }

        private ObservationLockSystem GetLockSystem()
        {
            if (graphHost == null) return null;
            return graphHost.ObservationLock as ObservationLockSystem;
        }

        private void EnsureStyles()
        {
            if (_stylesInitialized) return;
            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                richText = true,
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.7f, 0.85f, 1f) }
            };
            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                normal = { textColor = Color.white }
            };
            _lockedStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                normal = { textColor = new Color(1f, 0.4f, 0.3f) }
            };
            _graceStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                normal = { textColor = new Color(1f, 0.9f, 0.3f) }
            };
            _eligibleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                normal = { textColor = new Color(0.3f, 1f, 0.4f) }
            };
            _selectedStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 1f, 1f) }
            };
            _stylesInitialized = true;
        }
    }
}
