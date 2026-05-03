# Claude Code Task Pack / Implementation Prompts for Networked House Runtime

## Document objective
Provide a practical task pack for Claude Code that turns the networked-house architecture, co-op sprint plan, and contract layer into small, implementation-ready prompts. The goal is to reduce ambiguity, preserve ownership boundaries, and make every coding step testable and inspectable [file:12][file:13][file:14][file:15].

## Why this document exists
The architecture stack is now strong enough that the main implementation risk is not lack of ideas. The real risk is giving Claude prompts that are too broad, too magical, or too under-specified. Earlier project docs repeatedly show that AI-assisted Unity work goes better when tasks define what exists, what owns what, what files to touch, and what done looks like [file:12][file:13][file:14][file:15].

## Core use of this task pack
Use this document as the handoff layer between design docs and actual Unity implementation. Each task here is intentionally small enough to fit Claude Code’s strengths: scaffolding classes, following naming and namespace rules, implementing one bounded service at a time, and adding debug/test support alongside hidden-state runtime logic [file:12][file:13][file:15].

## Prompting rules for Claude Code
These rules should be attached to almost every implementation prompt.

### Always include
- exact file path targets,
- exact namespaces,
- class/interface names,
- ownership boundaries,
- acceptance criteria,
- test expectations,
- and debug visibility requirements [file:13][file:14].

### Never ask Claude to
- “add multiplayer,”
- “make co-op observation work,”
- “build the whole house runtime,”
- or “fix desync” without a bounded scope [file:12][file:13][file:15].

## Global implementation constraints
Every task in this pack assumes the following project rules [file:12][file:13][file:14][file:15].

- Runtime state is separate from definition assets [file:12][file:13][file:14].
- Scene objects stay thin and do not own simulation truth [file:12][file:13][file:14].
- Hidden systems must expose debug data [file:12][file:13][file:15].
- New capability should mostly mean new data, contracts, or bounded runtime services rather than giant rewrites [file:12][file:15].
- Interfaces and payloads from the contracts doc should be treated as the source of truth for cross-system communication [file:13][file:14].

## Recommended folder targets
These prompts assume a predictable structure based on the existing project layout guidance [file:13][file:14].

- `Assets/_Project/Scripts/World/Graph/Contracts/`
- `Assets/_Project/Scripts/World/Graph/Runtime/`
- `Assets/_Project/Scripts/World/Graph/Observation/`
- `Assets/_Project/Scripts/World/Graph/Mutation/`
- `Assets/_Project/Scripts/World/Graph/Portal/`
- `Assets/_Project/Scripts/Match/Networking/Contracts/`
- `Assets/_Project/Scripts/Match/Networking/Runtime/`
- `Assets/_Project/Scripts/UI/Debug/`
- `Assets/_Project/Tests/EditMode/`
- `Assets/_Project/Tests/PlayMode/`

## Recommended namespace baseline
- `GhostHunt.World.Graph.Contracts`
- `GhostHunt.World.Graph.Runtime`
- `GhostHunt.World.Graph.Observation`
- `GhostHunt.World.Graph.Mutation`
- `GhostHunt.World.Graph.Portal`
- `GhostHunt.Match.Networking.Contracts`
- `GhostHunt.Match.Networking.Runtime`
- `GhostHunt.UI.Debug`

## How to use each task
Each task below includes:
- purpose,
- prompt text you can paste into Claude Code,
- expected outputs,
- acceptance checks,
- and notes on what not to let Claude do.

## Task order
These tasks are sequenced to match the project’s broader milestone philosophy: prove one clean layer at a time instead of asking for a giant system blob [file:12][file:15].

| Order | Task | Purpose |
|---|---|---|
| 1 | Contract payload scaffolds | Define shared runtime shapes |
| 2 | Contract interfaces | Lock boundary surfaces |
| 3 | Graph runtime read model | Provide authoritative topology queries |
| 4 | Observation reporter | Generate player-local observation facts |
| 5 | Observation ledger | Aggregate shared protections |
| 6 | Threshold protection coordinator | Handle portal crossing safety |
| 7 | Mutation gate | Evaluate legality under protection rules |
| 8 | Portal authority service | Expose shared portal truth |
| 9 | House authority service | Publish authoritative snapshots |
| 10 | Client reconciler | Align local presentation to authority |
| 11 | Debug snapshot providers | Make all hidden state inspectable |
| 12 | Graybox integration pass | Wire minimal two-player runtime flow |
| 13 | Edit mode tests | Validate logic correctness |
| 14 | Play mode tests | Validate graybox behavior |

