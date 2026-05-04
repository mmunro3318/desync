# Performance Budgets for Room-Based Horror Games in Unity 6 URP
### Concrete Thresholds for 60fps on GTX 1060 / RTX 2060 — 2–4 Player LAN

***

## Executive Summary

At 60fps, every frame must complete in **≤16.67ms**. On a GTX 1060, a dark interior horror scene with flashlights and real-time shadows will typically spend 6–9ms on GPU rendering, 3–4ms on CPU render-thread preparation, 1–2ms on physics, and leave 3–4ms of headroom for gameplay logic and network tick. The budgets below are calibrated for that envelope. Exceeding any single budget does not automatically break the target, but exceeding two or more simultaneously will. Profile on real target hardware — developer machines with RTX 4090s will mask all of this.

***

## Frame Budget Allocation (GTX 1060, 1080p, 60fps)

| Subsystem | Budget | Hard Limit |
|---|---|---|
| **Total frame** | 16.67ms | 16.67ms |
| CPU main thread (gameplay, scripts, GC) | 4ms | 8ms |
| CPU render thread (SRP Batcher, draw prep) | 3ms | 5ms |
| GPU (geometry + lighting + shadows) | 7ms | 10ms |
| Post-processing pass | 1.5ms | 3ms |
| Physics (`FixedUpdate`) | 1ms | 2ms |
| Network tick (30Hz NGO tick) | 0.3ms | 0.5ms |
| **Remaining headroom** | **~1ms** | — |

The first profiling question is always **CPU-bound or GPU-bound?** Look at the Profiler's `Gfx.WaitForPresentOnGfxThread` marker. If it's consistently > 2ms, you're GPU-bound. If `BehaviourUpdate` or `PhysicsManager.FixedUpdate` are large, you're CPU-bound. The fix strategy is completely different for each.

***

## 1. Draw Call Budget (URP, Forward+)

### The SRP Batcher Reality Check

The SRP Batcher does not reduce *draw call count* — it reduces **CPU cost per draw call** by batching shader constant uploads. On mid-range PC at 1080p, the raw draw call count is rarely the bottleneck; it's shadow draw calls and per-light passes that crush the GPU.[^1]

A practical draw call budget for a single dark room on GTX 1060:

| Phase | Draw Call Target | Hard Limit |
|---|---|---|
| Opaque geometry (room + props) | ≤ 150 | 300 |
| Shadow casters (main + additional lights) | ≤ 200 | 400 |
| Transparent / alpha (dust, fog planes) | ≤ 30 | 60 |
| UI + post-process | ≤ 20 | 40 |
| **Total active scene draw calls** | **≤ 400** | **800** |

The shadow draw call column is the silent killer. Each shadow-casting light generates a full re-render of all shadow casters in its range. A point light counts as **six** shadow cube-face render passes — equivalent to six spot lights. With two flashlights (spot lights) and two atmospheric sconces (point lights), you're paying: 2 + (2 × 6) = 14 shadow passes × shadow caster geometry.[^2]

### Lighting Budget (URP Forward+ on GTX 1060)

URP Forward (legacy, non-Plus) caps additional lights at **8 per object**. Forward+ removes the per-object cap; the limit moves to **256 lights per camera** on PC.[^3][^4]

For a dark horror interior, the practical lighting budget is:

| Light Type | Max Shadow-Casters | Max Non-Shadow | Notes |
|---|---|---|---|
| Spot (flashlight) | 2 active | + 4 non-shadow | Player flashlights = 1 per player, max 4 |
| Point (ambient sconce/prop) | 0–2 | unlimited | Point light shadows = 6 shadow passes; avoid[^5] |
| Directional (moonlight through window) | 1 (cascaded) | n/a | 4 cascade max; each cascade is full re-render[^6] |
| Area (baked only) | n/a | n/a | URP has no real-time area lights |

**Red flag #1:** More than 3 active shadow-casting spot lights in a single room. At 30 Hz shadow update you can cheat by updating shadows at half-rate for distant lights.

