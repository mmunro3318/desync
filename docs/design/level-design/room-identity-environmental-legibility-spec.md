# Room Identity and Environmental Legibility Spec

## Document objective
Define how rooms, thresholds, landmarks, lighting, and environmental cues should be authored so players can recognize spaces, build mental maps, communicate locations, and notice when the house has changed. This spec exists to make spatial horror readable enough to be frightening on purpose rather than merely confusing [file:12][file:13][file:14][file:15].

## Why this spec exists
The project’s core horror depends on manipulated navigation, false familiarity, and topology that can change when unobserved. That only works if the baseline world is legible enough for players to form trust before the game betrays it. If rooms are too bland, too visually uniform, or too semantically weak, then mutation stops feeling uncanny and starts feeling random [file:12][file:13].

## Core question answered
How should the environment be authored so players can reliably say “I know where I am,” “I know where I came from,” and later “that should not be here” [file:12][file:13]?

## Scope
This spec covers:
- room identity,
- landmark design,
- threshold readability,
- route legibility,
- environmental differentiation,
- lighting cues,
- sound-adjacent spatial support,
- and debug methods for evaluating readability.

## Out of scope
This spec does **not** define:
- final art style production sheets,
- full sound design implementation,
- anomaly runtime logic,
- enemy behavior,
- or house graph contracts.

It defines the human-readable layer that those systems rely on.

## Design thesis
The player should be able to build a mental map from repeated local truths. Spatial horror is strongest when the world first teaches stable rules and then violates them precisely. Legibility is therefore not the opposite of horror; it is the prerequisite for meaningful disorientation [file:12][file:15].

## Baseline principles

### 1. Every room should be nameable
Players should be able to refer to rooms with natural language such as “the narrow bathroom,” “the split landing,” or “the kitchen with the island,” not just “that other room” [file:13][file:14].

### 2. Every threshold should read
Doorways, archways, and open passages should communicate where they lead, how important they are, and whether they are primary or secondary route choices.

### 3. Every path should be reconstructable
Players should usually be able to retrace a recent route by reading landmarks, room proportions, threshold order, and local lighting.

### 4. Every mutation should violate an earlier promise
The environment should first establish stable expectations, then break one expectation cleanly enough that the player notices the betrayal.

### 5. Confusion should be authored, not accidental
When players are lost, the game should know why. Loss of orientation should emerge from a deliberate design beat, not from rooms being visually sloppy or interchangeable [file:12][file:15].

## Room identity model
A room’s identity should be composed from multiple cue families rather than one single gimmick.

### Identity cue families
- spatial proportion,
- threshold layout,
- dominant fixture pattern,
- landmark object or silhouette,
- lighting character,
- window condition,
- floor/wall material pattern,
- and circulation shape.

### Rule
At least two cue families should be strong enough that the room remains identifiable even if one changes under stress, darkness, or layer shifts.

## Identity tiers
Rooms should not all have the same strength of identity.

### Tier 1: Anchor rooms
These are highly memorable rooms that stabilize the player’s mental map.

Examples:
- entry hall,
- kitchen,
- stair landing,
- a major living room,
- or a large double-height node.

### Tier 2: Support rooms
These are clear but less dominant rooms that help route reconstruction.

Examples:
- office,
- bedroom,
- bathroom,
- utility room.

### Tier 3: Transitional spaces
These are halls, narrow connectors, vestibules, and short passages. They should still read clearly, but their identity comes more from adjacency and route order than from internal complexity.

## Anchor room rules
Anchor rooms matter because they create orientation trust.

### Every generated house should include
- at least one strong entry anchor,
- one strong vertical anchor such as stair hall or landing,
- one strong mid-house anchor,
- and at least one distal anchor deep in the path network.

### Anchor rooms should differ by
- scale,
- lighting tone,
- fixture arrangement,
- connector pattern,
- or landmark silhouette.

## Transitional space rules
Transitional spaces are especially important in this project because halls and connectors will later be manipulated heavily.

### Transitional spaces should communicate
- directionality,
- connector count,
- whether the path bends or continues,
- and whether the player is moving toward openness or enclosure.

