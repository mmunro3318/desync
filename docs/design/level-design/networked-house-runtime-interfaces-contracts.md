# Networked House Runtime Interfaces / Contracts Doc

## Document objective
Define the contract layer for the networked impossible-house runtime. This document specifies the interfaces, runtime data shapes, ownership boundaries, event contracts, and versioning expectations that should connect graph, portal, observation, mutation, authority, and reconciliation systems without smearing networking assumptions across the entire codebase [file:12][file:13][file:14][file:15].

## Why this document exists
The project now has rules and sprint docs for graph truth, mutation, portal visibility, graybox vertical slice integration, co-op observation semantics, and co-op implementation work. The next risk is not missing ideas. The next risk is interface drift. If graph, portal, observation, mutation, authority, and render systems start reaching into each other with ad hoc direct references, the codebase will become hard to reason about and Claude will tend to generate implementation glue in the wrong layers [file:12][file:13][file:14][file:15].

## Core thesis
The networked impossible house needs a **stable contract layer**: authoritative systems own truth, local systems report facts or present views, and all cross-system communication should happen through narrow interfaces and explicit runtime payloads rather than scene-specific assumptions or giant manager objects [file:12][file:13][file:15].

## Design goals
This document should optimize for:
- narrow ownership,
- runtime-versus-definition separation,
- stable authority boundaries,
- testable contracts,
- and AI-friendly implementation scaffolding [file:12][file:13][file:14][file:15].

## Questions this document answers
- What services should exist as contract boundaries?
- What runtime data needs explicit shape definitions?
- Which system owns what and which systems may only query or report?
- How should mutation, observation, portal, and authority systems communicate?
- What events and snapshots should be considered public contracts?

## Out of scope
This document does **not** define:
- the exact networking library/package,
- transport-layer APIs,
- final serialization format,
- low-level RPC naming,
- full item or creature networking,
- or final production optimization.

It defines the conceptual and code-level contract surface that those later implementation choices should respect.

## Architectural principles carried forward
This doc intentionally extends the earlier architecture rules rather than replacing them [file:12][file:13][file:14][file:15].

- Runtime state stays separate from definition assets [file:12][file:13][file:14].
- Scene objects stay thin and do not own simulation truth [file:12][file:13][file:14].
- Hidden systems expose inspectable state through debug contracts [file:12][file:13][file:15].
- Systems should communicate through focused interfaces and payloads, not deep knowledge of each other’s private state [file:12][file:13][file:14].

## Contract philosophy
Think in three layers.

### 1. Definition layer
Design-time assets and authoring data.

### 2. Runtime domain layer
Authoritative match/house state and domain decisions.

### 3. Presentation/network edge layer
Client reports, snapshots, reconciliation, and local view handling.

The mistakes to avoid are:
- presentation objects owning authority truth,
- client reporters deciding mutation legality,
- graph services directly depending on local renderer internals,
- or mutation code reaching directly into portal VFX state.

## Contract categories
The networked house should expose a small set of contract categories.

### Core categories
- graph contracts,
- portal contracts,
- observation contracts,
- mutation contracts,
- authority contracts,
- reconciliation contracts,
- debug contracts.

## Ownership model
A stable ownership map is more important than perfect type names.

| Area | Primary owner | Secondary consumers |
|---|---|---|
| House graph truth | `IHouseGraphRuntime` | mutation, portal, observation, authority |
| Portal destination truth | `IPortalAuthorityService` | portal render, visibility, reconciliation |
| Observation protection truth | `IObservationLedgerService` | mutation gate, debug, authority |
| Mutation legality and commit | `IMutationAuthorityService` | debug, reconciliation, history |
| Shared house snapshot/versioning | `IHouseAuthorityService` | clients, debug, reconciliation |
| Local presentation and streaming | `ILocalHousePresentationService` | portal render, client reconciler |

This follows the project’s recurring theme that narrow roles beat giant mixed-purpose managers [file:12][file:13][file:15].

## Required contract families

