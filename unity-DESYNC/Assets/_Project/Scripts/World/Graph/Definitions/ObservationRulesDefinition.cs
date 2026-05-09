using UnityEngine;

namespace Desync.World.Graph.Definitions
{
    /// <summary>
    /// Tunable observation lock parameters. nodeGraceSeconds and edgeGraceSeconds
    /// control how long a target stays locked after the last observation reason
    /// clears. visibilityRefreshInterval controls how often visibility inputs
    /// are re-polled (0 = every frame).
    /// </summary>
    [CreateAssetMenu(fileName = "ObservationRulesDefinition", menuName = "Desync/Observation Rules")]
    public class ObservationRulesDefinition : ScriptableObject
    {
        [Header("Grace Timers")]
        [Tooltip("Seconds a node stays locked after the last observation reason clears")]
        public float nodeGraceSeconds = 2.0f;

        [Tooltip("Seconds an edge stays locked after the last observation reason clears")]
        public float edgeGraceSeconds = 1.5f;

        [Header("Visibility Polling")]
        [Tooltip("Seconds between visibility input re-evaluation. 0 = every frame.")]
        public float visibilityRefreshInterval = 0f;

        [Header("Debug")]
        [Tooltip("Log verbose observation lock state changes to console")]
        public bool lockDebugVerbose;
    }
}
