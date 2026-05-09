using System.Collections.Generic;

namespace Desync.World.Graph.Runtime
{
    /// <summary>
    /// Single-player local adapter for IObservationInputSource. Polls
    /// PlayerNodeTracker for occupancy and derives visibility from
    /// pre-set portal results and graph edge queries.
    ///
    /// This is a Sprint 2 local adapter, not the future co-op authority model.
    /// In co-op, observation truth moves to an authority-owned aggregation model.
    /// This interface exists to make that replacement possible without rewriting
    /// ObservationLockSystem.
    /// </summary>
    public class LocalObservationInputSource : IObservationInputSource
    {
        private readonly PlayerNodeTracker _tracker;
        private readonly SpatialGraphRuntime _graph;

        private IReadOnlyList<PortalVisibilityResult> _portalResults =
            System.Array.Empty<PortalVisibilityResult>();

        private readonly List<string> _occupiedCache = new(1);
        private readonly List<string> _visibleNodesCache = new(4);
        private readonly List<string> _visibleEdgesCache = new(4);

        public LocalObservationInputSource(
            PlayerNodeTracker tracker,
            SpatialGraphRuntime graph)
        {
            _tracker = tracker;
            _graph = graph;
        }

        /// <summary>
        /// Inject portal visibility results from the presentation system.
        /// Called each frame by the host before ObservationLockSystem.Tick().
        /// </summary>
        public void SetPortalResults(IReadOnlyList<PortalVisibilityResult> results)
        {
            _portalResults = results ?? System.Array.Empty<PortalVisibilityResult>();
        }

        public IReadOnlyList<string> GetOccupiedNodeIds()
        {
            _occupiedCache.Clear();

            string currentNodeId = _tracker != null ? _tracker.CurrentNodeId : null;
            if (!string.IsNullOrEmpty(currentNodeId))
                _occupiedCache.Add(currentNodeId);

            return _occupiedCache;
        }

        public IReadOnlyList<string> GetVisibleNodeIds()
        {
            _visibleNodesCache.Clear();

            for (int i = 0; i < _portalResults.Count; i++)
            {
                var result = _portalResults[i];
                if (result.IsVisible && !string.IsNullOrEmpty(result.DestinationNodeId))
                    _visibleNodesCache.Add(result.DestinationNodeId);
            }

            return _visibleNodesCache;
        }

        public IReadOnlyList<string> GetVisibleEdgeIds()
        {
            _visibleEdgesCache.Clear();

            string currentNodeId = _tracker != null ? _tracker.CurrentNodeId : null;
            if (string.IsNullOrEmpty(currentNodeId) || _graph == null)
                return _visibleEdgesCache;

            // Derive visible edges: for each visible destination, find the edge
            // connecting current node to that destination.
            // Sprint 2 approximation — future work may use explicit portal/edge identity.
            var edges = _graph.GetConnectedEdges(currentNodeId);
            var visibleNodes = GetVisibleNodeIds();

            for (int v = 0; v < visibleNodes.Count; v++)
            {
                var destNodeId = visibleNodes[v];
                for (int e = 0; e < edges.Count; e++)
                {
                    var edge = edges[e];
                    string otherNode = edge.sourceNodeId == currentNodeId
                        ? edge.targetNodeId
                        : edge.sourceNodeId;

                    if (otherNode == destNodeId)
                    {
                        _visibleEdgesCache.Add(edge.edgeId);
                        break;
                    }
                }
            }

            return _visibleEdgesCache;
        }
    }
}
