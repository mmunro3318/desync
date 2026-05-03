# Run 3 — URP Graphics and Lighting for a Co-op Horror Prototype in Unity 6.4

## Overview
This report covers Unity 6.4 Universal Render Pipeline (URP) setup, rendering path selection, lighting strategy, shadow configuration, light-leak debugging, post-processing atmosphere, and reflection probes — all scoped to a first-person indoor co-op horror prototype using graybox-first development. It addresses Checklist C from the project research brief.[cite:280][cite:263][cite:260]

The central recommendation is: **use Forward+ rendering path, set up a mixed baked/real-time lighting strategy from early prototype phases, fix shadow defaults that Unity 6 ships with at low quality, add a Global Volume post-processing profile with horror-tuned overrides, place one reflection probe per room, and understand the four common light-leak causes and their fixes before spending hours trying to find them by trial and error**.[cite:240][cite:269][cite:280][cite:250]

## Executive Summary
Unity 6 URP ships with lower default shadow quality than Unity 2022 LTS, which surprises developers migrating from older projects. The shadow resolution defaults, shadow distance, and soft shadow quality all need explicit tuning for an indoor horror game that depends on deep, crisp shadows for atmosphere.[cite:240]

For an indoor horror prototype with multiple point lights and spotlights per room, the **Forward+ rendering path** is the correct default. It removes the eight-light-per-object limit that classic Forward imposes, supports unlimited per-pixel lights per camera, and is Unity 6's documented recommendation for multi-light scenes.[cite:280][cite:261][cite:279]

Post-processing is not a "polish later" concern for a horror game. Even on graybox geometry, a Global Volume with ambient occlusion, color grading, vignette, and film grain turns a grey box into a space with atmosphere and reveals how lighting and spatial design are actually reading to the player.[cite:250][cite:235]

Light leaks between floors and through walls are one of the most common and most frustrating lighting bugs in indoor Unity scenes. They are mostly caused by incorrect shadow bias settings, thin geometry, too-low shadow resolution on spot/point lights, or baked probe sampling artifacts — and each cause has a specific fix.[cite:244][cite:269][cite:241]

## URP project setup for a new horror prototype
### Creating the project with URP
Unity 6 projects created from the URP template start with a pre-configured URP Asset, Universal Renderer Asset, and Global Volume. Starting from the URP template rather than the Standard 3D template avoids the manual pipeline conversion work that can cause renderer confusion early in a project.[cite:263][cite:264]

Key first-configuration steps after creating a URP project:
- Set the Rendering Path in the Universal Renderer Asset to **Forward+**.[cite:280][cite:261]
- Open Project Settings > Graphics and verify the URP Asset is assigned as the Scriptable Render Pipeline Settings.[cite:263]
- Open the URP Asset and adjust the Shadows section (see Shadow Settings section below).[cite:240][cite:269]
- Verify that Post Processing is enabled on the Main Camera.[cite:253][cite:250]

### Choosing a rendering path: Forward+ for this project
Unity 6 URP offers Forward, Forward+, and Deferred rendering paths. Unity's own rendering-path comparison documentation describes the decision framework clearly.[cite:280]

| Rendering path | When to use | Real-time lights per object |
|---|---|---|
| **Forward** | Scenes with very few lights; mobile targets[cite:280] | 9 |
| **Forward+** | Multi-light indoor scenes; multiple reflection probes; this project[cite:280][cite:261] | Unlimited (256 per camera) |
| **Deferred** | Many lights, no Rendering Layers, not mobile[cite:280] | Unlimited for opaque |

For this project, **Forward+ is the correct choice** because:
- A horror house interior will routinely have multiple simultaneous point lights (practical lamps, flickering sources, player-held flashlight, creature-emitted glow).
- Forward+ eliminates the per-object eight-light cap that causes lights to silently drop out in classic Forward.[cite:261][cite:279]
- Unity 6 already makes Forward+ the default URP renderer in new projects.[cite:279]

