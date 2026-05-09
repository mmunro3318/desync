using System.Collections.Generic;

namespace Desync.World.Graph.Runtime
{
    /// <summary>
    /// Per-node observation lock state. Tracks active lock reasons and grace timer.
    /// Sprint 2 substrate — mutation regions are the intended higher-level
    /// abstraction for anomaly systems.
    /// </summary>
    public struct NodeObservationState
    {
        private List<LockReason> _activeReasons;
        private float _graceRemaining;

        public IReadOnlyList<LockReason> ActiveReasons =>
            _activeReasons ?? (IReadOnlyList<LockReason>)System.Array.Empty<LockReason>();

        public float GraceRemaining => _graceRemaining > 0f ? _graceRemaining : 0f;

        public bool IsLocked => _activeReasons != null && _activeReasons.Count > 0;

        public bool IsMutationEligible => !IsLocked && _graceRemaining <= 0f;

        public void AddReason(LockReason reason)
        {
            _activeReasons ??= new List<LockReason>(4);
            if (!_activeReasons.Contains(reason))
                _activeReasons.Add(reason);
            _graceRemaining = 0f;
        }

        public void RemoveReason(LockReason reason)
        {
            _activeReasons?.Remove(reason);
        }

        public void StartGrace(float duration)
        {
            _graceRemaining = duration;
        }

        public void TickGrace(float deltaTime)
        {
            if (_graceRemaining > 0f)
            {
                _graceRemaining -= deltaTime;
                if (_graceRemaining < 0f)
                    _graceRemaining = 0f;
            }
        }

        /// <summary>
        /// Clears active reasons only, preserving grace timer.
        /// Used during per-tick reason rebuild.
        /// </summary>
        public void ClearReasons()
        {
            _activeReasons?.Clear();
        }

        /// <summary>
        /// Clears all state including grace timer. Used for Reset().
        /// </summary>
        public void Clear()
        {
            _activeReasons?.Clear();
            _graceRemaining = 0f;
        }
    }
}