**Red flag #2:** Using `ShadowResolution.VeryHigh` (4096) on additional lights. For flashlights, 512–1024 is sufficient and saves ~2ms/frame on a GTX 1060.[^7]

### The Forward+ Switch

If you need more than 6 real-time lights affecting objects per scene tile, switch the URP asset's rendering path to **Forward+**. This enables per-tile light culling so adjacent-room lights don't consume your per-object budget. Horror games benefit significantly because many small atmospheric lights (candles, monitors, cracks of light under doors) exist within the player's viewing range.[^4][^8]

***

## 2. Memory Budget Per Room

### Target Footprint

For a small indoor horror room (bathroom, study, kitchen — ≤ 200 m² floor area):

| Asset Category | Per-Room Budget | Notes |
|---|---|---|
| Textures (compressed, GPU) | 20–40 MB VRAM | 1K–2K textures, DXT5/BC7 compression |
| Meshes (CPU + GPU) | 5–10 MB | 10K–50K triangles typical for interior |
| Baked lightmap | 5–15 MB | 1 or 2 lightmap atlases at 1K resolution |
| Audio clips (loaded) | 2–5 MB | Room-specific ambient, FMOD streaming |
| Scripts / GC overhead | 1–3 MB | MonoBehaviour instance data |
| **Total per active room** | **33–73 MB** | — |
| **Total for 3 active rooms** (current + 2 adjacent) | **~100–220 MB** | What should be in memory at any moment |

With a 500MB game memory target for your full running game (OS, Unity runtime, persistent assets, audio), each room at 33–73 MB means you can comfortably support **4–6 rooms simultaneously** without exceeding VRAM on a 6GB GTX 1060.

**Red flag:** A single room scene that loads at > 150 MB is over-budget. Community examples of 500 MB per house indicate uncompressed PNG textures and no LODs. Unity's default texture compression reduces a 2048×2048 RGBA PNG from ~16 MB to ~2.7 MB (BC7). Always verify by checking `Memory Profiler → Textures` after a scene load.[^9]

### Texture Budget Rule

| Texture Type | Max Resolution | Compression |
|---|---|---|
| Wall / floor / ceiling (tiling) | 1024 × 1024 | BC1 (opaque) |
| Props with unique detail | 2048 × 2048 | BC7 |
| Normal maps | 1024 × 1024 | BC5 |
| Lightmap atlases | 1024 × 1024 per atlas | BC6H |
| Render textures (portals, mirrors) | 512 × 512 | — |

***

## 3. Room Load/Unload Latency

### The Horror Game Immersion Constraint

A visible hitch > 33ms (2 dropped frames at 60fps) is perceivable as a stutter and breaks horror immersion. An invisible hitch is ≤ 16ms — achievable only by pre-loading.

| Approach | Latency | Hitch Risk | Recommended Use |
|---|---|---|---|
| Pre-load adjacent rooms (renderer disabled) | 0ms for player | None | **Primary strategy** |
| `allowSceneActivation = false` + deferred enable | 1–16ms per frame over many frames | Low | Activation mitigation |
| Additive scene load (no pre-warm) | 1–5s off-thread + 1-frame hitch | High (activation spike) | Avoid without pre-load |
| Fade-to-black then load | Hitch masked during fade | None (masked) | Fallback for mutations |

**Target:** At 8–15 rooms, pre-load all rooms at startup, keep them loaded but renderer-disabled. The "load time" the player experiences is zero because everything is already in memory. Unload only when rooms become permanently inaccessible to all players.

At 50+ rooms, switch to Addressables streaming with a **2-hop pre-load radius**: the player's current room and all rooms within 2 graph hops are loaded; rooms 3+ hops out are unloaded. This means a load event triggers when the player reaches hop 1, giving 1–2 seconds of pre-load window before they can physically reach hop 2.

**Hiding unavoidable loads:**
- Door slam / lock animation (3 seconds of visual cover)
- Power outage / blackout effect (~1 second)
- Player crouch-squeeze through narrow passage (constrained movement = controlled timing)

***

