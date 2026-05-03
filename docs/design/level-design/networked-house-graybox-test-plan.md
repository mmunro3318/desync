# Networked House Graybox Test Plan

## Document objective
Define a repeatable graybox test plan for the networked impossible-house runtime. This plan specifies the test environment, scenario matrix, execution flow, observations to record, pass/fail expectations, and regression priorities needed to validate multiplayer graph truth, observation locks, threshold-crossing safety, mutation legality, portal authority, and client reconciliation [file:12][file:13][file:14][file:15].

## Why this document exists
The architecture docs, sprint docs, contract docs, task pack, and debug overlay spec all establish how the networked house **should** behave. The graybox test plan exists to prove whether it actually behaves that way in a repeatable runtime environment. Earlier project planning consistently emphasizes small, testable slices, graybox-first iteration, and debug visibility as the safest way to build hidden-state systems; this plan applies those same principles to the impossible-house multiplayer stack [file:12][file:13][file:14][file:15].

## Core thesis
The networked house should be validated through a small deterministic house slice and a scenario matrix, not through broad ad hoc playtests alone. The point of graybox testing is not to see whether the final game is scary yet. The point is to isolate correctness, contradictions, and readability in the runtime rules that make spatial horror possible [file:12][file:13][file:15].

## Test goals
This test plan should validate:
- shared graph authority,
- per-player observation contribution,
- aggregated protection behavior,
- threshold-crossing safety windows,
- mutation allow/reject logic,
- portal authority correctness,
- stale-client reconciliation,
- and debug-overlay usefulness [file:12][file:13][file:15].

## Out of scope
This plan does **not** aim to validate:
- final fun factor,
- final art readability,
- creature AI production behavior,
- lobby/matchmaking quality,
- voice chat,
- production networking optimization,
- or four-player scale.

This is a **two-player-first** runtime correctness plan.

## Test philosophy
Use the same project philosophy already established in the earlier docs [file:12][file:13][file:14][file:15].

- Graybox before polish [file:12][file:13][file:15].
- Prove one major question at a time [file:12][file:15].
- Keep tests deterministic where possible [file:13][file:14].
- Hidden systems must be debug-visible [file:12][file:13][file:15].
- Thin scenes, focused systems, and stable ownership matter during testing as much as during coding [file:12][file:13][file:14].

## Test environment
Use one dedicated graybox scene and one minimal deterministic house graph.

### Recommended scene
- `Assets/_Project/Scenes/Test/NetworkedHouse_Graybox.unity`

### Recommended graph slice
A very small graph is ideal:
- Entry,
- Hall A,
- Hall B,
- Side Room,
- Loop Return,
- Exit/Test Anchor.

This graph should include:
- at least one ordinary corridor chain,
- at least one loop opportunity,
- at least one portal/doorway that can remap,
- at least one threshold where crossing safety matters,
- and at least one mutation candidate region not currently observed.

## Test build assumptions
Assume:
- one host/server instance,
- one client instance,
- deterministic seed for graph/runtime setup,
- debug overlay enabled,
- mutation randomness either disabled or seeded.

## Required tooling before running tests
The plan assumes several tools are already present or near-present.

### Required runtime tooling
- networked house graybox harness,
- deterministic graph seed/setup,
- two-player test spawn positions,
- mutation trigger or deterministic mutation scheduler,
- portal authority state visibility,
- reconciliation trigger path,
- and the debug overlay from the debug spec [file:12][file:13][file:15].

### Required debug surfaces
At minimum, testers must be able to inspect:
- authoritative house version,
- topology version,
- portal version,
- each player’s current node,
- each player’s observation contribution,
- aggregated protected node and portal ids,
- last mutation decision,
- portal destination truth,
- reconciliation state per client [file:12][file:13][file:15].

## Roles during manual test sessions
Even with two people, role clarity helps.

### Host tester
Focus on shared authority truth, mutation decision outcomes, and snapshot publication.

### Client tester
Focus on local presentation, stale-state symptoms, and threshold-crossing feel.

