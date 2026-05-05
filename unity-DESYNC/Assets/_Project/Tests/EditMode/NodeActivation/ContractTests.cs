using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Desync.World.Graph.Runtime;

namespace Desync.Tests.EditMode.NodeActivation
{
    [TestFixture]
    public class ViewContextTests
    {
        [Test]
        public void Construction_RoundTripsAllFields()
        {
            var ctx = new ViewContext(
                playerId: "player_1",
                cameraPosition: new Vector3(1f, 2f, 3f),
                cameraForward: Vector3.forward,
                occupiedNodeId: "entry"
            );

            Assert.AreEqual("player_1", ctx.PlayerId);
            Assert.AreEqual(new Vector3(1f, 2f, 3f), ctx.CameraPosition);
            Assert.AreEqual(Vector3.forward, ctx.CameraForward);
            Assert.AreEqual("entry", ctx.OccupiedNodeId);
        }
    }

    [TestFixture]
    public class NodeActivationReasonTests
    {
        [Test]
        public void Flags_CanBeCombinedAndTested()
        {
            var reasons = NodeActivationReason.Occupied | NodeActivationReason.PortalVisible;

            Assert.IsTrue(reasons.HasFlag(NodeActivationReason.Occupied));
            Assert.IsTrue(reasons.HasFlag(NodeActivationReason.PortalVisible));
            Assert.IsFalse(reasons.HasFlag(NodeActivationReason.Adjacent));
            Assert.IsFalse(reasons.HasFlag(NodeActivationReason.DebugForced));
        }

        [Test]
        public void None_HasNoFlags()
        {
            var reasons = NodeActivationReason.None;

            Assert.IsFalse(reasons.HasFlag(NodeActivationReason.Occupied));
            Assert.IsFalse(reasons.HasFlag(NodeActivationReason.Adjacent));
            Assert.IsFalse(reasons.HasFlag(NodeActivationReason.PortalVisible));
            Assert.IsFalse(reasons.HasFlag(NodeActivationReason.DebugForced));
        }
    }

    [TestFixture]
    public class NodeActivationResolverTests
    {
        [Test]
        public void Resolve_Stub_ReturnsEmptyDictionary()
        {
            var resolver = new NodeActivationResolver();
            var ctx = new ViewContext("p1", Vector3.zero, Vector3.forward, "entry");
            var portalResults = new List<PortalVisibilityResult>();

            var result = resolver.Resolve(ctx, null, portalResults);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }
    }

    [TestFixture]
    public class PortalVisibilityContractTests
    {
        [Test]
        public void PortalProbeData_RoundTripsAllFields()
        {
            var probe = new PortalProbeData(
                anchorId: "anchor_1",
                destinationNodeId: "hall_a",
                portalPosition: new Vector3(0f, 1f, 2f),
                portalForward: Vector3.right,
                apertureSize: new Vector2(1.2f, 2.4f)
            );

            Assert.AreEqual("anchor_1", probe.AnchorId);
            Assert.AreEqual("hall_a", probe.DestinationNodeId);
            Assert.AreEqual(new Vector3(0f, 1f, 2f), probe.PortalPosition);
            Assert.AreEqual(Vector3.right, probe.PortalForward);
            Assert.AreEqual(new Vector2(1.2f, 2.4f), probe.ApertureSize);
        }

        [Test]
        public void PortalVisibilityResult_RoundTripsAllFields()
        {
            var result = new PortalVisibilityResult(
                anchorId: "anchor_2",
                destinationNodeId: "living",
                isVisible: true
            );

            Assert.AreEqual("anchor_2", result.AnchorId);
            Assert.AreEqual("living", result.DestinationNodeId);
            Assert.IsTrue(result.IsVisible);
        }
    }

    [TestFixture]
    public class NodePresentationHandleTests
    {
        [Test]
        public void SetPresentation_True_ActivatesGameObject()
        {
            var go = new GameObject("TestRoom");
            var handle = go.AddComponent<NodePresentationHandle>();
            go.SetActive(false);

            handle.SetPresentation(true);

            Assert.IsTrue(go.activeSelf);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void SetPresentation_False_DeactivatesGameObject()
        {
            var go = new GameObject("TestRoom");
            var handle = go.AddComponent<NodePresentationHandle>();
            go.SetActive(true);

            handle.SetPresentation(false);

            Assert.IsFalse(go.activeSelf);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void NodeId_ExposesSerializedValue()
        {
            var go = new GameObject("TestRoom");
            var handle = go.AddComponent<NodePresentationHandle>();

            // Default is null/empty
            Assert.IsTrue(string.IsNullOrEmpty(handle.NodeId));
            Object.DestroyImmediate(go);
        }

        [Test]
        public void SetPresentation_AfterDestroy_DoesNotThrow()
        {
            var go = new GameObject("TestRoom");
            var handle = go.AddComponent<NodePresentationHandle>();
            Object.DestroyImmediate(go);

            Assert.DoesNotThrow(() => handle.SetPresentation(true));
        }
    }
}
