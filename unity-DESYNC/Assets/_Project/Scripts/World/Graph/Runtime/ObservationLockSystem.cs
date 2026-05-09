using System.Collections.Generic;
using Desync.World.Graph.Definitions;

namespace Desync.World.Graph.Runtime
{
    /// <summary>
    /// Pure lock-evaluation and query service over externally supplied observation
    /// facts. Tracks which nodes and edges are observation-locked against mutation,
    /// manages grace timers, and exposes eligibility queries.
    ///
    /// This is a Sprint 2 substrate — mutation regions are the intended higher-level
    /// abstraction for anomaly systems. Perception gathering, mutation scheduling,
    /// stable-anchor policy, networking, and perception policy stay outside this class.
    /// </summary>
    public class ObservationLockSystem : IObservationLockQuery
    {
        private readonly IObservationInputSource _input;
        private readonly SpatialGraphRuntime _graph;
        private readonly ObservationRulesDefinition _rules;

        private readonly Dictionary<string, NodeObservationState> _nodeStates = new();
        private readonly Dictionary<string, EdgeObservationState> _edgeStates = new();

        private float _visibilityAccumulator;
        private bool _hasPolledVisibility;
        private HashSet<string> _lastVisibleNodeSet;
        private HashSet<string> _lastVisibleEdgeSet;

        private readonly HashSet<string> _debugForcedLockedNodes = new();
        private readonly HashSet<string> _debugForcedUnlockedNodes = new();
        private readonly HashSet<string> _debugForcedLockedEdges = new();
        private readonly HashSet<string> _debugForcedUnlockedEdges = new();

        private static readonly IReadOnlyList<LockReason> EmptyReasons =
            System.Array.Empty<LockReason>();

        public ObservationLockSystem(
            IObservationInputSource input,
            SpatialGraphRuntime graph,
            ObservationRulesDefinition rules)
        {
            _input = input;
            _graph = graph;
            _rules = rules;
        }

        public void Tick(float deltaTime)
        {
            // Occupancy is always polled every frame
            var occupiedNodeIds = _input.GetOccupiedNodeIds();
            var occupiedSet = new HashSet<string>(occupiedNodeIds);

            // Collect edge IDs adjacent to occupied nodes
            var adjacentEdgeSet = new HashSet<string>();
            for (int i = 0; i < occupiedNodeIds.Count; i++)
            {
                var edges = _graph.GetConnectedEdges(occupiedNodeIds[i]);
                for (int e = 0; e < edges.Count; e++)
                    adjacentEdgeSet.Add(edges[e].edgeId);
            }

            // Visibility polling gated by refresh interval accumulator
            bool shouldPollVisibility = ShouldPollVisibility(deltaTime);
            HashSet<string> visibleNodeSet;
            HashSet<string> visibleEdgeSet;

            if (shouldPollVisibility)
            {
                visibleNodeSet = new HashSet<string>(_input.GetVisibleNodeIds());
                visibleEdgeSet = new HashSet<string>(_input.GetVisibleEdgeIds());
                _lastVisibleNodeSet = visibleNodeSet;
                _lastVisibleEdgeSet = visibleEdgeSet;
            }
            else
            {
                // Reuse last polled visibility data
                visibleNodeSet = _lastVisibleNodeSet ?? new HashSet<string>();
                visibleEdgeSet = _lastVisibleEdgeSet ?? new HashSet<string>();
            }

            // Ensure state entries exist for all observed targets
            EnsureNodeEntries(occupiedSet, visibleNodeSet);
            EnsureEdgeEntries(adjacentEdgeSet, visibleEdgeSet);

            UpdateNodes(occupiedSet, visibleNodeSet, deltaTime);
            UpdateEdges(adjacentEdgeSet, visibleEdgeSet, deltaTime);
        }

