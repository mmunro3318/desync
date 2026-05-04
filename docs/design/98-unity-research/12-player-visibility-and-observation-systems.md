# Player Visibility and Observation Systems

**To inform _Observation Lock_ mechanics**

## Overview

The core challenge is that "observed" must mean *semantically* observed (player consciously looking at a space through open geometry), not just *technically* in-frustum. A fast layered system — frustum check → occlusion check → optional portal propagation — is the right architecture, and it should output a per-room `bool isObserved` with grace timer smoothing.

***

## 1. Frustum-Based Visibility

`GeometryUtility.CalculateFrustumPlanes` extracts the 6 planes of the camera's view volume at runtime, and `GeometryUtility.TestPlanesAABB` tests whether an AABB intersects those planes. This is your cheapest first-pass filter — if a room's bounding box is fully outside all 6 planes, it's definitely not observed. [docs.unity3d](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/GeometryUtility.TestPlanesAABB.html)

**Critical caveat:** the test is conservative and produces **false positives**. A large bounding box near a frustum edge or corner can pass the test even when not truly visible. This is fine for your use case — false positives just mean you're slightly *over-protecting* a room from mutation, which is the safe failure mode. [docs.unity3d](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/GeometryUtility.TestPlanesAABB.html)

**Even bigger caveat:** `OnBecameVisible` / `OnBecameInvisible` are **not suitable** for gameplay logic. They fire for any camera — including the Unity Editor scene-view camera — and also fire when an object's shadow enters a frustum. Experienced devs explicitly call these a "huge footgun" for gameplay systems. Use explicit frustum plane checks instead. [reddit](https://www.reddit.com/r/Unity3D/comments/12yfc1m/onbecamevisible_invisible_methods_to_detect_when/)

```csharp
// Cache and recalculate once per frame at the top of your ObservationManager
Plane[] _frustumPlanes = new Plane [devforum.roblox](https://devforum.roblox.com/t/how-would-i-go-about-scripting-scp-173/2450931);

void UpdateFrustumPlanes() {
    GeometryUtility.CalculateFrustumPlanes(Camera.main, _frustumPlanes);
}

bool IsRoomInFrustum(Room room) {
    return GeometryUtility.TestPlanesAABB(_frustumPlanes, room.Bounds);
}
```

For a tighter check you can also narrow the FOV slightly (e.g., use 60° instead of the actual 90° FOV) when constructing a custom frustum for "attentive" observation — rooms at the very periphery of vision arguably shouldn't count as "observed."

***

## 2. Occlusion-Based Visibility

Frustum says "could be seen"; occlusion says "actually is seen." A room behind a closed door passes the frustum test but is fully blocked — you need a second stage.

