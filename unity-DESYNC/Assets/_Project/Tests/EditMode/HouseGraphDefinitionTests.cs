using NUnit.Framework;
using UnityEngine;
using Desync.World.Graph.Definitions;

namespace Desync.Tests.EditMode
{
    public class HouseGraphDefinitionTests
    {
        [Test]
        public void CreateInstance_ReturnsNonNull()
        {
            var definition = ScriptableObject.CreateInstance<HouseGraphDefinition>();
            Assert.IsNotNull(definition);
            Object.DestroyImmediate(definition);
        }

        [Test]
        public void Nodes_DefaultsToEmptyArray()
        {
            var definition = ScriptableObject.CreateInstance<HouseGraphDefinition>();
            Assert.IsNotNull(definition.nodes);
            Assert.AreEqual(0, definition.nodes.Length);
            Object.DestroyImmediate(definition);
        }

        [Test]
        public void Edges_DefaultsToEmptyArray()
        {
            var definition = ScriptableObject.CreateInstance<HouseGraphDefinition>();
            Assert.IsNotNull(definition.edges);
            Assert.AreEqual(0, definition.edges.Length);
            Object.DestroyImmediate(definition);
        }

        [Test]
        public void NodeDefinition_StoresIdAndDisplayName()
        {
            var node = new HouseNodeDefinition
            {
                nodeId = "entry",
                displayName = "Entry Hall"
            };

            Assert.AreEqual("entry", node.nodeId);
            Assert.AreEqual("Entry Hall", node.displayName);
        }

        [Test]
        public void EdgeDefinition_StoresSourceAndTargetWithAnchors()
        {
            var edge = new HouseEdgeDefinition
            {
                edgeId = "entry_to_hall",
                sourceNodeId = "entry",
                targetNodeId = "hall_a",
                sourceAnchorId = "door_a",
                targetAnchorId = "door_b"
            };

            Assert.AreEqual("entry_to_hall", edge.edgeId);
            Assert.AreEqual("entry", edge.sourceNodeId);
            Assert.AreEqual("hall_a", edge.targetNodeId);
            Assert.AreEqual("door_a", edge.sourceAnchorId);
            Assert.AreEqual("door_b", edge.targetAnchorId);
        }

        [Test]
        public void PortalAnchorDefinition_StoresIdAndLocalPosition()
        {
            var anchor = new PortalAnchorDefinition
            {
                anchorId = "door_a",
                localPosition = new Vector3(1f, 0f, 2.5f),
                localRotation = Quaternion.identity
            };

            Assert.AreEqual("door_a", anchor.anchorId);
            Assert.AreEqual(new Vector3(1f, 0f, 2.5f), anchor.localPosition);
            Assert.AreEqual(Quaternion.identity, anchor.localRotation);
        }

        [Test]
        public void NodeDefinition_StoresPortalAnchors()
        {
            var node = new HouseNodeDefinition
            {
                nodeId = "entry",
                displayName = "Entry Hall",
                portalAnchors = new[]
                {
                    new PortalAnchorDefinition { anchorId = "door_a" },
                    new PortalAnchorDefinition { anchorId = "door_b" }
                }
            };

            Assert.AreEqual(2, node.portalAnchors.Length);
            Assert.AreEqual("door_a", node.portalAnchors[0].anchorId);
            Assert.AreEqual("door_b", node.portalAnchors[1].anchorId);
        }

        [Test]
        public void Validate_DetectsDuplicateNodeIds()
        {
            var definition = ScriptableObject.CreateInstance<HouseGraphDefinition>();
            definition.nodes = new[]
            {
                new HouseNodeDefinition { nodeId = "entry" },
                new HouseNodeDefinition { nodeId = "entry" }
            };

            var errors = definition.Validate();

            Assert.IsTrue(errors.Count > 0);
            Assert.IsTrue(errors.Exists(e => e.Contains("duplicate", System.StringComparison.OrdinalIgnoreCase)
                                            && e.Contains("entry")));
            Object.DestroyImmediate(definition);
        }

        [Test]
        public void Validate_DetectsDuplicateEdgeIds()
        {
            var definition = ScriptableObject.CreateInstance<HouseGraphDefinition>();
            definition.nodes = new[]
            {
                new HouseNodeDefinition { nodeId = "entry", portalAnchors = new[] { new PortalAnchorDefinition { anchorId = "door_a" } } },
                new HouseNodeDefinition { nodeId = "hall", portalAnchors = new[] { new PortalAnchorDefinition { anchorId = "door_b" } } }
            };
            definition.edges = new[]
            {
                new HouseEdgeDefinition { edgeId = "e1", sourceNodeId = "entry", targetNodeId = "hall", sourceAnchorId = "door_a", targetAnchorId = "door_b" },
                new HouseEdgeDefinition { edgeId = "e1", sourceNodeId = "hall", targetNodeId = "entry", sourceAnchorId = "door_b", targetAnchorId = "door_a" }
            };

            var errors = definition.Validate();

            Assert.IsTrue(errors.Exists(e => e.Contains("duplicate", System.StringComparison.OrdinalIgnoreCase)
                                            && e.Contains("e1")));
            Object.DestroyImmediate(definition);
        }

