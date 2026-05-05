using UnityEngine;

namespace Desync.World.Graph.Runtime
{
    public readonly struct ViewContext
    {
        public readonly string PlayerId;
        public readonly Vector3 CameraPosition;
        public readonly Vector3 CameraForward;
        public readonly string OccupiedNodeId;

        public ViewContext(string playerId, Vector3 cameraPosition, Vector3 cameraForward, string occupiedNodeId)
        {
            PlayerId = playerId;
            CameraPosition = cameraPosition;
            CameraForward = cameraForward;
            OccupiedNodeId = occupiedNodeId;
        }
    }
}
