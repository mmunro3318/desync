using System.Collections.Generic;
using Desync.World.Graph.Runtime;

namespace Desync.Tests.EditMode
{
    /// <summary>
    /// Test double for IObservationInputSource. Settable lists let tests
    /// control exactly what observation facts the lock system sees.
    /// </summary>
    internal class FakeObservationInputSource : IObservationInputSource
    {
        public List<string> OccupiedNodeIds = new();
        public List<string> VisibleNodeIds = new();
        public List<string> VisibleEdgeIds = new();

        public IReadOnlyList<string> GetOccupiedNodeIds() => OccupiedNodeIds;
        public IReadOnlyList<string> GetVisibleNodeIds() => VisibleNodeIds;
        public IReadOnlyList<string> GetVisibleEdgeIds() => VisibleEdgeIds;
    }
}
