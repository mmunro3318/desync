# Debug and Visualization Spec

## Document objective
Define the debug and visualization layer for house import, layout parsing, graph integration, and early spatial validation so developers can immediately inspect parsed grids, room ids, portal links, stair links, invalid tiles, and orphan spaces. This spec exists because hidden-state spatial systems become extremely hard to tune when structural truth is invisible or split across tools [file:12][file:13][file:14][file:15].

## Why this spec exists
The project now has importer contracts, graph integration rules, room semantics, and planned mutation systems. All of that complexity is manageable only if the intermediate spatial representations are visible before runtime horror logic gets layered on top. A debug view should make it obvious what the importer thinks the house is, what the graph thinks the house is, and where those interpretations disagree [file:12][file:13][file:14].

## Core question answered
How should the project visualize imported and normalized house structure so broken layout data, graph mistakes, and invalid topology are found early instead of surfacing later as “weird gameplay bugs” [file:12][file:13]?

## Debug philosophy
The project has repeatedly established that hidden systems must be inspectable and that debug visibility is part of the architecture, not an afterthought [file:12][file:13][file:15]. This spec applies that same philosophy to house import and graph construction specifically.

## Scope
This spec covers:
- parsed grid visualization,
- room id labeling,
- portal link visualization,
- stair link visualization,
- invalid tile visualization,
- orphan-space visualization,
- debug ownership and toggles,
- and validation-facing overlays.

## Out of scope
This spec does **not** define:
- final player-facing map UI,
- shipping accessibility map features,
- runtime horror effect debug for every gameplay system,
- or full anomaly playback tooling.

It focuses on structural import/build visibility.

## Primary debug goals
The debug system should answer these questions quickly:
- What tiles were parsed?
- Which room does each valid space belong to?
- Which portals connect which regions?
- Where do stairs connect across floors?
- Which tiles are invalid or contradictory?
- Which traversable spaces are orphaned or disconnected?

If a developer cannot answer those questions in under a minute, the debug layer is insufficient.

## Required visualizations
This spec treats the following as mandatory for the first pass:
- show parsed grid,
- show room ids,
- show portal links,
- show stair links,
- show invalid tiles,
- show orphan spaces.

## Visualization ownership
The debug system should read authoritative imported and normalized data, but should not define it. That keeps ownership aligned with the broader project rule that debug views visualize hidden state without becoming the game logic itself [file:12][file:13].

### Recommended ownership split
| System | Owns |
|---|---|
| Importer / Normalizer | Parsed and normalized house data |
| Validation layer | Issues, severities, and broken references |
| Graph integration | Node/portal/stair resolution |
| Debug visualization layer | Rendering these truths clearly |

## Display modes
The debug UI should expose a small set of explicit display modes instead of one overloaded “show everything” view.

### Recommended modes
- `Grid`
- `Rooms`
- `Portals`
- `Stairs`
- `Validation`
- `Connectivity`
- `Composite`

### Rule
`Composite` is useful, but every important layer must also be viewable in isolation.

## Parsed grid view
The parsed grid view is the foundation because every later visualization depends on confidence that the importer understood the tile field correctly.

### It should show
- all parsed cells,
- cell coordinates,
- primary cell type coloring,
- walkable vs non-walkable distinction,
- floor index,
- and optional wall/threshold markings.

### Rendering guidance
- Use consistent tile-aligned quads, gizmos, or editor overlays.
- Keep cell colors semantically stable between sessions.
- Make grid coordinates readable on zoom-in, not always-on at full density.

### Goal
A developer should immediately see whether the imported footprint matches the intended source layout.

## Color guidance for parsed grid
Use restrained, stable colors rather than stylistic gradients.

### Suggested baseline palette logic
- walkable interior: muted neutral or cool gray-blue,
- hall/transition cells: slightly distinct neutral accent,
- threshold cells: warm accent,
- walls/blocked cells: dark neutral,
- void/exterior: very faint neutral,
- stairs: strong but readable highlight,
- invalid tiles: saturated warning color,
- orphan spaces: distinct caution color different from invalid.

