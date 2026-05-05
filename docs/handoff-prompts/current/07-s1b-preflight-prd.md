# PRD: S1B Pre-Flight — Bug Fixes and Deferred Doc Housekeeping

> **Sprint context:** Pre-flight work to clean the repo before starting Sprint 1B (Portal Visibility + Node Activation).
> **Priority:** P[1] — blocks clean S1B start
> **Branch:** `fix/s1b-preflight` from `main`
> **Session flow:** `/tdd` (TD0014 code) → MCP editor session (TD0012 scene fix) → doc housekeeping

---

## Problem Statement

The test suite has 2 pre-existing failures (TD0012) that add noise to every test run, masking real regressions. A latent zero-quaternion bug (TD0014) in `PortalAnchorDefinition` will produce NaN transforms when the portal system reads anchor rotations in S1B. Two deferred handoff docs (04, 05) sit in `current/` with no TODO pointers and no trigger for when to resurface them.

These issues compound: developers filtering out "expected failures" will miss real regressions, and the quaternion bug will silently corrupt portal traversal data the moment S1B code reads anchor rotations.

## Solution

Fix both bugs, get to 75/75 green tests, and create traceable TODO pointers for deferred work before it gets forgotten.

1. **TD0014:** Add defensive quaternion sanitization in `SpatialGraphRuntime.Initialize()` and a warning in `HouseGraphDefinition.Validate()`. Belt-and-suspenders: the runtime safety net catches all construction paths, the validation warning makes it visible at authoring time.

2. **TD0012:** Adjust 2 geometry transforms in `House_Graybox.unity` to comply with the geometry grammar rules. The tests define correct expectations; the scene data drifted during S0.3.

3. **Deferred docs:** Move handoff docs 04/05 to `deferred/`, create TODO items (TD0015, TD0016) pointing to them with milestone-based resurface triggers.

## User Stories

1. As a developer running the test suite, I want all 75 tests to pass so that I can immediately identify new regressions without mentally filtering "known failures."
2. As a developer implementing portal traversal (S1B), I want anchor rotations to always be valid quaternions so that player teleport positioning never produces NaN transforms.
3. As a developer authoring graph definitions in the Inspector, I want a validation warning if I accidentally leave a portal anchor rotation at the zero default so that I catch the mistake before it reaches runtime.
4. As a developer constructing `PortalAnchorDefinition` programmatically (tests, future runtime generation), I want `Initialize()` to silently fix zero quaternions to identity so that forgetting to set rotation doesn't produce a latent NaN bomb.
5. As a developer reading `SpatialGraphRuntime.Initialize()`, I want a clear comment explaining the zero-quaternion guard so that I understand why the sanitization exists and where to look if it fires unexpectedly.
6. As a sprint planner reviewing `TODO.md`, I want deferred geometry work (validator TDD session, fix ship history) to have explicit TODO items with milestone triggers so that it resurfaces at the right time rather than being silently forgotten.
7. As a developer debugging geometry artifacts in a future sprint, I want a reference pointer (TD0016) to the S0.3 fix history so that I can trace the rationale for trim-inward, wall-midpoint, and hierarchy-of-dominance decisions.
8. As a developer starting S1B, I want the `handoff-prompts/current/` directory to contain only active and relevant docs so that the pre-flight context is clean.

## Implementation Decisions

### TD0014: Zero-Quaternion Sanitization

- **Fix location:** Two-site fix. Sanitize in `SpatialGraphRuntime.Initialize()` (runtime safety net) AND warn in `HouseGraphDefinition.Validate()` (authoring-time visibility).
- **No logging in Initialize():** `SpatialGraphRuntime` is a pure C# class with no `UnityEngine` dependency. Adding `Debug.LogWarning` would introduce engine coupling to a currently engine-agnostic query engine. The `Validate()` warning in `HouseGraphDefinition` (which already uses `UnityEngine`) covers the authoring case. Decision recorded in `ARCH.md`.
- **Sanitization logic:** Compare all 4 quaternion components to zero. If all zero, replace with `Quaternion.identity`. Add a descriptive comment block explaining the C# struct default behavior, the guard's purpose, and where to look if it fires unexpectedly.
- **Validate() warning:** Add the zero-quaternion check to the existing anchor validation loop. Append a warning string to the errors list (consistent with existing validation pattern).
- **Existing asset data is safe:** The `HouseGraphDefinition.asset` already has valid quaternion values for all anchors. This fix only catches future programmatic construction paths.

### TD0012: Geometry Test Scene Fix

- **Approach:** Fix scene geometry to match test expectations (Option A from pre-flight analysis). Not retiring tests.
- **Rationale:** `House_Graybox.unity` is explicitly kept as a lighting reference (per CLAUDE.md). Tests are cheap to satisfy (2 transform adjustments). Clean test suite is worth the 30-minute editor session.
- **Specific fixes needed:**
  - `CeilingsFlushWithWallTops`: GF_Ceiling top (2.75m) exceeds exterior wall tops (2.70m), expected <= 2.71m. Adjust GF_Ceiling Y-position or scale.
  - `FloorCeilingBoundsWithinExteriorWalls`: GF_Floor bounds.min.x (0.075m) extends past wall inner edge (0.15m), expected >= 0.14m. Adjust GF_Floor X-position or scale.