## 4. NetworkObject Budget (NGO 2.11.x)

### Bandwidth Model

NGO's `NetworkTransform` at the default 30 tick rate sends ~800 bytes/second per actively moving object at full-float precision. Enabling half-float precision (sufficient for human-scale movement) cuts this to ~400 bytes/second. Enabling threshold-based delta sync (only sends when position changes by > N units) cuts inactive objects to near zero.[^10]

For a 2–4 player horror game on LAN:

| Object Category | Count | Bandwidth (active) | Bandwidth (idle) |
|---|---|---|---|
| Player characters (`NetworkTransform` + animator) | 4 max | ~1.2 KB/s each | ~50 bytes/s |
| Door interactables (open/close `NetworkVariable`) | 5–10 per room, 3 active rooms | ~30 bytes/s each (event-only) | 0 |
| Trap triggers / pickup state | 10–20 per active area | ~10 bytes/s (event) | 0 |
| Room graph edges (`NetworkList<RoomEdge>`) | 1 list, 50 edges max | ~16 bytes per mutation | 0 |
| **LAN total (4 players, 30 active objects)** | — | **~8 KB/s** | — |
| **Internet total at same scale** | — | **~15 KB/s** | — |

**Red flag — LAN:** Total server outbound > 100 KB/s. This typically means runaway `NetworkTransform` updates or a `NetworkVariable` updating every frame (e.g., syncing a float that changes continuously with no dirty-check threshold).

**Red flag — Internet:** Latency > 100ms will make NGO's `NetworkTransform` interpolation visually stutter. Set `NetworkTransform.Interpolate = true` on all player transforms. NGO 2.x uses linear interpolation by default; for horror characters you may want to add a custom `SmoothedNetworkTransform` wrapper that uses a small buffer (3–4 ticks of lag) for smoother extrapolation under jitter.

### Hard NetworkObject Limits

NGO has no hard limit, but a 700+ object simultaneous spawn overflows Unity Transport's default buffer and silently fails. For a horror game at LAN scale (2–4 players, 8–15 rooms), you will have at most 200–300 `NetworkObject`s — well inside the safe zone. Stagger spawns at > 100 per burst as a protective habit.[^11]

***

## 5. Physics Budget

### Collider Categories

| Collider Type | Performance Cost | Max Before Bottleneck |
|---|---|---|
| Static MeshCollider (complex) | Medium — rebuild on any transform change[^12] | 500 per scene |
| Static BoxCollider / CapsuleCollider | Very low | 2000+ per scene |
| Kinematic Rigidbody + collider (door, prop) | Low | 200 per active room |
| Dynamic Rigidbody (physics props) | High per body | 30 per active room |
| CharacterController (each player) | Medium (2 PhysX queries/step) | 8 players |

For a horror room, use **compound primitive colliders** (box + box + capsule) instead of `MeshCollider` wherever the geometry allows. `MeshCollider` is appropriate only for irregular organic shapes (a spiral staircase, a collapsed wall). Tests show that sometimes a well-crafted mesh collider can outperform many individual box colliders, but this is geometry-dependent — profile your specific rooms.[^13]

**Absolutely critical:** Never move a static `MeshCollider` at runtime — even once per room connection event. Moving it forces PhysX to rebuild the internal BVH tree (broad-phase spatial structure), which can spike 5–10ms. Use `Kinematic Rigidbody` for anything that needs to move.[^12]

**Red flag:** `Physics.Simulate` or `FixedUpdate` taking > 2ms. Use the Profiler's Physics module to identify the culprit — usually a mesh collider on a moving object.

### Disable Physics on Unloaded Rooms

When a room's renderers are disabled (but scene still loaded), also:
1. `Collider.enabled = false` on all non-critical room colliders.
2. `Rigidbody.Sleep()` on all in-room physics props.
3. Keep only **trigger volumes** (portal zones, player detection volumes) active.

This ensures PhysX's broadphase only resolves collisions for the player's current room and immediate adjacents.

***

## 6. Profiling Methodology

### Priority Order for Room Streaming Investigation

