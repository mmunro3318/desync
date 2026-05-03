# Observation Lock / Spatial Mutation Rules Spec

## Document objective
Define the rule framework that governs when impossible-space topology may or may not change. This spec turns the project’s spatial-horror premise into a system contract: what counts as observation, what gets locked, when mutations are legal, what types of mutations are allowed, how legality is validated against the authoritative house graph, and what debug visibility is required to make the system tunable in play [file:12][file:13][file:14][file:15].

## Why this document exists
The house builder and graph sprints give the project structural truth. This document defines the rules that can bend that truth without collapsing readability. The game’s core hook depends on the player learning one terrifying rule: the house changes when it is not being witnessed. For that rule to feel legible instead of arbitrary, observation, locking, mutation eligibility, and visible consequence all need explicit ownership and consistent semantics [file:12][file:13].

## High-level thesis
The house may mutate only through legal graph operations applied to currently mutable topology regions, and regions become temporarily non-mutable when they are actively observed by at least one valid observer. The system’s job is not to create chaos. The system’s job is to create distrust while preserving enough consistency that players can form and lose beliefs in interesting ways [file:12][file:13][file:15].

## Design goals
This system should optimize for the following:
- spatial horror through changing structure,
- player-readable cause and effect,
- strong support for co-op perception rules,
- modular mutation content instead of bespoke one-off hacks,
- and debug-first iteration for hidden-state behavior [file:12][file:13][file:15].

## Questions this spec answers
- What counts as “observed”?
- What exactly gets locked by observation?
- When can a mutation attempt occur?
- How is a legal mutation chosen?
- What graph invariants must never be broken?
- How should multiplayer observation affect mutation authority?
- What debug surfaces are mandatory?

## Out of scope
This spec does **not** fully define:
- final monster AI behavior,
- audio implementation,
- final visual effects language,
- full network replication details,
- or final content taxonomy of every anomaly.

It defines the rules harness those systems will depend on.

## Core design statement
**If no valid observer is witnessing a mutable topology region, the house may rewrite that region through legal graph mutations. If any valid observer is witnessing it, that region is locked and must persist.** This is the foundational rule the entire spatial-horror experience will teach and exploit [file:12][file:13].

## Relationship to prior architecture
This spec assumes the graph sprint has established one authoritative runtime graph, because mutation legality must operate on graph truth rather than scene geometry guesses. It also inherits the project’s core architectural discipline: runtime state is separate from definitions, scene objects remain thin, and hidden systems must expose debug surfaces from the start [file:12][file:13][file:14][file:15].

## Primary concepts

### Observer
An entity capable of locking topology by valid perception.

### Observation
A runtime-confirmed state in which an observer currently witnesses a topology region strongly enough to prevent mutation.

### Lock
A temporary no-mutate condition applied to graph elements or mutation regions.

### Mutation region
A bounded subset of the house graph considered for one legal topology transform.

### Mutation operation
A graph-level change such as swapping a portal target, extending a hall chain, inserting a loop, substituting a room variant, or shifting a vertical link.

### Legality pass
A validation step that rejects mutations that would violate graph invariants, trap the simulation in unreadable states, or break authored safety rules.

## Ownership model
To preserve narrow ownership, this spec recommends the following runtime systems [file:13][file:14].

| System | Owns | Does not own |
|---|---|---|
| `ObservationService` | observer registration, perception tests, lock state | choosing mutations |
| `ObservationLockRegistry` | active lock records by node/portal/region | line-of-sight sensing |
| `SpatialMutationDirector` | scheduling mutation attempts, selecting candidates | raw graph authority |
| `MutationLegalityService` | validating candidate operations | rendering, audio, VFX |
| `HouseGraphMutationService` | applying approved graph changes | player sensing logic |
| `SpatialDebugOverlay` | mutation and lock visualization | mutation decisions |

This keeps observation, legality, execution, and debug from collapsing into one giant system, which aligns with the broader project design rules [file:12][file:13][file:15].

## Data versus runtime split
This spec should preserve the project-wide rule that definitions and runtime state remain separate [file:12][file:13][file:14].

### Definition-side examples
- `MutationRuleDefinition`
- `MutationOperationDefinition`
- `ObservationRuleDefinition`
- `LockPolicyDefinition`
- `MutationWeightProfile`

### Runtime-side examples
- `ObserverRuntimeState`
- `ObservationLockRecord`
- `MutationCandidate`
- `MutationAttemptRecord`
- `MutationCooldownState`
- `GraphMutationRuntimeState`