Community experience confirms Forward+ is well-suited to scenes with dozens of real-time lights, with one developer noting they achieved "hundreds of fully dynamic and shadowed visible local lights" on a mid-range GPU.[cite:259]

## Shadow settings: fixing Unity 6's low defaults
### The problem
Unity 6 URP ships with shadow defaults that are notably lower quality than Unity 2022 LTS. The main light shadow resolution, additional light shadow resolution, shadow distance, cascade settings, and soft shadow quality all default to values that look noticeably bad for a close-camera horror game.[cite:240]

This is one of the most important first configurations to make, not a polish item.

### Recommended shadow settings in the URP Asset
The following settings should be set in the URP Asset's **Shadows** section:[cite:240][cite:269][cite:268]

- **Main Light Shadow Resolution**: 4096 (Unity 6 defaults to much lower).[cite:240]
- **Additional Lights Shadow Resolution**: 2048 minimum; increase per-light if leaks persist.[cite:241][cite:240]
- **Shadow Distance**: 50–150 m. Keep as low as the gameplay allows; lower distance = higher cascade resolution near the camera.[cite:267][cite:265]
- **Cascade Count**: 4. Use shadow cascades for the directional light to preserve near-camera shadow quality at acceptable shadow distance.[cite:268][cite:267]
- **Soft Shadows**: Enable. Set Soft Shadow Quality on the directional light from Low to Medium or High.[cite:240]
- **Depth Bias / Normal Bias**: These control shadow acne and peter-panning. The Unity 6 troubleshooting docs recommend tuning per-light using Custom bias rather than the global default when specific lights are causing issues.[cite:269]

### Shadow bias explained simply
Two shadow artifacts compete with each other, and bias settings balance between them:[cite:269][cite:271]

- **Shadow acne**: self-shadowing noise on surfaces because the shadow map samples itself incorrectly. Fixed by increasing Depth Bias.
- **Peter-panning**: shadows detach and float from their casters. Caused by too-high Depth Bias.
- **Light leaking**: light bleeds through walls where it should be blocked. Often caused by too-high Normal Bias or too-low shadow resolution.[cite:269]

Start with defaults, identify which artifact is visible, and tune the per-light Custom bias value until both acne and leaking are acceptable. Do not touch global bias until you understand how the per-light setting works first.[cite:269]

## Light-leak debugging and prevention
### Why light leaks happen in indoor scenes
Light leaks between floors, through walls, and under doors are one of the most common and most discussed problems in indoor Unity scenes. Community troubleshooting consistently identifies four main causes:[cite:241][cite:244][cite:269]

**Cause 1: Thin geometry**
If walls, floors, or ceilings are too thin relative to the shadow map sample spacing, lights can bleed through. The Unity APV troubleshooting docs explicitly recommend: "Create thicker walls" as the first fix for probe-based light leaks.[cite:244]
Fix: Make walls at least 0.2–0.3 m thick, especially where floor-to-ceiling light separation is critical.

**Cause 2: Shadow resolution too low for point/spot lights**
As a spot light's outer angle increases, the effective shadow map resolution decreases, causing leaks at the edges. This is a documented and common issue.[cite:241]
Fix: For each spotlight causing leaks, set shadow resolution to a Custom value (e.g., 2048) rather than relying on the global default.[cite:241]

**Cause 3: Shadow bias misconfiguration**
Too-high Normal Bias values cause the shadow receiver to shift inward, creating gaps between the shadow and geometry that read as light bleeding through.[cite:269]
Fix: Reduce Normal Bias incrementally on the offending light. Set the light's Bias to Custom so you can tune per-light without affecting the whole scene.[cite:269]

**Cause 4: Baked probe sampling bleeding**
With Adaptive Probe Volumes (APV), probes placed near walls can sample lighting from the "wrong" side of the wall, causing baked indirect light to bleed into adjacent rooms.[cite:244]
Fix: Use Rendering Layers to assign interior and exterior probes to separate masks, preventing interior surfaces from sampling probe data from outside. Alternatively, use a Probe Adjustment Volume to override probe influence in leaking areas.[cite:244]

