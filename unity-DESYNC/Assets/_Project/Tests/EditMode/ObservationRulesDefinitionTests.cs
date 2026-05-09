using NUnit.Framework;
using UnityEngine;
using Desync.World.Graph.Definitions;

namespace Desync.Tests.EditMode
{
    [TestFixture]
    public class ObservationRulesDefinitionTests
    {
        [Test]
        public void Defaults_HaveReasonableValues()
        {
            var rules = ScriptableObject.CreateInstance<ObservationRulesDefinition>();

            Assert.AreEqual(2.0f, rules.nodeGraceSeconds, "Default node grace should be 2s");
            Assert.AreEqual(1.5f, rules.edgeGraceSeconds, "Default edge grace should be 1.5s");
            Assert.AreEqual(0f, rules.visibilityRefreshInterval, "Default refresh interval should be 0 (every frame)");
            Assert.IsFalse(rules.lockDebugVerbose, "Debug verbose should default to false");

            Object.DestroyImmediate(rules);
        }

        [Test]
        public void GraceSeconds_CanBeModified()
        {
            var rules = ScriptableObject.CreateInstance<ObservationRulesDefinition>();

            rules.nodeGraceSeconds = 5.0f;
            rules.edgeGraceSeconds = 3.0f;

            Assert.AreEqual(5.0f, rules.nodeGraceSeconds);
            Assert.AreEqual(3.0f, rules.edgeGraceSeconds);

            Object.DestroyImmediate(rules);
        }

        [Test]
        public void VisibilityRefreshInterval_ZeroMeansEveryFrame()
        {
            var rules = ScriptableObject.CreateInstance<ObservationRulesDefinition>();

            Assert.AreEqual(0f, rules.visibilityRefreshInterval,
                "0 means evaluate every frame — no accumulator skip");

            rules.visibilityRefreshInterval = 0.1f;
            Assert.AreEqual(0.1f, rules.visibilityRefreshInterval);

            Object.DestroyImmediate(rules);
        }
    }
}
