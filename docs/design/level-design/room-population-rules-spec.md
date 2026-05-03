# Room Population Rules Spec

## Document objective
Define how rooms receive fixtures, furniture, clutter, props, lights, anchors, and interactables after the structural shell and room-node layers are established. This spec exists to ensure room population is systematic, readable, replayable, and compatible with both navigation clarity and future anomaly systems [file:12][file:13][file:14][file:15].

## Why this spec exists
The project now has a baseline shell layer, a room-node layer, and a room-legibility layer. The missing bridge is a clear population system that can turn those abstractions into playable rooms without hand-placing every object or collapsing into chaotic random decoration. Population rules need to preserve circulation, support room identity, expose gameplay anchors, and leave space for later world-layer and anomaly variation [file:12][file:13][file:14].

## Core question answered
How should the game populate a room so it feels believable, readable, gameplay-ready, and later mutable, without requiring fully bespoke scene dressing for every instance [file:12][file:15]?

## Scope
This spec covers:
- fixture placement rules,
- furniture placement rules,
- clutter distribution,
- light population,
- interaction and anchor placement,
- room-category-specific guidance,
- variation controls,
- and debug validation for populated rooms.

## Out of scope
This spec does **not** define:
- final hero prop modeling,
- hand-authored narrative set dressing,
- enemy AI placement logic,
- anomaly runtime execution,
- or final production art polish.

It defines the rules layer that those later passes can build on.

## Population philosophy
Population should follow the broader project architecture: content should mostly come from data and clear rules rather than bespoke engine rewrites or one-off scene hacks [file:12][file:13][file:14][file:15]. A populated room should feel intentional, but the intention should come from constraints, tags, and weighted choices rather than manual labor every time.

## Population layer order
Population should happen in stable passes rather than all at once.

### Recommended pass order
1. Structural truth from shell and room node.
2. Fixture pass.
3. Primary furniture pass.
4. Secondary furniture/support pass.
5. Lighting pass.
6. Interaction and gameplay anchor pass.
7. Clutter and surface dressing pass.
8. Readability validation pass.
9. Variation/anomaly reservation pass.

This order matters because clutter should never determine circulation, and a side table should never invalidate a doorway that a fixture pass already claimed.

## Population ownership model
To avoid a giant “decorate everything” system, responsibilities should stay narrow, consistent with the project’s architecture principles [file:13][file:14][file:15].

| System | Owns | Does not own |
|---|---|---|
| `RoomPopulationDefinition` | Category-level rules and weights | Runtime scene instances |
| `RoomPopulationProfile` | Specific room instance preferences/overrides | Global generation orchestration |
| `FixturePlacementPass` | Semi-structural elements | Loose clutter |
| `FurniturePlacementPass` | Major room occupancy objects | Tool/gameplay logic |
| `LightingPlacementPass` | Baseline room lights and linked anchors | Final scare events |
| `AnchorPlacementPass` | Evidence, ghost, interaction, pickup, and debug anchors | Furniture aesthetics |
| `ClutterPlacementPass` | Secondary dressing and small props | Structural readability decisions |
| `PopulationValidator` | Circulation, overlap, and rule compliance | Generation policy |

## Source inputs
A population pass should read from:
- shell structure,
- room-node template,
- room category,
- room size/shape,
- connector positions,
- circulation zones,
- identity cues,
- and any profile overrides for this instance.

That keeps the room population grounded in actual room logic rather than random prefab scattering.

## Population outputs
A successful room population pass should produce:
- fixtures,
- major furniture,
- support furniture,
- lights,
- interaction points,
- anchor placeholders,
- clutter,
- and validation/debug metadata.

These outputs should be attachable to predictable scene hierarchy groups inside the room instance, consistent with the project’s preference for stable structure [file:13][file:14].

## Zone-first rule
Population should be zone-driven. Objects should not search the whole room indiscriminately.

### Zone families
- fixture zones,
- furniture zones,
- circulation zones,
- focal zones,
- clutter zones,
- interaction zones,
- and anomaly reservation zones.

### Rule
If a room lacks the right zones, the solution is to improve room-node data rather than writing special-case placement hacks.

## Fixture pass
Fixtures are the first non-structural population layer because they define the room’s practical logic.

### Fixtures include
- counters,
- sinks,
- toilet and tub placements,
- built-in shelves,
- wardrobes if treated as semi-structural,
- stair rails,
- fixed vanities,
- and major wall-mounted units.

