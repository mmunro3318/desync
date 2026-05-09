using System.Collections.Generic;

namespace Desync.World.Graph.Runtime
{
    /// <summary>
    /// Provides observation facts to the lock system: which nodes are occupied,
    /// which nodes/edges are visible through portals.
    ///
    /// LocalObservationInputSource is the Sprint 2 single-player local adapter.
    /// In co-op, observation truth moves to an authority-owned contribution/aggregation
    /// model — this interface exists to make that replacement possible without
    /// rewriting ObservationLockSystem.
    /// </summary>
    public interface IObservationInputSource
    {
        IReadOnlyList<string> GetOccupiedNodeIds();
        IReadOnlyList<string> GetVisibleNodeIds();
        IReadOnlyList<string> GetVisibleEdgeIds();
    }
}
