using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Desync.World.Graph.Runtime;

namespace Desync.Tests.EditMode.NodeActivation
{
    [TestFixture]
    public class PortalVisibilityControllerTests
    {
        [Test]
        public void EvaluatePortals_WithProbes_ReturnsResults()
        {
            var go = new GameObject("PortalVisController");
            var controller = go.AddComponent<PortalVisibilityController>();

            var probes = new List<PortalProbeData>
            {
                new PortalProbeData("anchor_1", "hall_a", Vector3.zero, Vector3.forward, new Vector2(1f, 2f))
            };

            var ctx = new ViewContext("p1", Vector3.zero, Vector3.forward, "entry");
            var results = controller.EvaluatePortals(ctx, probes);

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("anchor_1", results[0].AnchorId);
            Assert.AreEqual("hall_a", results[0].DestinationNodeId);
            // Gate 0 stub: always returns visible
            Assert.IsTrue(results[0].IsVisible);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void EvaluatePortals_EmptyProbes_ReturnsEmpty()
        {
            var go = new GameObject("PortalVisController");
            var controller = go.AddComponent<PortalVisibilityController>();

            var ctx = new ViewContext("p1", Vector3.zero, Vector3.forward, "entry");
            var results = controller.EvaluatePortals(ctx, new List<PortalProbeData>());

            Assert.AreEqual(0, results.Count);

            Object.DestroyImmediate(go);
        }
    }
}
