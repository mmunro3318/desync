using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Desync.World.Graph.Definitions;
using Desync.World.Graph.Runtime;

namespace Desync.Tests.EditMode
{
    [TestFixture]
    public class ObservationLockSystemTests
    {
        private HouseGraphDefinition _definition;
        private SpatialGraphRuntime _graph;
        private ObservationRulesDefinition _rules;
        private FakeObservationInputSource _input;

        [SetUp]
        public void SetUp()
        {
            // 3-node graph: entry -- hall_a -- living
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

            _rules = ScriptableObject.CreateInstance<ObservationRulesDefinition>();
            _input = new FakeObservationInputSource();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_definition);
            Object.DestroyImmediate(_rules);
        }

        #region Occupancy Lock

        [Test]
        public void Tick_OccupiedNode_IsLocked()
        {
            var system = new ObservationLockSystem(_input, _graph, _rules);
            _input.OccupiedNodeIds.Add("entry");

            system.Tick(0f);

            Assert.IsTrue(system.IsNodeLocked("entry"));
        }

        [Test]
        public void Tick_OccupiedNode_HasOccupiedReason()
        {
            var system = new ObservationLockSystem(_input, _graph, _rules);
            _input.OccupiedNodeIds.Add("entry");

            system.Tick(0f);

            var reasons = system.GetNodeLockReasons("entry");
            Assert.IsTrue(reasons.Contains(LockReason.Occupied));
        }

        [Test]
        public void Tick_OccupiedNode_IsNotMutationEligible()
        {
            var system = new ObservationLockSystem(_input, _graph, _rules);
            _input.OccupiedNodeIds.Add("entry");

            system.Tick(0f);

            Assert.IsFalse(system.IsNodeMutationEligible("entry"));
        }

        [Test]
        public void Tick_UnoccupiedNode_IsNotLocked()
        {
            var system = new ObservationLockSystem(_input, _graph, _rules);
            _input.OccupiedNodeIds.Add("entry");

            system.Tick(0f);

            Assert.IsFalse(system.IsNodeLocked("living"),
                "Node not occupied and not adjacent should not be locked");
        }

        [Test]
        public void Tick_UnoccupiedUnvisibleNode_IsMutationEligible()
        {
            var system = new ObservationLockSystem(_input, _graph, _rules);
            _input.OccupiedNodeIds.Add("entry");
            // No grace configured for this test path (node was never locked)

            system.Tick(0f);

            Assert.IsTrue(system.IsNodeMutationEligible("living"));
        }

        #endregion

        #region Adjacent Edge Lock

        [Test]
        public void Tick_EdgeConnectedToOccupiedNode_IsLocked()
        {
            var system = new ObservationLockSystem(_input, _graph, _rules);
            _input.OccupiedNodeIds.Add("entry");

            system.Tick(0f);

            Assert.IsTrue(system.IsEdgeLocked("entry_to_hall"),
                "Edge connected to occupied node should be locked");
        }

        [Test]
        public void Tick_EdgeConnectedToOccupiedNode_HasAdjacentReason()
        {
            var system = new ObservationLockSystem(_input, _graph, _rules);
            _input.OccupiedNodeIds.Add("entry");

            system.Tick(0f);

            var reasons = system.GetEdgeLockReasons("entry_to_hall");
            Assert.IsTrue(reasons.Contains(LockReason.AdjacentOccupiedEdge));
        }

        [Test]
        public void Tick_EdgeNotConnectedToOccupiedNode_IsNotLocked()
        {
            var system = new ObservationLockSystem(_input, _graph, _rules);
            _input.OccupiedNodeIds.Add("living");

            system.Tick(0f);

            Assert.IsFalse(system.IsEdgeLocked("entry_to_hall"),
                "Edge not connected to occupied node should not be locked");
        }

        #endregion

        #region Player Movement

        [Test]
        public void Tick_PlayerMoves_OldNodeUnlocks_NewNodeLocks()
        {
            var system = new ObservationLockSystem(_input, _graph, _rules);
            // Disable grace so unlock is immediate
            _rules.nodeGraceSeconds = 0f;
            _rules.edgeGraceSeconds = 0f;

            _input.OccupiedNodeIds.Add("entry");
            system.Tick(0f);
            Assert.IsTrue(system.IsNodeLocked("entry"));

            // Player moves to hall_a
            _input.OccupiedNodeIds.Clear();
            _input.OccupiedNodeIds.Add("hall_a");
            system.Tick(0f);

            Assert.IsFalse(system.IsNodeLocked("entry"),
                "Old node should unlock after player leaves (0 grace)");
            Assert.IsTrue(system.IsNodeLocked("hall_a"),
                "New node should be locked");
        }

        #endregion

        #region Enumeration

        [Test]
        public void GetAllNodeStates_ReturnsAllTrackedNodes()
        {
            var system = new ObservationLockSystem(_input, _graph, _rules);
            _input.OccupiedNodeIds.Add("entry");

            system.Tick(0f);

            var allStates = system.GetAllNodeStates();
            Assert.IsTrue(allStates.ContainsKey("entry"));
            Assert.IsTrue(allStates["entry"].IsLocked);
        }

        [Test]
        public void GetAllEdgeStates_ReturnsAllTrackedEdges()
        {
            var system = new ObservationLockSystem(_input, _graph, _rules);
            _input.OccupiedNodeIds.Add("entry");

            system.Tick(0f);

            var allStates = system.GetAllEdgeStates();
            Assert.IsTrue(allStates.ContainsKey("entry_to_hall"));
            Assert.IsTrue(allStates["entry_to_hall"].IsLocked);
        }

        #endregion

        #region Reset

        [Test]
        public void Reset_ClearsAllLockState()
        {
            var system = new ObservationLockSystem(_input, _graph, _rules);
            _input.OccupiedNodeIds.Add("entry");
            system.Tick(0f);
            Assert.IsTrue(system.IsNodeLocked("entry"));

            system.Reset();

            Assert.IsFalse(system.IsNodeLocked("entry"),
                "All lock state should be cleared after Reset");
            Assert.IsFalse(system.IsEdgeLocked("entry_to_hall"));
        }

        #endregion

        #region Unknown IDs

        [Test]
        public void QueryUnknownNodeId_ReturnsUnlockedAndEligible()
        {
            var system = new ObservationLockSystem(_input, _graph, _rules);
            system.Tick(0f);

            Assert.IsFalse(system.IsNodeLocked("nonexistent"));
            Assert.IsTrue(system.IsNodeMutationEligible("nonexistent"));
            Assert.AreEqual(0, system.GetNodeLockReasons("nonexistent").Count);
            Assert.AreEqual(0f, system.GetNodeGraceRemaining("nonexistent"));
        }

        [Test]
        public void QueryUnknownEdgeId_ReturnsUnlockedAndEligible()
        {
            var system = new ObservationLockSystem(_input, _graph, _rules);
            system.Tick(0f);

            Assert.IsFalse(system.IsEdgeLocked("nonexistent"));
            Assert.IsTrue(system.IsEdgeMutationEligible("nonexistent"));
            Assert.AreEqual(0, system.GetEdgeLockReasons("nonexistent").Count);
            Assert.AreEqual(0f, system.GetEdgeGraceRemaining("nonexistent"));
        }

        #endregion
    }
}
