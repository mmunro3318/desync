using System.Collections.Generic;
using UnityEngine;

namespace Desync.World.Graph.Runtime
{
    public class PortalVisibilityEvaluator : IPortalVisibilityEvaluator
    {
        private readonly float _dotThreshold;

        public PortalVisibilityEvaluator(float dotThreshold = 0.5f)
        {
            _dotThreshold = dotThreshold;
        }

        public IReadOnlyList<PortalVisibilityResult> Evaluate(
            ViewContext ctx,
            IReadOnlyList<PortalProbeData> probes)
        {
            var results = new List<PortalVisibilityResult>(probes.Count);

            for (int i = 0; i < probes.Count; i++)
            {
                bool visible = EvaluateSingle(ctx, probes[i]);
                results.Add(new PortalVisibilityResult(
                    probes[i].AnchorId,
                    probes[i].DestinationNodeId,
                    visible));
            }

            return results;
        }

        private bool EvaluateSingle(ViewContext ctx, PortalProbeData probe)
        {
            // Portal-crossing guard: if player is past the portal plane,
            // always keep destination visible to prevent mid-crossing deactivation.
            // PRD formula: dot(playerPos - portalPos, portalForward) < 0
            var portalToPlayer = ctx.CameraPosition - probe.PortalPosition;
            float planeDot = Vector3.Dot(portalToPlayer, probe.PortalForward);
            if (planeDot < 0f)
                return true;

            // Dot product: camera forward vs direction to portal
            var directionToPortal = (probe.PortalPosition - ctx.CameraPosition).normalized;
            float facingDot = Vector3.Dot(ctx.CameraForward, directionToPortal);
            return facingDot >= _dotThreshold;
        }
    }
}
