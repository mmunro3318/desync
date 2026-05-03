# House Layout DSL Spec

## Document objective
Define a constrained text-based authoring format for describing baseline house shells that can be parsed into Unity-ready structural data, room topology, openings, vertical relationships, and generation hints. The House Layout DSL exists to turn house design into data, keep scene authoring lightweight, and support AI-assisted import/build pipelines without relying on ad hoc scene construction [file:12][file:13][file:14][file:15].

## Why this spec exists
The project architecture already favors data-driven content, thin scene objects, graybox-first iteration, and strong debug visibility. A text-first house authoring format fits those principles by allowing new houses to be sketched, versioned, validated, diffed, transformed, and generated without hand-building every wall in the Unity editor [file:12][file:13][file:14][file:15].

## Scope
This DSL is for the **baseline house shell** only. Version 1 should define:
- floor footprint,
- walls,
- walkable interior space,
- doors and openings,
- windows,
- stairs,
- vertical voids/open-to-below regions,
- and optional room metadata through sidecar sections.

Version 1 should **not** attempt to fully encode final furnishing, clutter, lighting polish, or anomaly logic. Those should be layered later through other specs and systems [file:12][file:14].

## Core design principles

### 1. Data first
A new house should mostly mean new text files and derived assets, not manual scene reconstruction. This follows the same “new content should mostly mean new data” principle already established elsewhere in the project [file:12][file:13].

### 2. Deterministic import
The same DSL input should always produce the same parsed structural model unless the user explicitly enables optional inference or randomization.

### 3. Human-sketchable
A designer should be able to rough out a house in plain text quickly, reason about it visually, and maintain it in version control.

### 4. Machine-validated
The parser must be able to detect malformed rows, unknown tokens, impossible openings, disconnected stairs, broken floor alignment, and other structural problems before generation.

### 5. Layered semantics
The base layout grid should remain simple. Higher-level meaning such as room ids, tags, vertical spans, and generation hints should be attached through sidecar metadata blocks rather than overloading the main tile stream.

## Authoring model
Version 1 uses a **manifest + per-floor grid files** model.

### Required files
- one house manifest file,
- one or more floor layout files,
- optional metadata blocks embedded inside the manifest or separate sidecar files.

### Recommended file set
- `house.layout`
- `floor_0.txt`
- `floor_1.txt`
- `floor_2.txt`

The manifest defines global settings and floor registration. Each floor file defines only the tile grid for that level.

## Coordinate model
All floors are aligned to a shared top-left origin. The top-left cell of each floor grid is coordinate `(0,0)` in local floor space.

### Rules
- `x` increases to the right.
- `y` increases downward in the text file.
- floor index increases upward vertically.
- every tile occupies one square cell.
- the parser converts each tile into world-space coordinates using manifest settings.

This shared origin keeps multi-floor alignment deterministic and makes stair shafts, double-height regions, and open-to-below spaces easier to validate and debug.

## Unit model
Each tile represents one fixed square unit of space.

### Version 1 default
- `tileSize = 1.0` meters.
- `wallHeight = 3.0` meters.
- `floorThickness = 0.2` meters.
- `ceilingThickness = 0.2` meters.

### Rule
A token always occupies exactly one tile. Avoid multi-character geometry semantics such as “three Ds equals one door.” Larger architectural features must be represented by multiple tiles or by metadata.

## File structure

### Manifest format
The exact serialization may be `.layout`, `.yaml`, or `.json`, but the information content must include:
- house id,
- version,
- tile size,
- default heights,
- declared floor files,
- and optional global generation settings.

### Example manifest
```txt
@house
id: prototype_house_01
version: 1
origin: top-left
tileSize: 1.0
wallHeight: 3.0
floorThickness: 0.2
ceilingThickness: 0.2

@floors
0: floor_0.txt
1: floor_1.txt

@settings
buildCeilings: true
buildRoomVolumes: true
inferRooms: false
```

### Floor file format
A floor file contains only a rectangular grid of characters. Every row must have identical width. Blank lines are not allowed inside the grid.

## Tile grammar
Version 1 should use a small, strict token set.

| Token | Meaning | Notes |
|---|---|---|
| `#` | Solid wall tile | Generates wall geometry/collision. |
| `.` | Walkable interior floor tile | Default inhabitable space. |
| `D` | Door opening tile | Generates doorway + door anchor; actual door prefab may be inserted by pipeline. |
| `A` | Archway opening tile | Opening with arch trim, no physical door. |
| `O` | Open passage tile | Plain opening/portal connection, no door or arch trim required. |
| `W` | Window wall tile | Wall with window opening. |
| `S` | Stair tile | Participates in stair run generation and floor-to-floor linking. |
| `V` | Void / outside / no-build tile | No floor, no ceiling, no interior geometry. |
| `X` | Open-to-below tile | Walkability depends on floor context; indicates vertical opening/void region. |
| `F` | Furniture placeholder tile | Optional in v1; treated as walkable authoring hint unless furniture population is enabled. |