        private bool ShouldPollVisibility(float deltaTime)
        {
            // First tick always polls
            if (!_hasPolledVisibility)
            {
                _hasPolledVisibility = true;
                return true;
            }

            float interval = _rules.visibilityRefreshInterval;

            // <= 0 means every frame
            if (interval <= 0f)
                return true;

            _visibilityAccumulator += deltaTime;
            if (_visibilityAccumulator >= interval)
            {
                _visibilityAccumulator -= interval;
                return true;
            }

            return false;
        }

        private void UpdateNodes(
            HashSet<string> occupiedSet,
            HashSet<string> visibleNodeSet,
            float deltaTime)
        {
            // Snapshot keys to avoid modification during iteration
            var nodeIds = new List<string>(_nodeStates.Keys);

            for (int i = 0; i < nodeIds.Count; i++)
            {
                var nodeId = nodeIds[i];
                var state = _nodeStates[nodeId];
                bool wasLocked = state.IsLocked;

                // Rebuild reasons from scratch each tick (preserve grace timer)
                state.ClearReasons();

                if (occupiedSet.Contains(nodeId))
                    state.AddReason(LockReason.Occupied);

                if (visibleNodeSet.Contains(nodeId))
                    state.AddReason(LockReason.PortalVisible);

                if (_debugForcedLockedNodes.Contains(nodeId))
                    state.AddReason(LockReason.DebugForced);

                // Force-unlock: strip all reasons so node appears unlocked
                if (_debugForcedUnlockedNodes.Contains(nodeId))
                {
                    state.Clear();
                    _nodeStates[nodeId] = state;
                    continue;
                }

                // Grace: was locked, now no active reasons → start grace
                if (wasLocked && !state.IsLocked && state.GraceRemaining <= 0f)
                    state.StartGrace(_rules.nodeGraceSeconds);

                // Tick grace if in grace period and not re-locked
                if (!state.IsLocked && state.GraceRemaining > 0f)
                    state.TickGrace(deltaTime);

                _nodeStates[nodeId] = state;
            }
        }

        private void UpdateEdges(
            HashSet<string> adjacentEdgeSet,
            HashSet<string> visibleEdgeSet,
            float deltaTime)
        {
            var edgeIds = new List<string>(_edgeStates.Keys);

            for (int i = 0; i < edgeIds.Count; i++)
            {
                var edgeId = edgeIds[i];
                var state = _edgeStates[edgeId];
                bool wasLocked = state.IsLocked;

                state.ClearReasons();

                if (adjacentEdgeSet.Contains(edgeId))
                    state.AddReason(LockReason.AdjacentOccupiedEdge);

                if (visibleEdgeSet.Contains(edgeId))
                    state.AddReason(LockReason.PortalVisible);

                if (_debugForcedLockedEdges.Contains(edgeId))
                    state.AddReason(LockReason.DebugForced);

                if (_debugForcedUnlockedEdges.Contains(edgeId))
                {
                    state.Clear();
                    _edgeStates[edgeId] = state;
                    continue;
                }

                if (wasLocked && !state.IsLocked && state.GraceRemaining <= 0f)
                    state.StartGrace(_rules.edgeGraceSeconds);

                if (!state.IsLocked && state.GraceRemaining > 0f)
                    state.TickGrace(deltaTime);

                _edgeStates[edgeId] = state;
            }
        }

        private void EnsureNodeEntries(
            HashSet<string> occupiedSet,
            HashSet<string> visibleNodeSet)
        {
            foreach (var id in occupiedSet)
                if (!_nodeStates.ContainsKey(id))
                    _nodeStates[id] = new NodeObservationState();

            foreach (var id in visibleNodeSet)
                if (!_nodeStates.ContainsKey(id))
                    _nodeStates[id] = new NodeObservationState();
        }

        private void EnsureEdgeEntries(
            HashSet<string> adjacentEdgeSet,
            HashSet<string> visibleEdgeSet)
        {
            foreach (var id in adjacentEdgeSet)
                if (!_edgeStates.ContainsKey(id))
                    _edgeStates[id] = new EdgeObservationState();

            foreach (var id in visibleEdgeSet)
                if (!_edgeStates.ContainsKey(id))
                    _edgeStates[id] = new EdgeObservationState();
        }

