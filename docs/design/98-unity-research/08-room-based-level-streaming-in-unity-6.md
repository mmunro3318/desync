# Room-Based Level Streaming in Unity 6
### A Technical Analysis for Graph-Driven Horror Game Architecture

***

## Executive Summary

For a first-person horror game with a graph-mutating house layout in Unity 6, the recommended architecture is **Addressables-backed additive scene loading per room**, augmented by **manual renderer activation** for adjacent-room visibility, **Unity's built-in `ObjectPool<T>`** for recyclable room types, and a **stencil-buffer portal shader** for doorway rendering. At MVP scale (8–15 rooms) all rooms can stay in memory simultaneously with manual activation toggling; at 50+ rooms, Addressables streaming becomes mandatory. The multiplayer server should act as the single source of truth for which room scenes are loaded, using Unity NGO's `NetworkSceneManager` or Mirror's additive scene interest management to keep clients synchronized.

***

## 1. Additive Scene Loading (`LoadSceneAsync` Additive)

### How It Works

`SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive)` spreads asset deserialization across multiple frames, keeping the game loop alive while loading proceeds. The operation returns an `AsyncOperation`; setting `asyncOperation.allowSceneActivation = false` lets you pre-load up to 90% of the scene and then defer the final activation (the main-thread spike) to a moment you control — for example, when the player is not moving.[^1][^2]

The critical gotcha: that final 10% activation (calling `Awake()` and `OnEnable()` on every component) **always runs on the main thread in a single frame**, causing a measurable hitch regardless of how well the rest of the load was deferred. Unity's own Horizons engineering blog documents this and recommends keeping scene-root GameObjects **disabled** during load, activating them progressively over several frames to amortize the cost.[^2][^1]

### Memory Patterns

Loading additively means **both scenes live in memory simultaneously until the old one is unloaded**. A real-world test on a mid-tier Android device showed a ~40% RAM spike (1 GB → 1.4 GB peak) when loading a scene directly without an intermediate empty scene. Using an intermediate "loading" scene to release the old scene before bringing in the new one eliminates this spike. After calling `SceneManager.UnloadSceneAsync`, you **must** call `Resources.UnloadUnusedAssets()` or native memory (textures, meshes) will not be freed — this creates the classic staircase memory pattern where loads stack up. That call is itself async and can hitch if you run it during gameplay; place it during scene-transition windows.[^3][^4][^2]

### Occlusion Culling Caveat

If you use Unity's built-in baked occlusion culling (Umbra), all additive room scenes **must be open in the editor simultaneously during baking**. This is a significant authoring constraint for dynamically connected room graphs.[^5]

### Suitability Summary

| Criterion | Rating |
|---|---|
| Load latency | ~1–5 s depending on room complexity; mostly off main thread |
| Memory spike | High if rooms overlap; mitigated with empty-scene buffer |
| Frame hitch on activation | 1 large frame spike (all `Awake()` calls); mitigatable with disabled root GO trick |
| Works with Addressables | Yes — `Addressables.LoadSceneAsync` wraps `SceneManager` internally[^6] |
| MVP scale (8–15 rooms) | Overkill; prefabs simpler |
| 50+ room scale | Strongly preferred |

***

## 2. Prefab Instantiation

### Performance vs. Additive Scenes

Instantiating a room prefab avoids the scene serialization and manifest overhead, making it **faster for small rooms with no baked lighting**. However, each scene is capped at a 4 GB resource budget, and a single large scene containing many room prefabs can strain that limit. Critically, a scene carries its own lighting, lightmap UVs, and baked shadow data — prefabs do not. For a horror game heavily dependent on baked lighting, missing per-scene lightmaps is a major visual downgrade.[^7]

Community consensus among Unity developers is to treat scenes as the preferred vehicle for large asset sets, with prefabs used for **props, enemies, and interactables** within a scene rather than entire rooms. The Godot model (where everything is effectively a prefab) works because the engine's resource system was designed around it; Unity's engine internals still optimize best when large geometry lives in scenes.[^7]

