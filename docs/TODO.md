# TODO

Reference `docs/templates/TODO_TEMPLATES.md` for template on TODO structure to stub, record, and expand in this document.

**LAST_USED_ID:** TD0024

---

## TODO Items

## [TD0024] [FEATURE] Line-of-sight raycasting for observation visibility

**What:** Replace portal-visibility-only approximation with camera-ray LOS checks for observation lock. Currently, `LocalObservationInputSource` derives visibility entirely from `PortalVisibilityResult` data (portal anchor dot-product heuristic). True LOS would raycast from camera to node/portal targets and reject occluded results.
**Why:** Portal visibility fires for ALL adjacent nodes regardless of camera direction (C1 concern from Sprint 2). Without LOS, visibility lock adds zero information beyond adjacency. The observation system needs directional awareness to gate mutations meaningfully.
**How:** Add a raycast pass in `LocalObservationInputSource.GetVisibleNodeIds()` or a separate `IVisibilityRaycastProvider` that filters portal results against Physics.Raycast hits. Must handle: layer masking (only architectural geometry blocks LOS), multiple portal apertures per edge, performance budget (~5 raycasts/frame max for 5-node graph).

**Priority:** P[2]
**Effort:** ~3h (Size: M; Human: ~15m review, CC: ~3h)
**Regression risk:** Medium — changes observation truth model, may affect lock timing.
**Depends on:** Sprint 2 complete (observation lock system operational)
**Types:** [FEATURE]
**Tags:** [OBSERVATION, VISIBILITY, SPRINT3+]

**Added:** 2026-05-09 (Sprint 2 Phase 6 polish)
**Context Reference:**
- Sprint 2 concerns: docs/handoff-prompts/current/sprint-2-concerns.md (C1)
- PDD: docs/design/04-sprints/sprint-2-observation-lock-system.md

### Acceptance Criteria
- [ ] Visibility lock only fires for nodes the camera is actually pointed toward
- [ ] Occluded portals (wall between camera and portal aperture) do not trigger visibility lock
- [ ] Performance stays within frame budget (< 0.5ms for visibility pass)
- [ ] Existing observation lock tests still pass (raycast is additive filter, not replacement)
- [ ] Debug overlay shows which nodes passed/failed LOS check

---

## [TD0015] S1B: [FEATURE] Shared Contracts — ViewContext, activation types, resolver stub

**What:** Define the shared type contracts that all S1B systems depend on: `ViewContext` readonly struct, `[Flags] NodeActivationReason` enum, `NodeActivationResolver` class (stub returning empty dict), `PortalProbeData`/`PortalVisibilityResult` structs, `IPortalVisibilityEvaluator` interface, and `NodePresentationHandle` MonoBehaviour. Write EditMode tests for construction and flag combinations.
**Why:** All three parallel tracks (TD0017/TD0018/TD0019) and the Gate 0 integration (TD0016) depend on these shared types compiling and being testable. Defining contracts first prevents parallel tracks from making incompatible assumptions about data shapes.
**How:** TDD-first. Create files in `Scripts/World/Graph/Runtime/`. See PRD Phase 0 tasks P0-1 through P0-6 for exact signatures.

**Priority:** P[1]
**Effort:** ~1.5h (Size: S; Human: ~10m review, CC: ~1.5h)
**Regression risk:** Low — new files only, no existing code modified.
**Depends on:** Nothing — can start immediately
**Types:** [FEATURE]
**Tags:** [GRAPH, PORTAL, VISIBILITY, S1B]

**Added:** 2026-05-05 (S1B sprint planning)
**Context Reference:**
- Parent: S1B Sprint
- Source docs:
  - ~/.gstack/projects/spatial-horror/admin-main-design-20260505-sprint1b-prd.md (Phase 0)
  - docs/design/04-sprints/sprint-1b-portal-visibility-node-activation.md

### Acceptance Criteria
- [ ] `ViewContext` readonly struct compiles: `{ string PlayerId; Vector3 CameraPosition; Vector3 CameraForward; string OccupiedNodeId; }`
- [ ] `[Flags] NodeActivationReason` enum: None=0, Occupied=1, Adjacent=2, PortalVisible=4, DebugForced=8
- [ ] `NodeActivationResolver.Resolve()` stub compiles and returns empty dictionary
- [ ] `PortalProbeData` struct: AnchorId, DestinationNodeId, PortalPosition, PortalForward, ApertureSize
- [ ] `PortalVisibilityResult` struct: AnchorId, DestinationNodeId, IsVisible
- [ ] `IPortalVisibilityEvaluator` interface with `Evaluate(ViewContext, IReadOnlyList<PortalProbeData>)` signature
- [ ] `NodePresentationHandle` MonoBehaviour: NodeId property + SetPresentation(bool) with null-guard
- [ ] EditMode tests pass for ViewContext construction, enum flag operations, resolver stub behavior
- [ ] No usage of Camera.main, no global state, no NGO types

---

## [TD0016] S1B: [FEATURE] Gate 0 — Single portal end-to-end integration slice

