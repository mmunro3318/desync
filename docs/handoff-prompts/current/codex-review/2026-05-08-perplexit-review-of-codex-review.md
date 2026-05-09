Short version: the plan is good and implementable, and Codex’s review is mostly useful as a clarification pass rather than a redesign request.

I would keep the current implementation direction, accept several of Codex’s framing edits, and reject any pressure to turn Sprint 2 into region locks, full co-op authority, or true LOS work right now.

Keep
Keep the core shape exactly as your plan now describes it: pure C# ObservationLockSystem, hosted by GraphRuntimeHost, separate from presentation activation, with LocalObservationInputSource polling tracker plus portal visibility rather than reusing NodeActivationResolver.

Also keep the decisions you already locked around Phase 0 probe wiring, edge-ID derivation by graph query, per-target grace, dropping the duplicate dot-threshold tunable, and adding enumeration to IObservationLockQuery; those were the real execution blockers and your plan now resolves them cleanly.

Change
The main edits I’d make are wording edits, not architectural ones.

First, Codex is right that the plan should say explicitly that node/edge lock state is a Sprint 2 substrate, not the long-term anomaly-facing abstraction, and that LocalObservationInputSource is a temporary local adapter rather than the future co-op authority model.

Second, rewrite Phase 6’s “existing restart path in GraphRuntimeHost” language, because the repo truth appears closer to “debug reset exists today; formal round-reset ownership may need to be added” than “restart integration is already there.”

Defer
I would defer, not implement now, Codex’s broader concerns about observer identity, region locks, and full authoritative aggregation.

Codex itself frames those as long-horizon tensions, while the Sprint 2 plan is intentionally scoped to a 5-room slice, a local adapter seam, and debuggable mutation-gating before anomaly systems arrive.

Relatedly, keep edge-visibility derivation as the current graph-query approximation and do not expand PortalVisibilityResult yet unless threshold/portal identity becomes a concrete Sprint 3 requirement.

Add seams
Codex’s best medium-severity note is the one about preserving a dormant higher-priority protection seam.

Your plan already mentions ProtectedByRule and “no higher-priority rule marks it protected,” but I would make that more explicit in the architecture section with one sentence saying non-observation protection can be injected later without changing the core observation model.

I would also add Codex’s suggested guardrail sentence that ObservationLockSystem stays a pure evaluator/query service over externally supplied facts, so later work does not accrete mutation scheduling, networking, or perception policy into it.

Doc polish
The biggest presentational improvement is to move a few caveats out of the “NOT in Scope” table and into an explicit “Future seams / non-goals” subsection near the top of the plan.

That subsection should name four things plainly: region locks are future-facing, LocalObservationInputSource is local-only, probe wiring is a hard gate for visibility acceptance, and true LOS is a tracked TODO rather than Sprint 2 scope.

If you make those edits, I’d treat the design doc as ready to implement rather than needing another concept pass.