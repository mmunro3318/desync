# Debugging Floor-to-Floor Light Leaks in URP Modular Graybox Architecture
> **Target setup:** Unity 6, URP 17.x, first-person horror, modular graybox (separate mesh primitives assembled in-editor), two-story interior house, symptom is a "band of glitchy texture along the floor/ceiling seam, visible from the outside."

**Note:** Prior, non-debugging research at `docs/design/98-unity-research/07-urp-lighting....md`

***
## Triage: What Your Symptom Actually Points To
The key diagnostic here is **"visible only from the outside."** A true shadow-map light bleed is primarily visible *inside* the building — you'd see a lit floor on floor 2 that shouldn't be lit, or a lit wall face that's behind an occluder. A band artifact at the seam, only visible from outside, is almost never a shadow-map issue first. It points instead to **geometry construction problems at the modular seam**. The ordered checklist below reflects this: geometry issues come first, shadow/light issues second.

***
## 1. Systematic Debugging Checklist — Ordered by Likelihood
### Cause #1 (Most Likely): Z-Fighting at the Coplanar Seam
**What it looks like:** A shimmering, flickering checkerboard or banding pattern at the exact line where the floor 1 ceiling piece meets the floor 2 floor piece. The artifact is most visible from external oblique angles and may flicker as the camera moves. It is not consistently dark or light — it alternates.

**Why it happens:** In a modular graybox, Floor 1's ceiling mesh and Floor 2's floor mesh both have a face at exactly the same Y position. The GPU's depth buffer (Z-buffer) has limited precision. When two triangles occupy the exact same world-space plane, sub-pixel precision errors cause the renderer to flip between which triangle is "in front" on a per-pixel basis, producing the characteristic shimmer.[^1]

**How to confirm:**
1. In the Scene View, use the **Wireframe** overlay (top-left Shading dropdown → Wireframe) and zoom into the seam. You will see two overlapping face outlines at the same Y.
2. Select the ceiling piece, press **F** to focus it, and inspect its Y position + height. Select the floor piece of floor 2. If `ceiling_piece.transform.position.y + ceiling_piece_height == floor2_piece.transform.position.y`, you have coplanar faces.
3. You can also check via **Frame Debugger** (`Window > Analysis > Frame Debugger`) — step through draw calls; Z-fighting will show up as unstable color values in the depth pass.

**The correct fix:**
- **Option A (preferred):** Redesign the modular set so the floor 2 floor piece *starts* at the same Y as the floor 1 ceiling piece — meaning one mesh contains both the floor and ceiling at that level, eliminating the shared face entirely. This is the architecturally correct solution.
- **Option B:** Make one piece slightly smaller so its face is offset by at least 0.001–0.01 units from the other piece's face. Only use this for graybox prototyping; it introduces a micro-gap that may cause Cause #2 below.
- **Option C:** Merge the floor/ceiling transition into a single mesh section using ProBuilder's `Merge Objects` or by combining in Blender, so there is exactly one face at that boundary.

***
### Cause #2 (Very Likely): Geometry Gap / Visible Interior Through Exterior Crack
**What it looks like:** A thin bright or tinted horizontal line at the seam, not flickering but consistently showing the illuminated interior color. It may have a slight glow if bloom is enabled. It looks like you can see *into* the lit room through a crack.

**Why it happens:** Modular pieces assembled in-editor with floating-point transforms can have submillimeter gaps even when their transforms appear aligned. A gap as small as 0.001 units can let a camera ray pass through and hit the interior face of the floor/ceiling from the outside, which is lit by the floor 1 point or spot light.

**How to confirm:**
1. In the Scene View, enable **Backface Culling** visualization: select both meshes, use **Mesh Renderer Inspector > Bounds** to see their extents and check for gaps.
2. Zoom to the seam at maximum zoom in Scene View; any gap will be visible.
3. Temporarily add a **bright material** to one piece — a gap will show the bright color bleeding along the seam from outside.
4. Check **Edit > Snap Settings** — if snap was not enabled during placement, pieces may not be grid-aligned.

