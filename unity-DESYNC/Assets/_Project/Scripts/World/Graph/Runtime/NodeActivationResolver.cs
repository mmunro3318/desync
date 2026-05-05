using System.Collections.Generic;

namespace Desync.World.Graph.Runtime
{
    public class NodeActivationResolver
    {
        public IReadOnlyDictionary<string, NodeActivationReason> Resolve(
            ViewContext ctx,
            SpatialGraphRuntime graph,
            IReadOnlyList<PortalVisibilityResult> portalResults)
        {
            // TD0017: Replace with real activation logic. Do NOT mutate a shared
            // static dictionary — return a fresh instance per call to avoid
            // cross-caller corruption in multiplayer frames.
            return new Dictionary<string, NodeActivationReason>();
        }
    }
}