### Optional observer
Capture logs, screenshots, and result notes if a third person is available.

## Standard test loop
Use the same flow for most scenarios.

1. Launch host and client into the graybox scene.
2. Confirm both players spawn at expected positions.
3. Confirm debug overlay shows matching initial versions.
4. Reset any scenario-specific flags.
5. Execute the test script exactly.
6. Record expected versus actual state.
7. Capture screenshot/video if mismatch occurs.
8. Reset scene/runtime state before the next test.

## Result categories
Every test case should end in one of these result tags.

- Pass.
- Fail.
- Pass with concern.
- Inconclusive due to tool gap.
- Blocked by prior bug.

## Severity labels
Use simple severity tags.

- S1: contradiction of shared truth or hard desync.
- S2: mutation/portal behavior functionally wrong but recoverable.
- S3: debug/readability issue or non-blocking inconsistency.
- S4: polish/clarity issue.

## Scenario matrix
The test plan is organized by scenario families.

| Family | Goal |
|---|---|
| A | Baseline authority truth |
| B | Observation contribution and aggregation |
| C | Threshold crossing safety |
| D | Mutation legality |
| E | Portal authority correctness |
| F | Reconciliation and stale-state recovery |
| G | Combined stress cases |
| H | Debug overlay validation |

## Family A — Baseline authority truth
These tests make sure the house starts coherent before any advanced behavior occurs.

### A1 — Initial version alignment
**Goal:** Both players begin with matching authoritative version state.

**Setup:** Spawn host and client with no mutations triggered.

**Steps:**
1. Open summary and authority panels on both instances.
2. Compare house version, topology version, and portal version.
3. Compare current node assignments.

**Expected:**
- Both players report the same house/topology/portal versions.
- Each player is assigned the correct current node.
- No client is marked stale.

### A2 — Stable idle state
**Goal:** Idle runtime does not fabricate changes or stale warnings.

**Setup:** Both players remain still for a short interval.

**Steps:**
1. Do not move either player.
2. Watch summary, observation, and reconciliation panels.

**Expected:**
- No mutation fires unexpectedly unless the deterministic scheduler says it should.
- No stale-client warnings appear.
- Protected sets remain stable and readable.

## Family B — Observation contribution and aggregation
These tests validate per-player reporting and shared protection union logic.

### B1 — Single-player observation contribution
**Goal:** One player protects a region through observation while the other contributes nothing.

**Steps:**
1. Player A faces Hall A and portal P1.
2. Player B remains in Entry looking away.
3. Inspect Observation panel.

**Expected:**
- Player A contribution lists the expected protected node/portal ids.
- Aggregated protected set matches Player A’s contribution.
- Player B contribution is empty or minimal.

### B2 — Union protection from two players
**Goal:** Aggregated protection is the union of valid contributions.

**Steps:**
1. Player A observes Hall A.
2. Player B observes Side Room or Portal P2.
3. Inspect aggregated protected sets.

**Expected:**
- Aggregated protected nodes/portals include both players’ valid contributions.
- Per-player contributions remain distinguishable in debug.

### B3 — Contribution replacement
**Goal:** A player’s new contribution replaces the old one cleanly.

**Steps:**
1. Player A observes Hall A.
2. Player A rotates and moves to observe Hall B instead.
3. Inspect per-player and aggregated sets.

**Expected:**
- Player A’s old protected region falls away according to policy/grace behavior.
- New protected region appears correctly.
- No duplicate stale contribution remains attached to Player A.

## Family C — Threshold crossing safety
These tests validate the temporary mutation-safe window during doorway crossing.

### C1 — Portal crossing creates temporary protection
**Goal:** Active crossing raises threshold protection.

**Steps:**
1. Player A begins crossing portal P1.
2. Freeze or inspect immediately during crossing.
3. Observe threshold state in Observation and Portal panels.

**Expected:**
- Portal P1 shows threshold-crossing protection active.
- Related node/portal protection appears in aggregated sets.
- Debug reason explicitly mentions threshold crossing.

### C2 — Crossing protection expires
**Goal:** Temporary threshold protection clears after crossing ends.

