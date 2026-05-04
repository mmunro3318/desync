# Impossible Geometry Techniques in Real-Time Games
### A Comprehensive Survey for Unity 6 + URP Horror Game Development

***

## Executive Summary

"Impossible geometry" in games is not actual non-Euclidean mathematics — it is Euclidean geometry with deliberately broken topological connections. Rooms exist in separate coordinate spaces; portals, teleports, and stencil tricks sew them together in ways that violate global consistency while preserving local coherence. Every shipped implementation from Portal (2007) to Tea for God (2018–present) uses variants of three core primitives: **stencil-masked virtual cameras**, **render-texture portal surfaces**, and **threshold-crossing teleportation**. Understanding the trade-offs between these determines almost everything else in your architecture.[^1][^2]

***

## 1. Portal-Based Rendering

### Core Concept

The illusion of a connected impossible space rests on showing the player a view of a different coordinate space through a portal frame. Two established rendering paths exist, each with distinct trade-offs.[^3][^4]

### Stencil Buffer Portals (Preferred for URP)

The stencil buffer is an 8-bit-per-pixel store that can mask draw calls. The canonical algorithm is:[^5][^6]

1. Render the normal scene without portals.
2. For each portal frame, draw its mesh writing a unique integer to the stencil buffer (e.g. portal 1 → value 1, portal 2 → value 2) but writing nothing to the color or depth buffers.
3. Position a **virtual camera** so its offset from the exit portal equals the player camera's offset from the entrance portal.
4. Re-render the scene from the virtual camera, discarding all fragments where the stencil value ≠ this portal's index.
5. After the portal view is drawn, re-draw the portal frame into the **depth buffer** (color writes off) to correctly occlude objects in front of the portal.[^7][^3]

For recursive portals (portal visible through a portal), the approach increments the stencil value at each recursion level rather than using unique per-portal bits. Thomas Reidemeister's OpenGL writeup (2013) — one of the most-cited references — demonstrates that on backtracking from a deeper recursion level, you draw the same portal frame again but *decrement* the stencil value to restore it, avoiding a bitmask allocation scheme. A single 8-bit stencil buffer supports up to 255 levels of nesting this way.[^8][^3][^5]

Valve chose the stencil approach for Portal because "it renders the entire frame to the back buffer so you don't have any extra texture memory requirements" and completes in a single pass.[^4]

### Render Texture Portals (More Compatible, Lower Quality)

Each portal has a dedicated **virtual camera** that renders to a `RenderTexture` each frame. The portal surface is a mesh with a shader that samples the texture in **screen space**, not UV space, by converting vertex positions through clip-space and doing a perspective divide. Screen-space sampling ensures the image through the portal doesn't distort as the player moves laterally in front of it.[^9][^10]

The critical additional step is the **oblique projection matrix**: the virtual camera's near clip plane must be set to the portal's exit plane, not to a default near-distance. Without this, geometry behind the exit portal bleeds through into the portal image. Unity provides `Camera.CalculateObliqueMatrix()` for this purpose, fed a Vector4 clip-plane computed via the inverse transpose of the camera's world-to-camera matrix.[^10][^11]

**Render texture portals do not support recursion** by default — a portal cannot show itself through another portal without a separate recursive render loop. Sebastian Lague's open-source Unity implementation (github.com/SebLague/Portals) handles this by rendering the deepest iteration first and working outward.[^12][^13]

### Virtual Camera Positioning (Both Approaches)

The key math for positioning the virtual camera is: take the player camera's transform *relative to the entrance portal*, flip it 180° around the portal's Y axis, then apply that relative transform to the exit portal's world transform. This mirrors the player's perspective through the portal pair. In code (Unity):[^14][^10]

```csharp
Matrix4x4 m = entrancePortal.transform.worldToLocalMatrix;
virtualCam.transform.position = exitPortal.transform.TransformPoint(
    Quaternion.Euler(0, 180, 0) * m.MultiplyPoint(playerCam.transform.position)
);
```

### Seamless Threshold Crossing

When the player physically crosses a portal, teleportation must be invisible. The standard technique uses the **dot product** of the player's position delta against the portal's forward vector. If the dot product's sign changes from positive to negative between frames, the player has crossed the plane. At that instant:[^15][^14]

1. Mirror the player's position and velocity through the portal transform.
2. Set player camera's rotation to match the exit portal's orientation offset.
3. The render texture / stencil was already showing the destination — the teleport is invisible.

