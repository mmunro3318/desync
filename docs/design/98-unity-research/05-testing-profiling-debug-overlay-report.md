# Run 5 — Co-op Testing Workflow, Performance Profiling, and Runtime Debug Observability

## Overview

This report covers Checklist D (Part 2) and Checklist E from the project research brief: the full local multiplayer testing pipeline, the Unity 6 profiling toolchain, runtime debug/observability tooling, NGO-specific network stats monitoring, and object pooling patterns for a 2–4 player co-op horror prototype. All recommendations assume Unity 6.4 + NGO + URP on a small team with a solo or paired development workflow.

---

## Executive Summary

Three distinct problems require three distinct solutions. **Testing** is solved by Multiplayer Play Mode (MPPM) in Unity 6 for fast editor iteration plus ParrelSync as a fallback. **Profiling** is solved by the Unity Profiler (CPU/GPU bottleneck identification) + Memory Profiler package (leak detection) + Project Auditor (static analysis). **Runtime observability** is solved by a lightweight custom HUD built on `ProfilerRecorder` + the NGO Runtime Network Stats Monitor (RNSM) + a gated in-game debug console. These three layers operate at different phases of development and should be kept separate rather than merged into a single system.[cite:343][cite:354][cite:387]

---

## Part 1 — Local Multiplayer Testing

### Multiplayer Play Mode (MPPM): the primary tool

**Multiplayer Play Mode (MPPM)** is Unity's official local multiplayer testing solution introduced in Unity 6. It simulates up to four players simultaneously within the editor without building the project, using virtual player instances that share the same project.[cite:355][cite:358]

Install via **Window > Package Manager > Unity Registry > Multiplayer Play Mode**. Then open **Window > Multiplayer > Multiplayer Play Mode** and activate the desired number of virtual players.[cite:356][cite:358]

Key behaviors and constraints:[cite:355][cite:356][cite:358]
- Each virtual player runs a separate simulated editor instance but shares the same project assets and scene.
- All virtual players enter Play Mode together when you press Play in the main editor.
- MPPM is more resource-demanding than ParrelSync on lower-end machines — each virtual player consumes additional RAM and CPU.
- VPN active during MPPM can cause scene loading failures in NGO sessions; disable VPN during local testing.[cite:356]
- If using Unity Authentication, each virtual player must have a unique tag to disambiguate sessions.[cite:356]
- A known issue exists where the Runtime Network Stats Monitor (RNSM) reports incorrect bandwidth values when domain reload is disabled; enable domain reload during MPPM test sessions if accurate bandwidth figures are needed.[cite:395]

**MPPM vs. ParrelSync comparison:**

| Feature | MPPM | ParrelSync |
|---|---|---|
| Unity 6 support | Native[cite:355] | Yes (via UPM)[cite:348] |
| Number of instances | Up to 4 virtual players[cite:355] | Unlimited clones (RAM permitting)[cite:348] |
| Build required | No — runs in editor[cite:355] | No — runs in editor[cite:341] |
| Each instance debuggable | Limited | Full editor debugger on each clone[cite:341] |
| Setup complexity | Low — Unity package[cite:355] | Low — UPM install[cite:348] |
| Resource usage | Higher[cite:358] | Moderate[cite:341] |
| File editing in clone | N/A — shared project | Never edit assets in clone editors[cite:341] |
| Recommended for Unity 6 | **Yes — primary workflow**[cite:355] | Fallback or for full debugger access[cite:341] |

### ParrelSync: debugger-first fallback

ParrelSync opens additional editor instances that share the main project directory via symbolic links.[cite:348] It is the preferred tool when a specific bug requires attaching the Visual Studio debugger to multiple simultaneous client instances, which MPPM does not support with the same granularity.[cite:341][cite:344]

Install via UPM: add `https://github.com/VeriorPies/ParrelSync.git?path=/ParrelSync` to Package Manager. After cloning, open via **ParrelSync > Clones Manager > Create New Clone > Open in New Editor**.[cite:348]

**Hard rule**: never edit assets in clone editor windows. Only the main editor should write files; clones are read-only consumers.[cite:341]

### Test workflow for this project

Recommended iteration loop for the *impossible-house* prototype:

