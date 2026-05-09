using System.Collections.Generic;

namespace Desync.World.Graph.Runtime
{
    /// <summary>
    /// Query interface for observation lock state. Consumed by mutation systems
    /// and debug overlays. ObservationLockSystem implements this.
    /// </summary>
    public interface IObservationLockQuery
    {
        bool IsNodeLocked(string nodeId);
        bool IsEdgeLocked(string edgeId);
        bool IsNodeMutationEligible(string nodeId);
        bool IsEdgeMutationEligible(string edgeId);
        IReadOnlyList<LockReason> GetNodeLockReasons(string nodeId);
        IReadOnlyList<LockReason> GetEdgeLockReasons(string edgeId);
        float GetNodeGraceRemaining(string nodeId);
        float GetEdgeGraceRemaining(string edgeId);
        IReadOnlyDictionary<string, NodeObservationState> GetAllNodeStates();
        IReadOnlyDictionary<string, EdgeObservationState> GetAllEdgeStates();
    }
}
