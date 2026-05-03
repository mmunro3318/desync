# Spatial Mutation Sprint / PDD

## Sprint title
Spatial Mutation Sprint — Mutation Runtime Foundation

## Document objective
Turn the Observation Lock / Spatial Mutation Rules Spec into concrete implementation work. This sprint defines the classes, interfaces, runtime flow, milestone tasks, debug tools, and acceptance criteria needed to make unlocked house regions mutate through legal graph operations instead of ad hoc scene hacks [file:12][file:13][file:14][file:15].

## Why this sprint exists
The project now has three prerequisite layers in place conceptually: the house builder pipeline, the authoritative house graph, and the observation-lock rule framework. The next step is to convert that rule framework into a working runtime mutation stack. This sprint exists to prove the game’s signature mechanic as a real system: space changes only when legal, only when unlocked, and only through an inspectable mutation pipeline [file:12][file:13].

## Sprint thesis
This sprint should prove that the game can identify an unlocked mutation region, generate legal graph mutation candidates for that region, reject invalid operations with explicit reasons, apply an approved mutation through the authoritative graph layer, and expose the entire process through debug views that are trustworthy during playtesting [file:12][file:13][file:15].

## Questions this sprint answers
- Can unlocked space mutate through graph operations rather than scene tricks?
- Can mutation legality remain explicit and testable?
- Can the system preserve readability through cooldowns, stable anchors, and rejection rules?
- Can developers inspect why a mutation did or did not happen [file:12][file:13][file:15]?

## Out of scope
This sprint does **not** include:
- final audiovisual presentation of anomalies,
- final network replication,
- advanced multi-layer dimension rules,
- creature-aware mutation strategy,
- or a large library of mutation families.

This sprint is about proving the mutation runtime foundation, not the final content breadth.

## Sprint goals
By the end of this sprint, the project should support:
- receiving current mutable-region information from the observation lock layer,
- generating mutation candidates for one or more bounded regions,
- validating those candidates against graph invariants and anchor rules,
- applying at least one successful mutation family to the authoritative graph,
- tracking cooldowns and mutation history,
- and visualizing the whole process in debug [file:12][file:13][file:14].

## Architectural rules carried into implementation
This sprint must preserve the project’s core architecture rules.

- Runtime state stays separate from configuration definitions [file:12][file:13][file:14].
- Scene objects stay thin; mutation logic lives in systems and graph operations [file:12][file:13].
- Hidden state must be observable in debug [file:12][file:13][file:15].
- New mutation content should mostly mean new data and new operation classes, not rewrites of the framework [file:12][file:13].

## Relationship to prior docs
This sprint depends on three earlier systems being conceptually defined.

### Inputs from prior layers
- `GeneratedHouseArtifact` and graph-seed output,
- authoritative `HouseGraphInstance`,
- observation and region-lock state,
- stable anchor definitions,
- mutation legality rules from the observation/mutation spec [file:12][file:13][file:14].

### Outputs from this sprint
- mutation candidate model,
- mutation generator,
- legality rule pipeline,
- mutation scheduler/director,
- graph mutation executor,
- cooldown tracking,
- mutation history/debug UI,
- and at least one working mutation family.

## Recommended folder targets
This sprint should fit into the existing project structure and naming discipline [file:13][file:14].

### Suggested folders
- `Assets/_Project/Data/Rooms/Mutation/`
- `Assets/_Project/Scripts/World/Graph/Mutation/`
- `Assets/_Project/Scripts/World/Rooms/Observation/`
- `Assets/_Project/Scripts/UI/Debug/`
- `Assets/_Project/Scripts/Core/Events/`
- `Assets/_Project/Scenes/Test/`

## Suggested namespaces
Use the same namespace discipline established in the starter docs so future Claude tasks remain unambiguous [file:13].

### Recommended namespaces
- `GhostHunt.World.Graph`
- `GhostHunt.World.Graph.Mutation`
- `GhostHunt.World.Observation`
- `GhostHunt.UI.Debug`
- `GhostHunt.Core.Events`

## Milestone framing
This sprint breaks into two concrete milestones.

### SM-1
Mutation candidate generation and legality pipeline.

### SM-2
Mutation execution, cooldown control, event flow, and debug visibility.

## Core runtime model
This sprint should formalize a clean split between mutation definitions and runtime state, in line with the rest of the project [file:12][file:13][file:14].

### Definition-side examples
- `MutationRuleDefinition`
- `MutationOperationDefinition`
- `MutationWeightProfile`
- `MutationCooldownProfile`
- `StableAnchorPolicyDefinition`