## What counts as an observer
The system should support multiple observer categories, even if only players are active at first.

### Observer categories
- local player camera,
- remote player camera,
- scripted camera or debug observer,
- optional entity observer for future mechanics.

### MVP rule
For the first implementation pass, only active player cameras should count as lock-producing observers. That keeps the rule readable while the system stabilizes [file:12][file:13][file:15].

## What counts as observation
Observation should not mean “the player exists nearby.” It should mean a specific runtime perception test passed.

### Minimum observation checks
- observer is active,
- observer has line of sight or valid portal-mediated sight path,
- target is within observation volume/range,
- target is inside the observer’s viewing cone or explicitly designated awareness rule,
- target is not blocked by an occluder that invalidates the lock.

### Optional later modifiers
- flashlight on/off,
- sanity/perception distortion,
- blind states,
- one-way inter-dimensional sight rules.

## Observation targets
Observation can apply at multiple topological scales.

### Valid lock targets
- node,
- portal,
- vertical link,
- mutation region,
- optionally edge segment.

### Recommendation
For the first implementation, the system should resolve raw perception into **region locks** built from graph elements, rather than trying to manage independent mutation rights on every tiny element. Region-level mutation is easier to reason about and debug.

## Lock semantics
The simplest usable mental model is:
- an observed region is locked,
- an unobserved region is eligible,
- and locks persist only while at least one valid observer maintains them.

### Lock characteristics
- locks are temporary,
- locks are recomputed continuously or at regular intervals,
- multiple observers may contribute to one lock,
- lock source information should remain debuggable.

## Recommended lock record
A lock record should track enough metadata to explain why a mutation did not occur.

### Suggested fields
- `lockId`
- `targetType`
- `targetId`
- `observerId`
- `reason`
- `acquiredTime`
- `lastValidatedTime`
- `expiresTime` or invalidation mode

## Observation evaluation cadence
The system should not necessarily evaluate every possible relation every frame in the dumbest way.

### Recommended cadence
- lightweight coarse checks every frame or fixed update,
- lock resolution on a short tick,
- mutation scheduling on a slower cadence than observation evaluation.

### Design principle
Observation must feel immediate enough that players trust the rule, while mutation attempts should feel paced rather than noisy [file:12][file:13].

## Mutation trigger model
Mutations should not happen constantly just because something is unobserved. They should happen through a scheduler/director so pacing remains legible and tunable [file:12][file:13][file:15].

### Mutation may consider
- elapsed time since last mutation,
- player stress/progress state,
- depth into house,
- current expedition phase,
- number of active safe zones,
- active anomaly budget,
- and whether a candidate region is currently unlocked.

## Mutation candidate lifecycle
A mutation attempt should move through explicit phases.

1. Identify candidate region.
2. Confirm region is currently unlockable.
3. Generate candidate operations.
4. Run legality checks.
5. Select one operation by weighted rules.
6. Apply graph mutation.
7. Publish events, debug records, and presentation hooks.

This explicit lifecycle makes Claude-friendly implementation much easier and fits the project’s broader preference for narrow, inspectable systems [file:12][file:13].

## Types of spatial mutation
The system should support a family of operations, each implemented as a legal graph transform rather than a scene hack.

### Tier 1 mutations for early prototype
- portal destination swap,
- room variant substitution,
- corridor extension or contraction,
- loop insertion or loop removal,
- dead-end conversion,
- one-way sight bridge without traversal.

### Tier 2 mutations for later
- vertical displacement,
- pocket-space insertion,
- dimensional layer divergence,
- topology fork by player group separation,
- observer-dependent asymmetry.

### Recommendation
Prototype only 1–2 mutation families first, preferably **portal swap** and **corridor extension**, because they are conceptually clear and graph-friendly.

## Mutation region rules
A mutation should not necessarily rewrite the whole house.

### Region design goals
- mutations happen in bounded local regions,
- the rest of the house remains stable enough for orientation,
- regions are identifiable in debug,
- regions may overlap only under explicit rules.

### Good early rule
Choose one primary mutable region near but not directly containing the player, and one or more stable anchor regions that preserve global orientation.

## Stable anchors
To avoid fully arbitrary house behavior, some topology should remain intentionally stable.

### Stable anchor examples
- entry foyer,
- expedition camp/van/tent boundary,
- artifact chamber once revealed,
- authored checkpoint nodes,
- temporary salt/protective zones.

