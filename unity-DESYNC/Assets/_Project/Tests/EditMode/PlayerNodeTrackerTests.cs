using NUnit.Framework;
using UnityEngine;
using Desync.World.Graph.Runtime;

namespace Desync.Tests.EditMode
{
    public class PlayerNodeTrackerTests
    {
        private GameObject _go;
        private PlayerNodeTracker _tracker;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("PlayerNodeTrackerTest");
            _tracker = _go.AddComponent<PlayerNodeTracker>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void CurrentNodeId_InitiallyNull()
        {
            Assert.IsNull(_tracker.CurrentNodeId);
        }

        [Test]
        public void PreviousNodeId_InitiallyNull()
        {
            Assert.IsNull(_tracker.PreviousNodeId);
        }

        [Test]
        public void EnterNode_SetsCurrentNodeId()
        {
            _tracker.EnterNode("v_entry");
            Assert.AreEqual("v_entry", _tracker.CurrentNodeId);
        }

        [Test]
        public void EnterNode_SetsPreviousToOldCurrent()
        {
            _tracker.EnterNode("v_entry");
            _tracker.EnterNode("v_hall_a");
            Assert.AreEqual("v_entry", _tracker.PreviousNodeId);
            Assert.AreEqual("v_hall_a", _tracker.CurrentNodeId);
        }

        [Test]
        public void ExitNode_ClearsCurrentIfMatching()
        {
            _tracker.EnterNode("v_entry");
            _tracker.ExitNode("v_entry");
            Assert.IsNull(_tracker.CurrentNodeId);
        }

        [Test]
        public void ExitNode_SetsPreviousToExitedNode()
        {
            _tracker.EnterNode("v_entry");
            _tracker.ExitNode("v_entry");
            Assert.AreEqual("v_entry", _tracker.PreviousNodeId);
        }

        [Test]
        public void ExitNode_DoesNotClearIfDifferent()
        {
            _tracker.EnterNode("v_entry");
            _tracker.EnterNode("v_hall_a");
            _tracker.ExitNode("v_entry");
            Assert.AreEqual("v_hall_a", _tracker.CurrentNodeId);
        }

        [Test]
        public void ExitNode_DoesNotUpdatePreviousIfDifferent()
        {
            _tracker.EnterNode("v_entry");
            _tracker.EnterNode("v_hall_a");
            _tracker.ExitNode("v_entry");
            Assert.AreEqual("v_entry", _tracker.PreviousNodeId);
        }

        [Test]
        public void ExitNode_WhenNoCurrentNode_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _tracker.ExitNode("v_entry"));
        }

        [Test]
        public void EnterNode_NullId_Ignored()
        {
            _tracker.EnterNode(null);
            Assert.IsNull(_tracker.CurrentNodeId);
        }

        [Test]
        public void EnterNode_EmptyId_Ignored()
        {
            _tracker.EnterNode("");
            Assert.IsNull(_tracker.CurrentNodeId);
        }

        [Test]
        public void EnterNode_SameNodeTwice_NoStateChange()
        {
            _tracker.EnterNode("v_entry");
            _tracker.EnterNode("v_entry");
            Assert.AreEqual("v_entry", _tracker.CurrentNodeId);
            Assert.IsNull(_tracker.PreviousNodeId);
        }

        // --- Void zone transitions ---

        [Test]
        public void ExitToVoid_ThenEnterRoom_PreviousIsNull()
        {
            _tracker.EnterNode("v_entry");
            _tracker.ExitNode("v_entry"); // exit to void
            _tracker.EnterNode("v_hall_a"); // enter from void
            Assert.AreEqual("v_hall_a", _tracker.CurrentNodeId);
            Assert.IsNull(_tracker.PreviousNodeId);
        }

        [Test]
        public void ExitToVoid_CurrentIsNull_PreviousIsExitedRoom()
        {
            _tracker.EnterNode("v_entry");
            _tracker.ExitNode("v_entry");
            Assert.IsNull(_tracker.CurrentNodeId);
            Assert.AreEqual("v_entry", _tracker.PreviousNodeId);
        }

        // --- Overlapping trigger transitions (room-to-room) ---

        [Test]
        public void OverlapTransition_EnterNewBeforeExitOld_TracksCorrectly()
        {
            // Player enters hall_a trigger before exiting entry trigger
            _tracker.EnterNode("v_entry");
            _tracker.EnterNode("v_hall_a"); // overlap: enter new first
            _tracker.ExitNode("v_entry");   // then exit old
            Assert.AreEqual("v_hall_a", _tracker.CurrentNodeId);
            Assert.AreEqual("v_entry", _tracker.PreviousNodeId);
        }

        [Test]
        public void OverlapTransition_ExitOldBeforeEnterNew_TracksCorrectly()
        {
            // Player exits entry trigger before entering hall_a trigger
            _tracker.EnterNode("v_entry");
            _tracker.ExitNode("v_entry");   // exit old first (brief void)
            _tracker.EnterNode("v_hall_a"); // then enter new
            Assert.AreEqual("v_hall_a", _tracker.CurrentNodeId);
            // Previous is null because we passed through void
            Assert.IsNull(_tracker.PreviousNodeId);
        }

        // --- Multi-hop traversal ---

        [Test]
        public void MultiHopTraversal_PreviousTracksLastRoom()
        {
            _tracker.EnterNode("v_entry");
            _tracker.EnterNode("v_hall_a");
            _tracker.EnterNode("v_living");
            Assert.AreEqual("v_living", _tracker.CurrentNodeId);
            Assert.AreEqual("v_hall_a", _tracker.PreviousNodeId);
        }
    }
}
