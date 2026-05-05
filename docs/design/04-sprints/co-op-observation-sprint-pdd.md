# Co-op Observation Sprint / PDD

## Sprint title
Co-op Observation Sprint — Authority, Lock Ledger, and Two-Player Mutation Safety

## Document objective
Turn the Co-op Observation and Sync Rules Spec into concrete implementation work. This sprint defines the runtime models, classes, interfaces, milestone tasks, debug tooling, and two-player acceptance tests needed to make impossible-house observation rules behave fairly and consistently across multiple players [file:12][file:13][file:14][file:15].

## Why this sprint exists
The single-player impossible-house systems can define when space may mutate, how portals resolve, and how local presentation stays believable. The moment a second player joins, the core mechanic becomes a multiplayer authority problem. This sprint exists to ensure that shared topology truth, aggregated observation locks, threshold-crossing protection, and mutation approval all behave coherently under two-player co-op [file:12][file:13].

## Sprint thesis
This sprint should prove that a host/server-authoritative house can accept observation contributions from two players, aggregate them into shared protection rules, reject illegal mutations when any relevant player is still observing or occupying the affected region, and reconcile clients back to one shared house truth without obvious contradiction [file:12][file:13][file:15].

## Questions this sprint answers
- Can one authority own graph and mutation truth in co-op?
- Can two players contribute observation protections that meaningfully block mutation?
- Can threshold crossing create temporary mutation-safe protection windows?
- Can clients keep local presentation differences without diverging on shared house truth?
- Can developers inspect why a mutation was blocked or allowed [file:12][file:13][file:15]?

## Out of scope
This sprint does **not** include:
- full matchmaking/lobby flow,
- voice chat,
- final netcode optimization,
- production-grade lag compensation,
- final creature networking,
- or full four-player scaling.

This sprint is the first robust **two-player** co-op authority slice.

## Architectural rules carried forward
This sprint must preserve the project’s broader architecture rules [file:12][file:13][file:14][file:15].

- Runtime truth stays separate from design-time definition data [file:12][file:13][file:14].
- Hidden state must be visible in debug, especially in co-op [file:12][file:13][file:15].
- Systems should keep narrow ownership rather than collapsing into giant multiplayer managers [file:12][file:13][file:15].
- Local presentation may vary, but authoritative house truth may not [file:12][file:13].

## Relationship to prior docs
This sprint depends on:
- authoritative house graph runtime,
- observation-lock semantics,
- mutation legality runtime,
- portal authority rules,
- and the co-op observation and sync rules spec [file:12][file:13][file:14][file:15].

### Outputs from this sprint
- authority-owned observation ledger,
- client observation reporting path,
- mutation gate for co-op legality checks,
- threshold-crossing protection support,
- shared portal authority state integration,
- client reconciliation path,
- and two-player debug/graybox test workflow.

## Recommended folder targets
This sprint should land in predictable locations within the existing Unity project structure [file:13][file:14].

### Suggested folders
- `Assets/_Project/Scripts/Match/Networking/`
- `Assets/_Project/Scripts/World/Graph/Networking/`
- `Assets/_Project/Scripts/World/Graph/Observation/`
- `Assets/_Project/Scripts/World/Graph/Mutation/`
- `Assets/_Project/Scripts/UI/Debug/`
- `Assets/_Project/Scenes/Test/`
- `Assets/_Project/Data/Match/Networking/`

## Suggested namespaces
- `Desync.Match.Networking`
- `Desync.World.Graph.Networking`
- `Desync.World.Graph.Observation`
- `Desync.World.Graph.Mutation`
- `Desync.UI.Debug`

## Milestone framing
This sprint breaks into two milestones.

### CO-1
Observation reporting and shared protection ledger.

### CO-2
Mutation authority gate, reconciliation, and two-player graybox validation.

## Core runtime model
The sprint should formalize explicit runtime records instead of ad hoc booleans or direct scene references.

### Required runtime-side types
- `ObservationContributionRecord`
- `ObservationProtectionLedger`
- `PlayerNodeContextState`
- `CoopVisibilityContext`
- `MutationAuthorityDecision`
- `PortalAuthorityState`
- `NetworkHouseSnapshot`
- `ClientHouseReconciliationState`

## Suggested definition-side assets
Keep tunable networking/lock behavior in data where reasonable, consistent with the project’s data-driven philosophy [file:12][file:13][file:14].

