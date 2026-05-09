using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Desync.World.Graph.Definitions;
using Desync.World.Graph.Runtime;
using Desync.World.Graph.Authoring;

namespace Desync.Tests.EditMode
{
    [TestFixture]
    public class LocalObservationInputSourceTests
    {
        private HouseGraphDefinition _definition;
        private SpatialGraphRuntime _graph;
        private GameObject _trackerGo;
        private PlayerNodeTracker _tracker;

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
                    worldPosition = Vector3.zero,
                    portalAnchors = new[]
                    {
                        new PortalAnchorDefinition { anchorId = "door_a" }
                    }
                },
                new HouseNodeDefinition
                {
                    nodeId = "hall_a",
                    displayName = "Hall A",
                    worldPosition = new Vector3(6f, 0f, 0f),
                    portalAnchors = new[]
                    {
                        new PortalAnchorDefinition { anchorId = "door_b" },
                        new PortalAnchorDefinition { anchorId = "door_c" }
                    }
                },
                new HouseNodeDefinition
                {
                    nodeId = "living",
                    displayName = "Living",
                    worldPosition = new Vector3(12f, 0f, 0f),
                    portalAnchors = new[]
                    {
                        new PortalAnchorDefinition { anchorId = "door_d" }
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

            _graph = new SpatialGraphRuntime();
            _graph.Initialize(_definition);

            _trackerGo = new GameObject("Tracker");
            _tracker = _trackerGo.AddComponent<PlayerNodeTracker>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_trackerGo);
            Object.DestroyImmediate(_definition);
        }

        #region Occupied Nodes

        [Test]
        public void GetOccupiedNodeIds_ReturnsCurrentNode()
        {
            _tracker.EnterNode("entry");
            var source = new LocalObservationInputSource(_tracker, _graph);

            var occupied = source.GetOccupiedNodeIds();

            Assert.AreEqual(1, occupied.Count);
            Assert.AreEqual("entry", occupied[0]);
        }

        [Test]
        public void GetOccupiedNodeIds_NullCurrentNode_ReturnsEmpty()
        {
            // Tracker starts with null CurrentNodeId (TD0013 void-zone)
            var source = new LocalObservationInputSource(_tracker, _graph);

            var occupied = source.GetOccupiedNodeIds();

            Assert.AreEqual(0, occupied.Count);
        }

        #endregion

        #region Visible Nodes

        [Test]
        public void GetVisibleNodeIds_WithVisiblePortalResults_ReturnsDestinations()
        {
            _tracker.EnterNode("entry");
            var source = new LocalObservationInputSource(_tracker, _graph);

            // Inject pre-built portal results (simulates what EvaluatePortals returns)
            source.SetPortalResults(new List<PortalVisibilityResult>
            {
                new PortalVisibilityResult("door_a", "hall_a", true)
            });

            var visible = source.GetVisibleNodeIds();

            Assert.AreEqual(1, visible.Count);
            Assert.AreEqual("hall_a", visible[0]);
        }

        [Test]
        public void GetVisibleNodeIds_NotVisibleResults_ReturnsEmpty()
        {
            _tracker.EnterNode("entry");
            var source = new LocalObservationInputSource(_tracker, _graph);

            source.SetPortalResults(new List<PortalVisibilityResult>
            {
                new PortalVisibilityResult("door_a", "hall_a", false)
            });

            var visible = source.GetVisibleNodeIds();

            Assert.AreEqual(0, visible.Count);
        }

        #endregion

        #region Visible Edges

        [Test]
        public void GetVisibleEdgeIds_DerivesFromVisibleDestinationAndGraph()
        {
            _tracker.EnterNode("entry");
            var source = new LocalObservationInputSource(_tracker, _graph);

            // hall_a is visible through portal
            source.SetPortalResults(new List<PortalVisibilityResult>
            {
                new PortalVisibilityResult("door_a", "hall_a", true)
            });

            var visibleEdges = source.GetVisibleEdgeIds();

            Assert.IsTrue(visibleEdges.Contains("entry_to_hall"),
                "Edge connecting current node to visible destination should be visible");
        }

        [Test]
        public void GetVisibleEdgeIds_NoVisibleDestination_ReturnsEmpty()
        {
            _tracker.EnterNode("entry");
            var source = new LocalObservationInputSource(_tracker, _graph);

            source.SetPortalResults(new List<PortalVisibilityResult>());

            var visibleEdges = source.GetVisibleEdgeIds();

            Assert.AreEqual(0, visibleEdges.Count);
        }

        [Test]
        public void GetVisibleEdgeIds_NoMatchingEdge_SkipsGracefully()
        {
            _tracker.EnterNode("entry");
            var source = new LocalObservationInputSource(_tracker, _graph);

            // Visible destination "living" but no direct edge from "entry" to "living"
            source.SetPortalResults(new List<PortalVisibilityResult>
            {
                new PortalVisibilityResult("some_anchor", "living", true)
            });

            var visibleEdges = source.GetVisibleEdgeIds();

            Assert.IsFalse(visibleEdges.Contains("entry_to_hall"),
                "No edge from entry to living directly");
        }

        #endregion
    }
}
