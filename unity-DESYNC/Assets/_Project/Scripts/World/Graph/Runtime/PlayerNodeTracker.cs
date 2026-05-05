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
        /// Records entry into a node. Null or empty ids are ignored.
        /// </summary>
        public void EnterNode(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId)) return;
            _previousNodeId = _currentNodeId;
            _currentNodeId = nodeId;
        }

        /// <summary>
        /// Clears the current node only if it matches the exited node id,
        /// guarding against stale exits from a previously vacated room.
        /// </summary>
        public void ExitNode(string nodeId)
        {
            if (_currentNodeId == nodeId) _currentNodeId = null;
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
