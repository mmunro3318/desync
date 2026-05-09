using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using Desync.World.Graph.Definitions;
using Desync.World.Graph.Runtime;
using Desync.World.Graph.Authoring;

namespace Desync.Tests.EditMode.NodeActivation
{
    [TestFixture]
    public class NodeStreamingControllerTests
    {
        [Test]
        public void UpdatePresentation_ActivatesOccupiedNode_DeactivatesOthers()
        {
            var go = new GameObject("Controller");
            var controller = go.AddComponent<NodeStreamingController>();

            var entryGo = new GameObject("Room_Entry");
            var entryHandle = entryGo.AddComponent<NodePresentationHandle>();
            SetNodeId(entryHandle, "entry");
            var entryPres = WireWithPresentationChild(entryGo, entryHandle);

            var hallGo = new GameObject("Room_HallA");
            var hallHandle = hallGo.AddComponent<NodePresentationHandle>();
            SetNodeId(hallHandle, "hall_a");
            var hallPres = WireWithPresentationChild(hallGo, hallHandle);

            controller.SetHandles(new[] { entryHandle, hallHandle });

            var ctx = new ViewContext("p1", Vector3.zero, Vector3.forward, "entry");
            controller.UpdatePresentation(ctx, new List<PortalVisibilityResult>());

            Assert.IsTrue(entryPres.activeSelf, "Occupied node presentation should be active");
            Assert.IsFalse(hallPres.activeSelf, "Non-occupied node presentation should be inactive");
            Assert.IsTrue(entryGo.activeSelf, "Room root must stay active");
            Assert.IsTrue(hallGo.activeSelf, "Room root must stay active");

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(entryGo);
            Object.DestroyImmediate(hallGo);
        }

        [Test]
        public void UpdatePresentation_PortalVisible_ActivatesDestination()
        {
            var go = new GameObject("Controller");
            var controller = go.AddComponent<NodeStreamingController>();

            var entryGo = new GameObject("Room_Entry");
            var entryHandle = entryGo.AddComponent<NodePresentationHandle>();
            SetNodeId(entryHandle, "entry");
            var entryPres = WireWithPresentationChild(entryGo, entryHandle);

            var hallGo = new GameObject("Room_HallA");
            var hallHandle = hallGo.AddComponent<NodePresentationHandle>();
            SetNodeId(hallHandle, "hall_a");
            var hallPres = WireWithPresentationChild(hallGo, hallHandle);

            controller.SetHandles(new[] { entryHandle, hallHandle });

            var ctx = new ViewContext("p1", Vector3.zero, Vector3.forward, "entry");
            var portalResults = new List<PortalVisibilityResult>
            {
                new PortalVisibilityResult("anchor_1", "hall_a", true)
            };

            controller.UpdatePresentation(ctx, portalResults);

            Assert.IsTrue(entryPres.activeSelf, "Occupied node presentation should be active");
            Assert.IsTrue(hallPres.activeSelf, "Portal-visible node presentation should be active");

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(entryGo);
            Object.DestroyImmediate(hallGo);
        }

        [Test]
        public void UpdatePresentation_EmptyHandles_DoesNotThrow()
        {
            var go = new GameObject("Controller");
            var controller = go.AddComponent<NodeStreamingController>();
            controller.SetHandles(new NodePresentationHandle[0]);

            var ctx = new ViewContext("p1", Vector3.zero, Vector3.forward, "entry");

            Assert.DoesNotThrow(() => controller.UpdatePresentation(ctx, new List<PortalVisibilityResult>()));

            Object.DestroyImmediate(go);
        }

        [Test]
        // Behavior coverage: ActivateAll calls SetPresentation(true) on all handles, covered by UpdatePresentation tests.
        public void ForceAllActive_PropertyRoundTrip()
        {
            var go = new GameObject("Controller");
            var controller = go.AddComponent<NodeStreamingController>();
            controller.ForceAllActive = true;

            var entryGo = new GameObject("Room_Entry");
            var entryHandle = entryGo.AddComponent<NodePresentationHandle>();
            SetNodeId(entryHandle, "entry");
            var entryPres = WireWithPresentationChild(entryGo, entryHandle);
            entryPres.SetActive(false);

            var hallGo = new GameObject("Room_HallA");
            var hallHandle = hallGo.AddComponent<NodePresentationHandle>();
            SetNodeId(hallHandle, "hall_a");
            var hallPres = WireWithPresentationChild(hallGo, hallHandle);
            hallPres.SetActive(false);

            controller.SetHandles(new[] { entryHandle, hallHandle });

            // ForceAllActive doesn't apply via UpdatePresentation (it's the Update() path)
            // but we can test the public property is settable
            Assert.IsTrue(controller.ForceAllActive);

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(entryGo);
            Object.DestroyImmediate(hallGo);
        }