**The correct fix:**
- Enable **Increment Snapping** (`Ctrl+drag`, or toggle Grid Snapping in the Scene View toolbar) and re-snap all pieces to a consistent grid. For a 1-meter modular set, use 0.25m or 0.5m grid increments.
- **Overlap your pieces intentionally**: extend the ceiling piece up by 0.1 units and the floor 2 piece down by 0.1 units so they overlap rather than meeting flush. This eliminates gaps and is standard modular level design practice.[^2]
- Use ProBuilder's **Merge** or set the wall/floor pieces to share vertex positions precisely using ProBuilder's **Weld Vertices** tool.

***
### Cause #3 (Likely for Graybox): Backface Culling — Missing Interior or Exterior Face
**What it looks like:** A black band or a hole at the seam, rather than a lit artifact. From outside you see "nothing" or an inverted-shaded face. From inside, the floor or ceiling may look missing or dark where it shouldn't be.

**Why it happens:** Unity's URP Lit shader culls backfaces by default — only the front face (the face whose normal points toward the camera) is rendered. A primitive cube used as a floor has normals pointing *up* (toward the interior). From outside, the exterior face of that same floor is the backface, and it renders as invisible/transparent. This is not Z-fighting — it's a missing surface.[^3][^4]

**How to confirm:**
- In Scene View, switch to the **Normals** debug overlay (Scene View toolbar → **Debug Mode → Lighting → Normal**). You'll see cyan normal vectors. If the floor piece's normals all point inward (toward floor 1), the exterior face of the floor has no normal to render from outside.
- Alternatively, create a **test camera** outside the building and hit Play. If you see through the floor, it's a backface issue.

**The correct fix:**
- For graybox prototyping, the simplest fix is: select the floor piece's material → enable **Render Face: Both** in the URP Lit shader Material Inspector. This disables backface culling for that material. Be aware this has a small performance cost and renders the backface without correct shading (normals are inverted on the back, so shading will be flipped).[^4][^3]
- The architecturally correct fix is to model the floor as a solid box with both outward-facing normals (top face pointing up for floor surface, bottom face pointing down for visible ceiling from floor 0), rather than a flat plane. Even 0.1 units thick is sufficient.
- In ProBuilder: select the face, use **Flip Normals** on the bottom face of the floor piece to give it a correct downward-pointing normal.

***
### Cause #4 (Moderate): Shadow Bias Too High — Light Leaking at Seam
**What it looks like:** A slightly illuminated "halo" or bright edge along the seam on the geometry face that should be in shadow. This is the classic bias-induced light leak. Unlike Z-fighting, it does not flicker — it is a stable bright band following the mesh silhouette.

**Why it happens:** When Depth Bias on a light is set too high, URP pushes the shadow caster's depth value further from the light, creating a gap between the shadow boundary and the actual geometry surface. Near a thin geometry seam (floor/ceiling junction), this gap can be large enough for light to appear on the far side of the occluder.[^5][^6]

**How to confirm:**
1. Select the floor 1 light source (spot light or point light).
2. In **Light > Shadows**, set **Bias → Custom**, then temporarily set **Depth to 0** and **Normal to 0**. If the seam artifact disappears or shrinks significantly, shadow bias is the cause.
3. Reintroduce shadow acne (dark self-shadowing noise) as a side effect of zeroing bias — you'll then know your working range.

**The correct fix:**
- Set **Depth Bias to 0.5–1.0** (not higher — 2–4 is too aggressive for interior) and **Normal Bias to 0.3–0.5** as a starting point per-light.[^5]
- Do not rely solely on the global URP Asset bias — use **Custom** per-light for flashlights and practical sources.
- The community-verified best practice: set the **URP Asset global Normal Bias to 0**, then tune each light individually. The global and per-light values are additive, so high global + high per-light = severe peter-panning and leak.[^7][^8]

***
### Cause #5 (Moderate): Shadow Map Resolution Too Low for Thin Geometry
**What it looks like:** Light appears on the wrong side of a wall or floor edge in a way that follows the geometry contour, but is a soft gradient rather than a sharp seam. It is most visible near the light source and fades with distance.