**Option A — Raycast confirmation:** Cast 1–5 rays from the camera to sample points inside the room (door frame center, room center, corners). If *any* ray reaches an unobstructed point inside the room, it counts as observed. This is the same approach used for SCP-173 AI visibility logic and Weeping Angel implementations in Unity: combine a dot-product direction check with a `Physics.Raycast` LOS test. [youtube](https://www.youtube.com/watch?v=925eDNJuwvI)

```csharp
bool IsRoomOcclusionVisible(Room room, Transform cam) {
    Vector3[] samplePoints = room.GetSamplePoints(); // 3-5 points
    foreach (var point in samplePoints) {
        Vector3 dir = point - cam.position;
        if (!Physics.Raycast(cam.position, dir.normalized, dir.magnitude, 
                             LayerMask.GetMask("Walls", "Doors"))) {
            return true; // at least one unobstructed path
        }
    }
    return false;
}
```

**Option B — Door-state shortcut:** Maintain a `bool isDoorOpen` flag per portal. If the connecting door is closed, immediately return `false` without raycasting. This eliminates the most common case (closed-door occlusion) for near-zero cost.

**Unity's built-in Occlusion Culling** (`Window → Rendering → Occlusion Culling`) bakes static PVS data that the renderer uses automatically, but it's **render-only** — there's no runtime query API that returns "is room X occluded from the camera?". Unity's occlusion culling also only works for static objects. For dynamic gameplay visibility, you must implement your own raycast-based approach. [dev](https://dev.to/dumboprogrammer/50-tips-for-big-unity-games-1lpa)

***

## 3. Portal-Based Visibility

The portal visibility algorithm clips the view frustum recursively through openings. The conceptual model: [blog.selfshadow](https://blog.selfshadow.com/publications/practical-visibility/)

1. Start with the camera frustum in the player's current room.
2. For each portal (doorway) in that room, check if the portal's quad intersects the active frustum.
3. If it does, clip the frustum to the portal's opening and recurse into the adjacent room with that sub-frustum.
4. A room is "observed" if it was reached by any recursive step. [en.wikipedia](https://en.wikipedia.org/wiki/Portal_rendering)

This is the approach used by Quake/Half-Life's portal leaf BSP systems — the PVS in those engines doubles as both the renderer's culling data *and* the netcode's "what entities to send to this client" logic. [reddit](https://www.reddit.com/r/programming/comments/133vwr9/quakes_visibility_culling_explained/)

**For a horror game with ~10–30 rooms, a simplified version works well:**

```csharp
// Call once per frame, seed with player's current room
void PropagateVisibility(Room room, Plane[] activeFrustum, HashSet<Room> visited) {
    if (visited.Contains(room)) return;
    visited.Add(room);
    room.IsObserved = true;

    foreach (Portal portal in room.Portals) {
        if (!portal.IsOpen) continue;
        if (!GeometryUtility.TestPlanesAABB(activeFrustum, portal.Bounds)) continue;
        
        // Clip frustum to portal opening (simplified: use portal AABB as sub-frustum)
        Plane[] clippedFrustum = ClipFrustumToPortal(activeFrustum, portal);
        PropagateVisibility(portal.AdjacentRoom, clippedFrustum, visited);
    }
}
```

Full frustum clipping math is non-trivial, but for tight doorways you can approximate: compute the min/max screen-space extents of the doorframe quad, create new near/far plane pairs from them, and pass those into the recursive call. At the scale of a horror game (doors are narrow), this approximation is tight enough. [playtechs.blogspot](http://playtechs.blogspot.com/2007/03/2d-portal-visibility-part-1.html)

**Precomputed PVS** (like Quake's) pre-bakes cell-to-cell visibility offline. The runtime cost is just a lookup: "given I'm in cell X, which cells are *potentially* visible?". For a horror game with procedurally mutating rooms this is problematic — your geometry changes break the baked data. Use runtime portal traversal instead. [en.wikipedia](https://en.wikipedia.org/wiki/Potentially_visible_set)

***

## 4. The "Observation" Problem in Horror Games

SCP: Containment Breach's SCP-173 is the canonical example: the creature can only move when *no* player has direct line-of-sight to it. The implementation strategy, reconstructed from community dev forums  and Unity/UE5 tutorials: [youtube](https://www.youtube.com/watch?v=FgcCxTF2Xus)

**Three-stage observation check (per threat/room, per frame):**

| Stage | Method | Cost | Purpose |
|---|---|---|---|
| 1. Frustum broad phase | `CalculateFrustumPlanes` + `TestPlanesAABB` | ~0.01ms | Eliminate rooms clearly behind player |
| 2. Direction dot product | `Vector3.Dot(camForward, dirToRoom)` > threshold | ~0.001ms | Check if player is facing room |
| 3. LOS raycast | `Physics.Raycast` from camera to room samples | ~0.1–0.5ms | Confirm nothing blocks sightline |

The **dot product check** is the hidden gem: compute the angle between the camera's forward vector and the direction to the room center. If `dot < cos(FOV/2)`, skip raycasting entirely. This makes it spatially coherent with the frustum but costs only a multiply-add. [devforum.roblox](https://devforum.roblox.com/t/how-would-i-go-about-scripting-scp-173/2450931)

**Weeping Angel implementations** in UE5 Blueprints use the same logic: check if the enemy actor is within frustum range, then fire a ray to check for obstacles between player and entity. The "frozen while observed" state then drives the AI's movement permission flag. [youtube](https://www.youtube.com/watch?v=kiLugdYxazs)

**Heuristics used by actual horror games:**
- **Partial observation** (the door is 10% open): treat the portal as observed if the door AABB overlaps the frustum *and* at least one sample ray makes it through the opening
- **Looking at a wall near a room** does *not* count — only the room's interior sample points pass the raycast
- **Distance falloff**: some games (like SCP-173) only freeze the entity when the player is within a certain range and has LOS — this prevents "trivially safe" long-corridor observation locking the entire level

***

## 5. Multiplayer Observation (2–4 Players)

The observed state for any room is the **union** of all players' visibility sets: a room is locked if *any* player observes it.

**Architecture:**

The server should be authoritative on observation state. Each client calculates its own local visibility set and sends it to the server (or: the server independently calculates per-player visibility using server-side data). The server merges them with OR logic. [reddit](https://www.reddit.com/r/gamedesign/comments/ijvupi/server_authoritative_in_fps_multiplayer_how_to/)

```
Server tick:
  observedSet = {}
  for each player P:
      localVis = ComputeVisibility(P.cameraPos, P.cameraForward)
      observedSet = observedSet UNION localVis
  
  for each room R:
      R.canMutate = (R not in observedSet)
```

**Anti-cheat consideration:** If a player can spoof their camera direction to always look at threat rooms, they trivially prevent mutations. Options:
- Server computes visibility independently (requires server-side scene representation)
- Accept client-reported frustum data but rate-limit mutation checks (a spoofer can only prevent mutations in rooms they claim to watch, which doesn't break anything game-mechanically)

**Network efficiency:** Don't send per-pixel visibility. Send a `uint32` bitmask of room IDs (up to 32 rooms) per player per tick. For 4 players at 20 Hz, that's 4 × 4 bytes × 20 = 320 bytes/sec — trivially cheap. State sync via Unity's `NetworkVariable<uint>` in Netcode for GameObjects works perfectly here. [docs.unity3d](https://docs.unity3d.com/540/Documentation/Manual/net-HighLevelOverview.html)

***

## 6. Grace Timers and Hysteresis

Without smoothing, a player's natural head-bobbing or micro-saccades can break observation for 1–3 frames — enough to trigger a mutation that immediately snaps back when re-observed, creating visual flickering.

**Two-sided hysteresis pattern:**

```csharp
class RoomObservationState {
    float observedTimer = 0f;
    float unobservedTimer = 0f;
    bool lockedObserved = false;
    
    const float ENTER_GRACE = 0.0f;   // Instantly lock when observed
    const float EXIT_GRACE  = 1.5f;  // 1.5s after losing LOS before allowing mutation
    
    void Update(bool rawObserved) {
        if (rawObserved) {
            unobservedTimer = 0f;
            lockedObserved = true;   // Immediately lock
        } else {
            unobservedTimer += Time.deltaTime;
            if (unobservedTimer >= EXIT_GRACE) {
                lockedObserved = false;  // Allow mutation after grace
            }
        }
    }
}
```

**Recommended values for horror pacing:**

| Scenario | Exit Grace | Rationale |
|---|---|---|
| Player walks through doorway | 0.3–0.5s | Brief loss of LOS during traversal shouldn't trigger mutations |
| Player looks away intentionally | 1.5–3.0s | Creates tension — the world *wants* to change but waits |
| Player turns around completely | 3.0–5.0s | The "I heard something behind me" moment — the pause before turning back |
| Multiplayer, all players look away | 2.0s | Longer grace compensates for uncoordinated looking |

The **entry side** should have zero hysteresis (or very small, ~1 frame): the instant you look at a room, it locks. Asymmetric hysteresis is the key insight — *locking* is instant, *unlocking* is delayed. This creates the horror mechanic: you can protect a space immediately, but you have to keep watching to maintain that protection.

A **mutation cooldown** at the room level (e.g., a room can only mutate once every 30s regardless of observation state) prevents the "quantum flickering" problem where a rapidly-observing player causes a room to oscillate. This is separate from the grace timer.

***

## 7. Performance Budget

At 60fps, your total frame budget is 16.66ms. Your visibility system should consume no more than **0.5–1.0ms** per frame, leaving room for rendering, physics, AI, and audio. [unity](https://unity.com/how-to/best-practices-for-profiling-game-performance)

**Per-operation cost estimates (Unity, single-threaded):**

| Operation | Cost | Notes |
|---|---|---|
| `CalculateFrustumPlanes` | ~0.005ms | Call once per camera per frame |
| `TestPlanesAABB` | ~0.001ms | Per room, very cheap |
| `Vector3.Dot` direction check | ~0.0001ms | Nearly free |
| `Physics.Raycast` (single) | ~0.05–0.3ms | Depends on scene complexity |
| `Physics.RaycastNonAlloc` | ~0.03–0.2ms | Avoids GC allocation |

**Scaling math:** With 30 rooms, 3 rays per room, 4 players → 360 raycasts/frame. At ~0.15ms each = **54ms** — way over budget on the naive approach.

**Optimizations in order of impact:**

1. **Stage gate:** Only raycast rooms that pass *both* the frustum check AND the dot product test. In practice, ~70–80% of rooms are eliminated before hitting raycasts.

2. **Spatial locality:** Players can only meaningfully observe rooms within ~20m (limited render distance + horror fog). Distance-cull rooms beyond that radius entirely.

3. **Stagger updates:** Don't check all rooms every frame. Check 1/3 of rooms per frame on a round-robin schedule. With a 0.3s grace timer, missing 2 frames (~33ms) is invisible. [youtube](https://www.youtube.com/watch?v=dHLNqbKrJdg)

4. **Unity Jobs System:** Batch all raycasts into a `IJobParallelFor` using `Physics.RaycastCommand` (non-allocating). Schedule on frame N, complete on frame N+1. This shifts the work off the main thread. [youtube](https://www.youtube.com/watch?v=dHLNqbKrJdg)

```csharp
// Schedule in frame N
var commands = new NativeArray<RaycastCommand>(roomSamples, Allocator.TempJob);
var results  = new NativeArray<RaycastHit>(roomSamples, Allocator.TempJob);
_raycastJobHandle = RaycastCommand.ScheduleBatch(commands, results, 32);

// Complete in frame N+1 LateUpdate
_raycastJobHandle.Complete();
ProcessVisibilityResults(results);
```

5. **Portal short-circuit:** For rooms behind closed doors, skip *all* raycasts. This is often the single biggest win in a corridor horror game — most rooms at any moment are behind closed doors.

**Practical node count ceiling:**
- With staggering + portal short-circuits + Jobs: **50–100 rooms** is comfortable at 60fps on mid-range hardware
- Without any optimization (naive loop): **~10 rooms** before you're over budget
- The visibility system in a game like Alien: Isolation (which has complex per-room state tracking) runs its spatial queries at a sub-frame tick rate (10–20Hz) and interpolates between results [en.wikipedia](https://en.wikipedia.org/wiki/Potentially_visible_set)

**Debuggability tip:** Draw your current observation state using `Gizmos.DrawWireCube` on each room's bounds, colored green (observed) or red (unobserved), in `OnDrawGizmos`. This lets you visually validate your system while playtesting at no runtime cost.