## 1. Graph contracts
The graph contract layer should represent authoritative topological truth without depending on scene objects.

### Responsibilities
- query nodes, edges, portals,
- expose current topology version,
- expose current graph bindings,
- provide mutation-safe read access.

### Suggested interfaces
```csharp
public interface IHouseGraphRuntime
{
    string CurrentTopologyVersion { get; }
    bool TryGetNode(string nodeId, out HouseNodeRuntime node);
    bool TryGetPortal(string portalId, out PortalRuntimeState portal);
    IReadOnlyList<string> GetNeighborNodeIds(string nodeId);
}

public interface IHouseGraphQueryService
{
    bool IsNodeReachable(string fromNodeId, string toNodeId);
    IReadOnlyList<string> GetAffectedNodeIds(MutationCandidate candidate);
}
```

### Notes
- `IHouseGraphRuntime` is truth-facing.
- `IHouseGraphQueryService` is convenience/query-facing.
- Neither should depend on Unity scene hierarchy traversal to determine topology truth.

## 2. Portal contracts
Portal contracts should separate authoritative binding truth from local rendering behavior.

### Responsibilities
- expose portal destination truth,
- expose portal version state,
- expose whether portal is protected/transitioning,
- allow local systems to ask what a portal currently means.

### Suggested interfaces
```csharp
public interface IPortalAuthorityService
{
    bool TryGetPortalAuthorityState(string portalId, out PortalAuthorityState state);
    string CurrentPortalVersion { get; }
}

public interface IPortalVisibilityResolver
{
    PortalVisibilityState Resolve(string portalId, Camera activeCamera, IHouseGraphRuntime graph);
}
```

### Rule
`IPortalVisibilityResolver` may interpret camera and local view state, but it may not invent authoritative portal destinations.

## 3. Observation contracts
Observation contracts should formalize how player-local facts become shared protection truth.

### Responsibilities
- build player observation contributions,
- aggregate contributions into a shared ledger,
- answer whether nodes/portals are protected,
- expose why protection exists.

### Suggested interfaces
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
```

### Rule
Reporters report **facts**. Ledger services own **shared truth**.

## 4. Mutation contracts
Mutation contracts should make legality and commit explicit rather than implicit.

### Responsibilities
- represent mutation requests/candidates,
- evaluate legality,
- commit approved mutation,
- publish mutation result/history.

### Suggested interfaces
```csharp
public interface IMutationAuthorityService
{
    MutationAuthorityDecision Evaluate(MutationCandidate candidate);
    MutationCommitResult Commit(MutationCandidate candidate);
}

public interface IMutationHistoryService
{
    IReadOnlyList<MutationRecord> GetRecentMutations();
    MutationRecord? GetLastMutation();
}
```

### Rule
No system outside mutation authority should silently mutate topology.

## 5. Authority and snapshot contracts
These contracts define the shared house truth visible to clients and debug systems.

### Responsibilities
- expose current shared house snapshot,
- version the shared topology state,
- provide public read model for clients,
- expose reconciliation-friendly sequence values.

### Suggested interfaces
```csharp
public interface IHouseAuthorityService
{
    NetworkHouseSnapshot BuildSnapshot();
    string CurrentHouseVersion { get; }
}

public interface IHouseSnapshotPublisher
{
    void Publish(NetworkHouseSnapshot snapshot);
}
```

### Rule
The authoritative house snapshot is a public runtime contract, not a private bundle hidden inside one manager.

## 6. Reconciliation contracts
Reconciliation contracts should let clients converge on shared truth without owning it.

### Responsibilities
- consume authoritative snapshot/version,
- detect stale local state,
- request local corrections,
- expose reconciliation status.

### Suggested interfaces
```csharp
public interface IClientHouseReconciler
{
    ClientHouseReconciliationResult Reconcile(NetworkHouseSnapshot snapshot);
}

