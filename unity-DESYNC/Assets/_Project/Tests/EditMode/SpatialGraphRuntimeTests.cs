using NUnit.Framework;
using UnityEngine;
using Desync.World.Graph.Definitions;
using Desync.World.Graph.Runtime;

namespace Desync.Tests.EditMode
{
    public class SpatialGraphRuntimeTests
    {
        private HouseGraphDefinition _definition;
        private SpatialGraphRuntime _runtime;

        [SetUp]
        public void SetUp()
        {
            _definition = ScriptableObject.CreateInstance<HouseGraphDefinition>();
            _definition.nodes = new[]
            {
                new HouseNodeDefinition
                {
                    nodeId = "entry",
                    displayName = "Entry Hall",
                    worldPosition = Vector3.zero,
                    portalAnchors = new[]
                    {
                        new PortalAnchorDefinition { anchorId = "door_a", localPosition = new Vector3(2f, 0f, 0f) }
                    }
                },
                new HouseNodeDefinition
                {
                    nodeId = "hall_a",
                    displayName = "Hallway A",
                    worldPosition = new Vector3(5f, 0f, 0f),
                    portalAnchors = new[]
                    {
                        new PortalAnchorDefinition { anchorId = "door_b", localPosition = new Vector3(-2f, 0f, 0f) },
                        new PortalAnchorDefinition { anchorId = "door_c", localPosition = new Vector3(2f, 0f, 0f) }
                    }
                },
                new HouseNodeDefinition
                {
                    nodeId = "living",
                    displayName = "Living Room",
                    worldPosition = new Vector3(10f, 0f, 0f),
                    portalAnchors = new[]
                    {
                        new PortalAnchorDefinition { anchorId = "door_d", localPosition = new Vector3(-2f, 0f, 0f) }
                    }
                }
            };
            _definition.edges = new[]
            {
                new HouseEdgeDefinition
                {
                    edgeId = "entry_to_hall",
                    sourceNodeId = "entry",
                    targetNodeId = "hall_a",
                    sourceAnchorId = "door_a",
                    targetAnchorId = "door_b"
                },
                new HouseEdgeDefinition
                {
                    edgeId = "hall_to_living",
                    sourceNodeId = "hall_a",
                    targetNodeId = "living",
                    sourceAnchorId = "door_c",
                    targetAnchorId = "door_d"
                }
            };

            _runtime = new SpatialGraphRuntime();
            _runtime.Initialize(_definition);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_definition);
        }

        // --- GetNode ---

        [Test]
        public void GetNode_ValidId_ReturnsTrue()
        {
            bool found = _runtime.GetNode("entry", out var node);
            Assert.IsTrue(found);
            Assert.AreEqual("entry", node.nodeId);
            Assert.AreEqual("Entry Hall", node.displayName);
        }

        [Test]
        public void GetNode_InvalidId_ReturnsFalse()
        {
            bool found = _runtime.GetNode("nonexistent", out _);
            Assert.IsFalse(found);
        }

        // --- GetEdge ---

        [Test]
        public void GetEdge_ValidId_ReturnsTrue()
        {
            bool found = _runtime.GetEdge("entry_to_hall", out var edge);
            Assert.IsTrue(found);
            Assert.AreEqual("entry", edge.sourceNodeId);
            Assert.AreEqual("hall_a", edge.targetNodeId);
        }

        [Test]
        public void GetEdge_InvalidId_ReturnsFalse()
        {
            bool found = _runtime.GetEdge("nonexistent", out _);
            Assert.IsFalse(found);
        }

        // --- GetConnectedEdges ---

        [Test]
        public void GetConnectedEdges_ReturnsAllEdgesTouchingNode()
        {
            var edges = _runtime.GetConnectedEdges("hall_a");
            Assert.AreEqual(2, edges.Count);
        }

        [Test]
        public void GetConnectedEdges_LeafNode_ReturnsSingleEdge()
        {
            var edges = _runtime.GetConnectedEdges("entry");
            Assert.AreEqual(1, edges.Count);
            Assert.AreEqual("entry_to_hall", edges[0].edgeId);
        }

        [Test]
        public void GetConnectedEdges_InvalidNode_ReturnsEmptyList()
        {
            var edges = _runtime.GetConnectedEdges("nonexistent");
            Assert.AreEqual(0, edges.Count);
        }

