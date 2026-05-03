# Room Node Authoring Spec

## Document objective
Define how individual rooms are authored as reusable node templates that can plug into the generated house shell, preserve threshold contracts, support room identity, and later feed spatial anomaly systems. This spec establishes the room-node layer that sits between the baseline structural shell and later furnishing/population passes [file:12][file:13][file:14][file:15].

## Why this spec exists
The house shell DSL and builder pipeline solve large-scale structure, but the project also needs a consistent way to describe the internal logic of rooms: their connectors, semantic role, fixture expectations, furniture zones, sightline character, and anomaly compatibility. Without a room-node layer, the project risks either hand-authoring every room uniquely or pushing too much meaning into the coarse shell grid [file:12][file:13][file:14].

## Epic question answered
Can rooms be authored as modular, connector-aware, semantically meaningful building blocks that are reusable across house generation, understandable to AI tools, and stable enough to later support mutation and navigation systems [file:12][file:13]?

## Scope
This spec covers:
- room-node purpose and layering,
- connector and threshold contracts,
- room metadata,
- fixture and furniture zones,
- orientation rules,
- room identity guidance,
- anomaly compatibility hooks,
- and the relationship between authored room nodes and generated house shells.

## Out of scope
This spec does **not** define:
- final art asset creation,
- full furniture population algorithms,
- anomaly runtime behavior,
- complete procedural decoration systems,
- or the house shell importer itself.

Those systems depend on this layer but should remain separate.

## Position in the pipeline
The room-node layer sits **after** baseline shell generation and **before** final content population.

### Layer order
1. House Layout DSL defines gross structure.
2. House Builder Pipeline generates shell, rooms, and thresholds.
3. Room Node Authoring defines reusable room modules and local semantics.
4. Population systems place fixtures, furniture, clutter, lights, and anchors.
5. Runtime systems attach gameplay logic, mutation rules, and session content.

This layered approach follows the project’s broader rule of keeping content data separate from generated/runtime state [file:12][file:13][file:14].

## Room node definition
A room node is a reusable authored description of a room-scale play space. It is not just a prefab mesh and not just a label. It is a semantic and spatial module that declares:
- what kind of room it is,
- how it can connect,
- what local spatial rules it expects,
- where fixtures and furniture may exist,
- and what kinds of experiences it supports.

## Core room-node principles

### 1. Connector-aware
Every room node must explicitly define how it can connect to adjacent structure.

### 2. Semantically meaningful
A room node should express room identity beyond shape alone. It should know whether it is a kitchen, narrow hall, bathroom, closet, landing, bedroom, office, utility room, and so on [file:13][file:14].

### 3. Reusable, not generic sludge
A room node should be reusable across houses, but not so abstract that all rooms become forgettable.

### 4. Supports navigation legibility
Room nodes should help preserve room identity, threshold readability, and route memory, which are already core navigation goals for this project [cite:31][file:12][file:13].

### 5. Supports future mutation
A room node should carry enough metadata that later systems can reason about whether it can stretch, mirror, duplicate, swap, hide portals, or host alternative layer content.

## Authoring forms
Version 1 room nodes may be authored in one or both of these forms:

### A. Data-first room templates
A structured data asset or sidecar definition declares connectors, zones, tags, and placement rules, while final geometry/props are produced later.

### B. Local room layout sketches
A room may also have a small text-grid representation for internal planning, especially for furniture zones, fixture walls, and connector locations.

### Recommendation
Start data-first, with optional mini-grid support later. The shell pipeline is already text-heavy; room nodes should avoid becoming too clever too early.

## Relationship to `RoomDefinition`
The project already expects a `RoomDefinition` metadata layer and `RoomVolume`/`RoomRegistry` gameplay plumbing [file:13][file:14]. This spec extends that concept.

### Suggested distinction
- `RoomDefinition`: runtime/gameplay-facing metadata for a room instance.
- `RoomNodeTemplate`: reusable authoring template that can produce or describe many room instances.
- `RoomVolume`: physical bounds in the generated scene.
- `RoomRegistry`: tracks instantiated room volumes and supports queries [file:13].

This keeps reusable authoring logic separate from live match state and scene presence, consistent with the broader architecture [file:12][file:13].

## Connector model
Connectors are the most important part of room-node authoring.

### A connector declares
- connector id,
- local position,
- facing direction,
- threshold type,
- width category,
- vertical relation if any,
- whether the connector expects a wall fill when unused,
- and any semantic restrictions.

