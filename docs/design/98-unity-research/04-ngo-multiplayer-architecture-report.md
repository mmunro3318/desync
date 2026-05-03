# Run 4 — NGO Multiplayer Architecture for a 2–4 Player Co-op Horror Prototype

## Overview
This report covers Netcode for GameObjects (NGO) architecture, the client-server authority model, core NGO primitives (NetworkObject, NetworkBehaviour, NetworkVariable, RPCs, NetworkTransform), ownership and synchronization patterns, spawn/despawn rules, topology choices, and the most common pitfalls for 2–4 player co-op prototypes. It addresses Checklist D (Part 1) from the project research brief and assumes the Unity 6.4 + NGO + client-server (host) topology established in Run 0.[cite:38][cite:41][cite:336]

The central recommendation is: **use the client-server (host) topology, keep the server authoritative for all shared world state, give NetworkObject ownership only where a specific player needs to send authoritative updates, put all NGO initialization in `OnNetworkSpawn` (never `Awake` or `Start` for networked state), and use NetworkVariables for persistent state and RPCs for one-shot events**.[cite:295][cite:303][cite:318][cite:336]

## Executive Summary
NGO's architecture is not complicated once its five core rules are clear. Everything else — doors, interactables, observation locks, spatial runtime state, portals — follows from those rules cleanly. The hard part is not learning the API; it is internalizing the ownership model before writing any gameplay code, because violating it produces bugs that are invisible locally and catastrophic across machines.[cite:295][cite:300][cite:336]

The five core rules are:
1. Only the server/authority may spawn or despawn NetworkObjects.[cite:315]
2. `OnNetworkSpawn` is the correct initialization point for all NGO-related logic — not `Awake`, not `Start`.[cite:318][cite:321]
3. NetworkVariables store persistent replicated state; RPCs send transient events.[cite:303]
4. Server-authoritative means the server runs the simulation and validates decisions; clients request, they do not execute.[cite:295][cite:298][cite:309]
5. Scene loading must go through `NetworkSceneManager`, never raw `SceneManager`.[cite:233]

## NGO topology choice: client-server (host) for this project
### Two topologies available
NGO now supports two topologies: **client-server** (the default) and **distributed authority** (introduced in Unity 6).[cite:336][cite:330]

| Topology | Authority model | Best for | Notes |
|---|---|---|---|
| **Client-server (host)** | Server owns all authoritative state; clients request changes.[cite:295][cite:336] | 2–4 player co-op where one player is host; this project[cite:300] | Default NGO topology; best-documented; all official samples use it. |
| **Distributed authority** | Authority distributed across clients; session owner manages global state.[cite:330][cite:333] | Casual hosted sessions without a dedicated server; reduces cost | Still newer; carries documented edge-case bugs including late-join scene ordering issues.[cite:334] |

### Recommendation for this project
Use **client-server topology with a listen-server/host** (one player runs both server and client). This is the default NGO mode, the best-documented path, and the correct fit for a 2–4 player horror co-op where someone is hosting from their machine.[cite:38][cite:41][cite:336]

Distributed authority is an interesting future option but carries additional complexity, newer code paths with known edge-case bugs, and shifts conceptual ownership in ways that complicate the impossible-house's spatial authority model.[cite:330][cite:334][cite:336]

## Core NGO primitives
### NetworkManager
The central NGO component. It must be present in the scene before any networking starts, must persist across scene loads, and is the entry point for starting a host, server, or client session.[cite:41][cite:48]

**Critical rule**: NetworkManager should live in the bootstrap scene so it is never destroyed during gameplay scene transitions. Any scene loading that unloads the scene containing NetworkManager will disconnect all clients.[cite:230][cite:233]

NetworkManager owns:
- The list of registered network prefabs (all prefabs with NetworkObject components must be registered here).[cite:48]
- The Player Prefab reference.[cite:41]
- NetworkSceneManager (for multiplayer-safe scene loading).[cite:233]
- Connection approval callbacks.[cite:48]

