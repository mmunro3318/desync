using NUnit.Framework;
using Desync.World.Graph.Runtime;

namespace Desync.Tests.EditMode
{
    public class RuntimeNodeStateTests
    {
        [Test]
        public void NewState_DefaultsToActiveWithZeroOccupancy()
        {
            var state = new RuntimeNodeState("entry");

            Assert.AreEqual("entry", state.NodeId);
            Assert.IsTrue(state.IsActive);
            Assert.AreEqual(0, state.Occupancy);
        }

        [Test]
        public void IncrementOccupancy_IncreasesCount()
        {
            var state = new RuntimeNodeState("entry");

            state.IncrementOccupancy();
            Assert.AreEqual(1, state.Occupancy);

            state.IncrementOccupancy();
            Assert.AreEqual(2, state.Occupancy);
        }

        [Test]
        public void DecrementOccupancy_DecreasesCount()
        {
            var state = new RuntimeNodeState("entry");
            state.IncrementOccupancy();
            state.IncrementOccupancy();

            state.DecrementOccupancy();
            Assert.AreEqual(1, state.Occupancy);
        }

        [Test]
        public void DecrementOccupancy_DoesNotGoBelowZero()
        {
            var state = new RuntimeNodeState("entry");

            state.DecrementOccupancy();
            Assert.AreEqual(0, state.Occupancy);
        }

        [Test]
        public void SetActive_ChangesState()
        {
            var state = new RuntimeNodeState("entry");

            state.SetActive(false);
            Assert.IsFalse(state.IsActive);

            state.SetActive(true);
            Assert.IsTrue(state.IsActive);
        }
    }

    public class RuntimeEdgeStateTests
    {
        [Test]
        public void NewState_DefaultsToOpen()
        {
            var state = new RuntimeEdgeState("e1");

            Assert.AreEqual("e1", state.EdgeId);
            Assert.IsTrue(state.IsOpen);
        }

        [Test]
        public void SetOpen_ChangesState()
        {
            var state = new RuntimeEdgeState("e1");

            state.SetOpen(false);
            Assert.IsFalse(state.IsOpen);

            state.SetOpen(true);
            Assert.IsTrue(state.IsOpen);
        }
    }
}
