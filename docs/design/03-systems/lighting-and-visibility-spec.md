# Lighting and Visibility Spec

## Purpose
This document formalizes lighting as both atmosphere and navigation support in the impossible-house prototype. Lighting is not just visual polish. It is part of spatial comprehension, emotional pacing, route readability, and anomaly communication.

The system must support horror mood without sacrificing player orientation, because the game’s core tension depends on players trying to build mental maps under unstable spatial conditions [file:12][file:13][file:15].

## Design goals
Lighting and visibility should:
- create dread and uncertainty,
- preserve enough readability for navigation and co-op communication,
- reinforce room identity,
- support multiplayer fairness,
- expose anomalies clearly when desired and obscure them when useful,
- and remain debuggable and tunable [file:12][file:13].

## Core principle
Darkness is a design tool, not a blanket solution.

If players cannot tell where they are, where the exits are, what objects matter, or what changed, the game loses the exact spatial-horror tension it wants to create. The player should feel uncertain about reality, not blind in a technically muddy scene [file:12][file:15].

## Lighting responsibilities
Lighting in this game has four simultaneous jobs:

| Responsibility | Description |
|---|---|
| Atmosphere | Shapes dread, unease, isolation, and emotional tone |
| Navigation | Helps players read paths, thresholds, room boundaries, and landmarks |
| Interaction Readability | Makes doors, switches, props, seals, and threats readable |
| State Communication | Signals world-layer shifts, anomaly pressure, danger escalation, and safe zones |

If a lighting choice serves atmosphere but damages the other three roles too heavily, it should be revised.

## Visibility hierarchy
Every playable space should support a visibility hierarchy.

### Tier 1 — Route-critical readability
Must remain readable almost all the time.

Includes:
- doorways,
- corridor turns,
- stairs and landings,
- major thresholds,
- primary cover/obstruction edges,
- critical interaction points,
- extraction/safe boundary cues.

### Tier 2 — Recognition anchors
Should usually be readable enough to support room memory.

Includes:
- major landmark props,
- dominant wall features,
- distinctive furniture silhouettes,
- room-center massing,
- major windows or blocked windows,
- signature ceiling fixtures.

### Tier 3 — Atmospheric detail
May fall into partial darkness without breaking play.

Includes:
- clutter,
- wall residue,
- secondary décor,
- small surface items,
- cosmetic distress.

This hierarchy prevents the common horror mistake where important information disappears under the same darkness budget as decorative detail.

## Lighting layers
Treat lighting as layered information.

### 1. Base architectural light
Defines general space legibility.

Examples:
- low ambient fill,
- indirect corridor bounce,
- soft ceiling fixture contribution,
- moonlight/window spill,
- low-level floor bounce.

Role:
- prevent total visual collapse,
- preserve spatial massing,
- help players read room shape.

### 2. Navigation light
Supports route reading.

Examples:
- doorway rim light,
- stair tread visibility,
- hall-end light cues,
- edge light near major turns,
- contrast separation between open path and dead-end clutter.

Role:
- support movement decisions,
- reduce cheap collision frustration,
- make co-op callouts more reliable.

### 3. Landmark light
Supports recognition anchors.

Examples:
- table pool of light,
- piano silhouette light,
- bed wall light,
- shrine focal glow,
- emergency sign accent.

Role:
- help the player remember rooms,
- signal “this place matters.”

### 4. Systemic/interactable light
Supports gameplay interaction.

Examples:
- readable switch plate,
- artifact glow,
- fuse box status light,
- key item silhouette pop,
- interactable prompt anchor exposure.

Role:
- prevent interaction hunting from becoming pixel-search noise.

### 5. Threat/anomaly light
Communicates danger or reality instability.

Examples:
- flicker fields,
- color-temperature shift,
- overbright doorway,
- swallowed darkness pocket,
- unnatural directional source,
- vanishing light in an observed corridor.

Role:
- create tension,
- mark state changes,
- support impossible-space storytelling.

## Room identity through lighting
Each room archetype should have a baseline light signature.

A signature can be defined through:
- dominant light source position,
- brightness envelope,
- contrast profile,
- color temperature range,
- silhouette behavior,
- and landmark emphasis.

Examples:
- hallway: elongated guidance and threshold rhythm,
- bedroom: low warm local pools around bed/side furniture,
- bathroom: harsher reflective feel with strong sink/mirror zone,
- living room: broader, layered pools with central furniture anchor,
- storage: partial occlusion and fragmented visibility.

This is important because room light signatures help the player recognize when a “same” room is not actually the same [file:12].

## Visibility support for impossible geometry
Since the game relies on spatial distortion, lighting must help players reason about changed geometry without over-explaining it.

### Use lighting to support these reads
- a corridor is longer than it should be,
- a doorway now leads somewhere wrong,
- a room has substituted but keeps partial identity,
- a loop has reset,
- another world-layer is bleeding through,
- an unobserved space changed state.

### Techniques
- preserve a known landmark light in a wrong context,
- shift doorway framing light while keeping adjacent space familiar,
- change depth cue distribution along a corridor,
- alter far-end contrast or vanishing-point brightness,
- keep local room signature but corrupt the transition zone.

## Multiplayer lighting rules
Lighting has to remain fair and consistent enough for co-op play.

### Rules
- critical route readability should not depend on one player’s private post-process state,
- major anomaly lighting states should replicate or resolve consistently when gameplay-relevant,
- player-carried lights must behave predictably across clients,
- flicker or failure states tied to gameplay should have authoritative ownership,
- and visibility-dependent mechanics should not rely on a client-only visual illusion if it affects shared truth.

