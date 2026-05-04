using UnityEngine;
using Desync.World.Graph.Definitions;
using Desync.World.Graph.Runtime;

namespace Desync.World.Graph
{
    /// <summary>
    /// Thin scene host for SpatialGraphRuntime. Place on a GameObject in the
    /// gameplay scene. Loads the graph definition on Awake and exposes the
    /// runtime for other systems to query.
    /// </summary>
    public class GraphRuntimeHost : MonoBehaviour
    {
        [Header("Graph Data")]
        [SerializeField] private HouseGraphDefinition graphDefinition;

        private SpatialGraphRuntime _runtime;

        public SpatialGraphRuntime Runtime => _runtime;
        public HouseGraphDefinition Definition => graphDefinition;

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
        }
    }
}
