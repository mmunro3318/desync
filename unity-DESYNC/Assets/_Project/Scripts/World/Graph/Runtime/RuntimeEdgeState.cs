namespace Desync.World.Graph.Runtime
{
    /// <summary>
    /// Per-edge runtime state: whether the passage is open or closed.
    /// Closed edges block portal traversal (used by mutations in S3).
    /// </summary>
    public class RuntimeEdgeState
    {
        public string EdgeId { get; }
        public bool IsOpen { get; private set; }

        public RuntimeEdgeState(string edgeId)
        {
            EdgeId = edgeId;
            IsOpen = true;
        }

        public void SetOpen(bool open) => IsOpen = open;
    }
}
