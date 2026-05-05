using System.Collections.Generic;
using Desync.World.Graph.Definitions;

namespace Desync.World.Graph.Runtime
{
    /// <summary>
    /// Runtime query engine for the house graph. Initialized from a
    /// HouseGraphDefinition ScriptableObject; provides O(1) lookups
    /// via internal dictionaries.
    /// </summary>
    public class SpatialGraphRuntime
    {
        private Dictionary<string, HouseNodeDefinition> _nodes = new();
        private Dictionary<string, HouseEdgeDefinition> _edges = new();
        private Dictionary<string, List<HouseEdgeDefinition>> _connectedEdges = new();
        private Dictionary<string, Dictionary<string, PortalAnchorDefinition>> _anchors = new();

        public int NodeCount => _nodes.Count;
        public int EdgeCount => _edges.Count;

        public void Initialize(HouseGraphDefinition definition)
        {
            Reset();

            if (definition == null)
                return;

            foreach (var node in definition.nodes)
            {
                _nodes[node.nodeId] = node;
                _connectedEdges[node.nodeId] = new List<HouseEdgeDefinition>();

                var anchorMap = new Dictionary<string, PortalAnchorDefinition>();
                if (node.portalAnchors != null)
                {
                    foreach (var anchor in node.portalAnchors)
                    {
                        anchorMap[anchor.anchorId] = anchor;
                    }
                }
                _anchors[node.nodeId] = anchorMap;
            }

            foreach (var edge in definition.edges)
            {
                _edges[edge.edgeId] = edge;

                if (_connectedEdges.TryGetValue(edge.sourceNodeId, out var sourceList))
                    sourceList.Add(edge);
                if (_connectedEdges.TryGetValue(edge.targetNodeId, out var targetList))
                    targetList.Add(edge);
            }
        }

        public bool GetNode(string nodeId, out HouseNodeDefinition node)
        {
            node = default;
            if (string.IsNullOrEmpty(nodeId)) return false;
            return _nodes.TryGetValue(nodeId, out node);
        }

        public bool GetEdge(string edgeId, out HouseEdgeDefinition edge)
        {
            edge = default;
            if (string.IsNullOrEmpty(edgeId)) return false;
            return _edges.TryGetValue(edgeId, out edge);
        }

        private static readonly List<HouseEdgeDefinition> EmptyEdgeList = new();

        public IReadOnlyList<HouseEdgeDefinition> GetConnectedEdges(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId)) return EmptyEdgeList;
            if (_connectedEdges.TryGetValue(nodeId, out var edges))
                return edges.AsReadOnly();
            return EmptyEdgeList;
        }

        /// <summary>
        /// Given an edge and the node you're standing in, returns the node
        /// on the other side. Edges are bidirectional.
        /// </summary>
        public bool GetDestinationNode(string edgeId, string currentNodeId, out string destinationNodeId)
        {
            destinationNodeId = null;

            if (string.IsNullOrEmpty(edgeId)) return false;
            if (!_edges.TryGetValue(edgeId, out var edge))
                return false;

            if (edge.sourceNodeId == currentNodeId)
            {
                destinationNodeId = edge.targetNodeId;
                return true;
            }

            if (edge.targetNodeId == currentNodeId)
            {
                destinationNodeId = edge.sourceNodeId;
                return true;
            }

            return false;
        }

        public bool GetPortalAnchor(string nodeId, string anchorId, out PortalAnchorDefinition anchor)
        {
            anchor = default;

            if (!_anchors.TryGetValue(nodeId, out var anchorMap))
                return false;

            return anchorMap.TryGetValue(anchorId, out anchor);
        }

        public void Reset()
        {
            _nodes.Clear();
            _edges.Clear();
            _connectedEdges.Clear();
            _anchors.Clear();
        }
    }
}
