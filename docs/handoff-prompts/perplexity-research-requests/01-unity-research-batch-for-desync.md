# Perplexity Deep Research Prompts — DESYNC Development

## How to Use This Document

This document contains research prompts organized into **batches**. Each batch is a coherent research thread.

- **Sequential prompts** within a batch should be fed in order in the **same Perplexity thread** (they build on prior context/results).
- **Separate batches** should be run in **new/clean threads** (independent topics, no shared context needed).
- Feed one prompt at a time. Wait for the full report before feeding the next.
- Save each report as a markdown file in `docs/design/98-unity-research/` with the naming convention: `07-<topic-slug>.md`, `08-<topic-slug>.md`, etc. (continuing from existing numbering).

## Priority Key

- **P0 (Critical):** Blocks S0.1/S0.2, must be done before M1 begins
- **P1 (High):** Blocks or significantly de-risks M1 sprints
- **P2 (Medium):** De-risks M2/M3, can run in parallel with M1 development
- **P3 (Low):** Post-POC concerns, run when idle

---

## Batch A: URP Lighting & Graphics Debugging (P0)

> Run in a single thread. Sequential prompts that build context.

### A1 — URP Lighting Architecture in Unity 6

```
I'm developing a first-person horror game in Unity 6 with URP 17.x. The game features a multi-floor interior house where players explore with flashlights in near-darkness.

I need a comprehensive technical deep dive on how URP handles lighting in enclosed interior spaces:

1. How do real-time shadows work in URP? Specifically: shadow cascades, shadow distance, shadow bias/normal bias, and how these interact with enclosed spaces vs open worlds.

2. How does light "containment" work? In a multi-floor house, what prevents a point light or spot light on floor 1 from bleeding through geometry to floor 2? What are the common failure modes?

3. Light Layers in URP — how do they work, what are the limitations, and when should they be used for floor/room isolation?

4. Light Probes vs Adaptive Probe Volumes in Unity 6 URP — which is recommended for interior horror, and how do they handle floor separation?

5. Volume Profiles and Volume overrides — how do local volumes interact with global volumes, and what happens at boundaries?

6. Common URP interior lighting bugs and their fixes: light leak through walls, shadow acne on thin geometry, peter-panning on floors/ceilings, banding in dark scenes.

Please cite official Unity 6 documentation and community solutions where possible. I need actionable debugging steps, not just conceptual overviews.
```

### A2 — Debugging a Specific Floor-to-Floor Light Leak

```
Building on the URP lighting context: I have a specific bug in my Unity 6 URP project.

**The problem:** Light from floor 1 of a two-story house visibly bleeds through the ceiling/floor geometry to floor 2. The house is a modular graybox (separate mesh pieces for walls, floors, ceilings, assembled in-editor).

**What I need:**
1. A systematic debugging checklist for diagnosing floor-to-floor light leaks in URP modular architecture. Walk me through each possible cause in order of likelihood.

2. For each cause, explain: what it looks like visually, how to confirm it's the culprit, and the correct fix.

3. Specific URP settings and their impact: Light → Shadow settings, URP Asset → Shadows panel, individual light shadow bias values, mesh renderer shadow casting modes.

4. How does mesh construction affect light leakage? (gaps in modular pieces, single-sided vs double-sided geometry, backface culling, normal direction)

5. What role do Rendering Layers play in preventing cross-floor light contamination, and how do I set them up?

6. For a horror game where darkness is critical: what's the recommended shadow resolution, cascade count, and shadow distance for a small interior (20m x 20m footprint, 2 floors)?

I want to fix this properly, not mask it with shadow planes or overlapping geometry hacks.
```

---

## Batch B: Runtime Geometry Loading & Room-Based Streaming (P0)

> Run in a single thread. Sequential prompts building toward our house graph architecture.

### B1 — Room-Based Level Streaming in Unity 6

