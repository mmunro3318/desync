# S1A Sprint Wrapup Handoff

> **Status:** STUB — fill after smoke test passes, before /document-release -> /ship -> /review

## Sprint objective (from PDD)
Define and implement the first minimal house graph as data, then load it into a runtime shell that can be queried, debugged, and reset.

## What shipped
- [ ] Fill after smoke test

## What was deferred (and why)

### Network sync (NetworkVariable<HouseSnapshot>)
**Originally planned for:** Session 3 (per checkpoint, not per sprint PDD)
**Deferred because:** Not in S1A sprint PDD scope. The ARCH decision about "full snapshot sync" defines the *strategy* for when sync is needed, not a requirement to implement it in S1A. S1A's done criteria are: graph data exists, runtime queries work, debug output explains state. Network sync belongs in S1B or when multiplayer graph features are first needed.
**Where it goes:** S1B or S3 (multiplayer graph sync sprint). The ARCH.md decision (AD-005: full snapshot sync, evolve to deltas) stands as the design intent.

### SpatialGraphRuntime -> HouseGraphRuntime rename (TD0011)
**Originally surfaced by:** Counter-drift session
**Deferred because:** Taste call, not a drift bug. "Spatial" describes function, "House" matches type family. Either works. Not worth blocking S1A ship.
**Where it goes:** TD0011, post-S1A merge. Low effort (15m), low risk.

### House_Graybox geometry test failures (TD0012)
**Originally surfaced by:** S0.3 geometry grammar rules landing before scene geometry was updated
**Deferred because:** Not S1A scope. The tests are correct (they define expected behavior), the scene geometry is stale.
**Where it goes:** TD0012 at P1. Fix scene geometry or retire tests if House_Graybox is superseded.

## Decisions made during S1A
- [ ] Fill from ARCH.md S1A decisions (AD-001 through AD-009)

## Files created/modified
- [ ] Fill from git diff main...feat/s1a-house-graph-runtime

## Test coverage
- [ ] Fill final test count and pass rate

## Handoff to /document-release -> /ship -> /review
1. Run /document-release to update README, ROADMAP, CLAUDE.md for what shipped
2. Run /ship to create PR (squash merge target: main)
3. Run /review for pre-landing code review
4. After merge: run /counter-drift to catch any naming drift S1A introduced
