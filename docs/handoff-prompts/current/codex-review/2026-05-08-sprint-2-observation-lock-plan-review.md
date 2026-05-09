# Sprint 2 Observation Lock Plan Review

Date: 2026-05-08

Reviewer: Codex

Source plan reviewed:
- `C:\Users\admin\.gstack\projects\spatial-horror\admin-sprint2-observation-lock-eng-review-test-plan-20260507-161200.md`
- User-pasted writeup in current session

Primary repo context reviewed:
- `docs/ARCH.md`
- `docs/design/04-sprints/sprint-2-observation-lock-system.md`
- `docs/design/03-systems/observation-lock-spatial-mutation-rules-spec.md`
- `docs/design/03-systems/co-op-observation-and-sync-rules-spec.md`
- `docs/design/02-architecture/spatial-runtime-framework.md`
- `docs/design/02-architecture/networked-house-runtime-interfaces-contracts.md`
- `unity-DESYNC/Assets/_Project/Scripts/World/Graph/...`
- `unity-DESYNC/Assets/_Project/Tests/EditMode/...`

## Executive Judgment

The plan is directionally strong and substantially aligned with the current codebase shape. It respects the repo's strongest architectural decisions:

- pure C# runtime logic hosted by thin Unity scene objects,
- debug-first treatment of hidden state,
- strict separation between design-time definitions and runtime state,
- and narrow Sprint 2 scope instead of prematurely building anomaly logic.

However, the plan currently under-documents three strategic constraints that are already load-bearing in project docs and should be made explicit before this gets atomized into mini-sprints:

1. per-node/per-edge locks are a Sprint 2 implementation substrate, not the long-term mutation abstraction,
2. the local observation input path is a temporary single-player adapter, not the future authority model,
3. stable-anchor or higher-priority protection needs a preserved seam now, even if the first implementation does not use it.

My recommendation is: **approve the plan conditionally**, after a short revision pass that makes those constraints explicit and corrects a few implementation-assumption mismatches.

---

## What The Plan Gets Right

### 1. Pure C# core hosted by `GraphRuntimeHost` is the correct runtime shape

This is aligned with both code and architecture docs.

- `docs/ARCH.md` explicitly locks `SpatialGraphRuntime` as pure C# and keeps `GraphRuntimeHost` as the Unity bridge.
- `unity-DESYNC/Assets/_Project/Scripts/World/Graph/GraphRuntimeHost.cs` already follows this exact pattern.

Using the same shape for `ObservationLockSystem` is consistent and lowers architectural risk.

### 2. Keeping observation legality separate from presentation activation is correct

The plan’s decision to avoid routing observation through `NodeActivationResolver` is sound.

- `NodeStreamingController` currently owns presentation activation only.
- `PortalVisibilityController` owns visibility evaluation inputs.
- `NodeActivationResolver` is already narrowly scoped to active-node presentation reasons.

That separation matches `docs/design/02-architecture/spatial-runtime-framework.md`, which distinguishes node activation, portal visibility, and observation gating as separate concerns.

### 3. Removing `portalVisibilityDotThreshold` from observation rules is the right boundary

The current code already places that tuning inside:

- `unity-DESYNC/Assets/_Project/Scripts/World/Graph/Runtime/PortalVisibilityController.cs`

Duplicating that threshold into `ObservationRulesDefinition` would split ownership of one perception rule across two systems. The plan’s revised decision to drop it is correct.

### 4. Debug-first treatment is strongly aligned with project vision

The plan’s overlay/gizmo emphasis is consistent with the repo’s most repeated design rule:

- hidden spatial systems must be legible in runtime,
- systems are not considered “done” without useful debug surfaces.

This is reinforced by:

- `docs/ARCH.md`
- `docs/design/02-architecture/spatial-runtime-framework.md`
- `docs/design/03-systems/observation-lock-spatial-mutation-rules-spec.md`

### 5. The plan follows the real module layout better than the abstract framework docs do

The repo’s implemented graph stack currently lives under:

- `Assets/_Project/Scripts/World/Graph/...`

and existing debug overlays already live under:

- `Assets/_Project/Scripts/World/Graph/Debug/...`

The reviewed plan mostly follows this reality, which is a better execution choice than force-fitting the work into the framework docs’ more abstract future namespace layout.

---

## Findings

## High Severity

### H1. The plan hardens node/edge locks without clearly labeling that this diverges from the broader region-lock direction

The most important strategic gap is not in the implementation mechanics. It is in the abstraction story.

`docs/design/03-systems/observation-lock-spatial-mutation-rules-spec.md` repeatedly points toward **region-level mutation gating** as the preferred long-term model:

