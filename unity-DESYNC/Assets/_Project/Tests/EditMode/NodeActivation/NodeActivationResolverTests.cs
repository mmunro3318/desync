using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Desync.World.Graph.Definitions;
using Desync.World.Graph.Runtime;

namespace Desync.Tests.EditMode.NodeActivation
{
    [TestFixture]
    public class NodeActivationResolverTests
    {
        private SpatialGraphRuntime _graph;
        private NodeActivationResolver _resolver;

        [SetUp]
        public void SetUp()
        {
            _resolver = new NodeActivationResolver();
            _graph = new SpatialGraphRuntime();
        }

        private HouseGraphDefinition CreateSimpleGraph()
        {
            // 3 nodes: entry -- hall_a -- living (linear chain)
            var def = ScriptableObject.CreateInstance<HouseGraphDefinition>();
            def.nodes = new[]
            {
                new HouseNodeDefinition { nodeId = "entry" },
                new HouseNodeDefinition { nodeId = "hall_a" },
                new HouseNodeDefinition { nodeId = "living" }
            };
            def.edges = new[]
            {
                new HouseEdgeDefinition { edgeId = "e1", sourceNodeId = "entry", targetNodeId = "hall_a" },
                new HouseEdgeDefinition { edgeId = "e2", sourceNodeId = "hall_a", targetNodeId = "living" }
            };
            return def;
        }

        [Test]
        public void Resolve_OccupiedNode_GetsOccupiedFlag()
        {
            var def = CreateSimpleGraph();
            _graph.Initialize(def);

            var ctx = new ViewContext("p1", Vector3.zero, Vector3.forward, "entry");
            var result = _resolver.Resolve(ctx, _graph, new List<PortalVisibilityResult>());

            Assert.IsTrue(result.ContainsKey("entry"));
            Assert.IsTrue(result["entry"].HasFlag(NodeActivationReason.Occupied));

            Object.DestroyImmediate(def);
        }

        [Test]
        public void Resolve_AdjacentNodes_GetAdjacentFlag()
        {
            var def = CreateSimpleGraph();
            _graph.Initialize(def);

            var ctx = new ViewContext("p1", Vector3.zero, Vector3.forward, "entry");
            var result = _resolver.Resolve(ctx, _graph, new List<PortalVisibilityResult>());

            // hall_a is 1-hop from entry
            Assert.IsTrue(result.ContainsKey("hall_a"));
            Assert.IsTrue(result["hall_a"].HasFlag(NodeActivationReason.Adjacent));
            // living is 2-hop — should NOT be in result
            Assert.IsFalse(result.ContainsKey("living"));

            Object.DestroyImmediate(def);
        }

        [Test]
        public void Resolve_PortalVisibleDestination_GetsPortalVisibleFlag()
        {
            var def = CreateSimpleGraph();
            _graph.Initialize(def);

            var ctx = new ViewContext("p1", Vector3.zero, Vector3.forward, "entry");
            var portalResults = new List<PortalVisibilityResult>
            {
                new PortalVisibilityResult("a1", "hall_a", true)
            };

            var result = _resolver.Resolve(ctx, _graph, portalResults);

            Assert.IsTrue(result["hall_a"].HasFlag(NodeActivationReason.PortalVisible));

            Object.DestroyImmediate(def);
        }

        [Test]
        public void Resolve_SameNode_AccumulatesFlags()
        {
            var def = CreateSimpleGraph();
            _graph.Initialize(def);

            // Standing in entry, hall_a is both adjacent AND portal-visible
            var ctx = new ViewContext("p1", Vector3.zero, Vector3.forward, "entry");
            var portalResults = new List<PortalVisibilityResult>
            {
                new PortalVisibilityResult("a1", "hall_a", true)
            };

            var result = _resolver.Resolve(ctx, _graph, portalResults);

            var hallReasons = result["hall_a"];
            Assert.IsTrue(hallReasons.HasFlag(NodeActivationReason.Adjacent));
            Assert.IsTrue(hallReasons.HasFlag(NodeActivationReason.PortalVisible));

            Object.DestroyImmediate(def);
        }

        [Test]
        public void Resolve_NonVisiblePortal_DoesNotAddFlag()
        {
            var def = CreateSimpleGraph();
            _graph.Initialize(def);

            var ctx = new ViewContext("p1", Vector3.zero, Vector3.forward, "entry");
            var portalResults = new List<PortalVisibilityResult>
            {
                new PortalVisibilityResult("a1", "living", false) // NOT visible
            };

            var result = _resolver.Resolve(ctx, _graph, portalResults);

            // living is 2-hop and not portal-visible, should not appear
            Assert.IsFalse(result.ContainsKey("living"));

            Object.DestroyImmediate(def);
        }

        [Test]
        public void Resolve_NullGraph_ReturnsOccupiedOnly()
        {
            var ctx = new ViewContext("p1", Vector3.zero, Vector3.forward, "entry");
            var result = _resolver.Resolve(ctx, null, new List<PortalVisibilityResult>());

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.ContainsKey("entry"));
            Assert.IsTrue(result["entry"].HasFlag(NodeActivationReason.Occupied));
        }

        [Test]
        public void Resolve_EmptyOccupiedNodeId_ReturnsEmptyDict()
        {
            var ctx = new ViewContext("p1", Vector3.zero, Vector3.forward, "");
            var result = _resolver.Resolve(ctx, _graph, new List<PortalVisibilityResult>());

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void Resolve_SecondCall_DoesNotCorruptFirstResult()
        {
            var def = CreateSimpleGraph();
            _graph.Initialize(def);

            var ctx1 = new ViewContext("p1", Vector3.zero, Vector3.forward, "entry");
            var result1 = _resolver.Resolve(ctx1, _graph, new List<PortalVisibilityResult>());

            // result1: entry (Occupied) + hall_a (Adjacent)
            Assert.AreEqual(2, result1.Count);
            Assert.IsTrue(result1.ContainsKey("entry"));

            var ctx2 = new ViewContext("p1", Vector3.zero, Vector3.forward, "living");
            _resolver.Resolve(ctx2, _graph, new List<PortalVisibilityResult>());

            // result1 must still hold its original data
            Assert.AreEqual(2, result1.Count, "First result corrupted by second Resolve call");
            Assert.IsTrue(result1.ContainsKey("entry"), "First result lost 'entry' key after second Resolve");
            Assert.IsTrue(result1.ContainsKey("hall_a"), "First result lost 'hall_a' key after second Resolve");

            Object.DestroyImmediate(def);
        }
    }
}
