using UnityEngine;

namespace Desync.World.Graph.Runtime
{
    public class NodePresentationHandle : MonoBehaviour
    {
        [Header("Node Identity")]
        [SerializeField] private string nodeId;

        public string NodeId => nodeId;

        // WARNING: SetActive(false) disables the entire room root, including the
        // BoxCollider trigger that PlayerNodeTracker uses for occupancy detection.
        // If the activation resolver ever incorrectly deactivates the occupied room,
        // the trigger that would re-activate it is also disabled — self-lockout.
        // Currently safe because Occupied always activates the current room.
        // Long-term: separate presentation root (toggled) from tracking root (always active).
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