public interface ILocalHousePresentationService
{
    void ApplyTopologySnapshot(NetworkHouseSnapshot snapshot);
    void RefreshPortalPresentation(string portalId);
}
```

### Rule
Reconciliation may update local presentation state, but it may not reinterpret shared authority truth.

## 7. Debug contracts
Debug should be first-class, not an afterthought, consistent with the broader project guidance [file:12][file:13][file:15].

### Responsibilities
- expose per-system snapshots,
- provide reasons for decisions,
- support graybox inspection.

### Suggested interfaces
```csharp
public interface IHouseDebugSnapshotProvider
{
    HouseDebugSnapshot BuildDebugSnapshot();
}

public interface IMutationDecisionTraceProvider
{
    MutationDecisionTrace GetLastDecisionTrace();
}
```

## Shared runtime payloads
The contract layer should define explicit payloads rather than loosely structured dictionaries or scene references.

### Required payloads
- `HouseNodeRuntime`
- `PortalRuntimeState`
- `PortalAuthorityState`
- `ObservationContributionRecord`
- `ObservationProtectionSnapshot`
- `MutationCandidate`
- `MutationAuthorityDecision`
- `MutationCommitResult`
- `MutationRecord`
- `NetworkHouseSnapshot`
- `ClientHouseReconciliationResult`
- `HouseDebugSnapshot`
- `MutationDecisionTrace`

## Payload design rules
Payloads should follow a few non-negotiable rules.

### Rule 1
Use stable ids rather than scene references whenever data crosses contract boundaries.

### Rule 2
Payloads should clearly distinguish authoritative fields from client-local or derived fields.

### Rule 3
Contract payloads should be serializable in principle, even if final networking serialization is decided later.

### Rule 4
Payloads should include version or sequence metadata whenever stale-state reconciliation matters.

## Suggested payload examples

### `ObservationContributionRecord`
Suggested fields:
- `playerId`
- `currentNodeId`
- `protectedNodeIds`
- `protectedPortalIds`
- `contributionTypes`
- `isThresholdCrossing`
- `timestamp`
- `localSequence`

### `PortalAuthorityState`
Suggested fields:
- `portalId`
- `destinationNodeId`
- `portalVersion`
- `isProtected`
- `isTransitionLocked`

### `MutationAuthorityDecision`
Suggested fields:
- `candidateId`
- `isApproved`
- `rejectionReason`
- `affectedNodeIds`
- `affectedPortalIds`
- `topologyVersionBefore`
- `topologyVersionAfter` (optional until commit)

### `NetworkHouseSnapshot`
Suggested fields:
- `houseVersion`
- `topologyVersion`
- `portalVersion`
- `protectedNodeIds`
- `protectedPortalIds`
- `recentMutationIds`
- `artifactState`
- `exitState`

## Event contracts
The networked house should also expose a small set of high-value domain events.

### Recommended domain events
- `OnHouseSnapshotPublished`
- `OnMutationEvaluated`
- `OnMutationCommitted`
- `OnPortalAuthorityChanged`
- `OnObservationLedgerUpdated`
- `OnClientReconciled`
- `OnThresholdProtectionChanged`

### Event rules
- events announce results, they do not replace authoritative queries,
- events should carry ids and versions,
- listeners should never infer missing truth from an event alone.

## Query versus command separation
Keep commands and queries distinct where possible.

### Commands
- submit observation contribution,
- evaluate mutation candidate,
- commit mutation,
- publish snapshot,
- request reconciliation.

### Queries
- get current topology version,
- ask whether node/portal is protected,
- get current portal authority state,
- get current house snapshot,
- get last mutation trace.

This helps prevent services from becoming opaque “do everything” objects.

## Scene object contract rules
Scene objects must stay thin, which is a recurring core rule in the earlier docs [file:12][file:13][file:14].

### Scene objects may own
- local trigger hooks,
- references to stable ids,
- presentation transitions,
- interaction forwarding.

### Scene objects may not own
- graph truth,
- mutation legality,
- observation-ledger aggregation,
- shared portal authority,
- reconciliation authority.

## Suggested namespace map
To keep Claude and future contributors sane, the contract layer should have predictable namespaces built on your existing structure [file:13][file:14].

### Suggested namespaces
- `GhostHunt.World.Graph.Contracts`
- `GhostHunt.World.Graph.Runtime`
- `GhostHunt.World.Graph.Observation`
- `GhostHunt.World.Graph.Mutation`
- `GhostHunt.World.Graph.Portal`
- `GhostHunt.Match.Networking.Contracts`
- `GhostHunt.UI.Debug.Contracts`

## Suggested folder map
A boring structure is a good structure here, consistent with the earlier folder guidance [file:13][file:14].

### Suggested folders
- `Assets/_Project/Scripts/World/Graph/Contracts/`
- `Assets/_Project/Scripts/World/Graph/Runtime/`
- `Assets/_Project/Scripts/World/Graph/Observation/`
- `Assets/_Project/Scripts/World/Graph/Mutation/`
- `Assets/_Project/Scripts/World/Graph/Portal/`
- `Assets/_Project/Scripts/Match/Networking/Contracts/`
- `Assets/_Project/Scripts/UI/Debug/Contracts/`

## Contract dependency rules
This is the part most likely to save you pain later.

### Allowed dependency direction
- definition assets -> runtime consumers,
- runtime domain services -> contracts,
- local presentation systems -> authority/query contracts,
- debug systems -> public runtime/debug contracts.

### Disallowed dependency direction
- graph runtime directly depending on local renderers,
- mutation services depending on portal VFX classes,
- observation reporters deciding authoritative legality,
- debug overlay reaching into private fields instead of using debug contracts,
- scene door scripts deciding shared topology truth.

## Suggested implementation pattern
For each major system, prefer this three-part shape:
- contract interface,
- runtime implementation,
- debug snapshot provider.

That gives Claude a predictable pattern to follow and makes future refactors far safer [file:12][file:13][file:15].

## AI-assisted development guidance
This doc should be used as the source of truth when asking Claude to scaffold networked house code. The best prompts should reference exact interfaces, payloads, and ownership rules from this document so the generated code plugs into a stable contract layer instead of inventing its own assumptions [file:12][file:13][file:15].

### Good prompt examples
- implement `IObservationLedgerService` using the contract payloads in the contracts doc,
- create serializable runtime records for `NetworkHouseSnapshot` and `MutationAuthorityDecision`,
- add a debug snapshot provider for the mutation authority service.

### Bad prompt examples
- make the networked house system,
- wire up all multiplayer mutation logic,
- fix desync everywhere.

## Testing implications
Stable contracts make it easier to test logic without full scene boot.

### Logic tests should target
- payload correctness,
- version propagation,
- mutation decision outputs,
- observation aggregation queries,
- reconciliation results.

### Graybox tests should target
- service integration,
- event ordering,
- visible reconciliation after mutation.

## Debug requirements
The contract layer is not complete unless debug systems can consume public state from it, which is consistent with the project’s debug-first philosophy [file:12][file:13][file:15].

### Minimum debug-read models
- current house version,
- topology version,
- portal version,
- aggregated protected sets,
- last mutation decision,
- last mutation commit,
- per-player contribution summary,
- reconciliation state per client.

## Acceptance checklist
This document is successfully realized in code when:
- graph, portal, observation, mutation, authority, and reconciliation systems communicate through explicit contracts rather than scene-coupled assumptions [file:12][file:13][file:14],
- ownership boundaries are narrow and visible,
- runtime payloads use stable ids and version data,
- local presentation systems can consume authority truth without owning it,
- debug systems can inspect major hidden-state decisions through public debug contracts,
- and Claude can implement individual services against the shared contract layer without inventing conflicting shapes for the same domain concepts [file:12][file:13][file:15].

## Recommended next document
After this contracts doc, the strongest next step is **Claude Code Task Pack / Implementation Prompts for Networked House Runtime**, because the architecture stack is now deep enough that the best leverage comes from converting these contracts and sprint docs into carefully scoped implementation prompts with acceptance checks and expected file targets.