### NetworkObject
The component that marks a GameObject as a networked entity. Any object that needs to be synchronized across the network must have a `NetworkObject` component.[cite:294][cite:315]

Important distinctions:
- **In-scene placed NetworkObjects**: placed in the scene in the editor, spawned automatically when the scene loads over the network.[cite:318] `OnNetworkSpawn` is called after `Start` for these.
- **Dynamically spawned NetworkObjects**: prefabs instantiated at runtime via `NetworkObject.Spawn()`.[cite:315] `OnNetworkSpawn` is called before `Start` for these.

**Common mistake**: treating in-scene placed and dynamically spawned NetworkObjects as identical. The `Awake`/`Start`/`OnNetworkSpawn` call order differs between the two, causing initialization bugs if code assumes one order and gets the other.[cite:318][cite:321]

### NetworkBehaviour
An abstract class that inherits from MonoBehaviour. Any MonoBehaviour that needs to access NGO features — NetworkVariables, RPCs, `IsServer`, `IsClient`, `IsOwner`, `OnNetworkSpawn` — must inherit from `NetworkBehaviour` instead of `MonoBehaviour`.[cite:294][cite:318]

A NetworkBehaviour must be on a GameObject that has a `NetworkObject` component — either on the same object or a parent.[cite:294]

**Key lifecycle events**:[cite:318][cite:321]
- `OnNetworkSpawn()`: called when the object is fully spawned and network state is ready. **All NGO initialization belongs here**, not in `Awake` or `Start`.
- `OnNetworkDespawn()`: called before the object is removed from the network. Clean up subscriptions and state here.
- `OnGainedOwnership()` / `OnLostOwnership()`: called when ownership changes.

### NetworkVariable
The primary mechanism for persistent replicated state. A `NetworkVariable<T>` holds a value that is automatically synchronized to all clients whenever it changes on the server.[cite:72][cite:303]

Key behavior:
- By default, only the server can write; all clients can read.[cite:72]
- Changes are eventually consistent: if a value changes five times rapidly, only the latest value is guaranteed to arrive, not all five changes.[cite:303]
- Late-joining clients receive the current value automatically — this is its main advantage over RPCs.[cite:303]

**Correct use: when to use NetworkVariable**[cite:303]:
- Any state that must be correct for a late-joining client.
- Any state that needs to be continuously readable by any client.
- Examples: door open/closed state, observation lock state, player health, item carried status, room visibility flags.

```csharp
public class DoorStateSync : NetworkBehaviour
{
    public NetworkVariable<bool> IsOpen = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        IsOpen.OnValueChanged += OnDoorStateChanged;
    }

    private void OnDoorStateChanged(bool previous, bool current)
    {
        // Runs on all clients. Update visual/audio here.
    }
}
```

### RPCs (Remote Procedure Calls)
Mechanism for sending one-shot events across the network. NGO uses the `[Rpc]` attribute system in NGO 2.x, with `[ServerRpc]` and `[ClientRpc]` attribute variants still documented.[cite:304][cite:303]

**Correct use: when to use RPC**[cite:303]:
- Transient events that do not need persistent state.
- Events where you need all parameter values delivered together atomically.
- One-shot triggers: "play this sound," "trigger this animation," "this door was kicked."
- Client-to-server: client sends input event to server; server validates and acts.

**Critical rule**: never mix RPCs and NetworkVariable ownership changes in the same flow without careful ordering. Community bug reports and GitHub issues confirm that changing ownership and updating a NetworkVariable in the same server RPC can cause the `OnValueChanged` callback to not fire on non-host clients in some NGO versions.[cite:296]

### NetworkTransform
The built-in component for synchronizing position, rotation, and scale across the network. It interpolates between received network ticks to produce smooth movement on remote clients.[cite:308][cite:305]

NGO's documentation explicitly recommends using `NetworkTransform` for smooth client-side interpolation by running clients slightly behind the server, giving them time to transition smoothly between state updates.[cite:308]