### Runtime-side examples
- `MutationCandidate`
- `MutationAttemptRecord`
- `MutationCooldownState`
- `RegionMutationState`
- `MutationHistoryEntry`
- `MutationDirectorState`

## Suggested core classes

| Class | Responsibility |
|---|---|
| `SpatialMutationDirector` | decides when a mutation attempt should occur |
| `MutationCandidateGenerator` | generates possible graph operations for eligible regions |
| `MutationLegalityService` | runs legality checks against graph and rules |
| `MutationCooldownTracker` | tracks per-region and global cooldowns |
| `HouseGraphMutationService` | applies approved mutation operations to the graph |
| `MutationHistoryLog` | stores recent attempts and outcomes |
| `SpatialMutationDebugOverlay` | visualizes mutable regions, cooldowns, and mutation results |
| `MutationSelectionPanel` | shows detailed data for selected candidate or history entry |

This keeps scheduling, generation, validation, execution, and debugging separated, which is consistent with your project-wide ownership discipline [file:12][file:13][file:15].

## Suggested interfaces
The exact signatures can evolve, but the sprint should lock in narrow contracts rather than direct system coupling [file:13][file:14].

```csharp
public interface IMutationCandidateGenerator
{
    IReadOnlyList<MutationCandidate> GenerateCandidates(HouseGraphInstance graph, IReadOnlyList<string> eligibleRegionIds);
}

public interface IMutationLegalityRule
{
    bool Evaluate(MutationCandidate candidate, HouseGraphInstance graph, out string rejectionReason);
}

public interface IGraphMutationOperation
{
    string OperationId { get; }
    bool CanApply(HouseGraphInstance graph);
    void Apply(HouseGraphInstance graph);
}
```

## Recommended first mutation families
To stay within vertical-slice discipline, the sprint should implement only one primary family and one optional secondary family [file:12][file:13][file:15].

### Primary family
- corridor extension or contraction

### Secondary family
- portal destination swap

These two are readable, graph-native, and closely aligned with your design goals around looping paths and impossible hallways.

## Candidate lifecycle
Every mutation attempt should move through an explicit pipeline.

1. Query currently eligible mutation regions.
2. Filter by local and global cooldown state.
3. Generate candidates for those regions.
4. Run legality rules on each candidate.
5. Weight/select one candidate or reject all.
6. Apply the chosen graph operation.
7. Record the outcome in mutation history.
8. Publish events and refresh debug views.

This explicit lifecycle keeps the system inspectable and Claude-friendly [file:12][file:13].

## Legality goals
Legality checks are the heart of the system. They prevent the house from feeling random, buggy, or unfair.

### Core legality constraints
- graph validity must be preserved,
- stable anchors must remain protected,
- required traversal continuity must remain intact,
- no operation may produce unresolved portal or stair endpoints,
- anti-thrash and anti-immediate-reversal rules must be respected,
- candidate region must currently be mutable,
- and operation-specific preconditions must be satisfied.

## Suggested legality rule classes
- `GraphIntegrityLegalityRule`
- `StableAnchorLegalityRule`
- `ReachabilityLegalityRule`
- `PortalEndpointLegalityRule`
- `CooldownLegalityRule`
- `NoImmediateReversalLegalityRule`
- `OperationSpecificLegalityRule`

## Cooldown model
Cooldowns should exist at more than one scale.

### Recommended cooldown layers
- global mutation cooldown,
- per-region cooldown,
- per-operation-family cooldown,
- optional per-node/portal cooldown for future tuning.

### Design goal
Cooldowns should preserve tension and readability, not merely rate-limit performance [file:12][file:13].

## Mutation history model
The system should remember recent attempts so debugging and tuning are possible.

### Suggested fields for `MutationHistoryEntry`
- `attemptId`
- `timestamp`
- `regionId`
- `operationFamily`
- `operationId`
- `accepted`
- `rejectionReason`
- `graphRevisionBefore`
- `graphRevisionAfter`

## Stable anchors and protected zones
This sprint should treat protected topology as a first-class input.

### Examples
- entry node cluster,
- safe camp or van threshold,
- artifact chamber after reveal,
- explicitly authored no-mutate debug zones.

### Rule
A mutation generator may propose candidates near anchors, but legality should reject candidates that would violate anchor policy.

## SM-1 overview
SM-1 proves that the game can reason about possible mutations correctly.

### SM-1 answers
Can the system generate mutation candidates for unlocked regions and evaluate them with explicit legality rules?

