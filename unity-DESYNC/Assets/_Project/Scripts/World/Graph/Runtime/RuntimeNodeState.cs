namespace Desync.World.Graph.Runtime
{
    /// <summary>
    /// Per-node runtime state: occupancy count and active/inactive flag.
    /// This is the mutable state that gets synced over the network.
    /// </summary>
    public class RuntimeNodeState
    {
        public string NodeId { get; }
        public bool IsActive { get; private set; }
        public int Occupancy { get; private set; }

        public RuntimeNodeState(string nodeId)
        {
            NodeId = nodeId;
            IsActive = true;
            Occupancy = 0;
        }

        public void IncrementOccupancy() => Occupancy++;

        public void DecrementOccupancy()
        {
            if (Occupancy > 0) Occupancy--;
        }

        public void SetActive(bool active) => IsActive = active;
    }
}
