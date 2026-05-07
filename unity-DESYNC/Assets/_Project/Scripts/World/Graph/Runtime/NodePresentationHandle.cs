using UnityEngine;

namespace Desync.World.Graph.Runtime
{
    public class NodePresentationHandle : MonoBehaviour
    {
        [Header("Node Identity")]
        [SerializeField] private string nodeId;

        [Header("Presentation")]
        [SerializeField] private Transform presentationRoot;

        public string NodeId => nodeId;

        public void SetPresentation(bool active)
        {
            if (this == null || gameObject == null)
            {
                global::UnityEngine.Debug.LogWarning("[NodePresentationHandle] Cannot set presentation — GameObject is destroyed.");
                return;
            }

            if (presentationRoot == null)
            {
                global::UnityEngine.Debug.LogWarning($"[NodePresentationHandle] presentationRoot is null on '{gameObject.name}'. Assign in Inspector.");
                return;
            }

            presentationRoot.gameObject.SetActive(active);
        }

        private void OnValidate()
        {
            if (presentationRoot == null)
                global::UnityEngine.Debug.LogWarning($"[NodePresentationHandle] presentationRoot is not assigned on '{name}'.", this);
        }
    }
}
