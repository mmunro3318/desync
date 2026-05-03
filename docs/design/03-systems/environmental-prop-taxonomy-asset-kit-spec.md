# Environmental Prop Taxonomy / Asset Kit Spec

## Purpose
This document defines the environmental prop families, authoring tags, prefab kit rules, and placement metadata that the house population system can consume. The goal is to make room dressing modular, legible, and systemic instead of hand-placed chaos.

This spec treats props as both:
- atmosphere-building content,
- and navigational/readability tools inside an impossible-house horror game.

The system should prefer structured authoring over ad hoc decoration, because scene clutter becomes impossible to tune once rooms can mutate, substitute, or appear under alternate world states [file:12][file:13][file:14].

## Design goals
The prop taxonomy must support the following goals:
- give each room a readable identity,
- support procedural or semi-procedural population,
- preserve navigation legibility under anomaly pressure,
- allow world-state variants to swap or degrade room dressing coherently,
- and expose clean tags for debug and authoring [file:12][file:13].

Props should not merely fill empty space. They should help answer one or more of these questions:
- What kind of room is this?
- What routes, affordances, or obstructions matter here?
- What changed between states?
- What is safe to ignore versus important to inspect?

## Core principles
- Room identity comes first; prop density comes second [file:13].
- Navigation readability beats realism when the two conflict [file:12].
- Props should reinforce spatial memory, not erase it.
- Population rules should be data-driven and tag-based rather than hardcoded per room.
- The same base room can be dressed differently across world layers, but its recognition anchors should persist unless the anomaly explicitly targets recognition.
- Any systemically relevant prop family must expose debug-visible tags and category metadata [file:12][file:13].

## Authoring model
Each placeable environmental object should be described across four layers:

| Layer | Purpose |
|---|---|
| Prefab Family | Broad reusable class of prop, such as seating, shelving, clutter cluster, wall feature |
| Prop Definition | Data asset describing semantics, tags, allowed rooms, and variant rules |
| Prefab Variant | The actual prefab used in scene/runtime population |
| Placement Rule | The population constraints that determine where and when it may appear |

This follows the same runtime-vs-definition split used elsewhere in the project: authoring data describes what a prop is allowed to be, while room population runtime decides what actually appears in a specific instance [file:12][file:13][file:14].

## Prop family taxonomy
The population system should operate on prop families first, then pick concrete prefabs or clusters from inside those families.

### 1. Structural props
These reinforce the room shell or suggest architectural intent.

Examples:
- door trim,
- baseboards,
- vents,
- radiators,
- built-in cabinetry,
- arch details,
- ceiling fixtures,
- support columns,
- stair rails.

Rules:
- usually authored as part of room modules or socketable shell add-ons,
- low mutation frequency,
- often persistent across world layers,
- should rarely be random clutter.

### 2. Landmark props
These are high-recognition anchors that help players remember a room.

Examples:
- piano,
- grandfather clock,
- shrine cabinet,
- large aquarium,
- dining table,
- hospital bed,
- wheelchair cluster,
- dramatic wall art,
- cracked mirror feature,
- unique bookshelf silhouette.

Rules:
- one or two per room at most,
- strongest contribution to room identity,
- may persist across variants even if textures/materials shift,
- should be intentionally chosen, not fully random.

### 3. Functional furniture props
These define ordinary use and support room classification.

Examples:
- bed,
- desk,
- dresser,
- sofa,
- side table,
- shelving,
- cabinet,
- sink unit,
- exam cart,
- lockers,
- dining chair set.

Rules:
- medium frequency,
- selected by room archetype,
- may be replaced by alternate-world equivalents,
- should respect walkability and sightline rules.

### 4. Secondary support props
These reinforce room purpose without defining it alone.

Examples:
- lamps,
- desk fans,
- laundry baskets,
- folded linens,
- wall hooks,
- standing coat rack,
- bulletin boards,
- medicine trays,
- laundry carts,
- TV stand accessories.

Rules:
- higher variety,
- medium-to-high mutation tolerance,
- useful for state-change signaling.

### 5. Surface clutter props
These create lived-in texture and subtle pattern differences.

Examples:
- books,
- cups,
- bottles,
- loose papers,
- cosmetics,
- tools,
- toys,
- boxes,
- pill containers,
- candles,
- food remnants.

Rules:
- should often be authored as curated clusters rather than many tiny singles,
- should be density-limited,
- should avoid overwhelming interaction readability,
- must not block traversal or produce noise that confuses key landmarks.

### 6. Wall and ceiling props
These shape silhouette and orientation.