### Connector types
- hinged door,
- archway,
- open passage,
- stair access,
- balcony/landing edge,
- concealed/latent connector for anomaly use later.

## Connector rules
- Every connector must be explicit.
- A room node must define its valid connector count range.
- Unused connectors must resolve cleanly, usually as wall-fill or blocked variation.
- Connector orientation must be unambiguous.
- Connectors should map cleanly onto shell thresholds generated by the builder pipeline.

This is especially important because thresholds already matter as navigation and graph entities, not mere visual holes [cite:31][file:13].

## Room categories
Version 1 should support a clear set of baseline room categories. These categories exist to guide population and navigation, not to hard-lock creativity.

### Recommended starting categories
- Entry
- Hall
- Stair Hall
- Landing
- Living Room
- Dining Room
- Kitchen
- Bathroom
- Bedroom
- Office
- Closet
- Utility
- Storage
- Basement Room
- Attic Room
- Transitional Space

Categories should be tags/data, not giant subclasses.

## Required room-node fields
Every room node template should include at least:
- `roomNodeId`
- `displayName`
- `roomCategory`
- `sizeClass`
- `floorSupport`
- `connectorDefinitions`
- `requiredZones`
- `optionalZones`
- `roomTags`
- `navigationIdentityNotes`
- `anomalyCompatibilityTags`
- `authoringNotes`

## Size and shape model
A room node should define size and proportion expectations even if exact dimensions vary.

### Suggested fields
- minimum width/depth,
- preferred width/depth,
- supported aspect ratios,
- ceiling-height class,
- double-height compatibility,
- and floor-span rules.

This helps keep room nodes structurally honest when attached to the shell and later assists mutation systems that care about stretchability and substitution.

## Zone model
Each room node should define internal zones rather than locking every object to exact coordinates.

### Zone families
- fixture zones,
- furniture zones,
- circulation zones,
- anchor zones,
- focal zones,
- clutter zones,
- and reserved anomaly zones.

### Why zones matter
Zones let the room remain adaptable while still preserving a believable internal logic. They are especially useful when you want the room to be repopulated, mirrored, texture-shifted, or slightly altered across layers.

## Fixture zones
Fixtures are semi-structural room elements whose placement should be more constrained than generic furniture.

### Examples
- kitchen counter runs,
- sink walls,
- bathroom vanity,
- toilet location,
- tub/shower footprint,
- built-in shelving,
- stair guardrail edge.

### Rule
Fixture zones should typically reference wall orientation and connector relation. A bathroom template, for example, should know which wall can host plumbing fixtures relative to door position, rather than relying entirely on freeform furniture logic.

## Furniture zones
Furniture zones describe plausible occupancy areas for larger movable or semi-movable objects.

### Examples
- sofa wall,
- bed zone,
- table footprint,
- desk zone,
- shelf zone,
- chair cluster,
- loose-storage zone.

### Rule
Furniture zones should preserve circulation. A room node should not permit furniture placements that routinely destroy walkability unless explicitly intended for a cluttered variation.

## Circulation and sightlines
A room node should define the expected movement and visual character of the space.

### Useful fields
- central circulation path,
- perimeter circulation allowance,
- expected sightline length,
- partial occlusion allowance,
- hiding-nook potential,
- and threshold-to-threshold visibility.

This matters because rooms in this game are not just decoration; they shape fear, pursuit, navigation, and false familiarity [cite:31][file:12].

## Navigation identity requirements
The navigation docs already established that rooms must be memorable enough to name, describe, and mistrust later [cite:31]. Room nodes should therefore include intentional identity cues.

### Every room node should answer
- what makes this room recognizable,
- how would players refer to it verbally,
- which threshold feels primary,
- what local landmark is most stable,
- and what aspect could later be subtly violated by mutation.

### Example identity cues
- room proportion,
- a dominant fixture wall,
- a broken light location,
- a window grouping,
- a split-level step,
- a central island,
- or a distinct dead-end alcove.

## Threshold adjacency rules
A room node should specify how it behaves relative to its connectors.

### Examples
- bathroom doors should usually not open directly into certain room categories unless explicitly allowed,
- hall nodes may require at least two connectors,
- closets may allow exactly one connector,
- kitchens may prefer one wide opening and one secondary connector,
- landing nodes may require at least one stair connector.

These rules help generated layouts stay plausible before anomaly systems intentionally distort them.