**Why it happens:** The ceiling or floor geometry at the seam may be thinner than a single shadow map texel at that distance. The shadow depth pass literally cannot resolve the geometry boundary, and the sample falls on the lit side of the occluder.[^2]

**How to confirm:**
- Open **Frame Debugger** → navigate to the shadow map pass for the floor 1 light → view the shadow atlas tile. Zoom into the area near the seam. If the floor/ceiling geometry appears as fewer than 2–3 texels wide in the shadow map, resolution is the issue.
- Increase the light's shadow resolution temporarily to its maximum (4096 per-light via Custom resolution) and check if the leak disappears.

**The correct fix:**
- Increase the floor 1 light's shadow resolution: **Light > Shadows > Resolution: Custom → 2048 or 4096**.[^9][^2]
- Make the floor/ceiling transition piece at least **0.25–0.5 m thick** (instead of a flat primitive). This gives the shadow map significantly more resolution to work with.
- Reduce the light's range — a tighter range means less world-space area to cover with the shadow map, meaning higher effective texel density at the seam.

***
### Cause #6 (Less Likely for Graybox): APV / Lightmap GI Bleed
**What it looks like:** A subtle warm or colored tone visible at the seam from outside, matching the ambient lighting color of floor 1. This is an indirect GI contribution, not a direct shadow artifact. It is baked, so it appears in Play Mode and Editor but won't change if you move the runtime light.

**Why it happens:** APV probes placed near the floor/ceiling boundary may be inside the geometry or straddling the wall boundary. These probes store floor 1 lighting data and bleed it to floor 2 surfaces because the probe is ambiguously assigned to both floors' geometry.[^10][^11]

**How to confirm:**
- In the Scene View, enable **Gizmos > Light Probes** to visualize APV probe positions. Zoom to the seam — any probe inside the geometry is suspect.
- Temporarily disable GI (Window > Rendering > Lighting > disable **Auto Generate**, then **Clear Baked Data**). If the artifact disappears, it's a baked GI issue.

**The correct fix:**
- Add a **Probe Adjustment Volume** (`GameObject > Light > Probe Volumes > Probe Adjustment Volume`) sized to encompass the boundary geometry, set Mode to **Invalidate Probes** to remove probes inside the wall/floor geometry.[^10]
- Use Rendering Layer Masks (see Section 5 below) to assign Floor 1 and Floor 2 surfaces to separate APV zones.

***
## 2. URP Settings Reference for Each Cause
### URP Asset → Shadows Panel
| Setting | Location | Impact on Seam Artifact |
|---|---|---|
| **Max Distance** | URP Asset > Shadows | Reduces world coverage per cascade texel; set to 20–30m for interior |
| **Depth Bias (global)** | URP Asset > Shadows | Default additive base; set to **0.5** |
| **Normal Bias (global)** | URP Asset > Shadows | Set to **0** — tune per-light only[^8][^7] |
| **Additional Lights Shadow Atlas** | URP Asset > Shadows | Must be large enough for all shadow maps; 4096 minimum |
### Light Component → Shadow Settings
| Setting | Value for Interior Horror | Why |
|---|---|---|
| **Shadow Type** | Soft Shadows | PCF filtering hides acne at seam edges |
| **Soft Shadows Quality** | Medium | Match or override pipeline setting[^12] |
| **Bias** | Custom | Never leave at "Use Pipeline Settings" for close-range lights |
| **Depth** | 0.5–1.0 | Start at 0.5; increase only if acne appears |
| **Normal** | 0.3–0.5 | Decrease if peter-panning at floor contact |
| **Near Plane** | 0.1–0.2 | Lower = more accurate shadow at close range |
| **Resolution** | Custom: 2048 | Per flashlight; prevents thin wall resolution failure |
### MeshRenderer → Shadow Casting Mode
All wall, floor, and ceiling MeshRenderers must be set to **Cast Shadows: On**. The specific options and their behavior:

