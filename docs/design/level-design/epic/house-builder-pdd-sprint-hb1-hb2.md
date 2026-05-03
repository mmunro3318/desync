# House Builder PDD / Sprint Doc

## Sprint title
House Builder Sprint — HB-1 and HB-2 Implementation Plan

## Document objective
Turn Milestones HB-1 and HB-2 into concrete implementation work for the house import and build foundation. This sprint doc translates the recent architecture work on importer contracts, graph integration, room population boundaries, and debug visualization into a sequence Claude Code can execute in narrow, testable tasks [file:12][file:13][file:14][file:15].

## Why this sprint exists
The project now has enough design architecture to stop talking abstractly and start building the first real house-builder stack. The goal is not a final procedural house system yet. The goal is to produce a trustworthy pipeline that can ingest layout data, validate and normalize it, surface issues visually, and hand off a graph-ready artifact that later systems can consume without inventing their own structural truth [file:12][file:13][file:14].

## Sprint thesis
HB-1 and HB-2 should prove that the project can take a structured house layout definition, convert it into a validated artifact, inspect it in tools, and expose enough graph-ready structural truth that future room population, visibility, and mutation systems have a stable base [file:12][file:13].

## Milestone framing
This sprint treats the two milestones as follows.

### HB-1
Importer foundation: canonical contracts, parsing, validation, normalization, and artifact generation.

### HB-2
Structural debug and graph-ready integration: grid/room/portal/stair visualization, orphan/invalid detection, and graph-seed export/inspection.

## Out of scope
This sprint does **not** include:
- final anomaly runtime,
- final procedural generation heuristics,
- final art population,
- AI navigation,
- multiplayer replication,
- or polished player-facing map UX.

Those systems depend on HB-1/HB-2, but are not part of this sprint.

## Sprint goals
By the end of this sprint, the project should support:
- importing or loading a sample house layout definition,
- validating it with structured issues,
- normalizing it into a `GeneratedHouseArtifact`,
- visualizing parsed tiles, room ids, portals, stairs, invalid tiles, and orphan spaces,
- and exporting graph-ready seed data for later house graph integration [file:12][file:13][file:14].

## Architecture rules carried into implementation
This sprint should honor the broader project rules already established across your docs.

- Runtime state stays separate from design/import definitions [file:12][file:13][file:14].
- Scene objects stay thin; the importer/build logic lives in focused systems [file:12][file:13][file:15].
- Debug visibility is mandatory for hidden or structural systems [file:12][file:13][file:15].
- Narrow modules beat giant manager gods [file:13][file:15].

## Deliverables
The expected sprint deliverables are:
- importer contract classes,
- validator classes,
- normalizer/builder classes,
- `GeneratedHouseArtifact` model,
- sample input asset(s),
- validation report output,
- debug visualization tools,
- graph-seed export layer,
- and a small graybox validation scene or editor workflow [file:13][file:14].

## Recommended folder targets
This sprint should land mostly inside the existing project structure rather than inventing a parallel architecture [file:13][file:14].

### Suggested folders
- `Assets/_Project/Data/Rooms/Import/`
- `Assets/_Project/Data/Rooms/Graphs/`
- `Assets/_Project/Scripts/World/Rooms/Import/`
- `Assets/_Project/Scripts/World/Graph/`
- `Assets/_Project/Scripts/World/Rooms/Debug/`
- `Assets/_Project/Scripts/Editor/HouseImport/`
- `Assets/_Project/Scenes/Test/` or `House_Graybox/`

## Suggested implementation slices
The sprint should be broken into narrow vertical tasks rather than by abstract discipline.

### Recommended slice order
1. Contract types.
2. Import result wrapper.
3. Validation pass framework.
4. Normalization and artifact generation.
5. Sample layout assets.
6. Debug visualization foundation.
7. Portal/stair/connectivity overlays.
8. Graph-seed export and inspection.
9. End-to-end test scene/editor pass.

## HB-1 overview
HB-1 is about trustworthy data ingestion.

### HB-1 answers
Can the project ingest a house layout in a stable canonical shape, validate it, and produce an artifact that downstream systems can trust [file:12][file:13]?

## HB-1 tasks

### HB1-T1 — Create canonical contract models
Implement the baseline serializable contract types from the importer doc.

#### Required types
- `HouseLayoutDefinition`
- `FloorLayoutDefinition`
- `TileCell`
- `RoomDefinition` or code-safe renamed equivalent
- `PortalDefinition`
- `GeneratedHouseArtifact`
- `ValidationIssue`
- `ValidationReport`

#### Acceptance criteria
- All types compile.
- Fields match the agreed conceptual ownership.
- No runtime-only state leaks into import definitions.
- Types are usable without scene references [file:12][file:13][file:14].

### HB1-T2 — Add supporting enums and value objects
Create shared enums/value objects used by the contracts.

#### Examples
- `CellType`
- `PortalType`
- `ValidationSeverity`
- `ImportStatus`
- endpoint reference type(s)
- coordinate structs
- bounds summaries

