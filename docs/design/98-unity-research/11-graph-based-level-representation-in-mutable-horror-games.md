# Graph-Based Level Representation in Mutable Horror Game Worlds

A comprehensive survey of spatial graph data structures, mutation patterns, observation-gating, incremental pathfinding, and serialization for a horror game with a dynamically reconfiguring house.

***

## 1. Data Structures for Spatial Level Graphs

### Core Options and Their Tradeoffs

Three representations dominate spatial graph use in games: adjacency matrix, adjacency list, and half-edge (doubly-linked edge list). For a sparse, mutable horror-house graph the adjacency list wins on nearly every axis.

| Structure | Neighbor lookup | Edge exists? | Add node | Remove node | Memory | Mutation speed |
|---|---|---|---|---|---|---|
| Adjacency matrix | O(V) | O(1) | O(V²) rebuild | O(V²) rebuild | O(V²) | Very slow on insert/delete |
| Adjacency list (List) | O(k) | O(k) | O(1) | O(E) scan | O(V+E) | Fast |
| Adjacency list (HashSet neighbors) | O(1) avg | O(1) avg | O(1) | O(1) avg | O(V+E) | Fastest |
| Half-edge / DCEL | O(1) per half-edge | O(1) | O(1) amortized | O(1) amortized | O(E) twin/next ptrs | Fast for manifold topology |

An adjacency matrix uses O(V²) memory and requires an O(V²) rebuild every time you insert or delete a node — catastrophic for a graph that mutates at runtime. For sparse graphs (a house has ~4 doors per room, not 50), an adjacency list using `Dictionary<RoomNode, HashSet<Portal>>` gives O(1) average neighbor lookup and O(1) edge insertion.[^1][^2]

The **half-edge (DCEL)** structure is compelling when you need manifold guarantees: every directed half-edge stores a twin (opposite direction), a next (around its face), and a vertex. This makes winding-order and bidirectionality invariants trivially checkable as pointer comparisons instead of costly vector math. It is the structure used in geometry kernels (Geometry Central) because it satisfies both contiguous storage and O(1) amortized updates. However, half-edge is overkill if your "faces" are just rooms and "edges" are door openings — adopt it only if you model room polygonal footprints and need topological adjacency queries.[^3][^4][^5]

**Recommended architecture** for a mutable horror house: a `Dictionary<Guid, RoomNode>` node registry with `HashSet<Portal>` neighbor sets per node (adjacency list), plus a parallel `Dictionary<Guid, Portal>` portal registry for O(1) edge-by-id lookup. This decouples node identity from index so you can insert and remove without invalidating existing references.

```csharp
// Core types
[Serializable]
public class RoomNode {
    public Guid Id { get; } = Guid.NewGuid();
    public string Label;
    public ObservationState ObsState = ObservationState.Unobserved;
    public List<Portal> Portals = new(); // outgoing half-edges
    // Spatial bounds for renderer / AI
    public Bounds WorldBounds;
}

[Serializable]
public class Portal {
    public Guid Id { get; } = Guid.NewGuid();
    public RoomNode From;
    public RoomNode To;
    public Portal Twin; // half-edge twin — always keep in sync
    // Transform that maps local coords in From → local coords in To
    public Matrix4x4 TransitionMatrix;
    public bool IsLocked;
}

public class LevelGraph {
    readonly Dictionary<Guid, RoomNode> _nodes = new();
    readonly Dictionary<Guid, Portal>   _portals = new();

    public RoomNode AddRoom(string label) {
        var node = new RoomNode { Label = label };
        _nodes[node.Id] = node;
        return node;
    }

    // Creates a bidirectional portal pair (twin half-edges)
    public (Portal fwd, Portal rev) AddBidirectionalPortal(RoomNode a, RoomNode b,
            Matrix4x4 aToB) {
        var fwd = new Portal { From = a, To = b, TransitionMatrix = aToB };
        var rev = new Portal { From = b, To = a, TransitionMatrix = aToB.inverse };
        fwd.Twin = rev;
        rev.Twin = fwd;
        a.Portals.Add(fwd);
        b.Portals.Add(rev);
        _portals[fwd.Id] = fwd;
        _portals[rev.Id] = rev;
        return (fwd, rev);
    }
}
```

***

## 2. Existing Implementations in Games and Engines

### Cell-and-Portal Graph (Quake / Source Engine)

The canonical production model is the **cell-and-portal graph**: each convex spatial region is a *cell* (leaf/sector), and each opening between cells is a *portal* (directed edge). In Quake's map toolchain, the `bsp` compiler generates portals automatically, then the `vis` tool pre-computes a Potentially Visible Set (PVS) — a per-leaf bitmask of which other leaves could possibly be seen. The portal file format is minimal: number of points, two leaf indices, then the 3D polygon.[^6][^7]