To handle objects that straddle the portal plane, spawn a **mesh clone** of the object in the exit space while the original is partially through, and destroy the original once fully through.[^13][^14]

***

## 2. The Infinite Corridor / Looping Hall

### The Teleport-Swap Method (Proven in Shipped Games)

The simplest robust implementation, used in Antichamber, Super Mario 64's infinite staircase, and countless indie horror games:[^16][^17][^1]

- Build **two or three identical corridor segments** back to back in world space.
- Place invisible threshold triggers at the seam between segments.
- When the player crosses the forward threshold, teleport them back to the equivalent position in the rearmost segment, preserving momentum and look direction.
- Optional: Procedurally re-seed any random elements in front of them before they arrive.

The loop feels seamless only if the geometry is **strictly identical at the teleport boundaries** — the same art, the same lighting bake, the same shadows. Any discrepancy (light probe mismatch, a texture UV seam, a prop that clips differently) reads as a pop. Uniform-looking corridors with no distinctive landmarks are significantly easier to loop convincingly; that's the primary reason horror games favor the archetype.[^18][^19][^20]

### Preventing the "Look Back" Break

The most common failure mode: the player turns around the moment they're teleported and sees a visible discontinuity. Solutions:

- **Occlude with an event**: close a door, flicker the lights, trigger a loud audio cue — any hard sensory cut that frames the transition as intentional.[^19]
- **Keep the "back" view short**: doorframes and tight bends prevent the player seeing far enough behind to notice the seam.[^1]
- **Pre-teleport while still in the portal's depth**: only trigger once the player is fully through, so the door/threshold geometry occludes the previous segment.

### Procedural Segment Streaming (Advanced)

For randomized infinite corridors, spawn and destroy segments on demand — one ahead, one behind, one current. This fails gracefully on straight corridors but becomes complex at branches and corners because the spawn radius must be radial, not axial. Simpler implementations limit branching to T-intersections where only one corridor is visible at a time.[^21][^22]

***

## 3. The TARDIS Effect: Interior Bigger than Exterior

### The Two-World Approach

The universally shipped technique: the "small exterior" and the "large interior" are in **entirely separate locations in world space**. They share no geometry. The exterior door is a portal (stencil or render-texture) that looks into the interior space. On crossing the threshold, the player teleports to the interior. Returning through the door teleports them back.[^23][^24]

The illusion holds because the player cannot see both spaces simultaneously — they can only see the interior *through* the portal frame while outside, and once inside, there is no window to the exterior. The Antichamber team used Unreal Engine's built-in UTPortal system for exactly this. For Unity, the same effect uses the render-texture portal approach with an oblique projection clip so the room geometry behind the portal plane doesn't bleed through.[^24][^25][^23]

### Scale-FOV Tricks (Non-Teleport)

For a more disorienting effect without a hard teleport, use **forced perspective**: scale the exterior model to be visually small (e.g., a shed) while placing the camera exit point much further in, giving a sense of vast depth. Superliminal demonstrates the principle from the player's side: the game constantly resizes objects based on the measured distance between the object and whatever is behind it, so what looks like the same-size object is actually being scaled dynamically per-frame.[^26]

### The Render Capture Method (Unreal-style, adaptable to Unity)

Place the full-size interior room somewhere distant in the level. Mount a `SceneCapture2D` (Unreal) or a Unity camera targeting a `RenderTexture` at a position inside that room. The door portal surface samples this texture. When the player approaches, the camera is repositioned to mirror their approach direction, maintaining the parallax. On threshold cross, teleport. This removes the need for actual geometry at the door location.[^27][^24]

***

## 4. Room Substitution: Same Door, Different Room

### State-Machine Room Variants

Antichamber's core spatial trick is a **directional state machine for rooms**: the same corridor can connect to different rooms depending on *how* you entered it. Implementation:[^28][^17]

- Each room registers which portal was used to enter it.
- Portals are bidirectional but each direction maps to a different destination room variant.
- The variant is loaded (or revealed by layer switching) before the player arrives.

The key to invisibility: the swap happens **while the target space is fully occluded** by geometry — a doorframe, a bend, a fade to black, or a period when the player is looking forward through the new portal, not backward.[^25][^29]

### Instant Geometry Swap (Occluded Swap Method)

The Mind Palace developer's approach is clean: when the player crosses the portal, **the map changes around them** — not the player teleporting to a new room, but the room re-seeding itself. This works when:[^29]

