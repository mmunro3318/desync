# Multiplayer Room Loading with Netcode for GameObjects 2.11.x
### Server-Authoritative Graph Streaming for a First-Person Horror Game

***

## Executive Summary

The correct architectural principle for your graph-mutating horror house is: **the server owns graph truth, clients own geometry rendering**. Room geometry should never be `NetworkObject`s — only the graph state and player positions are networked. Each client independently loads/unloads room scenes based on the replicated graph, with `NetworkSceneManager`'s `SceneEventType.LoadEventCompleted` acting as a hard gate before any gameplay in the new topology is unlocked. This keeps bandwidth low, prevents the `NetworkObject` spawn storm problem, and makes late-joining clients trivially synchronizable.

***

## 1. NGO Runtime Spawning: Limits and Sync Cost

### Soft Spawn Limits

NGO itself has no hard-coded upper limit on `NetworkObject` count, but the underlying `Unity Transport` layer has a default send/receive buffer size that causes silent drop failures when spawning hundreds of objects simultaneously. An early NGO 1.x issue report found that spawning 700+ objects at once (each a minimal prefab with just a `NetworkObject`) caused clients to simply not receive them. The workaround — and the current best practice in NGO 2.x — is to:[^1]

1. Stagger spawns (no more than 100–200 per frame burst).
2. Increase the `Unity Transport` max payload size in `NetworkManager → Transport Settings`.
3. Use in-scene placed `NetworkObject`s for any object that must exist on connection (they are synchronized via scene loading, not individual spawn messages).[^2]

For a horror game, players, interactables (doors, props), and trap triggers should be `NetworkObject`s. Room *geometry* (walls, floors, ceilings, lights) absolutely should not be.

### Sync Cost Per NetworkObject

Every active `NetworkObject` with dirty `NetworkVariable`s generates an `UpdateVars` packet each tick it changes. Objects with no dirty state generate no traffic — but they still occupy a slot in NGO's internal object map and are included in the synchronization message for late-joining clients. Keeping room geometry out of the `NetworkObject` system means late joiners don't pay a synchronization cost proportional to world size.[^3]

| Object Type | Should Be NetworkObject? | Reason |
|---|---|---|
| Room geometry (walls, mesh) | ❌ No | Loaded per scene; no per-frame state to sync |
| Portal anchor/trigger volume | ✅ Yes (lightweight) | Must sync "which room ID connects here" |
| Door (open/closed state) | ✅ Yes | Per-frame interaction state |
| Player character | ✅ Yes | Position, animation, inventory |
| Room graph controller | ✅ Yes (singleton) | Owns the adjacency list NetworkVariable |
| Props / interactables | ✅ Yes | Pickup state, interaction events |

***

## 2. Room Geometry vs. Graph State: What Gets Networked

### Core Principle: Geometry is a Side Effect of Graph State

Room geometry is not networked. Only the **graph state** (which rooms exist, which rooms are connected) is networked. Clients use the replicated graph to determine which scenes to load locally. This is the same pattern used in large-scale multiplayer games — the server tells clients "you should see rooms [A, B, C]" and each client independently handles the loading.

```
Server                          Client 1                    Client 2
─────                           ────────                    ────────
GraphState NetworkVariable      Reads graph state           Reads graph state
  Rooms: [A, B, C]              Loads Scene_RoomA           Loads Scene_RoomA
  Edges: A↔B, A↔C              Loads Scene_RoomB           Loads Scene_RoomB
                                Loads Scene_RoomC           Loads Scene_RoomC
  [mutation: A no longer        Receives OnValueChanged     Receives OnValueChanged
   connects to B, now to D]     → Unload B, Load D          → Unload B, Load D
```

This architecture means the server never spawns room geometry `NetworkObject`s, never sends mesh data over the wire, and never runs out of spawn slots because of level geometry.

***

## 3. Server-Owned Graph Truth + Client-Rendered View

### The Pattern

Place a single `RoomGraphAuthority` `NetworkBehaviour` in your persistent bootstrap scene. It holds the canonical adjacency list as a `NetworkList<RoomEdge>` (see Section 4). Clients subscribe to `OnListChanged` and reconcile their loaded scenes accordingly.