#### Acceptance criteria
- Contracts no longer depend on magic strings for critical structural categories.
- Endpoint references are explicit enough to support both room-based and cell-based importers.

### HB1-T3 — Create import result wrapper
Build a structured wrapper for importer outputs.

#### Suggested wrapper
- `layout`
- `validationReport`
- `artifact`
- `rawSourceSummary`
- `importStatus`

#### Acceptance criteria
- Importers do not return only a bare layout object.
- Error/warning conditions can be surfaced without exceptions being the only control flow.

### HB1-T4 — Implement validator framework
Create a validator pipeline with phased validation.

#### Required phases
1. Schema validation.
2. Identity validation.
3. Reference validation.
4. Spatial bounds validation.
5. Topology/connectivity validation.
6. Normalization-readiness validation.

#### Acceptance criteria
- Validators emit structured `ValidationIssue` records.
- Severity counts populate `ValidationReport`.
- The same broken source produces consistent issue output across runs.

### HB1-T5 — Implement normalization layer
Build a normalizer that converts permissive importer data into strict artifact data.

#### Normalization responsibilities
- fill defaults,
- sort floors,
- resolve ids,
- resolve endpoint references,
- derive stable lookup maps,
- and reject unrecoverable contradictions.

#### Acceptance criteria
- Normalization never silently drops bad structure.
- Risky or lossy decisions emit issues.
- Successful normalization produces a deterministic artifact from the same input seed/source.

### HB1-T6 — Build `GeneratedHouseArtifact`
Produce a build artifact suitable for graph-ready downstream use.

#### Artifact should include
- normalized layout,
- room index,
- portal index,
- floor map,
- graph-seed data,
- validation report,
- provenance/build metadata.

#### Acceptance criteria
- Artifact is immutable after publication by convention or API.
- Downstream graph/debug systems can consume it without revisiting raw source parsing.

### HB1-T7 — Create sample layout fixtures
Author at least three layout fixtures for validation and regression testing.

#### Recommended fixtures
- `house_valid_minimal`
- `house_invalid_broken_portal`
- `house_orphan_component`

#### Acceptance criteria
- One fixture passes cleanly.
- One fixture produces portal/reference failures.
- One fixture produces connectivity/orphan issues.
- Fixtures are easy for Claude and humans to inspect.

### HB1-T8 — Add automated smoke tests
Create small automated tests for the importer/build layer.

#### Suggested tests
- contract deserialize test,
- duplicate floor index failure,
- portal unknown endpoint failure,
- orphan component detection,
- deterministic normalization test.

#### Acceptance criteria
- Tests can run without full scene boot.
- Failures point at importer/build logic rather than generic null-reference crashes.

## HB-1 completion definition
HB-1 is complete when a sample layout can be imported, validated, normalized, and emitted as a `GeneratedHouseArtifact`, and when known-bad fixtures produce structured issues rather than ambiguous failures [file:12][file:13][file:14].

## HB-2 overview
HB-2 is about structural visibility and graph readiness.

### HB-2 answers
Can the project inspect parsed house structure visually and prove that the normalized artifact contains the graph-ready truth later systems need [file:12][file:13]?

## HB-2 tasks

### HB2-T1 — Create debug overlay controller
Implement the top-level debug mode controller.

#### Suggested responsibilities
- mode switching,
- floor switching,
- toggle categories,
- selection routing,
- and panel visibility.

#### Acceptance criteria
- A developer can switch among grid, rooms, portals, stairs, validation, connectivity, and composite views.
- Modes can be isolated cleanly.

### HB2-T2 — Implement parsed grid renderer
Draw imported/normalized tiles.

#### Should show
- cell coordinates,
- primary cell type,
- walkability,
- floor index,
- optional threshold/wall markers.

#### Acceptance criteria
- Grid view matches fixture footprint.
- Coordinates and cell types are inspectable.
- Zoom/readability behavior is acceptable in graybox scale.

### HB2-T3 — Implement room id renderer
Render room fills and labels.

#### Should show
- room ids,
- room names if present,
- room types,
- member-cell grouping.

#### Acceptance criteria
- Split, overlapping, or malformed room membership becomes visually obvious.
- Tiny rooms still expose id through selection or hover.

### HB2-T4 — Implement portal link renderer
Render portal markers and linked endpoints.

#### Should show
- portal ids,
- portal type,
- source/target references,
- directionality,
- unresolved endpoints.

#### Acceptance criteria
- Developers can inspect portal connectivity without opening raw asset data.
- Broken portal references are obvious both visually and textually.

### HB2-T5 — Implement stair link renderer
Render stair relations across floors.

#### Should show
- stair ids,
- source floor,
- destination floor,
- linked landings/endpoints,
- and unresolved stair links.

#### Acceptance criteria
- Stair continuity across floors is visually understandable.
- Selecting one stair endpoint reveals the paired endpoint.

### HB2-T6 — Implement validation renderer
Spatialize structured validation issues.

#### Should show
- invalid tiles,
- issue severity,
- issue code,
- and short reason.