Stable anchors give the player places to triangulate from, which protects the experience from becoming unreadable randomness [file:12][file:13].

## Safety and readability invariants
Mutations must never violate core structural rules.

### Invariants that should always hold
- the graph remains internally valid,
- required anchor nodes remain reachable unless the current game phase explicitly permits otherwise,
- players are not placed inside invalid geometry,
- portals always resolve to valid endpoints,
- vertical links remain coherent,
- required objective routes are not permanently deleted without replacement,
- and house state remains debuggable after mutation.

## Co-op observation rules
Because your concept is stronger in co-op, observation needs explicit multiplayer semantics even before networking is perfect.

### Core co-op rule
If **any** valid player observer is currently locking a region, that region is not eligible for mutation.

### Design consequences
- one player can preserve a corridor while another explores elsewhere,
- two players looking into the same threshold can keep it fixed,
- players can intentionally manipulate sightlines to force or prevent change,
- betrayal/miscoordination becomes naturally systemic.

This rule directly supports the social-spatial horror fantasy you described.

## One-way and cross-dimension observation
Later layers of the game may allow asymmetric sight.

### Examples
- player A sees player B through a dimensional bleed,
- player A can see a room in another layer but cannot enter,
- an entity may observe the player while remaining unseen.

### Rule recommendation
Treat cross-layer observation as valid only when the graph and visibility rules explicitly authorize it. Do not let rendering coincidences create locks accidentally.

## Hysteresis and anti-flicker rules
Without smoothing, the house may thrash between locked and unlocked states.

### Recommended protections
- short lock persistence after line-of-sight loss,
- mutation cooldown per region,
- global mutation cooldown,
- observer reacquire grace period,
- and no immediate reversal of the last mutation unless explicitly allowed.

These rules help preserve readability and stop the system from looking buggy rather than uncanny.

## Suggested core classes
The exact naming can evolve, but the following class map should be close to implementation-ready.

| Class | Responsibility |
|---|---|
| `ObservationService` | gathers observer data and computes observation results |
| `ObserverRuntimeState` | per-observer perception state |
| `ObservationLockRegistry` | stores active locks and region lock summaries |
| `SpatialMutationDirector` | schedules mutation opportunities |
| `MutationCandidateGenerator` | produces graph operation candidates for a region |
| `MutationLegalityService` | validates candidate graph transforms |
| `HouseGraphMutationService` | applies approved graph mutations |
| `MutationCooldownTracker` | tracks local and global cooldowns |
| `SpatialDebugOverlay` | visualizes locks, candidates, and mutation history |

## Suggested interfaces
The system should prefer narrow contracts over global direct access, consistent with the rest of the project [file:13][file:14].

```csharp
public interface IObserverSource
{
    string ObserverId { get; }
    bool IsObservationActive();
    ObservationSample GetObservationSample();
}

public interface IMutationLegalityRule
{
    bool Evaluate(MutationCandidate candidate, HouseGraphInstance graph, out string reason);
}

public interface IGraphMutationOperation
{
    string OperationId { get; }
    bool CanApply(HouseGraphInstance graph);
    void Apply(HouseGraphInstance graph);
}
```

## Event flow
The system should publish explicit events so future audio, VFX, UI, AI, and networking can respond without hard references [file:13].

### Suggested events
- `OnObservationLockAcquired`
- `OnObservationLockReleased`
- `OnMutationCandidateGenerated`
- `OnMutationAttemptRejected`
- `OnMutationApplied`
- `OnMutationRegionCooldownStarted`

## Recommended data assets
To remain data-driven, many rules should live in assets rather than hardcoded switch statements [file:12][file:13][file:14].

### Suggested assets
- `ObservationRuleDefinition`
- `LockPolicyDefinition`
- `MutationRuleDefinition`
- `MutationOperationDefinition`
- `MutationWeightProfile`
- `MutationCooldownProfile`

## Suggested first implementation slice
The first pass should be aggressively narrow.

### Prototype rule set
- one observer type: player camera,
- one lock target scale: mutation region,
- one stable anchor set: entry + safe zone,
- one mutation family: corridor extension,
- one secondary mutation family: portal swap,
- one debug overlay panel showing observers, locked regions, eligible regions, and last mutation outcome.

This fits the broader project discipline of proving one major question before broadening scope [file:12][file:13][file:15].

## Milestone framing
This spec recommends two concrete implementation milestones.

### OL-1
Observation lock foundation: observers, lock registry, region eligibility, and debug surfaces.

