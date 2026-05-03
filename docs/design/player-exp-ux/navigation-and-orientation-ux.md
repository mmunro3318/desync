# Navigation and Orientation UX

## Document objective
Define how players understand, lose, recover, and negotiate spatial orientation inside a mutating house without relying on a player-facing map. This document specifies the navigation contract for room readability, breadcrumb tools, environmental legibility, false familiarity, multiplayer route-sharing, and the strict boundary between player-facing orientation and developer-facing graph debug [file:12][file:13][file:14][file:15][cite:31][cite:32].

## Why this doc exists
Navigation is not a support feature in this game. It is one of the primary horror systems. Earlier project planning already treated rooms, room topology, line-of-sight spaces, and debug visibility as foundational; this document takes that further by defining how navigation should feel to the player once the house begins mutating and the graph becomes unstable [file:12][file:13][file:14][file:15].

## Core navigation principle
Players should usually know **where they are approximately**, but should not always trust **how spaces connect right now**. The game should attack route confidence more than raw location awareness. Disorientation should come from unstable continuity, not from meaningless visual sameness or unreadable level art [cite:31][file:12].

## Orientation goals
A strong navigation UX should support all of the following at once:
- players can form a provisional mental map,
- that mental map can become unreliable under mutation,
- players can use tools and collaboration to restore partial trust,
- and fear emerges from maintaining orientation under instability rather than from being dropped into noise.

## What navigation should feel like
The intended feeling is not “I have no idea where I am.” It is closer to:
- “I think I know where I am, but I no longer trust the route.”
- “This place is almost familiar, which is worse.”
- “If I do not mark this now, the house will erase my certainty.”
- “I know home base exists, but one bad turn could detach me from it forever.”

That distinction matters because it keeps the game tense and analyzable instead of merely confusing [cite:31].

## No-map policy
Players should not receive an automap, minimap, floor plan, compass route, or objective path line during play [cite:31].

### Why
A player-facing map would collapse the game’s core tension by turning a hostile space into a solved diagram. The house should be navigated through memory, marks, visible landmarks, route habits, and conversation rather than through abstract omniscient UI [cite:31][file:12].

### Exception
A full graph/debug map is allowed for development only. It should remain clearly debug-gated and never appear in the fiction-facing experience [cite:31][file:13][file:15].

## Orientation layers
Navigation should be supported through five layered sources of information.

### 1. Base architectural legibility
Even before anomalies occur, the baseline house must be readable enough to navigate. Earlier docs already emphasized that maps should function first as room topology and tension containers, not just as art sets [file:12].

This means the base house should provide:
- recognizable room identities,
- discernible transitions between room types,
- meaningful door/hall/corner relationships,
- and a sense that spaces belong to a coherent domestic or site-like structure.

### 2. Local environmental cues
Players should use doors, corners, furniture silhouettes, lighting changes, wall damage, sound pockets, and visual landmarks to form a memory of route sequences.

### 3. Player-authored breadcrumbs
Chalk, spray paint, chemlights, dropped supplies, and later tools should allow players to externalize memory into the world [cite:31].

### 4. Social orientation
Players should use speech, witnessing, and co-presence to negotiate route truth with each other rather than receiving interface-driven synchronization [cite:31][cite:32].

### 5. Developer-only graph inspection
For iteration and debugging, the graph state must remain inspectable so mutations can be understood and tuned without guesswork [file:13][file:15].

## Base house readability rules
The starting house must be legible enough that getting lost later feels like the house’s fault, not the level’s fault.

### Rules
- Rooms should have stable base identities.
- Hallways should differ in length, shape, or landmark composition enough to be learnable.
- Doors and thresholds should create memorable transitions.
- Major spaces should not all share the same proportions, dressing rhythm, and lighting.
- Early runs should let players establish a trustworthy baseline mental model before that model is violated.

This aligns with the earlier requirement that room semantics be clean from the start and that the house feel worth existing in before deeper mechanics are layered on top [file:12][file:15].