```
PRT1
11           <- leaf count
12           <- portal count
4 0 1 (880 -224 -8) (880 -272 -8) (880 -272 72) (880 -224 72)
```

At runtime the portal *data structure* is gone; the engine walks the pre-baked PVS bitfield. Source Engine extends this with "clusters" of leaves, and Portal 2 adds a portal stencil depth parameter (`r_portal_stencil_depth`) controlling recursive look-through depth. None of this is mutable at runtime — it is a static precomputed graph — but it provides the canonical vocabulary (cells, portals, directed edges, twin half-edges) that your mutable graph should borrow.[^8][^7][^6]

### Antichamber (Unreal Engine 3)

Antichamber is the closest published precedent for your horror game. Alexander Bruce implemented its non-Euclidean effect not with custom geometry but with **room-swap triggers**: warp zones that instantly teleport the player when they cross a threshold. The "world" is a collection of disconnected Unreal levels co-located at the origin; entering a doorway fires a trigger that loads the adjacent room at the correct relative offset and teleports the player. The effect that "turning around leads to a different room" is achieved by placing different teleport destinations on the *back* face of a doorway brush than the *front* face. The graph topology is hand-authored in Unreal Script; mutation happens by changing which level a trigger references — a runtime graph edge reroute.[^9][^10][^11]

### Manifold Garden (Unity)

William Chyr's 2019 puzzle game wraps space recursively (falling off a floor leads back to the ceiling). The implementation splits levels into **main levels** (positioned at world origin 0,0,0) and **hallway levels** (positioned far away, e.g. offset 100,000 units) — each a separate Unity Scene. Transitions load both scenes simultaneously and perform a seamless handoff. This is a subgraph-attachment operation: the hallway scene is inserted as an intermediate node in the graph, with entry and exit portals wired to its endpoints.[^12][^13]

### Procedural Dungeon Generators

The procedural generation pattern surveyed in the Tiny Keep / Game Developer blog uses a three-phase pipeline:[^14]
1. Room placement with physics separation
2. Delaunay triangulation to connect room centers into a complete graph
3. Minimum spanning tree extraction, then ~10–15% of extra edges re-added for cycles

The data structures returned are: a list of rooms (id, position, size), an adjacency graph (node → room id, edges → distance in tiles), and a 2D tile grid for physical layout. The `graph-dungeon-generator` TypeScript project follows this pattern, using an `Node<Room>` tree to represent the topology and separately rendering a tilemap. For mutation, any of these edges can be surgically modified at runtime.[^15][^14]

***

## 3. Graph Mutation Patterns

### Edge Reroute: What Data Changes

When a door that used to connect Room A → Room B is rerouted to connect Room A → Room C, the following data must be updated atomically:

1. Remove the `Portal(A→B)` from `A.Portals`
2. Remove the twin `Portal(B→A)` from `B.Portals`
3. Remove both portals from `_portals` registry
4. Create new `Portal(A→C)` and twin `Portal(C→A)`, register both
5. Invalidate any active AI paths that referenced either dead portal

**Consistency invariants** you must maintain:
- **No dangling edges:** every portal's `.From` and `.To` must exist in `_nodes`
- **Twin symmetry:** `portal.Twin.Twin == portal` always
- **No orphan rooms:** every node must be reachable from a designated root (or at least have an "accessible" flag set false if intentionally isolated)
- **Observer gating:** the mutation must not fire while any observer has line-of-sight to either involved room (see §5)

```csharp
public void ReroutePortal(Portal portalAtoB, RoomNode newTarget) {
    // Validate observation gate
    if (portalAtoB.From.ObsState == ObservationState.Observed ||
        portalAtoB.To.ObsState == ObservationState.Observed ||
        newTarget.ObsState == ObservationState.Observed)
        throw new InvalidOperationException("Cannot mutate observed room.");

    var oldTarget = portalAtoB.To;
    var twin = portalAtoB.Twin;

    // 1. Remove old edges
    oldTarget.Portals.Remove(twin);
    _portals.Remove(twin.Id);

    // 2. Rewire forward portal
    portalAtoB.To = newTarget;
    portalAtoB.TransitionMatrix = ComputeTransitionMatrix(portalAtoB.From, newTarget);

    // 3. Create new reverse portal
    var newTwin = new Portal {
        From = newTarget,
        To   = portalAtoB.From,
        Twin = portalAtoB,
        TransitionMatrix = portalAtoB.TransitionMatrix.inverse
    };
    portalAtoB.Twin = newTwin;
    newTarget.Portals.Add(newTwin);
    _portals[newTwin.Id] = newTwin;

    // 4. Notify pathfinding system
    OnGraphEdgeChanged?.Invoke(portalAtoB);
}
```

