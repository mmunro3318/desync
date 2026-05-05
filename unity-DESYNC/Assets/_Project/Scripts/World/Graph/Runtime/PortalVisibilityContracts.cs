using System.Collections.Generic;
using UnityEngine;

namespace Desync.World.Graph.Runtime
{
    public readonly struct PortalProbeData
    {
        public readonly string AnchorId;
        public readonly string DestinationNodeId;
        public readonly Vector3 PortalPosition;
        public readonly Vector3 PortalForward;
        public readonly Vector2 ApertureSize;

        public PortalProbeData(string anchorId, string destinationNodeId, Vector3 portalPosition, Vector3 portalForward, Vector2 apertureSize)
        {
            AnchorId = anchorId;
            DestinationNodeId = destinationNodeId;
            PortalPosition = portalPosition;
            PortalForward = portalForward;
            ApertureSize = apertureSize;
        }
    }

    public readonly struct PortalVisibilityResult
    {
        public readonly string AnchorId;
        public readonly string DestinationNodeId;
        public readonly bool IsVisible;

        public PortalVisibilityResult(string anchorId, string destinationNodeId, bool isVisible)
        {
            AnchorId = anchorId;
            DestinationNodeId = destinationNodeId;
            IsVisible = isVisible;
        }
    }

    public interface IPortalVisibilityEvaluator
    {
        IReadOnlyList<PortalVisibilityResult> Evaluate(ViewContext ctx, IReadOnlyList<PortalProbeData> probes);
    }
}