```csharp
// Server-side: authoritative graph controller
public class RoomGraphAuthority : NetworkBehaviour
{
    // Replicated edge list — clients receive full state on connection
    public NetworkList<RoomEdge> Edges;

    // Replicated "occupied rooms" — drives which scenes clients must have loaded
    public NetworkList<int> ActiveRoomIds;

    private void Awake()
    {
        Edges = new NetworkList<RoomEdge>(
            new List<RoomEdge>(),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        ActiveRoomIds = new NetworkList<int>(
            new List<int>(),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
    }
}
```

Clients subscribe in `OnNetworkSpawn`:

```csharp
public override void OnNetworkSpawn()
{
    if (IsClient)
    {
        Edges.OnListChanged += OnEdgesChanged;
        ActiveRoomIds.OnListChanged += OnActiveRoomsChanged;
        // On first connection, full state is delivered — trigger initial load
        RoomStreamingClient.Instance.ReconcileWithGraph(Edges, ActiveRoomIds);
    }
}
```

### Preventing Client Drift

Client drift — where a client's loaded-scene state diverges from the server's authoritative graph — is the core reliability problem. Prevent it with three mechanisms:

**1. Server-Only Mutations:** All graph mutations run exclusively on the server. Any client attempting to call a mutation method should be rejected:

```csharp
[ServerRpc(RequireOwnership = false)]
public void RequestMutationServerRpc(int fromRoomId, int toRoomId, ServerRpcParams rpcParams = default)
{
    // Only server processes — client just requested, never applies locally
    ApplyMutation(fromRoomId, toRoomId);
}

private void ApplyMutation(int fromRoomId, int toRoomId)
{
    if (!IsServer) return;
    // Modifying NetworkList automatically replicates to all clients
    var edgeToRemove = FindEdge(fromRoomId);
    Edges.Remove(edgeToRemove);
    Edges.Add(new RoomEdge { RoomA = fromRoomId, RoomB = toRoomId });
    // ActiveRoomIds update happens here too
}
```

**2. Full State Reconciliation on Connect:** When a late-joining client connects, `NetworkList` delivers the current full snapshot before any `OnListChanged` callbacks fire. This means new clients always start in a consistent state, not a partially-replayed state.[^4]

**3. Version Counter:** Add a `NetworkVariable<uint> GraphVersion` that increments on every mutation. Clients assert their local loaded set matches the current version. If a client's local load/unload coroutine is still running when a second mutation arrives (version N+1 before N's load completes), it should queue mutations rather than dropping them:

```csharp
private Queue<GraphMutation> _pendingMutations = new();
private bool _isProcessingMutation = false;

private void OnEdgesChanged(NetworkListEvent<RoomEdge> changeEvent)
{
    _pendingMutations.Enqueue(BuildMutation(changeEvent));
    if (!_isProcessingMutation)
        StartCoroutine(ProcessMutationQueue());
}

private IEnumerator ProcessMutationQueue()
{
    _isProcessingMutation = true;
    while (_pendingMutations.Count > 0)
    {
        var mutation = _pendingMutations.Dequeue();
        yield return StartCoroutine(ApplyMutationLocally(mutation));
    }
    _isProcessingMutation = false;
}
```

***

## 4. NetworkVariable vs. ClientRpc for Graph State

### Use NetworkList<RoomEdge> for Edges (Not NetworkVariable)

`NetworkVariable<T>` syncs a **single value**; replacing the whole graph on each mutation means serializing the entire edge list every time. `NetworkList<T>` is designed for append/remove delta sync — it sends only the changed item, not the full list.[^5]

For `NetworkList` to work with a custom struct, the struct must implement `INetworkSerializable` and `IEquatable<T>`:[^6][^5]