**Important NetworkTransform behaviors**:
- By default in client-server mode, the server owns and drives the transform; clients receive interpolated updates.[cite:295][cite:308]
- For player-owned movement, the common pattern is a custom `NetworkTransform` subclass that overrides `OnIsServerAuthoritative()` to return `false` for owner-driven movement, while the server still validates and reconciles.[cite:306][cite:309]
- The interpolation period should always be less than the server send interval to avoid stutter.[cite:308]

## Ownership and authority model
### The default: server owns everything
In client-server NGO, the server owns all spawned NetworkObjects by default. Only the server may spawn and despawn NetworkObjects. Clients calling `Destroy()` on a NetworkObject they do not own generates an error and has no effect across the network.[cite:295][cite:315][cite:316]

```
// WRONG — calling on a client that does not own the object
Destroy(gameObject); // "Destroy a spawned NetworkObject on a non-host client is not valid"

// CORRECT — clients request; server acts
[ServerRpc]
private void RequestDestroyServerRpc() {
    GetComponent<NetworkObject>().Despawn();
}
```

### Giving ownership to a client
Ownership can be transferred to a specific client using `NetworkObject.ChangeOwnership(clientId)`. This is appropriate for:
- Player-owned objects (each player owns their player NetworkObject).[cite:295]
- Objects a specific player picks up and carries.[cite:295]
- Anything where a single client needs to write directly to a NetworkVariable without a server round-trip.

Ownership should be transferred **back to the server** (or another appropriate client) when the object is dropped or the original owner disconnects.[cite:295][cite:315]

### Server-authoritative interaction pattern
For interactables in a horror co-op game (doors, drawers, switches, items), the correct pattern is:
1. Client detects input and sends a `[ServerRpc]` with the interaction request.
2. Server validates the request (is this client close enough? is the object locked? who has authority?).
3. Server updates the relevant `NetworkVariable` or triggers a `[ClientRpc]` for the visual/audio result.
4. All clients read the updated `NetworkVariable` and update their local presentation.

```csharp
public class DoorInteractable : NetworkBehaviour
{
    public NetworkVariable<bool> IsOpen = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Client calls this to request interaction
    [ServerRpc(RequireOwnership = false)]
    public void RequestToggleDoorServerRpc(ServerRpcParams rpcParams = default)
    {
        // Server validates, then updates state
        IsOpen.Value = !IsOpen.Value;
    }

    public override void OnNetworkSpawn()
    {
        IsOpen.OnValueChanged += HandleDoorStateChanged;
    }

    private void HandleDoorStateChanged(bool previous, bool current)
    {
        // Visual/audio update — runs on all clients including server
        PlayDoorAnimation(current);
    }
}
```

Note `RequireOwnership = false` — this allows any client (not just the owner) to send the ServerRpc, which is the correct setting for shared interactables.[cite:295][cite:304]

## In-scene placed vs. dynamically spawned NetworkObjects
This is one of the most important practical distinctions for this project.[cite:318]

### In-scene placed NetworkObjects
- Placed in the scene editor.
- Automatically registered with NGO when the scene loads via `NetworkSceneManager`.
- Used for: static world interactables (doors, switches, locks), room trigger volumes, observation zones, static props with networked state.
- `OnNetworkSpawn` is called **after** `Start`.[cite:318][cite:321]
- **Important**: do not put in-scene placed NetworkObjects in a scene that is not managed by `NetworkSceneManager`. They will not be properly registered.[cite:337]

### Dynamically spawned NetworkObjects
- Instantiated at runtime from registered prefabs via `NetworkObject.Spawn()` (server only).
- Used for: player characters, picked-up items, spawned creatures, runtime-created entities.
- `OnNetworkSpawn` is called **before** `Start`.[cite:318][cite:321]
- Prefabs must be in the NetworkManager's Network Prefab list, or they will fail to spawn on clients.

**The initialization trap**: if initialization code in `Start()` accesses a component that is set up in `OnNetworkSpawn()`, it will work for in-scene placed objects (where `Start` runs first) but fail for dynamically spawned objects (where `OnNetworkSpawn` runs first). Always initialize in a shared `Initialize()` method called from both `Start` and `OnNetworkSpawn`.[cite:318][cite:321]

