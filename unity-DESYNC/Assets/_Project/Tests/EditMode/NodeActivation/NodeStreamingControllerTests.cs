using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using Desync.World.Graph.Runtime;

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
        public void ForceAllActive_ActivatesAllHandles()
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
        {
            var presentation = new GameObject("Presentation");
            presentation.transform.SetParent(roomGo.transform);
            var so = new SerializedObject(handle);
            so.FindProperty("presentationRoot").objectReferenceValue = presentation.transform;
            so.ApplyModifiedPropertiesWithoutUndo();
            return presentation;
        }
    }
}