1. The geometry immediately around the portal threshold is identical in both variants (matching "wallpaper" tiles).
2. The swap fires the moment the portal threshold is crossed, before the player can look back.
3. The old room is never visible through the new portal.

A practical Unity implementation: use layer masks or `SetActive()` to enable variant B and disable variant A simultaneously, one frame after the portal trigger fires. If any matching geometry surrounds the trigger, the swap is invisible even at 60fps.[^29]

### Crossfade During Transition

For smoother swaps (less reliant on tight geometry matching), add a brief full-screen vignette or ambient occlusion pulse on the frame of the swap. This masks the one-frame flicker of a geometry change and is used by several indie horror games.[^19]

***

## 5. Maintaining Local Coherence

### Why Local Coherence Is Achievable

The human perceptual system checks local consistency, not global topology. Impossible figure research demonstrates that figures obeying pictorial rules locally (perspective, shading, interposition) but violating them globally still appear spatially plausible in the immediate view. Games exploit this: the player's near field (within ~5 meters) must be physically correct; the global layout can be completely nonsensical.[^30][^31]

### Coherence Checklist

| Element | What to Preserve | What Breaks It |
|---|---|---|
| **Lighting** | Match light probe grids, shadow bakes, and ambient color at portal seams | Sudden ambient color change after teleport; mismatched directional light angle |
| **Audio** | Preserve reverb tail across portals; don't hard-cut room IR | Silence gap or reverb snap on threshold crossing |
| **Physics** | Transfer linear/angular velocity and apply the portal's rotation delta | Object spinning or reversing direction on teleport |
| **Carried objects** | Spawn clone in exit space, match transform; destroy original on full crossing | Object visually sticking at the portal plane |
| **Camera bobbing** | Preserve bob phase across teleport | Head-bob stutter on threshold |
| **Shadows** | Avoid real-time shadow cameras that see through portals into wrong spaces | Shadow leaking from the exit room into the entrance room |

### Predictable Break Points

- **Player looks backward during teleport**: solved by only firing the teleport when the player's movement is clearly forward (dot product check), and by immediately hiding the back side with geometry.
- **Portal visible through another portal with mismatched depth**: oblique projection clip planes prevent this.[^11]
- **Clipping into portal frame edge**: thicker invisible collider behind the portal frame, plus slicing shader that cuts held objects at the portal plane.[^13]
- **AI path navigation across impossible spaces**: navmeshes in separate spaces cannot cross; each world has its own navmesh, and AI must be spawned/destroyed at portal transitions.[^32][^33]

***

## 6. Multiplayer Impossible Spaces

### The Fundamental Challenge

In a networked game, two players in the "same corridor" entered from different portals may be in **entirely different coordinate spaces**. From the server's perspective, they are at completely different world-space positions. Their local views are consistent with their own topology, but the concept of "being in the same place" loses meaning.[^34][^32]

### Architecture Options

**Option A — Separate Coordinate Systems (Most Common)**
Each impossible room is a self-contained coordinate space. Players always know their own-space position. For inter-player interaction (combat, coop puzzles), the server resolves events by tracking which *logical room ID* each player is in, not their world position. A projectile crossing a portal is re-instantiated in the exit space with transformed velocity. This is essentially how Tea for God handles its procedurally generated impossible VR spaces.[^33][^35]

**Option B — Observer Divergence (Horror-Specific)**
Two players entering the same door from different directions intentionally see *different rooms*. The server tracks each player's last-used entry portal and delivers different room state. Players can describe what they see but literally cannot share the same view. This is powerful for asymmetric horror and co-op investigation games — used conceptually in Spaceflux's fractal arena design.[^36]

**Option C — Consensus + Hidden Duplication**
The impossible space resolves to one "true" room; all players experience it identically once *inside* it. The spatial impossibility only exists in the *approach* (the exterior looks small, but entering it, everyone sees the same large room). This is the safest for typical co-op scenarios.

### Practical Constraint

No shipped multiplayer game with full portal-based impossible geometry (in the game-engine stencil/teleport sense) has been documented with detailed public technical write-ups. HyperRogue (2-player, non-Euclidean math engine) and Tea for God (VR, procedural impossible spaces, single-player) are the closest references. Implementing full portal-based impossible geometry in a synchronous multiplayer context remains an open engineering problem with no off-the-shelf solutions.[^37][^34]

