using UnityEngine;

namespace Desync.World.Graph.Authoring
{
    /// <summary>
    /// Scene-to-graph bridge for room nodes. Place on the root GameObject
    /// of each room prefab. The nodeId must match an entry in the
    /// HouseGraphDefinition ScriptableObject.
    /// </summary>
    public class RoomNodeAuthoring : MonoBehaviour
    {
        [Header("Graph Identity")]
        [Tooltip("Must match a nodeId in the HouseGraphDefinition asset")]
        [SerializeField] private string nodeId;

        [Header("Room Volume")]
        [Tooltip("BoxCollider trigger used for GetNodeForPosition (player-to-node resolver)")]
        [SerializeField] private BoxCollider roomVolume;

        public string NodeId => nodeId;
        public BoxCollider RoomVolume => roomVolume;

        private void OnValidate()
        {
            if (roomVolume != null && !roomVolume.isTrigger)
            {
                Debug.LogWarning($"[RoomNodeAuthoring] Room volume on '{gameObject.name}' must be a trigger. Setting isTrigger = true.", this);
                roomVolume.isTrigger = true;
            }
        }
    }
}