**What:** Build the thinnest possible end-to-end path proving S1B contracts survive Unity runtime: one occupied node (entry), one portal candidate, one destination activation (hall_a), one debug reason displayed. Stubs everywhere — the goal is proving the wiring works, not the logic.
**Why:** Gate 0 is the risk-burner. If contracts fail under Unity's trigger timing, camera lifecycle, or GameObject activation rules, we discover it here before 3 parallel tracks fan out and build on bad assumptions.
**How:** Stub `NodeStreamingController` (hardcoded occupied node), stub `PortalVisibilityController` (always returns visible), wire to House_Prototype (entry + hall_a only), stub IMGUI debug overlay showing reasons.

**Priority:** P[1]
**Effort:** ~2h (Size: S; Human: ~30m Play mode validation, CC: ~1.5h)
**Regression risk:** Low — stubs only, no production logic. Scene wiring is additive.
**Depends on:** TD0015
**Types:** [FEATURE]
**Tags:** [GRAPH, PORTAL, VISIBILITY, S1B, GATE]

**Added:** 2026-05-05 (S1B sprint planning)
**Context Reference:**
- Parent: S1B Sprint
- Source docs:
  - ~/.gstack/projects/spatial-horror/admin-main-design-20260505-sprint1b-prd.md (Phase 1)

### Acceptance Criteria (Exit criteria — must be painfully explicit)
- [ ] One occupied node source: entry node reported as Occupied in active set
- [ ] One portal candidate: portal between entry→hall_a evaluated (stub: always visible)
- [ ] One destination activation path: hall_a presentation root toggles to active via SetPresentation(true)
- [ ] One debug reason visible: IMGUI overlay shows "entry: Occupied" and "hall_a: PortalVisible"
- [ ] No globals: no Camera.main usage, no singleton "current player" assumption, ViewContext passed through
- [ ] Presentation toggle works without frame hitches or console errors
- [ ] **Gate 0 checkpoint:** If contracts need revision based on Unity runtime behavior, revise TD0015 outputs before proceeding to tracks

---

## [TD0017] S1B Track A: [FEATURE] Node activation resolver + streaming controller

**What:** Implement `NodeActivationResolver.Resolve()` (occupied + 1-hop adjacent + portal results merge) and `NodeStreamingController` MonoBehaviour (thin wrapper calling resolver each frame, toggling NodePresentationHandles). Includes debug override mode and assignment of handles to all 5 House_Prototype nodes.
**Why:** This is the core activation logic — decides which rooms are on/off based on graph topology and player position. Without it, all rooms stay permanently active (current S1A state).
**How:** TDD-first. Pure C# resolver logic (testable in EditMode), thin MonoBehaviour scene adapter. Reuse cleared dict to minimize GC. See PRD Track A tasks TA-1 through TA-6.

