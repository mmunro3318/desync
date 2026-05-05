using System.Collections.Generic;
using UnityEngine;

namespace Desync.World.Graph.Runtime
{
    public class NodeStreamingController : MonoBehaviour
    {
        private NodePresentationHandle[] _handles = System.Array.Empty<NodePresentationHandle>();
        private readonly HashSet<string> _activeNodeIds = new();

        public void SetHandles(IReadOnlyList<NodePresentationHandle> handles)
        {
            _handles = new NodePresentationHandle[handles.Count];
            for (int i = 0; i < handles.Count; i++)
                _handles[i] = handles[i];
        }

        public void UpdatePresentation(ViewContext ctx, IReadOnlyList<PortalVisibilityResult> portalResults)
        {
            _activeNodeIds.Clear();

            // Occupied node is always active
            if (!string.IsNullOrEmpty(ctx.OccupiedNodeId))
                _activeNodeIds.Add(ctx.OccupiedNodeId);

            // Portal-visible destinations are active
            for (int i = 0; i < portalResults.Count; i++)
            {
                if (portalResults[i].IsVisible)
                    _activeNodeIds.Add(portalResults[i].DestinationNodeId);
            }

            // Toggle presentation handles
            for (int i = 0; i < _handles.Length; i++)
            {
                var handle = _handles[i];
                if (handle == null) continue;

                bool shouldBeActive = _activeNodeIds.Contains(handle.NodeId);
                handle.SetPresentation(shouldBeActive);
            }
        }
    }
}