1. **Day-to-day feature work**: use MPPM with 2 virtual players (host + 1 client). Fast iteration, no build step.
2. **Debugging a specific NGO sync bug**: switch to ParrelSync to attach two debugger instances simultaneously.
3. **Pre-commit multi-player smoke tests**: MPPM with 3–4 virtual players to verify all player slots work.
4. **Full cross-machine validation**: one development machine as host, a second device as client — do this before each significant feature merge to catch "works locally but fails across machines" issues.

### Automated test coverage for networked code

Unity's Play Mode Tests run in the editor and can drive NGO sessions programmatically for logic validation. Use them for:
- Verifying that `NetworkVariable` initial values arrive correctly for late joiners.
- Confirming that `OnNetworkSpawn`/`OnNetworkDespawn` callbacks fire in the correct order.
- Validating ownership transfer logic for interactables.

Keep Play Mode Tests gated behind an assembly that is not included in player builds.

---

## Part 2 — Performance Profiling

### The profiling workflow: bottleneck first, optimize second

Unity's official guidance establishes one rule above all others: **determine your bottleneck before optimizing**.[cite:346] The bottleneck is either CPU-bound or GPU-bound, never "both equally" in practice. Targeting the wrong one wastes time.

**Frame budget reference:**

| Target FPS | Frame budget |
|---|---|
| 60 fps | 16.67 ms |
| 30 fps | 33.33 ms |

**Identifying CPU vs. GPU bottleneck in the Profiler:**[cite:346][cite:359]
- If `Gfx.WaitForPresentOnGfxThread` dominates on the main thread → **GPU-bound**: the CPU is waiting for the GPU to finish.
- If `Gfx.WaitForCommands` appears on the render thread → **CPU-bound**: the GPU is waiting for the CPU.
- Compare main thread CPU time (excluding VSync) against GPU time. Whichever is higher is the bottleneck.

### Unity Profiler

The Unity Profiler is the primary performance investigation tool. Access via **Window > Analysis > Profiler** (or Ctrl+7).[cite:354]

**Essential workflow steps:**[cite:346][cite:354]
1. Profile development builds on target hardware, not in-editor (editor adds overhead that skews results).
2. Enable GPU Usage recording in the Profiler toolbar to see GPU frame time alongside CPU.
3. Use **Timeline** view to see per-frame thread activity; use **Hierarchy** view to rank markers by self-time.
4. Look for allocation spikes (yellow GC markers) — any GC allocation per-frame in hot paths is a target.
5. Enable **Deep Profile** only for specific investigation sessions; it adds 10–20% overhead and should not be the default mode.

**Key markers to watch for a co-op horror prototype:**[cite:346][cite:354]
- `BehaviourUpdate` — total MonoBehaviour Update time; high values indicate too many Update calls or expensive per-frame logic.
- `PlayerLoop` > `FixedUpdate.PhysicsFixedUpdate` — physics cost; relevant for first-person controller and rigid-body props.
- `Render.Mesh` / `Camera.Render` — rendering cost; relevant to URP shadow pass and draw call count.
- NGO tick markers — network send/receive cost per frame.

### Memory Profiler Package

For detecting memory leaks and tracking allocation hot spots, the **Memory Profiler package** (installed via Package Manager) supersedes the in-editor Memory Profiler module in Unity 6.[cite:364]

**Leak detection workflow:**[cite:363][cite:364]
1. Attach the Memory Profiler to a running development build.
2. Load an empty/start scene and take a baseline snapshot.
3. Play through a gameplay session (including scene transitions).
4. Unload or transition to an empty scene, then take a second snapshot.
5. Use **Compare Snapshots** mode — objects present only in the second snapshot are memory leak candidates.

Common sources of leaks in NGO projects:[cite:363][cite:364]
- `NetworkVariable.OnValueChanged` subscriptions not unsubscribed in `OnNetworkDespawn`.
- Static references to destroyed scene objects after `NetworkSceneManager` transitions.
- Lists and collections declared inside `Update()` instead of as class-level cached fields.
- Addressable assets not explicitly released after a scene unload.

**GC hygiene rules for hot paths:**[cite:363][cite:364]
- Never instantiate `List<T>`, `Dictionary<K,V>`, or similar collections inside `Update()`, `FixedUpdate()`, or per-frame NetworkVariable callbacks. Declare them at class level and use `.Clear()` to reset.
- Avoid string concatenation inside Update; use `StringBuilder` with pre-allocated capacity.
- Use structs for frequently created, short-lived value types to keep them on the stack.

