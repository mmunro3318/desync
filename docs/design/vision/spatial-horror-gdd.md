# Spatial Horror Prototype — GDD

## Project premise
This project is a Unity prototype horror game centered on spatial instability rather than ghost investigation. The player enters a house that should obey domestic rules, but the layout can shift whenever spaces are unobserved, creating a horror loop built on navigation, uncertainty, and reality drift rather than deduction about a monster class. This pivot keeps the earlier architecture priorities worth preserving: modular systems, data-driven tuning, thin scene objects, and mandatory debug visibility for hidden-state behavior [file:12][file:13][file:14].

The thematic lens is an "asylum" interpreted as seeking reprieve from a reality that keeps changing its rules. The emotional target is not just fear, but disorientation, failed certainty, and the exhaustion of trying to stabilize a world that refuses to remain legible.

## Pillars
1. Spatial horror is the core pillar. The house itself is the main antagonist system, and navigation distortion is the primary source of dread.
2. Readable systemic horror. Changes must feel eerie but still obey rules the player can gradually learn.
3. Modular mutation architecture. New anomalies should mostly be added as data and plug-in logic rather than one-off scene hacks, following the earlier project's data-first philosophy [file:12][file:13].
4. Graybox first, atmosphere second. Lighting, audio, textures, and models should amplify a working spatial loop rather than substitute for one [file:12].
5. Debug-first development. Every hidden system must expose current state, legal mutations, anchor progress, and lock conditions in play mode so tuning is possible [file:13][file:14].

## MVP definition
The MVP is complete when the player can enter a house, experience a stable baseline floorplan, trigger and survive looping spatial anomalies, locate and destroy 3 anchors, and end the round by collapsing the anomaly and restoring the house. The MVP includes one core spatial mechanic first: looping alt-structure traversal between the normal house and a shadow-house interpretation of the same topology. More advanced anomaly categories, expedition systems, and layered simulation loops are explicitly out of scope.

### MVP must include
- First-person exploration with flashlight and basic interaction.
- One baseline house graph with authored rooms and doors.
- One shadow-house / alt-structure layer mapped to the same house graph.
- One looping traversal mechanic that can route the player through impossible continuity.
- Three destroyable anchors as the win condition.
- Match flow with round start, active play, win, fail, and restart.
- Basic audiovisual atmosphere using free library assets.
- Debug overlays for house graph state, active layer, active loop segments, anchor state, and mutation locks.

### MVP must not include
- Expedition, supply, food, battery meta loops.
- Multiple anomaly families beyond the loop/shadow-house core.
- Advanced narrative systems.
- Full procedural house generation.
- Deep multiplayer features as a hard dependency.
- Complex combat.
- More than one simple stalking entity.

## Core player loop
1. Enter the house.
2. Learn its normal rhythm and baseline geography.
3. Notice spatial inconsistencies and exploit or survive them.
4. Traverse between stable house and shadow-house states.
5. Find and destroy 3 anchors.
6. Escape or survive the collapse.

The loop should feel increasingly oppressive as anchors are removed, but the escalation must primarily intensify spatial pressure first. Creature pressure comes later and should reinforce the navigation game rather than replace it.

## Experience goals
- The player should doubt their memory of the layout within the first few minutes.
- The player should learn that observation matters: what is seen is constrained, what is unobserved can drift.
- The player should feel that the house is coherent at a systems level even when impossible at a human level.
- The player should finish one run understanding a simple rule set, not a pile of random scares.

## System overview

### 1. Spatial graph
The runtime owns a canonical house graph: rooms, connectors, doorways, hall segments, and anchor nodes. Scene-authored objects remain thin and define locations, volumes, and visible geometry, while reusable systems own topology logic and routing behavior, matching the earlier architecture pattern of keeping reusable logic out of scene clutter [file:12][file:13][file:14].

### 2. Layer system
The house has at least two structural presentations:
- Base House.
- Shadow House.

Both layers share a compatible topological backbone but differ in route outcomes, gating, presentation, and anchor access. This lets the same authored map produce impossible traversal without requiring a fully procedural world in the MVP.

### 3. Observation lock system
Observed spaces are temporarily locked against mutation. Unobserved connectors or rooms become eligible for reconfiguration. For the MVP, observation should be simplified and deterministic enough to debug clearly: a space is locked when it is inside a validated player visibility set, occupied set, or recent-observation grace timer.

### 4. Loop anomaly system
The first anomaly family is not "everything weird," but a single deep mechanic:
- looping connectors,
- Mobius-like continuity through a familiar hall,
- and alt-routing into the shadow house.

This is the core proof-of-fun mechanic. Additional anomaly families come later only after this one is stable and readable.

### 5. Anchor system
Three anchors exist in the run. Each anchor is hidden behind traversal pressure rather than puzzle complexity. Destroying an anchor should:
- remove one source of spatial instability,
- raise global hostility or distortion,
- and move the player toward the end state.

### 6. Match flow
The project should preserve the earlier benefit of a narrow round-state owner such as a `MatchManager`, because win/loss transitions and restart behavior need a clear orchestration layer rather than being smeared across world scripts [file:4][file:13]. Suggested states:
- Booting
- Exploring
- AnchorDestroyed
- Collapse
- RoundWon
- RoundLost

### 7. Stalker entity (post spatial-core)
After the spatial loop is fun, add one simple stalking entity with FSM behavior, basic model, and simple animation set. It exists to pressure navigation and timing, not to become the primary source of complexity.

## Architecture rules
These rules are preserved from the earlier project because they are still the right rules for this game:
- Runtime state must remain separate from content definitions [file:12][file:13].
- Scene objects stay thin and primarily expose anchors, volumes, portals, and visuals [file:12][file:13].
- New anomaly content should mostly mean new data plus modular evaluators, not engine-wide rewrites [file:12].
- Hidden systems must be inspectable in a debug overlay from the start [file:13][file:14].