| Mode | Behavior | Use Case |
|---|---|---|
| **On** | Renders in shadow passes; visible normally | Walls, floors, ceilings — always use this |
| **Two Sided** | Casts shadows from both face normals | Use for thin planes where backface must occlude |
| **Shadows Only** | Not rendered normally, only in shadow pass | Invisible occluder planes — avoid in modular graybox |
| **Off** | Never casts shadows | Dynamic debris, small props only |

For a modular graybox where floor/ceiling pieces are flat planes, set them to **Two Sided** shadow casting. This ensures the downward-facing backface of the ceiling piece still casts a shadow from a floor 1 light below it, even if the face normal points upward.[^3]

***
## 3. Mesh Construction and Light Leakage
### Single-Sided vs. Double-Sided Geometry
Unity's URP Lit shader is single-sided (front-face only) by default. A primitive plane or cube used as a floor has normals pointing in one direction only. From the direction opposite the normal, the surface is transparent to both light rays and camera rays. This creates two distinct problems in a modular graybox:

1. **Rendering transparency**: A camera outside the building looking at the ceiling bottom sees through it if the normal points inward.
2. **Shadow transparency**: A light above the ceiling (or coming through from floor 1) is not blocked by the ceiling plane because the shadow pass only renders front faces of shadow casters. A plane's shadow map is only rendered from the side its normal faces.

**Fix**: Replace flat plane floors/ceilings with thin box geometry (0.1m minimum). This gives each face a distinct normal direction — top normal points up (floor surface), bottom normal points down (ceiling surface visible from below), and side normals point outward (edge fills).[^13]
### Backface Culling and URP Materials
The URP Lit shader has a **Render Face** property in the material inspector with three options:[^3][^4]

- **Front**: Default — only renders faces whose normals face the camera.
- **Back**: Only renders backfaces (rarely useful).
- **Both**: Renders both faces — equivalent to `Cull Off` in the shader. Use this for graybox rapid prototyping. Note that backface shading uses the flipped normal, so lighting on the backface will appear inverted — this is a limitation of the double-sided workaround, not a bug.

For final art assets, **never rely on double-sided materials** to compensate for modular geometry gaps. Model box geometry with explicit face normals instead.
### Normal Direction and the Shadow Pass
The shadow map render pass uses the mesh's vertex normals to determine which triangles to render for shadow casting. A triangle with a normal facing away from the light (dot product < 0) is back-face culled from the shadow pass. If your floor piece's bottom face normal points up (toward the ceiling, away from the floor 1 light below), the floor piece does **not** appear in the floor 1 light's shadow atlas, making it effectively transparent to that light's shadows.

**Immediate fix**: In the MeshRenderer Inspector, set **Cast Shadows → Two Sided**. This overrides the normal check for the shadow pass and forces the geometry to cast shadows from both face directions.
### Modular Piece Construction Rules
| Rule | Rationale |
|---|---|
| Make floors/ceilings box geometry ≥ 0.1m thick | Side normals plug visual gaps; top/bottom both face correctly |
| Overlap pieces at edges by 0.1m | Eliminates sub-millimeter gaps from floating-point transform alignment[^2] |
| Grid-snap all pieces during placement | Prevents submillimeter gaps from non-snapped placement |
| Ensure closed geometry (no open edges) | Open edges at corners allow light to "peek" through adjacent geometry seams |
| Set Cast Shadows = Two Sided on flat planes | Forces shadow occlusion from both directions if using flat geometry |

