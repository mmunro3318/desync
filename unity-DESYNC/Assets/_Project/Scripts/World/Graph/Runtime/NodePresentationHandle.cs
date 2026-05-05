using UnityEngine;

namespace Desync.World.Graph.Runtime
{
    public class NodePresentationHandle : MonoBehaviour
    {
        [Header("Node Identity")]
        [SerializeField] private string nodeId;

        public string NodeId => nodeId;

        public void SetPresentation(bool active)
        {
            if (this == null || gameObject == null)
            {
                global::UnityEngine.Debug.LogWarning("[NodePresentationHandle] Cannot set presentation — GameObject is destroyed.");
                return;
            }

            gameObject.SetActive(active);
        }
    }
}