## Task 1 — Contract payload scaffolds

### Purpose
Create the serializable runtime records used across graph, portal, observation, mutation, authority, and reconciliation.

### Claude prompt
```text
Create serializable runtime payload classes/records for the networked house runtime.

Target folder:
Assets/_Project/Scripts/Match/Networking/Contracts/

Namespaces:
GhostHunt.Match.Networking.Contracts
GhostHunt.World.Graph.Contracts

Create only data shapes, no business logic.

Required types:
- HouseNodeRuntime
- PortalRuntimeState
- PortalAuthorityState
- ObservationContributionRecord
- ObservationProtectionSnapshot
- MutationCandidate
- MutationAuthorityDecision
- MutationCommitResult
- MutationRecord
- NetworkHouseSnapshot
- ClientHouseReconciliationResult
- HouseDebugSnapshot
- MutationDecisionTrace

Requirements:
- use stable string ids, not scene references,
- include version/sequence fields where stale state matters,
- mark types serializable where appropriate,
- keep fields explicit and readable,
- add XML summary comments,
- do not add networking-library-specific code,
- do not create services yet.

Also add one small helper enum file for rejection/protection reason types if needed.
```

### Acceptance checks
- types compile,
- no service logic is included,
- ids and version fields exist where needed,
- no scene-object dependencies appear.

### Watch-outs
Claude may try to over-engineer with generics or add transport-specific annotations. Reject that.

## Task 2 — Contract interfaces

### Purpose
Define the public interfaces that separate authority, query, observation, mutation, portal, reconciliation, and debug responsibilities.

### Claude prompt
```text
Implement the public interfaces for the networked house runtime contract layer.

Target folders:
Assets/_Project/Scripts/World/Graph/Contracts/
Assets/_Project/Scripts/Match/Networking/Contracts/
Assets/_Project/Scripts/UI/Debug/

Namespaces:
GhostHunt.World.Graph.Contracts
GhostHunt.Match.Networking.Contracts
GhostHunt.UI.Debug

Create interfaces only, no runtime implementations.

Required interfaces:
- IHouseGraphRuntime
- IHouseGraphQueryService
- IPortalAuthorityService
- IPortalVisibilityResolver
- IPlayerObservationReporter
- IObservationLedgerService
- IMutationAuthorityService
- IMutationHistoryService
- IHouseAuthorityService
- IHouseSnapshotPublisher
- IClientHouseReconciler
- ILocalHousePresentationService
- IHouseDebugSnapshotProvider
- IMutationDecisionTraceProvider

Rules:
- keep methods narrow,
- prefer query/command clarity,
- use payloads from the contracts package,
- do not add concrete manager implementations,
- add XML docs describing ownership boundaries.
```

### Acceptance checks
- interfaces compile against payload types,
- no Unity scene traversal or concrete service references appear,
- ownership boundaries are clearly documented.

## Task 3 — Graph runtime read model

### Purpose
Create the first concrete authoritative graph read service for nodes, portals, neighbors, and topology version.

### Claude prompt
```text
Implement a first-pass authoritative graph runtime read model.

Target folder:
Assets/_Project/Scripts/World/Graph/Runtime/

Namespace:
GhostHunt.World.Graph.Runtime

Implement:
- HouseGraphRuntime : IHouseGraphRuntime
- HouseGraphQueryService : IHouseGraphQueryService

Requirements:
- read-only runtime graph access only,
- support node lookup, portal lookup, neighbor lookup,
- expose current topology version,
- use in-memory collections keyed by stable ids,
- no scene hierarchy traversal as the source of truth,
- no mutation commit logic yet,
- add constructor/setup methods suitable for later test seeding.

Also add minimal edit mode tests covering:
- valid node lookup,
- valid portal lookup,
- neighbor query,
- topology version exposure.
```

### Acceptance checks
- can seed a tiny graph in tests,
- graph queries work without booting a scene,
- no mutation logic leaks into graph read classes.

