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

        #region Grace Timers

        [Test]
        public void Tick_NodeWasOccupied_ThenLeaves_GracePreventsMutation()
        {
            _rules.nodeGraceSeconds = 2.0f;
            var system = new ObservationLockSystem(_input, _graph, _rules);

            // Frame 1: player occupies entry
            _input.OccupiedNodeIds.Add("entry");
            system.Tick(0f);
            Assert.IsTrue(system.IsNodeLocked("entry"));

            // Frame 2: player leaves — grace starts
            _input.OccupiedNodeIds.Clear();
            system.Tick(0.5f);
            Assert.IsFalse(system.IsNodeLocked("entry"), "No active reasons");
            Assert.IsFalse(system.IsNodeMutationEligible("entry"), "Should be in grace");
            Assert.AreEqual(1.5f, system.GetNodeGraceRemaining("entry"), 0.01f);

            // Frame 3: grace continues
            system.Tick(0.5f);
            Assert.IsFalse(system.IsNodeMutationEligible("entry"), "Still in grace");
            Assert.AreEqual(1.0f, system.GetNodeGraceRemaining("entry"), 0.01f);

            // Frame 4: grace continues
            system.Tick(0.5f);
            Assert.AreEqual(0.5f, system.GetNodeGraceRemaining("entry"), 0.01f);

            // Frame 5: grace expires
            system.Tick(0.5f);
            Assert.IsTrue(system.IsNodeMutationEligible("entry"), "Grace expired");
            Assert.AreEqual(0f, system.GetNodeGraceRemaining("entry"));
        }

        [Test]
        public void Tick_EdgeGrace_DecrementsOverMultipleTicks()
        {
            _rules.edgeGraceSeconds = 1.0f;
            var system = new ObservationLockSystem(_input, _graph, _rules);

            // Lock edge via occupancy
            _input.OccupiedNodeIds.Add("entry");
            system.Tick(0f);
            Assert.IsTrue(system.IsEdgeLocked("entry_to_hall"));

            // Player leaves — edge grace starts
            _input.OccupiedNodeIds.Clear();
            system.Tick(0.4f);
            Assert.IsFalse(system.IsEdgeMutationEligible("entry_to_hall"), "In grace");

            system.Tick(0.4f);
            Assert.IsFalse(system.IsEdgeMutationEligible("entry_to_hall"), "Still in grace");

            system.Tick(0.4f);
            Assert.IsTrue(system.IsEdgeMutationEligible("entry_to_hall"), "Grace expired");
        }

        [Test]
        public void Tick_ReLockDuringGrace_ClearsGraceTimer()
        {
            _rules.nodeGraceSeconds = 2.0f;
            var system = new ObservationLockSystem(_input, _graph, _rules);

            // Occupy, then leave to start grace
            _input.OccupiedNodeIds.Add("entry");
            system.Tick(0f);
            _input.OccupiedNodeIds.Clear();
            system.Tick(0.5f);
            Assert.IsTrue(system.GetNodeGraceRemaining("entry") > 0f, "Should be in grace");

            // Re-occupy — grace should clear
            _input.OccupiedNodeIds.Add("entry");
            system.Tick(0f);
            Assert.IsTrue(system.IsNodeLocked("entry"));
            Assert.AreEqual(0f, system.GetNodeGraceRemaining("entry"),
                "Grace should reset when re-locked");
        }

        [Test]
        public void Tick_GraceDoesNotRestartAfterExpiry()
        {
            _rules.nodeGraceSeconds = 0.5f;
            var system = new ObservationLockSystem(_input, _graph, _rules);

            // Occupy, leave, grace starts and expires
            _input.OccupiedNodeIds.Add("entry");
            system.Tick(0f);
            _input.OccupiedNodeIds.Clear();
            system.Tick(0.5f);
            Assert.IsTrue(system.IsNodeMutationEligible("entry"), "Grace expired");

            // Another tick — should stay eligible, not restart grace
            system.Tick(0.5f);
            Assert.IsTrue(system.IsNodeMutationEligible("entry"),
                "Should stay eligible, not restart grace");
        }

        #endregion

        #region Visibility Lock

        [Test]
        public void Tick_VisibleNode_IsLockedWithPortalVisibleReason()
        {
            var system = new ObservationLockSystem(_input, _graph, _rules);
            _input.VisibleNodeIds.Add("hall_a");

            system.Tick(0f);

            Assert.IsTrue(system.IsNodeLocked("hall_a"));
            var reasons = system.GetNodeLockReasons("hall_a");
            Assert.IsTrue(reasons.Contains(LockReason.PortalVisible));
        }

        [Test]
        public void Tick_VisibleEdge_IsLockedWithPortalVisibleReason()
        {
            var system = new ObservationLockSystem(_input, _graph, _rules);
            _input.VisibleEdgeIds.Add("entry_to_hall");

            system.Tick(0f);

            Assert.IsTrue(system.IsEdgeLocked("entry_to_hall"));
            var reasons = system.GetEdgeLockReasons("entry_to_hall");
            Assert.IsTrue(reasons.Contains(LockReason.PortalVisible));
        }

        [Test]
        public void Tick_NodeOccupiedAndVisible_HasBothReasons()
        {
            var system = new ObservationLockSystem(_input, _graph, _rules);
            _input.OccupiedNodeIds.Add("entry");
            _input.VisibleNodeIds.Add("entry");

            system.Tick(0f);

            var reasons = system.GetNodeLockReasons("entry");
            Assert.IsTrue(reasons.Contains(LockReason.Occupied));
            Assert.IsTrue(reasons.Contains(LockReason.PortalVisible));
        }

        [Test]
        public void Tick_VisibilityDropsButOccupied_NoGraceStarts()
        {
            _rules.nodeGraceSeconds = 2.0f;
            var system = new ObservationLockSystem(_input, _graph, _rules);

            // Both occupied and visible
            _input.OccupiedNodeIds.Add("entry");
            _input.VisibleNodeIds.Add("entry");
            system.Tick(0f);

            // Visibility drops but still occupied — grace should NOT start
            _input.VisibleNodeIds.Clear();
            system.Tick(0.5f);

            Assert.IsTrue(system.IsNodeLocked("entry"), "Still occupied");
            Assert.AreEqual(0f, system.GetNodeGraceRemaining("entry"),
                "No grace — still has active reason");
        }

        #endregion

        #region Visibility Refresh Interval

        [Test]
        public void Tick_VisibilityRefreshInterval_SkipsVisibilityPolling()
        {
            // This test verifies the accumulator pattern exists.
            // With interval > 0, visibility inputs should only be re-evaluated
            // after the interval elapses.
            _rules.visibilityRefreshInterval = 1.0f;
            var system = new ObservationLockSystem(_input, _graph, _rules);

            _input.OccupiedNodeIds.Add("entry");
            _input.VisibleNodeIds.Add("hall_a");
            system.Tick(0f);
            Assert.IsTrue(system.IsNodeLocked("hall_a"), "First tick always evaluates");

            // Remove visibility, tick with small dt — should skip re-evaluation
            _input.VisibleNodeIds.Clear();
            system.Tick(0.3f);
            // hall_a should still appear locked because visibility wasn't re-polled
            Assert.IsTrue(system.IsNodeLocked("hall_a"),
                "Visibility not re-polled before interval elapses");

            // Tick past the interval — now it should re-evaluate
            system.Tick(0.8f);
            Assert.IsFalse(system.IsNodeLocked("hall_a"),
                "Visibility re-polled after interval elapsed");
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

        #region Debug Override

        [Test]
        public void ForceNodeLock_LocksUnoccupiedNode()
        {
            var system = new ObservationLockSystem(_input, _graph, _rules);
            system.Tick(0f);

            system.ForceNodeLock("living");

            Assert.IsTrue(system.IsNodeLocked("living"));
            var reasons = system.GetNodeLockReasons("living");
            Assert.IsTrue(reasons.Contains(LockReason.DebugForced));
        }

        [Test]
        public void ForceNodeUnlock_OverridesOccupancyLock()
        {
            var system = new ObservationLockSystem(_input, _graph, _rules);
            _input.OccupiedNodeIds.Add("entry");
            system.Tick(0f);
            Assert.IsTrue(system.IsNodeLocked("entry"));

            system.ForceNodeUnlock("entry");
            system.Tick(0f);

            Assert.IsFalse(system.IsNodeLocked("entry"));
            Assert.IsTrue(system.IsNodeMutationEligible("entry"));
        }

        [Test]
        public void ForceNodeLock_SurvivesTick()
        {
            var system = new ObservationLockSystem(_input, _graph, _rules);

            system.ForceNodeLock("living");
            system.Tick(0f);

            Assert.IsTrue(system.IsNodeLocked("living"));
            Assert.IsTrue(system.GetNodeLockReasons("living").Contains(LockReason.DebugForced));
        }

        [Test]
        public void ForceNodeUnlock_SurvivesMultipleTicks()
        {
            _rules.nodeGraceSeconds = 5f;
            var system = new ObservationLockSystem(_input, _graph, _rules);
            _input.OccupiedNodeIds.Add("entry");
            system.Tick(0f);

            system.ForceNodeUnlock("entry");
            system.Tick(0.1f);
            Assert.IsTrue(system.IsNodeMutationEligible("entry"), "First tick after unlock");

            system.Tick(0.1f);
            Assert.IsTrue(system.IsNodeMutationEligible("entry"), "Second tick after unlock");
        }

        [Test]
        public void Reset_ClearsDebugOverrides()
        {
            var system = new ObservationLockSystem(_input, _graph, _rules);

            system.ForceNodeLock("living");
            Assert.IsTrue(system.IsNodeLocked("living"));

            system.Reset();
            system.Tick(0f);

            Assert.IsFalse(system.IsNodeLocked("living"),
                "Debug override should be cleared after Reset");
        }

        #endregion
    }
}