Use the Unity 6 Profiler (`Window > Analysis > Profiler`) with the **Timeline view**, not the Hierarchy view — Timeline shows the actual work distribution across CPU threads and GPU.

**Step 1 — Find the bottleneck type (CPU vs GPU)**
Look for `Gfx.WaitForPresentOnGfxThread` in the main thread row. If this marker is > 4ms, you are GPU-bound. If the main thread is fully saturated with no `WaitForPresent` gaps, you are CPU-bound.

**Step 2 — For GPU-bound: open Frame Debugger**
`Window > Analysis > Frame Debugger`. Expand `Draw Opaques > Shadow Map` passes. Count shadow caster draw calls per light. If any light spawns > 150 shadow draw calls, that light is the culprit.

**Step 3 — For CPU-bound: watch these markers**

| Marker | Red Flag Threshold | Cause |
|---|---|---|
| `BehaviourUpdate` | > 2ms | Too many MonoBehaviour `Update()` calls |
| `PhysicsManager.FixedUpdate` | > 2ms | Too many dynamic rigidbodies or mesh collider rebuilds |
| `GC.Collect` | Any occurrence > 0.5ms | Managed allocation per frame (string ops, LINQ, `new` in Update) |
| `Loading.UpdatePreloading` | > 1ms steady | Scene or Addressable load in progress |
| `Canvas.SendWillRenderCanvases` | > 1ms | UI rebuilding every frame |

**Step 4 — Memory: use Memory Profiler package separately**
`Window > Analysis > Memory Profiler`. Take a snapshot after loading all active rooms. In **TreeMap** view, sort by size. Categories to watch:

| Memory Category | Target | Red Flag |
|---|---|---|
| Textures (GPU) | < 300 MB total | > 500 MB |
| Meshes | < 50 MB | > 100 MB |
| Managed Heap (GC) | < 50 MB | > 100 MB |
| Native / Untracked (leaked scenes) | Should drop after `UnloadSceneAsync` | Staircase pattern = missing `Release()` |
| RenderTextures | < 50 MB | > 100 MB |

**The staircase pattern** — take three consecutive snapshots (before load, after load, after unload). If the "after unload" snapshot does not return to near the "before load" value, you have a memory leak: either a missing `Addressables.Release()` call, a dangling Addressable handle, or a `static` reference keeping assets alive.[^14]

**Step 5 — Network: NGO Network Profiler**
`Window > Analysis > Profiler > Network (NGO)`. This module (added in NGO 1.1+) shows per-`NetworkObject` bytes sent/received per tick.[^15]

| Network Metric | Target (LAN) | Red Flag |
|---|---|---|
| Total server outbound (per tick) | < 3 KB/tick at 30Hz = < 90 KB/s | > 100 KB/s |
| Per-player `NetworkTransform` bytes/s | < 1.5 KB/s | > 3 KB/s |
| `NetworkList` update size | < 50 bytes per mutation | > 200 bytes (struct too large) |
| Messages dropped | 0 | Any dropped = buffer overflow |

***

## 7. URP Dark Scene Performance Traps

### Trap 1: Volumetric Fog Cost

URP's built-in volumetric fog (added in Unity 6 URP via `Full Screen Pass Renderer Feature` or third-party) is a **per-pixel ray march** operation. Step count controls quality vs. performance. At 32 ray march steps (default), volumetric fog costs 2–4ms on a GTX 1060 at 1080p. Reduce to 16 steps for indoor horror — the difference is imperceptible in dark corridors. A Unity 6.4 URP interior study shows the effects are achievable at reasonable cost with proper step-size tuning.[^16][^17]

**Do not use full-screen volumetric fog.** Scope it to `LocalVolumetricFog` volumes placed only inside rooms that need it. An entire-scene volumetric fog effect in a 15-room house would march rays through geometry that the player can't see.

### Trap 2: Shadow Cascade Misconfiguration