## Task 4 — Observation reporter

### Purpose
Build the player-local service that reports observation-relevant facts to authority.

### Claude prompt
```text
Implement PlayerObservationReporter for the networked house runtime.

Target folder:
Assets/_Project/Scripts/World/Graph/Observation/

Namespace:
GhostHunt.World.Graph.Observation

Implement:
- PlayerObservationReporter : IPlayerObservationReporter

Requirements:
- produce ObservationContributionRecord from a provided PlayerContext,
- include current node id,
- include protected node ids and protected portal ids,
- include threshold-crossing flag,
- do not decide authoritative legality,
- do not reference networking transport APIs,
- keep field derivation methods small and testable.

Also create a very small PlayerContext runtime model if needed, in the same namespace.

Add edit mode tests for:
- contribution with one protected node,
- contribution with portal protection,
- threshold crossing contribution,
- empty contribution when no valid context exists.
```

### Acceptance checks
- reporter emits facts only,
- no authority logic included,
- test coverage exists for common input shapes.

## Task 5 — Observation ledger

### Purpose
Aggregate contributions from multiple players into one shared protection ledger.

### Claude prompt
```text
Implement ObservationLedgerService for shared protection aggregation.

Target folder:
Assets/_Project/Scripts/World/Graph/Observation/

Namespace:
GhostHunt.World.Graph.Observation

Implement:
- ObservationLedgerService : IObservationLedgerService

Requirements:
- ingest multiple ObservationContributionRecord entries,
- aggregate node protection by union,
- aggregate portal protection by union,
- expose ObservationProtectionSnapshot,
- support contribution replacement by player id,
- support simple grace-window release hooks but keep policy inputs injectable,
- do not commit mutations,
- do not depend on scene objects.

Add edit mode tests for:
- union protection from two players,
- replacing a player contribution,
- portal protection aggregation,
- grace-window retention behavior.
```

### Acceptance checks
- two players can protect the same or different regions,
- latest player contribution replaces old contribution cleanly,
- snapshot results are inspectable in tests.

## Task 6 — Threshold protection coordinator

### Purpose
Handle temporary mutation-safe windows during doorway/portal crossing.

### Claude prompt
```text
Implement ThresholdProtectionCoordinator.

Target folder:
Assets/_Project/Scripts/World/Graph/Observation/

Namespace:
GhostHunt.World.Graph.Observation

Implement:
- ThresholdProtectionCoordinator

Requirements:
- create temporary protection for active portal crossing,
- track start time and expiry,
- expose protected portal/node ids for the crossing window,
- integrate cleanly with ObservationLedgerService without owning the ledger,
- keep policy durations injectable,
- add small public query methods for debug.

Add edit mode tests for:
- crossing creates temporary protection,
- protection expires,
- repeated crossing refreshes protection.
```

### Acceptance checks
- crossing safety exists as a first-class system,
- protection expires deterministically,
- logic is isolated from portal VFX and scene triggers.

## Task 7 — Mutation gate

### Purpose
Evaluate whether a mutation candidate is legal under shared protection state.

### Claude prompt
```text
Implement CoopMutationGate / mutation authority evaluation service.

Target folder:
Assets/_Project/Scripts/World/Graph/Mutation/

Namespace:
GhostHunt.World.Graph.Mutation

Implement:
- CoopMutationGate : IMutationAuthorityService

Scope for this task:
- implement Evaluate(MutationCandidate)
- Commit can return a placeholder NotImplemented or safe stub result for now

Requirements:
- use IObservationLedgerService,
- query affected nodes/portals from IHouseGraphQueryService or candidate payload,
- reject mutation if any affected node/portal is protected,
- return structured MutationAuthorityDecision with rejection reason and affected ids,
- do not mutate graph state yet.

Add edit mode tests for:
- legal mutation with no protected ids,
- rejected mutation when affected node is protected,
- rejected mutation when affected portal is protected,
- correct reason payload on rejection.
```

### Acceptance checks
- legality can be tested independently of mutation commit,
- rejection reasons are explicit,
- service uses contracts rather than direct scene references.

## Task 8 — Portal authority service

### Purpose
Expose authoritative portal destination truth and versioning.