```csharp
public struct RoomEdge : INetworkSerializable, IEquatable<RoomEdge>
{
    public int RoomA;          // Room graph node ID
    public int RoomB;          // Room graph node ID
    public int PortalId;       // Which portal connects them
    public bool IsActive;      // Edge currently traversable?

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref RoomA);
        serializer.SerializeValue(ref RoomB);
        serializer.SerializeValue(ref PortalId);
        serializer.SerializeValue(ref IsActive);
    }

    // IEquatable is required by NetworkList for dirty-checking
    public bool Equals(RoomEdge other) =>
        RoomA == other.RoomA && RoomB == other.RoomB &&
        PortalId == other.PortalId && IsActive == other.IsActive;
}
```

**Known NGO 2.x gotcha:** NGO 2.0 introduced `NetworkList<T>` with IEquatable requirements but a codegen bug meant custom `NetworkVariable` subclasses didn't always trigger serialization generation. As of NGO 2.11.x this is resolved, but always verify: add a plain `NetworkVariable<int>` alongside any custom-serialized type to force the codegen pass.[^7]

### When to Use ClientRpc Instead

Use `ClientRpc` for **one-shot events** that are not state, specifically:

- `[ClientRpc] PlayMutationAnimationClientRpc(int portalId)` — plays the "doorway warps" VFX; no state to preserve if a client reconnects mid-animation.
- `[ClientRpc] TeleportPlayerClientRpc(Vector3 safePosition)` — the doorway-contested position correction (see Section 5).

Do NOT use `ClientRpc` to communicate graph state. If a client disconnects and reconnects, it will miss any RPC sent during the disconnect window. `NetworkList` survives reconnection because late joiners receive the full current snapshot.

### Dictionary / HashMap Approach (Avoid for Now)

NGO 2.x added `NativeHashMap` support to `NetworkVariable`, but the API is unstable and community reports show it frequently fails to sync or fire `OnValueChanged`. An adjacency list encoded as `NetworkList<RoomEdge>` is safer, more debuggable, and trivially serializable.[^8][^5]

***

## 5. The Portal Problem: Contested Player Position During Mutation

### The Scenario

Player stands in the doorway between Room A and Room B. Server decides Room A's east portal now connects to Room C instead of Room B. The player is physically in the B-side of the transition zone.

### Server-Side Resolution Strategy

The server must resolve this **before** committing the mutation to the `NetworkList`. The resolution sequence:

```csharp
private IEnumerator MutateEdgeWithSafetyCheck(
    int fromRoomId, int oldRoomId, int newRoomId, int portalId)
{
    // 1. Find all players in or near the affected portal zone
    var contestedPlayers = FindPlayersInPortalZone(portalId);

    if (contestedPlayers.Count > 0)
    {
        // 2a. Option A: Defer mutation until portal zone is clear
        yield return new WaitUntil(() =>
            FindPlayersInPortalZone(portalId).Count == 0);

        // 2b. Option B (horror design): Force-teleport players to safe zone
        // in fromRoomId, then mutate immediately
        foreach (var player in contestedPlayers)
        {
            var safePos = GetSafePositionInRoom(fromRoomId);
            player.GetComponent<PlayerNetworkController>()
                  .TeleportPlayerClientRpc(safePos);
        }

        // Short delay to let teleport settle before mutation
        yield return new WaitForSeconds(0.1f);
    }

    // 3. Lock the portal (disable traversal) server-side
    SetPortalTraversable(portalId, false);

    // 4. Commit graph mutation — replicates to all clients via NetworkList
    var oldEdge = FindEdge(fromRoomId, oldRoomId, portalId);
    Edges.Remove(oldEdge);
    Edges.Add(new RoomEdge
    {
        RoomA = fromRoomId,
        RoomB = newRoomId,
        PortalId = portalId,
        IsActive = false   // remains locked until load completes
    });

    // 5. Wait for all clients to finish loading Room C
    // (see Section 6 — LoadEventCompleted gate)
    yield return StartCoroutine(WaitForAllClientsLoaded(newRoomId));

    // 6. Re-enable portal traversal
    SetPortalTraversable(portalId, true);
    UpdateEdgeActive(fromRoomId, newRoomId, portalId, true);
}
```

### Why Force-Teleport is the Correct Horror Design