```
I'm building a first-person horror game in Unity 6 where the house layout is represented as a graph of room nodes connected by portal edges. The graph can mutate at runtime (rooms disconnect, reconnect to different rooms, new rooms get inserted).

I need a comprehensive technical analysis of how to implement room-based streaming/loading in Unity 6:

1. **Additive Scene Loading** — how does LoadSceneAsync(Additive) work? Can individual rooms be separate scenes loaded/unloaded independently? What are the performance characteristics (load time, memory spike, frame hitch)?

2. **Prefab Instantiation** — rooms as prefabs instantiated/destroyed at runtime. Performance vs additive scenes? Memory patterns? Can prefabs be loaded from Addressables for async loading?

3. **Addressables System** — how does Unity's Addressables package handle runtime asset loading? Load/unload lifecycle, memory management, reference counting, async patterns.

4. **Object Pooling** — for a game where rooms cycle (same room types reappear), should room geometry be pooled rather than destroyed/reinstantiated?

5. **Occlusion and Culling** — if only occupied + adjacent rooms should be rendered, what's the best approach? Occlusion Culling (baked), runtime occlusion (camera-based), or manual activation (enable/disable renderers)?

6. **The Portal Problem** — when looking through a doorway from Room A into Room B, Room B must be visible. How do existing games handle this? (Portal rendering, stencil buffer tricks, camera tricks, or just "load adjacent rooms always"?)

For each approach, I need: memory cost, CPU cost per frame, load latency, and suitability for a multiplayer game where the server must track which rooms are active for all players.

My house is small for MVP (~8-15 rooms), but may grow to 50+ rooms post-MVP. I need an approach that works at both scales.
```

### B2 — Multiplayer Room Loading with Netcode for GameObjects

```
Building on the room streaming context: my game uses Unity's Netcode for GameObjects (NGO) 2.11.x for multiplayer (2-4 players, server-authoritative).

**The multiplayer challenge:**
The house graph is server-authoritative. When the server decides a room mutation occurs (Room A now connects to Room C instead of Room B), all clients must:
1. Unload Room B's geometry (if no player is in/adjacent to it)
2. Load Room C's geometry (because it's now adjacent to an occupied room)
3. Update portal visuals (doorway now shows Room C)
4. Do this without visible popping, hitching, or desyncing player positions

**What I need:**
1. How does NGO handle runtime spawning/despawning of complex prefabs? Is there a limit on NetworkObject count? What's the sync cost per spawned object?

2. Should room geometry be NetworkObjects, or should only the graph state be networked (with each client independently loading the correct geometry based on replicated graph state)?

3. Pattern: "server owns graph truth, clients render their own view" — is this viable with NGO? How do you prevent client drift (client thinks Room B is loaded, server already swapped to Room C)?

4. NetworkVariable<T> for graph state — can I sync an entire graph structure, or should I sync individual edge changes as events (ClientRpc)?

5. What happens if a player is standing in a doorway during a mutation? How should the network authority handle contested state?

6. Loading latency across network — if one client loads Room C faster than another, what patterns prevent visible desync (one player sees the room, other sees void)?

Practical patterns with code examples preferred. I'm using Unity 6 with NGO 2.11.x.
```

### B3 — Performance Budgets for Room-Based Horror Games

```
Building on the room streaming and multiplayer context: I need to establish performance budgets for my room-based horror game.

**Target specs:** 60fps on mid-range hardware (GTX 1060 / RTX 2060 equivalent), 2-4 players on LAN.

**What I need:**
1. How many draw calls can URP handle before frame drops? What's the budget for a dark interior scene with flashlights and dynamic shadows?

2. Memory budget per room: if each room is a prefab with mesh, colliders, lights, and props — what's a reasonable per-room memory footprint to target?

3. Room load/unload latency: what's acceptable for a horror game? (Visible hitch breaks immersion.) How do you hide loading (pre-load adjacent rooms? Fade transitions? Portal occlusion?)

4. NetworkObject budget: with NGO, how many synced NetworkObjects can exist before network bandwidth becomes a problem on LAN? On internet (future)?

5. Physics budget: if rooms have colliders and the player has a character controller, how many active colliders can exist without PhysX becoming a bottleneck?

6. Profiling methodology: which Unity Profiler modules should I watch for room streaming (Memory Profiler, Frame Debugger, Network Profiler)? What are the red-flag numbers?

7. Known performance traps with URP in dark scenes: shadow rendering cost scales with light count, volumetric effects cost, post-processing in enclosed spaces.

I want concrete numbers and thresholds I can use as "if we exceed X, we have a problem" during development.
```

