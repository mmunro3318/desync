using UnityEngine;
using Desync.World.Graph.Authoring;

namespace Desync.World.Graph.Runtime
{
    /// <summary>
    /// Tracks which graph node the local player currently occupies.
    /// Listens for trigger enter/exit events from RoomNodeAuthoring colliders
    /// and maintains a simple current/previous node state machine.
    /// Attach to the player GameObject alongside a Collider.
    /// </summary>
    public class PlayerNodeTracker : MonoBehaviour
    {
        private string _currentNodeId;
        private string _previousNodeId;

        public string CurrentNodeId => _currentNodeId;
        public string PreviousNodeId => _previousNodeId;

        /// <summary>
        /// Records entry into a node. When entering from the void (null zone),
        /// previous is set to null to reflect the gap in traversal.
        /// </summary>
        public void EnterNode(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId)) return;
            if (_currentNodeId == nodeId) return; // already in this node
            _previousNodeId = _currentNodeId; // null if arriving from void
            _currentNodeId = nodeId;
        }

        /// <summary>
        /// Records exit from a node. Only clears current if it matches the
        /// exited node id, guarding against stale exits when triggers overlap
        /// during room-to-room transitions.
        /// </summary>
        public void ExitNode(string nodeId)
        {
            if (_currentNodeId != nodeId) return; // stale exit from old room
            _previousNodeId = _currentNodeId;
            _currentNodeId = null;
        }

        private void OnTriggerEnter(Collider other)
        {
            var room = other.GetComponent<RoomNodeAuthoring>();
            if (room != null) EnterNode(room.NodeId);
        }

        private void OnTriggerExit(Collider other)
        {
            var room = other.GetComponent<RoomNodeAuthoring>();
            if (room != null) ExitNode(room.NodeId);
        }
    }
}