### Fixture rules
- Fixtures should respect wall orientation.
- Fixtures should not block connectors.
- Fixtures should support room identity.
- Fixtures should establish plausible use-patterns before decorative objects appear.
- Fixtures should often act as room landmarks, especially in kitchens, bathrooms, utility rooms, and offices.

## Primary furniture pass
Primary furniture defines the room’s occupancy and silhouette.

### Primary furniture includes
- beds,
- sofas,
- dining tables,
- desks,
- bookcases,
- large cabinets,
- major chairs or seating clusters,
- and freestanding islands where applicable.

### Primary furniture rules
- One dominant furniture composition per room should be favored over many equally weighted objects.
- Furniture should reinforce the room’s callout identity.
- Furniture should preserve main circulation paths.
- Furniture should frame thresholds and sightlines intentionally, not randomly.
- At least one orientation-readable feature should remain visible from a likely entry point.

## Secondary furniture pass
Secondary furniture supports plausibility and variation without defining the whole room.

### Examples
- side tables,
- nightstands,
- stools,
- extra shelving,
- benches,
- coat racks,
- low storage,
- and support chairs.

### Rule
Secondary furniture should never undermine the readability created by fixtures and primary furniture. If a room starts feeling crowded or ambiguous, secondary furniture should be reduced first.

## Clutter pass
Clutter should communicate use and history, not just “fill space.”

### Good clutter categories
- surface clutter,
- shelf clutter,
- floor clutter in low-traffic zones,
- wall dressing,
- utility debris,
- soft goods,
- and neglected-object clusters.

### Clutter rules
- Clutter belongs to support zones, not circulation routes.
- Clutter density should vary by room type.
- Clutter should not become the primary identity cue unless intentionally used as a major landmark.
- Small clutter should come last and be easiest to disable for clarity or performance.

## Circulation preservation
Circulation is non-negotiable for both gameplay and readability [file:12][file:15].

### Population must preserve
- connector clearance,
- main route continuity,
- key turning radii,
- first-person collision sanity,
- and visual confirmation that a route remains traversable.

### Rule
Any placement pass that breaks circulation should fail validation rather than silently accept bad output.

## Threshold clearance rules
Thresholds need explicit breathing room because they are navigation promises and later mutation sites [file:12][file:13].

### Reserve around thresholds
- a clear interaction zone for doors,
- player passage width,
- sightline allowance into or out of the next space,
- and enough clean wall area that the threshold reads from multiple angles.

### Rule
Do not place “flavor” props adjacent to thresholds if they make doors unreadable or make rooms feel accidentally similar.

## Light population rules
Lighting is both atmosphere and orientation support [file:12][file:13].

### Light layers
- structural room light,
- task/accent light,
- local practical light source,
- and optional anomaly-reserved light positions.

### Baseline rules
- Most rooms should have one clear dominant light logic.
- Light placement should reinforce room category and route priority.
- Halls and landings should support path reading.
- Rooms should not all share the same fixture cadence.
- Some lights may intentionally be broken or absent, but absence should still read as authored.

## Interaction and anchor placement
Populated rooms should remain gameplay-ready.

### Anchor types
- evidence anchors,
- ghost interaction anchors,
- pickup spawn anchors,
- inspection anchors,
- safe interactables,
- debug anchors,
- and optional future anomaly anchor points.

### Rule
Gameplay anchors should be integrated with room logic, not floating arbitrarily. If an evidence anchor exists, it should feel like it belongs to the room’s surfaces, fixtures, or behavioral affordances [file:13][file:14].

## Anchor hierarchy
Not every room should support every anchor equally.

### Example distribution
- kitchens and utility rooms may support more fixture-linked anchors,
- bedrooms may support bedside or dresser anchors,
- halls may support fewer resting-surface anchors but more route-event anchors,
- bathrooms may support mirror/sink/light-linked anchors,
- stair halls may support vertical event anchors.

This keeps room behavior grounded in room semantics.

## Identity preservation rule
Population must preserve the room identity defined in the room-node and legibility specs.

### Population should reinforce
- dominant landmark,
- threshold hierarchy,
- room silhouette,
- route memory,
- and callout quality.

### Population should not do
- create three competing focal points,
- block the most important landmark,
- erase connector readability,
- or make two adjacent rooms accidentally interchangeable.

## Variation model
Population should support variation without sacrificing recognizability.

### Variation levers
- object selection,
- object count,
- arrangement choice within zone,
- material family,
- wear/decay state,
- clutter density,
- and optional room-use subtype.

