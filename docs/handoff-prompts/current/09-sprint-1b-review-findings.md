# Sprint 1B Pre-Landing Review Findings

**Date:** 2026-05-06
**Branch:** `feat/s1b-shared-contracts`
**Reviewers:** Claude structured, Claude adversarial, Codex adversarial (gpt-5.4), testing specialist, maintainability specialist
**Status:** Fixes required before /ship

---

## Summary

S1B is code-complete (22/25 plan items DONE, 1 PARTIAL, 1 intentionally deferred). 121+ tests pass. The review found 2 critical issues and 2 informational items that should be fixed before landing. Mike reviewed findings via Perplexity research and confirmed: fix all four.

---

## Critical Fix 1: Camera.main Violation

**File:** `Scripts/World/Graph/Runtime/NodeStreamingController.cs:59`

**Problem:** `_playerCamera = Camera.main;` violates PRD US-5 acceptance criteria ("No usage of Camera.main") and the architectural guardrail ("No Camera.main. Camera reference passed via context"). In multiplayer, `FindAnyObjectByType<PlayerNodeTracker>()` at line 57 can bind a remote player's tracker while `Camera.main` returns the local camera, mixing occupancy from one player with view direction from another. Silent cross-player room deactivation.

**Confirmed by:** Claude adversarial, Codex adversarial, maintainability specialist (3 independent sources).

**Fix direction:** Resolve camera through the player hierarchy. The `PlayerNodeTracker` lives on the player GO which also has (or can provide) the camera. Bind both tracker and camera from the same locally-owned player, not via global scene searches. The `_playerTracker` lookup at line 57 has the same problem (can find remote player's tracker). Both should resolve from the local player's hierarchy.

**Related cleanup:** The per-frame `FindAnyObjectByType` calls at lines 56-59 have no `_searched` guard (unlike `DiscoverReferences` which has `_discoveredAtRuntime`). Once the camera/tracker binding is fixed via injection, these per-frame searches should be eliminated or guarded. Tag: `TD-PERF-FIND`.

---

## Critical Fix 2: Shared Mutable Return Aliasing

**Files:**
- `Scripts/World/Graph/Runtime/NodeActivationResolver.cs:8` (`_result` dictionary)
- `Scripts/World/Graph/Runtime/PortalVisibilityEvaluator.cs:9` (`_results` list)

**Problem:** Both classes return `IReadOnlyDictionary` / `IReadOnlyList` backed by a single mutable instance field that is `.Clear()`ed at the start of each call. Any consumer holding a reference to a previous return sees its data silently overwritten.

`NodeStreamingController` stores the return in `_lastResult` (line 92). `SpatialDebugGizmos` reads `_streamingController.LastResult` directly in `OnDrawGizmos` (editor thread, different cadence from Update). If gizmos draw mid-resolve, they iterate a half-cleared collection.

The debug overlay (`SpatialVisibilityDebugOverlay.SetActivationState`) already does a defensive copy (correct pattern). The gizmos do not.

**Confirmed by:** Claude adversarial, testing specialist (2 independent sources).

**Fix direction:** Two options:
- **(A) Copy on read in gizmos** (matching the overlay pattern). Snapshot `LastResult` into a local dict at the top of `OnDrawGizmos`. Cheapest change, consistent with existing overlay approach.
- **(B) Return fresh collections from Resolve/Evaluate.** More defensive but allocates per-frame. Could pool/swap double-buffer if GC is a concern (tag: `TD-GC-STREAMING` already exists for this).

Option A is recommended for S1B scope. Option B is the cleaner long-term contract.

---

## Informational Fix 3: SetActive Coupling Warning

**File:** `Scripts/World/Graph/Runtime/NodePresentationHandle.cs:20`

**Problem:** `gameObject.SetActive(false)` disables the entire room root, which also owns the `BoxCollider` trigger used by `PlayerNodeTracker` for room occupancy detection. If the activation resolver ever incorrectly deactivates the occupied room, the trigger that would re-activate it is also disabled. Self-lockout loop.

Currently safe because Occupied always activates the current room. But any future bug in the resolver creates an unrecoverable state.

**Confirmed by:** Codex adversarial (unique finding).

**Fix direction:** Add a `// WARNING:` comment documenting the coupling. Longer-term, consider separating "presentation root" (visual geometry, toggled) from "tracking root" (triggers + authoring, always active). Not blocking for S1B.

---

## Informational Fix 4: Gizmo Color Priority

**File:** `Scripts/World/Graph/Debug/SpatialDebugGizmos.cs:103`

**Problem:** `GetNodeColor` checks `PortalVisible` before `Occupied`. A node that is both Occupied AND PortalVisible renders yellow (portal-visible) instead of green (active/occupied). The occupied room showing as yellow is misleading in debug view since Occupied is the stronger semantic.

**Fix direction:** Check Occupied flag first (highest priority), then PortalVisible, then Adjacent, then inactive. One-line reorder.

---

## Deferred / Not Blocking

These were flagged by reviewers but are intentional or out of scope:

| Item | Status | Rationale |
|------|--------|-----------|
| TB-3 PortalViewProbe not implemented | Intentionally deferred | Checkpoint notes this. Portal path passes empty probes. Occupied + Adjacent activation validated. |
| `PlayerId: "local"` hardcoded | In scope | PRD: "multiplayer-aware, not multiplayer-complete." Single-player wiring is correct for S1B. |
| `ApertureSize` field unused by evaluator | Known simplification | PRD: portal visibility = destination active, not frustum/occlusion test. |
| PlayerNodeTracker void transitions (prev=null) | Tested, intentional | 8 tests document this behavior. Design choice for void gap handling. |
| Missing XML docs on public contracts | Low priority | Code is self-documenting via naming. Not blocking. |
| Test gaps (tautological ForceAllActive test, missing boundary tests) | Follow-up | Testing specialist found 9 items. None block shipping but worth addressing post-merge. |

---

## Test Status

- 121+ EditMode tests passing (75 S1A + 46 S1B including 8 PlayerNodeTracker bugfix tests)
- Manual Gate 2 walkthrough passed in prior session
- No console errors

## Fix Execution Plan

Use `/tdd` skill for fixes 1 and 2 (they touch runtime logic that needs test coverage). Fix 3 is a comment. Fix 4 is a one-line reorder in debug-only code.

1. **Fix Camera.main** -- refactor `NodeStreamingController` to accept camera+tracker via injection or local-player hierarchy resolution. Add tests for the new binding path. Remove per-frame `FindAnyObjectByType` for tracker/camera.
2. **Fix aliasing** -- snapshot `LastResult` in `SpatialDebugGizmos.OnDrawGizmos`. Add a test that calls `Resolve` twice and verifies the aliasing contract.
3. **Add SetActive warning comment** on `NodePresentationHandle.SetPresentation`.
4. **Fix gizmo color priority** -- reorder flag checks in `GetNodeColor`.
5. **Re-run tests, verify in Play mode, then /ship.**