## NetworkObject parenting rules
NGO has specific rules for parenting that developers commonly violate.[cite:331]

Key rules:
- A NetworkObject can only be parented under another **spawned** NetworkObject during a network session.[cite:331]
- You cannot nest NetworkObject components inside a network prefab in the editor (only in-scene placed NetworkObjects can have nested child NetworkObjects).[cite:331][cite:294]
- Parent-child NetworkObject relationships are replicated to all clients including late joiners, but only if the server performs the parenting.[cite:331]

**Relevant for this project**: when a player picks up an item (NetworkObject), the "held item is parented to player" relationship must be set by the server, not the client. The common pattern is a ServerRpc that performs the parenting after validating the pickup request.[cite:331][cite:295]

## Synchronization boundaries for this project
Not every value should be a NetworkVariable. Defining synchronization scope early is one of the most important architectural decisions.[cite:300][cite:303]

### What should be networked
| State | Mechanism | Notes |
|---|---|---|
| Door open/closed | NetworkVariable<bool>[cite:72] | All clients need current state; late joiners need it too |
| Item carried by player | NetworkVariable<ulong> (owner client ID) or NetworkVariable<bool>[cite:72] | Drives all visual + interaction logic |
| Observation lock active | NetworkVariable<bool>[cite:72] | Core gameplay state |
| Room graph connectivity | Server-managed NetworkVariables or RPC-triggered state update | Depends on spatial runtime design |
| Player position/rotation | NetworkTransform[cite:308] | Use built-in component |
| Player health/fear state | NetworkVariable<float>[cite:72] | Persistent, readable by all |
| One-shot audio triggers | ClientRpc[cite:303][cite:304] | Transient; no need to persist |
| Animation one-shot triggers | ClientRpc[cite:303][cite:304] | Transient |
| Interaction request | ServerRpc[cite:304] | Client requests; server validates |

### What should NOT be networked
- Local visual effects (particles, local audio, screen shake) — run these locally on receipt of state change.
- Camera state — each client owns their own camera; never replicate it.
- UI state — each client manages their own UI.
- Cursor/input state — never replicate raw input; replicate the results of inputs instead.[cite:300][cite:309]

## Critical NGO pitfalls for this project
### 1. Raw SceneManager.LoadScene in multiplayer
Already covered in Run 2 but bears repeating: any scene load during an active NGO session must go through `NetworkManager.Singleton.SceneManager.LoadScene`. Raw `SceneManager` calls cause client disconnects, GUID mismatches, and scene state corruption.[cite:233][cite:230]

### 2. Initializing NetworkVariable subscriptions outside OnNetworkSpawn
Subscribing to `NetworkVariable.OnValueChanged` in `Awake` or `Start` instead of `OnNetworkSpawn` causes the subscription to fire before the network is ready or not fire at all. Always subscribe inside `OnNetworkSpawn` and unsubscribe in `OnNetworkDespawn`.[cite:318][cite:321]

### 3. Clients calling Destroy() on NetworkObjects
Non-owner clients cannot destroy NetworkObjects. The call must go through a ServerRpc that calls `NetworkObject.Despawn()`.[cite:315][cite:316]

### 4. Forgetting RequireOwnership = false on shared interactables
By default, a `[ServerRpc]` can only be called by the client that owns the NetworkObject. Interactables like doors are owned by the server. Any client should be able to trigger them. Always use `[ServerRpc(RequireOwnership = false)]` on interactables that any player should be able to use.[cite:295][cite:304]

### 5. OnValueChanged not firing after ownership changes in the same RPC
This is a documented NGO bug in some versions: changing ownership and updating a NetworkVariable in the same ServerRpc can result in `OnValueChanged` not being delivered to non-host clients.[cite:296] Separate ownership-change calls and NetworkVariable updates into distinct steps or RPCs when both are needed.

### 6. Prefab not in the NetworkPrefab list
Any prefab containing a NetworkObject that is spawned dynamically must be registered in the NetworkManager's Network Prefabs list. Failing to register it causes a client-side error and the object does not appear on remote clients. This is one of the most common NGO beginner mistakes.[cite:41][cite:48]