### Rule
Variation should change expression, not destroy room identity. A kitchen without its counters, sink logic, or central landmark has stopped being a variation and become a different room.

## Population profiles
A room instance may use a profile to shift how the same room node is expressed.

### Example profiles
- inhabited,
- abandoned,
- overgrown,
- decayed,
- partially cleared,
- ritualized,
- mirrored-layer variant,
- and expedition-disturbed.

### Rule
Profiles should modify weights and allowed sets more often than they change core placement rules.

## Density control
Rooms need explicit density budgets.

### Density dimensions
- fixture density,
- primary furniture density,
- support furniture density,
- clutter density,
- and visual noise density.

### Rule
A high-density room still needs readable thresholds and at least one stable orientation cue. Density should never make all spaces equally chaotic.

## Room-category guidance
Different room types need different baseline logic.

### Halls and landings
- prioritize route clarity,
- keep clutter sparse,
- use walls and light rhythm as main cues,
- allow a small number of landmark props only.

### Kitchens
- fixture logic first,
- counters and sink define the room,
- island optional but identity-rich,
- leave multiple navigable lanes.

### Bathrooms
- strong fixture wall logic,
- compact but readable,
- mirror/light interactions important,
- clutter minimal to moderate.

### Bedrooms
- bed placement often dominant,
- support one or two secondary landmarks,
- maintain clean approach from door,
- allow stronger soft-clutter expression.

### Utility/storage
- denser support objects allowed,
- but preserve category silhouette,
- support strong material and fixture identity.

### Living rooms
- seating cluster or central landmark defines the room,
- thresholds should remain readable,
- support both open and more enclosed arrangements.

## Overworld and anomaly readiness
Population should leave deliberate slack for later anomaly systems.

### Reserve for future systems
- hidden wall-fill swaps,
- latent door positions,
- duplicate landmark variants,
- one-way visibility arrangements,
- stretch-safe corridors,
- and room-state overlays.

### Rule
Do not fully occupy every usable wall or floor segment. Some structural freedom should remain for later manipulations.

## Determinism and seeds
Population should be reproducible for debugging and multiplayer sync planning.

### Rule
Given the same room template, population profile, and generation seed, the same baseline placement result should be produced unless an explicit non-deterministic mode is enabled.

This matches the broader project preference for inspectable, tunable systems [file:12][file:13][file:15].

## Prefab and asset strategy
The existing project structure already separates environment prefabs, interactables, items, and data assets [file:13][file:14]. Population should respect that.

### Suggested asset split
- prefabs define reusable object setups,
- data assets define placement eligibility and tags,
- population rules choose among eligible sets,
- scene instances stay thin.

### Rule
If adding a new prop category requires rewriting the generator, the population system is too rigid.

## Debug requirements
Population needs strong debug visibility, because poor placement will otherwise look like bad art rather than bad rules [file:12][file:13][file:15].

### Debug views should show
- zone overlays,
- object provenance,
- placement pass ownership,
- blocked/failed placements,
- circulation paths,
- threshold clearance heatmaps,
- anchor categories,
- and density summaries.

### Useful toggles
- fixtures only,
- primary furniture only,
- clutter only,
- anchors only,
- grayscale readability view,
- and collision/clearance view.

## Validation rules
A populated room should fail validation when:
- a connector is obstructed,
- a required fixture is missing,
- a major zone is unfulfilled,
- collision overlap exceeds tolerance,
- route continuity breaks,
- landmark visibility is lost from expected entry positions,
- or visual noise exceeds density targets.

## Graybox-first recommendation
Version 1 should populate with graybox prefabs first, not final art [file:12][file:15]. That lets the system prove circulation, identity, and anchor logic before higher-cost asset work begins.

### Good first graybox set
- tables,
- shelves,
- sofa block,
- bed block,
- counters,
- sink placeholder,
- tub/toilet placeholders,
- light fixture placeholder,
- pickup surface markers,
- and debug landmark markers [file:13].

## Acceptance criteria
This spec is successful when:
- populated rooms read as plausible room types rather than random prop piles [file:12][file:13],
- fixtures and furniture preserve circulation and threshold clarity,
- room identity and callout quality are reinforced rather than diluted,
- gameplay anchors feel logically embedded in room semantics [file:13][file:14],
- variation changes expression without erasing recognizability,
- population remains data-driven and reusable rather than bespoke per room [file:12][file:14][file:15],
- and debug tools make placement failures inspectable early [file:12][file:13][file:15].