        [Test]
        public void Validate_DetectsEdgeReferencingNonexistentNode()
        {
            var definition = ScriptableObject.CreateInstance<HouseGraphDefinition>();
            definition.nodes = new[]
            {
                new HouseNodeDefinition { nodeId = "entry", portalAnchors = new[] { new PortalAnchorDefinition { anchorId = "door_a" } } }
            };
            definition.edges = new[]
            {
                new HouseEdgeDefinition { edgeId = "e1", sourceNodeId = "entry", targetNodeId = "ghost_room", sourceAnchorId = "door_a", targetAnchorId = "door_b" }
            };

            var errors = definition.Validate();

            Assert.IsTrue(errors.Exists(e => e.Contains("ghost_room")));
            Object.DestroyImmediate(definition);
        }

        [Test]
        public void Validate_DetectsEdgeReferencingNonexistentAnchor()
        {
            var definition = ScriptableObject.CreateInstance<HouseGraphDefinition>();
            definition.nodes = new[]
            {
                new HouseNodeDefinition { nodeId = "entry", portalAnchors = new[] { new PortalAnchorDefinition { anchorId = "door_a" } } },
                new HouseNodeDefinition { nodeId = "hall", portalAnchors = new[] { new PortalAnchorDefinition { anchorId = "door_b" } } }
            };
            definition.edges = new[]
            {
                new HouseEdgeDefinition { edgeId = "e1", sourceNodeId = "entry", targetNodeId = "hall", sourceAnchorId = "door_a", targetAnchorId = "nonexistent" }
            };

            var errors = definition.Validate();

            Assert.IsTrue(errors.Exists(e => e.Contains("nonexistent")));
            Object.DestroyImmediate(definition);
        }

        [Test]
        public void Validate_DetectsNullNodeId()
        {
            var definition = ScriptableObject.CreateInstance<HouseGraphDefinition>();
            definition.nodes = new[]
            {
                new HouseNodeDefinition { nodeId = null, displayName = "Unnamed" }
            };

            var errors = definition.Validate();

            Assert.IsTrue(errors.Count > 0);
            Assert.IsTrue(errors.Exists(e => e.Contains("null", System.StringComparison.OrdinalIgnoreCase)
                                            || e.Contains("empty", System.StringComparison.OrdinalIgnoreCase)));
            Object.DestroyImmediate(definition);
        }

        [Test]
        public void Validate_DetectsEmptyNodeId()
        {
            var definition = ScriptableObject.CreateInstance<HouseGraphDefinition>();
            definition.nodes = new[]
            {
                new HouseNodeDefinition { nodeId = "", displayName = "Unnamed" }
            };

            var errors = definition.Validate();

            Assert.IsTrue(errors.Count > 0);
            Object.DestroyImmediate(definition);
        }

        [Test]
        public void Validate_DetectsDuplicateAnchorIdsWithinNode()
        {
            var definition = ScriptableObject.CreateInstance<HouseGraphDefinition>();
            definition.nodes = new[]
            {
                new HouseNodeDefinition
                {
                    nodeId = "entry",
                    portalAnchors = new[]
                    {
                        new PortalAnchorDefinition { anchorId = "door_a" },
                        new PortalAnchorDefinition { anchorId = "door_a" }
                    }
                }
            };

            var errors = definition.Validate();

            Assert.IsTrue(errors.Count > 0);
            Assert.IsTrue(errors.Exists(e => e.Contains("door_a")));
            Object.DestroyImmediate(definition);
        }

        [Test]
        public void Validate_WarnsOnZeroQuaternionRotation()
        {
            var definition = ScriptableObject.CreateInstance<HouseGraphDefinition>();
            definition.nodes = new[]
            {
                new HouseNodeDefinition
                {
                    nodeId = "room",
                    portalAnchors = new[]
                    {
                        // localRotation omitted — zero-inits to (0,0,0,0)
                        new PortalAnchorDefinition { anchorId = "portal_a", localPosition = Vector3.forward }
                    }
                }
            };

            var errors = definition.Validate();

            Assert.IsTrue(errors.Exists(e => e.Contains("portal_a") && e.Contains("quaternion", System.StringComparison.OrdinalIgnoreCase)),
                "Validate should warn about zero quaternion on portal_a");
            Object.DestroyImmediate(definition);
        }

        [Test]
        public void Validate_ReturnsEmptyForValidGraph()
        {
            var definition = ScriptableObject.CreateInstance<HouseGraphDefinition>();
            definition.nodes = new[]
            {
                new HouseNodeDefinition { nodeId = "entry", portalAnchors = new[] { new PortalAnchorDefinition { anchorId = "door_a", localRotation = Quaternion.identity } } },
                new HouseNodeDefinition { nodeId = "hall", portalAnchors = new[] { new PortalAnchorDefinition { anchorId = "door_b", localRotation = Quaternion.identity } } }
            };
            definition.edges = new[]
            {
                new HouseEdgeDefinition { edgeId = "e1", sourceNodeId = "entry", targetNodeId = "hall", sourceAnchorId = "door_a", targetAnchorId = "door_b" }
            };

            var errors = definition.Validate();

            Assert.AreEqual(0, errors.Count, "Valid graph should produce no validation errors");
            Object.DestroyImmediate(definition);
        }
    }
}