URP supports 1–4 shadow cascades for the main directional light. Each cascade performs a full shadow map render of all shadow casters in its range. For an indoor horror game with no outdoor directional light visible, **disable the main directional light entirely** and rely on flashlights + baked ambient. If you need a directional light (moonlight through windows), use 2 cascades maximum, set `Max Distance` to match your largest room dimension (~15–20m), not the Unity default of 50m.[^18][^6]

### Trap 3: Post-Processing Stack Cost

| Effect | Cost (GTX 1060, 1080p) | Recommendation |
|---|---|---|
| Bloom (URP default) | ~0.8ms | Reduce `Scatter` to 0.5; use `High Quality` off |
| Depth of Field (Bokeh) | ~2ms | Use `Gaussian` mode, not Bokeh |
| Screen Space Ambient Occlusion | ~1.5ms | SSAO for a dark horror game — likely redundant with baked AO |
| Color Grading (LUT) | ~0.2ms | Negligible; keep |
| Vignette | ~0.05ms | Negligible; keep |
| Film Grain | ~0.1ms | Negligible; keep |
| Chromatic Aberration | ~0.05ms | Negligible; keep |
| Motion Blur | ~1ms | Avoid unless cinematic sequences |

**Red flag:** Post-processing exceeding 3ms total. SSAO + Bokeh DoF + Bloom simultaneously on a GTX 1060 will consume 4ms — 25% of your entire frame budget.

### Trap 4: Reflection Probes in Every Room

A `ReflectionProbe` set to **Real-Time** mode renders the entire scene from its position every frame — an additional full render pass at the probe's resolution. For dark indoor rooms, bake reflection probes at authoring time and set them to **Baked** mode. Reserve real-time probes only for surfaces with moving reflections (active monitors, water). One real-time reflection probe per room = one extra full render per frame.

### Trap 5: Transparent Object Overdraw

Horror games love particle smoke, fog planes, and ghost effects — all transparent. Transparent objects cannot be batched by the SRP Batcher and render back-to-front, meaning each pixel may be shaded multiple times (overdraw). In a corridor with smoke, dust, and a light shaft, overdraw can reach 8× — 8 fragment shaders running per pixel. Keep transparent objects' alpha coverage tight and use billboard quads rather than thick particle volumes. Overdraw is GPU fill-rate bound and will hit a GTX 1060 (fill rate: ~73 GPixels/s) faster than a modern card.

***

## Quick-Reference Threshold Table

| System | Target | ⚠️ Warning | 🚨 Red Flag |
|---|---|---|---|
| Draw calls (total, SRP batched) | < 400 | 600 | > 800 |
| Shadow-casting lights active | ≤ 3 spot | 4 spot | ≥ 2 point shadow-casters |
| Shadow resolution (additional) | 512 | 1024 | 4096 |
| VRAM per active room set | < 200 MB | 350 MB | > 500 MB |
| Room load time (foreground) | Invisible (pre-loaded) | < 1s | > 2s visible |
| Active dynamic rigidbodies | < 30 | 50 | > 100 |
| Active mesh colliders (moving) | 0 | 1–2 | > 5 |
| NetworkObjects total | < 200 | 400 | > 700 |
| Network bandwidth (server out) | < 50 KB/s LAN | 80 KB/s | > 100 KB/s |
| GC.Collect per frame | 0 | Occasional | Every frame |
| Post-processing total | < 2ms | 3ms | > 4ms |
| Volumetric fog march steps | 16 | 24 | 32 (full screen) |
| CPU BehaviourUpdate | < 2ms | 3ms | > 4ms |
| GPU total frame | < 8ms | 12ms | > 14ms |

---

## References