### Addressables-Backed Prefabs

You can mark any room prefab as Addressable and load it via `Addressables.InstantiateAsync(key)` — this loads the underlying AssetBundle on demand, instantiates the prefab, and hooks it into Addressables' reference-counting system so the bundle unloads when no instances remain. Note that `Addressables.InstantiateAsync` has associated overhead; if you need to spawn the same room type repeatedly per frame, call `Addressables.LoadAssetAsync` once, cache the result, and use `GameObject.Instantiate()` directly.[^8]

### When to Prefer Prefabs Over Scenes

- Room geometry is simple (< 500 objects, no baked GI)
- Room layout is entirely procedural (no designer-authored lightmaps needed)
- You need fast in-editor iteration and want to skip build steps

***

## 3. Unity Addressables System

### Architecture Overview

Addressables wraps AssetBundles behind a key-based API. Every asset is identified by an address (string) or `AssetReference` (type-safe). When `LoadAssetAsync` is called, Addressables checks if the bundle is already in memory; on a cache hit it returns the handle immediately (same frame).[^9][^10]

### Reference Counting Lifecycle

The system increments a ref-count each time you call a load method and decrements it on `Addressables.Release(handle)`. The underlying AssetBundle is only unloaded when **every** asset in it reaches ref-count zero. This has an important implication for room streaming: if two rooms share a texture atlas in the same bundle and you release one room's assets while the other room is still loaded, the shared bundle stays alive — this is correct behavior. But if you release the *last* item in a bundle and then immediately re-request an asset from it, Unity performs a full unload/reload cycle, causing **asset churn** and a double latency hit. Combat this by keeping rooms in their own Addressable groups (one bundle per room) with shared materials placed in a dedicated `Shared_Materials` group.[^11]

### Async Patterns in Unity 6

Three patterns are available for awaiting loads:[^9]

```csharp
// 1. Coroutine (most common, compatible with all Unity versions)
loadHandle = Addressables.LoadAssetAsync<GameObject>(address);
yield return loadHandle;

// 2. Callback (fires same frame if already loaded)
loadHandle.Completed += h => { /* use h.Result */ };

// 3. async/await (C# 7+, Unity 6 supports natively)
await loadHandle.Task;
```

Unity 6 makes the `await` pattern a first-class citizen, but beware: awaiting in a `MonoBehaviour` without proper cancellation tokens can keep handles alive after scene unloads. Always call `Addressables.Release(loadHandle)` in `OnDestroy`.[^9]

### Addressables vs. Plain SceneManager

Without Addressables, `UnloadSceneAsync` does NOT free native assets (textures, meshes) automatically — you must manually call `Resources.UnloadUnusedAssets()`, which itself hitches. Addressables' reference counting handles this transparently as long as you mirror every load with a release. For a game that streams rooms in and out continuously, **Addressables is the correct long-term choice** even if plain `SceneManager` suffices at MVP scale.[^12][^11]

***

## 4. Object Pooling for Rooms

### When Pooling Applies

Unity's built-in `ObjectPool<T>` (introduced in Unity 2021, fully mature in Unity 6) is designed for objects that are repeatedly created and destroyed. For a horror game with recurring room *types* (hallway, bathroom, study), pooling room GameObjects avoids repeated instantiation and GC pressure. The pool keeps deactivated instances in a collection; `Get()` activates one, `Release()` returns it.[^13]

However, pooling is most valuable when:
- Instantiation cost is high (complex room with many components)
- The same room *type* reappears frequently (roguelite-style layout generation)
- Room state is fully resettable (no persistent interactable state baked in)

If each room is unique (only ever appears once in a session), pooling adds complexity without benefit.

### Pool + Addressables Pattern

The strongest pattern for 50+ room scale combines both systems:

