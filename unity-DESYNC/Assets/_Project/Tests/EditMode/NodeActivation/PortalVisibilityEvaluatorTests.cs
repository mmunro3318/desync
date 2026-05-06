using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Desync.World.Graph.Runtime;

namespace Desync.Tests.EditMode.NodeActivation
{
    [TestFixture]
    public class PortalVisibilityEvaluatorTests
    {
        private PortalVisibilityEvaluator _evaluator;

        [SetUp]
        public void SetUp()
        {
            _evaluator = new PortalVisibilityEvaluator(dotThreshold: 0.5f);
        }

        [Test]
        public void Evaluate_FacingPortal_ReturnsVisible()
        {
            // Camera at origin looking forward (+Z), portal ahead at (0,0,5) facing back (-Z)
            var ctx = new ViewContext("p1", Vector3.zero, Vector3.forward, "entry");
            var probes = new List<PortalProbeData>
            {
                new PortalProbeData("a1", "hall_a", new Vector3(0, 0, 5), -Vector3.forward, new Vector2(1, 2))
            };

            var results = _evaluator.Evaluate(ctx, probes);

            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(results[0].IsVisible);
            Assert.AreEqual("hall_a", results[0].DestinationNodeId);
        }

        [Test]
        public void Evaluate_FacingAway_ReturnsNotVisible()
        {
            // Camera at origin looking backward (-Z), portal ahead at (0,0,5)
            var ctx = new ViewContext("p1", Vector3.zero, -Vector3.forward, "entry");
            var probes = new List<PortalProbeData>
            {
                new PortalProbeData("a1", "hall_a", new Vector3(0, 0, 5), -Vector3.forward, new Vector2(1, 2))
            };

            var results = _evaluator.Evaluate(ctx, probes);

            Assert.AreEqual(1, results.Count);
            Assert.IsFalse(results[0].IsVisible);
        }

        [Test]
        public void Evaluate_WithinThresholdAngle_ReturnsVisible()
        {
            // 59 degrees is within the 60-degree (0.5 dot) cone
            float angle = 59f * Mathf.Deg2Rad;
            var forward = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)).normalized;
            var ctx = new ViewContext("p1", Vector3.zero, forward, "entry");
            var probes = new List<PortalProbeData>
            {
                new PortalProbeData("a1", "hall_a", new Vector3(0, 0, 5), -Vector3.forward, new Vector2(1, 2))
            };

            var results = _evaluator.Evaluate(ctx, probes);

            Assert.IsTrue(results[0].IsVisible);
        }

        [Test]
        public void Evaluate_JustPastThreshold_ReturnsNotVisible()
        {
            // Slightly past 60 degrees — dot < 0.5
            float angle = 65f * Mathf.Deg2Rad;
            var forward = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)).normalized;
            var ctx = new ViewContext("p1", Vector3.zero, forward, "entry");
            var probes = new List<PortalProbeData>
            {
                new PortalProbeData("a1", "hall_a", new Vector3(0, 0, 5), -Vector3.forward, new Vector2(1, 2))
            };

            var results = _evaluator.Evaluate(ctx, probes);

            Assert.IsFalse(results[0].IsVisible);
        }

        [Test]
        public void Evaluate_PlayerPastPortalPlane_AlwaysVisible()
        {
            // Portal-crossing guard: player has crossed past the portal plane.
            // Portal at (0,0,5) facing -Z. Player at (0,0,6) = past the portal plane.
            // Even if camera faces away, destination must stay active.
            var ctx = new ViewContext("p1", new Vector3(0, 0, 6), -Vector3.forward, "entry");
            var probes = new List<PortalProbeData>
            {
                new PortalProbeData("a1", "hall_a", new Vector3(0, 0, 5), -Vector3.forward, new Vector2(1, 2))
            };

            var results = _evaluator.Evaluate(ctx, probes);

            Assert.IsTrue(results[0].IsVisible, "Player past portal plane should always see destination");
        }

        [Test]
        public void Evaluate_PlayerBeforePortalPlane_FacingAway_NotVisible()
        {
            // Player at (0,0,3) = before portal plane at (0,0,5) facing -Z.
            // Facing away. Normal dot-product rules apply — not visible.
            var ctx = new ViewContext("p1", new Vector3(0, 0, 3), -Vector3.forward, "entry");
            var probes = new List<PortalProbeData>
            {
                new PortalProbeData("a1", "hall_a", new Vector3(0, 0, 5), -Vector3.forward, new Vector2(1, 2))
            };

            var results = _evaluator.Evaluate(ctx, probes);

            Assert.IsFalse(results[0].IsVisible);
        }

        [Test]
        public void Evaluate_EmptyProbes_ReturnsEmpty()
        {
            var ctx = new ViewContext("p1", Vector3.zero, Vector3.forward, "entry");

            var results = _evaluator.Evaluate(ctx, new List<PortalProbeData>());

            Assert.AreEqual(0, results.Count);
        }

        [Test]
        public void Evaluate_MultipleProbes_EvaluatedIndependently()
        {
            var ctx = new ViewContext("p1", Vector3.zero, Vector3.forward, "entry");
            var probes = new List<PortalProbeData>
            {
                // Portal ahead — visible
                new PortalProbeData("a1", "hall_a", new Vector3(0, 0, 5), -Vector3.forward, new Vector2(1, 2)),
                // Portal behind — not visible
                new PortalProbeData("a2", "kitchen", new Vector3(0, 0, -5), Vector3.forward, new Vector2(1, 2))
            };

            var results = _evaluator.Evaluate(ctx, probes);

            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results[0].IsVisible, "Portal ahead should be visible");
            Assert.IsFalse(results[1].IsVisible, "Portal behind should not be visible");
        }
    }
}