#### Acceptance criteria
- Errors are impossible to miss.
- Clicking a validation issue highlights the relevant tile/room/portal when spatially representable.

### HB2-T7 — Implement connectivity/orphan renderer
Visualize connected components and orphan spaces.

#### Should show
- main connected component,
- disconnected components,
- orphan group id,
- optional route trace from selected room/cell.

#### Acceptance criteria
- Orphan spaces are distinguishable from invalid tiles.
- A valid-looking but disconnected region is easy to identify.

### HB2-T8 — Build selection/inspection panel
Add a small inspector panel for selected tile, room, portal, or issue.

#### Should show
- ids,
- coordinates,
- category/type,
- neighbor references,
- associated validation issues,
- and related portal/room links.

#### Acceptance criteria
- The panel gives enough metadata to debug most issues without opening code.

### HB2-T9 — Export graph-seed records
Add the graph-seed handoff from artifact to graph integration layer.

#### Graph-seed examples
- candidate room nodes,
- portal records,
- stair link records,
- floor relations,
- connectivity summaries.

#### Acceptance criteria
- Graph-seed export is deterministic.
- Downstream graph integration can consume the artifact without re-inferring topology from visual geometry.

### HB2-T10 — End-to-end test scene/editor workflow
Create one practical place to run the whole HB-1/HB-2 pipeline.

#### Could be
- an editor import window,
- a test scene with overlay controls,
- or both.

#### Acceptance criteria
- A developer can load a fixture, inspect all required debug views, and confirm artifact/graph-seed output in one workflow.

## HB-2 completion definition
HB-2 is complete when imported house data can be visually inspected through parsed-grid, room, portal, stair, validation, and connectivity overlays, and when graph-seed output is available for the next graph-focused sprint [file:12][file:13][file:14].

## Suggested task ownership by subsystem
To keep Claude Code focused, tasks should map to narrow ownership areas, consistent with your established modularity rules [file:12][file:13][file:14].

| Area | Main files |
|---|---|
| Contracts | serializable data models |
| Validation | validators, issue codes, reports |
| Normalization | builders, resolvers, artifact generation |
| Fixtures/tests | sample assets and edit-mode tests |
| Visualization | renderers and selection UI |
| Graph handoff | graph-seed export and readers |

## Claude Code workflow guidance
Claude should not be asked to “build the house system” in one shot. This sprint is best executed through narrow, acceptance-driven prompts, which matches the overall project strategy of constraining AI work with clear ownership and done states [file:12][file:13].

### Good task prompt shape
- one subsystem at a time,
- explicit file targets,
- exact classes to create or modify,
- acceptance checklist,
- and a request for temporary debug/test scaffolding.

### Bad task prompt shape
- “Implement the whole importer and graph system.”
- “Figure out the best architecture.”
- “Make the house builder work.”

## Suggested Claude task sequence
A practical order for Claude-assisted implementation is:
1. Contract models and enums.
2. Validation issue/report types.
3. Validator passes.
4. Normalizer and artifact builder.
5. Sample fixtures and edit-mode tests.
6. Debug overlay controller.
7. Grid/room renderers.
8. Portal/stair renderers.
9. Validation/connectivity renderers.
10. Selection panel.
11. Graph-seed export.

This order keeps the most foundational data concerns ahead of visualization and keeps visualization ahead of graph consumers.

## Testing strategy
The sprint should use three testing layers.

### 1. Data tests
Validate parsing, reference resolution, deterministic normalization, and issue generation.

### 2. Visual sanity checks
Load fixtures and verify overlays by screenshot or direct inspection.

### 3. End-to-end smoke test
Load fixture into the test workflow and confirm artifact, debug overlays, and graph-seed export all work in sequence.

## Risks and mitigations

### Risk 1
The importer contract becomes too loosely typed and starts drifting.

### Mitigation
Lock enums/value objects early and fail aggressively on invalid references.

### Risk 2
Debug views become noisy and unreadable.

### Mitigation
Isolated modes first, composite second.

### Risk 3
Normalization hides structural mistakes.

### Mitigation
Require warnings/errors for risky repairs and preserve issue provenance in artifact metadata.

### Risk 4
Claude starts improvising runtime graph state inside importer definitions.

### Mitigation
Keep runtime-vs-definition separation explicit in every task prompt [file:12][file:13][file:14].

## Acceptance checklist
This sprint is successful when all of the following are true:
- canonical importer contract classes exist and compile [file:12][file:13][file:14],
- a sample layout can be imported into canonical definitions,
- structured validation issues are emitted for broken data,
- normalization produces a deterministic `GeneratedHouseArtifact`,
- parsed grid, room ids, portal links, stair links, invalid tiles, and orphan spaces can all be visualized,
- selection tools expose useful metadata for tiles, rooms, portals, and issues,
- graph-seed data is exported from the artifact for future graph integration,
- and Claude-friendly narrow tasks can be assigned from this document without major reinterpretation [file:12][file:13][file:14][file:15].