## Wall-fill behavior
Unused connectors must not leave visual nonsense.

### A room node may define
- default wall-fill style,
- alternate blocked style,
- latent anomaly-sealed style,
- window substitution allowance,
- or furniture-against-wall substitute behavior.

This is important because the same room node may appear in multiple layouts with different active connector counts.

## Rotation and mirroring
Room nodes should support controlled transformation.

### Allowed transforms
- 90-degree rotation,
- 180-degree rotation,
- horizontal mirror,
- vertical mirror,
- no-transform if the room contains directional assumptions that break under mirroring.

### Rule
Transform support must be explicit, not assumed. Bathrooms, stair rooms, and kitchens often have strong directional logic.

## Vertical compatibility
Because the house supports multiple floors and open-to-below conditions, room nodes must express vertical assumptions.

### Examples
- supports only standard ceiling,
- supports double-height,
- supports mezzanine edge,
- supports stair penetration,
- supports attic slope later,
- incompatible with open-to-below shell region.

## Anomaly compatibility tags
The room node layer should already prepare for later mutation systems.

### Useful compatibility tags
- stretchable,
- swappable,
- loopable,
- mirror-safe,
- duplicate-safe,
- hidden-portal-capable,
- texture-layer-friendly,
- one-way-visibility candidate.

These tags should remain descriptive and permissive. They should not implement anomaly behavior themselves.

## Suggested `RoomNodeTemplate` data shape
```txt
RoomNodeTemplate
  roomNodeId
  displayName
  roomCategory
  sizeClass
  allowedTransforms
  connectorDefinitions[]
  requiredZones[]
  optionalZones[]
  navigationIdentityNotes
  thresholdBehaviorRules[]
  anomalyCompatibilityTags[]
  verticalCompatibility
  authoringNotes
```

## Optional mini-grid authoring
A room node may later support a local mini-grid text format for internal planning.

### Good uses
- rough fixture placement,
- connector sketching,
- local circulation planning,
- furniture footprint design.

### Caution
Do not force every room node to become a mini-map. The room-node layer should stay semantically expressive rather than collapsing into another coarse shell DSL.

## Population handoff
Room nodes should not fully place every object themselves. Instead, they should hand clear constraints to later population systems.

### The handoff should communicate
- where fixtures may go,
- where large furniture may go,
- where circulation must remain open,
- which local landmarks should be preserved,
- and which anomaly-sensitive spaces must remain available.

## Debug requirements
The room-node system needs its own developer visibility, consistent with the project’s broader debug-first rule [file:12][file:13][file:15].

### Debug should show
- room-node template name,
- connector locations and orientations,
- active vs unused connectors,
- zone overlays,
- required vs optional zones,
- transform state,
- anomaly compatibility tags,
- and navigation identity notes.

## Authoring workflow
A practical room-node workflow should look like this:
1. Define a baseline room category.
2. Declare connector contracts.
3. Define minimum/probable size and transform support.
4. Add fixture, furniture, and circulation zones.
5. Record navigation identity notes.
6. Add anomaly compatibility tags.
7. Test the node in one or more generated shells.
8. Adjust until the room reads clearly in play and in debug.

This keeps room authorship grounded in function and readability before decoration.

## Initial room-node library
Version 1 should begin with a small, highly useful room-node set rather than a giant taxonomy [file:12][file:15].

### Recommended starters
- `RN_Entry_Small`
- `RN_Hall_Straight`
- `RN_Hall_Turn`
- `RN_Landing_Small`
- `RN_Living_Open`
- `RN_Kitchen_Galley`
- `RN_Kitchen_Island`
- `RN_Bathroom_Small`
- `RN_Bedroom_Small`
- `RN_Closet_Narrow`
- `RN_Utility_Compact`
- `RN_Stair_Core`

This is enough to establish the pipeline and playtest room readability without overextending the content surface.

## Acceptance criteria
This spec is successful when:
- room templates can be authored as reusable data rather than one-off scene hacks [file:12][file:13],
- connectors map cleanly to generated thresholds from the house builder pipeline,
- room identity is strong enough to support navigation and verbal callouts [cite:31][cite:32],
- fixture/furniture placement can be guided without hardcoding every prop,
- room templates expose enough metadata for future anomaly systems,
- and the system remains modular enough that adding a new room type mostly means adding data and template content rather than rewriting engine code [file:12][file:14][file:15].
