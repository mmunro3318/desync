# House Builder Pipeline Epic

## Epic objective
Define the end-to-end pipeline that converts House Layout DSL source files into validated structural data, generated Unity house geometry, room and threshold scaffolding, graph-ready connectivity data, and developer-facing debug artifacts. This epic exists to make baseline house creation deterministic, data-driven, inspectable, and reusable across multiple prototype houses [file:12][file:13][file:14][file:15].

## Why this epic exists
The project already favors data-driven content, thin scene objects, graybox-first iteration, and debug-first development. A dedicated builder pipeline applies those same architectural rules to level creation by ensuring that house layout input lives as versionable source data, generation passes remain modular, and generated scene structure does not become hand-authored drift inside Unity [file:12][file:13][file:14][file:15].

## Epic question answered
Can a text-authored house sketch be converted into a trustworthy Unity baseline house shell quickly enough that level iteration becomes mostly authoring-and-regenerate rather than manual scene construction [file:12][file:14]?

## In scope
This epic covers:
- reading House Layout DSL inputs,
- parsing source files into an intermediate model,
- validating structural correctness,
- generating baseline structural output,
- creating room/threshold scaffolding,
- producing graph-ready connectivity data,
- writing debug artifacts and overlays,
- and integrating the importer into the Unity project structure [file:13][file:14].

## Out of scope
This epic does **not** include:
- final furnishing and clutter passes,
- final texture/material art direction,
- anomaly mutation logic,
- final lighting polish,
- enemy/gameplay content placement,
- or complete procedural decoration systems.

The builder should solve the baseline shell first. Atmosphere and content population can layer on top later, consistent with the project’s graybox-first philosophy [file:12][file:15].

## Pipeline philosophy

### 1. Source of truth is text
The DSL files are the canonical authoring source for a generated house shell. Generated Unity outputs are build artifacts, not the primary design source.

### 2. Parse first, generate second
The importer must create an intermediate validated data model before any Unity objects are spawned. Generation should never operate directly on raw character rows.

### 3. Deterministic by default
Given the same input files and settings, the pipeline should produce the same structural result every time unless optional inference/randomization is explicitly enabled.

### 4. Builder output is scaffolding
The pipeline should generate a clean, playable, inspectable house shell and authoring scaffolds such as room volumes and thresholds, not a production-finished environment [file:12][file:14].

### 5. Debug visibility is mandatory
Every major transformation step should expose enough visibility to understand what was parsed, inferred, generated, rejected, or auto-corrected [file:12][file:13][file:15].

## Pipeline stages
The builder should be implemented as a series of explicit stages.

### Stage 0: Source discovery
Input files are located, referenced by manifest, and checked for existence.

### Stage 1: Parse
Manifest and floor text files are parsed into an intermediate structural representation.

### Stage 2: Validate
The intermediate model is checked for token legality, rectangularity, floor alignment, threshold ambiguity, stair coherence, room metadata consistency, and other structural rules.

### Stage 3: Normalize
Convenience transformations occur here, such as wall-segment merging, contiguous stair grouping, threshold orientation resolution, and region indexing.

### Stage 4: Generate structural shell
The builder creates baseline floor, wall, ceiling, opening, window, and stair representation in Unity.

### Stage 5: Generate scaffolds
The builder creates room volumes, room labels, threshold anchors, graph-edge seed data, and optional debug markers [file:13][file:14].

### Stage 6: Emit artifacts
The pipeline writes generated assets, reports, and optional debug snapshots.

### Stage 7: Present results
The user receives an import summary with errors, warnings, counts, and references to generated scene content.

## Intermediate model
The pipeline should center around a stable intermediate representation, because the project already benefits from separating definition data from runtime/generated state [file:12][file:13].

### Suggested data objects
- `HouseLayoutDefinition`
- `FloorLayoutDefinition`
- `TileCell`
- `RoomLayoutDefinition`
- `ThresholdDefinition`
- `VerticalRegionDefinition`
- `StairRunDefinition`
- `ConnectivitySeedDefinition`
- `LayoutValidationReport`
- `HouseBuildResult`

### Rule
The intermediate model should remain engine-agnostic enough that parsing and validation can run without spawning GameObjects.

## Unity integration targets
This pipeline should land naturally inside the existing project structure, which already reserves space for `Data`, `Prefabs`, `World/Rooms`, and `Editor` scripts [file:13][file:14].