Examples:
- framed art,
- signage,
- corkboards,
- hanging wires,
- ceiling stains,
- monitors,
- mounted shelves,
- exposed pipes,
- emergency lights,
- directional placards.

Rules:
- useful for navigation and route memory,
- excellent for world-state swaps,
- should align with room style kit.

### 7. Floor occupation props
These change movement feel and route reading.

Examples:
- rugs,
- boxes,
- toppled furniture,
- bags,
- floor fans,
- debris piles,
- puddle decals,
- wheelchairs,
- gurneys,
- trash heaps.

Rules:
- highest risk for frustrating navigation,
- require explicit clearance constraints,
- should be used carefully in multiplayer spaces,
- should never make routes feel accidentally blocked unless authored as intentional obstruction content.

### 8. Obstruction / tension props
These create route pressure, partial occlusion, or anxiety.

Examples:
- hanging sheets,
- privacy curtains,
- stacked boxes,
- fallen shelving,
- narrow squeeze clutter,
- half-open partitions,
- scaffold-like blockages.

Rules:
- should be sparse and intentional,
- can help horror pacing,
- must remain legible in low light,
- should be tagged as obstruction-bearing content for navigation logic.

### 9. Interactive environmental props
These are not inventory items, but matter for interaction or systemic response.

Examples:
- light switches,
- fuse boxes,
- valves,
- breaker panels,
- door chains,
- intercoms,
- radio sets,
- call buttons,
- generator pull cords.

Rules:
- must be tagged clearly for interaction systems,
- should not be hidden inside clutter noise,
- require consistent prompt anchor positioning,
- need stronger visibility/readability treatment than decorative props [file:13][file:14].

### 10. Narrative residue props
These imply history, decay, or identity fracture.

Examples:
- personal notes,
- torn posters,
- protest flyers,
- medication schedules,
- family portraits,
- ritual traces,
- restraint marks,
- identity fragments,
- repeated symbols,
- altered photographs.

Rules:
- can carry theme heavily,
- should be sparse and intentional,
- should support atmosphere without forcing exposition,
- may be used to signal world-state drift or anomaly escalation.

## Mandatory prop metadata
Every prop definition should expose structured metadata the population system can consume.

| Field | Purpose |
|---|---|
| `propId` | Stable authoring identifier |
| `displayName` | Human-readable editor label |
| `family` | Taxonomy family |
| `subtype` | Narrower classification |
| `roomArchetypesAllowed` | Allowed room types |
| `roomTagsPreferred` | Tags that increase selection weight |
| `worldLayersAllowed` | Which world states may use it |
| `rarityWeight` | Base selection weight |
| `landmarkScore` | How strongly it contributes to room recognition |
| `clutterScore` | Density contribution for budget rules |
| `obstructionScore` | Route/sightline impact |
| `interactivityType` | None, inspectable, toggle, stateful, objective-linked |
| `socketType` | Floor, wall, ceiling, surface, corner, doorway, free |
| `boundsProfile` | Footprint and clearance metadata |
| `lightingAffinity` | Wants key light, fill light, silhouette light, darkness-tolerant |
| `variantSetId` | Links to alternate prefabs/material variants |
| `canAppearDamaged` | Whether degraded variants are valid |
| `canAppearDisplaced` | Whether shifted/displaced placement is allowed |
| `debugTags` | Explicit debug-readable tags |

This mirrors the data-driven content approach already established in the project’s earlier architecture docs, where content should be tunable through assets rather than hidden in scene-only setup [file:12][file:13][file:14].

## Required tag groups
Use a limited, explicit tag vocabulary.

### Room role tags
Examples:
- `bedroom`
- `kitchen`
- `living-space`
- `bathroom`
- `study`
- `medical`
- `storage`
- `hallway`
- `entry`
- `stair`
- `service`
- `institutional`

### Mood/state tags
Examples:
- `normal`
- `neglected`
- `abandoned`
- `contaminated`
- `ritualized`
- `queer-domestic`
- `sterile`
- `flooded`
- `burned`
- `overgrown`

### Navigation tags
Examples:
- `landmark`
- `sightline-blocker`
- `route-edge`
- `door-adjacent`
- `corner-anchor`
- `centerpiece`
- `orientation-aid`
- `safe-cover`
- `avoid-near-threshold`

### System tags
Examples:
- `interactive`
- `population-core`
- `surface-clutter`
- `structural`
- `anomaly-reactive`
- `world-layer-variant`
- `objective-adjacent`
- `debug-important`

## Prefab family kit rules
Prop kits should be authored in families, not as disconnected one-off objects.

Each family should ideally include:
- 1–3 major prefabs,
- 2–6 medium variants,
- 3–10 clutter or accessory variants,
- at least one damaged or altered state if appropriate,
- and a clear material/style language.