From a game-feel standpoint, being teleported out of a doorway *is* the horror mechanic. NGO's own latency documentation recommends using "controlled desyncs" — letting clients re-interpret how they reach the authoritative state, so long as they converge. The player is teleported to safety; each client sees the VFX of their choice; all clients converge on the player being in Room A. This is mechanically sound and thematically coherent with a house that mutates.[^9]

***

## 6. Loading Latency Desync: Preventing One Player Seeing Void

This is the most important reliability concern. If Player 1 loads Room C in 0.8s and Player 2 takes 3s, Player 1 must not see Player 2 standing in a void for 2.2 seconds.

### The Gate Pattern: LoadEventCompleted

NGO's `SceneEventType.LoadEventCompleted` fires on the server (and all clients) **only after every connected client has finished loading the scene**. This is your primary synchronization primitive:[^10][^2]

```csharp
// Server-side: subscribe to scene events after loading Room C
private void StartRoomCLoad(string roomCSceneName)
{
    NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneEvent;
    NetworkManager.Singleton.SceneManager.LoadScene(
        roomCSceneName, LoadSceneMode.Additive);
}

private void OnSceneEvent(SceneEvent sceneEvent)
{
    switch (sceneEvent.SceneEventType)
    {
        case SceneEventType.Load:
            // Individual client started loading — track progress
            _loadingClients.Add(sceneEvent.ClientId);
            break;

        case SceneEventType.LoadComplete:
            // Individual client finished — could update a loading UI
            _loadingClients.Remove(sceneEvent.ClientId);
            Debug.Log($"Client {sceneEvent.ClientId} loaded {sceneEvent.SceneName}");
            break;

        case SceneEventType.LoadEventCompleted:
            // ALL clients finished — safe to unlock portal
            NetworkManager.Singleton.SceneManager.OnSceneEvent -= OnSceneEvent;
            OnAllClientsLoadedRoom(sceneEvent.SceneName);
            break;
    }
}

private void OnAllClientsLoadedRoom(string roomSceneName)
{
    // Now safe to: spawn room NetworkObjects, enable portals, unlock movement
    SetPortalTraversable(GetPortalForScene(roomSceneName), true);
    SpawnRoomInteractables(roomSceneName);
}
```

**Critical edge case:** A known NGO bug (fixed in 2.x but worth guarding against) caused `LoadEventCompleted` to fire early when a 4th client connected during an existing load, because the "all clients" count was calculated before the new client's confirmation arrived. Guard against this with a manual count check:[^11]

```csharp
case SceneEventType.LoadComplete:
    _completedClients.Add(sceneEvent.ClientId);
    // Manual guard: only proceed if all *currently connected* clients confirmed
    int expectedCount = NetworkManager.Singleton.ConnectedClientsList.Count;
    if (_completedClients.Count >= expectedCount)
        OnAllClientsLoadedRoom(sceneEvent.SceneName);
    break;
```

### What to Show During the Load Gap

Never show void. Use a layered approach:

| State | What Players See |
|---|---|
| Portal mutation triggered | Portal visual locks/seals (door slams shut, wall appears) |
| Loading in progress | Portal remains sealed; player in Room A continues normally |
| All clients loaded | Portal seal dissolves, Room C visible through doorway |
| Players may traverse | Portal traversal re-enabled via server |

This means the player in Room A is never waiting staring at a loading screen — they simply can't go through the door yet. The door-slam is part of the horror atmosphere.

### Per-Client Scene Filtering with VerifySceneBeforeLoading

Not every client needs every room loaded — a client in Room A adjacent to B and C should never need to load Room F (on the far side of the house). Use `VerifySceneBeforeLoading` to filter which NGO-ordered scene loads a client should honor:[^12][^13]

```csharp
// Client-side: filter which server-ordered scene loads to accept
private void RegisterSceneVerification()
{
    NetworkManager.Singleton.SceneManager.VerifySceneBeforeLoading +=
        VerifySceneBeforeLoading;
}

private bool VerifySceneBeforeLoading(int sceneIndex, string sceneName,
    LoadSceneMode loadSceneMode)
{
    // Client checks its local graph view: is this room within my load radius?
    int roomId = RoomRegistry.GetRoomIdForScene(sceneName);
    return _localGraph.IsWithinLoadRadius(
        playerCurrentRoomId: LocalPlayer.CurrentRoomId,
        targetRoomId: roomId,
        radius: 2); // Load 2 hops ahead
}
```