### Suggested data assets
- `HouseGraphDefinition`
- `RoomDefinition`
- `ConnectorDefinition`
- `LayerDefinition`
- `AnomalyDefinition`
- `AnchorDefinition`
- `MatchRulesDefinition`
- `StalkerDefinition` (post-MVP spatial core)

### Suggested runtime owners
- `MatchManager`
- `SpatialGraphRuntime`
- `LayerStateController`
- `ObservationLockSystem`
- `AnomalyDirector`
- `AnchorManager`
- `DebugOverlay`
- `StalkerController` (later)

## Folder and scene direction
The earlier folder and scene discipline is still useful: a boring, predictable Unity structure reduces drift and makes AI-assisted code generation more reliable [file:13][file:14]. Preserve the scene model of `Bootstrap`, `Test`, `House_Graybox`, and `House_Prototype`, because it cleanly separates startup flow, isolated testing, graybox validation, and the evolving main play scene [file:13][file:14].

Recommended adaptation under `Assets/_Project/`:
- `Art/Materials`, `Models`, `Audio`, `VFX`, `Animations`, `Prefabs`
- `Data/Spatial`, `Data/Anomalies`, `Data/Match`, `Data/AI`, `Data/UI`
- `Scenes/Bootstrap`, `Scenes/Test`, `Scenes/House_Graybox`, `Scenes/House_Prototype`
- `Scripts/Core`, `Player`, `Interaction`, `World`, `Spatial`, `Match`, `AI`, `UI`, `Audio`, `Editor`

## Milestones

### Milestone 0 — Foundation salvage
Goal:
- stabilize existing player controller, flashlight, lighting, and interaction shell,
- preserve current prototype house,
- restore reliable local test flow,
- and keep multiplayer troubleshooting out of the MVP critical path.

Question answered:
- does it feel good to exist inside the house as a horror space at all?

### Milestone 1 — Spatial core vertical slice
Goal:
- canonical house graph,
- base house + shadow house layers,
- one loop anomaly family,
- observation lock rules,
- debug graph visibility.

Question answered:
- is spatial horror through looping impossible continuity actually fun?

### Milestone 2 — MVP loop
Goal:
- 3 anchors,
- anchor destruction flow,
- escalation rules,
- round win/loss flow,
- restart loop,
- repeated playtest stability.

Question answered:
- does the full round work as a coherent game and not just a cool tech demo?

### Milestone 3 — Atmosphere and free assets
Goal:
- free library textures,
- basic environment models,
- ambient audio and SFX,
- minimal UI pass,
- stronger visual distinction between base and shadow house.

Question answered:
- does atmosphere amplify the spatial loop without obscuring readability?

### Milestone 4 — Stalker pass
Goal:
- one FSM stalker,
- basic model,
- basic locomotion/idle/alert animation,
- pressure behavior linked to anchor progression.

Question answered:
- does a single threat improve the loop after the house itself already works?

## Post-MVP roadmap
Only after the MVP is stable should you add:
- room mutation sets,
- room multiplication,
- house sprawl/expansion,
- stretched hallways,
- alien geometry,
- alt-world overlays,
- richer multiplayer observation rules,
- and more bespoke entities.

This sequencing follows the earlier roadmap pattern of introducing content in narrow slices and proving one major question at a time before broadening the system surface area [file:12][file:15].

## Asset strategy
Use free library assets for the MVP:
- textures for walls, floors, decay, mold, plaster, wallpaper, grime,
- simple environment props,
- one simple humanoid or abstract stalker model,
- footstep, ambience, impact, whisper, distant house-settling audio,
- and minimal animation packs for idle, walk, pursuit, and hit/react if needed.

Asset selection should obey one rule: readability over abundance. The shadow-house layer should be legible at a glance through texture, lighting, and audio treatment rather than by flooding the player with too many prop variations.

## Dev workflow for Claude Code
Claude should be fed a narrow, high-context doc stack per sprint because broad ambiguous prompts increase the chance it gets stuck in Unity implementation churn. The earlier checklist-driven docs worked because they defined ownership, acceptance tests, and explicit phase boundaries; preserve that format here [file:1][file:13].

Per sprint, provide Claude:
1. One current sprint PDD.
2. One architecture/contracts doc.
3. One scene hierarchy doc.
4. One debug expectations doc.
5. One Unity docs/context source list.

Task framing should follow this pattern:
- objective,
- files to create or edit,
- contracts/interfaces touched,
- acceptance tests,
- non-goals.

## MVP acceptance criteria
The MVP is complete when all of the following are true:
- The player can enter the house and begin a live round.
- The game supports a baseline authored house and a shadow-house variant.
- Looping traversal can route the player through impossible but rule-bound continuity.
- Observation locks prevent or permit mutation in clearly debug-visible ways.
- Three anchors can be found and destroyed in one run.
- Destroying all anchors resolves the anomaly and ends the round cleanly.
- The game is playable across repeated runs without blocker bugs.
- Basic free-library art, textures, and sound make the house legible and tense.
- The spatial loop is fun before the stalker is added.

## Key risks
- Overbuilding procedural generation before the loop mechanic is fun.
- Letting the house become random rather than interpretable.
- Implementing too many anomaly families at once.
- Treating the stalker as core before the spatial system works.
- Failing to build debug tools for mutation eligibility, active routes, and observation state.

## One-line pitch
A first-person spatial horror prototype where an ordinary house folds into a looping shadow-structure whenever nobody is looking, and the player must destroy three anchors to make reality hold still long enough to escape.