**Steps:**
1. Player A completes crossing.
2. Wait past the configured grace/expiry window.
3. Inspect protection state.

**Expected:**
- Threshold protection expires predictably.
- Portal is no longer marked crossing-protected unless another reason exists.

### C3 — Repeated crossing refreshes protection
**Goal:** Repeated use refreshes crossing safety without breaking cleanup.

**Steps:**
1. Player A crosses P1.
2. Quickly re-crosses P1.
3. Inspect expiry timestamps or sequence changes.

**Expected:**
- Protection refreshes cleanly.
- No stuck permanent crossing state appears.

## Family D — Mutation legality
These tests validate the allow/reject rules for topology changes.

### D1 — Mutation rejected when affected node is protected
**Goal:** Protected regions block mutation.

**Steps:**
1. Player A protects Hall A.
2. Trigger a mutation candidate affecting Hall A.
3. Inspect Mutation panel.

**Expected:**
- Mutation is rejected.
- Rejection reason names the protected node or portal.
- Topology version does not change.

### D2 — Mutation rejected during threshold crossing
**Goal:** Crossing safety blocks conflicting mutation.

**Steps:**
1. Player A begins crossing P1.
2. Trigger mutation candidate affecting P1 or its destination region.
3. Inspect decision trace.

**Expected:**
- Mutation is rejected.
- Rejection reason explicitly cites threshold protection or transition lock.

### D3 — Mutation allowed when no protected ids conflict
**Goal:** Legal mutation commits cleanly when the affected region is unobserved.

**Steps:**
1. Move both players away from the target mutation region.
2. Trigger mutation candidate affecting an unprotected region.
3. Inspect summary, authority, and mutation panels.

**Expected:**
- Mutation is approved and committed.
- Topology version increments.
- Recent mutation history records the event.

### D4 — Sequential mutation consistency
**Goal:** Multiple legal mutations maintain coherent version history.

**Steps:**
1. Run two or three legal mutations in sequence.
2. Compare mutation history and topology version progression.

**Expected:**
- Versions advance monotonically.
- History order matches commit order.
- No client shows inconsistent mutation history.

## Family E — Portal authority correctness
These tests validate that portal truth is shared even if local presentation differs briefly.

### E1 — Authoritative portal destination visible in debug
**Goal:** Portal authority state can be inspected directly.

**Steps:**
1. Select portal P1 in the Portal panel.
2. Compare authoritative destination and local rendered destination on host and client.

**Expected:**
- Authoritative destination is clear and consistent.
- If local render differs temporarily, that difference is visible rather than hidden.

### E2 — Portal remap after legal mutation
**Goal:** A portal updates to new authority state after mutation.

**Steps:**
1. Trigger a legal mutation that changes portal P1 destination.
2. Inspect portal state on both players.

**Expected:**
- Authoritative portal destination updates.
- Portal version updates.
- Both players reconcile to the new truth.

### E3 — Portal transition lock during crossing
**Goal:** Portal authority prevents contradictory remap during active crossing.

**Steps:**
1. Player A begins crossing P1.
2. Attempt to trigger portal remap for P1.
3. Inspect decision state.

**Expected:**
- Portal is protected/locked.
- Remap does not occur while unsafe.

## Family F — Reconciliation and stale-state recovery
These tests validate client recovery behavior.

### F1 — Client stale version detection
**Goal:** Stale client state is detectable.

**Steps:**
1. Force or simulate stale client versions after a committed mutation.
2. Inspect Reconciliation panel.

**Expected:**
- Client is marked stale.
- Mismatch reason is visible.
- Authority remains correct.

### F2 — Client reconciliation after stale state
**Goal:** Stale client can recover to shared truth.

**Steps:**
1. Create stale client state.
2. Trigger reconciliation path.
3. Inspect post-reconciliation versions.

**Expected:**
- Client updates to authoritative house/topology/portal versions.
- Reconciliation result is visible in history or current panel state.

### F3 — Reconciliation does not invent new truth
**Goal:** Recovery consumes authority truth rather than reinterpreting it.