1. **Addressables** manages per-room **asset bundles** — loading geometry and textures on demand.
2. **ObjectPool** manages **instantiated room GameObjects** — reusing the object skeleton while resetting state.

```csharp
// On room request
var handle = Addressables.LoadAssetAsync<GameObject>(roomKey);
await handle.Task;
var roomGO = pool.Get(); // reuse existing if available
roomGO.GetComponent<RoomController>().Initialize(handle.Result);

// On room release
pool.Release(roomGO);
Addressables.Release(handle); // decrements ref count
```

This way, Addressables unloads the bundle only when all pool instances of that room type are released and the ref-count drops to zero.

### Memory Cost

A deactivated pooled room still occupies GPU memory (textures remain resident on the GPU until explicitly freed). For 50+ rooms, pool only the 5–10 room types most likely to recur; let rare rooms be instantiated/destroyed normally.

***

## 5. Occlusion and Culling

### Unity's Baked Occlusion Culling (Umbra)

Unity 6 continues to use the **Umbra** occlusion culling system. During baking, Umbra voxelizes the scene, merges empty voxels into cells, and builds portal connections between cells. At runtime, the camera's current cell is looked up and a portal-graph traversal determines which cells (and thus objects) are visible. This is extremely fast at runtime (sub-millisecond queries) but requires all geometry to be **static** and present at bake time — making it fundamentally incompatible with a dynamically mutating room graph.[^14][^15][^16]

**Key limitation:** If your project generates or modifies scene geometry at runtime (inserting rooms, reconnecting portals), Unity's built-in occlusion culling is explicitly documented as unsuitable. Dynamic GameObjects cannot act as occluders.[^14]

### Occlusion Portals (Dynamic Doors)

Unity does offer **Occlusion Portal** components as a middle ground — box-shaped volumes that can be toggled open/closed at runtime via `myOcclusionPortal.open = bool`. When closed, the portal acts as an occluder for anything behind it (e.g., a closed door blocks rendering of the room beyond). This works within a baked dataset, but the portal **cannot be moved** — only toggled — so it won't handle dynamically repositioned doorways.[^17][^15]

### Manual Renderer Activation (Recommended for Graph-Mutating Rooms)

For a graph that mutates at runtime, **manual renderer enable/disable is the most practical culling strategy**. The approach:

1. Maintain a set of "currently visible" rooms = occupied room + all neighbors in the graph (distance ≤ 1 hop).
2. On graph mutation (room reconnection), diff the old and new visible sets.
3. Call `renderer.enabled = false` on rooms leaving the visible set; `renderer.enabled = true` on rooms entering it.

This is zero-cost for hidden rooms (no draw calls), and the enable/disable toggle is O(N renderers) on the frame it runs but zero cost otherwise. Combined with frustum culling (automatic, always on), this is highly efficient for the 8–50 room scales described.

| Approach | Works with Dynamic Graph | Runtime CPU Cost | Bake Required |
|---|---|---|---|
| Umbra Baked Occlusion | ❌ Static only[^14] | ~0.1 ms/frame query | Yes (minutes–hours) |
| Occlusion Portals | Partial (toggle only)[^15] | ~0 ms | Yes (baked data required) |
| Manual Renderer Enable | ✅ Fully dynamic | ~0 ms (outside toggle frames) | No |
| Camera Frustum Culling | ✅ Automatic | Automatic, ~0.1 ms | No |

***

## 6. The Portal Problem: Rendering Through Doorways

When the player looks through a doorway from Room A into Room B, Room B must be **visible** before it is fully "entered." This is one of the most nuanced rendering challenges in indoor games.

### Approach A: "Always Load Adjacent Rooms"

The simplest solution. When the player occupies Room A, ensure Room B (and all graph-adjacent rooms) are loaded and their renderers are enabled. The camera naturally sees them through the doorway opening. No special rendering tricks required.

**Cost:** Memory proportional to graph connectivity (worst case: a hub room with 6 doors loads 6 neighbor rooms simultaneously). For a 15-room house this is trivial; for 50+ rooms with high connectivity, memory budget must be designed for.

