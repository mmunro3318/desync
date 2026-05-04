# Handoff: Geometry Grammar Fix — Document, Ship, Review

> **Session flow:** `/document-release` -> `/ship` -> `/review`
> **Branch:** `fix/quick-fix-geometry-bug-repeat`
> **Base:** `main`

## What Was Done

Systematic fix of coplanar geometry artifacts (z-fighting, light leak banding) across the entire `House_Graybox.unity` scene. Root cause: modular graybox pieces shared exact face positions due to IEEE 754 float precision, causing renderer z-fighting visible as banding artifacts on the building exterior.

### Changes

**Scene (`House_Graybox.unity`):**
- 6 horizontal separators: XZ edges extended to exterior wall midpoint (0.075m from outer face), tops raised 0.05m above wall tops
- 17 internal walls: trimmed inward so exterior-facing ends sit 0.05m inside the exterior wall inner face (were extending to outer face, causing z-fighting)
- 4 railings: trimmed at exterior walls (same as internal walls), bases lowered 0.05m into floor slabs
- Hall_W3: fixed envelope violation (extended 1m past building back wall)

**Docs:**
- `GEOMETRY_GRAMMAR.md` — codified construction rules for coplanar-safe modular geometry (5 rule groups: horizontal separators, exterior walls, internal walls, railings, general constraints)
    - **Note:** this grammar is a **brittle** solution that will likely break as we scale level assets, it should be marked as a TODO for review, and noted as a potential failure point during dev.
- `04-geometry-validator-tdd-handoff.md` — seed prompt for a `/tdd` session to build EditMode validator tests + `GeometryGrammarValidator` utility class
- `TODO.md` — added procedural room geometry builder to tooling backlog

### Key Architecture Decisions

1. **Trim inward, not extend outward:** Internal walls that meet exterior walls are trimmed so their end is 0.05m inside the exterior wall inner face. First attempt (extend outward) pushed walls through the building exterior.
2. **Separators extend to wall midpoint:** Horizontal separators (floors/ceilings/roof) extend to 50% of exterior wall thickness (0.075m from outer face). Too far in = visible gap; too far out = z-fighting at outer face.
3. **Hierarchy of dominance:** Horizontal separators > exterior walls > internal walls > railings. The "host" piece is never modified; the terminating piece adjusts.

## For `/document-release`

Update these docs to reflect the geometry grammar:
- `docs/ARCH.md` — the "URP lighting: modular graybox floor/ceiling construction" section needs updating to reference `GEOMETRY_GRAMMAR.md` and reflect the corrected rules (trim inward, wall midpoint extent). The existing construction rules in ARCH.md were from the first ~~light-leak~~ fix and are now superseded by the grammar.
    - **Note:** we affectionately refer to the bug as the "light-leak" bug/fix out of habit/canon because that was the original hypothesis... it was in fact a **geometry/coplanar z-fighting** situation.
- `CLAUDE.md` — the "URP + lighting guardrails" section references the old ceiling-only fix. Update to reference the grammar and note the broader fix.
- Check if any other docs reference the old "inset to wall inner edges" rule and update them.

## For `/ship`

- Branch: `fix/quick-fix-geometry-bug-repeat`
- Base: `main`
- Tests: 7/7 EditMode tests pass (including `NetworkBootstrapConsistencyTests`)
- The branch has a revert commit in its history (first fix attempt was wrong). Squash merge recommended to keep history clean.

## For `/review`

Key things to verify:
- Scene diff: confirm all transform changes are trim-inward (no negative positions, no values past building envelope)
- Grammar doc: check rules are internally consistent (R1.3 wall midpoint, R3.1 trim to inner face minus 0.05m)
- TDD handoff: verify test descriptions match corrected grammar rules
- No code changes (scene-only + docs) — no new scripts, no API changes

## Deferred Work

- **Geometry validator tests:** TDD handoff written at `04-geometry-validator-tdd-handoff.md`. Run in a separate clean session with `/tdd`. Tests should initially FAIL against any scene that hasn't been fixed, PASS against the fixed scene.
- **Procedural room builder:** Added to `TODO.md` under Tooling/Debug Concepts. Review when S1A room node materialization is underway.
- **GF_Ceiling MeshRenderer:** Discovered disabled during fix session. Likely intentional (SF_Floor covers it from above). Verify and document.