In short: atmospheric presentation can vary locally in small ways, but gameplay-relevant light state must have coherent authority boundaries.

## Player-carried light rules
The flashlight or carried light is both comfort and trap.

### It should do the following
- give reliable close-range inspection,
- restore confidence in immediate interaction spaces,
- sharpen silhouettes in front-facing play,
- and create dependency that makes power loss meaningful.

### It should not do the following
- completely flatten atmospheric contrast,
- erase room light identity,
- function as the only navigation readability source,
- or produce so much noise/shadow instability that multiplayer visibility becomes confusing.

## Safe zones and refuge lighting
If safe or lower-threat zones exist, their lighting should communicate psychological relief without becoming visually disconnected from the rest of the house.

Traits might include:
- steadier light behavior,
- more stable contrast,
- clearer threshold cues,
- reduced flicker volatility,
- and stronger recognition anchors.

This helps the player feel the difference between threatened and stabilized space.

## Darkness budget
Every room should have a darkness budget, not unlimited darkness.

Questions to ask per room:
- Can the player identify exits?
- Can the player identify the dominant landmark?
- Can the player detect major obstructions?
- Can the player spot interactive props without tedious scanning?
- Can two players verbally coordinate what they are seeing?

If the answer is no, the room is probably too dark or too visually noisy for this game’s design goals.

## Indoor lighting guidelines
For the prototype, favor readability-first indoor lighting discipline.

### Do
- maintain low-level fill that preserves room shape,
- use stronger contrast at thresholds and landmarks,
- create predictable brightness around interaction points,
- make vertical transitions readable,
- and support silhouette clarity at player height.

### Avoid
- uniformly muddy darkness,
- hyper-contrasty black void corners with gameplay content inside them,
- excessive tiny practicals that create noisy light speckle,
- over-flickering that destroys depth perception,
- and lighting setups that depend on perfect player gamma settings.

## Practical handling of light leaks
The project already has history with a floor-to-floor light leak artifact in a test house, which means lighting setup must be treated as a system design concern rather than a polish problem [file:13][file:15].

### The spec should enforce these checks
- floors and ceilings must have real thickness when needed,
- room shell modules should avoid paper-thin geometry where shadow separation matters,
- light sources should be audited for range and shadow settings,
- baked vs realtime assumptions should be explicit per light family,
- light culling and shadow bias values should be reviewed,
- and vertical adjacency between floors must be tested early.

### Rule
Do not “solve” light leaks with random darkness. Fix geometry, light ranges, shadow settings, or authoring discipline first.

## Lighting state model
Lighting should expose explicit state categories for systemic control.

Suggested states:
- `Stable`
- `Dimmed`
- `Flickering`
- `Failed`
- `Anomalous`
- `Safe`
- `ObjectiveActive`

These may exist per room, per fixture group, or per world layer. What matters is that light state is queryable, debuggable, and not buried in scene-only animation hacks [file:12][file:13].

## Fixture grouping
Lights should be grouped intentionally.

Example groups:
- room-base,
- corridor-chain,
- stairwell,
- landmark-accent,
- interactable-highlight,
- emergency-failsafe,
- anomaly-override.

Grouping matters because:
- it makes scripted transitions coherent,
- enables multiplayer replication boundaries,
- and reduces scene authoring chaos.

## Visibility and prop coordination
Lighting and population systems must cooperate.

### Coordination rules
- landmark props should receive enough silhouette support to remain recognizable,
- obstruction props must not vanish into unreadable darkness if they affect pathing,
- interactive environmental props require visibility support stronger than decorative clutter,
- and world-layer prop swaps should be considered alongside lighting signatures.

This is one reason the prop taxonomy includes `landmarkScore`, `obstructionScore`, and `lightingAffinity` metadata.

## Rendering and post-processing discipline
Post-processing should support the visibility hierarchy, not collapse it.

### Use carefully
- exposure control,
- bloom on targeted highlights only,
- vignette for peripheral pressure,
- film grain for texture,
- color grading for world-state tone shifts,
- fog/volumetrics only where depth still reads cleanly.

### Avoid
- crushing blacks until geometry disappears,
- bloom that erases threshold readability,
- color grades that make all rooms feel samey,
- and fog that destroys corridor legibility in a navigation-driven horror game.

## Debug requirements
Lighting work is not complete unless developers can inspect:
- current room light state,
- active fixture groups,
- light intensity ranges,
- visibility tier coverage,
- dark-zone warnings,
- obstruction readability warnings,
- anomaly-driven overrides,
- and multiplayer-relevant lighting authority state [file:12][file:13].

Useful debug views:
- route visibility overlay,
- interactable visibility overlay,
- landmark silhouette check,
- floor-to-floor leak test scene,
- client vs host light state comparison.

## MVP scope
For the earliest vertical slice, lighting does not need full production horror polish. It does need disciplined support for navigation and anomaly readability.

MVP priorities:
- readable hallway and threshold lighting,
- stable room signatures for 4–6 core room archetypes,
- basic player flashlight behavior,
- one anomaly-lighting response pattern,
- one safe/steady lighting mode,
- and debug visibility for light states.

## Acceptance criteria
This spec is successfully implemented when:
- route-critical geometry remains readable in all core playable spaces,
- room light signatures help distinguish archetypes,
- interactive props remain visible enough for clean play,
- major anomalies can be signaled through lighting without destroying navigation,
- gameplay-relevant lighting states are grouped and queryable,
- vertical light leakage is actively tested and constrained,
- and lighting debug tools make failures diagnosable rather than mysterious [file:12][file:13][file:15].