- observation should resolve into region locks,
- mutation eligibility should be reasoned about at mutation-region scale,
- stable anchors and bounded mutable regions are part of the readability model.

The reviewed Sprint 2 plan instead defines:

- `NodeObservationState`
- `EdgeObservationState`
- `IObservationLockQuery` on node/edge ids

That is acceptable as a Sprint 2 implementation substrate. It is not acceptable if later agents interpret it as the project’s canonical mutation-lock model.

### Impact

If this remains implicit, later S3/S4 work will either:

- build mutation systems directly on node/edge lock APIs and deepen the wrong abstraction, or
- reopen the whole design later and incur avoidable churn.

### Required correction

The plan should state explicitly:

> Sprint 2 uses per-node and per-edge lock state as its internal observation substrate for the current 5-room slice. Mutation regions remain the intended higher-level abstraction for anomaly systems, and later mutation-facing systems may aggregate node/edge lock state into region eligibility.

Without that sentence, the plan is too easy to over-read.

---

## Medium Severity

### M1. The plan understates the multiplayer/authority evolution path

This repo already treats co-op observation semantics as more than a vague future idea.

Relevant docs:

- `docs/ARCH.md`
- `docs/design/03-systems/co-op-observation-and-sync-rules-spec.md`
- `docs/design/02-architecture/networked-house-runtime-interfaces-contracts.md`

Those docs establish a stable direction:

- one authoritative house truth,
- many players may contribute protection,
- authority owns aggregated observation truth,
- threshold crossing is a first-class protection reason,
- clients report facts, not final mutation legality.

The reviewed plan introduces:

- `IObservationInputSource`
- `LocalObservationInputSource`
- list-shaped occupied/visible node/edge methods

This is a good short-term seam. The problem is that the plan does not clearly state what it is **not**:

- it is not the future authoritative observation ledger,
- it is not the co-op aggregation model,
- it is not the final contract surface for network truth.

### Impact

Without that clarification, task atomization may accidentally freeze a local single-player adapter into a pseudo-authoritative system shape.

### Required correction

Add an explicit note:

> `LocalObservationInputSource` is a single-player local adapter for Sprint 2. In co-op, observation truth is expected to move to an authority-owned contribution/aggregation model, and this interface exists to make that replacement possible without rewriting `ObservationLockSystem`.

---

### M2. Stable-anchor and higher-priority protection seams are not preserved strongly enough

The Sprint 2 PDD and broader mutation rules both assume that observation is not the only future source of protection.

Relevant design pressure:

- stable anchors are a readability guardrail,
- some spaces may be permanently or temporarily protected independent of observation,
- future mutation legality is not “unobserved => always eligible”.

The plan mentions `ProtectedByRule` in examples and alludes to “no higher-priority rule marks it protected,” but it does not preserve a concrete extension seam strongly enough.

### Impact

If Sprint 2 only exposes occupancy/visibility/grace as hard-coded reasons and does not preserve a clean higher-priority protection hook, Sprint 3 anomaly work will likely need API changes instead of additive extension.

### Required correction

At minimum, preserve one of these:

1. `LockReason.ProtectedByRule` plus a small injection point for non-observation protection, or
2. a simple query/policy seam that can answer “is this node/edge protected independent of observation?”

The hook can be dormant in Sprint 2. It should still exist.

---

### M3. The reset/restart integration claims are ahead of repo reality

The plan’s Phase 6 says:

- `Reset()` clears all state
- integrate with existing restart path in `GraphRuntimeHost`

That “existing restart path” is not really present in current code.

What actually exists:

- `GraphRuntimeHost` initializes the graph in `Awake()`
- `SpatialDebugOverlay` has an `F5` path that manually calls `Runtime.Reset()` then `Runtime.Initialize(...)`

Files:

- `unity-DESYNC/Assets/_Project/Scripts/World/Graph/GraphRuntimeHost.cs`
- `unity-DESYNC/Assets/_Project/Scripts/World/Graph/Debug/SpatialDebugOverlay.cs`

There is no obvious shared round-reset orchestration service in the graph stack today, and no visible `MatchManager` implementation in current code to anchor this claim.

### Impact

If work is atomized assuming a stable restart hook already exists, downstream tasks will underspecify where observation state should actually reset.

### Required correction

Rewrite this part of the plan to match repo truth:

- there is a debug runtime reset path today,
- full round/restart orchestration may require a new owning hook,
- do not imply `GraphRuntimeHost` already owns restart semantics beyond graph initialization.

---

### M4. Phase 0 is a hard dependency, not just “enabling work”

The reviewed plan correctly calls out TD0018 wiring as Phase 0. The repo confirms this is still unresolved.

Current code:

