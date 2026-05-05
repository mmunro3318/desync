namespace Desync.World.Graph.Runtime
{
    /// <summary>
    /// Resolves portal traversals: given an edge and the node you're in,
    /// returns the destination node and anchor on the other side.
    /// This is a pure topology query — it does NOT check RuntimeEdgeState.IsOpen.
    /// Traversal gating (can the player actually walk through?) belongs in a
    /// higher-level system that consults both this resolver and edge state.
    /// </summary>
    public class PortalResolver
    {
        private readonly SpatialGraphRuntime _runtime;

        public PortalResolver(SpatialGraphRuntime runtime)
        {
            _runtime = runtime;
        }

        public bool Resolve(string edgeId, string currentNodeId, out PortalResolution result)
        {
            result = default;

            if (!_runtime.GetEdge(edgeId, out var edge))
                return false;

            if (!_runtime.GetDestinationNode(edgeId, currentNodeId, out var destNodeId))
                return false;

            // The destination anchor is the anchor on the destination side
            string destAnchorId = (edge.sourceNodeId == currentNodeId)
                ? edge.targetAnchorId
                : edge.sourceAnchorId;

            result = new PortalResolution
            {
                destinationNodeId = destNodeId,
                destinationAnchorId = destAnchorId
            };

            return true;
        }
    }

    public struct PortalResolution
    {
        public string destinationNodeId;
        public string destinationAnchorId;
    }
}
