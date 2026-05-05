using NUnit.Framework;
using UnityEngine;
using Desync.World.Graph.Debug;

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
                new System.Collections.Generic.Dictionary<string, Desync.World.Graph.Runtime.NodeActivationReason>()));

            Object.DestroyImmediate(go);
        }
    }
}