### Project Auditor

**Project Auditor** is a static analysis tool built into Unity 6 (as a package in Unity 6.1+) that scans scripts, assets, and project settings for performance, memory, and build-size issues without running the game.[cite:370][cite:374]

Open via **Window > Analysis > Project Auditor**. Click **Start Analysis** and review the generated report.[cite:370][cite:374]

Key checks relevant to this project:[cite:369][cite:370]
- Scripts: allocations in hot paths, missing `RequireComponent` attributes, boxing/unboxing, expensive LINQ.
- Assets: texture import settings (uncompressed textures, oversized mipmaps), mesh import settings.
- Project Settings: disabled optimized mesh data, suboptimal quality settings for the target platform.
- Build: unused shader variants, redundant packages.

Run Project Auditor at the start of each sprint as a static health check, not just before shipping.

---

## Part 3 — Runtime Debug Observability

### Design principle: layers, not a monolith

Debug observability should be built in three layers, each with a different audience and lifetime:[cite:347][cite:350]

| Layer | Audience | Toggle mechanism | Ships in release? |
|---|---|---|---|
| **Performance HUD** | Developer/QA | `DEVELOPMENT_BUILD` define or hotkey | No |
| **NGO network stats** | Developer | `DEVELOPMENT_BUILD` or scripting define | No (by default) |
| **In-game debug console** | Developer/QA | Hotkey / gesture, gated by scripting define | No |

### Layer 1 — Performance HUD (ProfilerRecorder)

The `ProfilerRecorder` API allows reading profiler counters directly in runtime code and displaying them in a custom in-game overlay — no Profiler window connection required, works in development builds on device.[cite:383][cite:384]

```csharp
using Unity.Profiling;
using UnityEngine;
using TMPro;
using System.Text;

public class PerformanceHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statsText;

    private ProfilerRecorder _mainThreadRecorder;
    private ProfilerRecorder _gcMemoryRecorder;
    private ProfilerRecorder _drawCallsRecorder;
    private readonly StringBuilder _sb = new StringBuilder(256);

    private void OnEnable()
    {
        _mainThreadRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 15);
        _gcMemoryRecorder   = ProfilerRecorder.StartNew(ProfilerCategory.Memory,   "GC Reserved Memory");
        _drawCallsRecorder  = ProfilerRecorder.StartNew(ProfilerCategory.Render,   "Draw Calls Count");
    }

    private void OnDisable()
    {
        _mainThreadRecorder.Dispose();
        _gcMemoryRecorder.Dispose();
        _drawCallsRecorder.Dispose();
    }

    private void Update()
    {
        if (!statsText) return;
        _sb.Clear();
        _sb.AppendLine($"Frame: {GetAvgMs(_mainThreadRecorder):F1} ms");
        _sb.AppendLine($"GC Mem: {_gcMemoryRecorder.LastValue / (1024 * 1024)} MB");
        _sb.AppendLine($"Draw Calls: {_drawCallsRecorder.LastValue}");
        statsText.text = _sb.ToString();
    }

    private static double GetAvgMs(ProfilerRecorder recorder)
    {
        if (recorder.Capacity == 0) return 0;
        long sum = 0;
        for (int i = 0; i < recorder.Count; i++) sum += recorder.GetSample(i).Value;
        return sum / (double)recorder.Count * 1e-6;
    }
}
```

**Key rules for the HUD:**[cite:384][cite:389]
- Always `Dispose()` recorders in `OnDisable` — undisposed recorders are a managed memory leak.
- Pre-allocate `StringBuilder` at class level; never `new StringBuilder()` in `Update`.
- Update text at a lower frequency (every 0.2–0.5 seconds) rather than every frame to avoid text mesh rebuild cost.
- Gate the entire HUD behind `#if DEVELOPMENT_BUILD` or a compile-time define so it cannot ship to players.
- In URP, the Rendering Debugger's **Display Stats** panel provides CPU + GPU timing and bottleneck percentage in development builds without writing any code — use this first before building a custom HUD.[cite:389]

Recommended HUD metrics for this project:
- Frame time (ms) and FPS
- GC Reserved Memory (MB)
- Draw Calls count
- Active NetworkObjects count (custom counter via `NetworkManager.Singleton.SpawnManager.SpawnedObjects.Count`)
- Local player position (for spatial debugging during room graph testing)