**Verdict:** Correct approach for MVP. Zero rendering overhead, zero shader complexity.

### Approach B: Render Texture + Second Camera

Place a secondary camera in Room B aligned to look at what the player would see through the doorway. Render to a `RenderTexture` and apply it to the doorway plane as a material.[^18]

**Problems:**
- Each doorway requires a full additional render pass. A Reddit thread measuring portal rendering overhead found **~4 ms per additional camera** in URP — with six doors open simultaneously, that's ~24 ms lost per frame.[^19]
- URP's forward pipeline is not optimized for multiple render targets. HDRP is worse due to deferred pass overhead.[^19]
- Portal appears as a "flat TV screen" — lacks depth continuity for seamless transitions.[^18]
- VR is essentially broken (no stereo).[^18]

**Verdict:** Avoid unless you can throttle cameras to render every 4th frame, and even then the latency artifact (Room B is 4 frames stale) can look wrong in VR or fast movement.[^19]

### Approach C: Stencil Buffer Portals

A stencil-based portal renders the doorway correctly at 1:1 pixel density with no separate camera pass. The technique:[^20]

1. Render the **doorway frame mesh** first, writing a unique stencil reference value per portal (e.g., `1`) but not writing to color or depth.[^21]
2. Render Room B's geometry with a stencil test: only draw pixels where stencil == 1.[^21]
3. Repair the depth buffer using an invisible mesh (to prevent Room A geometry from overdrawing Room B).[^20]

The Death's Door implementation (recreated in Unity URP) shows this works cleanly in a Shader Graph node with stencil test blocks. The depth repair step is the trickiest part and requires render order management.[^20]

**Cost:** No additional camera — just extra render state switches and depth buffer passes. Total overhead is roughly **0.5–1 ms per portal** visible on screen, far below the render texture approach.[^18]

**VR suitability:** Significantly better than render textures; pixel density is always 1:1.[^18]

**Caveat:** Requires URP or HDRP with custom shader passes. Stencil-based portals in Unity 6 URP need the Full Screen Pass Renderer Feature or a custom `ScriptableRenderPass` to manage render ordering correctly.

### Approach D: Geometry Portals (BSP/PVS-style)

Classic Quake-era approach: precompute potentially visible sets (PVS) so the engine knows exactly which rooms can see which. Not practical in Unity 6 for a dynamic graph, as it requires engine-level BSP tooling not exposed in the editor.

### Recommendation Matrix

| Approach | Render Cost | Visual Quality | Dynamic Graph Support | Complexity |
|---|---|---|---|---|
| Always load adjacent rooms | Zero extra | Native geometry | ✅ | Low |
| Render Texture camera | ~4 ms/portal[^19] | "TV screen" artifact | ✅ | Medium |
| Stencil buffer | ~0.5–1 ms/portal[^18] | Seamless | ✅ | High |
| Baked PVS/BSP | ~0 ms | Native | ❌ | Very High |

***

## 7. Multiplayer Architecture

### Server as Room Graph Authority

In a multiplayer scenario, the **server must be the single authoritative source** of which rooms are "active" in the graph. Clients should never independently load or unload rooms — they respond to server-issued room events. This maps cleanly onto Unity's `NetworkSceneManager` (NGO) or Mirror's additive scene management.

### Unity Netcode for GameObjects (NGO)

`NetworkSceneManager.LoadScene(roomScene, LoadSceneMode.Additive)` causes the server to load the scene and then **automatically propagate the load event to all connected clients**. The server always finishes loading and spawning `NetworkObject`s before clients receive the load event message. NGO's `PostSynchronizationSceneUnloading` flag can be used to ensure late-joining clients only load the rooms that are currently active on the server, not stale rooms from a previous session.[^22][^23]

