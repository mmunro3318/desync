# TODO Index

Glance-only summary of active TODOs. Read before major planning or scoping decisions.

- Full details → `docs/TODO.md`
- Format rules / tier system → `docs/templates/TODO_TEMPLATES.md`
- Closed items → `docs/TODO_ARCHIVE.md`

---

## Active TODOs

| ID | Milestone | Type | Title | Depends on | Added |
|---|---|---|---|---|---|
| TD0003 | M0 | TECH_DEBT | UNITY_MCP_LESSONS.md: broaden to general dev insights doc | — | 2026-05-04 |
| TD0004 | M1 | FEATURE | Procedural Room Geometry Builder | S1A design | 2026-05-04 |
| TD0005 | M1 | TECH_DEBT | HouseGrayboxGeometryTests: replace GameObject.Find | S1A (trigger) | 2026-05-04 |
| TD0006 | M1 | KNOWN_BUG | ComputeExteriorWallInteriorBounds: square panel misclassification | non-rect rooms (trigger) | 2026-05-04 |
| TD0007 | M1 | KNOWN_BUG | ComputeExteriorWallInteriorBounds: unbounded interior for one-sided walls | open-plan rooms (trigger) | 2026-05-04 |
| TD0008 | M1 | TECH_DEBT | GeometryGrammarValidator: hardcoded minInset 0.05f | per-room validation infra (trigger) | 2026-05-04 |
| TD0009 | M0 | KNOWN_BUG | House_Graybox: GF_Ceiling/SF_Floor coplanar — stagger inter-floor slab Y positions | — | 2026-05-04 |
| TD0010 | M0 | TECH_DEBT | House_Graybox: 4 unnamed scene-embedded PhysicsMaterials | — | 2026-05-04 |

## Counts

- **M0 open:** 3 (TD0003, TD0009, TD0010)
- **M1 open:** 5 (TD0004–TD0008)
- **LAST_USED_ID:** TD0010 *(mirror of `docs/TODO.md` header)*

## Archived / closed

See `docs/TODO_ARCHIVE.md`. Currently empty — no completed items archived yet.

