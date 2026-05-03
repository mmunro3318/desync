# Co-op Observation and Sync Rules Spec

## Document objective
Define the multiplayer authority model and cross-player observation semantics for the impossible-house prototype. This spec explains how observation locks behave when multiple players are present, which machine or system owns authoritative spatial truth, how local camera views can differ without breaking shared reality, and what rules prevent cross-player contradictions during graph mutation and portal presentation [file:12][file:13][file:14][file:15].

## Why this document exists
The single-player impossible-house stack is now reasonably well defined: graph truth, observation locks, spatial mutation, portal visibility, and a graybox vertical slice plan. Multiplayer changes the hardest rule in the whole game: “space can change when unobserved.” In co-op, “unobserved” is no longer a local camera question. It becomes a shared authority problem involving multiple cameras, multiple active node contexts, possible asymmetric views, and network timing [file:12][file:13].

## Core thesis
The house must have one authoritative topology state, but observation protection may be created by any player whose camera or local presence currently validates a protected region. In other words: **many players can prevent change, but only one authority decides whether change actually occurs** [file:12][file:13][file:15].

## Design goals
This spec should optimize for:
- one authoritative house truth,
- clear cross-player observation-lock semantics,
- low-contradiction portal and mutation presentation,
- network-friendly system ownership,
- and strong debug visibility for multiplayer hidden state [file:12][file:13][file:15].

## Questions this spec answers
- Who owns authoritative graph and mutation truth in co-op?
- What counts as a region being “observed” when multiple players are involved?
- How should locks be aggregated across players?
- What can differ locally per player view, and what must remain globally shared?
- How should portal visibility interact with shared topology authority?
- What contradiction classes must be prevented?

## Out of scope
This spec does **not** fully define:
- the final transport/netcode package choice,
- matchmaking/lobbies,
- voice chat,
- anti-cheat,
- final lag compensation,
- or every replication detail for items and creatures.

It focuses on co-op house truth, observation semantics, and mutation/network consistency.

## Baseline networking principle
The prototype should continue to follow the earlier architectural rule that systems have narrow ownership and that runtime truth is separated from design-time definitions [file:12][file:13][file:14]. In multiplayer form, that means one runtime authority owns the current house graph instance, observation-lock state, and mutation execution, while clients primarily contribute observation reports, player state, and local presentation [file:12][file:13].

## Authority model
Use a **server-authoritative** or host-authoritative simulation model for impossible-house truth.

### Authority owns
- current `HouseGraphInstance`,
- active portal bindings,
- observation-lock ledger,
- mutation eligibility and execution,
- artifact state,
- exit state,
- pressure-system truth.

### Clients own locally
- input,
- camera transform,
- local render presentation,
- local threshold transition presentation,
- local debug visibility.

### Clients do not own
- graph mutation approval,
- portal destination truth,
- shared lock truth,
- or artifact completion truth.

This fits the broader project philosophy of preventing scene-local hacks and keeping hidden systems inspectable and centralized [file:12][file:13][file:15].

## Shared truth versus local truth
The multiplayer model needs a clean distinction.

### Shared truth
The following must be globally authoritative and eventually consistent for all players:
- node topology,
- portal bindings,
- mutation history,
- observation protection state,
- artifact/exit progression,
- and pressure-state truth.

### Local presentation truth
The following may vary by player without breaking global authority:
- which nearby nodes are currently rendered,
- threshold transition timing during crossing,
- portal framing based on camera angle,
- local debug overlays,
- some temporary smoothing/interpolation states.

### Rule of thumb
Players may have different **views**, but they may not have different **house truth** unless the game explicitly introduces dimension-separated rules later.

## Observation semantics in co-op
A region should be considered observation-protected if **any** player is currently satisfying the lock conditions for that region according to authority-validated reports.

### Core rule
If one player is actively observing a protected region or portal-critical space, mutation affecting that protected set may not execute.

### Implication
Observation is aggregated as a union of player protections, not an average or majority vote.

This is the safest interpretation for preserving the game’s promise that looking at something can hold it in place [file:12][file:13].

## Observation sources
Authority should accept observation contributions from more than one signal type.

### Recommended sources
- direct camera sightline into protected portal/region,
- player occupancy in protected node,
- threshold commitment state during crossing,
- explicit temporary render/transition guard raised by the portal system.

### Recommendation
Start with camera sightline + occupied node + threshold transition guards only. Add more sources later only if a concrete contradiction requires them.

