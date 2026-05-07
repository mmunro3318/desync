using System.Collections.Generic;
using UnityEngine;
using Desync.World.Graph.Definitions;
using Desync.World.Graph.Runtime;

namespace Desync.World.Graph.Debug
{
    /// <summary>
    /// Scene-view gizmo overlay for the house graph. Draws nodes colored by activation state
    /// (green=active/occupied, yellow=portal-visible, gray=inactive), edges, and portal anchors.
    /// </summary>
    public class SpatialDebugGizmos : MonoBehaviour
    {
        [Header("Host")]
        [SerializeField] private GraphRuntimeHost graphHost;

        [Header("Topology Colors")]
        [SerializeField] private Color edgeColor   = new Color(0.3f, 0.6f, 1.0f, 0.8f);
        [SerializeField] private Color anchorColor = new Color(1.0f, 0.5f, 0.2f, 0.8f);

        [Header("Activation Colors")]
        [SerializeField] private Color activeNodeColor        = new Color(0.2f, 0.9f, 0.3f, 0.5f);
        [SerializeField] private Color portalVisibleNodeColor = new Color(1.0f, 0.9f, 0.2f, 0.5f);
        [SerializeField] private Color inactiveNodeColor      = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        [SerializeField] private Color sightlineColor         = new Color(1.0f, 1.0f, 0.0f, 0.6f);

        private static readonly Vector3 NodeSize     = new Vector3(5f, 3f, 5f);
        private const float NodeYOffset              = 1.5f;
        private const float AnchorYOffset            = 1.25f;
        private const float AnchorRadius             = 0.3f;

        // Cached to avoid per-frame allocation in OnDrawGizmos
        private Dictionary<string, HouseNodeDefinition> _cachedNodeLookup;
        private HouseNodeDefinition[] _cachedNodesSource;
        private NodeStreamingController _streamingController;
        private bool _controllerSearched;
#if UNITY_EDITOR
        private GUIStyle _nodeStyle;
        private GUIStyle _edgeStyle;
        private GUIStyle _anchorStyle;
#endif

        private void OnDrawGizmos()
        {
            if (graphHost == null || graphHost.Definition == null) return;

            var def = graphHost.Definition;
            if (_cachedNodeLookup == null || _cachedNodesSource != def.nodes)
            {
                _cachedNodeLookup = BuildNodeLookup(def.nodes);
                _cachedNodesSource = def.nodes;
            }

            FindController();
            var activation = _streamingController != null ? _streamingController.LastResult : null;

            DrawNodes(def.nodes, activation);
            DrawEdges(def.edges, _cachedNodeLookup);
            DrawAnchors(def.nodes);
            DrawPortalSightlines(activation);
        }

        private void FindController()
        {
            if (_streamingController != null || _controllerSearched) return;
            _streamingController = FindAnyObjectByType<NodeStreamingController>();
            _controllerSearched = true;
        }

        private static Dictionary<string, HouseNodeDefinition> BuildNodeLookup(HouseNodeDefinition[] nodes)
        {
            var map = new Dictionary<string, HouseNodeDefinition>(nodes.Length);
            foreach (var n in nodes)
                if (!string.IsNullOrEmpty(n.nodeId)) map[n.nodeId] = n;
            return map;
        }

        private void DrawNodes(HouseNodeDefinition[] nodes,
            IReadOnlyDictionary<string, NodeActivationReason> activation)
        {
#if UNITY_EDITOR
            _nodeStyle ??= new GUIStyle { normal = { textColor = Color.green } };
#endif
            foreach (var node in nodes)
            {
                Gizmos.color = GetNodeColor(node.nodeId, activation);
                var center = node.worldPosition + Vector3.up * NodeYOffset;
                Gizmos.DrawWireCube(center, NodeSize);
#if UNITY_EDITOR
                UnityEditor.Handles.Label(center + Vector3.up * (NodeSize.y * 0.5f),
                    $"{node.nodeId}\n{node.displayName}", _nodeStyle);
#endif
            }
        }

        private Color GetNodeColor(string nodeId,
            IReadOnlyDictionary<string, NodeActivationReason> activation)
        {
            if (activation == null || !activation.TryGetValue(nodeId, out var reason))
                return inactiveNodeColor;

            if (reason == NodeActivationReason.None) return inactiveNodeColor;
            if ((reason & NodeActivationReason.Occupied) != 0) return activeNodeColor;
            if ((reason & NodeActivationReason.PortalVisible) != 0) return portalVisibleNodeColor;
            return activeNodeColor; // Adjacent or DebugForced
        }

        private void DrawEdges(HouseEdgeDefinition[] edges,
            Dictionary<string, HouseNodeDefinition> nodeById)
        {
            Gizmos.color = edgeColor;
#if UNITY_EDITOR
            _edgeStyle ??= new GUIStyle { normal = { textColor = new Color(0.5f, 0.8f, 1.0f) } };
#endif
            foreach (var edge in edges)
            {
                if (!nodeById.TryGetValue(edge.sourceNodeId, out var src)) continue;
                if (!nodeById.TryGetValue(edge.targetNodeId, out var tgt)) continue;

                var a = src.worldPosition + Vector3.up * NodeYOffset;
                var b = tgt.worldPosition + Vector3.up * NodeYOffset;
                Gizmos.DrawLine(a, b);
#if UNITY_EDITOR
                UnityEditor.Handles.Label((a + b) * 0.5f, edge.edgeId, _edgeStyle);
#endif
            }
        }

        private void DrawAnchors(HouseNodeDefinition[] nodes)
        {
            Gizmos.color = anchorColor;
#if UNITY_EDITOR
            _anchorStyle ??= new GUIStyle { normal = { textColor = new Color(1.0f, 0.6f, 0.3f) } };
#endif
            foreach (var node in nodes)
            {
                if (node.portalAnchors == null) continue;
                foreach (var anchor in node.portalAnchors)
                {
                    var pos = node.worldPosition + anchor.localPosition + Vector3.up * AnchorYOffset;
                    Gizmos.DrawWireSphere(pos, AnchorRadius);
#if UNITY_EDITOR
                    UnityEditor.Handles.Label(pos + Vector3.up * AnchorRadius, anchor.anchorId, _anchorStyle);
#endif
                }
            }
        }

        /// <summary>
        /// Draws sightline rays from the streaming controller's transform to each
        /// portal-visible node's world position.
        /// </summary>
        private void DrawPortalSightlines(
            IReadOnlyDictionary<string, NodeActivationReason> activation)
        {
            if (activation == null || _streamingController == null) return;

            Gizmos.color = sightlineColor;
            var origin = _streamingController.transform.position + Vector3.up * NodeYOffset;

            foreach (var kvp in activation)
            {
                if ((kvp.Value & NodeActivationReason.PortalVisible) == 0) continue;
                if (!_cachedNodeLookup.TryGetValue(kvp.Key, out var nodeDef)) continue;

                var target = nodeDef.worldPosition + Vector3.up * NodeYOffset;
                Gizmos.DrawLine(origin, target);
            }
        }
    }
}