### Recommended locations
- DSL source assets or imports in `Assets/_Project/Data/Rooms/Layouts/`
- generated room/house data assets in `Assets/_Project/Data/Rooms/Generated/`
- structural prefabs or generated roots in `Assets/_Project/Art/Prefabs/Environment/Generated/`
- builder/editor tooling in `Assets/_Project/Scripts/Editor/`
- shared runtime readers in `Assets/_Project/Scripts/World/Rooms/`

## Ownership boundaries
To avoid a giant importer god-object, the pipeline should use narrow responsibilities, in line with the rest of the architecture [file:13][file:14][file:15].

| Class/System | Owns | Does not own |
|---|---|---|
| `HouseLayoutImporter` | Entry point for import/build orchestration | Mesh generation details, runtime room logic |
| `HouseLayoutParser` | Parsing manifest and grid text into raw model | Unity scene generation |
| `HouseLayoutValidator` | Errors, warnings, structural legality | Instantiation |
| `HouseLayoutNormalizer` | Derived regions, merged walls, orientation inference | Authoritative room gameplay semantics |
| `HouseShellBuilder` | Floors, walls, ceilings, openings, stair shell | Room gameplay metadata |
| `RoomScaffoldBuilder` | Room volumes, labels, threshold anchors | Match logic |
| `GraphSeedBuilder` | Node/edge seed data from structure | Runtime anomaly execution |
| `HouseBuildReporter` | Import logs, summaries, debug reports | Game logic |
| `HouseBuildDebugView` | Developer visualization overlays | Final player-facing UX |

## Structural generation outputs
The pipeline should generate at least the following outputs from a valid house definition:
- floor surfaces,
- wall surfaces,
- ceiling surfaces if enabled,
- doorway openings,
- archway openings,
- open passage thresholds,
- window segments,
- stair runs,
- generated root hierarchy,
- and collision-ready baseline geometry.

These outputs should be sufficient to support the “walk around a house and touch things” foundation milestone already established as the correct first proof target [file:12][file:15].

## Scaffold generation outputs
In addition to visible geometry, the builder should emit authoring/gameplay scaffolds.

### Required scaffolds
- `RoomVolume` instances or generated room bounds [file:13][file:14]
- threshold anchors for `D`, `A`, and `O`
- room labels or ids for debug
- stair-link markers
- optional spawn-anchor placeholders for later systems
- optional light-anchor placeholders for later population

This matters because rooms and thresholds are gameplay entities, not just art objects [file:12][file:13].

## Graph seed generation
The house shell should produce graph-friendly connectivity data, even if the full House Graph Core is implemented separately. The builder should identify traversable regions and threshold connections so later graph systems do not need to rediscover structural truth from loose scene meshes.

### Graph seed data should include
- room id or provisional room id,
- neighboring thresholds,
- threshold type,
- floor index,
- vertical connections,
- and connectivity groups.

This aligns with the project’s broader push toward explicit topology and inspectable hidden systems [file:12][file:13].

## Room generation strategy
Version 1 should support two room-definition modes.

### Mode A: Explicit metadata rooms
Rooms are declared in sidecar metadata and the builder instantiates corresponding room volumes directly.

### Mode B: Assisted inference
The builder flood-fills walkable regions and proposes room candidates, then either accepts them in debug/dev mode or writes suggested metadata for the designer to refine.

### Default recommendation
Start with explicit metadata rooms first. It is safer, more deterministic, and easier to debug than relying on automatic room inference too early.

## Threshold generation strategy
Thresholds are semantically important. The builder should not just cut holes in walls; it should generate explicit threshold entities for doorways, archways, and open passages.

### Each threshold output should include
- threshold id,
- source floor,
- world transform,
- orientation,
- threshold type,
- connected regions/rooms if known,
- and a builder provenance tag.

That makes later interaction, portal logic, and graph mutation systems much easier to attach.

## Stair generation strategy
Stairs are more than decorative meshes. They are structural connectors between floors.

### Stair builder responsibilities
- detect contiguous stair regions,
- resolve direction or require explicit metadata,
- generate physical stair shell,
- link source floor and destination floor,
- and expose a debug-visible stair definition.

### Rule
The stair pass should fail loudly when a stair run cannot be resolved meaningfully across floors.

## Geometry strategy
The pipeline should remain agnostic about whether the house shell is produced through procedural mesh generation, modular prefab placement, or a hybrid approach. However, the first implementation should optimize for speed of iteration, inspectability, and easy rebuilding over perfect geometric elegance [file:12][file:15].

