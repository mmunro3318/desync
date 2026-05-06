using System.Collections.Generic;
using Desync.World.Graph.Definitions;

namespace Desync.World.Graph.Runtime
{
    public class NodeActivationResolver
    {
        private readonly Dictionary<string, NodeActivationReason> _result = new();

        public IReadOnlyDictionary<string, NodeActivationReason> Resolve(
            ViewContext ctx,
            SpatialGraphRuntime graph,
            IReadOnlyList<PortalVisibilityResult> portalResults)
        {
            _result.Clear();

            if (string.IsNullOrEmpty(ctx.OccupiedNodeId))
                return _result;

            // Occupied node always active
            AddReason(ctx.OccupiedNodeId, NodeActivationReason.Occupied);

            // 1-hop adjacent nodes
            if (graph != null)
            {
                var edges = graph.GetConnectedEdges(ctx.OccupiedNodeId);
                for (int i = 0; i < edges.Count; i++)
                {
                    var edge = edges[i];
                    string adjacentId = edge.sourceNodeId == ctx.OccupiedNodeId
                        ? edge.targetNodeId
                        : edge.sourceNodeId;
                    AddReason(adjacentId, NodeActivationReason.Adjacent);
                }
            }

            // Portal-visible destinations
            if (portalResults != null)
            {
                for (int i = 0; i < portalResults.Count; i++)
                {
                    if (portalResults[i].IsVisible)
                        AddReason(portalResults[i].DestinationNodeId, NodeActivationReason.PortalVisible);
                }
            }

            return _result;
        }

        private void AddReason(string nodeId, NodeActivationReason reason)
        {
            if (_result.TryGetValue(nodeId, out var existing))
                _result[nodeId] = existing | reason;
            else
                _result[nodeId] = reason;
        }
    }
}