Example family kits:
- bedroom kit,
- institutional hallway kit,
- bathroom decay kit,
- ritual residue kit,
- storage overflow kit,
- domestic living room kit,
- medical equipment kit.

## Prop population budgets
Each room should have explicit budget caps to prevent unreadable clutter.

### Suggested budget axes
- Landmark budget,
- functional furniture budget,
- support prop budget,
- clutter budget,
- obstruction budget,
- interactive prop budget.

### Example rule
A bedroom might allow:
- 1 landmark,
- 3–6 functional furniture placements,
- 2–5 support props,
- 3–10 clutter clusters,
- 0–2 obstruction props,
- 1–2 interactive props.

The exact numbers are tuning values, but the important rule is that population should consume budgets rather than endlessly roll decorative noise [file:13][file:14].

## Socket and placement model
Population should occur through socket-aware placement whenever possible.

Supported socket types:
- floor-center,
- floor-edge,
- wall-low,
- wall-mid,
- wall-high,
- ceiling,
- surface-small,
- surface-medium,
- surface-large,
- corner,
- threshold-adjacent,
- room-centerpiece.

Why this matters:
- keeps props from floating or colliding,
- improves deterministic authoring,
- enables room-module reuse,
- and makes alternate-world swaps cleaner.

## Recognition anchors
Every room archetype should define recognition anchors that should remain stable unless intentionally disrupted.

Examples:
- a bedroom keeps bed orientation or a dominant wall feature,
- a kitchen keeps major counter/sink massing,
- a hallway keeps its dominant wall rhythm and obstruction logic,
- a study keeps desk/bookshelf relationship.

The population system should not randomize away these anchors casually, because impossible-space horror still needs enough continuity for player memory to matter [file:12].

## World-layer variants
The same room may appear across multiple world layers. Props should support layered mutation through controlled variance.

Variant strategies:
- material swap only,
- clutter escalation,
- prop substitution within same family,
- displacement/rotation drift,
- damaged/degraded version,
- residue overlay,
- selective disappearance.

Rules:
- preserve silhouette anchors where possible,
- increase emotional distortion without destroying all recognition at once,
- and keep pathing valid unless the anomaly system intentionally changes route logic.

## Prop interaction with anomalies
Anomaly systems may use props in several ways:
- reinforce false familiarity,
- subtly contradict remembered room identity,
- signal world-layer shifts,
- narrow or widen perceived routes,
- mark that a space has been replaced or looped.

To support that, props should expose anomaly-reactive flags such as:
- `canDuplicate`
- `canDisappear`
- `canShiftSurface`
- `canMisalign`
- `canAppearTooMany`
- `canBecomeProxyLandmark`

Use these sparingly. Too much prop instability destroys navigational play instead of enhancing it.

## Population pipeline responsibilities

| System | Responsibility |
|---|---|
| Room Archetype Definition | Defines room purpose, budget profile, and required anchors |
| Prop Definition Assets | Define prop semantics and allowed contexts |
| Population Rule Set | Resolves tag matching, weights, and budget spending |
| Socket Map | Defines legal placement points in room modules |
| Runtime Population Pass | Instantiates selected props for a room instance |
| World Layer Variant Pass | Applies material/substitution/degradation changes |
| Debug Overlay | Shows tags, budgets, selection reasons, and blocked placements |

This keeps population logic modular and inspectable instead of being buried in scene decoration [file:12][file:13].

## Debug requirements
The population system is not complete unless developers can inspect:
- room archetype,
- active room tags,
- current world layer,
- budgets used/remaining,
- selected prop families,
- rejected candidates and why,
- active landmark anchors,
- and obstruction/collision warnings [file:12][file:13].

## MVP scope
For the earliest vertical slice, do not attempt the full taxonomy breadth.

Start with these families:
- structural shell props,
- landmark props,
- functional furniture props,
- wall props,
- clutter clusters,
- interactive environmental props.

Start with these room archetypes:
- hallway,
- bedroom,
- living room,
- bathroom,
- storage,
- stair landing.

That is enough to prove:
- room recognition,
- population variation,
- variant swapping,
- and navigation readability under low complexity.

## Acceptance criteria
This spec is successfully implemented when:
- prop definitions exist as structured data assets,
- room population consumes family tags and budgets rather than ad hoc random spawns,
- each room archetype preserves at least one recognition anchor,
- world-layer variants can alter dressing without destroying room identity,
- navigation-critical props expose obstruction and landmark semantics,
- interactive props remain readable inside populated rooms,
- and debug tooling can explain why each important prop was placed [file:12][file:13][file:14].