## Observation ledger
The authority should maintain a structured observation ledger rather than a few booleans.

### Suggested tracked data
- player id,
- current node id,
- camera context,
- protected node ids,
- protected portal ids,
- protection reason,
- timestamp/sequence,
- expiration window if applicable.

This makes co-op debugging and desync diagnosis far easier than ad hoc flags [file:12][file:13][file:15].

## Suggested runtime models
Use explicit runtime records.

### Examples
- `ObservationContributionRecord`
- `ObservationProtectionLedger`
- `CoopVisibilityContext`
- `MutationAuthorityDecision`
- `PortalAuthorityState`
- `PlayerNodeContextState`
- `NetworkHouseSnapshot`

## Lock aggregation rules
Observation protection should be compositional and explicit.

### Node rule
A node is lock-protected if at least one valid player contribution protects it.

### Portal rule
A portal is lock-protected if at least one player currently observes the threshold or if any player is in protected crossing state for that portal.

### Mutation candidate rule
A mutation candidate is legal only if every node/portal/edge it would alter is currently unprotected by the aggregated ledger.

## Lock release rules
Releasing protection should be slightly conservative to avoid network flicker.

### Recommendation
Use a short authority-side grace window before a lock fully releases after the last contributing player drops observation. This smooths packet jitter and prevents a mutation from firing immediately because a client momentarily lost line of sight due to latency or threshold crossing noise.

### Caution
Keep the grace window short. The house should still feel responsive, not sticky.

## Suggested authority pipeline
A useful co-op pipeline is:
1. clients send player node/camera observation contributions,
2. authority validates and updates protection ledger,
3. authority evaluates mutation eligibility against aggregated protections,
4. authority commits mutation or defers it,
5. authority broadcasts updated graph/portal/mutation snapshot,
6. clients reconcile local render state and presentation.

This preserves one house truth while still allowing each player’s camera to matter [file:12][file:13].

## Client reporting model
Clients should not send “the house changed.” They should send only the local facts authority needs.

### Clients report
- player transform,
- camera direction,
- current node/portal context,
- threshold crossing state,
- local observation candidates.

### Authority derives
- whether observation is valid,
- what protections are active,
- whether mutation is legal,
- and what the current shared topology is.

This follows the broader project rule that hidden truth should live in focused systems rather than scene-local assumptions [file:12][file:13][file:14].

## Portal visibility in co-op
Portal presentation can remain locally evaluated per client, but the portal’s **destination truth** must still come from authority.

### Shared portal truth
- portal id,
- destination node id,
- active binding version,
- mutation lock state if relevant.

### Local portal presentation
- whether the portal is in view,
- current threshold framing,
- transition smoothing,
- local render warm-up.

### Rule
No client should invent a different portal destination than the authority-provided binding.

## Cross-player contradiction classes
These are the contradiction classes multiplayer must prevent first.

### Class 1 — mutation while another player still observes
One player sees a hall/door clearly, but another player’s movement elsewhere causes the house to mutate anyway.

### Class 2 — divergent portal truth
Two players are meant to be in the same shared house state, but the same portal resolves to different destinations due to client-local drift.

### Class 3 — threshold crossing invalidation
A player is midway through a portal crossing and the authority permits a mutation that invalidates the source/destination continuity of that crossing.

### Class 4 — stale-client lock release
Authority thinks a space is unobserved because a client update arrived late or a contribution dropped briefly.

### Class 5 — visible desync after mutation
Authority mutates legally, but clients present conflicting visible states too long after reconciliation.

## Priority rule
Prevent Class 1 through Class 4 first. Class 5 can be mitigated with presentation smoothing once authority semantics are correct.

## Crossing semantics in co-op
Threshold crossing should create temporary shared protection around the crossing player’s source/destination path.

### Rule
If a player has entered a threshold transition window, the authority should protect the relevant portal and involved nodes from mutation until the crossing state resolves or times out.

### Why
Without this rule, co-op movement near doors becomes the easiest way to create unfair or visually broken contradictions.

## Player occupancy semantics
Node occupancy matters even when a player is not directly looking at every affected surface.

### Recommendation
At minimum, the authority should treat the currently occupied node and any actively crossed portal as protected from mutation by that player.

### Reason
A player standing inside a node should not have their immediate local context rewritten out from under them unless the game later explicitly supports a special override event.

