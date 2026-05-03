# Impossible House Graybox Vertical Slice Plan

## Document objective
Define the first integrated playable slice of the impossible-house prototype. This document turns the existing architecture stack into one graybox milestone that proves the player can enter an impossible house, navigate locally coherent but globally unstable space, observe at least one legal spatial mutation, and complete a minimal investigation objective inside a debuggable first-person prototype [file:12][file:13][file:14][file:15].

## Why this document exists
The project now has several subsystem-level documents: house graph, observation locks, spatial mutation, portal visibility, and portal visibility sprint planning. The next step is not another isolated subsystem. The next step is to define a single integrated playable target that proves those systems can work together inside one scene and one player loop [file:12][file:13][file:15].

## Vertical slice thesis
The graybox vertical slice should answer one question clearly: **is the impossible-house loop already compelling before art polish and content breadth?** The slice succeeds if a player can feel the core fantasy of entering a house whose local reality remains believable while its deeper topology shifts according to observation rules, and can complete one small objective inside that pressure [file:12][file:13][file:15].

## What this slice should prove
This slice should prove all of the following in one playable build:
- first-person movement and interaction feel acceptable inside the house [file:12][file:13][file:15],
- room/node topology exists as authoritative graph truth [file:12][file:13],
- at least one spatial mutation family works legally and visibly affects navigation [file:12][file:13],
- portal visibility and threshold crossing preserve local camera believability [file:12][file:13],
- a minimal objective gives the player a reason to navigate the anomaly [file:12][file:15],
- and debug tools expose enough hidden state to tune the whole loop [file:12][file:13][file:15].

## Out of scope
This vertical slice does **not** require:
- final art,
- final audio,
- full multiplayer,
- final creature behavior suite,
- multiple dimension-layer systems,
- inventory complexity,
- or a full expedition meta-loop.

It is a narrow, honest proof slice.

## Product statement for the slice
A player enters a graybox house containing a small but impossible spatial network. The player explores with a flashlight, traverses locally coherent thresholds, and must locate and destroy a single destabilizing artifact while the house legally mutates when space is unobserved. The run ends in success when the artifact is destroyed and the exit becomes stable, or in failure if the player is caught by the pressure system or trapped by resource loss [file:12][file:13][file:15].

## Slice design pillars

### 1. Local coherence
The immediate area around the player should feel spatially trustworthy even when the house is globally impossible [file:12][file:13].

### 2. Navigational unease
The player should notice that routes, door outcomes, or corridor lengths cannot be trusted once observation is broken [file:12][file:13].

### 3. Readable objective pressure
The player must have a clear reason to push deeper rather than just admire the tech [file:12][file:15].

### 4. Debuggability
Every hidden state that materially affects play should be visible during development [file:12][file:13][file:15].

## Player fantasy in the slice
The fantasy is not “shoot a monster” or “solve a puzzle box.” The fantasy is: enter a place that refuses stable meaning, learn how its rules betray expectation, and force a path to resolution before the house fully owns the route you thought you understood. That keeps the slice aligned with the broader shift away from ghost-clone structure and toward spatial horror rooted in unstable reality [file:12][file:13].

## Minimal player loop
The slice should support a compact loop:
1. Enter the house and learn the basic layout.
2. Discover that topology is unstable under specific observation conditions.
3. Use navigation and observation skill to reach a deeper node containing the artifact.
4. Destroy or disable the artifact.
5. Escape through the now-stabilized exit.

This loop is narrow enough for graybox development but strong enough to reveal whether the core concept has tension [file:12][file:15].

## Minimum playable content
The slice should include:
- one house seed,
- one small connected graph,
- one mutation family implemented well,
- one artifact objective,
- one pressure source,
- one entry and one exit state,
- and one debug HUD [file:12][file:13][file:15].

## Scene target
Use `House_Prototype` as the integrated scene target, with `Bootstrap` and `Test` preserved for startup and isolated testing, following the previously established scene plan [file:13][file:14].

## Suggested scene composition
The graybox slice should remain small and intentional.

### Recommended node count
- 5 to 8 meaningful nodes,
- 1 entry node,
- 1 loop corridor or remap candidate,
- 1 deeper objective node,
- 1 optional shortcut or misleading return path.

This is large enough to produce disorientation but small enough to inspect thoroughly.

## Suggested graph layout
A practical first layout could be:
- Entry foyer,
- main hall,
- side room,
- stair/hall extension,
- loop corridor,
- artifact room,
- unstable return connector.

This supports route confusion without requiring a huge house.

## Minimal system stack required
The vertical slice depends on these systems being present, even if still graybox-simple [file:12][file:13][file:14][file:15].

