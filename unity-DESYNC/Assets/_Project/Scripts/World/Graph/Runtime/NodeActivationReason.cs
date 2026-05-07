using System;

namespace Desync.World.Graph.Runtime
{
    [Flags]
    public enum NodeActivationReason
    {
        None = 0,
        Occupied = 1,
        Adjacent = 2,
        PortalVisible = 4,
        DebugForced = 8
    }
}