### Rule
Color semantics must stay stable across all debug modes so a “bad tile” never changes meaning by overlay.

## Room id view
The room id view should help validate semantic grouping and membership.

### It should show
- room boundaries or colored room fills,
- room ids,
- room display names where space allows,
- room type,
- and optional member-cell counts.

### Rule
Room ids should be visible at both cell level and room centroid level when possible. This helps catch split rooms, accidental overlaps, and membership leakage.

## Room label placement guidance
Use centroid labels when the room is large enough, and fall back to edge labels or inspector hover when the room is tiny. Labels should prefer readability over precision artfulness.

## Portal link view
Portal links are one of the most important structural overlays because impossible-space gameplay depends heavily on connector truth [file:12][file:13].

### It should show
- portal ids,
- source and target room ids or region refs,
- portal type,
- orientation,
- bidirectional vs one-way state,
- and unresolved endpoints when present.

### Visual form
- draw a marker at the portal location,
- draw a link line or arrow between connected spaces,
- and label the line with portal id on hover or selected state.

### Rule
Portal links should remain inspectable even when room fills are disabled.

## Stair link view
Stairs are often the first place multi-floor truth goes bad, so they need explicit debugging.

### It should show
- stair portal ids,
- source floor and target floor,
- source and destination landing cells or room ids,
- direction of traversal if asymmetric,
- and unresolved stair endpoints.

### Visual form
- use vertical linking indicators,
- floor-jump arrows,
- or paired markers with same stair id across floors.

### Rule
A developer should be able to stand on one floor’s debug view and understand where the stair resolves on the next floor without mentally guessing.

## Invalid tile view
Validation errors should have spatial representation, not just console logs.

### Invalid tile examples
- out-of-bounds coordinates,
- contradictory cell type flags,
- tile references nonexistent room,
- threshold cell with no portal,
- blocked cell marked walkable,
- overlapping ownership,
- or malformed stair markers.

### It should show
- invalid tile location,
- issue code,
- issue severity,
- and a short reason.

### Rule
Invalid tiles should be the loudest visual signal in the system. Broken data must be impossible to miss.

## Orphan space view
Orphan spaces are valid-looking spaces that fail connectivity expectations.

### Orphan examples
- walkable region disconnected from all intended traversal,
- room area with no portal path,
- isolated hall island,
- stair landing that cannot connect onward,
- or a room whose member cells do not reach its declared connector set.

### It should show
- disconnected region outline/fill,
- orphan group id,
- size or cell count,
- nearest expected connection if computable,
- and related validation issue links.

### Why this matters
These are especially dangerous because they may not look “wrong” in the raw grid until traversal or graph logic fails.

## Connectivity visualization
A dedicated connectivity view should help differentiate invalidity from disconnection.

### It should show
- connected components,
- main reachable component,
- orphan component colors,
- and route traces from a selected cell or room.

### Rule
This mode should make it trivial to see whether the importer created multiple isolated navigable islands by mistake.

## Composite view
The composite view is the high-information mode for deeper debugging.

### It may combine
- room fill,
- room ids,
- portals,
- stairs,
- invalid tiles,
- orphan spaces,
- and selected-cell metadata.

### Rule
Composite mode should be layered and filterable, not a permanent wall of noise.

## Interaction model
The debug layer should support both passive viewing and lightweight inspection.

### Minimum interactions
- toggle overlay categories,
- click/select tile,
- click/select room,
- click/select portal,
- hover to inspect metadata,
- jump between related issues,
- and switch floors.

### Nice-to-have interactions
- focus camera on selected issue,
- isolate connected component,
- highlight route between two rooms,
- and open validation issue detail panel.

## Selection panel
Selection should populate a structured inspector panel.