### Orphan Detection

After any mutation, run a BFS/DFS from your designated entry room and verify all "accessible" rooms are reachable. An O(V+E) reachability check is cheap for a small house graph and should be asserted in debug builds:

```csharp
public bool IsConnected(RoomNode root) {
    var visited = new HashSet<RoomNode>();
    var queue = new Queue<RoomNode>();
    queue.Enqueue(root);
    while (queue.Count > 0) {
        var n = queue.Dequeue();
        if (!visited.Add(n)) continue;
        foreach (var p in n.Portals) queue.Enqueue(p.To);
    }
    return visited.Count == _nodes.Values.Count(r => r.IsAccessible);
}
```

***

## 4. Subgraph Insertion (The TARDIS Anomaly)

### The Graph Operation

"Bigger on the inside" is a **subgraph insertion with portal rewiring**. Before insertion, door D connects Room A → Room B. After insertion, door D connects A → Antechamber, Antechamber → ... → ExitRoom, ExitRoom → B. The exterior perceives A and B as adjacent; the interior is an arbitrarily large subgraph.

The atomic operation:
1. Detach the existing `Portal(A→B)` / `Portal(B→A)` pair
2. Insert subgraph: a set of new rooms connected by their own internal portals
3. Identify two **boundary portals** in the subgraph: `Entry` (faces A) and `Exit` (faces B)
4. Wire `A.Portals ← Portal(A→Entry.room)` and `B.Portals ← Portal(B→Exit.room)`
5. The transition matrices on entry/exit portals encode the spatial mismatch (small door → large interior)

```csharp
public void InsertSubgraph(Portal existingAtoB, LevelGraph subgraph,
                            RoomNode entryRoom, RoomNode exitRoom) {
    var a = existingAtoB.From;
    var b = existingAtoB.To;

    // Guard: no observers
    AssertUnobserved(a, b);

    // Detach old portal pair
    RemovePortalPair(existingAtoB);

    // Merge subgraph nodes/portals into main registry
    foreach (var (id, node) in subgraph._nodes) _nodes[id] = node;
    foreach (var (id, portal) in subgraph._portals) _portals[id] = portal;

    // Wire exterior boundary
    // A → entryRoom: the door looks small, but the transition matrix scales space
    var entryScale = Matrix4x4.Scale(new Vector3(10, 10, 10)); // TARDIS scale factor
    AddBidirectionalPortal(a, entryRoom, entryScale);
    AddBidirectionalPortal(exitRoom, b, Matrix4x4.identity);
}
```

**Entry/exit consistency invariants:**
- The subgraph must have exactly one entry and one exit node designated at insertion time; interior rooms may have any topology
- The `TransitionMatrix` on the entry portal must encode both the spatial scale jump and the orientation alignment so the renderer can seamlessly stitch the viewports
- If the subgraph is removed later, the `RemovePortalPair` logic must restore the original direct link A→B, or leave A and B disconnected if the design intent is "the door is now bricked up"

***

## 5. Observation-Gated Mutation

### The Core Problem

"A room can only change when unobserved" is architecturally the **Schrödinger's Room** pattern. You need to track, for every room, whether any observer (player camera, NPC with line-of-sight) is currently perceiving it. Mutation is gated behind that state.

### Three Valid Architectures

**Architecture A: Metadata on the Node (simple, tightly coupled)**
Each `RoomNode` carries an `ObservationState` enum and an observer reference count. The camera/AI query pipeline increments/decrements counts each frame.

```csharp
public enum ObservationState { Unobserved, Observed }

public class RoomNode {
    int _observerCount;
    public ObservationState ObsState =>
        _observerCount > 0 ? ObservationState.Observed : ObservationState.Unobserved;

    public void AddObserver()    => _observerCount++;
    public void RemoveObserver() => _observerCount = Mathf.Max(0, _observerCount - 1);
}
```

**Architecture B: Separate Observation System (decoupled, recommended)**
A dedicated `ObservationSystem` queries all active cameras/observers each frame, computes visible rooms via the portal graph (a BFS through portals that intersect the camera frustum), and stamps `ObservationState` onto nodes. The mutation system only reads this stamp — it never queries cameras directly.[^16][^17]