### 7. Works locally but fails across machines
The most common root causes of "works on host+local client but not on separate machines":
- Network Prefab list missing entries.[cite:41]
- State only initialized on server — clients never receive the initial value because they joined after the object was set (use NetworkVariable for initial state, not a one-shot RPC on spawn).[cite:303]
- Scene was loaded without `NetworkSceneManager`, so objects do not exist on the client.[cite:233][cite:230]
- Inspector-wired references that only exist in the editor context, not after a client-side instantiation.[cite:300]

## Recommended patterns for this project
### Player character pattern
```csharp
public class PlayerController : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) {
            // Disable local player components for non-owners
            GetComponent<CharacterController>().enabled = false;
            GetComponentInChildren<Camera>().enabled = false;
            return;
        }
        // Set up input, camera, etc. for local player only
    }
}
```
Each client owns their own player object. Only the owner runs input and camera logic. All other clients see the replicated transform and state.[cite:295][cite:308]

### Interactable world object pattern
```csharp
public class WorldInteractable : NetworkBehaviour
{
    public NetworkVariable<bool> IsActive = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        IsActive.OnValueChanged += OnActiveStateChanged;
        // Apply initial state on spawn for late joiners
        OnActiveStateChanged(false, IsActive.Value);
    }

    public override void OnNetworkDespawn()
    {
        IsActive.OnValueChanged -= OnActiveStateChanged;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestActivateServerRpc() {
        // Server validates and sets state
        IsActive.Value = true;
    }

    private void OnActiveStateChanged(bool prev, bool curr) {
        // Update visuals/audio on all clients
    }
}
```

## What Claude should generally do in networking code
- Always inherit from `NetworkBehaviour` when NGO features are needed.[cite:294]
- Put all NGO initialization in `OnNetworkSpawn`, always.[cite:318][cite:321]
- Unsubscribe from NetworkVariable events in `OnNetworkDespawn`.[cite:318]
- Use `NetworkVariable` for persistent shared state, RPCs for events.[cite:303]
- Use `[ServerRpc(RequireOwnership = false)]` on interactables any player can trigger.[cite:295][cite:304]
- Check `IsOwner` before running local-player-only logic (camera, input).[cite:295]
- Register all dynamic spawn prefabs in the NetworkManager's Network Prefab list.[cite:41][cite:48]
- Use `NetworkManager.Singleton.SceneManager.LoadScene` for all scene transitions.[cite:233]
- Apply initial state in `OnNetworkSpawn` from NetworkVariable value to support late joiners.[cite:303]

## What Claude should generally avoid
- Initializing NetworkVariable subscriptions in `Awake` or `Start`.[cite:318]
- Clients calling `Destroy()` on NetworkObjects they do not own.[cite:315][cite:316]
- Replicating raw input or camera state over the network.[cite:300][cite:309]
- Spawning NetworkObjects from client code (only server may spawn).[cite:315]
- Mixing ownership transfer and NetworkVariable updates in the same RPC without accounting for the known callback bug.[cite:296]
- Using distributed authority topology without understanding its additional complexity and edge cases.[cite:330][cite:334]
- Putting runtime gameplay state in serialized scene Inspector fields instead of NetworkVariables.[cite:303][cite:300]

## Conclusion
NGO's architecture becomes manageable once its ownership model, initialization order, and state/event split are understood clearly. For the impossible-house prototype, the practical mapping is direct: server-owned NetworkObjects for world state (doors, locks, room flags, observation locks), player-owned NetworkObjects for player characters, server-authoritative interaction RPCs for all shared interactables, and NetworkVariables for all state that late joiners need to receive correctly.[cite:295][cite:303][cite:315]

The patterns in this report — especially the interactable template with `RequireOwnership = false` and `OnNetworkSpawn`-based initialization — should become the standard templates Claude uses when generating any NGO component code in this repo.[cite:295][cite:318][cite:321]