---

## Batch C: Graph Data Structures for Spatial Games (P1)

> Run in a single thread. This informs the House Graph architecture decisions.

### C1 — Graph-Based Level Representation in Games

```
I'm building a horror game where the house layout is a graph that mutates at runtime. Rooms are nodes, doorways/portals are edges. The graph can change while players are inside it (edges rerouted, nodes inserted/removed, subgraphs attached).

I need a comprehensive survey of how games and game engines represent mutable spatial graphs:

1. **Data structures:** What graph representations work best for a spatial level? Adjacency list, adjacency matrix, half-edge, or something else? Tradeoffs for query speed vs mutation speed?

2. **Existing implementations:** How do games like Rooms (2012), Superliminal, Antichamber, or procedural dungeon generators represent their level topology? Any published GDC talks or postmortems?

3. **Graph mutation patterns:** When a graph edge is rerouted at runtime (door now leads somewhere else), what data needs to update? What are the consistency invariants? (No orphan nodes, no dangling edges, portal bidirectionality, etc.)

4. **Subgraph insertion:** When a "Tardis anomaly" inserts a new subgraph behind a door (interior bigger than exterior), what's the graph operation? How do you maintain entry/exit consistency?

5. **Observation-gated mutation:** The rule is "nodes can only mutate when unobserved." How should the graph track observation state? As node metadata? As a separate system querying the graph? As temporal locks?

6. **Pathfinding on mutable graphs:** If an AI entity needs to navigate the graph, and the graph changes mid-path, what pathfinding approaches handle this gracefully? (Replanning, incremental search like D* Lite, or hierarchical?)

7. **Serialization:** For saving/loading game state or syncing over network, what's an efficient graph serialization format for a spatial level graph?

Focus on practical implementation patterns, not pure CS theory. Code examples in C# preferred.
```

### C2 — Implementing a House Graph Runtime in Unity/C#

```
Building on the graph data structures context: I need to implement this in Unity 6 with C#.

**My specific requirements:**
- Graph of 8-50 room nodes with portal edges
- Server-authoritative (NGO multiplayer)
- Mutations happen at runtime (edge reroutes, node swaps, subgraph insertions)
- Must support queries: adjacency, path existence, observation state, mutation eligibility
- Must be debuggable (expose state for overlay rendering)
- Must be serializable (for network sync and save/load)

**What I need:**
1. Recommended C# data structure for the graph. Should nodes/edges be classes, structs, ScriptableObjects, or ECS components? What are Unity-specific considerations (GC pressure, serialization, inspector visibility)?

2. How to separate graph "definition" (the authored baseline) from graph "runtime state" (current mutated version). Pattern for resetting to baseline on round restart.

3. Interface design: what queries should the graph expose publicly? (GetAdjacentNodes, GetPortalDestination, IsPathBetween, GetMutationEligibleEdges, etc.)

4. Event system: when the graph mutates, how should dependent systems be notified? C# events, UnityEvents, message bus, or observer pattern? Unity-specific tradeoffs.

5. NetworkVariable serialization: NGO requires INetworkSerializable for custom types. How do you serialize a mutable graph efficiently? (Full snapshot vs delta encoding vs event-sourced mutations?)

6. Testing strategy: how to unit test a mutable graph without needing a running Unity editor? (EditMode tests, dependency injection patterns for graph consumers.)

7. Memory patterns: if rooms are pooled/instantiated based on graph state, how does the graph "own" versus "reference" the room geometry? Clean separation of logical graph from physical scene representation.

Show me the architecture: interfaces, key classes, and how they wire together. I want to know the right abstraction boundaries before I write code.
```

---

## Batch D: Observation & Visibility Systems in Games (P1)