**Steps:**
1. Create a mismatch.
2. Reconcile.
3. Compare authoritative state, local state, and post-reconcile snapshots.

**Expected:**
- Local view conforms to authority.
- No secondary “corrected” truth appears on the client.

## Family G — Combined stress cases
These tests intentionally combine systems.

### G1 — Player A watches, Player B tempts mutation
**Goal:** Shared protection works across distributed players.

**Steps:**
1. Player A watches a hall.
2. Player B moves elsewhere and attempts to trigger mutation in A’s observed region.

**Expected:**
- Mutation is rejected.
- Reason credits the relevant protection source.

### G2 — One player crossing, one player looking away
**Goal:** Threshold crossing protection dominates even when the second player is not observing.

**Steps:**
1. Player A crosses P1.
2. Player B looks away and removes ordinary observation protection.
3. Attempt conflicting mutation.

**Expected:**
- Crossing protection still blocks unsafe mutation.

### G3 — Reconciliation after legal mutation during low observation
**Goal:** Legal mutation plus stale client recovery remains coherent.

**Steps:**
1. Both players leave a mutation region unprotected.
2. Trigger legal mutation.
3. Delay client update or simulate stale state.
4. Reconcile.

**Expected:**
- Mutation commits once.
- Stale client recovers cleanly.
- Portal and topology versions match after reconciliation.

## Family H — Debug overlay validation
These tests verify that the debug layer itself is useful enough.

### H1 — “What is true right now?” test
**Goal:** A tester can answer the core question quickly.

**Steps:**
1. During any live scenario, ask the tester to identify:
   - current house version,
   - current protected portal ids,
   - who is protecting them,
   - and why the last mutation did or did not happen.

**Expected:**
- Tester can answer correctly within a short glance/inspection window.

### H2 — Panel sufficiency test
**Goal:** Required data is present in panels without digging into logs.

**Expected:**
- Authority, Observation, Mutation, Portal, and Reconciliation panels expose enough information to explain the scenario.

### H3 — World gizmo correlation test
**Goal:** Spatial debug visuals line up with panel truth.

**Expected:**
- protected nodes/portals shown in gizmos match panel state,
- player current node indicators are correct,
- mutation-affected region visuals align with mutation traces.

## Execution cadence
Not every test needs to run every day.

### Daily smoke set
- A1,
- B1,
- C1,
- D1,
- D3,
- E2,
- F2,
- H1.

### Pre-merge or milestone set
Run all A–H families relevant to touched systems.

### Regression set after bug fixes
Always rerun the original failing scenario plus at least one nearby scenario from the same family.

## Recording format
Use a lightweight but consistent result log.

### Suggested fields
- test id,
- date/build hash,
- host/client participants,
- result tag,
- severity if failed,
- expected behavior,
- actual behavior,
- screenshot/video reference,
- notes on likely subsystem.

## Failure triage guidance
When a test fails, classify the likely source early.

### Common buckets
- authority truth bug,
- observation contribution bug,
- ledger aggregation bug,
- threshold safety bug,
- mutation gate bug,
- portal authority bug,
- reconciliation bug,
- debug overlay visibility gap.

This helps keep fixes targeted and consistent with your narrow-ownership architecture [file:12][file:13][file:15].

## Exit criteria for the graybox runtime slice
The networked-house graybox slice is in good shape when:
- baseline authority truth is stable,
- per-player observation and aggregated protections are inspectable and correct,
- threshold crossing consistently prevents unsafe remaps,
- legal and illegal mutations behave predictably,
- portal authority updates coherently,
- stale clients can recover to shared truth,
- and the debug overlay can explain failures quickly enough to support rapid iteration [file:12][file:13][file:15].

## Recommended next document
After this test plan, the strongest next step is **Networked House Vertical Slice Integration Checklist**, because you now have architecture, tasks, debug surfaces, and tests; the next useful layer is a single checklist describing what must be wired together in the Unity scene and runtime harness to move from isolated services into a playable co-op impossible-house slice.

