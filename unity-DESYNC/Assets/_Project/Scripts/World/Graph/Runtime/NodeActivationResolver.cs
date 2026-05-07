using System.Collections.Generic;
using Desync.World.Graph.Definitions;

namespace Desync.World.Graph.Runtime
{
    public class NodeActivationResolver
    {
        public IReadOnlyDictionary<string, NodeActivationReason> Resolve(
            ViewContext ctx,
            SpatialGraphRuntime graph,
            IReadOnlyList<PortalVisibilityResult> portalResults)
        {
            var result = new Dictionary<string, NodeActivationReason>();

            if (string.IsNullOrEmpty(ctx.OccupiedNodeId))
                return result;

            // Occupied node always active
            AddReason(result, ctx.OccupiedNodeId, NodeActivationReason.Occupied);

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
                    AddReason(result, adjacentId, NodeActivationReason.Adjacent);
                }
            }

            // Portal-visible destinations
            if (portalResults != null)
            {
                for (int i = 0; i < portalResults.Count; i++)
                {
                    if (portalResults[i].IsVisible)
                        AddReason(result, portalResults[i].DestinationNodeId, NodeActivationReason.PortalVisible);
                }
            }

            return result;
        }

        private static void AddReason(Dictionary<string, NodeActivationReason> result,
            string nodeId, NodeActivationReason reason)
        {
            if (result.TryGetValue(nodeId, out var existing))
                result[nodeId] = existing | reason;
            else
                result[nodeId] = reason;
        }
    }
}
