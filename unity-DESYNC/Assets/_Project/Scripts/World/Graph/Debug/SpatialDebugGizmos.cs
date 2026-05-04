using System.Collections.Generic;
using UnityEngine;
using Desync.World.Graph.Definitions;

namespace Desync.World.Graph.Debug
{
    /// <summary>
    /// Scene-view gizmo overlay for the house graph. Attach to any GameObject alongside
    /// (or pointing at) a GraphRuntimeHost to see nodes, edges, and portal anchors.
    /// </summary>
    public class SpatialDebugGizmos : MonoBehaviour
    {
        [Header("Host")]
        [SerializeField] private GraphRuntimeHost graphHost;

        [Header("Colors")]
        [SerializeField] private Color nodeColor   = new Color(0.2f, 0.8f, 0.3f, 0.3f);
        [SerializeField] private Color edgeColor   = new Color(0.3f, 0.6f, 1.0f, 0.8f);
        [SerializeField] private Color anchorColor = new Color(1.0f, 0.5f, 0.2f, 0.8f);

        private static readonly Vector3 NodeSize     = new Vector3(5f, 3f, 5f);
        private const float NodeYOffset              = 1.5f;
        private const float AnchorYOffset            = 1.25f;
        private const float AnchorRadius             = 0.3f;

        private void OnDrawGizmos()
        {
            if (graphHost == null || graphHost.Definition == null) return;

            var def = graphHost.Definition;
            // Build a quick lookup so edge drawing is O(1) per edge.
            var nodeById = BuildNodeLookup(def.nodes);

            DrawNodes(def.nodes);
            DrawEdges(def.edges, nodeById);
            DrawAnchors(def.nodes);
        }

        private static Dictionary<string, HouseNodeDefinition> BuildNodeLookup(HouseNodeDefinition[] nodes)
        {
            var map = new Dictionary<string, HouseNodeDefinition>(nodes.Length);
            foreach (var n in nodes)
                if (!string.IsNullOrEmpty(n.nodeId)) map[n.nodeId] = n;
            return map;
        }

        private void DrawNodes(HouseNodeDefinition[] nodes)
        {
            Gizmos.color = nodeColor;
#if UNITY_EDITOR
            var style = new GUIStyle { normal = { textColor = Color.green } };
#endif
            foreach (var node in nodes)
            {
                var center = node.worldPosition + Vector3.up * NodeYOffset;
                Gizmos.DrawWireCube(center, NodeSize);
#if UNITY_EDITOR
                UnityEditor.Handles.Label(center + Vector3.up * (NodeSize.y * 0.5f),
                    $"{node.nodeId}\n{node.displayName}", style);
#endif
            }
        }

        private void DrawEdges(HouseEdgeDefinition[] edges,
            Dictionary<string, HouseNodeDefinition> nodeById)
        {
            Gizmos.color = edgeColor;
#if UNITY_EDITOR
            var style = new GUIStyle { normal = { textColor = new Color(0.5f, 0.8f, 1.0f) } };
#endif
            foreach (var edge in edges)
            {
                if (!nodeById.TryGetValue(edge.sourceNodeId, out var src)) continue;
                if (!nodeById.TryGetValue(edge.targetNodeId, out var tgt)) continue;

                var a = src.worldPosition + Vector3.up * NodeYOffset;
                var b = tgt.worldPosition + Vector3.up * NodeYOffset;
                Gizmos.DrawLine(a, b);
#if UNITY_EDITOR
                UnityEditor.Handles.Label((a + b) * 0.5f, edge.edgeId, style);
#endif
            }
        }

        private void DrawAnchors(HouseNodeDefinition[] nodes)
        {
            Gizmos.color = anchorColor;
#if UNITY_EDITOR
            var style = new GUIStyle { normal = { textColor = new Color(1.0f, 0.6f, 0.3f) } };
#endif
            foreach (var node in nodes)
            {
                if (node.portalAnchors == null) continue;
                foreach (var anchor in node.portalAnchors)
                {
                    var pos = node.worldPosition + anchor.localPosition + Vector3.up * AnchorYOffset;
                    Gizmos.DrawWireSphere(pos, AnchorRadius);
#if UNITY_EDITOR
                    UnityEditor.Handles.Label(pos + Vector3.up * AnchorRadius, anchor.anchorId, style);
#endif
                }
            }
        }
    }
}