## Room identity and semantic anchoring
Every room should carry at least one strong identity anchor. Earlier planning already centered `RoomVolume`, `RoomDefinition`, and room metadata as foundational authoring tools; navigation UX should treat those systems as legibility primitives, not just technical labels [file:13][file:14].

### Room identity can come from
- plan shape,
- doorway arrangement,
- dominant furnishing silhouette,
- repeated object cluster,
- lighting temperature,
- floor/wall material contrast,
- sound character,
- or narrative residue.

### Rule
A player should be able to say “the narrow red hall,” “the kitchen-like room with the island,” or “the room with the broken lamp” even when a full route is uncertain. That kind of language is essential for multiplayer verbal navigation [cite:31][cite:32].

## Threshold readability
In a game about shifting adjacency, thresholds matter as much as rooms.

### Doorways, arches, and turns should communicate
- what kind of space they seem to lead into,
- whether the transition feels ordinary or suspect,
- whether the route is narrow, open, looping, or concealed,
- and whether crossing the threshold likely commits the player to greater risk.

### Design implication
Thresholds should be readable enough that players can make intentional decisions about depth and retreat, even when the house later betrays those decisions.

## False familiarity
One of the game’s strongest orientation tools should be *near sameness*. A space should often feel almost—but not perfectly—known [cite:31].

### Techniques
- repeat a hall with one altered landmark,
- shift the order of familiar props,
- preserve a doorway but change what lies beyond it,
- keep room silhouette but alter access paths,
- reuse a texture set while changing scale or adjacency.

### Rule
False familiarity should create suspicion and pattern strain, not cheap gotcha confusion. The player should be able to notice that something is wrong if they are attentive.

## Breadcrumb tools
Breadcrumb tools are the main player-authored orientation system [cite:31].

### Required tool families
- **Surface marks:** chalk, spray paint, charcoal, tape, or equivalent.
- **Placed beacons:** chemlights, candles, battery lights, signal pegs, or equivalent.
- **Dropped caches:** deliberate item placement as route memory.
- **Future advanced tools:** probes, drones, directional listeners, temporary mapping instruments.

### Design rules
- Surface marks should be quick and cheap.
- Beacon placement should be readable at a glance in darkness.
- Marks should persist long enough to matter.
- Marks should be local, not magically visible through walls.
- Players should be able to develop personal marking conventions.

### Important tension rule
Breadcrumbs should support orientation, but never guarantee safety. A marked path can still mutate, loop, split, or become dangerous. The tools preserve memory, not certainty [cite:31].

## Mark readability rules
A useful mark must answer one of these questions quickly:
- Have I been here before?
- Did I come from this direction?
- Was this route considered safe or unsafe?
- Is this path part of our return chain?
- Is this a dead end, loop, or suspect threshold?

### Suggested conventions to support
- single mark = visited,
- double mark = route to retreat,
- crossed mark = do not trust,
- beacon cluster = temporary rally point.

The game does not need to hardcode all of these meanings, but the tools should make this sort of player-created language natural.

## Lighting as orientation
Lighting should support route memory before it supports spectacle. Distinct light pools, color temperature differences, practical lamp groupings, and darkness pockets can all help players build local spatial memory.

### Rules
- light variation should create navigable character,
- not every corridor should share the same value structure,
- and the house should sometimes weaponize lighting by preserving silhouette familiarity while altering path truth.

## Sound as orientation
Sound should help players estimate occupancy, distance, openness, and teammate presence without turning into a sonar minimap.

### Sound should help communicate
- whether a corridor opens into a larger volume,
- whether another player is nearby,
- whether a known room still sounds “like itself,”
- whether a placed beacon or device is active,
- and whether a route is becoming hostile or unstable.

Localized voice is especially important because the project is verbal-communication-first [cite:31][cite:32].

## Multiplayer orientation
Because the game is multiplayer-first, navigation should support partial, uneven, and contested understanding [cite:31][cite:32].

### Desired co-op situations
- one player holds a threshold while another scouts,
- two players disagree about whether a route changed,
- a player calls out a mark or room identity for others to find,
- a separated player tries to return using only memory and speech,
- a witness stabilizes confidence even when they cannot stabilize the route itself.