1. [Unity Draw Call Batching: The Ultimate Guide (2026 Update)](https://thegamedev.guru/unity-performance/draw-call-optimization/) - Draw calls are never a problem. That is, until you add one more element and suddenly your render thr...

2. [Any tips to improve performance of having a lot of dynamic ... - Reddit](https://www.reddit.com/r/Unity3D/comments/1ingycj/any_tips_to_improve_performance_of_having_a_lot/) - The lights have shadows off ( too much performance cost ) They are ... Wait... why can Unity URP han...

3. [Wait... why can Unity URP handle this many real-time lights? - Reddit](https://www.reddit.com/r/Unity3D/comments/1okyhw1/wait_why_can_unity_urp_handle_this_many_realtime/) - URP also supports deferred. ... Rendering path is forward+. ... Forward+ has no limitation on number...

4. [Forward+ (Plus) Rendering in Unity URP 14+ - Shine Your Lights!](https://thegamedev.guru/unity-gpu-performance/forward-plus/) - With forward rendering, performance is really good when you have a little amount of lights, but when...

5. [Optimize shadow rendering in URP - Unity - Manual](https://docs.unity3d.com/6000.3/Documentation/Manual/shadows-optimization.html) - Optimize shadow rendering in URP. This page describes optimization techniques that help you make sha...

6. [URP features | Universal RP | 17.0.0](https://docs.unity.cn/Packages/com.unity.render-pipelines.universal@17.0/manual/urp-feature-list.html) - URP supports up to 4 shadow cascades. To access the property, navigate to URP Asset > Shadows > Casc...

7. [LOD Your Lights - Performance Optimization | Unity Tutorial - YouTube](https://www.youtube.com/watch?v=4uFKwR0SrZo) - One of the most popular optimization techniques is LOD, but rarely does anyone talk about LODing the...

8. [Forward and Forward+ rendering paths in URP - Unity - Manual](https://docs.unity3d.com/6000.1/Documentation/Manual/urp/rendering/forward-rendering-paths.html) - Using the Forward+ rendering path reduces the number of lights Unity calculates for each GameObject....

9. [My game contains houses that are about 500 mebabytes each in ...](https://www.reddit.com/r/Unity3D/comments/13hgkzz/my_game_contains_houses_that_are_about_500/) - Currently I have a game with a lot of houses, each house takes up about 500mb including the textures...

10. [NetworkTransform | Netcode for GameObjects | 2.4.4 - Unity - Manual](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.4/manual/components/networktransform.html) - For objects with one or more child objects, should you synchronize world or local space axis values?...

11. [Spawning too many network objects at once #1570 - GitHub](https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/issues/1570) - If the Prefab contains only a NetworkObject, it works for up to 700 objects (750 does not). If the P...

12. [Move static colliders to prevent performance issues - Unity - Manual](https://docs.unity3d.com/2022.3/Documentation/Manual/physics-optimization-cpu-static-colliders.html) - If you want to move a static collider, the recommended best practice is that you don't add a Rigidbo...

13. [#gamedev tip: Simple colliders tend to be much more efficient ...](https://www.reddit.com/r/Unity3D/comments/1d2b6dw/gamedev_tip_simple_colliders_tend_to_be_much_more/) - You can often get better collision performance out of using several simple collider shapes than one ...

14. [How to use Unity's memory profiling tools](https://unity.com/how-to/use-memory-profiling-unity) - This page provides information on two tools for analyzing memory usage in your application in Unity:...

15. [NEW Unity Multiplayer Tools Are Here - LEARN XR BLOG](https://blog.learnxr.io/unity-development/new-unity-multiplayer-features) - Network Profiler: (For NGO) which allows you to see a detailed breakdown of network objects and mess...

16. [Your first Volumetric Fog Shader | Unity URP - YouTube](https://www.youtube.com/watch?v=8P338C9vYEE) - A gentle introduction into creating a volumetric fog shader from scratch, explaining the overall log...

17. [Unity 6.4 URP Interior Lighting & Volumetric Fog Study - YouTube](https://www.youtube.com/watch?v=YWrSaMMLeIw) - Unity 6.4 URP Interior Environment Study □ Built with custom URP template (URPLabStudio) □ Volumetri...

18. [Target 60fps Unity URP Performance Optimisation & Analysis for ...](https://www.youtube.com/watch?v=FjcNSjf7tvw) - Speaker: Srinivas Veeraraghavan, Co-Founder, at Dot9 Games #IGDC #IGDC2022 #IndiaGameDeveloperConfer...