**Directional light layer culling**
A directional light configured to affect all layers will cast through everything including floors. Use Culling Mask on the directional light to exclude indoor layers from receiving the directional light contribution, or simply disable the directional light for interior-only scenes.[cite:249]

### Practical debugging workflow
When a new light leak appears:
1. Identify whether it is a real-time shadow leak or a baked probe leak. Toggle between baked and real-time in the scene to isolate.
2. For real-time: check wall thickness, check the offending light's shadow resolution and bias.
3. For baked: use the Rendering Debugger to visualize probe placement. Look for probes straddling wall geometry.
4. Fix geometry thickness first — it is the fastest fix when applicable.
5. Apply per-light shadow resolution and bias changes next.
6. Use probe masking or Probe Adjustment Volumes only if geometry and bias fixes do not resolve the issue.[cite:244][cite:269][cite:241]

## Real-time vs. baked lighting strategy for prototype phases
### The choice
Unity supports real-time, baked, and Mixed lighting modes.[cite:260][cite:258] Each has a different cost-quality tradeoff:

| Mode | Cost | Best use |
|---|---|---|
| **Real-time** | Higher GPU cost; flexible[cite:258][cite:260] | Flashlights, flickering horror lights, player-influenced lights |
| **Baked** | Bake time cost; very cheap at runtime[cite:258][cite:260] | Static architectural bounce light; ambient fill |
| **Mixed** | Middle path; bakes indirect, keeps direct real-time[cite:260] | Dominant room fill lights that do not move |

Community practice for indoor games converges on a **hybrid approach**: bake the indirect bounce light and ambient fill that will not change, keep real-time shadows for the lights that move or flicker, and use Mixed mode for dominant fill lights where baked indirect plus real-time direct is the right tradeoff.[cite:258][cite:255]

### Prototype recommendation
For early prototype phases, use **fully real-time lighting** until the spatial layout and room design stabilize. Baking too early means rebaking after every room geometry change, which slows iteration speed dramatically.[cite:260][cite:235]

Introduce baked or mixed lighting when:
- Room layout is stable and unlikely to change in bulk.
- A specific room's performance budget is being exceeded by real-time light counts.
- The baked indirect bounce quality is noticeably adding to horror atmosphere.[cite:258][cite:260]

## Indoor lighting setup for horror
### Layer zero: scene-wide ambient darkness
Horror interiors should start from darkness, not from Unity's default bright skybox ambient. The first step is reducing environment ambient lighting to near zero or a very dark neutral value.[cite:235][cite:239]

In Lighting settings (Window > Rendering > Lighting):
- Set **Environment Lighting Source** to Color, not Skybox.
- Set ambient color to near-black (very dark cool grey).[cite:235][cite:239]

This ensures no surface is accidentally lit by sky ambient when it should be in shadow.

### Room fill lighting strategy
For indoor horror rooms, a useful layered lighting model is:[cite:239][cite:287][cite:235]

1. **Practical lights**: Point lights or spot lights placed at real-world light source positions (lamps, ceiling fixtures, windows). These are the "real" room lights.
2. **Fill light**: One low-intensity point light placed in the center of a room with no shadows, used to prevent the room from being completely pitch-black where practical lights do not reach. Kept subtle.
3. **Accent lights**: Small, colored, low-intensity point lights placed at dramatic positions to add interest and separation.
4. **Player flashlight**: A dynamic spot light attached to the player, casting real-time shadows. This is typically one of the most important dynamic lights in a horror co-op game.[cite:239][cite:287]

### Shadow-casting light count discipline
Every real-time shadow-casting light is expensive. A horror game with four simultaneous players, each carrying a flashlight, plus several room lights, can easily exceed budget without light management.[cite:284][cite:265]

