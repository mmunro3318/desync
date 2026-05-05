using UnityEngine;

namespace Desync.World.Graph.Authoring
{
    /// <summary>
    /// Scene-to-graph bridge for portal anchors (doorways). Place on a
    /// child GameObject of the room prefab at the doorway position.
    /// The anchorId must match an entry in the parent node's
    /// portalAnchors array in HouseGraphDefinition.
    /// </summary>
    public class PortalAnchorAuthoring : MonoBehaviour
    {
        [Header("Anchor Identity")]
        [Tooltip("Must match an anchorId in the parent node's portalAnchors array")]
        [SerializeField] private string anchorId;

        [Header("Crossing Detection")]
        [Tooltip("BoxCollider trigger for detecting player doorway crossing")]
        [SerializeField] private BoxCollider crossingTrigger;

        public string AnchorId => anchorId;
        public BoxCollider CrossingTrigger => crossingTrigger;

        private void OnValidate()
        {
            if (crossingTrigger != null && !crossingTrigger.isTrigger)
            {
                global::UnityEngine.Debug.LogWarning($"[PortalAnchorAuthoring] Crossing trigger on '{gameObject.name}' must be a trigger. Setting isTrigger = true.", this);
                crossingTrigger.isTrigger = true;
            }
        }
    }
}