### OL-2
Spatial mutation foundation: candidate generation, legality checks, operation application, cooldowns, and mutation event flow.

## OL-1 overview
OL-1 is about proving that observation can reliably freeze space.

### OL-1 answers
Can the game consistently determine whether a region is currently observed and therefore not eligible for mutation?

## OL-1 tasks

### OL1-T1 — Create observer contracts and runtime state
Implement `IObserverSource`, `ObserverRuntimeState`, and observer registration flow.

### OL1-T2 — Implement observation sampling
Create the first line-of-sight / cone / range sampling rules for player camera observers.

### OL1-T3 — Define mutation regions
Add region records that map graph elements into lockable mutation regions.

### OL1-T4 — Implement lock registry
Build `ObservationLockRegistry` with lock acquire, refresh, and release semantics.

### OL1-T5 — Add region eligibility query
Expose `IsRegionMutable(regionId)` or equivalent API for downstream mutation systems.

### OL1-T6 — Build observation debug overlay
Show observers, observed regions, lock owners, last validation time, and current mutable regions.

### OL1 acceptance criteria
- active player observation can lock a mutation region,
- loss of observation releases the region after the chosen policy,
- debug tools explain why a region is or is not mutable,
- and stable anchor regions can be marked permanently non-mutable [file:12][file:13][file:15].

## OL-2 overview
OL-2 is about proving that unlocked space can mutate legally.

### OL-2 answers
Can the game choose and apply a legal graph mutation only in currently mutable space?

## OL-2 tasks

### OL2-T1 — Implement mutation candidate model
Create `MutationCandidate` and `MutationAttemptRecord` types.

### OL2-T2 — Implement candidate generator
Generate candidate graph operations from the current graph and unlocked regions.

### OL2-T3 — Implement legality rules
Add legality checks for graph validity, anchor reachability, endpoint integrity, cooldown conflict, and anti-thrash rules.

### OL2-T4 — Implement mutation scheduler/director
Build `SpatialMutationDirector` to pace attempts using cooldowns and weights.

### OL2-T5 — Implement graph mutation application
Create `HouseGraphMutationService` with operation execution and event publication.

### OL2-T6 — Add mutation history/debug panel
Show attempted operation, accepted/rejected status, reason, target region, cooldown state, and last applied mutation.

### OL2 acceptance criteria
- mutations occur only in currently eligible regions,
- illegal operations are rejected with explicit reasons,
- successful mutations preserve graph validity,
- and debug history makes the result inspectable in runtime [file:12][file:13][file:15].

## Debug requirements
This system is not done unless debug tooling makes hidden state legible, because that requirement is already core to the project architecture [file:12][file:13][file:15].

### Required debug views
- observer list and state,
- region lock map,
- currently mutable regions,
- last failed mutation reason,
- last successful mutation,
- active cooldowns,
- anchor regions,
- current graph revision or mutation count.

## Testing strategy
Use three layers of testing.

### 1. Rule tests
Validate observation lock acquisition, release timing, and legality-rule behavior.

### 2. Graph mutation tests
Validate that candidate operations preserve graph invariants or fail correctly.

### 3. Runtime graybox tests
Place one or two players in a graybox scene, inspect lock behavior, and confirm the house changes only when sight rules permit it.

## Risks and mitigations

### Risk 1
Observation rules feel inconsistent to players.

### Mitigation
Prefer a simple line-of-sight + region lock rule first, then add nuance later.

### Risk 2
Mutation becomes random and unreadable.

### Mitigation
Use stable anchors, bounded regions, cooldowns, and a narrow first mutation family.

### Risk 3
The system thrashes when players glance around corners.

### Mitigation
Add hysteresis, reacquire grace, and anti-reversal rules.

### Risk 4
Scene rendering accidentally implies observation that the graph system does not recognize.

### Mitigation
Make observation use explicit rule evaluation and debug overlays rather than trusting presentation coincidence.

## Acceptance checklist
This spec is successfully implemented when:
- observation is defined by explicit runtime rules rather than vague proximity,
- active valid observers can lock mutation regions reliably,
- unlocked regions can be identified and queried clearly,
- mutation candidates are generated as legal graph operations rather than scene hacks,
- legality checks preserve graph invariants and gameplay anchor rules,
- successful mutations publish debug-visible history,
- and the overall system supports the project’s design goal of readable, modular, impossible-space horror rather than arbitrary randomness [file:12][file:13][file:14][file:15].