**Owns:** Active-set computation and presentation toggling.
**Does not own:** Portal-facing evaluation (that's TD0018).

**Priority:** P[1]
**Effort:** ~3.5h (Size: M; Human: ~15m review, CC: ~3h)
**Regression risk:** Medium — modifies scene (adds components to 5 room prefabs). Does not modify existing C# files.
**Depends on:** TD0016 (Gate 0 proves contracts work)
**Types:** [FEATURE]
**Tags:** [GRAPH, PORTAL, VISIBILITY, S1B, TRACK_A]

**Added:** 2026-05-05 (S1B sprint planning)
**Context Reference:**
- Parent: S1B Sprint
- Source docs:
  - ~/.gstack/projects/spatial-horror/admin-main-design-20260505-sprint1b-prd.md (Track A)

### Acceptance Criteria
- [ ] `NodeActivationResolver.Resolve()` returns occupied node with Occupied flag
- [ ] Resolver returns 1-hop adjacent nodes with Adjacent flag (via `GetConnectedEdges`)
- [ ] Resolver merges portal visibility results with PortalVisible flag
- [ ] EditMode tests cover: occupied set, adjacent set, combined reasons, empty graph, invalid node ID
- [ ] `NodeStreamingController` calls resolver each frame, toggles NodePresentationHandles accordingly
- [ ] `[SerializeField] bool forceAllActive` debug override bypasses resolver (all nodes stay active)
- [ ] All 5 Room_* prefabs in House_Prototype have NodePresentationHandle assigned
- [ ] Controller lifecycle tests: init, reset, handle missing/destroyed nodes gracefully (logs warning)
- [ ] Walking between nodes in Play mode updates active set correctly

---

## [TD0018] S1B Track B: [FEATURE] Portal visibility evaluator + viewer-context probe

**What:** Implement `PortalVisibilityEvaluator` (pure C# dot-product check with portal-crossing guard) and `PortalViewProbe` MonoBehaviour (reads portal forward/bounds from PortalAnchorAuthoring) and `PortalVisibilityController` (iterates probes, calls evaluator, exposes results for NodeStreamingController).
**Why:** This makes looking through a doorway actually activate the destination room — the core "portal visibility" behavior that distinguishes S1B from simple adjacency.
**How:** TDD-first. Pure C# evaluator (testable in EditMode), thin MonoBehaviour probe/controller. Portal forward = `PortalAnchorAuthoring.transform.forward`. Aperture = `BoxCollider.size`. Default dot-product threshold: 0.5 (60-degree cone), exposed as SerializeField.

**Owns:** Per-viewer portal-visible decisions.
**Does not own:** Activation policy (that's TD0017).

**Priority:** P[1]
**Effort:** ~3.5h (Size: M; Human: ~15m review, CC: ~3h)
**Regression risk:** Low — new files only. Does not modify PortalAnchorAuthoring (reads existing properties).
**Depends on:** TD0016 (Gate 0 proves contracts work)
**Types:** [FEATURE]
**Tags:** [GRAPH, PORTAL, VISIBILITY, S1B, TRACK_B]

**Added:** 2026-05-05 (S1B sprint planning)
**Context Reference:**
- Parent: S1B Sprint
- Source docs:
  - ~/.gstack/projects/spatial-horror/admin-main-design-20260505-sprint1b-prd.md (Track B)

### Acceptance Criteria
- [ ] `PortalVisibilityEvaluator`: facing portal (dot > threshold) returns visible
- [ ] Evaluator: facing away returns not visible
- [ ] Evaluator: **portal-crossing guard** — player past portal plane (dot of playerPos-portalPos vs portalForward < 0) always returns true
- [ ] Evaluator: degenerate inputs (zero forward, default ViewContext) handled without exception
- [ ] EditMode tests cover: facing, away, edge angles, past-plane, degenerate inputs
- [ ] `PortalViewProbe` reads transform.forward, BoxCollider.size, transform.position from PortalAnchorAuthoring GO
- [ ] `PortalVisibilityController` iterates probes for occupied node's portals, exposes `IReadOnlyList<PortalVisibilityResult>`
- [ ] Wired to House_Prototype scene portal anchors
- [ ] Deterministic: same camera position/direction always produces same result
- [ ] Dot-product threshold exposed as `[SerializeField] float portalAngleTolerance = 0.5f`

---

## [TD0019] S1B Track C: [FEATURE] Debug visibility overlay + gizmos against public queries

**What:** Create `SpatialVisibilityDebugOverlay` (IMGUI, F4 toggle) showing active nodes with reasons and portal-visible destinations. Extend existing `SpatialDebugGizmos.cs` with visibility wireframes (active=green, portal-visible=yellow, inactive=gray, portal sightline rays).
**Why:** Debug-first rule: if you can't see it, you can't tune it. The overlay must explain why any node is active using only the debug tools — no source code reading required.
**How:** Overlay consumes public query APIs from NodeStreamingController/PortalVisibilityController. Does not reach into controller internals. F4 key (F3 remains S1A graph topology). Tests verify overlay handles null/missing controller state gracefully.

**Owns:** Explanation/display of activation state.
**Does not own:** Anything — reads only, never writes to controllers.

**Priority:** P[1]
**Effort:** ~2h (Size: S; Human: ~10m review, CC: ~1.5h)
**Regression risk:** Low — new file + extension of existing gizmos. No logic changes.
**Depends on:** TD0016 (Gate 0 proves contracts work)
**Types:** [FEATURE]
**Tags:** [DEBUG, OVERLAY, VISIBILITY, S1B, TRACK_C]

**Added:** 2026-05-05 (S1B sprint planning)
**Context Reference:**
- Parent: S1B Sprint
- Source docs:
  - ~/.gstack/projects/spatial-horror/admin-main-design-20260505-sprint1b-prd.md (Track C)

### Acceptance Criteria
- [ ] `SpatialVisibilityDebugOverlay` toggled with F4 key (F3 unchanged)
- [ ] Overlay lists every active node with at least one reason (Occupied, Adjacent, PortalVisible, DebugForced)
- [ ] Overlay shows active portal destinations with node IDs
- [ ] Overlay shows current ViewContext (player ID, occupied node)
- [ ] `SpatialDebugGizmos.cs` extended: active nodes green wireframe, portal-visible yellow, inactive gray
- [ ] Portal sightline rays drawn from camera to portal position
- [ ] Overlay does not throw when no controller exists (null-safe)
- [ ] Handles empty state gracefully (no nodes, no portals)
- [ ] Tests: overlay safe with null controllers, empty results, missing references

---

## [TD0020] S1B: [FEATURE] Integration wiring + Gate 1/Gate 2 smoke test

**What:** Wire Track A + Track B together (PortalVisibilityController feeds results into NodeStreamingController). Wire Track C debug overlay to read from live controllers. Run Gate 1 (transit harness: entry→hall_a→living) and Gate 2 (full 5-node smoke test per sprint PDD checklist). Fix integration issues.
**Why:** Individual tracks work in isolation but haven't proven they compose correctly. Gate 1 validates transitions don't thrash. Gate 2 validates the full sprint acceptance criteria on the authored test slice.
**How:** Wire controllers in House_Prototype scene. Run PDD smoke test (10 items). Includes reset/stability testing (repeated enter/look/turn/restart cycles). Budget 60m buffer for integration fixes.

**Priority:** P[1]
**Effort:** ~3.5h (Size: M; Human: ~1h Play mode testing, CC: ~2.5h)
**Regression risk:** Medium — final integration may reveal issues requiring fixes in TD0017/TD0018.
**Depends on:** TD0017, TD0018, TD0019
**Types:** [FEATURE]
**Tags:** [GRAPH, PORTAL, VISIBILITY, S1B, INTEGRATION, GATE]

**Added:** 2026-05-05 (S1B sprint planning)
**Context Reference:**
- Parent: S1B Sprint
- Source docs:
  - ~/.gstack/projects/spatial-horror/admin-main-design-20260505-sprint1b-prd.md (Phase 3)
  - docs/design/04-sprints/sprint-1b-portal-visibility-node-activation.md (smoke test checklist)

### Acceptance Criteria
- [ ] PortalVisibilityController results flow into NodeStreamingController's Resolve() call
- [ ] Debug overlay reads from live controllers (not stubs)
- [ ] **Gate 1 — Transit harness:** Walk entry→hall_a→living without activation thrash during doorway transitions
- [ ] **Gate 2 — Full smoke test (all 10 PDD items):**
  - [ ] Launch from Bootstrap
  - [ ] Enter House_Prototype
  - [ ] Occupied node active
  - [ ] Adjacent nodes active
  - [ ] Look through doorway → destination marked PortalVisible
  - [ ] Turn away → portal-visible status updates
  - [ ] Move to new node → active-node set updates
  - [ ] Toggle debug override → all nodes active
  - [ ] Restart round
  - [ ] Activation state resets cleanly, no blocker errors
- [ ] Reset/stability: repeated enter/look/turn/restart cycles produce no stale state
- [ ] No critical console errors
- [ ] All EditMode tests still pass (73 existing + ~30 new from S1B)

---

## [TD0013] S2+: [KNOWN_BUG] PlayerNodeTracker trigger overlap race — non-deterministic event ordering

**What:** `PlayerNodeTracker.OnTriggerEnter/Exit` relies on Unity firing trigger events in a specific order during doorway transitions (enter new room before exiting old room). Unity does not guarantee this ordering. If `Exit(A)` fires before `Enter(B)`, `CurrentNodeId` is momentarily null. If two rooms' triggers overlap and events arrive in the wrong order, the player can be tracked as being in the wrong room.
**Why:** Any downstream system polling `CurrentNodeId` (observation, entity AI, mutation triggers) could make wrong decisions based on stale or null node state. Currently benign because S1A only uses the tracker for debug overlay display.
**How:** Add a debounce/hysteresis mechanism: buffer exit events for one frame, or use a priority system that prefers the most recent enter over any exit. Alternatively, track all overlapping rooms and use volume containment to resolve ambiguity. Needs tests for the overlap scenario.

**Priority:** P[2]
**Effort:** ~1h (Size: S; Human: ~10m review, CC: ~50m implementation + tests)
**Regression risk:** Medium — changes to trigger handling affect all room transitions.
**Depends on:** Nothing
**Types:** [KNOWN_BUG]
**Tags:** [GRAPH, PLAYER_TRACKING, S2]

**Added:** 2026-05-04 (pre-landing review, adversarial finding)

---

## [TD0014] ~~S2+~~ S1B-preflight: [KNOWN_BUG] PortalAnchorDefinition.localRotation defaults to invalid Quaternion(0,0,0,0) — **RESOLVED 2026-05-05**

**What:** C# struct default for `Quaternion` is `(0,0,0,0)`, not `Quaternion.identity`. Any code that reads `localRotation` from a default-initialized `PortalAnchorDefinition` will get an invalid zero quaternion. Applying this as a rotation produces NaN transforms. Currently nothing reads `localRotation` at runtime, but the portal system (S2) will need it.
**Why:** Latent NaN bomb. When portal traversal reads anchor rotation for player teleport positioning, uninitialized rotations will silently corrupt transform data.
**How:** Either initialize `localRotation = Quaternion.identity` in a constructor/factory method, or add validation in `SpatialGraphRuntime.Initialize` to replace zero quaternions with identity.

**Priority:** P[2]
**Effort:** ~15m (Size: XS; Human: ~5m, CC: ~10m)
**Regression risk:** Low — no current consumers of localRotation.
**Depends on:** Nothing
**Types:** [KNOWN_BUG]
**Tags:** [GRAPH, PORTAL, S2]

**Added:** 2026-05-04 (pre-landing review, adversarial finding)
**Resolved:** 2026-05-05 — `SpatialGraphRuntime.Initialize()` sanitizes zero quaternions to identity; `HouseGraphDefinition.Validate()` warns on authoring-time detection. Commit `13e6c23`.

---

## [TD0012] ~~S0.3~~ S1B-preflight: [BUG] Fix House_Graybox geometry test failures — grammar rules drift — **RESOLVED 2026-05-05**

**What:** 2 EditMode tests in `HouseGrayboxGeometryTests` fail because `House_Graybox.unity` scene geometry does not comply with the geometry grammar rules codified in S0.3. Specifically: (1) `CeilingsFlushWithWallTops` — GF_Ceiling top (2.75m) exceeds exterior wall tops (2.70m), expected <= 2.71m. (2) `FloorCeilingBoundsWithinExteriorWalls` — GF_Floor bounds.min.x (0.075m) extends past wall inner edge (0.15m), expected >= 0.14m.
**Why:** The geometry grammar tests were merged (S0.3 fix branch) but the scene geometry was not updated to match the stricter tolerances. These 2 failures run on every test suite execution and mask real regressions. Every new test run shows "2 failures" and developers have to mentally filter them out.
**How:** Open `House_Graybox.unity`, adjust GF_Ceiling height and GF_Floor bounds to comply with `GEOMETRY_GRAMMAR.md` rules. Alternatively, if House_Graybox is being superseded by House_Prototype, consider whether these tests should target the new scene or be retired.

**Priority:** P[1]
**Effort:** ~30m (Size: XS; Human: ~10m, CC: ~20m)
**Regression risk:** Low — scene geometry adjustment only. Tests already define the expected state.
**Depends on:** Nothing
**Types:** [BUG]
**Tags:** [GEOMETRY, TESTING, S0.3]

**Added:** 2026-05-04 (geometry grammar rules landed hours before scene compliance could be addressed)
**Resolved:** 2026-05-05 — Root cause: tests contradicted GEOMETRY_GRAMMAR.md (R1.2 cap rule, R1.3 midpoint rule). Fixed tests to match grammar, restored scene to correct S0.3 state. All 5 HouseGrayboxGeometryTests pass. Commit `6dca886`.

---

## [TD0003] M0: [TECH_DEBT] UNITY_MCP_LESSONS.md: broaden to general dev insights doc

**What:** Rename `UNITY_MCP_LESSONS.md` to `INSIGHTS.md` and restructure it as a general dev insights doc. Sections: Unity MCP gotchas, Windows/bash shell quirks (PATH refresh after installs, cmd.exe vs bash), Git workflow patterns, Claude Code session patterns (token-burning antipatterns).
**Why:** The current file is narrowly scoped to Unity MCP. Workflow discoveries from S0.3 (shell environment quirks, Claude Code session patterns) have no home and are being lost between sessions.
**How:** Rename the file, update all references in CLAUDE.md and docs index, restructure with new section headers. Migrate any pending observations from current session notes.

**Priority:** P[~4]
**Effort:** ~1h (Size: XS; Human: ~30m, CC: ~15m)
**Regression risk:** Low — doc-only rename. Update references in CLAUDE.md and `docs/design/00-index/repo-docs-index-claude-file-map.md`.
**Depends on:** Nothing
**Types:** [TECH_DEBT]
**Tags:** [DOCS, DEVEX, WORKFLOW]

**Added:** 2026-05-04 (S0.3 workflow hardening)
**Context Reference:**
- Parent: None
- Source docs:
  - docs/UNITY_MCP_LESSONS.md

---

## [TD0011] S1A: [NAMING] Consider SpatialGraphRuntime → HouseGraphRuntime rename

**What:** The class `SpatialGraphRuntime` uses "Spatial" prefix while all other types in `Desync.World.Graph` use "House" prefix (`HouseGraphDefinition`, `HouseNodeDefinition`, `HouseEdgeDefinition`). Consider renaming to `HouseGraphRuntime` for consistency.
**Why:** Counter-drift session flagged the naming inconsistency. The "Spatial" prefix comes from the pre-migration framework spec (`Desync.Spatial.*`), while the namespace moved to `Desync.World.Graph.*`. Either name is defensible — "Spatial" describes what it does (spatial queries), "House" matches the type family. Taste call, not a bug.
**How:** `git mv` + find/replace across 5 referencing files (SpatialGraphRuntime.cs, PortalResolver.cs, GraphRuntimeHost.cs, SpatialGraphRuntimeTests.cs, PortalResolverTests.cs). Low risk — plain C# class, no scene/prefab GUID references.

**Priority:** P[~5]
**Effort:** ~15m (Size: XS; Human: ~5m, CC: ~10m)
**Regression risk:** Low — rename + reference update only.
**Depends on:** S1A merge to main
**Types:** [NAMING]
**Tags:** [GRAPH, CONSISTENCY]

**Added:** 2026-05-04 (counter-drift session flagged, deferred by Mike)

**`HouseGraphRuntime` referenced in following design docs:**
  | docs/ARCH.md  | "pending rename" note    │ The canonical table we added — accurately reflects the open decision    |
  │ 02-architecture/networked-house-runtime-interfaces-contracts.md │ IHouseGraphRuntime (interface) │ Pre-existing — interface name is separate from the concrete class  |
  │ 05-debug-and-testing/impossible-house-graybox-vertical-slice-plan.md │ HouseGraphRuntime or equivalent        │ Pre-existing hedged reference  │
  │ 06-claude-prompts/claude-code-task-pack-networked-house-runtime.md   │ HouseGraphRuntime : IHouseGraphRuntime │ Pre-existing prompt pack  |

---

## [TD0021] ~~S2~~ S1C: [TECH_DEBT] Wire BindLocalPlayer in player spawn path — **RESOLVED**

**Resolved:** 2026-05-07 (S1C Phase 1). `PlayerMotor.OnNetworkSpawn()` now calls `BindLocalStreamingContext()` for the local owner. `OnNetworkDespawn()` clears with `BindLocalPlayer(null, null)`. See ARCH.md S1C entry for concern-mixing rationale.

---

## [TD0022] ~~S2~~ S1C: [BUG] Gizmo activation colors not displaying — **RESOLVED**

**Resolved:** 2026-05-07 (auto-resolved by TD0021 fix in S1C Phase 1). Verify gizmo colors return during Phase 4 validation gate.

---

## [TD0023] Pre-Demo: [FEATURE] In-game pause/quit path for builds

**What:** Standalone builds have no way to pause or quit the game. `Application.Quit()` is not wired to any input. Players must Alt+F4.
**Why:** Needed before any external playtest or friend demo. Not sprint-scoped but blocks showing the game to anyone.
**How:** Minimal ESC menu: pause (Time.timeScale=0), resume, quit (Application.Quit). No multiplayer disconnect handling needed for first pass.

**Priority:** P[2]
**Effort:** ~1h (Size: XS; Human: ~5m review, CC: ~45m)
**Regression risk:** Low — additive UI, no system changes.
**Depends on:** Nothing
**Types:** [FEATURE]
**Tags:** [UI, DEMO, PRE-DEMO]

**Added:** 2026-05-07 (captured during S1C eng review)

---

## Tooling / Debug Concepts

## [TD0004] M1: [FEATURE] Procedural Room Geometry Builder

**What:** Declarative room definition (dimensions, wall openings, door positions) that generates grammar-compliant geometry automatically, following the rules in `GEOMETRY_GRAMMAR.md`. Three use cases: (a) runtime tool for house-graph room materialization, (b) offline editor tool for rapidly generating batches of test rooms, (c) foundation for a level editor.
**Why:** The current hand-authored graybox is a manual coplanar-fix exercise each time geometry changes. As the house graph introduces new room nodes (S1A), authors will need to produce grammar-compliant rooms without manually applying 5 rule groups. A builder eliminates this entirely.
**How:** Unity Editor tool (or runtime MonoBehaviour) that accepts room parameters (dimensions, wall openings) and emits ProBuilder geometry that satisfies all rules in `GEOMETRY_GRAMMAR.md`. Integrate with house-graph room node materialization pipeline.

**Priority:** P[~2]
**Effort:** ~1–2 days (Size: M; Human: ~8–12h, CC: ~2–4h)
**Regression risk:** Medium — any bug in the builder produces z-fighting in all generated rooms. Must be validated against `GEOMETRY_GRAMMAR.md` invariants (see TD0005–TD0008 for test harness).
**Depends on:** S1A room node materialization design
**Types:** [FEATURE]
**Tags:** [TOOLING, GEOMETRY, HOUSE_GRAPH, S1A]

**Added:** 2026-05-04 (S0.3 geometry fix)
**Context Reference:**
- Parent: None
- Source docs:
  - docs/handoff-prompts/current/GEOMETRY_GRAMMAR.md
  - docs/handoff-prompts/current/04-geometry-validator-tdd-handoff.md

### Informal Tooling Ideas (not yet scoped)

- In-game level editor for manual graph manipulation (add nodes, swap edges, spawn entity)
- Mini-map / graph visualization (`M` key overlay)
- Graph layout rendering (solved problem — find library for visual graph from data structure)
- Blueprint-style room sketches with edge connections drawn between them
- Room codes displayed on floors in neon/radioactive orange

- ~~**Update GameBootstrap.gameplaySceneName for House_Prototype**~~ — **Completed:** S1A (2026-05-04). Both code default and serialized scene value updated.

- ~~**Add room-volume trigger colliders for GetNodeForPosition**~~ — **Completed:** S1A (2026-05-04). All 5 room prefabs have BoxCollider (isTrigger=true) + PlayerNodeTracker uses OnTriggerEnter/Exit.

---

## Nascent — Geometry Test Maturity

Items surfaced by S0.3 adversarial review (2026-05-04). Not blocking now, but will silently break as the house graph introduces new room geometry. Review when S1A room nodes land.

## [TD0005] M1: [TECH_DEBT] HouseGrayboxGeometryTests: replace GameObject.Find with tag/type search

**What:** `HouseGrayboxGeometryTests` uses `GameObject.Find("name")` which returns the first match. When house graph spawns room nodes with child objects, duplicate names become likely and tests will silently assert on the wrong object.
**Why:** Silent mismatch — the test passes, but it's checking the wrong GameObject. This is a correctness hazard that grows with each new room node added.
**How:** Replace `GameObject.Find("name")` calls with `FindObjectsByType` + a unique naming contract, or tag-based lookup. Assert exactly one match per search to catch naming collisions early.

**Priority:** P[~3]
**Effort:** ~1h (Size: XS; Human: ~30m, CC: ~15m)
**Regression risk:** Low — test-only change. Risk is that the replacement lookup introduces its own assumptions.
**Depends on:** S1A room node materialization (review trigger)
**Types:** [TECH_DEBT, TESTING]
**Tags:** [TESTING, GEOMETRY, HOUSE_GRAPH]

**Added:** 2026-05-04 (S0.3 adversarial review)
**Context Reference:**
- Parent: None
- Source docs:
  - docs/handoff-prompts/current/04-geometry-validator-tdd-handoff.md

## [TD0006] M1: [KNOWN_BUG] ComputeExteriorWallInteriorBounds: square panel misclassification

**What:** `ComputeExteriorWallInteriorBounds` classifies walls by comparing `size.x < size.z`. Square panels (e.g., corner pieces from ProBuilder) will be misclassified, silently producing wrong interior bounds.
**Why:** Misclassified walls return wrong interior bounds, which causes geometry validator tests to produce incorrect results without any error — silent false positives.
**How:** Add an aspect-ratio guard (e.g., assert `|size.x - size.z| > threshold`) or an explicit failure path for ambiguous panels. Document the expected wall geometry constraint.

**Priority:** P[~3]
**Effort:** ~1h (Size: XS; Human: ~30m, CC: ~15m)
**Regression risk:** Low — validator utility only. No runtime impact.
**Depends on:** House graph introducing non-rectangular room geometry (review trigger)
**Types:** [KNOWN_BUG, TESTING]
**Tags:** [TESTING, GEOMETRY, HOUSE_GRAPH]

**Added:** 2026-05-04 (S0.3 adversarial review)
**Context Reference:**
- Parent: None
- Source docs:
  - docs/handoff-prompts/current/04-geometry-validator-tdd-handoff.md

## [TD0007] M1: [KNOWN_BUG] ComputeExteriorWallInteriorBounds: unbounded interior for one-sided wall configurations

**What:** Sentinel checks (`float.MinValue`/`float.MaxValue`) verify that *some* wall was found per axis, but don't catch configurations where walls exist on only one side of center (L-shaped rooms, open-plan layouts). Interior bounds would be silently unbounded on the missing side.
**Why:** Silent unbounded bounds — `float.MaxValue` bounds would pass all subsequent geometry checks, masking incorrectly constructed rooms.
**How:** After computing bounds, assert that all four sides (min/max X and Z) were set from real wall geometry (i.e., none remain at sentinel values). Fail explicitly with a diagnostic message.

**Priority:** P[~3]
**Effort:** ~1h (Size: XS; Human: ~30m, CC: ~15m)
**Regression risk:** Low — validator utility only. No runtime impact.
**Depends on:** Non-rectangular or open-plan room layouts (review trigger)
**Types:** [KNOWN_BUG, TESTING]
**Tags:** [TESTING, GEOMETRY, HOUSE_GRAPH]

**Added:** 2026-05-04 (S0.3 adversarial review)
**Context Reference:**
- Parent: None
- Source docs:
  - docs/handoff-prompts/current/04-geometry-validator-tdd-handoff.md

## [TD0008] M1: [TECH_DEBT] GeometryGrammarValidator: hardcoded minInset 0.05f

**What:** The relational invariant test uses a hardcoded minimum inset threshold of `0.05f`. Correct for current graybox geometry but has a shelf life — once we deviate from hardcoded geometry constants or introduce per-room-node validation, this becomes a silent correctness assumption.
**Why:** Hardcoded geometry constants drift. As the grammar evolves (or per-room dimensions vary), tests using a fixed `0.05f` will pass for wrong geometry.
**How:** Derive the overlap constant from `GEOMETRY_GRAMMAR.md`'s R5.3 value (currently 0.05m) via a shared constant or from the actual wall geometry. Or delete the invariant test and replace with per-room-node validation once that infrastructure exists.

**Priority:** P[~4]
**Effort:** ~30m (Size: XS; Human: ~15m, CC: ~10m)
**Regression risk:** Low — test-only change.
**Depends on:** Per-room-node geometry validation infrastructure (review trigger)
**Types:** [TECH_DEBT, TESTING]
**Tags:** [TESTING, GEOMETRY]

**Added:** 2026-05-04 (S0.3 adversarial review)
**Context Reference:**
- Parent: None
- Source docs:
  - docs/handoff-prompts/current/04-geometry-validator-tdd-handoff.md

## [TD0009] M0: [KNOWN_BUG] GF_Ceiling/SF_Floor coplanar — stagger inter-floor slab positions
**What:** In `House_Graybox.unity`, GF_Ceiling and SF_Floor_* are both at Y=2.70 with scale.y=0.1, giving identical extents [2.65, 2.75]. Bottom faces coplanar at Y=2.65, top faces coplanar at Y=2.75. S0.3 moved GF_Ceiling from Y=2.65→2.70 (to comply with R1.2 wall-top burial), inadvertently matching SF_Floor's position. Shipped with visual confirmation (no visible banding at time of test), but coplanar faces exist in data and may surface under baked lighting or different view angles.
**Why:** Coplanar faces cause z-fighting under some rendering conditions. Grammar R1.4 requires staggered inter-floor slabs but doesn't explicitly prohibit full coincidence. Future geometry validators will report this as a violation.
**How:** Move SF_Floor_A/B/C from Y=2.70 to Y=2.725 (top=2.775, not coplanar with GF_Ceiling top=2.75) and update stairwell/landing colliders accordingly. Or keep GF_Ceiling at Y=2.70 and move SF_Floor to Y=2.75. Verify both floors' collision and visual with lighting enabled. Add a dedicated R1.4 test that checks no two inter-floor separators share exact XZ overlap at the same Y center.

**Priority:** P[~2]
**Effort:** ~1–2h (Size: XS; Human: ~1h ProBuilder session, CC: ~15m coord update)
**Regression risk:** Medium — stairwell geometry connects GF_Ceiling and SF_Floor; moving one requires verifying the other doesn't produce a gap or new violation.
**Depends on:** Nothing — fix anytime before baked lighting or geometry validator TDD session (TD0003)
**Types:** [KNOWN_BUG]
**Tags:** [GEOMETRY, SCENE, HOUSE_GRAYBOX]

**Added:** 2026-05-04 (S0.3 ship adversarial review — user-accepted risk, flagged for downstream fix)
**Context Reference:**
- Parent: None
- Source docs:
  - docs/handoff-prompts/current/GEOMETRY_GRAMMAR.md (R1.2, R1.4)
  - unity-DESYNC/Assets/_Project/Scenes/House_Graybox.unity

## [TD0010] M0: [TECH_DEBT] House_Graybox.unity: 4 unnamed scene-embedded PhysicsMaterials
**What:** ProBuilder editing during S0.3 auto-injected 4 unnamed `PhysicsMaterial` objects (friction=0.6) directly into `House_Graybox.unity`'s scene YAML. They are embedded as inline scene objects with `m_Name: ""` rather than reusable `.physicsMaterial` assets. Only 4 of ~17 modified colliders got them; others retain Unity defaults (friction 0.4). Inconsistent and unintended.
**Why:** Unnamed embedded physics materials are tech debt — not reusable, not named, inconsistent friction across the scene. Will confuse future physics tuning.
**How:** Open scene in editor, find the 4 affected colliders (search for colliders with non-default friction), strip the inline materials, and if friction tuning is needed create a proper named `.physicsMaterial` asset in `Assets/_Project/Art/Materials/Physics/`.

**Priority:** P[~4]
**Effort:** ~30m (Size: XS; Human: ~20m, CC: ~10m)
**Regression risk:** Low — physics material removal returns those colliders to Unity defaults. Graybox physics aren't tuned yet.
**Depends on:** Nothing — fix on next scene-edit pass
**Types:** [TECH_DEBT]
**Tags:** [SCENE, PHYSICS, HOUSE_GRAYBOX]

**Added:** 2026-05-04 (S0.3 ship adversarial review — ProBuilder editing side effect)
**Context Reference:**
- Parent: None
- Source docs:
  - unity-DESYNC/Assets/_Project/Scenes/House_Graybox.unity

---

## Creative Backlog

*Do **not** use TODO_TEMPLATES.md for these* 

Ideas captured during development that expand scope beyond current sprint. Debate at next sprint boundary — do not implement mid-sprint without explicit justification.

### Post-MVP House Concepts
- Rose Manor / Rose Estate — massive haunted estate (Stephen King inspired) that evolves/grows
- Warehouses, prisons, schools as alternate house levels
- Apartment complex — British flats with NPCs still present as anomaly spreads between units

### Post-MVP Entity Concepts
- Special movement: melt through walls/floors/ceilings, blink-step, dissipation into motes
- Particle physics on dissipation (motes carry forward momentum)
- Richer hunt: anticipate player path, portal ahead to cut off escape
- Multiple entity types/variants with different behaviors
- Entity "Gaze" effect as observation mechanic (blurred vision, sluggish movement when watched by entity)
- Psychic scream on flee that triggers mutation node + house response

### Post-MVP Gameplay Concepts
- Anchor spawning threshold (10 anchors = failure / godling summoned) replacing timer-based lose
- Asymmetric play mode: one player as the Lovecraftian entity/architect
- "Hopeless last chance" on failure state (Lovecraftian tradition)
- Player death in collapsing node: infinite alien landscape sequence before suffocation

### Post-MVP Atmosphere Concepts
- Localized + migrating house sounds (groaning moves through house as mutation propagates)
- Echo system for sounds through corridors from source rooms
- House shudder animation during mutation events
- Generation-coded mutation visuals (blue → green → yellow filters showing mutation age)