## Token semantics

### `#` wall
A wall tile represents solid boundary structure. Adjacent wall tiles are merged by the builder into longer wall segments where possible.

### `.` interior floor
A walkable tile generates base floor geometry and participates in room inference, adjacency, navigation, and room-volume generation.

### `D` door
A door tile represents an intentional threshold that may host a physical door prefab, frame, interaction component, and graph edge. The builder should infer orientation from surrounding tiles or explicit metadata.

### `A` archway
An archway is a threshold opening with no closable door. It is semantically stronger than `O` because it carries architectural framing.

### `O` open passage
An open passage represents a threshold connection with no door or arch treatment. Use it for hall openings, cased-less transitions, or pure connection slots.

### `W` window
A window tile is structurally wall-like, but includes an opening to exterior or a non-passable view space. It should generate wall plus window treatment, not a traversable connection.

### `S` stairs
Stair tiles define where a vertical traversal structure exists. A connected region of stair tiles should be interpreted as one stair run and validated against the adjacent floor layout.

### `V` void
Void tiles are outside the built house footprint. They generate nothing and are useful for sketching irregular footprints while preserving rectangular file shape.

### `X` open-to-below
An `X` tile indicates that this floor contains a vertical opening rather than standard floor surface. This is intended for great rooms, stair voids, mezzanines, balcony edges, and similar multi-height spaces.

### `F` furniture placeholder
Furniture placeholder tiles should be treated as optional authoring hints only in version 1. They may later map to room-population logic, but they should not block the baseline shell generator.

## Floor file example
```txt
VVVV##########VVVV
VVVV#........#VVVV
VVVV#........#VVVV
####A........#####
W...O............#
W...O............#
#######DDD########
```

## Structural rules

### Rectangularity
Every floor grid must be rectangular. All rows must be identical width.

### Shared alignment
All floor files in a house should use the same width and height in version 1. This simplifies vertical validation and top-left alignment.

### Connectivity expectation
At least one connected walkable region should exist on the primary floor. Disconnected regions are allowed only when intentionally supported by design or by metadata.

### Threshold legality
`D`, `A`, and `O` tiles should only appear where they connect meaningful spatial regions. A threshold tile embedded fully inside wall mass or fully inside open floor with no boundary role should fail validation.

### Stair legality
A stair region must connect meaningfully to another floor or terminate in an explicitly allowed incomplete-dev state. Orphan stair regions should raise a validation error.

### Window legality
A window should border an interior walkable region on at least one side and void/exterior space on another side. A window between two interior spaces should fail or warn.

## Orientation inference
Version 1 may infer orientation for `D`, `A`, `O`, `W`, and `S` from local neighborhood patterns.

### Example rule
If a `D` tile has wall/open/wall alignment horizontally and solid separation vertically, the builder treats it as an east-west door span. If the inverse is true, the builder treats it as north-south.

### Rule
If orientation cannot be inferred unambiguously, the parser must raise an error or require explicit metadata.

## Metadata blocks
The main grid should remain simple. Additional semantics belong in metadata.

### Supported metadata categories in version 1
- room declarations,
- explicit room rectangles or flood-fill anchors,
- vertical region declarations,
- stair linkage,
- generation hints,
- and explicit overrides where inference is ambiguous.

### Example metadata block
```txt
@rooms
room: Entry floor=0 rect=(4,1,6,4)
room: Hall floor=0 rect=(1,4,12,2)
room: GreatRoom floor=0 rect=(8,5,10,8)

@vertical
openToBelow: floor=1 rect=(9,5,4,3)

@stairs
link: floor=0 rect=(14,7,2,4) toFloor=1 arrival=(14,3)
```

## Room semantics
This DSL is not required to fully encode final room identity, but it should support optional room labeling because room semantics matter for navigation, graph generation, and later gameplay systems [file:12][file:13][file:14].

### Version 1 room support
A room may be defined by:
- explicit rectangle,
- explicit tile list,
- or later by flood-fill seeded from an anchor tile.

### Room metadata may include
- `roomId`
- `displayName`
- `roomType`
- `tags`
- `floor`
- `bounds`
- `spawnEligible`
- `notes`

This fits the existing plan to use room metadata and registry systems rather than embedding all meaning in scene geometry [file:13][file:14].