**Important:** The server still loads *all* rooms that any player needs. `VerifySceneBeforeLoading` prevents individual clients from loading rooms outside their viewport — it does not prevent the server-side load.

***

## 7. Full Integration: The Mutation Flow End-to-End

```
SERVER                                  CLIENT 1                CLIENT 2
──────                                  ────────                ────────

[Trigger: Room A → C swap]

1. FindPlayersInPortalZone()
   → Player 2 is in zone
   → TeleportPlayerClientRpc(safePos)                          [Teleport to Room A]

2. SetPortalTraversable(false)

3. Edges.Remove(A↔B)
   Edges.Add(A↔C, IsActive=false)        [OnListChanged fires] [OnListChanged fires]
                                         [Queues mutation]      [Queues mutation]
                                         [Unload Room B]        [Unload Room B]
                                         [Load Room C...]       [Load Room C...]

4. NetworkSceneManager.LoadScene("Room_C", Additive)
                                         [SceneEventType.Load]  [SceneEventType.Load]
                                         ...(0.8s)...           ...(2.5s)...

5. SceneEventType.LoadComplete (Client 1)
   SceneEventType.LoadComplete (Client 2)

6. SceneEventType.LoadEventCompleted
   (all clients confirmed)
   → SetPortalTraversable(true)
   → UpdateEdge IsActive = true          [OnValueChanged]       [OnValueChanged]
   → SpawnRoomInteractables("Room_C")    [Portal opens]         [Portal opens]
   → PlayMutationVFXClientRpc(portalId)  [VFX plays]            [VFX plays]
```

***

## 8. Late-Joining Clients

NGO's additive synchronization mode handles late joiners cleanly because `NetworkList` delivers its full current state on connection. The late-joining client:[^14][^4]

1. Receives the current `Edges` snapshot — knows exactly which rooms should be loaded.
2. Receives the `ActiveRoomIds` snapshot — knows which rooms have active players.
3. Loads only rooms within its personal `VerifySceneBeforeLoading` radius.
4. Is blocked from spawning in-room `NetworkObject`s until the server confirms all relevant scenes are loaded.

Use `PostSynchronizationSceneUnloading = true` on the client's `NetworkSceneManager` to ensure any locally pre-loaded rooms (from a previous session) that aren't in the server's current graph are unloaded automatically on connect.[^14]

***

## 9. Bootstrap Scene Architecture

```
bootstrap.unity (never unloads, DontDestroyOnLoad)
├── NetworkManager
├── RoomGraphAuthority (NetworkBehaviour — owns NetworkList<RoomEdge>)
├── RoomStreamingServer (server-only MonoBehaviour — drives mutations)
└── RoomStreamingClient (client-only MonoBehaviour — reconciles local scenes)

Room_Hallway_01.unity (additively loaded/unloaded per graph state)
├── [Room geometry — static, no NetworkObjects]
├── RoomPortal_North (NetworkObject — lightweight, syncs RoomEdge link)
└── RoomPortal_South (NetworkObject)

Room_Bathroom_02.unity (additively loaded/unloaded per graph state)
├── [Room geometry]
├── Door_Interactable (NetworkObject — door open/close state)
└── LightSwitch (NetworkObject)
```

This architecture ensures:
- The `NetworkManager` never gets destroyed by scene unloads[^15]
- Room geometry never enters NGO's object tracking
- Graph state is always a `NetworkList` delta-sync, not full-state RPC blasts
- Late joiners receive consistent state through NGO's built-in synchronization

***

## Performance Summary