### Useful transitional cues
- runner rug or floor strip,
- wall rhythm,
- repeating light spacing,
- side-door count,
- ceiling compression,
- or a visible terminus condition.

## Threshold legibility
Thresholds are not just collision gaps. They are promises about adjacency.

### A threshold should communicate
- opening type,
- expected destination scale,
- route priority,
- whether it is optional or mandatory,
- and whether it belongs to the current room more than the next one.

### Threshold cue tools
- frame shape,
- trim style,
- door type,
- width,
- elevation change,
- lighting contrast,
- partial sightline into destination,
- and object framing beyond the threshold.

This aligns with the project’s emphasis on rooms and thresholds as gameplay entities rather than generic art openings [file:12][file:13][file:14].

## Primary vs secondary routes
A room with multiple exits should not present them as equal unless that ambiguity is intentional.

### Primary route cues
- wider opening,
- stronger sightline,
- brighter value contrast,
- stronger landmark framing,
- cleaner circulation path.

### Secondary route cues
- narrower opening,
- offset location,
- partial occlusion,
- weaker light pull,
- or less direct floor alignment.

This matters because players must be able to remember not just rooms, but also the route hierarchy through them.

## Local landmark design
A local landmark is any environmental feature that helps a player verify identity and orientation inside or across rooms.

### Good landmarks
- central island,
- crooked chandelier,
- broken ceiling panel,
- boarded window cluster,
- single red chair,
- sloped attic wall,
- or sink wall with mirror.

### Bad landmarks
- generic clutter piles,
- random small props,
- details only visible from one precise angle,
- or repeated decorative motifs used everywhere.

### Rule
Landmarks should be visible, durable in silhouette, and tied to room logic rather than arbitrary decoration.

## Landmark distribution
Landmarks should exist at multiple scales.

### Scales
- macro landmarks: stairs, atrium, kitchen island, major window bank,
- room landmarks: bed niche, fireplace wall, bathroom vanity,
- path landmarks: bent hall end table, broken sconce, hanging beam,
- micro cues: wallpaper tear pattern, stain, picture frame cluster.

### Rule
Macro and room landmarks should carry the bulk of orientation. Micro cues are support, not primary orientation load-bearing features.

## Lighting as orientation support
Lighting should do more than set mood. It should help define room identity and route memory [file:12][file:13].

### Lighting can reinforce
- room category,
- room importance,
- route priority,
- occupancy feel,
- and whether the player is entering safety, exposure, or ambiguity.

### Good baseline uses
- warmer entry anchor,
- harsh bathroom top-light,
- broad diffuse kitchen light,
- dim segmented hallway pools,
- low side-lit bedroom feel,
- vertical emphasis at stairs.

### Rule
Do not make every room equally dark. Uniform darkness destroys map legibility and weakens the effect of later lighting-based horror beats.

## Contrast and value hierarchy
Environmental readability depends heavily on value contrast, not just color.

### Use value contrast to clarify
- path continuation,
- threshold framing,
- room depth,
- interactable silhouettes,
- and dominant fixture walls.

### Rule
The player should usually be able to parse the main navigable volume of a room within a brief glance, even under stress.

## Material and texture differentiation
Materials should support recognition without becoming loud theme-park coding.

### Good differentiation
- bathroom tile vs bedroom wood,
- kitchen backsplash and counter rhythm,
- utility concrete or unfinished textures,
- hall runner or worn corridor floor,
- wallpaper family shifts by zone.

### Rule
Use materials as a secondary identity layer. Room identity should not collapse if textures are swapped in another world layer.

## Shape language
Spatial silhouette matters as much as material.

### Useful shape traits
- long narrow rectangle,
- square compression,
- L-turn interior,
- split-level step,
- sloped ceiling,
- alcove bite,
- offset door placement,
- central blockage.

### Rule
At least some room identity should survive in grayscale blockout form. If a room is only recognizable after final dressing, it is underdesigned.

## Route memory design
Players reconstruct routes by sequence, not just by single rooms.