### Practical recommendation
Start with modular prefab placement or simple generated quads/cubes for:
- floors,
- walls,
- ceilings,
- and openings.

Fancy mesh merging can come later if performance or authoring needs justify it.

## Build modes
The builder should support at least two modes.

### 1. Preview build
Fast generation for iteration, debug, and visual inspection.

### 2. Commit build
Writes or updates generated assets/prefabs/scene content as the current working house artifact.

An optional future third mode could export reports only without Unity object generation.

## Rebuild behavior
The system should support safe repeated regeneration, because the project depends on rapid iteration [file:12][file:15].

### Rules
- generated content should live under a clear generated root,
- regeneration should replace or refresh that root predictably,
- hand-authored non-generated content should not be silently destroyed,
- and generated artifacts should include provenance metadata so stale outputs can be detected.

## Error handling and reporting
The builder should never fail silently.

### Import result should report
- success/failure state,
- hard errors,
- warnings,
- source file names,
- counts of floors, rooms, thresholds, stairs, windows, and generated objects,
- and whether any inferred assumptions were applied.

### Error classes
- source error,
- parse error,
- validation error,
- normalization ambiguity,
- generation failure,
- and post-build integrity failure.

## Debug tooling
This pipeline needs rich editor/debug support because it performs hidden transformations [file:12][file:13][file:15].

### Required debug views
- raw tile grid preview,
- coordinate overlay,
- token-type coloring,
- room-region overlay,
- threshold/orientation overlay,
- stair-link overlay,
- graph-seed overlay,
- and generated object hierarchy summary.

### Required reports
- validation report,
- normalization summary,
- build summary,
- and unresolved ambiguity list.

## Editor workflow
The pipeline should fit a simple Unity editor workflow.

### Suggested flow
1. Designer edits DSL files.
2. User selects the house manifest asset or menu action.
3. Parser and validator run.
4. Errors/warnings are shown.
5. If valid, preview or commit build generates the house.
6. Generated root appears in `House_Graybox` or `House_Prototype`.
7. Debug overlays allow rapid inspection.

This matches the broader goal of keeping iteration tight and avoiding manual scene drift [file:12][file:14].

## Proposed scene hierarchy output
A generated house should appear under a predictable root.

```txt
GeneratedHouse_[HouseId]
  Source
  Structure
    Floors
    Walls
    Ceilings
    Windows
    Stairs
    Thresholds
  Rooms
    RoomVolumes
    RoomLabels
  GraphSeeds
  Debug
```

This aligns with the project’s preference for boring, stable structure over improvised hierarchy sprawl [file:13][file:14].

## Milestone ladder
This epic should be implemented in narrow slices, following the project’s general milestone philosophy [file:12][file:15].

### Milestone HB-1: Parse and validate only
Goal:
- manifest read,
- floor file parse,
- validation output,
- no Unity generation yet.

Question answered:
- can the DSL be read safely and inspected clearly?

### Milestone HB-2: Generate baseline shell
Goal:
- floors,
- walls,
- ceilings,
- openings,
- simple stairs,
- generated root hierarchy.

Question answered:
- can a text house become a walkable graybox shell?

### Milestone HB-3: Generate room and threshold scaffolds
Goal:
- room volumes,
- threshold anchors,
- room labels,
- graph seed artifacts.

Question answered:
- can the shell become gameplay-meaningful structure?

### Milestone HB-4: Editor tooling and rebuild flow
Goal:
- preview/commit build,
- safe regeneration,
- reports,
- debug overlays.

Question answered:
- is the pipeline usable enough for daily iteration?

### Milestone HB-5: Import quality pass
Goal:
- edge-case handling,
- stronger ambiguity reporting,
- multi-floor stability,
- sample house library.

Question answered:
- is the builder stable enough to become the normal house-authoring workflow?

## Acceptance criteria
This epic is complete when all of the following are true:
- a valid House Layout DSL manifest and floor set can be imported into Unity reliably,
- parsing and validation happen before generation,
- common authoring mistakes produce readable errors rather than broken scene output,
- a valid input can generate a playable structural shell with floors, walls, ceilings, openings, and stairs,
- room volumes and threshold anchors are generated as scaffolds [file:13][file:14],
- graph-ready connectivity seed data is emitted,
- generated output can be rebuilt repeatedly without destructive scene chaos,
- and debug tooling makes each transformation stage inspectable [file:12][file:13][file:15].