### Suggested assets
- `CoopObservationPolicyDefinition`
- `ObservationGraceWindowProfile`
- `MutationGatePolicyDefinition`
- `PortalAuthorityPolicyDefinition`
- `ClientReconciliationProfile`

## Suggested core classes

| Class | Responsibility |
|---|---|
| `HouseAuthorityService` | owns authoritative graph/mutation state in co-op |
| `PlayerObservationReporter` | builds local observation contribution data |
| `ObservationLedgerService` | aggregates valid player protections |
| `CoopMutationGate` | evaluates mutation legality under shared protections |
| `PortalAuthorityService` | exposes shared portal binding truth |
| `ClientRenderReconciler` | reconciles local presentation to shared house truth |
| `CoopDebugOverlay` | visualizes per-player and aggregated protection state |
| `ThresholdProtectionCoordinator` | raises temporary protection during doorway crossing |

This keeps the co-op system modular and consistent with earlier ownership rules [file:12][file:13][file:15].

## Suggested interfaces
The exact signatures can evolve, but the sprint should lock down the narrow contracts that Claude and future sprints can rely on [file:13][file:14].

```csharp
public interface IPlayerObservationReporter
{
    ObservationContributionRecord BuildContribution(PlayerContext context);
}

public interface IObservationLedgerService
{
    bool IsNodeProtected(string nodeId);
    bool IsPortalProtected(string portalId);
    ObservationProtectionSnapshot BuildSnapshot();
}

public interface ICoopMutationGate
{
    MutationAuthorityDecision Evaluate(MutationCandidate candidate);
}
```

## Suggested first implementation slice
Keep the first co-op slice very small.

### Prototype policy
- 2 players only,
- host-authoritative simulation,
- one house graph seed,
- one mutation family,
- one portal class,
- one shared artifact objective,
- debug overlay showing both players’ observation contributions and aggregated protections.

This follows the broader milestone advice to prove one clear question at a time instead of scaling prematurely [file:12][file:13][file:15].

## CO-1 overview
CO-1 proves that observation contributions from two players can produce stable shared protection truth.

### CO-1 answers
Can the host/server collect observation-relevant player state and aggregate it into a multiplayer-safe protection ledger?

## CO-1 tasks

### CO1-T1 — Create co-op runtime state types
Implement:
- `ObservationContributionRecord`
- `ObservationProtectionLedger`
- `PlayerNodeContextState`
- `CoopVisibilityContext`
- `ObservationProtectionSnapshot`

#### Acceptance criteria
- Runtime types compile and cleanly represent per-player contributions and aggregated protection state.
- Types remain separate from policy assets and scene-authored objects [file:12][file:13][file:14].

### CO1-T2 — Create co-op policy assets and enums
Implement:
- `CoopObservationPolicyDefinition`
- `ObservationGraceWindowProfile`
- `ProtectedReasonType` enum
- `ObservationContributionType` enum

#### Acceptance criteria
- grace windows, occupancy protection, and portal protection behavior are tunable in data rather than buried in scene-specific code [file:12][file:13][file:14].

### CO1-T3 — Implement `PlayerObservationReporter`
Build the client-side reporter that packages local facts for authority.

#### Responsibilities
- read player transform and current node,
- identify observed/protected candidates,
- identify active threshold crossing state,
- produce a contribution record.

#### Acceptance criteria
- reporter sends local facts, not authoritative mutation decisions.
- contribution generation can run independently for each player.

### CO1-T4 — Implement `ObservationLedgerService`
Build the host/server service that aggregates contributions.

#### Responsibilities
- validate incoming contribution records,
- maintain per-player active protections,
- expose aggregated protected nodes/portals,
- apply short grace-window release rules.

#### Acceptance criteria
- any player’s valid contribution can protect a node or portal.
- release behavior is stable enough to avoid flicker from transient update gaps.

### CO1-T5 — Implement `ThresholdProtectionCoordinator`
Add explicit protection support for players actively crossing portals.

#### Responsibilities
- mark protected portal and related node context during crossing,
- clear protection on commit or timeout,
- expose crossing-protection reason in debug.

#### Acceptance criteria
- doorway crossing creates a temporary shared mutation-safe window.
- protection does not persist indefinitely after crossing ends.