```csharp
public class ObservationSystem : MonoBehaviour {
    [SerializeField] Camera[] _observers;
    LevelGraph _graph;

    void LateUpdate() {
        // Reset all
        foreach (var node in _graph.AllNodes) node.AddObserver(); // reset trick: use version stamp

        // BFS from each observer's current room through visible portals
        foreach (var cam in _observers) {
            var currentRoom = _graph.GetRoomContaining(cam.transform.position);
            if (currentRoom == null) continue;
            MarkVisible(currentRoom, cam);
        }
    }

    void MarkVisible(RoomNode startRoom, Camera cam) {
        var visited = new HashSet<RoomNode>();
        var queue = new Queue<RoomNode>();
        queue.Enqueue(startRoom);
        while (queue.Count > 0) {
            var room = queue.Dequeue();
            if (!visited.Add(room)) continue;
            room.AddObserver(); // mark visible
            foreach (var portal in room.Portals) {
                if (IsPortalVisibleFrom(portal, cam))
                    queue.Enqueue(portal.To);
            }
        }
    }
}
```

**Architecture C: Temporal Locks (deferred mutation)**
Rather than blocking mutations outright, you enqueue them and apply each mutation the first frame the involved rooms become unobserved. This prevents mutation starvation and gives the horror game an opportunity to time mutations for maximum unease — the change happens *right after* the player looks away.

```csharp
public class DeferredMutation {
    public Action MutationAction;
    public HashSet<RoomNode> RequiredUnobservedRooms;
}

public class MutationQueue : MonoBehaviour {
    readonly Queue<DeferredMutation> _pending = new();

    void Update() {
        if (_pending.Count == 0) return;
        var m = _pending.Peek();
        if (m.RequiredUnobservedRooms.All(r => r.ObsState == ObservationState.Unobserved)) {
            _pending.Dequeue();
            m.MutationAction();
        }
    }

    public void Enqueue(DeferredMutation mutation) => _pending.Enqueue(mutation);
}
```

Architecture C is ideal for your horror use case: the game can schedule mutations during loading/transitions and they will naturally fire the moment the player stops watching, creating an eerie sense that the house is always changing *just out of sight*.[^16]

***

## 6. Pathfinding on Mutable Graphs

### The Problem

An AI entity mid-path holds a reference to a sequence of rooms and portals. If the graph mutates (a portal is removed, rerouted, or a new room inserted), that cached path may reference dead portals or stale costs.

### D* Lite — Incremental Repair

D* Lite (Dynamic A*) is the recommended algorithm for mutable graphs. It runs A* in reverse from goal → start during initialization, then maintains two cost values per node: `G` (best known cost from start) and `RHS` (one-step lookahead best cost). When an edge cost changes (portal removed = edge cost → ∞, portal added = new finite cost), D* Lite re-evaluates only the affected nodes instead of replanning from scratch. This makes it O(k log k) per update where k is the number of affected nodes, vs O(V log V) for a full replan.[^18][^19]

A Unity C# implementation exists in production:[^20]

```csharp
// When a portal is destroyed (edge removed):
void OnPortalDestroyed(Portal deadPortal) {
    // Increase the cost of traversal through this edge to infinity
    deadPortal.TraversalCost = float.PositiveInfinity;
    // D* Lite only re-evaluates nodes locally around the changed edge
    _dStarLite.RecalculateNode(deadPortal.From);
}

// When a portal is rerouted:
void OnPortalRerouted(Portal portal, RoomNode oldTarget, RoomNode newTarget) {
    // Remove old cost
    portal.TraversalCost = float.PositiveInfinity;
    _dStarLite.RecalculateNode((Node<RoomNode>)portal.From);
    // Add new edge at correct cost
    portal.To = newTarget;
    portal.TraversalCost = Vector3.Distance(portal.From.WorldBounds.center,
                                             newTarget.WorldBounds.center);
    _dStarLite.RecalculateNode((Node<RoomNode>)portal.From);
}
```

D* Lite handles rapid changes more reliably than LPA* because its backward search from goal adapts more quickly, though both algorithms struggle when the entire graph topology changes simultaneously.[^21]

### HPA* — Hierarchical Abstraction for Large Graphs

If your house grows large (100+ rooms), use **Hierarchical Pathfinding A* (HPA*)** which abstracts the graph into clusters. Local cluster-to-cluster transition costs are pre-computed; global pathfinding searches the abstract cluster graph. When a mutation occurs, only the affected cluster's internal graph and its inter-cluster edges need to be rebuilt — not the entire hierarchy.[^22][^23][^24]

