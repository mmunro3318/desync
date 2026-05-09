namespace Desync.World.Graph.Runtime
{
    /// <summary>
    /// Reasons a node or edge is observation-locked against mutation.
    /// Multiple reasons can be active simultaneously on a single target.
    /// </summary>
    public enum LockReason
    {
        None = 0,
        Occupied,
        AdjacentOccupiedEdge,
        PortalVisible,
        GracePeriod,
        DebugForced,

        /// <summary>
        /// Dormant seam for non-observation protection (stable anchors,
        /// higher-priority rules). No Sprint 2 code path sets this value,
        /// but eligibility queries handle it so future protection rules
        /// can be injected without changing the core observation model.
        /// </summary>
        ProtectedByRule,
    }
}