### To support route memory
- vary threshold order,
- vary left/right turn rhythm,
- vary compression-to-release pacing,
- place anchor rooms at meaningful route intervals,
- and ensure at least one memorable feature appears near decision points.

### Example
A route may be remembered as: entry hall, narrow right turn hall, bright kitchen opening, left past island, up stair landing. That is the level of cognitive chunking the environment should support.

## False familiarity design
The project specifically benefits from spaces that initially seem familiar and later feel wrong.

### Effective false familiarity requires
1. a baseline room identity,
2. repeated exposure to that room or route,
3. player confidence in remembering it,
4. a precise later violation.

### Good violations
- extra door,
- missing window,
- longer hall,
- stair landing on wrong side,
- sink wall mirrored,
- light source missing,
- impossible repeated room,
- or anchor landmark displaced.

### Bad violations
- random clutter scatter,
- vague texture noise,
- lighting so dark nothing can be checked,
- or multiple unrelated changes at once that erase the original comparison.

## Environmental storytelling restraint
The house can carry emotional history, but readability comes first.

### Rule
Do not bury orientation cues under excessive clutter, hyper-dense set dressing, or overly busy storytelling details. A spatial horror house should feel lived-in enough to matter, but clean enough that the player can parse structure and notice change.

## Communication in co-op
Because the design may involve multiplayer, rooms and paths should support callouts.

### Good callout properties
- easy names,
- singular standout features,
- clear left/right references,
- recognizable thresholds,
- and low ambiguity between adjacent rooms.

### Bad callout properties
- three rooms that all look almost identical,
- indistinct short halls,
- multiple doors with no framing differences,
- or landmarks hidden behind furniture.

## Readability under pressure
Legibility must survive flashlight play, stress, and partial visibility.

### Test conditions
- low light,
- moving flashlight beam,
- sprinting past thresholds,
- partial obstruction by another player,
- and quick backward glances.

### Rule
A room’s dominant orientation cues should still register in fragments. Players often identify spaces from partial glimpses, not leisurely study.

## Readability across world layers
If the same structural house appears in multiple layers or overworlds, the identity system should persist across them.

### Preserve across layers
- core room silhouette,
- threshold pattern,
- anchor landmark placement or its deliberate absence,
- circulation logic,
- and at least one stable recognition cue.

### Allow to vary
- texture family,
- lighting mood,
- object state,
- contamination level,
- secondary decor.

This lets players experience “same place, wrong world” rather than “different place entirely.”

## Debug and evaluation tools
Environmental legibility should be tested, not guessed [file:12][file:13][file:15].

### Useful debug views
- room-id label overlay,
- threshold-id overlay,
- route-path trace,
- anchor-room highlight,
- landmark annotation mode,
- sightline cone previews,
- and grayscale/value preview capture.

### Useful playtest questions
- could the player name the room,
- could the player explain how they got there,
- could the player describe two distinguishing features,
- did the player notice the mutation,
- and was the confusion attributable to intended design rather than baseline unreadability.

## Evaluation heuristics
A room/environment pass should be considered legible when most testers can:
- identify anchor rooms reliably,
- describe routes using room sequence and landmarks,
- distinguish primary from secondary exits,
- notice intentional mutations,
- and recover orientation after a short disruption.

## Graybox-first implications
This spec should influence graybox work immediately, not only final art [file:12][file:15].

### Graybox must already communicate
- room scale differences,
- connector hierarchy,
- major landmarks,
- lighting blocks,
- and route structure.

If legibility is postponed to late art polish, the level will be much harder to repair.

## Acceptance criteria
This spec is successful when:
- most rooms are naturally nameable and distinguishable [file:13][file:14],
- anchor rooms stabilize the player’s mental map,
- thresholds communicate route hierarchy and adjacency clearly,
- local landmarks support both solo recognition and co-op callouts,
- room identity survives low-light and partial-visibility conditions,
- false-familiarity mutations are noticeable because the baseline environment was trustworthy,
- and graybox layouts can already be evaluated for orientation quality before final art [file:12][file:15].

