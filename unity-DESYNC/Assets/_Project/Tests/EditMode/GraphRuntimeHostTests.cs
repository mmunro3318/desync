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
                    portalAnchors = new[] { new PortalAnchorDefinition { anchorId = "door_a" } }
                },
                new HouseNodeDefinition
                {
                    nodeId = "hall",
                    displayName = "Hallway",
                    worldPosition = new Vector3(5f, 0f, 0f),
                    portalAnchors = new[] { new PortalAnchorDefinition { anchorId = "door_b" } }
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