| Concern | Cost | Mitigation |
|---|---|---|
| `NetworkList<RoomEdge>` bandwidth per mutation | ~16 bytes per edge delta | Struct is 4 ints — minimal serialization cost[^16] |
| `LoadEventCompleted` wait time | Worst-case: slowest client's load time (~3–5s) | Show portal-sealed visual; don't block other gameplay |
| Spawning room interactables on `LoadEventCompleted` | Burst spawn on server | Stagger to ≤100 NetworkObjects/frame[^1] |
| Late joiner synchronization | Proportional to active edge count, not room count | Keep edge count small; geometry is never synced |
| Client drift on rapid mutations | Possible if mutation queue backs up | Serialize mutations through a queue; use GraphVersion counter |

---

## References

1. [Spawning too many network objects at once #1570 - GitHub](https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/issues/1570) - If the Prefab contains only a NetworkObject, it works for up to 700 objects (750 does not). If the P...

2. [Scene events | Netcode for GameObjects | 2.5.1 - Unity - Manual](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.5/manual/basics/scenemanagement/scene-events.html) - SceneEventType.LoadEventCompleted : signifies that the server and all clients have finished loading ...

3. [Advanced State Synchronization - Unity - Manual](https://docs.unity3d.com/2020.3/Documentation/Manual/UNetStateSync-Advanced.html) - This page is only relevant for advanced developers who need customized synchronization solutions tha...

4. [NetworkVariables | Netcode for GameObjects | 2.5.1 - Unity - Manual](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.5/manual/basics/networkvariable.html) - When a client first connects, it's synchronized with the current value of the NetworkVariable . Typi...

5. [NetworkVariable<Dictionary> not syncing or firing OnValueChanged ...](https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/issues/2993) - This results in clients not syncing. (only the value on the server seems to change, which network va...

6. [INetworkSerializable | Netcode for GameObjects | 2.11.0](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.11/manual/advanced-topics/serialization/inetworkserializable.html) - You can use the INetworkSerializable interface to define custom serializable types. struct MyComplex...

7. [Custom Network Variables do not trigger Code Generation ... - GitHub](https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/issues/2686) - Custom Network Variables do not trigger Code Generation for Network Serialization #2686 ... Removing...

8. [How to use the "built-in support for `NativeHashMap`" of ... - GitHub](https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/issues/2960) - NGO v1.9.1 Unity 2022.3.22f1 as release blog says, NetworkVariable now includes built-in support for...

9. [Tricks and patterns to deal with latency | Netcode for GameObjects](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.7/manual/learn/dealing-with-latency.html) - Tricks and patterns to deal with latency. TL;DRs. As mentioned in latency and tick, waiting for your...

10. [Using NetworkSceneManager | Netcode for GameObjects | 2.4.4](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.4/manual/basics/scenemanagement/using-networkscenemanager.html) - Each client receives the SceneEventType.LoadEventCompleted event. At this point all clients have com...

11. [SceneEventType.LoadEventCompleted fires early because ... - GitHub](https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/issues/1362) - LoadEventCompleted will fire while the fourth client is still loading and the fourth client will rec...

12. [Class NetworkSceneManager | Netcode for GameObjects | 1.3.1](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@1.3/api/Unity.Netcode.NetworkSceneManager.html) - VerifySceneBeforeLoadingDelegateHandler that is invoked before the server or client loads a scene du...

13. [Field VerifySceneBeforeLoading | Netcode for GameObjects | 1.7.1](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@1.7/api/Unity.Netcode.NetworkSceneManager.VerifySceneBeforeLoading.html) - Client Side: In order for clients to be notified of this condition you must assign the VerifySceneBe...

14. [Client synchronization mode | Netcode for GameObjects | 2.7.0](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.7/manual/basics/scenemanagement/client-synchronization-mode.html) - By using LoadSceneMode.additive client synchronization, the synchronizing client only has to load on...

15. [Unity Netcode Scene Switch Disconnects Clients - NGO Load Event ...](https://gamineai.com/help/unity-netcode-scene-switch-disconnects-clients-ngo-load-event-fix) - Symptom: Clients appear to load the wrong scene, or desync then disconnect; logs may show NGO warnin...

16. [INetworkSerializable | Netcode for GameObjects | 2.5.1](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.5/manual/advanced-topics/serialization/inetworkserializable.html) - It's possible to recursively serialize nested members with INetworkSerializable interface down in th...

