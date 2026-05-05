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
        public void ExitNode_DoesNotClearIfDifferent()
        {
            _tracker.EnterNode("v_entry");
            _tracker.EnterNode("v_hall_a");
            _tracker.ExitNode("v_entry");
            Assert.AreEqual("v_hall_a", _tracker.CurrentNodeId);
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
    }
}