## SM-1 tasks

### SM1-T1 — Create mutation runtime models
Implement:
- `MutationCandidate`
- `MutationAttemptRecord`
- `MutationHistoryEntry`
- `MutationCooldownState`
- `RegionMutationState`

#### Acceptance criteria
- Runtime types compile and remain separate from definition assets.
- They capture region, operation family, legality outcome, and cooldown-relevant metadata.

### SM1-T2 — Create definition assets and enums
Implement the first data-side mutation configuration types.

#### Suggested types
- `MutationRuleDefinition`
- `MutationOperationDefinition`
- `MutationWeightProfile`
- `MutationCooldownProfile`
- `MutationOperationFamily` enum

#### Acceptance criteria
- Mutation scheduling and weighting can be tuned without rewriting the whole runtime system [file:12][file:13][file:14].

### SM1-T3 — Implement eligible-region query integration
Connect to the observation/lock layer to retrieve currently mutable region ids.

#### Acceptance criteria
- Mutation systems do not guess mutable state independently.
- Regions currently locked by valid observation are never treated as eligible [file:12][file:13].

### SM1-T4 — Implement `MutationCandidateGenerator`
Build the candidate generation layer for the first mutation families.

#### Should support at minimum
- corridor extension/contraction candidates,
- portal swap candidates.

#### Acceptance criteria
- Candidate generation is deterministic for a fixed graph state and random seed.
- Generated candidates are bounded to declared regions.
- Generator output is inspectable in debug.

### SM1-T5 — Implement legality rule pipeline
Build `MutationLegalityService` and its first legality-rule modules.

#### Required first rules
- graph integrity,
- anchor protection,
- reachability,
- cooldown compliance,
- no-immediate-reversal,
- operation-specific preconditions.

#### Acceptance criteria
- Rejected candidates produce explicit reasons.
- Legality evaluation is testable without scene boot.

### SM1-T6 — Add candidate debug panel
Show generated candidates, legality result, selected region, and rejection reasons.

#### Acceptance criteria
- A developer can inspect why a region produced zero valid candidates.
- Candidate details are readable enough to tune generation rules.

### SM1-T7 — Add edit-mode legality tests
Create targeted tests for common acceptance and rejection cases.

#### Suggested tests
- mutation rejected when region is locked,
- mutation rejected when anchor path is broken,
- mutation rejected during cooldown,
- valid corridor extension accepted,
- valid portal swap accepted.

#### Acceptance criteria
- Tests run outside full gameplay scenes.
- Failures point to mutation code directly.

## SM-1 completion definition
SM-1 is complete when unlocked regions can produce inspectable mutation candidates, legality rules can accept or reject them with explicit reasons, and the system is testable without requiring full play-mode orchestration [file:12][file:13][file:15].

## SM-2 overview
SM-2 proves that legal mutations can actually change the house runtime state.

### SM-2 answers
Can the system schedule, apply, record, and expose legal mutations during runtime without breaking graph validity or player readability?

## SM-2 tasks

### SM2-T1 — Implement `SpatialMutationDirector`
Build the scheduler/director that decides when to attempt mutation.

#### Inputs may include
- elapsed time,
- global cooldown,
- region cooldown,
- progression pressure,
- anomaly budget,
- or debug/manual trigger.

#### Acceptance criteria
- Mutation attempts are paced and tunable.
- The director can be driven manually in debug for deterministic testing.

### SM2-T2 — Implement `MutationCooldownTracker`
Track global and regional cooldown state.

#### Acceptance criteria
- Cooldowns are queryable by region and operation family.
- Cooldown state is exposed to debug systems.

### SM2-T3 — Implement `HouseGraphMutationService`
Apply approved graph mutation operations to the authoritative graph.

#### Responsibilities
- verify operation still valid at apply time,
- mutate graph state,
- increment graph revision or mutation counter,
- publish mutation events,
- and hand back summary data.

#### Acceptance criteria
- A legal mutation changes graph state predictably.
- Invalid-at-apply-time candidates fail safely and visibly.

### SM2-T4 — Implement first executable mutation operations
Create concrete graph operation classes for the first mutation families.

#### Required first operations
- one corridor extension/contraction implementation,
- one portal swap implementation.

#### Acceptance criteria
- At least one mutation family works end to end in runtime.
- Operations do not depend on scene geometry hacks to define success.

### SM2-T5 — Implement mutation events and history log
Add event publication and persistent recent history.