### Claude prompt
```text
Implement PortalAuthorityService.

Target folder:
Assets/_Project/Scripts/World/Graph/Portal/

Namespace:
GhostHunt.World.Graph.Portal

Implement:
- PortalAuthorityService : IPortalAuthorityService

Requirements:
- store authoritative PortalAuthorityState keyed by portal id,
- expose CurrentPortalVersion,
- allow runtime state replacement/update through explicit methods,
- do not include local rendering logic,
- do not include threshold VFX logic,
- add simple debug-friendly query helpers.

Add edit mode tests for:
- portal state lookup,
- missing portal lookup,
- version changes when portal state changes.
```

### Acceptance checks
- portal truth is separate from local presentation,
- version field changes predictably,
- no renderer coupling exists.

## Task 9 — House authority service

### Purpose
Publish the authoritative house snapshot and versioned read model for clients.

### Claude prompt
```text
Implement HouseAuthorityService and a simple snapshot publisher abstraction.

Target folder:
Assets/_Project/Scripts/Match/Networking/Runtime/

Namespace:
GhostHunt.Match.Networking.Runtime

Implement:
- HouseAuthorityService : IHouseAuthorityService
- InMemoryHouseSnapshotPublisher : IHouseSnapshotPublisher

Requirements:
- build NetworkHouseSnapshot from current graph, portal authority, and observation ledger state,
- expose CurrentHouseVersion,
- publish snapshots through the publisher interface,
- keep implementation transport-agnostic,
- do not implement lobby/session flow,
- do not add network library dependencies.

Add edit mode tests for:
- snapshot includes topology version,
- snapshot includes portal version,
- snapshot includes protected ids,
- publish method receives expected snapshot.
```

### Acceptance checks
- authoritative snapshot is composable from current services,
- publisher abstraction keeps transport separate,
- no gameplay UI logic leaks in.

## Task 10 — Client reconciler

### Purpose
Apply authoritative snapshots to local presentation without claiming ownership of truth.

### Claude prompt
```text
Implement ClientHouseReconciler and a simple local presentation adapter contract implementation stub.

Target folders:
Assets/_Project/Scripts/Match/Networking/Runtime/
Assets/_Project/Scripts/World/Graph/Runtime/

Namespaces:
GhostHunt.Match.Networking.Runtime
GhostHunt.World.Graph.Runtime

Implement:
- ClientHouseReconciler : IClientHouseReconciler
- StubLocalHousePresentationService : ILocalHousePresentationService

Requirements:
- consume NetworkHouseSnapshot,
- compare incoming versions to local known versions,
- request local snapshot application when stale,
- return ClientHouseReconciliationResult with explicit status,
- do not reinterpret authority truth,
- keep presentation service thin and replaceable.

Add edit mode tests for:
- no-op reconciliation when versions match,
- local refresh when topology version is stale,
- local refresh when portal version is stale,
- result payload reports what changed.
```

### Acceptance checks
- reconciliation logic is explicit and testable,
- local presentation remains a consumer of authority truth,
- no transport dependencies exist.

## Task 11 — Debug snapshot providers

### Purpose
Expose inspectable public debug state for hidden multiplayer systems.

### Claude prompt
```text
Implement debug snapshot providers for the networked house runtime.

Target folders:
Assets/_Project/Scripts/UI/Debug/
Assets/_Project/Scripts/Match/Networking/Runtime/
Assets/_Project/Scripts/World/Graph/Mutation/

Namespaces:
GhostHunt.UI.Debug
GhostHunt.Match.Networking.Runtime
GhostHunt.World.Graph.Mutation

Implement:
- one HouseDebugSnapshotProvider : IHouseDebugSnapshotProvider
- one MutationDecisionTraceProvider : IMutationDecisionTraceProvider
- one minimal CoopDebugOverlay presenter class that consumes those providers

Requirements:
- expose current house version, topology version, portal version,
- expose aggregated protected node ids and portal ids,
- expose last mutation decision trace,
- keep the overlay presenter passive and read-only,
- do not place gameplay logic into debug code.
```

### Acceptance checks
- major hidden state is visible through public contracts,
- debug presenter only reads public snapshots,
- debug does not depend on private fields.

## Task 12 — Graybox integration pass

### Purpose
Wire the minimal two-player authority loop in a controllable graybox environment.

