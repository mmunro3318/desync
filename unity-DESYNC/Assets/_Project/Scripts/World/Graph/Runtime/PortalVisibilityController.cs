using System.Collections.Generic;
using UnityEngine;

namespace Desync.World.Graph.Runtime
{
    public class PortalVisibilityController : MonoBehaviour
    {
        [Header("Tuning")]
        [Tooltip("Dot product threshold for portal visibility (0.5 = 60-degree cone)")]
        [SerializeField] private float dotThreshold = 0.5f;

        private PortalVisibilityEvaluator _evaluator;

        public IReadOnlyList<PortalVisibilityResult> EvaluatePortals(
            ViewContext ctx,
            IReadOnlyList<PortalProbeData> probes)
        {
            if (_evaluator == null)
                _evaluator = new PortalVisibilityEvaluator(dotThreshold);

            return _evaluator.Evaluate(ctx, probes);
        }
    }
}