### Layer 2 — NGO Runtime Network Stats Monitor (RNSM)

The **Runtime Network Stats Monitor (RNSM)** is an official Unity component in the **Multiplayer Tools** package that displays NGO-specific network stats as an on-screen overlay at runtime.[cite:387][cite:390][cite:393]

Install the **Multiplayer Tools** package via Package Manager. Then add the `RuntimeNetStatsMonitor` component to any GameObject in your scene.[cite:387]

The RNSM displays configurable stats including:[cite:387][cite:390]
- Bandwidth (bytes/sec sent and received per client and server)
- NetworkVariable sync counts
- RPC call counts
- Packet loss and round-trip time

**RNSM is disabled in release builds by default.** To enable it in development builds only, it respects the `DEVELOPMENT_BUILD` scripting define automatically. To force-enable in all builds, add `UNITY_MP_TOOLS_NET_STATS_MONITOR_IMPLEMENTATION_ENABLED` to Scripting Define Symbols.[cite:387]

**Known issue**: RNSM reports incorrect bandwidth stats when domain reload is disabled. If `Enter Play Mode Options` has domain reload turned off for faster iteration, RNSM bandwidth figures will be unreliable. Re-enable domain reload when interpreting bandwidth data.[cite:395]

Custom stats can be injected into the RNSM display via `AddCustomValue()` — useful for surfacing game-specific network metrics like "doors synced this frame" or "observation lock NetworkVariable write count."[cite:387]

### Layer 3 — In-game Debug Console

A runtime debug console lets developers and QA test specific states without replaying all prerequisite steps from scratch — skip to a room, force a door to its open state, spawn all players, trigger the observation lock mechanic.[cite:373]

**Recommended library: yasirkula's In-Game Debug Console** (free, Asset Store and GitHub). It captures all `Debug.Log`/`LogWarning`/`LogError` output in a toggleable overlay, supports custom command registration, handles mobile, and costs 1 SetPass call with Sprite Packing enabled.[cite:376]

Alternative: **DavidF-Dev's DeveloperConsole** uses an attribute-driven `[ConsoleCommand]` system that requires less registration boilerplate.[cite:373]

**Command examples relevant to this project:**

```csharp
// Example using a command registration pattern
// Gate entire class with #if DEVELOPMENT_BUILD

#if DEVELOPMENT_BUILD
[ConsoleCommand("god")]
static void GodMode() => PlayerController.Local.SetInvincible(true);

[ConsoleCommand("skiproom")]
static void SkipToRoom(string roomId) => RoomManager.Instance.TeleportToRoom(roomId);

[ConsoleCommand("opendoor")]
static void ForceOpenDoor(int networkObjectId)
{
    // Server-side only — useful in host mode testing
    if (!NetworkManager.Singleton.IsServer) return;
    var obj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[(ulong)networkObjectId];
    obj?.GetComponent<DoorStateSync>()?.ForceOpen();
}

[ConsoleCommand("spawnall")]
static void SpawnAllPlayers() => GameManager.Instance.ForceSpawnAllClients();

[ConsoleCommand("resetflags")]
static void ResetRoomFlags() => RoomGraphManager.Instance.ResetAllNetworkFlags();
#endif
```

**Console security rules:**[cite:373]
- Gate all command registration behind `#if DEVELOPMENT_BUILD` or a scripting define — not just a runtime bool check. A runtime bool ships the code; a compile-time define removes it from the binary entirely.
- Never expose commands that modify server-authoritative state from client builds in ways that bypass NGO validation — restrict those commands to host/server context only.
- Document all debug commands in a QA handbook so they are not tribal knowledge.

---

## Part 4 — Object Pooling

### Why pooling matters in a networked co-op game

NGO's `NetworkObject.Spawn()` and `NetworkObject.Despawn()` are not free. Frequent instantiation and destruction of networked objects creates GC pressure on the server and generates additional network traffic for spawn/despawn messages. Object pooling addresses both.[cite:388][cite:391]

Unity 6 ships `UnityEngine.Pool.ObjectPool<T>` as the built-in pooling class — prefer it over custom pool implementations.[cite:391]

### Using UnityEngine.Pool