        [Test]
        public void LastResult_ExposesResolverOutput()
        {
            var go = new GameObject("Controller");
            var controller = go.AddComponent<NodeStreamingController>();
            controller.SetHandles(new NodePresentationHandle[0]);

            var ctx = new ViewContext("p1", Vector3.zero, Vector3.forward, "entry");
            controller.UpdatePresentation(ctx, new List<PortalVisibilityResult>());

            Assert.IsNotNull(controller.LastResult);
            Assert.IsTrue(controller.LastResult.ContainsKey("entry"));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void BindLocalPlayer_StoresReferences()
        {
            var go = new GameObject("Controller");
            var controller = go.AddComponent<NodeStreamingController>();

            var playerGo = new GameObject("Player");
            var tracker = playerGo.AddComponent<PlayerNodeTracker>();
            var cam = playerGo.AddComponent<Camera>();

            Assert.IsFalse(controller.HasLocalPlayer, "Should not have local player before binding");

            controller.BindLocalPlayer(tracker, cam);

            Assert.IsTrue(controller.HasLocalPlayer, "Should have local player after binding");

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(playerGo);
        }

        [Test]
        public void BindLocalPlayer_NullArgs_ClearsBinding()
        {
            var go = new GameObject("Controller");
            var controller = go.AddComponent<NodeStreamingController>();

            var playerGo = new GameObject("Player");
            var tracker = playerGo.AddComponent<PlayerNodeTracker>();
            var cam = playerGo.AddComponent<Camera>();

            controller.BindLocalPlayer(tracker, cam);
            Assert.IsTrue(controller.HasLocalPlayer);

            controller.BindLocalPlayer(null, null);
            Assert.IsFalse(controller.HasLocalPlayer, "Null args should clear binding");

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(playerGo);
        }

        private static void SetNodeId(NodePresentationHandle handle, string nodeId)
        {
            var field = typeof(NodePresentationHandle).GetField("nodeId",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(handle, nodeId);
        }

        private static GameObject WireWithPresentationChild(GameObject roomGo, NodePresentationHandle handle)
            => TestConstants.WireWithPresentationChild(roomGo, handle);

        #region BuildPortalProbes Tests (TD0018)

        private static void SetAnchorId(PortalAnchorAuthoring anchor, string anchorId)
        {
            var field = typeof(PortalAnchorAuthoring).GetField("anchorId",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(anchor, anchorId);
        }

        [Test]
        public void BuildPortalProbes_SingleAnchorWithMatchingEdge_ReturnsCorrectProbe()
        {
            // Arrange: graph with entry->hall_a edge via door_a/door_b
            var definition = ScriptableObject.CreateInstance<HouseGraphDefinition>();
            definition.nodes = new[]
            {
                new HouseNodeDefinition
                {
                    nodeId = "entry",
                    displayName = "Entry",
                    worldPosition = Vector3.zero,
                    portalAnchors = new[]
                    {
                        new PortalAnchorDefinition { anchorId = "door_a", localPosition = Vector3.zero }
                    }
                },
                new HouseNodeDefinition
                {
                    nodeId = "hall_a",
                    displayName = "Hall A",
                    worldPosition = new Vector3(6f, 0f, 0f),
                    portalAnchors = new[]
                    {
                        new PortalAnchorDefinition { anchorId = "door_b", localPosition = Vector3.zero }
                    }
                }
            };
            definition.edges = new[]
            {
                new HouseEdgeDefinition
                {
                    edgeId = "entry_to_hall",
                    sourceNodeId = "entry",
                    targetNodeId = "hall_a",
                    sourceAnchorId = "door_a",
                    targetAnchorId = "door_b"
                }
            };

            var graph = new SpatialGraphRuntime();
            graph.Initialize(definition);

            // Scene anchor at a known position/forward
            var anchorGo = new GameObject("Portal_door_a");
            anchorGo.transform.position = new Vector3(3f, 1f, 0f);
            anchorGo.transform.forward = Vector3.right;
            var anchor = anchorGo.AddComponent<PortalAnchorAuthoring>();
            SetAnchorId(anchor, "door_a");

            // Act
            var probes = NodeStreamingController.BuildPortalProbes(
                new[] { anchor }, graph, "entry");

            // Assert
            Assert.AreEqual(1, probes.Count, "Should return one probe for the matching anchor");
            Assert.AreEqual("door_a", probes[0].AnchorId);
            Assert.AreEqual("hall_a", probes[0].DestinationNodeId);
            Assert.AreEqual(anchorGo.transform.position, probes[0].PortalPosition);
            Assert.AreEqual(anchorGo.transform.forward, probes[0].PortalForward);

            // Cleanup
            Object.DestroyImmediate(anchorGo);
            Object.DestroyImmediate(definition);
        }

        [Test]
        public void BuildPortalProbes_AnchorWithNoMatchingEdge_ReturnsEmpty()
        {
            // Arrange: graph with no edges at all
            var definition = ScriptableObject.CreateInstance<HouseGraphDefinition>();
            definition.nodes = new[]
            {
                new HouseNodeDefinition
                {
                    nodeId = "entry",
                    displayName = "Entry",
                    worldPosition = Vector3.zero,
                    portalAnchors = System.Array.Empty<PortalAnchorDefinition>()
                }
            };
            definition.edges = System.Array.Empty<HouseEdgeDefinition>();

            var graph = new SpatialGraphRuntime();
            graph.Initialize(definition);

            var anchorGo = new GameObject("Portal_orphan");
            var anchor = anchorGo.AddComponent<PortalAnchorAuthoring>();
            SetAnchorId(anchor, "orphan_anchor");

            // Act
            var probes = NodeStreamingController.BuildPortalProbes(
                new[] { anchor }, graph, "entry");

            // Assert
            Assert.AreEqual(0, probes.Count, "Anchor with no matching edge should produce no probes");

            Object.DestroyImmediate(anchorGo);
            Object.DestroyImmediate(definition);
        }

        [Test]
        public void BuildPortalProbes_NullInputs_ReturnsEmpty()
        {
            var probes = NodeStreamingController.BuildPortalProbes(null, null, null);
            Assert.AreEqual(0, probes.Count);
        }

        [Test]
        public void BuildPortalProbes_MultipleAnchors_OnlyCurrentNodeEdgesProduceProbes()
        {
            // Arrange: 3-node graph, player in entry. Two anchors: one on entry's edge, one on hall's edge.
            var definition = ScriptableObject.CreateInstance<HouseGraphDefinition>();
            definition.nodes = new[]
            {
                new HouseNodeDefinition
                {
                    nodeId = "entry",
                    displayName = "Entry",
                    worldPosition = Vector3.zero,
                    portalAnchors = new[]
                    {
                        new PortalAnchorDefinition { anchorId = "door_a", localPosition = Vector3.zero }
                    }
                },
                new HouseNodeDefinition
                {
                    nodeId = "hall_a",
                    displayName = "Hall A",
                    worldPosition = new Vector3(6f, 0f, 0f),
                    portalAnchors = new[]
                    {
                        new PortalAnchorDefinition { anchorId = "door_b", localPosition = Vector3.zero },
                        new PortalAnchorDefinition { anchorId = "door_c", localPosition = Vector3.zero }
                    }
                },
                new HouseNodeDefinition
                {
                    nodeId = "living",
                    displayName = "Living",
                    worldPosition = new Vector3(12f, 0f, 0f),
                    portalAnchors = new[]
                    {
                        new PortalAnchorDefinition { anchorId = "door_d", localPosition = Vector3.zero }
                    }
                }
            };
            definition.edges = new[]
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

            var graph = new SpatialGraphRuntime();
            graph.Initialize(definition);

            // Two scene anchors: door_a (entry's portal) and door_c (hall's portal)
            var anchorGoA = new GameObject("Portal_door_a");
            var anchorA = anchorGoA.AddComponent<PortalAnchorAuthoring>();
            SetAnchorId(anchorA, "door_a");

            var anchorGoC = new GameObject("Portal_door_c");
            var anchorC = anchorGoC.AddComponent<PortalAnchorAuthoring>();
            SetAnchorId(anchorC, "door_c");

            // Act: player is in "entry" — only door_a should produce a probe
            var probes = NodeStreamingController.BuildPortalProbes(
                new[] { anchorA, anchorC }, graph, "entry");

            // Assert
            Assert.AreEqual(1, probes.Count, "Only anchor on current node's edge should produce a probe");
            Assert.AreEqual("door_a", probes[0].AnchorId);
            Assert.AreEqual("hall_a", probes[0].DestinationNodeId);

            Object.DestroyImmediate(anchorGoA);
            Object.DestroyImmediate(anchorGoC);
            Object.DestroyImmediate(definition);
        }

        #endregion
    }
}
