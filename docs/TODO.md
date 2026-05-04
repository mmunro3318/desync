# TODO

Reference `docs/templates/TODO_TEMPLATES.md` for template on TODO structure to stub, record, and expand in this document. 

---

## TODO Items

- **Expand UNITY_MCP_LESSONS.md to INSIGHTS.md** — Broaden from Unity MCP-specific gotchas to a general dev insights doc with sections for: Unity MCP, Windows/bash shell quirks (PATH refresh after installs, cmd.exe vs bash), Git workflow patterns, Claude Code session patterns (token-burning antipatterns). Rename and restructure. (Identified during S0.3 workflow hardening, 2026-05-04)

---

## Nascent — Geometry Test Maturity

Items surfaced by S0.3 adversarial review (2026-05-04). Not blocking now, but will silently break as the house graph introduces new room geometry. Review when S1A room nodes land.

- **GameObject.Find name collision risk** — `HouseGrayboxGeometryTests` uses `GameObject.Find("name")` which returns the first match. When house graph spawns room nodes with child objects, duplicate names become likely. Fix: search by tag, use `FindObjectsByType` with a unique naming contract, or assert exactly one match. *Review when: S1A room node materialization lands.*
- **Square wall panel breaks thin-axis heuristic** — `ComputeExteriorWallInteriorBounds` classifies walls by comparing `size.x < size.z`. Square panels (e.g., corner pieces from ProBuilder) will be misclassified, silently producing wrong interior bounds. Fix: add aspect-ratio guard or explicit failure on ambiguous panels. *Review when: house graph introduces non-rectangular room geometry.*
- **Single-sided wall produces unbounded interior** — The sentinel checks (`float.MinValue`/`float.MaxValue`) verify that *some* wall was found per axis, but don't catch configurations where walls exist on only one side of center (e.g., L-shaped rooms, open-plan layouts). Interior bounds would be silently unbounded on the missing side. *Review when: non-rectangular or open-plan room layouts are authored.*
- **minInset hardcoded at 0.05f** — The relational invariant test uses a hardcoded minimum inset threshold. Correct for current graybox geometry, but has a shelf life. Should be deleted or derived from actual wall geometry once we have more robust per-room-node validation. *Review when: we deviate from hardcoded geometry constants, or per-room-node tests replace the monolithic scene test.*

---

## Creative Backlog

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

### Tooling / Debug Concepts
- **Procedural Room Geometry Builder** — declarative room definition (dimensions, wall openings, door positions) that generates grammar-compliant geometry automatically. Would eliminate manual coplanar-fix work entirely. Useful as: (a) runtime tool for house-graph room materialization, (b) offline editor tool for rapidly generating batches of test rooms, (c) foundation for a level editor. See `docs/handoff-prompts/current/GEOMETRY_GRAMMAR.md` for the construction rules it would encode. *Review when: S1A room node materialization design is underway, or when manual room authoring becomes a bottleneck.* (Identified 2026-05-04)
- In-game level editor for manual graph manipulation (add nodes, swap edges, spawn entity)
- Mini-map / graph visualization (`M` key overlay)
- Graph layout rendering (solved problem — find library for visual graph from data structure)
- Blueprint-style room sketches with edge connections drawn between them
- Room codes displayed on floors in neon/radioactive orange