### UX rule
The game should give players enough shared vocabulary and legibility to talk about space, but not enough interface authority to bypass that conversation.

## Route trust model
Navigation should be thought of as a sliding scale of trust, not a binary of “known” and “unknown.”

### A route may be
- unvisited,
- visited but unmarked,
- marked and tentatively trusted,
- marked but contradicted,
- known unstable,
- or thought safe but recently suspicious.

This is useful for both design and debug because it frames navigation as evolving player confidence rather than simple maze completion.

## Safe boundary and home-base orientation
If the session includes a safe boundary such as an entry zone, van, tent, or outer edge, that boundary must remain emotionally and spatially important [cite:31].

### Rules
- the boundary should be easy to identify,
- players should use it as a reset point for route planning,
- it should feel separate from deep-house uncertainty,
- and getting detached from it should feel frightening.

The farther a player moves from reliable return paths, the stronger navigation dread should become.

## Mutation readability
When the house changes, the player does not need a literal notification that “graph edge 12 changed.” But the world should provide some legible consequence that something is off [cite:31].

### Readable mutation signals may include
- lost or displaced marks,
- changed adjacency,
- impossible loops,
- repeated rooms with altered details,
- altered acoustics,
- inconsistent return travel time,
- or teammate testimony that no longer matches current space.

### Rule
A mutation should usually be discoverable through discrepancy. Players should notice a mismatch between memory and present reality.

## Avoid these failure modes
Navigation UX should avoid becoming:
- visually samey to the point of meaninglessness,
- dependent on perfect memory with no externalization tools,
- so random that marks feel useless,
- so guided that players stop making route decisions,
- or so punishing that co-op communication cannot repair mistakes.

## Developer-facing graph debug
Developer tools should expose the full orientation truth because testing this game without graph inspection would be miserable [file:12][file:13][file:15].

### Debug should show
- current room/node for each player [file:13],
- neighboring rooms or active edges when needed [file:13],
- current active graph topology,
- mutation history or latest change reason,
- placed breadcrumb objects,
- safe-boundary relation,
- and any route-lock or observation-lock state once implemented.

### Separation rule
These tools exist for development and tuning. They must never become standard player orientation aids.

## Ownership guidance
To keep implementation modular, follow narrow ownership boundaries [file:13][file:14]:
- `RoomDefinition` owns static room identity metadata.
- `RoomVolume` owns physical room bounds and local detection hooks [file:13].
- `RoomRegistry` owns room queries and neighbor lookup [file:13].
- House graph systems own adjacency truth and mutation history.
- Breadcrumb item systems own placement and persistence of player marks.
- `HUDController` owns only minimal player-facing guidance, not global route logic.
- `DebugOverlay` owns developer-facing graph and room truth [file:13].

## Playtest questions
Use these questions during navigation-focused sessions:
- Could players form a mental map before major mutations began?
- Could they verbally describe rooms and routes to each other [cite:32]?
- Did breadcrumb tools become habitual or were they ignored [cite:31]?
- Did players feel uncertainty about connection, not total ignorance of place?
- Could attentive players notice when a familiar path had become false?
- Did getting deeper from the safe boundary increase dread?
- Did players ever feel cheated because the level lacked readable identity?

## Acceptance criteria
This document is serving the project correctly when:
- players can build provisional route knowledge without a map [cite:31],
- room and threshold identity are strong enough for verbal navigation [file:13][cite:32],
- breadcrumb tools become central to survival behavior [cite:31],
- mutations undermine trust without reducing the house to random noise [cite:31],
- debug tools provide full topology visibility for development [file:12][file:13][file:15],
- and multiplayer navigation feels socially negotiated rather than UI-solved [cite:31][cite:32].

## Relationship to future docs
This document defines how players maintain and lose orientation. Future docs should connect it to anomaly design, breadcrumb item implementation, room-identity authoring rules, and the graph debug toolset [file:13][file:14].
