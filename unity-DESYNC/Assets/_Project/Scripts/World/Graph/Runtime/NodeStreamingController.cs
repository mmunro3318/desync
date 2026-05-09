using System.Collections.Generic;
using Desync.World.Graph;
using Desync.World.Graph.Authoring;
using UnityEngine;

namespace Desync.World.Graph.Runtime
{
    public class NodeStreamingController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GraphRuntimeHost graphHost;
        [SerializeField] private PortalVisibilityController portalController;
        [SerializeField] private NodePresentationHandle[] handles = System.Array.Empty<NodePresentationHandle>();

        [Header("Debug")]
        [SerializeField] private bool forceAllActive;

        private PlayerNodeTracker _playerTracker;
        private Camera _playerCamera;
        private PortalAnchorAuthoring[] _portalAnchors = System.Array.Empty<PortalAnchorAuthoring>();
        private readonly NodeActivationResolver _resolver = new();
        private IReadOnlyDictionary<string, NodeActivationReason> _lastResult;
        private bool _discoveredAtRuntime;

        public IReadOnlyDictionary<string, NodeActivationReason> LastResult => _lastResult;
        public bool ForceAllActive { get => forceAllActive; set => forceAllActive = value; }
        public bool HasLocalPlayer => _playerTracker != null && _playerCamera != null;

        /// <summary>
        /// Binds the local player's tracker and camera. Call from player spawn/bootstrap code.
        /// Pass null to clear (e.g. on disconnect/respawn).
        /// </summary>
        public void BindLocalPlayer(PlayerNodeTracker tracker, Camera cam)
        {
            _playerTracker = tracker;
            _playerCamera = cam;
        }

        public void SetHandles(IReadOnlyList<NodePresentationHandle> newHandles)
        {
            handles = new NodePresentationHandle[newHandles.Count];
            for (int i = 0; i < newHandles.Count; i++)
                handles[i] = newHandles[i];
        }

        private void DiscoverReferences()
        {
            if (_discoveredAtRuntime) return;
            _discoveredAtRuntime = true;

            if (graphHost == null)
                graphHost = FindAnyObjectByType<GraphRuntimeHost>();
            if (portalController == null)
                portalController = FindAnyObjectByType<PortalVisibilityController>();
            if (handles == null || handles.Length == 0)
                handles = FindObjectsByType<NodePresentationHandle>(FindObjectsInactive.Exclude);
            if (_portalAnchors == null || _portalAnchors.Length == 0)
                _portalAnchors = FindObjectsByType<PortalAnchorAuthoring>(FindObjectsInactive.Include);
        }

        private void Update()
        {
            DiscoverReferences();

            if (forceAllActive)
            {
                ActivateAll();
                return;
            }

            if (_playerTracker == null || _playerCamera == null)
                return;

            if (string.IsNullOrEmpty(_playerTracker.CurrentNodeId))
                return;

            var ctx = BuildViewContext();
            var portalResults = GetPortalResults(ctx);

            // Feed portal results to observation lock system before its next Tick
            if (graphHost != null && graphHost.ObservationInput != null)
                graphHost.ObservationInput.SetPortalResults(portalResults);

            UpdatePresentation(ctx, portalResults);
        }

        private ViewContext BuildViewContext()
        {
            var camTransform = _playerCamera != null ? _playerCamera.transform : transform;
            return new ViewContext(
                playerId: "local",
                cameraPosition: camTransform.position,
                cameraForward: camTransform.forward,
                occupiedNodeId: _playerTracker.CurrentNodeId
            );
        }

        private IReadOnlyList<PortalVisibilityResult> GetPortalResults(ViewContext ctx)
        {
            if (portalController == null)
                return System.Array.Empty<PortalVisibilityResult>();

            var graph = graphHost != null ? graphHost.Runtime : null;
            if (graph == null)
                return System.Array.Empty<PortalVisibilityResult>();

            var probes = BuildPortalProbes(_portalAnchors, graph, ctx.OccupiedNodeId);
            return portalController.EvaluatePortals(ctx, probes);
        }

        /// <summary>
        /// Builds PortalProbeData from scene-placed portal anchors by looking up
        /// destination node IDs from graph edges. Only anchors whose anchorId
        /// appears on an edge connected to currentNodeId produce probes.
        /// </summary>
        public static List<PortalProbeData> BuildPortalProbes(
            PortalAnchorAuthoring[] anchors,
            SpatialGraphRuntime graph,
            string currentNodeId)
        {
            var probes = new List<PortalProbeData>();

            if (anchors == null || graph == null || string.IsNullOrEmpty(currentNodeId))
                return probes;

            var edges = graph.GetConnectedEdges(currentNodeId);

            for (int i = 0; i < anchors.Length; i++)
            {
                var anchor = anchors[i];
                if (anchor == null) continue;

                string anchorId = anchor.AnchorId;
                if (string.IsNullOrEmpty(anchorId)) continue;

                // Find the edge that connects this anchor to a destination
                string destinationNodeId = null;
                for (int e = 0; e < edges.Count; e++)
                {
                    var edge = edges[e];
                    if (edge.sourceNodeId == currentNodeId && edge.sourceAnchorId == anchorId)
                    {
                        destinationNodeId = edge.targetNodeId;
                        break;
                    }
                    if (edge.targetNodeId == currentNodeId && edge.targetAnchorId == anchorId)
                    {
                        destinationNodeId = edge.sourceNodeId;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(destinationNodeId))
                    continue;

                var t = anchor.transform;
                probes.Add(new PortalProbeData(
                    anchorId,
                    destinationNodeId,
                    t.position,
                    t.forward,
                    Vector2.one));
            }

            return probes;
        }

        public void UpdatePresentation(ViewContext ctx, IReadOnlyList<PortalVisibilityResult> portalResults)
        {
            var graph = graphHost != null ? graphHost.Runtime : null;
            _lastResult = _resolver.Resolve(ctx, graph, portalResults);

            for (int i = 0; i < handles.Length; i++)
            {
                var handle = handles[i];
                if (handle == null) continue;

                bool shouldBeActive = _lastResult.ContainsKey(handle.NodeId);
                handle.SetPresentation(shouldBeActive);
            }
        }

        private void ActivateAll()
        {
            for (int i = 0; i < handles.Length; i++)
            {
                if (handles[i] != null)
                    handles[i].SetPresentation(true);
            }
        }
    }
}
