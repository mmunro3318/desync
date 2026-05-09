using System.Collections.Generic;
using UnityEngine;
using Desync.World.Graph.Definitions;
using Desync.World.Graph.Runtime;

namespace Desync.World.Graph.Debug
{
    /// <summary>
    /// Scene-view gizmo overlay for observation lock state. Draws translucent
    /// cubes over nodes and colored edge lines indicating lock/grace/eligible.
    /// Complements SpatialDebugGizmos (topology) with mutation-legality coloring.
    /// </summary>
    public class ObservationDebugGizmos : MonoBehaviour
    {
        [Header("Host")]
        [SerializeField] private GraphRuntimeHost graphHost;

        [Header("Lock State Colors")]
        [SerializeField] private Color lockedColor   = new Color(1.0f, 0.2f, 0.2f, 0.35f);
        [SerializeField] private Color graceColor    = new Color(1.0f, 0.9f, 0.2f, 0.35f);
        [SerializeField] private Color eligibleColor = new Color(0.2f, 1.0f, 0.3f, 0.2f);
        [SerializeField] private Color unknownColor  = new Color(0.5f, 0.5f, 0.5f, 0.15f);

        [Header("Edge Lock Colors")]
        [SerializeField] private Color edgeLockedColor   = new Color(1.0f, 0.3f, 0.3f, 0.7f);
        [SerializeField] private Color edgeGraceColor    = new Color(1.0f, 0.85f, 0.2f, 0.5f);
        [SerializeField] private Color edgeEligibleColor = new Color(0.3f, 0.9f, 0.4f, 0.3f);

        private static readonly Vector3 NodeSize = new Vector3(4.5f, 2.5f, 4.5f);
        private const float NodeYOffset = 1.5f;

        private Dictionary<string, HouseNodeDefinition> _nodeLookup;
        private HouseNodeDefinition[] _cachedNodesSource;

#if UNITY_EDITOR
        private GUIStyle _lockLabelStyle;
#endif

        private void OnDrawGizmos()
        {
            if (graphHost == null || graphHost.Definition == null) return;

            var lockQuery = graphHost.ObservationLock;
            if (lockQuery == null) return;

            var def = graphHost.Definition;
            if (_nodeLookup == null || _cachedNodesSource != def.nodes)
            {
                _nodeLookup = BuildNodeLookup(def.nodes);
                _cachedNodesSource = def.nodes;
            }

            DrawNodeLockState(def.nodes, lockQuery);
            DrawEdgeLockState(def.edges, lockQuery);
        }

        private void DrawNodeLockState(HouseNodeDefinition[] nodes, IObservationLockQuery lockQuery)
        {
#if UNITY_EDITOR
            _lockLabelStyle ??= new GUIStyle
            {
                fontSize = 10,
                alignment = TextAnchor.UpperCenter,
                normal = { textColor = Color.white }
            };
#endif
            foreach (var node in nodes)
            {
                var center = node.worldPosition + Vector3.up * NodeYOffset;
                var state = GetNodeLockColor(node.nodeId, lockQuery, out string label);

                Gizmos.color = state;
                Gizmos.DrawCube(center, NodeSize);

#if UNITY_EDITOR
                float grace = lockQuery.GetNodeGraceRemaining(node.nodeId);
                string graceStr = grace > 0f ? $"\ngrace: {grace:F1}s" : "";
                UnityEditor.Handles.Label(
                    center + Vector3.down * (NodeSize.y * 0.5f + 0.2f),
                    $"{label}{graceStr}",
                    _lockLabelStyle);
#endif
            }
        }

        private Color GetNodeLockColor(string nodeId, IObservationLockQuery lockQuery, out string label)
        {
            if (lockQuery.IsNodeLocked(nodeId))
            {
                var reasons = lockQuery.GetNodeLockReasons(nodeId);
                label = FormatReasons(reasons);
                return lockedColor;
            }

            float grace = lockQuery.GetNodeGraceRemaining(nodeId);
            if (grace > 0f)
            {
                label = "GRACE";
                return graceColor;
            }

            if (lockQuery.IsNodeMutationEligible(nodeId))
            {
                label = "ELIGIBLE";
                return eligibleColor;
            }

            label = "";
            return unknownColor;
        }

        private void DrawEdgeLockState(HouseEdgeDefinition[] edges, IObservationLockQuery lockQuery)
        {
            foreach (var edge in edges)
            {
                if (!_nodeLookup.TryGetValue(edge.sourceNodeId, out var src)) continue;
                if (!_nodeLookup.TryGetValue(edge.targetNodeId, out var tgt)) continue;

                var a = src.worldPosition + Vector3.up * NodeYOffset;
                var b = tgt.worldPosition + Vector3.up * NodeYOffset;

                Gizmos.color = GetEdgeLockColor(edge.edgeId, lockQuery);

                // Draw thicker line by offsetting slightly
                var offset = Vector3.up * 0.15f;
                Gizmos.DrawLine(a + offset, b + offset);
                Gizmos.DrawLine(a - offset, b - offset);
            }
        }

        private Color GetEdgeLockColor(string edgeId, IObservationLockQuery lockQuery)
        {
            if (lockQuery.IsEdgeLocked(edgeId))
                return edgeLockedColor;

            float grace = lockQuery.GetEdgeGraceRemaining(edgeId);
            if (grace > 0f)
                return edgeGraceColor;

            return edgeEligibleColor;
        }

        private static string FormatReasons(IReadOnlyList<LockReason> reasons)
        {
            if (reasons == null || reasons.Count == 0) return "";
            var sb = new System.Text.StringBuilder(24);
            for (int i = 0; i < reasons.Count; i++)
            {
                if (i > 0) sb.Append('+');
                sb.Append(reasons[i] switch
                {
                    LockReason.None => "",
                    LockReason.Occupied => "OCC",
                    LockReason.AdjacentOccupiedEdge => "ADJ",
                    LockReason.PortalVisible => "VIS",
                    LockReason.DebugForced => "DBG",
                    LockReason.ProtectedByRule => "RULE",
                    LockReason.GracePeriod => "GRC",
                    _ => "?"
                });
            }
            return sb.ToString();
        }

        private static Dictionary<string, HouseNodeDefinition> BuildNodeLookup(HouseNodeDefinition[] nodes)
        {
            var map = new Dictionary<string, HouseNodeDefinition>(nodes.Length);
            foreach (var n in nodes)
                if (!string.IsNullOrEmpty(n.nodeId)) map[n.nodeId] = n;
            return map;
        }
    }
}