The `ExtendedGraph` pattern (Davide Aversa) cleanly handles temporary node insertion (start and goal) by wrapping the underlying graph with just two extra nodes, avoiding any modification of the base structure:[^24]

```csharp
// HPA* abstract graph cluster structure (abbreviated)
public class HPAGraph {
    public List<List<Cluster>> LevelClusters = new(); // [level][cluster_index]
    public List<HierNode> StaticNodes = new();
    public List<Entrance> Entrances = new();

    public HierNode InsertTemporaryNode(Vector2Int pos, int level, out bool shouldDelete) {
        // Tries to find existing node; creates temporary one if not found
        shouldDelete = false;
        var node = NodeExists(pos);
        if (node != null) return node;
        shouldDelete = true;
        var newNode = new HierNode(pos, level, isTemporary: true);
        ConnectNodeToBorderCluster(newNode, DetermineCluster(pos, level), level);
        return newNode;
    }
}
```

### Comparison for Your Use Case

| Approach | Best for | Graph mutation cost | Memory |
|---|---|---|---|
| Full A* replan | Tiny graphs (<20 rooms) | O(V log V) per change | Low |
| D* Lite | Frequent small mutations | O(k log k) local repair | Medium |
| HPA* | Large graphs (100+ rooms) | O(cluster) per mutation | Higher (precomputed hierarchy) |
| HPA* + D* Lite hybrid | Large + frequent mutations | Best of both | Highest |

For a horror house with ~20–80 rooms, **D* Lite alone** is the right choice: lightweight, handles mid-path changes gracefully, and has a working Unity C# reference implementation.[^20]

***

## 7. Graph Serialization

### Requirements

Save/load and network sync both need to serialize the graph faithfully without losing:
- Node identities (Guid)
- Edge topology (twin relationships)
- Portal metadata (transition matrices, locks, observation state)
- Mutation queue state

### Format Options

| Format | Size | Speed | Schema-evolution | Best for |
|---|---|---|---|---|
| JSON (System.Text.Json) | Large | Slow | Easy (nullable) | Debug, level editor |
| MessagePack | ~50% of JSON | 2–3× faster serialize | Attribute-tagged | Save files |
| Protobuf-net | ~40–60% of JSON | 1.5–2× faster | Strong (field numbers) | Network sync |
| Custom binary | Smallest | Fastest | Hard (manual versioning) | Streaming large worlds |

For save files, MessagePack is ideal: it is language-agnostic, binary, significantly smaller and faster than JSON, and performs best when you define public fields rather than properties.[^25][^26][^27]

### Serialization Schema

The key serialization challenge is that `Portal.Twin` creates a reference cycle. Solve this by serializing by ID and resolving references on load:

```csharp
[MessagePackObject]
public class SerializedGraph {
    [Key(0)] public List<SerializedRoom>   Rooms   = new();
    [Key(1)] public List<SerializedPortal> Portals = new();
    [Key(2)] public List<SerializedDeferredMutation> PendingMutations = new();
    [Key(3)] public string RootRoomId; // entry point for connectivity validation
}

[MessagePackObject]
public class SerializedRoom {
    [Key(0)] public string Id;
    [Key(1)] public string Label;
    [Key(2)] public string ObsState;   // "Observed" | "Unobserved"
    [Key(3)] public float[] WorldBounds; // center + extents, 6 floats
}

[MessagePackObject]
public class SerializedPortal {
    [Key(0)] public string Id;
    [Key(1)] public string FromRoomId;
    [Key(2)] public string ToRoomId;
    [Key(3)] public string TwinId;       // resolve after loading all portals
    [Key(4)] public float[] Matrix;      // 16 floats, row-major
    [Key(5)] public bool IsLocked;
}

// Serializer
public static class GraphSerializer {
    public static byte[] Serialize(LevelGraph graph) {
        var data = new SerializedGraph {
            RootRoomId = graph.RootRoom.Id.ToString()
        };
        foreach (var node in graph.AllNodes)
            data.Rooms.Add(new SerializedRoom {
                Id     = node.Id.ToString(),
                Label  = node.Label,
                ObsState = node.ObsState.ToString(),
                WorldBounds = new[] {
                    node.WorldBounds.center.x, node.WorldBounds.center.y, node.WorldBounds.center.z,
                    node.WorldBounds.size.x,   node.WorldBounds.size.y,   node.WorldBounds.size.z
                }
            });
        foreach (var portal in graph.AllPortals)
            data.Portals.Add(new SerializedPortal {
                Id         = portal.Id.ToString(),
                FromRoomId = portal.From.Id.ToString(),
                ToRoomId   = portal.To.Id.ToString(),
                TwinId     = portal.Twin?.Id.ToString(),
                Matrix     = MatrixToFloats(portal.TransitionMatrix),
                IsLocked   = portal.IsLocked
            });
        return MessagePackSerializer.Serialize(data);
    }

    public static LevelGraph Deserialize(byte[] bytes) {
        var data = MessagePackSerializer.Deserialize<SerializedGraph>(bytes);
        var graph = new LevelGraph();

        // Pass 1: create all nodes
        var nodeMap = new Dictionary<string, RoomNode>();
        foreach (var r in data.Rooms) {
            var node = graph.AddRoomWithId(Guid.Parse(r.Id), r.Label);
            node.ObsState = Enum.Parse<ObservationState>(r.ObsState);
            nodeMap[r.Id] = node;
        }

        // Pass 2: create portals (no twins yet)
        var portalMap = new Dictionary<string, Portal>();
        foreach (var p in data.Portals) {
            var portal = new Portal {
                From              = nodeMap[p.FromRoomId],
                To                = nodeMap[p.ToRoomId],
                TransitionMatrix  = FloatsToMatrix(p.Matrix),
                IsLocked          = p.IsLocked
            };
            nodeMap[p.FromRoomId].Portals.Add(portal);
            portalMap[p.Id] = portal;
        }

        // Pass 3: resolve twin references
        foreach (var p in data.Portals)
            if (p.TwinId != null && portalMap.TryGetValue(p.TwinId, out var twin))
                portalMap[p.Id].Twin = twin;

        return graph;
    }

    static float[] MatrixToFloats(Matrix4x4 m) {
        var f = new float[^16];
        for (int i = 0; i < 16; i++) f[i] = m[i];
        return f;
    }
    static Matrix4x4 FloatsToMatrix(float[] f) {
        var m = new Matrix4x4();
        for (int i = 0; i < 16; i++) m[i] = f[i];
        return m;
    }
}
```

