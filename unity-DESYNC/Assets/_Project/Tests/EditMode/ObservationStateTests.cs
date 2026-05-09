using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Desync.World.Graph.Runtime;

namespace Desync.Tests.EditMode
{
    [TestFixture]
    public class ObservationStateTests
    {
        #region LockReason

        [Test]
        public void LockReason_ValuesAreDistinct()
        {
            var values = System.Enum.GetValues(typeof(LockReason));
            var seen = new HashSet<int>();
            foreach (var v in values)
                Assert.IsTrue(seen.Add((int)v), $"Duplicate LockReason value: {v}");
        }

        [Test]
        public void LockReason_ContainsExpectedValues()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(LockReason), LockReason.Occupied));
            Assert.IsTrue(System.Enum.IsDefined(typeof(LockReason), LockReason.AdjacentOccupiedEdge));
            Assert.IsTrue(System.Enum.IsDefined(typeof(LockReason), LockReason.PortalVisible));
            Assert.IsTrue(System.Enum.IsDefined(typeof(LockReason), LockReason.GracePeriod));
            Assert.IsTrue(System.Enum.IsDefined(typeof(LockReason), LockReason.DebugForced));
            Assert.IsTrue(System.Enum.IsDefined(typeof(LockReason), LockReason.ProtectedByRule));
            Assert.IsTrue(System.Enum.IsDefined(typeof(LockReason), LockReason.None));
        }

        #endregion

        #region NodeObservationState

        [Test]
        public void NodeObservationState_DefaultIsUnlockedAndEligible()
        {
            var state = new NodeObservationState();
            Assert.IsFalse(state.IsLocked);
            Assert.IsTrue(state.IsMutationEligible);
            Assert.AreEqual(0, state.ActiveReasons.Count);
            Assert.AreEqual(0f, state.GraceRemaining);
        }

        [Test]
        public void NodeObservationState_AddReason_LocksAndPreventsEligibility()
        {
            var state = new NodeObservationState();
            state.AddReason(LockReason.Occupied);

            Assert.IsTrue(state.IsLocked);
            Assert.IsFalse(state.IsMutationEligible);
            Assert.AreEqual(1, state.ActiveReasons.Count);
            Assert.IsTrue(state.ActiveReasons.Contains(LockReason.Occupied));
        }

        [Test]
        public void NodeObservationState_RemoveLastReason_UnlocksButNotEligibleDuringGrace()
        {
            var state = new NodeObservationState();
            state.AddReason(LockReason.Occupied);
            state.RemoveReason(LockReason.Occupied);
            state.StartGrace(2.0f);

            Assert.IsFalse(state.IsLocked);
            Assert.IsFalse(state.IsMutationEligible, "Should not be eligible during grace period");
            Assert.AreEqual(2.0f, state.GraceRemaining);
        }

        [Test]
        public void NodeObservationState_GraceExpires_BecomesEligible()
        {
            var state = new NodeObservationState();
            state.StartGrace(1.0f);

            state.TickGrace(0.5f);
            Assert.IsFalse(state.IsMutationEligible, "Still in grace");
            Assert.AreEqual(0.5f, state.GraceRemaining, 0.001f);

            state.TickGrace(0.5f);
            Assert.IsTrue(state.IsMutationEligible, "Grace expired");
            Assert.AreEqual(0f, state.GraceRemaining);
        }

        [Test]
        public void NodeObservationState_ReAddReason_ClearsGrace()
        {
            var state = new NodeObservationState();
            state.StartGrace(2.0f);
            state.AddReason(LockReason.PortalVisible);

            Assert.IsTrue(state.IsLocked);
            Assert.AreEqual(0f, state.GraceRemaining, "Grace should reset when re-locked");
        }

        [Test]
        public void NodeObservationState_Clear_ResetsEverything()
        {
            var state = new NodeObservationState();
            state.AddReason(LockReason.Occupied);
            state.AddReason(LockReason.PortalVisible);
            state.StartGrace(1.5f);

            state.Clear();

            Assert.IsFalse(state.IsLocked);
            Assert.IsTrue(state.IsMutationEligible);
            Assert.AreEqual(0, state.ActiveReasons.Count);
            Assert.AreEqual(0f, state.GraceRemaining);
        }

        #endregion

        #region EdgeObservationState

        [Test]
        public void EdgeObservationState_DefaultIsUnlockedAndEligible()
        {
            var state = new EdgeObservationState();
            Assert.IsFalse(state.IsLocked);
            Assert.IsTrue(state.IsMutationEligible);
            Assert.AreEqual(0, state.ActiveReasons.Count);
            Assert.AreEqual(0f, state.GraceRemaining);
        }

        [Test]
        public void EdgeObservationState_AddReason_LocksAndPreventsEligibility()
        {
            var state = new EdgeObservationState();
            state.AddReason(LockReason.AdjacentOccupiedEdge);

            Assert.IsTrue(state.IsLocked);
            Assert.IsFalse(state.IsMutationEligible);
            Assert.AreEqual(1, state.ActiveReasons.Count);
            Assert.IsTrue(state.ActiveReasons.Contains(LockReason.AdjacentOccupiedEdge));
        }

        [Test]
        public void EdgeObservationState_GraceExpires_BecomesEligible()
        {
            var state = new EdgeObservationState();
            state.StartGrace(1.0f);

            state.TickGrace(1.0f);
            Assert.IsTrue(state.IsMutationEligible);
        }

        #endregion
    }
}