Practical rules:
- Limit hard shadow-casting real-time lights per room to 3–4 maximum in prototype.[cite:284]
- Flashlights and player-held lights should cast shadows. Room fill lights generally should not.[cite:239]
- Flickering lights should update their shadow mask only on relevant frames; avoid per-frame shadow recalculation for lights with simple flicker math.[cite:284]

## Post-processing for horror atmosphere
### Why it matters from day one
Even on plain ProBuilder graybox geometry, a well-tuned post-processing Global Volume transforms how the space reads. Color grading, vignette, ambient occlusion, and film grain together create a mood that makes it possible to evaluate spatial layout, lighting, and scale accurately — not just technically verify that lights are on.[cite:250][cite:235]

Post-processing in URP is handled through **Volumes** — Global Volumes apply to the whole scene; local trigger-based Volumes apply within a defined bounds and blend when the camera enters them.[cite:251][cite:253]

### Setting up the Global Volume
The main steps are:
1. Create an empty GameObject, add a **Volume** component.
2. Check **Is Global**.
3. Click **New** to create a Volume Profile asset.
4. Add overrides for each post-processing effect.[cite:253][cite:257]

Note: Screen Space Ambient Occlusion (SSAO) in URP 6 is configured as a **Renderer Feature** on the Universal Renderer Asset, not as a Volume override. Add it there separately.[cite:250]

### Horror-tuned post-processing stack
A recommended base profile for graybox horror testing:[cite:250][cite:235][cite:243]

| Effect | Setting direction |
|---|---|
| **Tonemapping** | ACES or Neutral. Avoid Filmic unless you specifically want high contrast.[cite:250] |
| **Color Adjustments** | Reduce saturation (–10 to –30). Increase contrast slightly (+15 to +30). Lower post exposure to keep the scene dark.[cite:250] |
| **Split Toning** | Add blue-green tint to shadows, subtle warm tint to highlights. Classic horror palette.[cite:250][cite:243] |
| **Vignette** | Intensity 0.3–0.5. Smoothness 0.4. Rounds the player's view toward darkness.[cite:250] |
| **Film Grain** | Intensity 0.2–0.4. Type Thin. Adds texture and subconscious unease.[cite:250] |
| **Bloom** | Low threshold, low intensity. Allows practical lights to glow without blowing out.[cite:250] |
| **Chromatic Aberration** | Use sparingly (0.1–0.3) for distress states; not at baseline.[cite:250] |
| **Screen Space Ambient Occlusion** | Add as Renderer Feature. SSAO darkens crevices, corners, and contact shadows significantly — critical for horror mood.[cite:250] |

### Per-room volume triggers (Resident Evil approach)
Different rooms can have different post-processing atmospheres using local (non-global) Volumes with trigger colliders.[cite:251] When the player enters the volume, the profile blends in over a `Blend Distance`. This technique is widely used in horror games to shift color tone, increase vignette, or change ambient occlusion intensity per room.[cite:251]

For this project's impossible-house logic, this pattern maps naturally to the room graph: each room can carry its own Volume profile asset, and the impossible-space runtime can enable/disable or swap volume assets when rooms transition.[cite:251]

## Reflection probes and ambient fill
### Why reflection probes matter indoors
In URP, surfaces using PBR materials sample reflection from either the skybox or the nearest Reflection Probe. Without probes, metallic and glossy surfaces inside a dark building reflect the sky instead of the room they are in, which looks wrong and breaks immersion.[cite:285][cite:287][cite:291]