### Network Sync Delta Compression

For multiplayer, avoid sending the full graph every tick. Instead, emit **mutation events** as small delta messages:

```csharp
[MessagePackObject]
public abstract class GraphDelta { [Key(0)] public long Timestamp; }

[MessagePackObject]
public class PortalRerouteDelta : GraphDelta {
    [Key(1)] public string PortalId;
    [Key(2)] public string NewTargetRoomId;
}

[MessagePackObject]
public class SubgraphInsertDelta : GraphDelta {
    [Key(1)] public string AnchorPortalId;
    [Key(2)] public SerializedGraph NewSubgraph;
}
```

Clients apply deltas in timestamp order, validating invariants (twin symmetry, no dangling edges) before committing. This is the same two-pass deserialization pattern above, applied to incremental patches.[^28]

***

## Integration Notes

### Published GDC Talks and Postmortems

- **Antichamber: Three Years of Hardcore Iteration** (GDC 2013, Alexander Bruce) — covers the warp-zone/trigger approach and iteration philosophy; non-public vault video but the title reveals the room-swap mechanism[^29][^30]
- **Manifold Garden: Level Design in Impossible Geometry** (GDC 2016, William Chyr) — covers scene-splitting, hallway graphs, and the challenge of designing in a wrapped topology[^31][^12]
- **Hierarchical Dynamic Pathfinding for Large Voxel Worlds** (GDC 2017, Castle Story) — hierarchical pathfinding in a dynamically deformable world; directly applicable to HPA* on a mutable room graph[^32]
- **Portals and Quake PVS** (30fps.net) — the most complete public explanation of how a cell-and-portal graph is built and queried, with reference Python code[^6]

### Portal Rendering Consistency

Graph topology and render are two separate concerns but must stay in sync. When you reroute a portal in the graph, the stencil-buffer portal renderer must also update which camera it uses to render the view through that door. The stencil depth method (increment on recurse, decrement on backtrack) supports up to 255 recursive portal depths with an 8-bit buffer, making it robust even if a mutation creates a loop.[^33][^34]

### Impossible-Space Rendering (Antichamber Style)

The practical pattern in Unity for your horror house is: each room is a self-contained sub-scene loaded additively. Doorway trigger boxes fire the teleport. Rooms that overlap spatially are differentiated by stencil value. When the graph mutates, you swap which trigger-destination the doorway trigger references — a single field assignment that is always safe to do between fixed-update ticks. This maps one-to-one to the `ReroutePortal()` method above, where the `TransitionMatrix` encodes the offset needed to align the two sub-scenes.[^35][^36][^37][^9]

---

## References

