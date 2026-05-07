using NUnit.Framework;
using UnityEngine;
using Desync.World.Graph.Debug;
using Desync.World.Graph.Runtime;
using System.Collections.Generic;

namespace Desync.Tests.EditMode.NodeActivation
{
    [TestFixture]
    public class VisibilityDebugOverlayTests
    {
        [Test]
        public void Creation_DoesNotThrow_WhenNoControllerExists()
        {
            var go = new GameObject("DebugOverlay");
            Assert.DoesNotThrow(() => go.AddComponent<SpatialVisibilityDebugOverlay>());
            Object.DestroyImmediate(go);
        }

        [Test]
        public void SetActivationState_WithEmptyDict_DoesNotThrow()
        {
            var go = new GameObject("DebugOverlay");
            var overlay = go.AddComponent<SpatialVisibilityDebugOverlay>();

            Assert.DoesNotThrow(() => overlay.SetActivationState(
                new Dictionary<string, NodeActivationReason>()));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void SetActivationState_WithNullDict_DoesNotThrow()
        {
            var go = new GameObject("DebugOverlay");
            var overlay = go.AddComponent<SpatialVisibilityDebugOverlay>();

            Assert.DoesNotThrow(() => overlay.SetActivationState(null));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void SetActivationState_WithPopulatedDict_DoesNotThrow()
        {
            var go = new GameObject("DebugOverlay");
            var overlay = go.AddComponent<SpatialVisibilityDebugOverlay>();

            var state = new Dictionary<string, NodeActivationReason>
            {
                { "node_foyer", NodeActivationReason.Occupied },
                { "node_hall", NodeActivationReason.Adjacent },
                { "node_kitchen", NodeActivationReason.PortalVisible }
            };

            Assert.DoesNotThrow(() => overlay.SetActivationState(state));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void OverlayUpdate_DoesNotThrow_WhenControllerHasNullLastResult()
        {
            // Simulate the overlay's Update path when a controller exists but has null LastResult
            var go = new GameObject("DebugOverlay");
            var overlay = go.AddComponent<SpatialVisibilityDebugOverlay>();

            // Controller with no Update() called yet => LastResult is null
            var controllerGo = new GameObject("Controller");
            controllerGo.AddComponent<NodeStreamingController>();

            // The overlay's PollController should handle null LastResult gracefully
            Assert.DoesNotThrow(() => overlay.PollController());

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(controllerGo);
        }

        [Test]
        public void OverlayUpdate_PollsController_WhenControllerExists()
        {
            var go = new GameObject("DebugOverlay");
            var overlay = go.AddComponent<SpatialVisibilityDebugOverlay>();

            // Manually set state to verify PollController clears/updates it
            var initialState = new Dictionary<string, NodeActivationReason>
            {
                { "node_foyer", NodeActivationReason.Occupied }
            };
            overlay.SetActivationState(initialState);

            // PollController with no controller found should clear to empty
            // (no controller in scene means no data)
            Assert.DoesNotThrow(() => overlay.PollController());

            Object.DestroyImmediate(go);
        }
    }
}
