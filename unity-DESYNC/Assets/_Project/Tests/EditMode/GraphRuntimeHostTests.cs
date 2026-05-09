using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Desync.World.Graph;
using Desync.World.Graph.Definitions;
using Desync.World.Graph.Runtime;

namespace Desync.Tests.EditMode
{
    public class GraphRuntimeHostTests
    {
        private GameObject _go;
        private GraphRuntimeHost _host;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("GraphRuntimeHost_Test");
            _host = _go.AddComponent<GraphRuntimeHost>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        // --- Component attachment ---

        [Test]
        public void AddComponent_ReturnsNonNull()
        {
            Assert.IsNotNull(_host);
        }

        // --- Before Awake (EditMode: Awake not called automatically) ---

        [Test]
        public void Runtime_BeforeAwake_IsNull()
        {
            Assert.IsNull(_host.Runtime);
        }

        [Test]
        public void Definition_BeforeAwake_IsNull()
        {
            Assert.IsNull(_host.Definition);
        }

        // --- Awake via reflection with null definition ---

        [Test]
        public void Awake_NullDefinition_RuntimeRemainsNull()
        {
            LogAssert.Expect(LogType.Error, "[GraphRuntimeHost] No HouseGraphDefinition assigned.");
            InvokeAwake(_host);
            Assert.IsNull(_host.Runtime);
        }

        // --- Awake via reflection with invalid definition ---

        [Test]
        public void Awake_InvalidDefinition_RuntimeRemainsNull()
        {
            // A definition with a duplicate node id fails validation.
            var def = ScriptableObject.CreateInstance<HouseGraphDefinition>();
            def.nodes = new[]
            {
                new HouseNodeDefinition { nodeId = "entry" },
                new HouseNodeDefinition { nodeId = "entry" }
            };
            SetSerializedField(_host, "graphDefinition", def);

            LogAssert.Expect(LogType.Error, "[GraphRuntimeHost] Validation: Duplicate node ID: 'entry'");
            InvokeAwake(_host);

            Assert.IsNull(_host.Runtime);
            Object.DestroyImmediate(def);
        }

        // --- Awake via reflection with valid definition ---

        [Test]
        public void Awake_ValidDefinition_RuntimeIsNotNull()
        {
            var def = BuildMinimalValidDefinition();
            SetSerializedField(_host, "graphDefinition", def);

            InvokeAwake(_host);

            Assert.IsNotNull(_host.Runtime);
            Object.DestroyImmediate(def);
        }

        [Test]
        public void Awake_ValidDefinition_DefinitionPropertyMatchesAssigned()
        {
            var def = BuildMinimalValidDefinition();
            SetSerializedField(_host, "graphDefinition", def);

            InvokeAwake(_host);

            Assert.AreSame(def, _host.Definition);
            Object.DestroyImmediate(def);
        }

        [Test]
        public void Awake_ValidDefinition_RuntimeHasCorrectNodeCount()
        {
            var def = BuildMinimalValidDefinition();
            SetSerializedField(_host, "graphDefinition", def);

            InvokeAwake(_host);

            Assert.AreEqual(2, _host.Runtime.NodeCount);
            Object.DestroyImmediate(def);
        }

        [Test]
        public void Awake_ValidDefinition_RuntimeHasCorrectEdgeCount()
        {
            var def = BuildMinimalValidDefinition();
            SetSerializedField(_host, "graphDefinition", def);

            InvokeAwake(_host);

            Assert.AreEqual(1, _host.Runtime.EdgeCount);
            Object.DestroyImmediate(def);
        }

        #region Observation Binding

        [Test]
        public void BindObservationTracker_ReplacesLockSystemInstance()
        {
            var def = BuildMinimalValidDefinition();
            var rules = ScriptableObject.CreateInstance<ObservationRulesDefinition>();
            SetSerializedField(_host, "graphDefinition", def);
            SetSerializedField(_host, "observationRules", rules);
            InvokeAwake(_host);

            var preBind = _host.ObservationLock;
            Assert.IsNotNull(preBind, "Lock system should exist after Awake with rules");

            var trackerGo = new GameObject("Tracker");
            var tracker = trackerGo.AddComponent<PlayerNodeTracker>();

            _host.BindObservationTracker(tracker);

            Assert.IsNotNull(_host.ObservationLock, "Lock system should exist after bind");
            Assert.AreNotSame(preBind, _host.ObservationLock,
                "Bind should create a new lock system instance");

            Object.DestroyImmediate(trackerGo);
            Object.DestroyImmediate(def);
            Object.DestroyImmediate(rules);
        }

        [Test]
        public void BindObservationTracker_NoRules_DoesNotThrow()
        {
            var def = BuildMinimalValidDefinition();
            SetSerializedField(_host, "graphDefinition", def);
            LogAssert.Expect(LogType.Warning,
                "[GraphRuntimeHost] No ObservationRulesDefinition assigned — observation lock disabled.");
            InvokeAwake(_host);

            var trackerGo = new GameObject("Tracker");
            var tracker = trackerGo.AddComponent<PlayerNodeTracker>();

            Assert.DoesNotThrow(() => _host.BindObservationTracker(tracker),
                "Should not throw when observation system was not initialized");

            Object.DestroyImmediate(trackerGo);
            Object.DestroyImmediate(def);
        }