### CO1-T6 — Build co-op protection debug overlay
Show:
- each player’s current node,
- each player’s active observation contributions,
- aggregated protected nodes,
- aggregated protected portals,
- grace-window countdowns,
- threshold protection state.

#### Acceptance criteria
- a developer can understand why a region is currently protected.
- per-player and aggregated views can be toggled independently.

### CO1-T7 — Add logic tests for protection aggregation
Create tests for:
- union-based node protection,
- portal protection from any player,
- grace-window release behavior,
- threshold protection creation and cleanup.

#### Acceptance criteria
- tests validate the ledger without requiring a full gameplay run.
- common aggregation mistakes fail loudly.

## CO-1 completion definition
CO-1 is complete when two players can contribute observation state and the authority can aggregate it into stable node/portal protection truth visible in debug [file:12][file:13][file:15].

## CO-2 overview
CO-2 proves that the multiplayer authority can safely gate mutation and reconcile clients back to one house truth.

### CO-2 answers
Can the game reject illegal mutations under shared observation, commit legal ones under authority, and keep both players synchronized on the resulting house truth?

## CO-2 tasks

### CO2-T1 — Create authority and reconciliation runtime types
Implement:
- `MutationAuthorityDecision`
- `PortalAuthorityState`
- `NetworkHouseSnapshot`
- `ClientHouseReconciliationState`
- `HouseTopologyVersion`

#### Acceptance criteria
- runtime models can represent legality decisions, authoritative versions, and client reconciliation state.

### CO2-T2 — Create authority policy assets
Implement:
- `MutationGatePolicyDefinition`
- `PortalAuthorityPolicyDefinition`
- `ClientReconciliationProfile`

#### Acceptance criteria
- mutation rejection rules, version handling, and reconciliation tolerances are tunable [file:12][file:13][file:14].

### CO2-T3 — Implement `HouseAuthorityService`
Build the authority owner for shared house truth.

#### Responsibilities
- own the authoritative graph instance,
- own mutation commit sequence,
- publish current shared topology snapshot,
- coordinate with observation ledger and portal authority.

#### Acceptance criteria
- one authority source owns current house truth.
- clients cannot directly commit topology changes.

### CO2-T4 — Implement `PortalAuthorityService`
Build the service that exposes shared portal binding truth.

#### Responsibilities
- store authoritative portal destinations,
- version portal binding changes,
- expose authoritative snapshot to clients.

#### Acceptance criteria
- clients can differ in local presentation, but not in portal destination truth.

### CO2-T5 — Implement `CoopMutationGate`
Build the mutation legality gate for co-op.

#### Responsibilities
- receive mutation candidate,
- query aggregated protection state,
- reject or allow based on affected protected sets,
- return detailed rejection reason.

#### Acceptance criteria
- a mutation touching protected nodes/portals is rejected.
- a legal mutation returns a structured approval decision.

### CO2-T6 — Implement `ClientRenderReconciler`
Build the client-side reconciliation path back to shared truth.

#### Responsibilities
- consume authoritative house snapshot/version,
- detect stale local portal/graph state,
- trigger local presentation refresh or correction,
- expose reconciliation details in debug.

#### Acceptance criteria
- clients recover cleanly from stale house versions.
- reconciliation does not invent alternate shared truth.

### CO2-T7 — Integrate mutation commit broadcast path
Wire the authority pipeline:
- client observations update ledger,
- mutation request evaluated by gate,
- legal mutation committed by authority,
- authoritative snapshot/version broadcast to clients,
- clients reconcile local view.

#### Acceptance criteria
- end-to-end authority loop works in a two-player graybox test.
- mutation broadcasts contain enough information for deterministic reconciliation.

### CO2-T8 — Build mutation-decision debug tools
Show:
- last mutation candidate,
- whether it was allowed or rejected,
- affected nodes/portals,
- rejection reason,
- current topology version,
- portal version state,
- reconciliation status per client.

#### Acceptance criteria
- a developer can explain every mutation allow/reject result during co-op testing.

### CO2-T9 — Add two-player graybox workflows
Create reproducible scenarios for:
- player A watching a hall while player B creates mutation opportunity,
- player A crossing a portal while player B changes local visibility elsewhere,
- both players looking away so mutation may fire,
- temporary observation packet loss simulation,
- stale client reconciliation test.