- `NodeStreamingController.GetPortalResults(...)`
- returns `portalController.EvaluatePortals(ctx, Array.Empty<PortalProbeData>())`
- includes a direct comment: `TD0018: Replace stub probes with real PortalViewProbe scene data`

File:

- `unity-DESYNC/Assets/_Project/Scripts/World/Graph/Runtime/NodeStreamingController.cs`

### Impact

Any visibility-lock or debug-overlay work that assumes live portal results before probe wiring exists is working on false inputs.

### Required correction

When atomized, treat these as strict gates:

1. probe truth wiring,
2. visibility-lock logic,
3. visibility-related overlay/gizmo acceptance.

Do not let those become parallel “nice if possible” lanes unless the probe data contract is already stable.

---

### M5. Edge visibility derivation is workable for Sprint 2, but it is only an approximation

The plan derives visible edge ids via:

- visible destination node ids from portal results,
- plus `SpatialGraphRuntime.GetConnectedEdges(currentNode)`

Repo reality supports that approximation:

- `PortalVisibilityResult` currently contains `AnchorId`, `DestinationNodeId`, and `IsVisible`
- `SpatialGraphRuntime` supports connected-edge lookup and destination-node resolution

Files:

- `unity-DESYNC/Assets/_Project/Scripts/World/Graph/Runtime/PortalVisibilityContracts.cs`
- `unity-DESYNC/Assets/_Project/Scripts/World/Graph/Runtime/SpatialGraphRuntime.cs`

This is adequate for the current single-hop portal slice. It is not a complete future portal identity model.

### Impact

If this is documented as if it were the final contract, later threshold/crossing/protection work will be forced to reverse-engineer portal identity from insufficient data.

### Required correction

Add one note:

> Edge visibility derivation in Sprint 2 is a graph-query approximation over the current portal results contract. Future portal/threshold authority work may replace this with explicit portal/edge identity reporting.

---

### M6. Namespace and module-placement drift should be acknowledged explicitly

There is mild but persistent doc drift between:

- framework docs that place observation conceptually under mutation-oriented namespaces and UI debug directories,
- and the actual implemented repo layout under `World/Graph`.

The plan follows the repo. That is the correct choice for execution. But the mismatch should be acknowledged in the review or plan notes so future contributors stop oscillating.

### Recommended resolution

Short-term:

- keep Sprint 2 under `World/Graph` to match the implemented graph module.

Long-term:

- optionally clean up framework docs or add an `ARCH.md` note clarifying that observation-lock remains within the graph stack for now, even though its downstream consumer is mutation legality.

---

## Low Severity

### L1. The plan’s ownership split is mostly right, but the wording should guard against future accretion

The plan says `ObservationLockSystem` owns:

- collecting lock inputs,
- computing lock state,
- grace timers,
- eligibility queries,
- debug override

That is acceptable for Sprint 2 only if it remains a pure evaluator/registry over input snapshots.

The danger is not in the first implementation. The danger is later adding:

- mutation scheduling concerns,
- portal perception policy,
- stable-anchor rule policy,
- or networking concerns

directly into the same class.

### Recommended correction

Add one sentence:

> `ObservationLockSystem` remains a pure lock-evaluation and query service over externally supplied observation facts; perception gathering and mutation selection stay outside it even if the first local adapter lives nearby.

---

### L2. The plan’s use of current test style is well aligned with the repo

The proposed EditMode-first approach fits current practice.

Current examples:

- `GraphRuntimeHostTests.cs`
- `NodeStreamingControllerTests.cs`
- `PortalVisibilityControllerTests.cs`
- `PortalVisibilityEvaluatorTests.cs`

These existing tests already validate the patterns the plan is proposing:

- pure logic tests for C# runtime classes,
- reflection-driven lifecycle calls for `MonoBehaviour` hosts,
- lightweight scene-free EditMode coverage.

This part of the plan should remain unchanged.

---

## Codebase Fit Review

## Existing seams the plan can reuse cleanly

The following repo seams are real and compatible with the plan:

- `GraphRuntimeHost` as a Unity host for pure runtime services
- `SpatialGraphRuntime` as graph query source
- `PlayerNodeTracker` as current local occupancy signal
- `PortalVisibilityController` / `PortalVisibilityEvaluator` as current visibility signal
- existing IMGUI overlays and gizmo patterns under `World/Graph/Debug`
- existing EditMode test style and graph test utilities

Those are solid foundations.

## Existing hazards the plan needs to account for

### `PlayerNodeTracker` null-gap risk is real

`PlayerNodeTracker.ExitNode(...)` can leave `CurrentNodeId == null` during transitions.

File:

