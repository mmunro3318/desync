using NUnit.Framework;
using UnityEngine;
using Desync.World.Graph.Definitions;
using Desync.World.Graph.Runtime;

namespace Desync.Tests.EditMode
{
    public class PortalResolverTests
    {
        private HouseGraphDefinition _definition;
        private SpatialGraphRuntime _runtime;
        private PortalResolver _resolver;

        [SetUp]
        public void SetUp()
        {
            _definition = ScriptableObject.CreateInstance<HouseGraphDefinition>();
            _definition.nodes = new[]
            {
                new HouseNodeDefinition
                {
                    nodeId = "entry",
                    displayName = "Entry",
                    portalAnchors = new[]
                    {
                        new PortalAnchorDefinition { anchorId = "door_a", localPosition = new Vector3(3f, 0f, 0f), localRotation = Quaternion.identity }
                    }
                },
                new HouseNodeDefinition
                {
                    nodeId = "hall",
                    displayName = "Hall",
                    portalAnchors = new[]
                    {
                        new PortalAnchorDefinition { anchorId = "door_b", localPosition = new Vector3(-3f, 0f, 0f), localRotation = Quaternion.identity },
                        new PortalAnchorDefinition { anchorId = "door_c", localPosition = new Vector3(3f, 0f, 0f), localRotation = Quaternion.identity }
                    }
                },
                new HouseNodeDefinition
                {
                    nodeId = "living",
                    displayName = "Living Room",
                    portalAnchors = new[]
                    {
                        new PortalAnchorDefinition { anchorId = "door_d", localPosition = new Vector3(-3f, 0f, 0f), localRotation = Quaternion.identity }
                    }
                }
            };
            _definition.edges = new[]
            {
                new HouseEdgeDefinition { edgeId = "e1", sourceNodeId = "entry", targetNodeId = "hall", sourceAnchorId = "door_a", targetAnchorId = "door_b" },
                new HouseEdgeDefinition { edgeId = "e2", sourceNodeId = "hall", targetNodeId = "living", sourceAnchorId = "door_c", targetAnchorId = "door_d" }
            };

            _runtime = new SpatialGraphRuntime();
            _runtime.Initialize(_definition);

            _resolver = new PortalResolver(_runtime);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_definition);
        }

        [Test]
        public void Resolve_ValidEdgeFromSource_ReturnsDestination()
        {
            bool found = _resolver.Resolve("e1", "entry", out var result);

            Assert.IsTrue(found);
            Assert.AreEqual("hall", result.destinationNodeId);
            Assert.AreEqual("door_b", result.destinationAnchorId);
        }

        [Test]
        public void Resolve_ValidEdgeFromTarget_ReturnsSource()
        {
            bool found = _resolver.Resolve("e1", "hall", out var result);

            Assert.IsTrue(found);
            Assert.AreEqual("entry", result.destinationNodeId);
            Assert.AreEqual("door_a", result.destinationAnchorId);
        }

        [Test]
        public void Resolve_InvalidEdge_ReturnsFalse()
        {
            bool found = _resolver.Resolve("nonexistent", "entry", out _);
            Assert.IsFalse(found);
        }

        [Test]
        public void Resolve_NodeNotOnEdge_ReturnsFalse()
        {
            bool found = _resolver.Resolve("e1", "living", out _);
            Assert.IsFalse(found);
        }

        [Test]
        public void Resolve_ChainedTraversal_WorksCorrectly()
        {
            // Walk entry -> hall via e1, then hall -> living via e2
            _resolver.Resolve("e1", "entry", out var first);
            Assert.AreEqual("hall", first.destinationNodeId);

            _resolver.Resolve("e2", first.destinationNodeId, out var second);
            Assert.AreEqual("living", second.destinationNodeId);
        }
    }
}
