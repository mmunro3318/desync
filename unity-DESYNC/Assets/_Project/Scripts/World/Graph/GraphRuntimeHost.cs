using UnityEngine;
using Desync.World.Graph.Definitions;
using Desync.World.Graph.Runtime;

namespace Desync.World.Graph
{
    /// <summary>
    /// Thin scene host for SpatialGraphRuntime and ObservationLockSystem.
    /// Loads the graph definition on Awake, creates the observation lock
    /// system, and ticks it each frame.
    /// </summary>
    [DefaultExecutionOrder(0)]
    public class GraphRuntimeHost : MonoBehaviour
    {
        [Header("Graph Data")]
        [SerializeField] private HouseGraphDefinition graphDefinition;

        [Header("Observation")]
        [SerializeField] private ObservationRulesDefinition observationRules;

        private SpatialGraphRuntime _runtime;
        private ObservationLockSystem _observationLock;
        private LocalObservationInputSource _observationInput;

        public SpatialGraphRuntime Runtime => _runtime;
        public HouseGraphDefinition Definition => graphDefinition;
        public IObservationLockQuery ObservationLock => _observationLock;
        public LocalObservationInputSource ObservationInput => _observationInput;

        private void Awake()
        {
            if (graphDefinition == null)
            {
                global::UnityEngine.Debug.LogError("[GraphRuntimeHost] No HouseGraphDefinition assigned.", this);
                return;
            }

            var errors = graphDefinition.Validate();
            if (errors.Count > 0)
            {
                foreach (var error in errors)
                    global::UnityEngine.Debug.LogError($"[GraphRuntimeHost] Validation: {error}", this);
                return;
            }

            _runtime = new SpatialGraphRuntime();
            _runtime.Initialize(graphDefinition);
            global::UnityEngine.Debug.Log($"[GraphRuntimeHost] Initialized graph — {_runtime.NodeCount} nodes, {_runtime.EdgeCount} edges");

            InitializeObservation();
        }

        private void InitializeObservation()
        {
            if (observationRules == null)
            {
                global::UnityEngine.Debug.LogWarning("[GraphRuntimeHost] No ObservationRulesDefinition assigned — observation lock disabled.", this);
                return;
            }

            // Tracker binding happens later via BindObservationTracker
            _observationInput = new LocalObservationInputSource(null, _runtime);
            _observationLock = new ObservationLockSystem(_observationInput, _runtime, observationRules);
        }

        private void Update()
        {
            _observationLock?.Tick(Time.deltaTime);
        }

        /// <summary>
        /// Bind the local player's tracker for observation input.
        /// Called from NodeStreamingController or player spawn code.
        /// </summary>
        public void BindObservationTracker(PlayerNodeTracker tracker)
        {
            if (_observationInput == null) return;

            _observationInput = new LocalObservationInputSource(tracker, _runtime);

            if (_observationLock != null && observationRules != null)
            {
                _observationLock.Reset();
                _observationLock = new ObservationLockSystem(_observationInput, _runtime, observationRules);
            }
        }

        /// <summary>
        /// Reset observation state (e.g., on round restart).
        /// </summary>
        public void ResetObservation()
        {
            _observationLock?.Reset();
        }
    }
}