### Claude prompt
```text
Create a graybox integration pass for the networked house runtime services.

Target folders:
Assets/_Project/Scripts/Match/Networking/Runtime/
Assets/_Project/Scenes/Test/
Assets/_Project/Tests/PlayMode/

Namespaces:
GhostHunt.Match.Networking.Runtime

Scope:
- create a lightweight composition root for test use only,
- wire graph runtime, portal authority, observation ledger, mutation gate, house authority service, and client reconciler,
- support a deterministic two-player test scenario,
- do not add production lobby code,
- do not add creature systems.

Provide:
- a small test harness component,
- one play mode smoke test,
- clear inspector fields or seed methods for graybox setup.
```

### Acceptance checks
- services can be exercised together in one scene/test harness,
- two-player deterministic scenarios can be run,
- integration remains minimal and not production-bloated.

## Task 13 — Edit mode test batch

### Purpose
Strengthen logic coverage before more live multiplayer complexity is added.

### Claude prompt
```text
Add an edit mode test batch covering the networked house runtime.

Target folder:
Assets/_Project/Tests/EditMode/

Requirements:
- group tests by service,
- avoid scene boot where possible,
- cover payload validity, version propagation, protection aggregation, mutation rejection, portal version changes, and reconciliation results,
- use small seeded graphs,
- keep tests deterministic.

Do not rewrite production code unless necessary to enable testability.
If you must, make the smallest constructor/injection changes possible.
```

### Acceptance checks
- core domain logic has focused deterministic coverage,
- tests stay fast,
- architecture remains injection-friendly.

## Task 14 — Play mode test batch

### Purpose
Validate the visible graybox behavior of two-player observation and mutation safety.

### Claude prompt
```text
Add a play mode test batch for the networked house graybox runtime.

Target folder:
Assets/_Project/Tests/PlayMode/

Requirements:
- include at least one two-player observation scenario,
- include one threshold crossing safety scenario,
- include one mutation rejection scenario,
- include one reconciliation-after-change scenario,
- use the graybox harness from the integration pass,
- keep scenarios deterministic and minimal.

Do not turn these into full end-to-end gameplay tests.
These are networked house behavior tests only.
```

### Acceptance checks
- key multiplayer impossible-space rules behave in a live harness,
- scenarios are reproducible,
- failures point to specific system regressions.

## Reusable prompt footer
Append this footer to most Claude implementation requests.

```text
Important constraints:
- Follow the ownership boundaries from the Networked House Runtime Interfaces / Contracts Doc.
- Preserve runtime-vs-definition separation.
- Keep scene objects thin.
- Add or update tests for the new logic.
- Add debug-readable output or snapshot support for any hidden state introduced.
- Do not introduce giant manager classes.
- Do not add networking-library-specific code unless explicitly requested.
- Explain any assumptions briefly before coding.
- Keep the implementation small and composable.
```

## Review checklist after each Claude task
After Claude finishes a task, manually verify:
- did it write only the requested files,
- did it keep ownership boundaries intact,
- did it avoid inventing new domain concepts that conflict with the docs,
- did it include test hooks,
- did it expose debug state if hidden logic was added,
- did it accidentally couple scene objects to authority truth [file:12][file:13][file:14][file:15]?

## Escalation rule
If Claude gets stuck or starts thrashing on implementation details, stop and create a narrower follow-up prompt rather than asking it to “just fix it.” That approach is consistent with the project’s existing milestone philosophy and with the lesson already learned from your first Unity sprint: bounded tasks and explicit ownership work much better than open-ended implementation requests [file:12][file:13][file:15].

## Best usage pattern
The best day-to-day usage pattern is:
1. pick one task,
2. paste the prompt,
3. review file diff and tests,
4. run/debug locally,
5. then generate the next prompt.

That gives you the same iterative rhythm your broader roadmap has been pushing from the start: small playable/testable slices, not giant speculative leaps [file:12][file:15].

## Recommended next document
After this task pack, the strongest next document is **Networked House Debug Overlay Spec**, because once Claude starts implementing the authority and observation stack, the next thing that will save your sanity is a single explicit spec for what the debug HUD/panels must show, how the information is grouped, and which toggles/gizmos are mandatory for co-op spatial-horror tuning.