- **Method:** Unity MCP editor tools to adjust transforms. No ProBuilder mesh edits needed — these are transform-level adjustments.

### Doc Housekeeping

- **Move:** `04-geometry-validator-tdd-handoff.md` and `05-geometry-fix-ship-handoff.md` from `current/` to `deferred/`.
- **TD0015 (new):** Points to deferred `04-*`. Type: `[TECH_DEBT]`. Priority: `P[~3]`. Resurface trigger: before M1 room generation or procedural geometry work. Depends on TD0004.
- **TD0016 (new):** Points to deferred `05-*`. Type: `[REFERENCE]`. Priority: `P[~5]`. Resurface trigger: when debugging geometry artifacts or reviewing construction decisions. No action required — breadcrumb only.
- **ARCH.md update:** Add a note on the `SpatialGraphRuntime` pure-C# decision with debugging symptoms (if zero-quaternion sanitization fires silently without Validate, transforms may appear at world origin with no console warning — check anchor construction paths).

### TD0013: Deferred (Not in This PRD)

- **Decision:** Defer to S2 pre-flight. The `PlayerNodeTracker` trigger race only affects debug overlay today. Risk profile changes sharply at S2 (observation lock makes irreversible state decisions based on `CurrentNodeId`). S1B can tolerate a one-frame null because portal activation is additive.
- **Promotion trigger:** If S1B testing reveals visible room-activation flickering during doorway transitions, promote TD0013 to the current sprint.

## Testing Decisions

### What makes a good test here

Tests verify external behavior through public interfaces. For TD0014, the test constructs a `PortalAnchorDefinition` with default (zero) quaternion, passes it through the public API (`Initialize()` then `GetPortalAnchor()`, or `Validate()`), and asserts the output. No internal state inspection.

### Tests to write

**In `SpatialGraphRuntimeTests.cs`:**
- `Initialize_SanitizesZeroQuaternion_ToIdentity` — Construct a graph definition with a `PortalAnchorDefinition` that does NOT set `localRotation` (C# default = zero quaternion). Call `Initialize()`, then `GetPortalAnchor()`. Assert `localRotation == Quaternion.identity`.

**In `HouseGraphDefinitionTests.cs`:**
- `Validate_FlagsZeroQuaternionRotation` — Construct a graph definition with a zero-quaternion anchor. Call `Validate()`. Assert the errors list contains a warning mentioning the anchor ID.

### Prior art

- `SpatialGraphRuntimeTests.cs` — 20+ existing tests following the same pattern: create `HouseGraphDefinition` SO, populate with test data, call `Initialize()`, assert via public query methods.
- `HouseGraphDefinitionTests.cs` — existing `Validate_*` tests that construct invalid definitions and assert specific error strings in the returned list.
- `HouseGrayboxGeometryTests.cs` — 5 existing scene-validation tests. TD0012 fix makes the 2 failing tests pass without modifying test code.

### Test count targets

- Before: 73 tests (71 pass, 2 fail)
- After: 75 tests (75 pass, 0 fail)

## Out of Scope

- **Full `GeometryGrammarValidator` implementation** (TD0004/TD0015) — M1 scope, deferred until procedural room generation work begins.
- **TD0013 `PlayerNodeTracker` trigger race fix** — Deferred to S2 pre-flight. See deferral rationale above.
- **TD0009 GF_Ceiling/SF_Floor inter-floor coplanar fix** — Latent z-fighting in `House_Graybox` only, not visible under current lighting. Deferred.
- **TD0011 `SpatialGraphRuntime` rename** — Taste call, deferred indefinitely.
- **Any S1B feature work** — This PRD is strictly pre-flight cleanup.
- **Archiving handoff docs** — Docs move to `deferred/`, not `archived/`. Archive is for complete/obsolete docs only.

## Further Notes

### Execution sequence

1. **TD0014 code changes** via `/tdd` — red-green-refactor for the 2 new tests + implementation
2. **TD0012 scene fix** via Unity MCP — adjust 2 transforms, verify 5/5 geometry tests pass
3. **Doc housekeeping** — TODO.md updates, ARCH.md note, `git mv` handoff docs
4. **Verify** — full test suite 75/75 green

### Key architectural decision recorded

`SpatialGraphRuntime` is intentionally kept as a pure C# class with no `UnityEngine` dependency. Runtime logging/warnings for graph-related issues should go through the boundary layer (`HouseGraphDefinition`, `GraphRuntimeHost`, or a future injected logger), never through direct `UnityEngine.Debug` calls in the query engine. This preserves testability and prevents coupling creep.

### Debugging symptom if zero-quaternion guard fires silently

If portal anchors appear at world origin with no console warning, check `PortalAnchorDefinition` construction paths — `Initialize()` may have sanitized a zero quaternion that `Validate()` never saw. The comment in `Initialize()` documents this.
