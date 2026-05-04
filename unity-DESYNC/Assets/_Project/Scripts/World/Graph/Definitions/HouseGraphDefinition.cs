using System;
using System.Collections.Generic;
using UnityEngine;

namespace Desync.World.Graph.Definitions
{
    [CreateAssetMenu(fileName = "HouseGraphDefinition", menuName = "Desync/House Graph Definition")]
    public class HouseGraphDefinition : ScriptableObject
    {
        [Header("Graph Topology")]
        public HouseNodeDefinition[] nodes = Array.Empty<HouseNodeDefinition>();
        public HouseEdgeDefinition[] edges = Array.Empty<HouseEdgeDefinition>();

        /// <summary>
        /// Validates the graph definition for structural errors.
        /// Returns an empty list if the graph is valid.
        /// </summary>
        public List<string> Validate()
        {
            var errors = new List<string>();

            var nodeIds = new HashSet<string>();
            var nodeAnchors = new Dictionary<string, HashSet<string>>();

            foreach (var node in nodes)
            {
                if (!nodeIds.Add(node.nodeId))
                {
                    errors.Add($"Duplicate node ID: '{node.nodeId}'");
                    continue;
                }

                var anchors = new HashSet<string>();
                if (node.portalAnchors != null)
                {
                    foreach (var anchor in node.portalAnchors)
                    {
                        anchors.Add(anchor.anchorId);
                    }
                }
                nodeAnchors[node.nodeId] = anchors;
            }

            var edgeIds = new HashSet<string>();
            foreach (var edge in edges)
            {
                if (!edgeIds.Add(edge.edgeId))
                {
                    errors.Add($"Duplicate edge ID: '{edge.edgeId}'");
                    continue;
                }

                if (!nodeIds.Contains(edge.sourceNodeId))
                    errors.Add($"Edge '{edge.edgeId}' references nonexistent source node '{edge.sourceNodeId}'");

                if (!nodeIds.Contains(edge.targetNodeId))
                    errors.Add($"Edge '{edge.edgeId}' references nonexistent target node '{edge.targetNodeId}'");

                if (nodeAnchors.TryGetValue(edge.sourceNodeId, out var sourceAnchors)
                    && !sourceAnchors.Contains(edge.sourceAnchorId))
                    errors.Add($"Edge '{edge.edgeId}' references nonexistent anchor '{edge.sourceAnchorId}' on node '{edge.sourceNodeId}'");

                if (nodeAnchors.TryGetValue(edge.targetNodeId, out var targetAnchors)
                    && !targetAnchors.Contains(edge.targetAnchorId))
                    errors.Add($"Edge '{edge.edgeId}' references nonexistent anchor '{edge.targetAnchorId}' on node '{edge.targetNodeId}'");
            }

            return errors;
        }
    }

    [Serializable]
    public struct HouseNodeDefinition
    {
        public string nodeId;
        public string displayName;
        [Tooltip("Room prefab to instantiate for this node")]
        public GameObject roomPrefab;
        [Tooltip("World-space position where the room prefab is placed")]
        public Vector3 worldPosition;
        public PortalAnchorDefinition[] portalAnchors;
    }

    [Serializable]
    public struct HouseEdgeDefinition
    {
        public string edgeId;
        public string sourceNodeId;
        public string targetNodeId;
        public string sourceAnchorId;
        public string targetAnchorId;
    }

    [Serializable]
    public struct PortalAnchorDefinition
    {
        public string anchorId;
        public Vector3 localPosition;
        public Quaternion localRotation;
    }
}