Scene validation callbacks allow you to define **server-only scenes** (e.g., a game-state scene clients don't need) that are excluded from client synchronization.[^24]

### Mirror Networking

Mirror has a built-in example of additive multi-scene management with per-scene **interest management** — clients in a given scene (room) only receive network events from objects in their scene and adjacent scenes. The `MultiScene Network Manager` additively loads subscene instances and assigns players to their respective subscene. Mirror's `Scene Interest Management` component handles message filtering automatically.[^25][^26]

### What the Server Must Track

For a graph-mutating house, the server needs:

1. **Room graph state**: adjacency list, which rooms are connected to which via which portal edge.
2. **Active room set**: which rooms currently have at least one player within N hops (determines which scenes should be loaded on the server).
3. **Per-client visible set**: for each client, which rooms are within their visibility radius (determines which scenes to synchronize to that client).
4. **Graph mutation events**: when a room disconnects/reconnects, broadcast a graph-update RPC so all clients update their local graph state and request the appropriate scene loads/unloads.

### Load Timing and Race Conditions

Because `LoadSceneAsync` is asynchronous, clients may be in different room-load states momentarily. NGO's scene event system handles this with a `LoadEventCompleted` callback that fires only when **all clients** have finished loading the scene. Use this event to unlock gameplay in the new room (e.g., spawning room-specific objects, enabling traps) rather than relying on a fixed timer.[^23]

***

## 8. Recommended Architecture by Scale

### MVP (8–15 Rooms)

```
Persistent Scene (Game Manager, Player, Network Manager)
 └── All 15 Room Scenes loaded additively at startup
     └── Only current room + adjacent rooms have renderers enabled
     └── All other rooms: renderers disabled (zero draw calls)
     └── Manual renderer toggling on graph traversal
     └── Stencil portal shader on doorways (or "always load adjacent")
     └── Baked occlusion culling (all scenes open at bake time) if graph is mostly static
```

This is the simplest correct approach. Memory for 15 small indoor rooms is manageable (target < 500 MB total), and the activation cost is low. Use this until 50-room scale forces your hand.

### Post-MVP (50+ Rooms)

```
Persistent Scene (Game Manager, Player, Network Manager)
 └── Addressable Scene Groups (one bundle per room)
     └── RoomStreamingManager: maintains graph in memory as adjacency list
     └── On player move to Room A:
          - Queue unload of rooms outside radius 2 hops
          - Queue load of rooms within radius 2 hops
          - await Addressables.LoadSceneAsync for each new room
          - Activate scene (delayed via allowSceneActivation = false)
          - Enable renderers for radius 1 hop; disable for hop 2 (loaded but hidden)
     └── ObjectPool for recurring room types (hallways, bathrooms)
     └── Stencil portal rendering for visible doorways
     └── NetworkSceneManager for multiplayer scene synchronization
```

### Critical Code Sequence for Smooth Loading

```csharp
IEnumerator LoadRoom(string roomKey) {
    // 1. Load scene in background with activation deferred
    var handle = Addressables.LoadSceneAsync(roomKey, LoadSceneMode.Additive, activateOnLoad: false);
    yield return handle;

    // 2. Pre-warm: enable root GO while scene is still inactive
    // (reduces activation spike)
    var scene = handle.Result;

    // 3. Activate on player approach — choose a safe frame
    yield return scene.ActivateAsync();

    // 4. Enable renderers only when scene is active and player is adjacent
    SetRoomRenderersEnabled(scene, isAdjacentToPlayer: true);
}

IEnumerator UnloadRoom(string roomKey, AsyncOperationHandle<SceneInstance> handle) {
    SetRoomRenderersEnabled(handle.Result.Scene, false);
    yield return Addressables.UnloadSceneAsync(handle);
    // Addressables automatically tracks ref-count; no manual UnloadUnusedAssets needed
}
```

***

## Performance Cost Summary

| Technique | Memory Cost | CPU/Frame Cost | Load Latency | Multiplayer Fit |
|---|---|---|---|---|
| Additive Scene (SceneManager) | High during overlap | ~0 when idle | 1–5 s | Requires manual sync |
| Addressables Scene | Managed via ref-count | ~0 when idle | 1–5 s | Best with NetworkSceneManager[^6] |
| Prefab Instantiate | Spike on Instantiate | ~0 when idle | 50–500 ms | Manual sync needed |
| Addressables Prefab | Ref-counted, auto-unload | ~0 when idle | 50–500 ms | Manual sync needed |
| Renderer Enable/Disable | Zero (GPU) | ~0 per frame | < 1 ms | Trivial to sync |
| Umbra Occlusion Culling | +baked data in RAM | ~0.1 ms query | Bake-time cost | Static only[^14] |
| Render Texture Portal | +VRAM per portal | ~4 ms/portal[^19] | N/A | Works but costly |
| Stencil Portal | Negligible VRAM | ~0.5–1 ms/portal | N/A | Purely client-side |
| ObjectPool (room reuse) | Pool size × room size | ~0 per frame | ~0 (cached) | Pool state must be reset |

---

## References

1. [Avoiding Hitches When Loading Scenes in Unity - Meta for Developers](https://developers.meta.com/horizon/blog/avoiding-hitches-when-loading-scenes-in-unity/) - Why is my 'Async' load still causing frame drops? · Tip #1: Reduce the Number of GameObjects in your...

2. [Fix: Unity Scene Loading Freezes the Game | Bugnet Blog](https://bugnet.io/blog/fix-unity-scene-loading-freezes-game) - Quick answer: Unity freezes during scene loading because SceneManager.LoadScene is synchronous by de...

3. [Unity Scene Loading: is LoadSceneAsync actually better? : r/Unity3D](https://www.reddit.com/r/Unity3D/comments/1rbtans/unity_scene_loading_is_loadsceneasync_actually/) - RAM spikes => When using LoadSceneAsync , is there a point where both Scene 1 and Scene 2 are fully ...

4. [Fix Unity Additive Scene Not Unloading (Memory Leak) | Bugnet Blog](https://bugnet.io/blog/fix-unity-scene-not-unloading-additive-memory-leak) - Quick answer: Unity additive scenes often fail to unload because persistent scripts still hold refer...

5. [How to Use Occlusion Culling in Unity — The Sneaky Way](https://thegamedev.guru/unity-performance/occlusion-culling-tutorial/) - If you load scenes additively, then you need to bake occlusion culling with all of them open in the ...

6. [Load a scene | Addressables | 2.5.0 - Unity - Manual](https://docs.unity3d.com/Packages/com.unity.addressables@2.5/manual/LoadingScenes.html) - Addressables.LoadSceneAsync uses the Unity Engine SceneManager.LoadSceneAsync method internally. API...

7. [Pros and Cons of Loading Different Scenes vs. Instantiating Giant ...](https://www.reddit.com/r/Unity3D/comments/1nk8qdv/pros_and_cons_of_loading_different_scenes_vs/) - A problem you will quickly run into if you tried to use prefabs, is the resource limit, Every scene ...

8. [Memory management | Addressables | 1.17.17](https://docs.unity.cn/Packages/com.unity.addressables@1.17/manual/MemoryManagement.html) - When working with Addressable Assets, the primary way to ensure proper memory management is to mirro...

9. [Asynchronous loading | Addressables | 2.1.0 - Unity - Manual](https://docs.unity3d.com/Packages/com.unity.addressables@2.1/manual/load-assets-asynchronous.html) - Asynchronous loading. The Addressables system API is asynchronous and returns an AsyncOperationHandl...

10. [Configure your project to use Addressables - Unity - Manual](https://docs.unity3d.com/Packages/com.unity.addressables@2.6/manual/AddressableAssetsMigrationGuide.html) - Although you can integrate Addressables at any stage in a project's development, it's best practice ...

11. [Memory management overview | Addressables | 2.0.8 - Unity - Manual](https://docs.unity3d.com/Packages/com.unity.addressables@2.0/manual/MemoryManagement.html) - Memory management overview. The Addressables system keeps a reference count of every item it loads t...

12. [Scenes as Addressables vs SceneManager. What's the difference?](https://www.reddit.com/r/Unity3D/comments/sehqml/scenes_as_addressables_vs_scenemanager_whats_the/) - What is the difference between marking a scene as addressable and using Addressables.LoadSceneAsync(...

13. [How to Setup An Object Pool (New Built In Method) | Unity Tutorial](https://www.youtube.com/watch?v=GMbMPLykFQU) - ... Scene Explanation 01:17 - Analyzing the FPS and the Profiler 02:18 - Showing Where The Instantia...

14. [Occlusion culling - Unity - Manual](https://docs.unity3d.com/6000.4/Documentation/Manual/OcclusionCulling.html) - Occlusion culling works best in Scenes where small, well-defined areas are clearly separated from on...

15. [How to Use Occlusion Culling in Unity — The Sneaky Way](https://www.gamedeveloper.com/design/how-to-use-occlusion-culling-in-unity-the-sneaky-way) - An occlusion portal is a box-sized component that you can mark as open or closed. If it's open, then...

16. [Getting daily dose of occlusion culling : r/Unity3D - Reddit](https://www.reddit.com/r/Unity3D/comments/1kfmrth/getting_daily_dose_of_occlusion_culling/) - It is a lot faster for what you are doing in theory, it will basically test against the portal (door...

17. [How To Use Occlusion Culling In Unity | Step by Step Tutorial | HDRP](https://www.youtube.com/watch?v=DRsGt4OFQvU) - Hi:) This time, i have prepared a tutorial for you, on how Occlusion Culling works in Unity and how ...

18. [Portals Part 2 – Rendering Portals in UD2 - Triangular Pixels](https://blog.triangularpixels.com/uncategorized/portals-part-2-rendering-portals-in-ud2/) - Render textures basically put a second camera in the world, and instead of drawing it to the screen,...

19. [Improving the performance of rendering multiple cameras to ... - Reddit](https://www.reddit.com/r/Unity3D/comments/135m0h2/improving_the_performance_of_rendering_multiple/) - I lose about 4ms of render time per camera, and I need to render up to six cameras. The render textu...

20. [Remaking the Portal Effect from Deaths Door! - YouTube](https://www.youtube.com/watch?v=Ao21vjOEts4) - In this gamedev breakdown, I'll show you how the portal effect from Death's Door can be remade in Un...

21. [Portals | Part 2 - Stencil-based Portals - Daniel Ilett](https://danielilett.com/2019-12-14-tut4-2-portal-rendering/) - Rendering the view from behind one portal on the surface of its paired portal requires a bunch of sp...

22. [Client synchronization mode | Netcode for GameObjects | 2.4.4](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.4/manual/basics/scenemanagement/client-synchronization-mode.html) - Additive client synchronization is mode similar to additive scene loading in that any scenes the cli...

23. [Timing considerations | Netcode for GameObjects ... - Unity - Manual](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.6/manual/basics/scenemanagement/timing-considerations.html) - Looking at the timeline diagram below, "Loading an Additive Scene", we can see that it includes a se...

24. [More support for scenes already loaded #2377 - GitHub](https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/issues/2377) - Try additively loading scenes (LoadSceneMode.Additive) as that allows you to load and unload individ...

25. [Multiple Additive Scenes - Mirror Networking - GitBook](https://mirror-networking.gitbook.io/docs/manual/examples/multiple-additive-scenes) - Open the Main scene in the Editor and make sure the Game Scene field in the MultiScene Network Manag...

26. [Additive Levels and Scene Interest Management Mirror Networking](https://www.youtube.com/watch?v=zLph1UohnMU) - In this Unity Mirror Networking tutorial, I will show you how to set up additive levels and scene in...

