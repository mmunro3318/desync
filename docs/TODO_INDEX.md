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
| TD0011 | S1A | NAMING | Consider SpatialGraphRuntime → HouseGraphRuntime rename | S1A merge | 2026-05-04 |
| TD0013 | S2+ | KNOWN_BUG | PlayerNodeTracker trigger overlap race | — | 2026-05-04 |
| TD0015 | S1B | FEATURE | Shared Contracts — ViewContext, activation types, resolver stub | — | 2026-05-05 |
| TD0016 | S1B | FEATURE | Gate 0 — Single portal end-to-end integration slice | TD0015 | 2026-05-05 |
| TD0017 | S1B | FEATURE | Node activation resolver + streaming controller | TD0016 | 2026-05-05 |
| TD0018 | S1B | FEATURE | Portal visibility evaluator + viewer-context probe | TD0016 | 2026-05-05 |
| TD0019 | S1B | FEATURE | Debug visibility overlay + gizmos against public queries | TD0016 | 2026-05-05 |
| TD0020 | S1B | FEATURE | Integration wiring + Gate 1/Gate 2 smoke test | TD0017-19 | 2026-05-05 |

## Resolved (this session)

| ID | Type | Title | Resolved |
|---|---|---|---|
| TD0012 | BUG | Fix House_Graybox geometry test failures | 2026-05-05 |
| TD0014 | KNOWN_BUG | PortalAnchorDefinition.localRotation zero quaternion | 2026-05-05 |

## Counts

- **M0 open:** 3 (TD0003, TD0009, TD0010)
- **M1 open:** 5 (TD0004–TD0008)
- **S1A open:** 1 (TD0011)
- **S2+ open:** 1 (TD0013)
- **S1B open:** 6 (TD0015–TD0020)
- **Resolved:** 2 (TD0012, TD0014)
- **LAST_USED_ID:** TD0020 *(mirror of `docs/TODO.md` header)*

## Archived / closed

See `docs/TODO_ARCHIVE.md`. Currently empty — no completed items archived yet.
