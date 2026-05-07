using UnityEngine;
using UnityEditor;
using Desync.World.Graph.Runtime;

namespace Desync.Tests.EditMode.NodeActivation
{
    internal static class TestConstants
    {
        internal const string PresentationRootField = "presentationRoot";

        /// <summary>
        /// Creates a "Presentation" child under <paramref name="roomGo"/>, wires it into the
        /// handle's serialized presentationRoot field, and returns the child GameObject.
        /// </summary>
        internal static GameObject WireWithPresentationChild(
            GameObject roomGo, NodePresentationHandle handle)
        {
            var presentation = new GameObject("Presentation");
            presentation.transform.SetParent(roomGo.transform);
            var so = new SerializedObject(handle);
            so.FindProperty(PresentationRootField).objectReferenceValue = presentation.transform;
            so.ApplyModifiedPropertiesWithoutUndo();
            return presentation;
        }
    }
}