For an indoor horror prototype, one baked reflection probe per room (sized to fill the room's bounds) is the minimum viable approach.[cite:287][cite:291]

Setup steps:
1. Right-click in Hierarchy > Light > Reflection Probe.
2. Resize the probe's Box Volume to fill the room.
3. Enable Box Projection for more accurate box-space reflections.
4. Bake the probe (or set to Real-time for rooms with heavy dynamic lighting changes).[cite:285][cite:287][cite:291]

### Adaptive Probe Volumes (APV) vs. manual Light Probes
Unity 6 URP includes Adaptive Probe Volumes as a modern alternative to manually placed Light Probes for baked indirect lighting on dynamic objects.[cite:270][cite:272]

APV automatically generates probe density based on geometry, samples per-pixel rather than per-object, and produces better results than manual probe grids in complex indoor scenes.[cite:270][cite:272] However, it introduces the light-leak behavior described in the leak section above, and requires explicit geometry thickness and probe masking attention.[cite:244]

Recommendation for this project:
- **For prototype phase**: use manual Light Probes or skip probe-based indirect entirely (stay real-time).
- **Introduce APV** when baked lighting is appropriate and when geometry is stable enough to bake reliably.[cite:270][cite:272]

## Prototype-friendly vs. shipping-level visual practices
Not every graphics decision is a day-one concern. A useful distinction for this project is:

| Practice | Prototype phase | Shipping phase |
|---|---|---|
| URP Forward+ renderer | Day one[cite:280] | Day one |
| Fixed shadow defaults | Day one[cite:240] | Day one |
| Global Volume post-processing | Day one[cite:250] | Refined |
| Real-time lights only | Prototype start[cite:260] | Mixed/baked |
| Per-room Volume triggers | When rooms are stable[cite:251] | Yes |
| Baked lightmaps/APV | When layout is locked[cite:258][cite:270] | Yes |
| Reflection probes per room | Early (1 per room is cheap)[cite:287] | Yes, tuned |
| Volumetric fog | Optional, evaluate cost[cite:286][cite:284] | Optional |
| Shader Graph custom materials | Art pass only | Yes |
| LODs and occlusion culling | Mid-phase when profiling reveals need[cite:265] | Yes |

## Recommended defaults for this repo
### What Claude should generally do
- Set the Universal Renderer Asset rendering path to Forward+.[cite:280][cite:261]
- Increase main light shadow resolution to 4096 and additional lights to 2048 in the URP Asset.[cite:240]
- Set ambient environment lighting to near-black at project start.[cite:235][cite:239]
- Create a Global Volume with a horror-tuned post-processing profile from the first playable scene.[cite:250][cite:253]
- Add one baked reflection probe per room, sized to fill the room bounds.[cite:287][cite:291]
- When light leaks appear, check wall thickness and per-light shadow resolution before adjusting bias.[cite:241][cite:269]
- Set per-light shadow resolution to Custom (2048+) for any spotlight that is leaking through walls.[cite:241]
- Use Culling Mask on the directional light to prevent it from illuminating interior geometry.[cite:249]
- Use Screen Space Ambient Occlusion as a Renderer Feature, not as a Volume override in URP 6.[cite:250]

### What Claude should generally avoid
- Leaving Unity 6's low shadow quality defaults in place.[cite:240]
- Using classic Forward rendering when the scene has more than eight lights affecting any single object.[cite:261][cite:280]
- Assuming post-processing is polish-phase only — it is a core tool for evaluating horror spatial layout from day one.[cite:250][cite:235]
- Baking lighting before room geometry is stable — it wastes iteration time.[cite:258][cite:260]
- Relying on the default skybox ambient to light interior scenes.[cite:235][cite:239]
- Placing APV before understanding the wall thickness and probe masking requirements that prevent bleed.[cite:244]
- Adding volumetric fog in every room before profiling GPU cost.[cite:284][cite:286]

## Conclusion
URP configuration for an indoor horror prototype is not primarily about advanced graphics features — it is about correctly setting up the defaults that Unity ships with misconfigured, understanding how to compose indoor lighting from darkness rather than from ambient fill, and learning the four light-leak causes early enough to fix them before they cost hours.[cite:240][cite:269][cite:241]

The Forward+ rendering path, corrected shadow defaults, a horror-tuned Global Volume post-processing stack, one reflection probe per room, and disciplined real-time light budgeting give this prototype a strong visual foundation from the first graybox playtest — and keep lighting iteration fast while room design and spatial runtime logic are still being refined.[cite:280][cite:250][cite:287][cite:258]