        public void ForceNodeLock(string nodeId)
        {
            _debugForcedUnlockedNodes.Remove(nodeId);
            _debugForcedLockedNodes.Add(nodeId);
            if (!_nodeStates.ContainsKey(nodeId))
                _nodeStates[nodeId] = new NodeObservationState();
            var state = _nodeStates[nodeId];
            state.AddReason(LockReason.DebugForced);
            _nodeStates[nodeId] = state;
        }

        public void ForceNodeUnlock(string nodeId)
        {
            _debugForcedLockedNodes.Remove(nodeId);
            _debugForcedUnlockedNodes.Add(nodeId);
        }

        public void ForceEdgeLock(string edgeId)
        {
            _debugForcedUnlockedEdges.Remove(edgeId);
            _debugForcedLockedEdges.Add(edgeId);
            if (!_edgeStates.ContainsKey(edgeId))
                _edgeStates[edgeId] = new EdgeObservationState();
            var state = _edgeStates[edgeId];
            state.AddReason(LockReason.DebugForced);
            _edgeStates[edgeId] = state;
        }

        public void ForceEdgeUnlock(string edgeId)
        {
            _debugForcedLockedEdges.Remove(edgeId);
            _debugForcedUnlockedEdges.Add(edgeId);
        }

        public void ClearDebugOverrides()
        {
            _debugForcedLockedNodes.Clear();
            _debugForcedUnlockedNodes.Clear();
            _debugForcedLockedEdges.Clear();
            _debugForcedUnlockedEdges.Clear();
        }

        public void Reset()
        {
            _nodeStates.Clear();
            _edgeStates.Clear();
            _visibilityAccumulator = 0f;
            _hasPolledVisibility = false;
            _lastVisibleNodeSet = null;
            _lastVisibleEdgeSet = null;
            ClearDebugOverrides();
        }

        #region IObservationLockQuery

        public bool IsNodeLocked(string nodeId)
        {
            if (_nodeStates.TryGetValue(nodeId, out var state))
                return state.IsLocked;
            return false;
        }

        public bool IsEdgeLocked(string edgeId)
        {
            if (_edgeStates.TryGetValue(edgeId, out var state))
                return state.IsLocked;
            return false;
        }

        public bool IsNodeMutationEligible(string nodeId)
        {
            if (_nodeStates.TryGetValue(nodeId, out var state))
                return state.IsMutationEligible;
            return true;
        }

        public bool IsEdgeMutationEligible(string edgeId)
        {
            if (_edgeStates.TryGetValue(edgeId, out var state))
                return state.IsMutationEligible;
            return true;
        }

        public IReadOnlyList<LockReason> GetNodeLockReasons(string nodeId)
        {
            if (_nodeStates.TryGetValue(nodeId, out var state))
                return state.ActiveReasons;
            return EmptyReasons;
        }

        public IReadOnlyList<LockReason> GetEdgeLockReasons(string edgeId)
        {
            if (_edgeStates.TryGetValue(edgeId, out var state))
                return state.ActiveReasons;
            return EmptyReasons;
        }

        public float GetNodeGraceRemaining(string nodeId)
        {
            if (_nodeStates.TryGetValue(nodeId, out var state))
                return state.GraceRemaining;
            return 0f;
        }

        public float GetEdgeGraceRemaining(string edgeId)
        {
            if (_edgeStates.TryGetValue(edgeId, out var state))
                return state.GraceRemaining;
            return 0f;
        }

        public IReadOnlyDictionary<string, NodeObservationState> GetAllNodeStates()
            => _nodeStates;

        public IReadOnlyDictionary<string, EdgeObservationState> GetAllEdgeStates()
            => _edgeStates;

        #endregion
    }
}