- `unity-DESYNC/Assets/_Project/Scripts/World/Graph/Runtime/PlayerNodeTracker.cs`

This aligns with existing TODO:

- `docs/TODO.md` -> `TD0013`

The plan is correct to treat this as a known hazard and to guard or smooth it. That should stay explicit in execution tasks.

### Probe truth is not wired yet

This is the largest concrete implementation blocker for visibility-backed observation.

The plan recognizes it; task atomization must preserve it as a gate.

### Restart/reset orchestration is incomplete

As noted above, the graph stack has debug reset, not a robust match reset contract.

If observation reset depends on “existing restart integration,” the plan is overselling current implementation reality.

---

## Design/Vision Alignment Review

## Strong alignment

The plan is strongly aligned with the project’s core vision in the following ways:

- it reinforces the covenant that the house changes only when meaningfully unobserved,
- it favors legibility over cleverness,
- it exposes hidden state early,
- it keeps scene objects thin,
- it keeps rules data-driven,
- it avoids prematurely building anomaly execution.

This matches:

- `docs/ARCH.md`
- `docs/design/04-sprints/sprint-2-observation-lock-system.md`
- `docs/design/01-vision/spatial-horror-gdd.md`

## Partial alignment requiring explicit caveats

The plan partially aligns, but needs explicit caveats, on:

- region-level future mutation abstraction,
- co-op authority truth,
- stable anchor protection,
- threshold/crossing protection as a future first-class rule.

These are already part of the project’s intended design horizon and should not be left implicit.

---

## Recommended Revisions Before `/writing-plans`

These are the minimum changes I recommend before atomization.

### 1. Add an abstraction note

State clearly that node/edge lock state is the Sprint 2 internal substrate, while mutation regions remain the future anomaly-facing abstraction.

### 2. Add an authority-evolution note

State clearly that `LocalObservationInputSource` is a local single-player adapter, not the final co-op truth model.

### 3. Preserve a non-observation protection seam

Even if dormant in Sprint 2, retain a clean hook for:

- stable anchors,
- higher-priority protection rules,
- or future “protected by rule” legality decisions.

### 4. Rewrite the reset section to match current code

Do not imply that `GraphRuntimeHost` already owns restart semantics beyond initialization.

### 5. Treat probe wiring as a hard gate

Do not split visibility-lock implementation or visibility acceptance tasks in ways that assume probe truth already exists.

### 6. Add one module-placement note

Document that Sprint 2 remains under the existing `World/Graph` module layout for consistency with implemented code, even if some design docs describe a more mutation-oriented namespace future.

---

## Suggested Mini-Sprint Breakdown

I would not atomize directly from the current phase list. I would split it this way instead:

### Slice A. Visibility input truth

Owns:

- real `PortalProbeData` production,
- verification that live portal results are non-empty and stable in Play mode,
- any contract cleanup needed for probe-to-result identity.

Why first:

- everything visibility-related depends on this being real.

### Slice B. Observation runtime core

Owns:

- `LockReason`
- `NodeObservationState`
- `EdgeObservationState`
- `ObservationRulesDefinition`
- `ObservationLockSystem`
- pure EditMode coverage for lock logic, grace, eligibility, reset.

Why second:

- this is the real deep module and should stabilize before scene wiring.

### Slice C. Local adapter and host wiring

Owns:

- `IObservationInputSource`
- `LocalObservationInputSource`
- `GraphRuntimeHost` integration
- single-player occupancy/visibility fact collection

Why third:

- it binds the pure runtime core to real game signals after those signals are trustworthy.

### Slice D. Debug and explainability

Owns:

- observation overlay
- observation gizmos
- counts, reasons, grace visibility
- debug override behavior

Why fourth:

- it depends on stable query surfaces from the runtime core.

### Slice E. Reset and acceptance pass

Owns:

- observation reset semantics
- any restart-path hook needed beyond current debug reset
- final smoke-test acceptance

Why last:

- reset work is easiest to do correctly once the real state graph exists.

---

## Final Verdict

This is a good plan. It is not overbuilt, and it mostly respects both the current codebase and the project’s actual architectural decisions.

The plan should **not** be rejected.

It should be **tightened** before atomization so that execution tasks do not accidentally solidify Sprint 2 shortcuts into long-term design commitments.

### Approval status

**Approve with revision notes**

Required before task atomization:

- explicitly mark node/edge lock state as Sprint 2 substrate, not final mutation abstraction,
- explicitly mark `LocalObservationInputSource` as local-only and temporary,
- preserve a seam for non-observation protection such as stable anchors,
- correct the reset/restart language to match repo truth,
- and gate visibility work on real probe wiring.

If those corrections are made, the plan is safe to split into mini-sprints.