> Run in a single thread. Informs Sprint 2 (Observation Lock).

### D1 — Player Visibility and Observation Systems

```
I'm implementing an "observation lock" system for a horror game: spaces that are currently observed (seen by a player) cannot mutate. Only unobserved spaces are eligible for spatial changes.

I need a comprehensive analysis of how to determine "what the player can currently see" in a 3D first-person game:

1. **Frustum-based visibility:** Using the camera frustum to determine which rooms/nodes are "observed." How accurate is Unity's frustum culling? Can I query it at runtime? (Camera.CalculateFrustumPlanes, GeometryUtility.TestPlanesAABB, OnBecameVisible/Invisible)

2. **Occlusion-based visibility:** A room behind a closed door shouldn't count as "observed" even if it's technically in the frustum. How do games handle occlusion-aware visibility for gameplay logic (not just rendering)?

3. **Portal-based visibility:** In a portal/doorway-connected space, visibility propagates through open portals but stops at walls. How is this computed efficiently? (Portal visibility graphs, PVS systems, BSP-based approaches adapted for modern engines)

4. **The "observation" problem in horror games:** How do games like SCP: Containment Breach, Weeping Angels mods, or similar "don't look away" mechanics determine observation? What heuristics do they use?

5. **Multiplayer observation:** With 2-4 players, observation is a union of all players' visibility sets. How do you efficiently compute and sync this? Server-authoritative observation state?

6. **Grace timers and hysteresis:** To avoid rapid mutation flickering (player looks away for 0.1s, space mutates, player looks back), how should temporal smoothing work? What are good grace period values for horror pacing?

7. **Performance:** Observation checks run every frame (or every N frames). What's the performance budget? How many nodes can you check per frame before it becomes expensive?

I need practical implementation approaches, not perfect solutions. "Good enough" visibility that's fast and debuggable is better than pixel-perfect ray-traced observation that's expensive.
```

---

## Batch E: Spatial Anomaly & Impossible Geometry in Games (P2)

> Run in a single thread. Informs Sprint 3 and the anomaly families.

### E1 — Impossible Geometry Techniques in Real-Time Games

```
I'm building a horror game where the house layout is spatially impossible — corridors loop back on themselves, interiors are bigger than exteriors, and rooms connect in non-Euclidean ways. All running in real-time first-person perspective in Unity 6 with URP.

I need a comprehensive survey of techniques for creating impossible/non-Euclidean geometry in real-time 3D games:

1. **Portal-based rendering:** How do games like Portal, Antichamber, and Superliminal render impossible connections? (Stencil portals, render texture portals, camera teleportation, seamless threshold crossing)

2. **The "infinite corridor" / looping hall:** How is this actually implemented? (Teleport player on crossing threshold? Procedurally extend geometry? Scrolling room segments?) What makes it feel seamless vs janky?

3. **Interior bigger than exterior (Tardis effect):** Technical approaches for making a room appear small from outside but large inside. (Scale tricks, separate render spaces, portal transitions?)

4. **Room substitution:** A room that looks different depending on which door you enter from. How is this achieved without the player noticing the swap? (Preloaded variants, crossfade during portal transition, instant swap when occluded?)

5. **Maintaining local coherence:** The player's immediate surroundings must feel physically consistent even when the global topology is impossible. What techniques preserve this illusion? What breaks it?

6. **Multiplayer impossible spaces:** If two players are in the same "impossible" corridor but entered from different doors, do they see the same thing? How do games handle shared observation of non-Euclidean space?

7. **Performance considerations:** Impossible geometry often requires rendering the same space from multiple viewpoints (portal cameras) or maintaining multiple geometry sets. What's the rendering cost? How do you budget for it in URP?

8. **Unity-specific implementation:** Which of these techniques work well in Unity 6 + URP? Any known compatibility issues with URP's rendering pipeline for portal/stencil tricks?

Cite GDC talks, developer blogs, and open-source implementations where available. I want to know what's proven to work in shipped games.
```

---

## Batch F: AI Pathfinding on Dynamic Graphs (P2)

