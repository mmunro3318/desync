using System.Collections.Generic;
using Desync.World.Graph;
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
        private readonly NodeActivationResolver _resolver = new();
        private IReadOnlyDictionary<string, NodeActivationReason> _lastResult;

        public IReadOnlyDictionary<string, NodeActivationReason> LastResult => _lastResult;
        public bool ForceAllActive { get => forceAllActive; set => forceAllActive = value; }

        public void SetHandles(IReadOnlyList<NodePresentationHandle> newHandles)
        {
            handles = new NodePresentationHandle[newHandles.Count];
            for (int i = 0; i < newHandles.Count; i++)
                handles[i] = newHandles[i];
        }

        private void Update()
        {
            if (forceAllActive)
            {
                ActivateAll();
                return;
            }

            if (_playerTracker == null)
                _playerTracker = FindAnyObjectByType<PlayerNodeTracker>();
            if (_playerCamera == null)
                _playerCamera = Camera.main;

            if (_playerTracker == null || string.IsNullOrEmpty(_playerTracker.CurrentNodeId))
                return;

            var ctx = BuildViewContext();
            var portalResults = GetPortalResults(ctx);
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

            // TD0018: Replace stub probes with real PortalViewProbe scene data
            return portalController.EvaluatePortals(ctx, System.Array.Empty<PortalProbeData>());
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