#### Acceptance criteria
- scenarios can be reproduced without full content progression.
- outcomes are inspectable in debug.

### CO2-T10 — Add regression tests for contradiction cases
Cover:
- mutation while other player still observes,
- divergent portal truth blocked,
- threshold crossing protection blocks conflicting mutation,
- brief contribution drop does not immediately invalidate protected region,
- stale client can reconcile to current authority version.

#### Acceptance criteria
- highest-priority contradiction classes have targeted regression coverage.

## CO-2 completion definition
CO-2 is complete when the host/server can gate mutations using aggregated co-op observation truth, commit legal changes under authority, version shared topology state, and keep two players synchronized on one house truth with useful debug visibility [file:12][file:13][file:15].

## Suggested ownership map
To keep implementation sane, each system should stay narrow.

| Area | Owns | Does not own |
|---|---|---|
| Player observation reporter | local contribution facts | authoritative mutation truth |
| Observation ledger service | shared protections | portal visual presentation |
| Threshold protection coordinator | crossing-based protection | graph mutation rules |
| House authority service | authoritative house state | local camera smoothing |
| Coop mutation gate | legality decisions | client interpolation |
| Portal authority service | shared portal destination truth | threshold VFX |
| Client render reconciler | syncing local presentation to authority | topology ownership |
| Coop debug overlay | hidden multiplayer state visibility | gameplay decisions |

## Suggested Claude Code workflow
This sprint should be implemented with very small, explicit tasks. The earlier docs repeatedly show that AI implementation works best when ownership, acceptance criteria, and debug outputs are explicit rather than implied [file:12][file:13][file:15].

### Good prompt shape
- 1–3 classes at a time,
- exact folders/namespaces,
- required fields/methods,
- unit or logic tests,
- expected debug outputs.

### Bad prompt shape
- “Add co-op.”
- “Make multiplayer observation work.”
- “Fix sync issues.”

## Suggested Claude task order
1. co-op runtime state models,
2. co-op policy assets/enums,
3. player observation reporter,
4. observation ledger service,
5. threshold protection coordinator,
6. co-op debug overlay,
7. aggregation logic tests,
8. authority and reconciliation runtime types,
9. authority policy assets,
10. house authority service,
11. portal authority service,
12. co-op mutation gate,
13. client render reconciler,
14. end-to-end authority broadcast path,
15. mutation-decision debug tools,
16. two-player graybox workflows,
17. contradiction regression tests.

## Testing strategy
Use three testing layers.

### 1. Logic tests
Verify aggregation, grace windows, mutation-gate legality, and version reconciliation.

### 2. Two-player graybox tests
Run controlled host/client scenarios in the prototype house.

### 3. Resilience tests
Intentionally simulate stale contributions, short packet drops, and delayed updates to confirm that protection rules remain fair enough for play.

## Risks and mitigations

### Risk 1
Protection rules become so conservative that the house almost never mutates in co-op.

### Mitigation
Keep protected sets tight, reasons explicit, and test mutation frequency under real two-player movement.

### Risk 2
Authority is correct, but client presentation lags enough to look broken.

### Mitigation
Version snapshots explicitly and build reconciliation/debug tools early.

### Risk 3
Threshold crossing remains the biggest contradiction source.

### Mitigation
Treat crossing as a first-class protection system, not a derived afterthought.

### Risk 4
The sprint sprawls into full multiplayer production work.

### Mitigation
Keep the sprint strictly two-player and focused on house-truth safety, not full co-op polish.

## Acceptance checklist
This sprint is successful when:
- two players can contribute observation data to a shared host/server authority [file:12][file:13],
- any valid player observation can protect affected nodes and portals,
- threshold crossing creates temporary mutation-safe protection,
- shared mutation approval uses aggregated protection truth rather than client-local guesses,
- portal destination truth remains authority-owned and versioned,
- clients reconcile to one shared house state after legal mutation,
- and developers can inspect per-player contributions, aggregated protections, mutation decisions, and reconciliation state in debug during two-player tests [file:12][file:13][file:14][file:15].

## Recommended next document
After this sprint doc, the strongest next step is **Networked House Runtime Interfaces / Contracts Doc**, because the authority, ledger, and reconciliation work now need a precise contract layer that Claude can implement against without smearing networking assumptions across graph, portal, mutation, and render systems.