***
## 4. Rendering Layers — Setup for Floor Isolation
### What Rendering Layers Actually Do
Rendering Layers (not to be confused with Unity's GameObject layers) are bitmask tags applied to both Light components and MeshRenderer components. A light only illuminates a MeshRenderer if their Rendering Layer Masks share at least one common bit. This operates entirely independent of scene geometry — it is a software filter, not a physical occlusion system.[^14][^15]
### Step-by-Step Setup for a Two-Floor Horror House
**Step 1 — Enable Rendering Layers in URP:**
```
URP Asset Inspector
  → Lighting section
  → Click ⋮ (More) → Advanced Properties
  → Enable "Use Rendering Layers" checkbox
```


**Step 2 — Name Rendering Layers:**
```
Edit → Project Settings → Tags and Layers
  → Rendering Layers section
  → Layer 0: "Default"
  → Layer 1: "Floor1"
  → Layer 2: "Floor2"
  → Layer 3: "Shared"  (stairs, doorframes, player)
```

**Step 3 — Assign to MeshRenderers:**
```
Select all Floor 1 mesh pieces:
  MeshRenderer → Additional Settings → Rendering Layer Mask → Floor1

Select all Floor 2 mesh pieces:
  MeshRenderer → Additional Settings → Rendering Layer Mask → Floor2

Select structural/shared pieces:
  MeshRenderer → Additional Settings → Rendering Layer Mask → Shared
```

**Step 4 — Assign to Lights:**
```
Floor 1 practical light (sconce, lamp):
  Light → Rendering → Rendering Layers → [Floor1, Shared]

Floor 2 practical light:
  Light → Rendering → Rendering Layers → [Floor2, Shared]

Flashlight (player's spot light):
  Light → Rendering → Rendering Layers → [Floor1, Floor2, Shared, Default]
  (flashlight should illuminate everything it aims at)
```


**Step 5 — Custom Shadow Layers for structural occluders:**

If a floor 2 wall should cast a shadow from a floor 1 light (for correct visual occlusion) but not receive lighting from it:

```
Floor 1 light:
  Light → Shadows → Custom Shadow Layers → enable
  → Layer → Floor2 (or Shared)
```

This casts the shadow from floor 2 geometry without applying the floor 1 light's color/intensity to it.[^14][^16]
### What Rendering Layers Cannot Do
Rendering Layers do not affect post-processing volumes, bloom, ambient light, or sky contribution. A floor 2 mesh excluded from floor 1 light's Rendering Layers will still receive indirect GI from APV probes baked under floor 1 conditions, unless APV Rendering Layer Masks are also configured (see previous report, Section 4).[^10][^11]
### Performance Note
URP's Rendering Layer implementation stores the layer mask in a GPU texture channel. Using 1–8 layers requires one texture fetch. Adding a 9th–16th layer adds another texture fetch per fragment. Keep layers at 8 or below for the most efficient configuration.[^15][^17]

***
## 5. Recommended Shadow Settings for a 20m × 20m Interior, 2 Floors
### Why Interior Defaults Need Complete Overrides
Unity 6's default URP shadow quality is substantially lower than Unity 2022 defaults — the shadow resolution, distance, and soft shadow quality all default to lower values. For a horror game where darkness is gameplay-critical, every default must be explicitly overridden.[^12]
### Shadow Distance and Cascade Configuration
For a 20m × 20m footprint across 2 floors (approximately 24m from corner to corner, floor-to-ceiling at most 8–10m):

| Setting | Value | Reasoning |
|---|---|---|
| **Max Distance** | **20–25m** | Tight distance = dense texels. No shadow needed beyond the building footprint[^9] |
| **Cascade Count** | **1** | With directional light disabled (indoor), cascades waste atlas. If needed, use 2 at most |
| **Cascade 1 Split** | 50% of max (~10m) | Only matters if using 2 cascades; keeps nearest shadow crisp |
### Shadow Resolution Configuration
| Light Type | Atlas Contribution | Recommended Per-Light Resolution |
|---|---|---|
| Main Light (directional) | Own atlas (disable shadows) | Disable — it's indoor, irrelevant[^12][^18] |
| Flashlight (spot light) | 1 shadow map | Custom → **2048** (primary gameplay light) |
| Practical spot light (sconce, lamp) | 1 shadow map each | Custom → **1024** |
| Point light (ambient fill) | 6 shadow maps each | Disable shadows if possible; if required, use **512** |

**Additional Light Shadow Atlas size for a typical 2-floor setup** (1 flashlight + 4 spot sconces + 1 point):

```
Maps needed: 1(flashlight) + 4(sconces) + 6(point) = 11 shadow maps
Min atlas for 512×512 maps: 2048×2048 (holds 16 maps at 512)
Min atlas for 1024×1024 maps: 4096×4096 (holds 16 maps at 1024)
```

Set **URP Asset > Shadows > Additional Lights Shadow Atlas → 4096** minimum.[^9][^19]
### Soft Shadows
In Unity 6, check and override this explicitly on each light:
- **Light > Shadows > Soft Shadows Quality → Medium** (don't rely on "Use Pipeline Settings" — confirm the pipeline is not set to Low).[^12]
- For the flashlight: **High** quality. It's the most visible shadow source in the game.
### Summary Settings Block
```
URP Asset → Shadows
  Max Distance:              20
  Cascade Count:             1 (or 2 if directional light is used for fill)
  Depth Bias (global):       0.5
  Normal Bias (global):      0     ← tune per-light instead
  Additional Lights Atlas:   4096

Flashlight (Spot Light)
  Shadow Type:               Soft Shadows
  Soft Shadows Quality:      High
  Bias:                      Custom
  Depth:                     0.5
  Normal:                    0.4
  Near Plane:                0.15
  Resolution:                Custom → 2048

Practical Sconce (Spot)
  Shadow Type:               Soft Shadows
  Soft Shadows Quality:      Medium
  Bias:                      Custom
  Depth:                     0.8
  Normal:                    0.5
  Resolution:                Custom → 1024

Floor Fill (Point Light, if shadow-casting)
  Shadow Type:               Hard Shadows  ← Hard is cheaper for ambient fills
  Resolution:                Custom → 512
```

***
## 6. Definitive Diagnosis Order — The Actual Bug
Given your specific description ("band of glitchy texture along the edge, visible only from the outside"), run this five-step diagnosis sequence before touching any shadow settings:

**Step 1.** Enter **Wireframe mode** (Scene View top-left dropdown → Wireframe) and zoom to the seam. Overlapping edges = Z-fighting (Cause #1). Missing face or normal mismatch = Causes #2/#3.

**Step 2.** Temporarily set the material on both pieces to have **Render Face: Both**. If the "glitchy band" disappears, it was a backface culling issue (Cause #3) — the exterior face of a geometry piece was invisible.

**Step 3.** Move one piece up by 0.05m to create a deliberate gap and re-check from outside. If the artifact changes from flickering to a clean bright line, you have confirmed Z-fighting from coplanar faces.

**Step 4.** In the lighting panel, disable the floor 1 light. If the artifact disappears, it's a light/shadow cause (Causes #4/#5). If it remains, it's pure geometry (Causes #1/#2/#3).

**Step 5.** Only after confirming geometry is correct, investigate shadow bias and atlas resolution using the settings tables above.

The fastest definitive fix for a graybox: replace flat plane floor/ceiling primitives with thin box geometry (0.1m thick), overlap adjacent pieces by 0.1m at all edges, grid-snap all placements, and set Cast Shadows to **Two Sided** on all floor/ceiling renderers. This resolves Causes #1, #2, and #3 simultaneously without touching any rendering settings.

---

## References

1. [Has anyone noticed worse z-fighting in Unity 5? : r/Unity3D - Reddit](https://www.reddit.com/r/Unity3D/comments/38om9j/has_anyone_noticed_worse_zfighting_in_unity_5/) - If the planes are almost on top of eachother then you're going to see z-fighting with even a tight f...

2. [How can I get rid of these light bugs? I am using URP](https://www.reddit.com/r/Unity3D/comments/1nnloud/how_can_i_get_rid_of_these_light_bugs_i_am_using/) - This is a classic problem, address wall width or change light bias setting in urp settings or per li...

3. [I'm hoping there's an easy fix to one sided / disappearing textures?](https://www.reddit.com/r/Unity3D/comments/18jsuvu/im_hoping_theres_an_easy_fix_to_one_sided/) - You could just duplicate the mesh and rotate the game object 180 degrees rather than invert the face...

4. [Unity Basics: Triangle Winding, Culling Modes & Double ... - YouTube](https://www.youtube.com/watch?v=3WWKHt92XKQ) - ... culling mode with a double-sided material. In this video, I explain how to do so with the defaul...

5. [Troubleshooting shadows in URP - Unity - Manual](https://docs.unity3d.com/Manual/urp/shadows-troubleshooting-urp.html) - Adjust the shadow bias settings in URP. By adjusting the shadow bias values you can reduce or elimin...

6. [Troubleshooting shadows in URP - Unity - Manual](https://docs.unity3d.com/6000.1/Documentation/Manual/urp/shadows-troubleshooting-urp.html) - Adjust the shadow bias settings in URP · In the Shadows section of the Light component, ensure that ...

7. [What can be causing these artifacts on the shadows? : r/Unity3D](https://www.reddit.com/r/Unity3D/comments/1shhm9f/what_can_be_causing_these_artifacts_on_the_shadows/) - Adjust the shadow bias and/or make sure the mesh is actually fully welded like others have suggested...

8. [Fix Shadows in Unity | URP 2022 - YouTube](https://www.youtube.com/watch?v=ZCpQhpt2k6s) - If you are using Unity's default URP settings you may notice some poor shadow quality. This video wi...

9. [Configure shadow resolution in the Universal Render Pipeline](https://docs.unity3d.com/6000.1/Documentation/Manual/urp/shadow-resolution-urp.html) - To set the resolution of shadows from the main light, select URP Asset > Lighting > Main Light > Sha...

10. [Troubleshooting light leaks in Adaptive Probe Volumes ...](https://docs.unity3d.com/6000.3/Documentation/Manual/urp/probevolumes-troubleshoot-light-leaks.html) - Add a Volume to your scene and make sure its area overlaps the position of the camera. · Select Add ...

11. [Troubleshooting light leaks in Adaptive Probe Volumes in URP](https://docs.unity3d.com/Manual/urp/probevolumes-troubleshoot-light-leaks.html) - A light leak. Light leaks often occur when geometry receives light from a Light ProbeLight probes st...

12. [Why URP Shadows Look Worse in Unity 6 (And How to ... - YouTube](https://www.youtube.com/watch?v=JJUQZSnvK80) - Are your Unity 6 URP shadows looking worse than in Unity 2022? In this Unity tutorial, we'll explore...

13. [Troubleshooting lightmapping artifacts - Unity - Manual](https://docs.unity3d.com/6000.3/Documentation/Manual/troubleshooting-lightmapping-artifacts.html) - Light bleeding is caused by overlapping chart neighborhoods in the lightmap. This occurs when there ...

14. [Rendering Layers | Universal RP | 17.0.0](https://docs.unity.cn/Packages/com.unity.render-pipelines.universal@17.0/manual/features/rendering-layers.html) - The Rendering Layers feature lets you configure certain Lights to affect only specific GameObjects. ...

15. [Introduction to Rendering Layers in URP - Unity - Manual](https://docs.unity3d.com/6000.1/Documentation/Manual/urp/features/rendering-layers-introduction.html) - Keep the Rendering Layer count as small as possible. ... This is because when the Rendering Layers e...

16. [Enable Rendering Layers for Lights in URP - Unity - Manual](https://docs.unity3d.com/6000.4/Documentation/Manual/urp/features/rendering-layers-lights.html) - In the URP Asset, in the Lighting section, select Use Rendering Layers. How to edit Rendering Layer ...

17. [Introduction to Rendering Layers in URP - Unity - Manual](https://docs.unity3d.com/Manual/urp/features/rendering-layers-introduction.html) - ... Rendering Layers reaches 9, 17, 25, etc. This is because when the Rendering Layers exceed a mult...

18. [Configure shadow cascades - Unity - Manual](https://docs.unity3d.com/6000.3/Documentation/Manual/shadow-cascades-use.html) - In URP, configure shadow cascades using the Cascade Count property, and then configure the cascade s...

19. [Set the size of shadow atlases in URP](https://docs.unity.cn/6000.0/Documentation/Manual/urp/set-size-shadow-atlases.html) - Set the size of shadow atlases in URP. Universal RP renders all real-time shadows for a frame using ...