| System | Slice responsibility |
|---|---|
| Player movement/interaction | lets the player exist, move, and use the flashlight/objective interaction |
| House graph runtime | authoritative node and portal truth |
| Observation lock system | determines whether mutation is currently allowed |
| Spatial mutation runtime | applies one legal topology change family |
| Portal visibility/local render | keeps thresholds believable from first-person view |
| Objective system | tracks artifact state and unlocks exit condition |
| Pressure system | creates risk, urgency, or pursuit |
| Debug overlay | exposes hidden house state |

## What can be stubbed
To keep the slice honest but achievable, the following can remain minimal:
- inventory beyond flashlight + objective interaction,
- one placeholder pressure entity or even non-entity pressure,
- minimal UI text,
- one artifact prefab,
- one exit state machine,
- placeholder sounds and materials.

## Recommended pressure model
Do **not** overbuild the creature yet. For the graybox slice, use the cheapest pressure model that still creates urgency.

### Good pressure options
- a simple stalking entity with basic pursuit,
- an abstract hazard state that increases when the player lingers,
- or route closure/escalation pressure tied to mutation count.

### Recommendation
Use a lightweight **house pressure meter plus one occasional stalker manifestation** rather than a full AI creature suite. That protects scope while still testing fear and urgency [file:12][file:15].

## Objective model
Use one artifact objective only.

### Artifact behavior
- hidden in one deeper node,
- discoverable by navigation rather than deduction UI,
- destroyable through one simple interaction,
- destroying it stabilizes the exit route.

This gives the player a concrete purpose while keeping the loop focused [file:12][file:15].

## Mutation scope for the slice
Use only one mutation family in the integrated slice.

### Best candidate
A **corridor/door remap or short loop mutation** is the best first vertical-slice mutation because it clearly affects navigation, is easy to perceive, and works directly with your graph and portal docs [file:12][file:13].

### Avoid for now
- multiple world layers,
- large Tardis interiors,
- complex cross-dimensional player visibility,
- many simultaneous mutation families.

## Observation rule in the slice
The slice only needs one strong observation law.

### Recommended law
A portal connection or hallway route may mutate only when the relevant spaces are not actively observed by the player’s camera according to the observation-lock rules defined in the supporting docs [file:12][file:13].

That is enough to create the signature feel without adding too many competing rules.

## Portal/render expectations
Thresholds are critical in this slice. The player must be able to walk through doors and look down halls without catching obvious visual contradictions, which is why the local render and portal visibility harnesses are mandatory instead of optional polish [file:12][file:13][file:15].

## Slice-specific debug requirements
This vertical slice is not done unless a developer can inspect the hidden state live [file:12][file:13][file:15].

### Required debug panels
- current node id,
- current active set,
- portal destination mapping,
- observation-lock state,
- mutation eligibility,
- last mutation event,
- artifact state,
- exit state,
- pressure level,
- and any forced debug commands for mutation/lock toggles.

## Mandatory player-facing interactions
The slice needs only a few interactions, but they must work cleanly.

### Required interactions
- open/use threshold door if applicable,
- flashlight toggle,
- interact with artifact,
- exit interaction once stabilized,
- optional one debug/dev interaction for forcing events during tests.

## Suggested classes in play
This slice should rely on already planned focused systems rather than inventing one vertical-slice manager blob [file:12][file:13][file:14].

### Likely active classes
- `PlayerInputRouter`
- `PlayerMotor`
- `PlayerLook`
- `PlayerInteractor`
- `HUDController`
- `HouseGraphRuntime` or equivalent
- `ObservationLockService`
- `SpatialMutationService`
- `LocalVisibilityService`
- `PortalVisibilityResolver`
- `NodeStreamingController`
- `ThresholdTransitionController`
- `ArtifactObjectiveController`
- `HousePressureController`
- `DebugOverlay`

## Slice milestone breakdown
This vertical slice should be built through a few integrated milestones rather than one giant merge.

### VS-0 — Movement and interaction baseline
Question answered: does the player feel good inside the graybox house shell [file:12][file:13][file:15]?

Deliverables:
- working first-person movement,
- flashlight,
- threshold interaction,
- artifact interaction placeholder,
- HUD prompt and debug toggle.

### VS-1 — House graph playable shell
Question answered: can the graybox house already run on graph truth rather than just scene arrangement [file:12][file:13]?

Deliverables:
- 5–8 node graph bound to scene,
- stable node/portal ids,
- node binding validation,
- debug graph display.