#### Suggested events
- `OnMutationAttemptStarted`
- `OnMutationRejected`
- `OnMutationApplied`
- `OnMutationCooldownStarted`

#### Acceptance criteria
- Other systems can observe mutation outcomes without direct coupling.
- History entries preserve enough context for tuning.

### SM2-T6 — Build runtime mutation debug overlay
Visualize:
- eligible regions,
- cooling-down regions,
- pending candidate,
- last rejection reason,
- last successful mutation,
- and graph revision.

#### Acceptance criteria
- Runtime mutation state is understandable in graybox play.
- Debug categories can be toggled independently.

### SM2-T7 — Create graybox smoke test workflow
Add one practical scene workflow for observing mutations during play.

#### Could include
- auto-attempt mode,
- manual step mutation button,
- candidate cycle button,
- lock/unlock debug visualization.

#### Acceptance criteria
- A developer can reproduce, inspect, and screenshot mutation outcomes reliably.

### SM2-T8 — Add anti-thrash regression tests
Create tests for cooldown, no-reversal, and repeat-apply protections.

#### Acceptance criteria
- The same region cannot mutate in obviously broken rapid oscillation loops unless debug override explicitly allows it.

## SM-2 completion definition
SM-2 is complete when the runtime can schedule and apply at least one legal graph mutation family to unlocked regions, record and expose the result in debug, and preserve graph validity plus basic readability protections [file:12][file:13][file:15].

## Suggested ownership map
To keep Claude Code effective, each area should have a narrow owner.

| Area | Owns | Does not own |
|---|---|---|
| Candidate generator | proposes operations | final legality decisions |
| Legality service | accepts/rejects candidates | mutation scheduling |
| Mutation director | pacing and selection | low-level graph edits |
| Graph mutation service | operation execution | observation sensing |
| Cooldown tracker | mutation timing state | candidate generation |
| Debug overlay | visibility of hidden mutation state | mutation authority |

## Suggested Claude Code workflow
This sprint should be implemented through tightly scoped prompts, not broad “make mutation work” requests, because the project’s earlier docs already show that narrow ownership and explicit acceptance criteria produce better AI-assisted results [file:12][file:13][file:15].

### Good prompt shape
- 1–3 classes at a time,
- exact folder targets,
- explicit public methods/interfaces,
- defined acceptance tests,
- and required debug output.

### Bad prompt shape
- “Implement the full mutation system.”
- “Make the house start changing.”
- “Figure out spatial horror mechanics.”

## Suggested Claude task order
A practical implementation order is:
1. mutation runtime models,
2. mutation definition assets/enums,
3. eligible-region integration,
4. candidate generator,
5. legality service and rules,
6. candidate debug panel,
7. edit-mode legality tests,
8. mutation director,
9. cooldown tracker,
10. graph mutation service,
11. first executable operations,
12. mutation history/events,
13. runtime debug overlay,
14. graybox smoke workflow,
15. anti-thrash regression tests.

## Testing strategy
Use three layers of testing.

### 1. Pure rule tests
Verify legality-rule outcomes, cooldown logic, and reversal protections.

### 2. Graph mutation tests
Verify that concrete mutation operations preserve graph invariants and produce expected topology changes.

### 3. Graybox runtime tests
Verify that unlocked regions mutate in play, locked regions do not, and debug overlays explain the result.

## Risks and mitigations

### Risk 1
Mutation generation becomes too broad and produces nonsense candidates.

### Mitigation
Keep region-bounded generation and only implement one or two mutation families first.

### Risk 2
The system applies graph changes that are legal structurally but unreadable experientially.

### Mitigation
Enforce stable anchors, cooldowns, and anti-reversal protections early.

### Risk 3
Mutation logic drifts away from observation-lock truth.

### Mitigation
Require eligible-region input from the observation layer; do not duplicate mutability logic.

### Risk 4
Claude starts solving mutations with scene-specific hacks.

### Mitigation
Keep graph operations as the only authoritative mutation mechanism and require explicit debug summaries for every applied mutation [file:12][file:13][file:14].

## Acceptance checklist
This sprint is successful when:
- unlocked mutation regions are queried from the observation-lock layer rather than inferred ad hoc [file:12][file:13],
- the system can generate inspectable mutation candidates for those regions,
- legality rules accept or reject candidates with explicit reasons,
- at least one mutation family executes through authoritative graph operations,
- cooldown and anti-thrash protections are active,
- mutation history and events are exposed to debug systems,
- and the resulting runtime behavior supports readable impossible-space horror rather than arbitrary topology noise [file:12][file:13][file:14][file:15].