> Separate thread. Informs M4 (Stalker Entity) — can run during M1/M2 development.

### F1 — AI Navigation in Mutable Level Topology

```
I'm building a horror game where a stalker entity navigates a house whose layout changes at runtime (edges reroute, rooms swap, new rooms appear). The entity needs to pathfind through this mutable graph while appearing intelligent and purposeful.

**Context:** The house is a graph of room nodes connected by portal edges. The graph mutates when unobserved. The entity navigates by graph traversal (room-to-room), not by mesh-level NavMesh.

**What I need:**

1. **Graph-based pathfinding for room navigation:** If the entity paths from Room A to Room F, and the graph mutates mid-path (Room C now connects to Room G instead of Room D), what pathfinding approach handles this gracefully?
   - Full replan (A* from scratch)?
   - Incremental search (D* Lite)?
   - Hierarchical (plan at graph level, navigate within rooms via local NavMesh)?

2. **NavMesh for intra-room movement:** Within a single room, the entity needs to walk around furniture, through doorways, etc. How does Unity's NavMesh system handle runtime loading/unloading of NavMesh surfaces per room? (NavMeshSurface.BuildNavMesh(), NavMesh linking between rooms)

3. **Connecting room-level NavMeshes:** If each room has its own NavMesh, how do you connect them at portals/doorways? (NavMeshLink, OffMeshLinks, manual stitching)

4. **Runtime NavMesh updates:** When a room is loaded/unloaded or a portal destination changes, how quickly can NavMesh be rebuilt? Is it frame-blocking? Can it be async?

5. **Entity behavior during mutation:** If the entity's current path becomes invalid because the graph mutated: should it replan immediately, pause and "look confused," or use the mutation as a teleport/advantage (entity can use impossible space better than players)?

6. **Multiplayer authority:** Entity pathfinding should be server-authoritative. How do you sync entity position and state to clients without rubber-banding? (NetworkTransform interpolation, server-side physics, client prediction for NPCs)

7. **Horror AI patterns:** In games like Alien: Isolation, the AI uses "director" systems to control pacing rather than pure pathfinding. How does a horror game balance "smart enough to threaten" with "not so smart it's unfair"? What role does intentional suboptimal pathing play?

Focus on Unity 6 + NGO implementation patterns. The entity doesn't need perfect pathfinding — it needs to feel like it belongs in impossible space.
```

---

## Batch G: Multiplayer Horror Architecture Patterns (P2)

> Separate thread. De-risks multiplayer decisions across all milestones.

### G1 — Multiplayer Architecture for Co-op Horror Games

```
I'm building a 2-4 player co-op horror game in Unity 6 with Netcode for GameObjects (NGO) 2.11.x. The game features a house with mutable topology (rooms disconnect/reconnect), observation-based mechanics (observed spaces can't change), and a stalker entity.

I need a comprehensive analysis of multiplayer architecture patterns for co-op horror:

1. **Authority model for mutable world state:** The house graph mutates server-side. What's the right authority split? (Server owns graph + entity + anchors; clients own their position + observation input + interaction intent?)

2. **Observation as gameplay mechanic in multiplayer:** Multiple players observing different rooms creates a shared "observation coverage" that constrains mutations. How should this be computed? (Server collects per-player observation sets, computes union, broadcasts eligibility? Or each client computes locally and server validates?)

3. **Consistency vs responsiveness:** When a mutation occurs, server must ensure all clients see the same state. But network latency means clients may briefly see stale state. What patterns prevent "I walked into a room that just mutated" desync? (Prediction, rollback, lock-step, authoritative teleport?)

4. **Player separation in mutable space:** If Player A is in Room X and Player B is in Room Y, and the graph mutates to disconnect those rooms — how should the game handle this? (Prevent the mutation? Allow it and let players be separated? Warning system?)

5. **Horror pacing in multiplayer:** How do shipped co-op horror games (Phasmophobia, Lethal Company, Devour, Content Warning) handle the tension between "let players explore independently" and "keep the horror experience coherent for everyone"?

6. **Network bandwidth for spatial state:** If the graph has 50 nodes and mutates frequently, how much bandwidth does state sync consume? When should you use NetworkVariables (continuous state) vs RPCs (discrete events)?

7. **Testing multiplayer horror:** How do you test observation mechanics, mutation timing, and entity behavior with 2+ players during development? (Multiplayer Play Mode, ParrelSync, dedicated server builds, automated test harness?)

8. **Late-join and reconnection:** If a player joins mid-round or reconnects after a disconnect, how should the mutable world state be reconciled? (Full snapshot, delta from last known state, respawn at safe node?)

Cite architecture from shipped multiplayer horror games where possible. I'm looking for patterns that are proven at indie scale (2-4 players, LAN first, internet later).
```

