# TODO

Reference `docs/templates/TODO_TEMPLATES.md` for template on TODO structure to stub, record, and expand in this document.

**LAST_USED_ID:** TD0012

---

## TODO Items

## [TD0012] S0.3: [BUG] Fix House_Graybox geometry test failures — grammar rules drift

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

- **Update GameBootstrap.gameplaySceneName for House_Prototype** — Change default from `"House_Graybox"` to `"House_Prototype"` in `GameBootstrap.cs:9` and add `House_Prototype` to Build Settings. Current bootstrap targets the old graybox scene. Do this when House_Prototype scene is created in S1A Session 2. Depends on: House_Prototype scene existing. (Identified during S1A eng review, 2026-05-04)

- **Add room-volume trigger colliders for GetNodeForPosition** — Each room prefab needs a BoxCollider (isTrigger=true) covering the room's interior volume so GetNodeForPosition works everywhere, not just at doorway thresholds. Handle overlapping triggers at boundaries (last-entered-wins or priority). Needed for spawn, teleport, join-in-progress node resolution. Implement alongside room prefab creation in S1A Session 2. (Identified during S1A eng review, Codex outside voice, 2026-05-04)

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