### VS-2 — Observation plus mutation proof
Question answered: can one legal mutation family change route truth without the player catching obvious contradictions [file:12][file:13]?

Deliverables:
- observation lock integration,
- one mutation family,
- mutation debug commands,
- mutation history in overlay.

### VS-3 — Portal/local render proof
Question answered: does first-person threshold traversal remain believable [file:12][file:13]?

Deliverables:
- local active-set computation,
- portal resolution,
- node streaming state application,
- threshold continuity support,
- portal debug overlays.

### VS-4 — Objective pressure loop
Question answered: is there already a fun enough reason to navigate the impossible space [file:12][file:15]?

Deliverables:
- artifact objective,
- exit unlock rule,
- pressure meter or simple stalker,
- win/fail state.

### VS-5 — Integrated playtest pass
Question answered: is the slice worth evolving into the jam MVP?

Deliverables:
- one stable run from entry to exit,
- debug usability pass,
- issue log from 3–5 playtests,
- list of post-slice priorities.

## Acceptance criteria for the slice
The graybox vertical slice is successful when all of the following are true:
- the player can enter and navigate the house in first person with readable controls [file:12][file:13][file:15],
- the environment is backed by graph/runtime truth rather than purely ad hoc scene layout [file:12][file:13],
- at least one mutation changes reachable path or destination behavior legally [file:12][file:13],
- the player does not see obvious threshold contradictions during ordinary play [file:12][file:13],
- the player can complete a single artifact objective and unlock an exit [file:12][file:15],
- a failure state or escalating pressure exists [file:12][file:15],
- and the developer can inspect the critical hidden state live in debug [file:12][file:13][file:15].

## Non-goals for acceptance
The slice does **not** fail because:
- art is ugly,
- the stalker is simplistic,
- content breadth is low,
- or audio is placeholder.

It fails only if the core fantasy is not readable, not playable, or not tunable.

## Suggested playtest questions
Use simple questions after each test:
- Did the player notice the house behaving impossibly?
- Did the player understand what they were trying to achieve?
- Did the player feel tricked in a cool way or just confused?
- Did thresholds feel believable?
- Did the pressure source add tension or just noise?
- Did the objective give enough reason to keep moving?

These questions focus on whether the slice is emotionally legible, not just technically functional.

## Recommended asset stance
Stay graybox-first. Use primitive geometry, clear lighting contrast, one strong flashlight, a few readable landmark props, and minimal material variation, because the purpose is to evaluate loop clarity and spatial manipulation before expensive content work [file:12][file:13][file:15].

## Recommended Claude workflow
This slice should be built as a sequence of narrow integration tasks with explicit acceptance conditions, following the broader lesson from your earlier docs that AI development works best when ownership and “done” are concrete [file:12][file:13][file:15].

### Good task shape
- wire one system into the slice scene,
- define scene objects and bindings,
- define tests/debug outputs,
- verify one run path works.

### Bad task shape
- “Build the vertical slice.”
- “Make the impossible house playable.”
- “Integrate all systems.”

## Suggested Claude task order
1. finalize graybox node list and ids,
2. bind node roots and portal ids in scene,
3. verify player/controller/flashlight interactions,
4. integrate graph runtime into scene bootstrap,
5. integrate observation-lock debug state,
6. integrate one mutation family and force-test tools,
7. integrate local visibility and streaming,
8. integrate portal threshold continuity,
9. implement artifact objective,
10. implement exit unlock,
11. add pressure model,
12. run graybox playtest checklist,
13. log defects and tune thresholds.

## Risks and mitigations

### Risk 1
The slice becomes a bag of half-working subsystems.

### Mitigation
Require every milestone to end in a playable run state.

### Risk 2
The mutation tech works but the player has no reason to care.

### Mitigation
Add the artifact objective early rather than as a final afterthought.

### Risk 3
The render harness hides contradictions but makes the house unreadable.

### Mitigation
Playtest for navigational comprehension, not just contradiction absence.

### Risk 4
The pressure system explodes scope.

### Mitigation
Use the cheapest pressure model that creates urgency and delay creature complexity.

## What success unlocks next
If this slice works, you will have real evidence that the project’s identity is viable. From there, the roadmap can safely expand into:
- more mutation families,
- stronger artifact/seal loops,
- deeper pressure entities,
- better landmarking and environmental storytelling,
- and co-op observation/sync rules [file:12][file:13][file:15].

## Recommended next document
After this vertical slice plan, the strongest next doc is **Co-op Observation and Sync Rules Spec**, because once the single-player impossible-space slice is defined, the hardest next design problem is preserving local truth when more than one player is observing different spaces at once [file:12][file:13].