        [Test]
        public void BindObservationTracker_ClearsPriorLockState()
        {
            var def = BuildMinimalValidDefinition();
            var rules = ScriptableObject.CreateInstance<ObservationRulesDefinition>();
            SetSerializedField(_host, "graphDefinition", def);
            SetSerializedField(_host, "observationRules", rules);
            InvokeAwake(_host);

            // Create real lock state via debug override
            var lockSystem = _host.ObservationLock as ObservationLockSystem;
            Assert.IsNotNull(lockSystem);
            lockSystem.ForceNodeLock("entry");
            Assert.IsTrue(_host.ObservationLock.IsNodeLocked("entry"),
                "Precondition: entry should be locked before rebind");

            var trackerGo = new GameObject("Tracker");
            var tracker = trackerGo.AddComponent<PlayerNodeTracker>();

            _host.BindObservationTracker(tracker);

            Assert.IsFalse(_host.ObservationLock.IsNodeLocked("entry"),
                "Lock state should be cleared after rebind");
            Assert.AreEqual(0, _host.ObservationLock.GetAllNodeStates().Count,
                "All node states should be empty after rebind");

            Object.DestroyImmediate(trackerGo);
            Object.DestroyImmediate(def);
            Object.DestroyImmediate(rules);
        }

        #endregion

        #region Observation Reset

        [Test]
        public void ResetObservation_NoRules_DoesNotThrow()
        {
            var def = BuildMinimalValidDefinition();
            SetSerializedField(_host, "graphDefinition", def);
            LogAssert.Expect(LogType.Warning,
                "[GraphRuntimeHost] No ObservationRulesDefinition assigned — observation lock disabled.");
            InvokeAwake(_host);

            Assert.DoesNotThrow(() => _host.ResetObservation(),
                "Should not throw when observation system was not initialized");

            Object.DestroyImmediate(def);
        }

        [Test]
        public void ResetObservation_ClearsLockState()
        {
            var def = BuildMinimalValidDefinition();
            var rules = ScriptableObject.CreateInstance<ObservationRulesDefinition>();
            SetSerializedField(_host, "graphDefinition", def);
            SetSerializedField(_host, "observationRules", rules);
            InvokeAwake(_host);

            // Create real lock state via debug override
            var lockSystem = _host.ObservationLock as ObservationLockSystem;
            Assert.IsNotNull(lockSystem);
            lockSystem.ForceNodeLock("entry");
            lockSystem.ForceEdgeLock("entry_to_hall");
            Assert.IsTrue(_host.ObservationLock.IsNodeLocked("entry"),
                "Precondition: node should be locked");
            Assert.IsTrue(_host.ObservationLock.IsEdgeLocked("entry_to_hall"),
                "Precondition: edge should be locked");

            _host.ResetObservation();

            Assert.AreEqual(0, _host.ObservationLock.GetAllNodeStates().Count,
                "All node states should be cleared after ResetObservation");
            Assert.AreEqual(0, _host.ObservationLock.GetAllEdgeStates().Count,
                "All edge states should be cleared after ResetObservation");

            Object.DestroyImmediate(def);
            Object.DestroyImmediate(rules);
        }

        #endregion

        // --- Helpers ---

        private static HouseGraphDefinition BuildMinimalValidDefinition()
        {
            var def = ScriptableObject.CreateInstance<HouseGraphDefinition>();
            def.nodes = new[]
            {
                new HouseNodeDefinition
                {
                    nodeId = "entry",
                    displayName = "Entry Hall",
                    worldPosition = Vector3.zero,
                    portalAnchors = new[] { new PortalAnchorDefinition { anchorId = "door_a", localRotation = Quaternion.identity } }
                },
                new HouseNodeDefinition
                {
                    nodeId = "hall",
                    displayName = "Hallway",
                    worldPosition = new Vector3(5f, 0f, 0f),
                    portalAnchors = new[] { new PortalAnchorDefinition { anchorId = "door_b", localRotation = Quaternion.identity } }
                }
            };
            def.edges = new[]
            {
                new HouseEdgeDefinition
                {
                    edgeId = "entry_to_hall",
                    sourceNodeId = "entry",
                    targetNodeId = "hall",
                    sourceAnchorId = "door_a",
                    targetAnchorId = "door_b"
                }
            };
            return def;
        }

        private static void InvokeAwake(GraphRuntimeHost host)
        {
            var method = typeof(GraphRuntimeHost).GetMethod(
                "Awake",
                BindingFlags.Instance | BindingFlags.NonPublic);
            method?.Invoke(host, null);
        }

        private static void SetSerializedField(GraphRuntimeHost host, string fieldName, object value)
        {
            var field = typeof(GraphRuntimeHost).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            field?.SetValue(host, value);
        }
    }
}
