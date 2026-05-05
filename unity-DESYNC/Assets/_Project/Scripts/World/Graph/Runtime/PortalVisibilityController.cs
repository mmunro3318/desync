using System.Collections.Generic;
using UnityEngine;

namespace Desync.World.Graph.Runtime
{
    public class PortalVisibilityController : MonoBehaviour
    {
        private readonly List<PortalVisibilityResult> _results = new();

        /// <summary>
        /// Gate 0 stub: always returns all probes as visible.
        /// TB-4 replaces this with real camera-facing evaluation.
        /// </summary>
        public IReadOnlyList<PortalVisibilityResult> EvaluatePortals(
            ViewContext ctx,
            IReadOnlyList<PortalProbeData> probes)
        {
            _results.Clear();

            for (int i = 0; i < probes.Count; i++)
            {
                _results.Add(new PortalVisibilityResult(
                    probes[i].AnchorId,
                    probes[i].DestinationNodeId,
                    isVisible: true // Stub: always visible
                ));
            }

            return _results;
        }
    }
}
