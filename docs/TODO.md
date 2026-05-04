# TODO

Reference `docs/templates/TODO_TEMPLATES.md` for template on TODO structure to stub, record, and expand in this document.

**LAST_USED_ID:** TD0008

---

## TODO Items

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