---

## Batch H: Unity ScriptableObject Architecture for Data-Driven Games (P1)

> Separate thread. Informs how we structure all game data definitions.

### H1 — ScriptableObject Patterns for Runtime Game Systems

```
I'm building a data-driven horror game in Unity 6 where many systems are configured via ScriptableObjects (room definitions, anomaly definitions, anchor definitions, match rules, entity behavior profiles).

I need a comprehensive guide to ScriptableObject architecture for runtime game systems:

1. **Definition vs Runtime State:** My architecture separates "definitions" (ScriptableObject assets, read-only at runtime) from "runtime state" (MonoBehaviour/plain C# instances, mutable). What are the best patterns for this? How do you instantiate runtime state from a definition without coupling them?

2. **ScriptableObject as configuration vs as data container:** When should SOs be used as simple config (tuning knobs) vs as complex data containers (graph definitions, behavior trees, room layouts)? Where's the line?

3. **ScriptableObject references in multiplayer:** If a client and server both reference the same SO definition, how do you ensure they're looking at the same asset? (Asset GUIDs, string IDs, enum mapping, NetworkObject references?)

4. **Large ScriptableObject graphs:** If I have 50 RoomDefinition SOs, each referencing MaterialProfile SOs, PropKit SOs, and LightingPreset SOs — how do I manage this web? (Asset database patterns, SO registries, addressable SO loading)

5. **Editor tooling for SOs:** What editor tools/patterns help manage large SO collections? (Custom inspectors, SO-based registries with validation, automated asset creation)

6. **Testing SOs:** How do you unit test systems that consume ScriptableObjects? (Test doubles, test-specific SOs, runtime SO creation in EditMode tests)

7. **Common pitfalls:** Shared state mutation bugs (accidentally modifying the asset at runtime), serialization edge cases, SO lifecycle in builds vs editor, null reference chains in SO graphs.

8. **Recommended folder structure:** For a project with 10+ SO types and 50+ SO instances, what folder organization prevents chaos?

Unity 6 specific. C# examples preferred. I'm using Netcode for GameObjects for multiplayer.
```

---

## Execution Schedule

| Batch | Priority | Run When | Thread | Est. Time |
|-------|----------|----------|--------|-----------|
| A (URP Lighting) | P0 | Before S0.1 session | Thread 1 | 2 prompts |
| B (Geometry Streaming) | P0 | Before/during S0.2 | Thread 2 | 3 prompts |
| C (Graph Data Structures) | P1 | During S0.1/S0.2, before S1A | Thread 3 | 2 prompts |
| D (Observation Systems) | P1 | During S1A, before S2 | Thread 4 | 1 prompt |
| H (ScriptableObject Arch) | P1 | Before S1A | Thread 5 | 1 prompt |
| E (Impossible Geometry) | P2 | During S1B/S2, before S3 | Thread 6 | 1 prompt |
| F (AI Pathfinding) | P2 | During M1/M2, before M4 | Thread 7 | 1 prompt |
| G (Multiplayer Patterns) | P2 | During M1, before S6 | Thread 8 | 1 prompt |

**Total: 8 threads, 13 prompts.** P0 batches (A, B) should complete before M1 development begins. P1 batches (C, D, H) should complete before their dependent sprints. P2 batches (E, F, G) can run in parallel with early development.