1. [What is better, adjacency lists or adjacency matrices for graph ...](https://stackoverflow.com/questions/2218322/what-is-better-adjacency-lists-or-adjacency-matrices-for-graph-problems-in-c) - The adjacency list can have the same upsides (and none of the downsides) of the adjacency matrix by ...

2. [What are the differences between graph adjacency list and matrix ...](https://math.answers.com/computer-science/What-are-the-differences-between-graph-adjacency-list-and-matrix-and-how-do-they-impact-the-efficiency-of-graph-operations) - The adjacency list is more memory-efficient for sparse graphs with fewer connections, as it only sto...

3. [Half-edge data structure - CS 418](https://cs418.cs.illinois.edu/website/text/halfedge.html) - A half-edge's next is never null, and walking next s always results in traveling around a loop consi...

4. [Halfedge mesh internals - Geometry Central](https://geometry-central.net/surface/surface_mesh/internals/) - The halfedge mesh structure is designed to simultaneously satisfy two core principles: contiguous st...

5. [Procedural Generation For Dummies: Half Edge Geometry](https://martindevans.me/game-development/2016/03/30/Procedural-Generation-For-Dummies-Half-Edge-Geometry/) - Half Edge meshes are a more powerful way to represent meshes than a traditional indexed mesh. I use ...

6. [Portals and Quake - 30fps.net](https://30fps.net/pages/pvs-portals-and-quake/) - Leaves (nodes) connected by portals (edges) in a cell-and-portal graph. Each portal is a 3D polygon....

7. [Binary space partitioning - Valve Developer Community](https://developer.valvesoftware.com/wiki/Binary_space_partitioning) - BSP stands for Binary Space Partitioning. .bsp is the file extension for maps/levels used by many BS...

8. [Guide :: How to truly max out Portal 2's Graphics - Steam Community](https://steamcommunity.com/sharedfiles/filedetails/?id=2294048092) - In this guide, you will learn how to heighten the graphical quality to the maximum without mods or R...

9. [Is it possible to create Non-Euclidean spaces in Unreal Engine?](https://www.reddit.com/r/unrealengine/comments/1g6qwlm/is_it_possible_to_create_noneuclidean_spaces_in/) - The creator of Antichamber made it in UE3 using unreal script. It is certainly possible. I made a no...

10. [How does the game actually work? : r/antichamber - Reddit](https://www.reddit.com/r/antichamber/comments/2wnvyc/how_does_the_game_actually_work/) - It's a mixture of seamlessly teleporting you around, modifying the world (mostly to add/remove certa...

11. ["Teleporting" the player character to different locations around one ...](https://www.reddit.com/r/Unity3D/comments/23s0lt/teleporting_the_player_character_to_different/) - My question is, how would I go about making the doors "teleport" the player to a different door in t...

12. [Manifold Garden: Level Design in Impossible Geometry - YouTube](https://www.youtube.com/watch?v=ed2zmmcEryw) - This 2016 GDC session from independent developer William Chyr focuses on the challenge of designing ...

13. [Manifold Garden - Development Update 12 - YouTube](https://www.youtube.com/watch?v=PQyc7rDkGvM) - Manifold Garden is now playable from start to finish. In ... Manifold Garden: Level Design in Imposs...

14. [Procedural Dungeon Generation Algorithm - Game Developer](https://www.gamedeveloper.com/programming/procedural-dungeon-generation-algorithm) - This post explains a technique for generating randomized dungeons that was first described by the de...

15. [halftheopposite/graph-dungeon-generator - GitHub](https://github.com/halftheopposite/graph-dungeon-generator) - A simple graph-based procedural dungeon generator. Want to play with the generator? Try the demo her...

16. [Game Design with Observer Pattern | by Jason Li | Dev Genius](https://blog.devgenius.io/game-design-with-observer-pattern-3368561f40f5) - What Is The Pattern? The Observer pattern is the relationship where one subject can signal its many ...

17. [Create modular and maintainable code with the observer pattern](https://learn.unity.com/tutorial/create-modular-and-maintainable-code-with-the-observer-pattern) - By implementing common game programming design patterns in your Unity project, you can efficiently b...

18. [Where can I find information on the D* or D* Lite pathfinding ...](https://stackoverflow.com/questions/2900718/where-can-i-find-information-on-the-d-or-d-lite-pathfinding-algorithm) - As opposed to repeated A* search, the D* Lite algorithm avoids replanning from scratch and increment...

19. [Path-Planning Algorithms: A Comparative Study between A* and D ...](https://engineering.miko.ai/path-planning-algorithms-a-comparative-study-between-a-and-d-lite-01133b28b8b4) - The D* Lite algorithm stands for Dynamic A* and is thus an incremental path planning algorithm speci...

20. [Implementation of D* Lite Algorithm for Unity - GitHub Gist](https://gist.github.com/adammyhre/b8a2428c34e06ba9acbd70dceeadbcb6) - D* Lite is an incremental heuristic search algorithm that finds the shortest path in a graph. // fro...

21. [[PDF] Comparison of Pathfinding Algorithms in Dynamic and Congested ...](https://kth.diva-portal.org/smash/get/diva2:1985736/FULLTEXT01.pdf) - How do D* Lite and a custom-implemented LPA* compare in terms of replanning efficiency, path length,...

22. [Hierarchical Path-Finding A* Method | PDF - Scribd](https://www.scribd.com/doc/82055418/hpastar) - HPA* abstracts a map into linked local clusters. At the local level ... The hierarchical levels of t...

23. [Hierarchial-Pathfinding-Research](https://alexmelenchon.github.io/Hierarchial-Pathfinding-Research/) - It's a “technique” to divide the map so the desired Pathfinding Algorithm has an easier time constru...

24. [Boost Hierarchical Pathfinding with Extended Graphs - Davide Aversa](https://www.davideaversa.it/blog/boost-hierarchical-pathfinding-with-extended-graphs/) - The solution on the paper is simple: add start and target to the vertices in the abstract high-level...

25. [Performance Test – Binary serializers Part II - theburningmonk.com](https://theburningmonk.com/2011/12/performance-test-binary-serializers-part-ii/) - BinaryFormatter performs better with fields – faster serialization and smaller payload! 2. Protobuf-...

26. [MessagePack vs Protobuf – Which Binary Data Framework Should ...](https://cloudflare.domartisan.com/blog/messagepack-vs-protobuf-which-binary-data-framework-should-you-use-2) - MessagePack. Overview and Features. MessagePack is a binary serialization format designed for effici...

27. [Performant Entity Serialization: BSON vs MessagePack (vs JSON)](https://stackoverflow.com/questions/6355497/performant-entity-serialization-bson-vs-messagepack-vs-json) - Recently I've found MessagePack, an alternative binary serialization format to Google's Protocol Buf...

28. [Save and load running graph state | Behavior | 1.0.15 - Unity - Manual](https://docs.unity3d.com/Packages/com.unity.behavior@1.0/manual/serialization.html) - Save and load running graph state. Serialization saves the state of a behavior graph and loads it ba...

29. [Antichamber: Three Years of Hardcore Iteration - GDC Vault](https://www.gdcvault.com/play/1020586/Antichamber-Three-Years-of-Hardcore) - This talk will provide an in-depth analysis of the several years of refinement that went into turnin...

30. [Antichamber: An Overnight Success, Seven Years in the Making](https://www.gdcvault.com/play/1020776/Antichamber-An-Overnight-Success-Seven) - GDC Vault Logo. The Number One Educational Resource for the Game Industry. To view this video please...

31. [Level Design Workshop: Level Design in Impossible Geometry](https://gdcvault.com/play/1023553/Level-Design-Workshop-Level-Design) - This talk will focus on the challenges and lessons learned in designing levels for Manifold Garden, ...

32. [Hierarchical Dynamic Pathfinding for Large Voxel Worlds - GDC Vault](https://gdcvault.com/play/1025320/Hierarchical-Dynamic-Pathfinding-for-Large) - Overview: This presentation explores the hierarchical pathfinding system developed for 'Castle Story...

33. [Rendering "Portal" by torinmr - GitHub Pages](https://torinmr.github.io/cs148/) - The stencil buffer is similar to the depth buffer, in that the buffer does not appear on the screen,...

34. [Rendering recursive portals with OpenGL - th0mas.nl](https://th0mas.nl/2013/05/19/rendering-recursive-portals-with-opengl/) - The general trick to doing the stencil buffer method recursively is to increase the stencil value fo...

35. [Stencil buffer and non euclidean geometry - Rendering](https://forums.unrealengine.com/t/stencil-buffer-and-non-euclidean-geometry/481564) - I have this project I made in another game engine using the stencil buffer to do non euclidean space...

36. [How to dynamically change whole rooms in a game - Blueprint](https://forums.unrealengine.com/t/how-to-dynamically-change-whole-rooms-in-a-game/151320) - There's a variety of methods. You can have the rooms as blueprints, it really depends how complex th...

37. [How to Create Simple Impossible Spaces in Unity - Adam Kehl](https://adamkehl.com/create-impossible-spaces-unity/) - Super simple way to create impossible spaces/levels in Unity using only colliders. No fancy masking ...