## Intermediate parsed model
The importer should not generate Unity objects directly from raw characters. It should first build an intermediate data model.

### Suggested data types
- `HouseLayoutDefinition`
- `FloorLayoutDefinition`
- `TileCell`
- `RoomLayoutDefinition`
- `VerticalRegionDefinition`
- `ThresholdDefinition`
- `StairLinkDefinition`
- `LayoutValidationReport`

This follows the broader project rule of separating authoring data from runtime/generated state [file:12][file:13].

## Generation outputs
The builder pipeline should convert the DSL into a generated structural package, which may include:
- floor meshes or floor prefabs,
- wall meshes or modular wall instances,
- ceiling meshes,
- door/window/stair anchor placements,
- room volumes,
- graph nodes and threshold/portal connections,
- and import debug artifacts.

The DSL itself should not care whether final Unity generation uses procedural meshes, modular prefabs, or a hybrid approach.

## Inference vs explicit authoring
Version 1 should prefer explicit correctness over aggressive inference.

### Safe to infer
- wall segment merging,
- simple door orientation,
- room flood-fill candidates,
- stair grouping by contiguous tiles,
- basic ceiling generation.

### Better explicit than inferred
- exact stair direction,
- unusual threshold orientation,
- room labels,
- double-height semantic meaning,
- special architectural exceptions.

## Validation requirements
Validation is mandatory. The parser should produce a structured report with errors and warnings.

### Hard errors
- unknown token,
- inconsistent row width,
- missing declared floor file,
- ambiguous threshold orientation,
- impossible stair linkage,
- room bounds outside grid,
- overlapping incompatible metadata regions,
- floor alignment mismatch,
- or empty/invalid house footprint.

### Warnings
- disconnected walkable region,
- suspicious door placement,
- untagged large room candidate,
- isolated furniture placeholder,
- excessive void fragmentation,
- or inferred orientation where explicit metadata would be safer.

## Debug requirements
Because the project already treats debug visibility as architectural, the layout importer must expose its truth clearly [file:12][file:13][file:15].

### Debug views should support
- rendered grid with coordinates,
- token-type overlay,
- room-label overlay,
- threshold orientation overlay,
- stair-link overlay,
- open-to-below region overlay,
- generated graph/node overlay,
- and validation report output.

This is important because house generation is a hidden transformation pipeline. Without visibility, iteration will become slow and unreliable.

## Versioning
The DSL must include a version field in the manifest.

### Rule
Importers should reject unsupported future versions rather than silently interpreting them incorrectly.

## Extension policy
Version 1 should remain intentionally modest. Likely future extensions include:
- richer room-shape declarations,
- explicit connector nodes for room modules,
- wall material/texturing hints,
- furniture classes and fixture zones,
- lighting anchor hints,
- anomaly-compatible mutation tags,
- and conversion tools from Unity scenes back into DSL representations.

Those should be added as backward-compatible metadata layers rather than by destabilizing the base tile grammar.

## Example full authoring set

### `house.layout`
```txt
@house
id: prototype_house_01
version: 1
origin: top-left
tileSize: 1.0
wallHeight: 3.0
floorThickness: 0.2
ceilingThickness: 0.2

@floors
0: floor_0.txt
1: floor_1.txt

@settings
buildCeilings: true
buildRoomVolumes: true
inferRooms: false

@rooms
room: Entry floor=0 rect=(4,1,6,4)
room: Hall floor=0 rect=(0,4,14,2)
room: GreatRoom floor=0 rect=(8,5,10,8)
room: Landing floor=1 rect=(10,3,5,3)

@stairs
link: floor=0 rect=(14,7,2,4) toFloor=1 arrival=(14,3)
```

### `floor_0.txt`
```txt
VVVV##########VVVV
VVVV#........#VVVV
VVVV#........#VVVV
####A........#####
W...O............#
W...O............#
#######DDD########
```

### `floor_1.txt`
```txt
VVVV##########VVVV
VVVV#....XX..#VVVV
VVVV#....XX..#VVVV
####A....XX..#####
W...O............#
W...O............#
#######DDD########
```

## Acceptance criteria
This spec is successful when all of the following are true:
- a designer can sketch a baseline multi-floor house shell using plain text,
- Claude Code or another tool can parse the files deterministically into an intermediate model,
- the importer can validate common structural mistakes before Unity generation,
- the generated output can create at least floors, walls, ceilings, openings, and room/threshold scaffolding,
- the data model supports later room graph and mutation systems without refactoring the base format,
- and the DSL remains simple enough that a human can still read and edit it directly [file:12][file:13][file:14].