        // --- GetDestinationNode ---

        [Test]
        public void GetDestinationNode_FromSource_ReturnsTarget()
        {
            bool found = _runtime.GetDestinationNode("entry_to_hall", "entry", out var destId);
            Assert.IsTrue(found);
            Assert.AreEqual("hall_a", destId);
        }

        [Test]
        public void GetDestinationNode_FromTarget_ReturnsSource()
        {
            bool found = _runtime.GetDestinationNode("entry_to_hall", "hall_a", out var destId);
            Assert.IsTrue(found);
            Assert.AreEqual("entry", destId);
        }

        [Test]
        public void GetDestinationNode_InvalidEdge_ReturnsFalse()
        {
            bool found = _runtime.GetDestinationNode("nonexistent", "entry", out _);
            Assert.IsFalse(found);
        }

        [Test]
        public void GetDestinationNode_NodeNotOnEdge_ReturnsFalse()
        {
            bool found = _runtime.GetDestinationNode("entry_to_hall", "living", out _);
            Assert.IsFalse(found);
        }

        // --- GetPortalAnchor ---

        [Test]
        public void GetPortalAnchor_ValidNodeAndAnchor_ReturnsTrue()
        {
            bool found = _runtime.GetPortalAnchor("entry", "door_a", out var anchor);
            Assert.IsTrue(found);
            Assert.AreEqual("door_a", anchor.anchorId);
            Assert.AreEqual(new Vector3(2f, 0f, 0f), anchor.localPosition);
        }

        [Test]
        public void GetPortalAnchor_InvalidNode_ReturnsFalse()
        {
            bool found = _runtime.GetPortalAnchor("nonexistent", "door_a", out _);
            Assert.IsFalse(found);
        }

        [Test]
        public void GetPortalAnchor_InvalidAnchor_ReturnsFalse()
        {
            bool found = _runtime.GetPortalAnchor("entry", "nonexistent", out _);
            Assert.IsFalse(found);
        }

        // --- NodeCount / EdgeCount ---

        [Test]
        public void NodeCount_ReturnsCorrectCount()
        {
            Assert.AreEqual(3, _runtime.NodeCount);
        }

        [Test]
        public void EdgeCount_ReturnsCorrectCount()
        {
            Assert.AreEqual(2, _runtime.EdgeCount);
        }

        // --- Reset ---

        [Test]
        public void Reset_ClearsAllState()
        {
            _runtime.Reset();

            Assert.AreEqual(0, _runtime.NodeCount);
            Assert.AreEqual(0, _runtime.EdgeCount);
            Assert.IsFalse(_runtime.GetNode("entry", out _));
        }

        // --- Reset: verify connected edges cleared ---

        [Test]
        public void Reset_ClearsConnectedEdges()
        {
            _runtime.Reset();

            var edges = _runtime.GetConnectedEdges("hall_a");
            Assert.AreEqual(0, edges.Count);
        }

        // --- GetConnectedEdges: returned list is not mutable by caller ---

        [Test]
        public void GetConnectedEdges_ReturnedListIsReadOnly()
        {
            var edges = _runtime.GetConnectedEdges("entry");
            // Verify the returned collection is read-only (IReadOnlyList)
            Assert.IsInstanceOf<System.Collections.Generic.IReadOnlyList<HouseEdgeDefinition>>(edges);
        }

        // --- Edge case: null definition ---

        [Test]
        public void Initialize_NullDefinition_DoesNotThrow()
        {
            var runtime = new SpatialGraphRuntime();
            Assert.DoesNotThrow(() => runtime.Initialize(null));
            Assert.AreEqual(0, runtime.NodeCount);
        }

        // --- Edge case: empty graph ---

        [Test]
        public void Initialize_EmptyGraph_DoesNotThrow()
        {
            var emptyDef = ScriptableObject.CreateInstance<HouseGraphDefinition>();
            var runtime = new SpatialGraphRuntime();

            Assert.DoesNotThrow(() => runtime.Initialize(emptyDef));
            Assert.AreEqual(0, runtime.NodeCount);

            Object.DestroyImmediate(emptyDef);
        }
    }
}