```csharp
using UnityEngine.Pool;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int defaultCapacity = 20;
    [SerializeField] private int maxSize = 100;

    private ObjectPool<GameObject> _pool;

    private void Awake()
    {
        _pool = new ObjectPool<GameObject>(
            createFunc:   () => Instantiate(projectilePrefab),
            actionOnGet:  obj => obj.SetActive(true),
            actionOnRelease: obj => obj.SetActive(false),
            actionOnDestroy: obj => Destroy(obj),
            collectionCheck: false,     // Disable in release builds
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );
    }

    public GameObject Get()  => _pool.Get();
    public void Release(GameObject obj) => _pool.Release(obj);
}
```

**Rules for pooling in NGO contexts:**[cite:388][cite:391]
- For networked objects, use `NetworkObject.Despawn(false)` (the `false` parameter keeps the GameObject alive for pooling) rather than `NetworkObject.Despawn()` which destroys the object.
- NGO has a built-in `NetworkPrefabHandler` interface that integrates with custom pools — implement it for networked prefabs that need pooling so spawn/despawn messages work correctly through the pool.[cite:391]
- Always reset all state (transform, NetworkVariables, health, animation) when returning an object to the pool. A pooled object that retains stale state from its last use is a subtle and hard-to-reproduce bug.
- Pre-warm pools during scene load or on a loading screen, not during gameplay, to avoid instantiation spikes at runtime.
- Disable `collectionCheck` in release builds (it causes a GC allocation per `Release()` call when enabled).

### What to pool in this project

| Object type | Pool? | Notes |
|---|---|---|
| Player characters | No | 2–4 instances; not worth pooling |
| In-scene interactables (doors, switches) | No | Static scene objects; no spawning |
| Short-lived VFX / particles | Yes | High frequency; significant GC savings |
| Audio source instances | Yes | Footsteps, ambient triggers |
| Dynamically spawned creatures | Yes if frequently spawned | Depends on design |
| UI notification popups | Yes | If using runtime world-space popups |

---

## What Claude should generally do

- Use MPPM as the primary local testing tool in Unity 6 for NGO co-op sessions.[cite:355][cite:358]
- Use ParrelSync when a full debugger on a secondary client instance is needed for a specific bug.[cite:341]
- Gate all debug HUD, RNSM, and console code behind `#if DEVELOPMENT_BUILD` or explicit scripting defines — never runtime booleans alone.[cite:373][cite:387]
- Always `Dispose()` `ProfilerRecorder` instances in `OnDisable`.[cite:384][cite:385]
- Identify CPU vs. GPU bottleneck before attempting any optimization.[cite:346][cite:359]
- Use the Memory Profiler's Compare Snapshots mode for leak detection after scene transitions.[cite:363][cite:364]
- Run Project Auditor at the start of each sprint as a static project health check.[cite:370][cite:374]
- Use `UnityEngine.Pool.ObjectPool<T>` for all pooling needs; avoid custom pool implementations.[cite:391]
- Use `NetworkObject.Despawn(false)` to keep pooled networked objects alive after despawn.[cite:388][cite:391]

## What Claude should generally avoid

- Profiling in the Unity Editor and treating those figures as representative — always profile development builds on target hardware.[cite:346][cite:354]
- Running Deep Profile as the default mode — it adds significant overhead and should only be used for targeted investigation sessions.[cite:346]
- Subscribing to NetworkVariable events without unsubscribing in `OnNetworkDespawn` (memory leak).[cite:363]
- Allocating collections (`new List<T>()`, `new StringBuilder()`) inside `Update()` or per-frame callbacks.[cite:364]
- Instantiating and destroying networked objects frequently without pooling in gameplay-hot paths.[cite:388][cite:391]
- Shipping debug console command registration code in release builds (compile out, do not just hide).[cite:373]
- Interpreting RNSM bandwidth data when domain reload is disabled (figures will be incorrect).[cite:395]
- Editing assets in ParrelSync clone editor windows.[cite:341]

---

## Conclusion

A productive co-op development workflow for this project requires all three layers to be in place early: MPPM for daily iteration, a lightweight `ProfilerRecorder`-based HUD for continuous frame-time visibility, and the RNSM for NGO bandwidth monitoring. The Memory Profiler and Project Auditor are sprint-level tools run on a schedule, not firefighting tools. Object pooling via `UnityEngine.Pool` should be added when frequent dynamic spawning is confirmed by Profiler data — pre-mature pooling adds complexity without proven benefit.[cite:346][cite:354][cite:364][cite:391]