***

## 7. Performance Considerations

### Camera Overhead in URP

The Unity documentation is explicit: "An active camera runs through the entire rendering loop even if it renders nothing". In practice, the CPU overhead of switching render state between cameras can be severe. From a developer migrating Wavetale, removing three extra cameras reduced CPU time by **15ms per frame** on Switch. In HDRP, each render-texture camera cost 5–8 FPS even rendering minimal geometry; in URP, multiple cameras together cost only ~2 FPS for the same scene.[^38][^39][^40]

### Budgeting Portal Cameras

| Technique | Memory Cost | CPU/GPU Cost | Recursion | Notes |
|---|---|---|---|---|
| Render Texture | 1 RT per portal (+ mip chain) | Full scene re-render per portal | Manual loop required | Easy to implement; no MSAA on RT |
| Stencil Portal | None (stencil is free) | Partial re-render (stencil-clipped) | Up to 255 levels | More complex to implement in URP |
| Camera Stack Overlay | Per-stack cost | URP's standard composite overhead | N/A | For UI/weapon only, not portals |

### Optimization Techniques

- **Frustum cull portal cameras**: Don't render a portal camera if the portal frame is off-screen or behind the player. Check using `GeometryUtility.TestPlanesAABB` before calling `Render()`.
- **Reduce render texture resolution**: Portal textures at 50% of screen resolution are often imperceptible, halving fill-rate cost.
- **Limit recursion depth to 2–3**: Beyond 3 levels, the portal image is a few dozen pixels and contributes negligible visual quality at enormous cost.
- **Replace cameras with ScriptableRendererFeature**: For portals where you only need to render specific layers from a controlled perspective, a custom `ScriptableRenderPass` using `ScriptableRenderContext.DrawRenderers()` eliminates per-camera loop overhead.[^40][^41]
- **Use `RenderPipelineManager.beginCameraRendering`** to trigger portal camera renders at the right moment in URP's pipeline.[^42][^43]

### Shadow and Post-Processing Costs

Portal cameras by default inherit the main camera's shadow and post-processing settings. Disable shadow casting on portal cameras explicitly (`camera.clearFlags`, shadow distance = 0) and turn off expensive post-processing effects (DOF, bloom) on the virtual camera. In URP, shadow rendering is the single largest per-camera cost and must be stripped on portal cameras.[^44][^45]

***

## 8. Unity 6 + URP: Specific Compatibility and Implementation Notes

### The `OnPreCull` Replacement

The most common error when porting Built-in RP portal tutorials to URP: `OnPreCull` and `OnPreRender` **do not fire on cameras in URP**. Replace all portal camera update calls with:[^46]

```csharp
RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;

void OnBeginCameraRendering(ScriptableRenderContext ctx, Camera cam)
{
    if (cam == playerCamera) UpdateAllPortalCameras();
}
```

This fires before each camera renders and is the correct hook for positioning virtual cameras before they draw.[^43][^42]

### URP 17 (Unity 6) Render Graph Breaking Changes

Unity 6 moved URP to a Render Graph API. Key breaking changes affecting portal implementations:[^47]

- `ClearFlag.Depth` no longer implicitly clears the stencil buffer. You must use `ClearFlag.Stencil` explicitly for stencil-based portal approaches.[^47]
- `ScriptableRenderer.cameraColorTarget` and `cameraDepthTarget` are obsolete; use `cameraColorTargetHandle` and `cameraDepthTargetHandle`.[^47]
- Custom `ScriptableRenderPass` written for URP < 17 must be rewritten using the Render Graph API.
- **Compatibility Mode escape hatch**: Enable **Project Settings > Graphics > Render Graph > Compatibility Mode (Render Graph Disabled)** to use the old API while you port. This is a documented transitional path, not a permanent solution.[^48][^47]

### Stencil Buffer in URP

Stencil access in URP requires the depth-stencil format to be set correctly on any `RenderTexture` targets (not `None`). A known URP issue (present in 6000.0.0b16 and earlier): `Render Objects` renderer features do not write to the depth buffer unless the object's layer is included in the **Prepass Layer Mask**, a setting only available when Render Graph is enabled. Stencil-based portals should be implemented via a fully custom `ScriptableRendererFeature` rather than through the Render Objects feature.[^49][^50]

For stencil portals in URP, the workflow using Renderer Features is:[^51]