## Mutation authority rules
Mutation requests may originate from a director/system, but only authority commits them.

### Authority commit rules
A mutation may commit only when:
- candidate legality passes,
- all affected regions are unlocked by aggregated co-op observation rules,
- no protected crossing state conflicts,
- and the resulting graph state is valid.

### Broadcast rules
After commit, authority must broadcast at least:
- mutation id/type,
- affected node/portal ids,
- new graph/portal binding version,
- and commit sequence/timestamp.

## Snapshot and versioning rules
Versioning matters more in co-op impossible space than in ordinary co-op movement.

### Recommendation
Give shared graph and portal state an explicit version number or sequence id.

### Why
Clients need a stable way to know whether a portal view or local render state is stale relative to current shared topology.

## Suggested shared state groups
A minimal network state split could be:
- `HouseTopologySnapshot`
- `PortalBindingSnapshot`
- `ObservationProtectionSnapshot`
- `ObjectiveStateSnapshot`
- `PressureStateSnapshot`

Keep these separate rather than collapsing everything into one opaque match blob, consistent with the broader narrow-ownership philosophy [file:12][file:13][file:14].

## Suggested class ownership

| Class / Service | Responsibility |
|---|---|
| `HouseAuthorityService` | owns authoritative graph and mutation commit |
| `ObservationLedgerService` | aggregates player observation protections |
| `PlayerObservationReporter` | collects client-side observation candidates |
| `CoopMutationGate` | checks mutation requests against shared protections |
| `PortalAuthorityService` | owns shared portal binding truth |
| `ClientRenderReconciler` | reconciles local presentation to authority state |
| `CoopDebugOverlay` | shows shared and per-player protection state |

## Suggested interfaces
Keep contracts explicit and network-friendly.

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

## Debug requirements
Co-op impossible-space work will be miserable without explicit debug visibility, which is already a core architectural lesson in your earlier docs [file:12][file:13][file:15].

### Required debug views
- per-player current node,
- per-player active portal crossing state,
- per-player observation contributions,
- aggregated protected nodes,
- aggregated protected portals,
- mutation rejection reasons,
- current graph/portal version ids,
- last authority mutation commit,
- client reconciliation state.

## Recommended first co-op slice
Keep the first multiplayer slice brutally small.

### First slice target
- 2 players only,
- host-authoritative model,
- one mutation family,
- one portal class,
- one shared artifact objective,
- debug overlay showing both players’ protections.

### Goal question
Can two players move through the impossible house while shared observation rules prevent unfair or contradictory mutations?

## Testing strategy
Use three layers.

### 1. Authority logic tests
Test protection aggregation and mutation-gate decisions.

### 2. Two-player graybox tests
Walk one player through a threshold while the other tries to trigger or allow a mutation elsewhere.

### 3. Desync resilience tests
Simulate delayed observation updates or short packet loss to verify that grace windows and protection rules prevent obvious unfairness.

## Example scenarios to test
- Player A watches a hall while Player B turns away elsewhere; hall must remain stable.
- Player A crosses a door while Player B enters adjacent node; threshold continuity must hold.
- Both players look away from a candidate route; mutation may commit.
- Player A occupies node X while Player B explores node Y; mutation affecting X must remain blocked.
- Player A loses connection update briefly; authority grace window should avoid immediate illegal mutation.

## Risks and mitigations

### Risk 1
Locks become too conservative and mutation almost never fires.

### Mitigation
Keep protected sets tight and visibility criteria explicit.

### Risk 2
Locks become too loose and players see unfair mutations.

### Mitigation
Prefer union-based protection with short grace windows.

### Risk 3
Clients diverge on portal truth.

### Mitigation
Make portal destination authority-owned and versioned.

### Risk 4
Threshold crossing becomes the main desync failure case.

### Mitigation
Treat crossing state as a first-class protection reason, not a side effect.

## Acceptance checklist
This co-op rules spec is successfully realized when:
- the house has one authoritative topology and mutation truth [file:12][file:13],
- any player’s valid observation can prevent mutation of protected regions,
- portal destination truth remains authority-owned and shared,
- threshold crossing creates temporary protection against contradictory mutation,
- clients may differ in local presentation but not in authoritative house truth,
- mutation commits are versioned and reconcilable,
- and developers can inspect per-player and aggregated protection state in debug during graybox co-op tests [file:12][file:13][file:14][file:15].
