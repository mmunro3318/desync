using System.Collections.Generic;

namespace Desync.World.Graph.Runtime
{
    public class NodeActivationResolver
    {
        private static readonly Dictionary<string, NodeActivationReason> EmptyResult = new();

        public IReadOnlyDictionary<string, NodeActivationReason> Resolve(
            ViewContext ctx,
            SpatialGraphRuntime graph,
            IReadOnlyList<PortalVisibilityResult> portalResults)
        {
            return EmptyResult;
        }
    }
}