### Tile selection should show
- cell id,
- coordinates,
- floor,
- cell type,
- walkability,
- room id,
- portal refs,
- and validation issues.

### Room selection should show
- room id,
- room type,
- display name,
- member-cell count,
- connected portals,
- neighboring rooms,
- and disconnected-region status.

### Portal selection should show
- portal id,
- portal type,
- endpoints,
- orientation,
- bidirectional state,
- floor relation,
- and unresolved endpoint warnings.

## Floor navigation
Multi-floor layouts need explicit floor controls.

### Requirements
- switch floor index directly,
- step up/down floors,
- show all floors faded if needed,
- and support paired stair highlighting across floors.

### Rule
Never hide the existence of off-screen stair destinations when a stair is selected.

## Validation integration
The visualization system should bind directly to the structured validation contract established in the importer data model. Errors and warnings should be selectable from the validation report and reflected spatially where possible [file:12][file:13][file:14].

### Good integration behavior
- clicking a validation issue highlights the relevant tile/room/portal,
- issue severity affects visual styling,
- missing references show both textual issue and spatial fallback highlight,
- and unresolved references stay visible even if the related object is only partially built.

## Recommended severity styling
- `Info`: subtle marker or blue/cyan outline,
- `Warning`: amber/gold highlight,
- `Error`: strong orange-red highlight,
- `Fatal`: red + pulsing or persistent attention style.

### Rule
Severity styling should reinforce the validation contract without becoming visually ambiguous.

## Editor vs runtime support
The most useful implementation is likely dual-mode.

### Editor mode
- inspect imported data before play,
- validate assets,
- debug generation output quickly,
- and annotate issues during authoring.

### Runtime/dev scene mode
- verify that imported truth survived instantiation,
- compare parsed layout against scene realization,
- and support Claude-assisted screenshot/test workflows.

This matches the project’s preference for graybox-first iteration and inspectable hidden systems [file:12][file:13][file:15].

## Suggested Unity placement
This spec fits the current project structure best if split between editor and runtime debug locations [file:13][file:14].

### Likely folders
- `Assets/_Project/Scripts/Editor/HouseImport/`
- `Assets/_Project/Scripts/World/Rooms/Debug/`
- `Assets/_Project/Scripts/UI/Debug/`
- `Assets/_Project/Prefabs/Debug/`

## Suggested script responsibilities
A narrow, modular set of debug classes will fit the project better than one giant overlay manager [file:12][file:13][file:14].

| Class | Responsibility |
|---|---|
| `HouseDebugOverlayController` | Top-level toggles and mode switching |
| `GridDebugRenderer` | Draw parsed tile grid |
| `RoomDebugRenderer` | Draw room fills and room ids |
| `PortalDebugRenderer` | Draw portal markers and links |
| `StairDebugRenderer` | Draw stair links across floors |
| `ValidationDebugRenderer` | Draw invalid tiles and issue severity |
| `ConnectivityDebugRenderer` | Draw orphan spaces and connected components |
| `DebugSelectionPanel` | Show selected object metadata |

## Performance and readability rules
The debug layer does not need final-game performance, but it still needs discipline.

### Rules
- avoid always-on text for every tile at full zoom,
- cull labels by zoom level,
- render overlays deterministically,
- and ensure one mode can be understood without the others.

### Most important rule
Readability beats completeness. A partially filtered overlay that explains the problem is better than an everything-overlay that hides it.

## Acceptance criteria
This spec is successful when:
- developers can view the parsed grid and confirm importer footprint quickly [file:12][file:13],
- room ids and room membership errors are visually obvious,
- portal and stair connectivity can be inspected without reading raw data objects,
- invalid tiles surface with clear severity and reason,
- orphan spaces are distinguishable from merely unusual but valid spaces,
- validation issues can be navigated spatially,
- and the debug layer supports graybox iteration before full gameplay systems depend on the layout [file:12][file:13][file:15].