1. Add a **Render Objects** feature set to `AfterRenderingOpaques` that renders the portal frame meshes to the stencil buffer only (ColorMask 0, ZWrite Off).
2. Add a second **Render Objects** feature that renders the portal-destination layers only where stencil equals the portal's reference value.
3. Use `Stencil { Ref N; Comp Equal; Pass Keep }` in the destination render override.

### Oblique Projection Depth Texture Bug

A known Unity issue: depth textures generated with an oblique projection matrix cannot be used by standard post-processing shaders (SSAO, depth of field) because the depth encoding is different from regular projection. The documented workaround is to swap the camera back to a non-oblique matrix before the depth-only pass, render depth manually via a `CommandBuffer`, then restore the oblique matrix for the color pass. This adds ~1 draw call per portal camera per frame.[^52]

### Sebastian Lague's Portal System (Open Source, URP-Compatible)

The most widely referenced Unity portal implementation works in URP with the following fixes (documented in the repo's issues):[^53][^54][^12][^13]

- Enable Post Processing on **both** portal cameras, not just the main camera.
- If using Camera Stacking to render weapons on an overlay camera, remove the overlay camera and instead add a **Render Objects** ScriptableRendererFeature on its own layer. Camera stacking interferes with portal camera ordering.[^53]
- The shader ports directly to URP via Shader Graph's screen position node replacing the built-in RP's `ComputeScreenPos`.

### URP Render Texture Warnings in Unity 6

Setting a `RenderTexture`'s Depth Stencil Format to `None` and assigning it as a camera's Output Texture causes a runtime warning in URP + Render Graph mode: "Trying to render to a rendertexture without a depth buffer. URP+RG needs a depthbuffer to render." All portal `RenderTexture` assets should use at minimum `Depth32Stencil8` format.[^49]

***

## Technique Reference Matrix

| Technique | Shipped Examples | Unity 6 URP Status | Complexity | Performance Impact |
|---|---|---|---|---|
| Stencil portal | Portal, Portal 2 | Requires custom RendererFeature; Render Graph re-write needed | High | Low (no RT memory) |
| Render texture portal | Superliminal, most indie | Native support; `beginCameraRendering` hook required | Medium | High (full re-render per portal) |
| Threshold teleport (invisible) | Antichamber, SOMA, Control | Trivial in Unity | Low | Negligible |
| Infinite corridor (3-segment swap) | Mario 64 stairs, Antichamber halls | Trivial | Low | Negligible |
| TARDIS (separate world space) | TARDIS-style Source engine demos, Antichamber rooms | Works natively | Low–Medium | Negligible (no portal camera needed) |
| Room substitution (occluded swap) | Antichamber, Mind Palace | `SetActive()` + collider trigger | Medium | Negligible |
| Full hyperbolic geometry | Hyperbolica, HyperRogue | Not supported by standard Unity renderer; custom ray-marching required | Very High | Very High |

***

## Architectural Recommendation for a Horror Game

For a first-person Unity 6 URP horror game, the recommended stack is:

1. **Default to threshold teleportation** for all spatial impossibilities (looping corridors, room substitution, TARDIS rooms). It's invisible when occluded by a doorframe, requires no extra cameras, and has zero rendering cost.

2. **Add render-texture portals only where the player must see the impossibility live** (a window looking into a larger space, a mirror showing a different room). Budget 2–4 portal cameras maximum. Disable them when the portal is off-screen.

3. **Avoid stencil portals until you've fully migrated to URP 17 Render Graph** — the API surface is in flux and the depth-texture interaction bugs require non-trivial workarounds.

4. **Maintain coherence at seams with art direction, not code**: identical lighting bakes at teleport boundaries, audio reverb zones that span threshold points, and occluding geometry (doorframes, corners) that visually justify the transition.

5. **Multiplayer**: scope to Option C (consensus interior, impossible only in approach) unless asymmetric perception is a core design mechanic. Implementing full portal-based player synchronization across impossible spaces from scratch is a multi-month engineering project with no off-the-shelf solution.

---

## References

1. [Questions about Non Euclidean game design. : r/truegaming - Reddit](https://www.reddit.com/r/truegaming/comments/krlsf/questions_about_non_euclidean_game_design/) - The non-Euclidean geometry that is relevant to games is when the graphics on the screen can not be s...

2. [[PDF] Non-Euclidean Video Games:Exploring Player Perceptions and ...](https://pure.york.ac.uk/portal/files/101760843/Non-Euclidean_Video_Games_Exploring_Player_Perceptions_and_Experiences_inside_Impossible_Spaces.pdf) - Most of these types of games, however, were not created using non-Euclidean geometry to create these...

3. [Rendering "Portal" by torinmr - GitHub Pages](https://torinmr.github.io/cs148/) - Thomas's approach instead increments the stencil buffer when the portal is drawn for the first time,...

4. [Valve developers discuss Portal problems - CS50's Intro to Game ...](https://www.youtube.com/watch?v=riijspB9DIQ) - ... Portal? ⌨️ (0:04:40) Rendering ⌨️ (0:04:52) Texture vs Stencil Tradeoffs ⌨️ (0:09:35) Rendering ...

5. [Rendering recursive portals with OpenGL - th0mas.nl](https://th0mas.nl/2013/05/19/rendering-recursive-portals-with-opengl/) - Disable the stencil test, disable drawing to the color buffer, and enable drawing to the depth buffe...

6. [Rendering Portals in Virtual Reality - arXiv](https://arxiv.org/html/2601.20722v1) - Using the stencil buffer to render only the necessary parts of each portal could in theory allow for...

7. [opengl - how to implement "portal rendering" - Stack Overflow](https://stackoverflow.com/questions/38287235/opengl-how-to-implement-portal-rendering) - I tried solving the problem using different stencil functions and not empting the stencil buffer whe...

8. [[PDF] Rendering Portals](https://www.cs.rpi.edu/~cutler/classes/advancedgraphics/S21/final_projects/metzlr.pdf) - Otherwise, we start by first incrementing the val- ues within the stencil buffer everywhere within t...

9. [Portals | Part 2 - Stencil-based Portals - Daniel Ilett](https://danielilett.com/2019-12-14-tut4-2-portal-rendering/) - Portal Rendering. We're going to use the stencil buffer to render the portal surfaces - we've discus...

10. [Fully Functional Portals in Unity URP - YouTube](https://www.youtube.com/watch?v=PkGjYig8avo) - ... portals-urp -------------- ✨ Intro - 0:00 ✨ Recursive Rendering - 1:21 ✨ Oblique Projection Matr...

11. [Portals | Part 3 - Matrix Clipping - Daniel Ilett](https://danielilett.com/2019-12-18-tut4-3-matrix-matching/) - In order to clip the correct details, we must make use of an oblique projection matrix, which involv...

12. [SebLague/Portals: Portals in Unity - GitHub](https://github.com/SebLague/Portals) - Little test of portals in Unity. Note: in the two worlds scene, you'll need to have Blender installe...

13. [Coding Adventure: Portals - YouTube](https://www.youtube.com/watch?v=cWpFZbjtSQg) - Experimenting with portals, for science. The project is available here: https://github.com/SebLague/...

14. [How I Made Multiplayer Portals In Godot For My Game - YouTube](https://www.youtube.com/watch?v=KZZ3Xw9sfvE) - Project using my portal implementation: https://github.com/majikayogames/portal_demo Sebastian Lague...

15. [I managed to implement a seamless portal transition in my game.](https://www.reddit.com/r/IndieDev/comments/1cwix32/i_managed_to_implement_a_seamless_portal/) - The plane renders what the target portal's camera sees through a shader. When the player crosses the...

16. [I recreated the BEST non-euclidean environments - YouTube](https://www.youtube.com/watch?v=c4clJK5uOvw) - this took so long to edit hope you like it :) like if you like like if you dislike anyways why are y...

17. [Alexander Bruce and the Philosophy of Puzzle Games - YouTube](https://www.youtube.com/watch?v=wf2kHPJA5lw) - A critique of Antichamber and discussion of its creator, Alexander Bruce, as well as musings on the ...

18. [Looping corridor - Blueprint - Epic Developer Community Forums](https://forums.unrealengine.com/t/looping-corridor/79340) - I want to make a looping corridor. As the character walk straight he continuously opens the same doo...

19. [Non-Euclidian Geometry in Horror](https://horrorobsessive.com/2021/03/26/non-euclidian-geometry-in-horror/) - Non-Euclidian Geometry, or Nightmare Geometry as Lor Gislason calls it, provokes the horrifying natu...

20. [Something Scary Happens When You Break The Laws of ...](https://www.youtube.com/watch?v=GaGcLhhhbDs) - In nonukan geometry we decide to take one of those laws and have it be broken and suddenly it leads ...

21. [How to Make an Infinite Corridor in Unreal Engine 5 ... - YouTube](https://www.youtube.com/watch?v=jbHehyltn8A) - In this tutorial, we'll show you how to create a looping corridor effect in Unreal Engine 5. As the ...

22. [Devlog | Making an Infinite Corridor (and failing) - YouTube](https://www.youtube.com/watch?v=mmAS0i7ZnP0) - ... loop seamless. Having room spawn info would also help to destroy old ... I'm making a game engin...

23. [Making a TARDIS in Unity - it's bigger on the inside!](https://www.reddit.com/r/Unity3D/comments/agm90b/making_a_tardis_in_unity_its_bigger_on_the_inside/) - The original Unreal engine was entirely capable of doing TARDIS style effects in completely 3D envir...

24. [TARDIS effect? - Blueprint - Epic Developer Community Forums](https://forums.unrealengine.com/t/tardis-effect/51348) - Anyone know how I could make a bigger-on-the-inside effect? Here's an example of what I mean (this i...

25. [Guy makes a game engine that works in a very different way : r/videos](https://www.reddit.com/r/videos/comments/hm9zeb/guy_makes_a_game_engine_that_works_in_a_very/) - Antichamber was made almost entirely by one guy, Alexander Bruce, who said ... If he has to teleport...

26. [Out of Bounds Secrets | Superliminal - Boundary Break](https://www.youtube.com/watch?v=BSSaDsBJz4M) - More developer tricks and outabounds content is going to be shown today on an ongoing Series where w...

27. [UE52 Tardis FX - Bigger on Inside than outside](https://www.youtube.com/watch?v=gRg7ll77jrc) - Using a Scene Capture Component, along with some simple material and camera trickery. ... UE52 Tardi...

28. [Blocks, Passages, and Non-Euclidean Geometry (Antichamber)](https://videlais.com/2013/02/10/blocks-passages-and-non-euclidean-geometry-antichamber/) - While many parts of its world are non-Euclidean in nature, turning around in place can move you with...

29. [Deep Dive: Creating impossible spaces in Mind Palace](https://www.gamedeveloper.com/art/deep-dive-creating-impossible-spaces-mind-palace) - Matthew Chovan, solo developer on Mind Palace explores the design and technical creation of impossib...

30. [Impossible Figures in Perceptual Psychology - Fink](http://www.fink.com/papers/impossible.html) - These "impossible figures" use depth cues to seem three-dimensional, yet misuse them in such a way a...

31. [Horror Games, Impossible Architecture, and the Overlook Hotel](https://www.oxfordstudent.com/2012/08/16/horror-games-impossible-architecture-and-the-overlook-hotel/) - Interactive impossible spaces open up extra possibilities to create a sense of the uncanny – the lay...

32. [Non-Euclidean VR - Hidden Gem or Dead End? - Adventurous Studios](https://www.adventurousstudios.com/blog/Blog%20Post%20Title%20One-dpa63) - Play Space Constraints: Non-Euclidean games require a decent amount play space in order to function ...

33. [Tea For God = impossible spaces / non euclidean geometry ... - Reddit](https://www.reddit.com/r/Vive/comments/a03eq3/tea_for_god_impossible_spaces_non_euclidean/) - Just good old walking, as you walk every day. There is no artificial locomotion, no teleporting. Thi...

34. [Non-euclidean space multiplayer? : r/virtualreality - Reddit](https://www.reddit.com/r/virtualreality/comments/ddu0bd/noneuclidean_space_multiplayer/) - One thing I have not seen done in VR well, is a Non-Euclidean Multiplayer game. Would something like...

35. [Open world with impossible spaces - Tea For God by void room](https://void-room.itch.io/tea-for-god/devlog/235914/open-world-with-impossible-spaces) - Impossible spaces require the player to remain within a certain space. By their nature, they allow f...

36. [Spaceflux on Steam](https://store.steampowered.com/app/1344440/Spaceflux/) - An FPS arena shooter with mind-bending fractal maps! Fight in an impossible battle arena where the m...

37. [Plunge into Infinite VR Space with Tea for God in the Oculus Quest](https://www.digitalbodies.net/plunge-into-infinite-vr-space-with-tea-for-god-in-the-oculus-quest/) - A demonstration of how the use of non-euclidean geometry, procedural generation, and redirected walk...

38. [Use multiple cameras | Universal RP | 14.0.12 - Unity - Manual](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/manual/cameras-multiple.html) - If you use multiple cameras, it might make rendering slower. An active camera runs through the entir...

39. [Spent a few weeks rewriting everything from HDRP to URP : r/Unity3D](https://www.reddit.com/r/Unity3D/comments/1midj89/spent_a_few_weeks_rewriting_everything_from_hdrp/) - In URP, all those cameras together now cost me only about 2 FPS. It's likely that camera handling di...

40. [Avoiding CPU Overhead in Unity by Replacing Cameras with ...](https://agentlien.github.io/cameras/index.html) - Mainly, using a different camera allows you to render to separate render targets and use different l...

41. [Introduction to Scriptable Render Passes in URP - Unity - Manual](https://docs.unity3d.com/6000.1/Documentation/Manual/urp/renderer-features/intro-to-scriptable-render-passes.html) - A Scriptable Render Pass lets you to do the following: Change the properties of materials in your sc...

42. [Using the beginCameraRendering event | Universal RP | 7.4.3](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@7.4/manual/using-begincamerarendering.html) - Using the beginCameraRendering event. The example on this page shows how to use the beginCameraRende...

43. [RenderPipelineManager.beginCameraRendering - Unity - Manual](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Rendering.RenderPipelineManager-beginCameraRendering.html) - In the Universal Render Pipeline (URP) and the High Definition Render Pipeline (HDRP), Unity calls R...

44. [Configure for better performance in URP - Unity - Manual](https://docs.unity3d.com/6000.1/Documentation/Manual/urp/configure-for-better-performance.html) - To optimize URP for better performance, minimize the number of cameras you use. This also reduces pr...

45. [Optimize for better performance | Universal RP | 16.0.6](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@16.0/manual/optimize-for-better-performance.html) - If the performance of your Universal Render Pipeline (URP) project seems slow, you can analyze your ...

46. [URP and Built-in RP feature mapping - Ming Wai Chan](https://cmwdexint.com/2021/01/15/urp-and-built-in-rp-feature-mapping/) - Camera Rendering Callbacks, Called on Camera: · OnPreCull · OnPreRender · OnPostRender · OnRenderIma...

47. [Manual: Upgrade to URP 17 (Unity 6.0)](https://docs.unity3d.com/Manual/urp/upgrade-guide-unity-6.html) - For compatibility purpose, Unity 6 includes the option to disable the render graph system and use th...

48. [Compatibility Mode in URP - Unity - Manual](https://docs.unity3d.com/6000.0/Documentation/Manual/urp/compatibility-mode.html) - If you enable Compatibility Mode (Render Graph Disabled) in URP graphics settings, you can write a S...

49. [[URP][RG] Warning thrown when RenderTexture has Depth Stencil ...](https://issuetracker.unity3d.com/issues/urp-rendergraph-warning-thrown-when-rendertexture-has-depth-stencil-format-set-to-none) - A confusing warning is logged. Trying to render to a rendertexture without a depth buffer. URP+RG ne...

50. [[URP][Comp/RG] Render Objects Renderer Feature are not writing ...](https://issuetracker.unity3d.com/issues/render-objects-renderer-feature-are-not-writing-to-the-depth-buffer-when-write-depth-is-enabled) - 1. Open the attached "RenderObjectsDepthBugReport" project · 2. Open "Scenes/SampleScene" scene · 3....

51. [[TUTORIAL] Stencil buffer in Unity URP (cutting holes ... - YouTube](https://www.youtube.com/watch?v=y-SEiDTbszk) - Hey game dev enjoyers! Here we are: the mighty tutorial about the stencil buffer that I've been work...

52. [Weird boolean-like depth texture when using oblique projection ...](https://www.reddit.com/r/Unity3D/comments/xvmbcy/weird_booleanlike_depth_texture_when_using/) - I've resolved the issue long ago by swapping the matrix for a normal one and manually rendering the ...

53. [Conversion to HDRP · Issue #3 · SebLague/Portals - GitHub](https://github.com/SebLague/Portals/issues/3) - The portals no longer appear. This seems to be because HDRP does not use the OnPreCull event in Main...

54. [Converting the shaders from BRP to URP? · Issue #8 - GitHub](https://github.com/SebLague/Portals/issues/8) - The portal shader works fine with URP. To get the portals working in URP you need to enable Post Pro...

