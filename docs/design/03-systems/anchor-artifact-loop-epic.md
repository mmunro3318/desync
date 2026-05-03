# Anchor / Artifact Loop Epic

## Epic objective
Define the core objective loop that gives players a concrete reason to enter, traverse, survive, and ultimately resolve a spatially unstable house. The Anchor / Artifact Loop is successful when the game supports a clear start-to-finish expedition structure in which players identify, reach, and destroy or neutralize spatial anchors/artifacts that sustain the anomaly, triggering escalation during the run and a clean end-state when the final required objective is completed [file:1][file:12][file:13][file:15].

## Epic question
What are players actually trying to accomplish while the house mutates around them, and how can that objective be expressed as a modular, replayable loop rather than a one-off scripted sequence [file:1][file:12][file:15]?

## Why this epic exists
The spatial runtime, anomaly families, and graph systems answer how the house can become impossible, but they do not by themselves produce a satisfying game loop. The project still needs a player-facing objective spine that creates route-planning, risk escalation, return pressure, and a meaningful resolution state. This epic establishes that spine in the same modular way earlier project docs established ghost, evidence, and match flow in the clone architecture: clear ownership, runtime-vs-definition separation, and debug-first observability [file:1][file:12][file:13][file:14][file:15].

## Player fantasy
The player enters an ordinary-seeming site that is being held open by impossible anchors. To escape or restore normalcy, the player must go deeper into the unstable interior, find the things binding the house to its broken logic, and destroy or neutralize them before pressure, attrition, or the stalking entity makes retreat impossible.

This loop preserves what was strongest in the older clone structure—exploration, uncertainty, pressure, and final resolution—while replacing ghost identification with progressive destabilization of the anomaly [file:1][file:12][file:15].

## Core loop statement
1. Prepare at the safe boundary.
2. Enter the house.
3. Discover signals that point toward active anchors/artifacts.
4. Traverse unstable space to reach them.
5. Disable or destroy one anchor.
6. Survive the escalation that follows.
7. Repeat until the final threshold for collapse is reached.
8. Escape or complete the ending action before the house fully retaliates.

## Recommended MVP framing
For MVP, treat **anchor** and **artifact** as two layers of the same loop:
- **Anchor** = the gameplay objective slot sustaining the anomaly.
- **Artifact** = the in-world object/form the player actually finds and interacts with.

This lets the system remain abstract and modular while still presenting tangible world objects to the player. In code and data, the anchor is the gameplay truth; the artifact is its authored world manifestation.

## Pillars

### 1. Objective clarity
Players should always understand the current mission state at a high level: how many anchors remain, whether one is currently exposed or locatable, and what must happen next.

### 2. Spatial pressure
Anchors must force movement through unstable geometry. They should not all be solvable from one safe room.

### 3. Escalation on success
Destroying an anchor should make progress feel meaningful but dangerous. The house and any active entity should become less stable or more aggressive after each objective step.

### 4. Resolution with consequence
The final anchor should not end the game instantly. It should trigger a final return, collapse, extraction, or terminal interaction phase.

### 5. Modularity
Adding a new anchor/artifact type should mostly mean authoring new data and interaction definitions, not rewriting the match framework [file:12][file:13][file:14][file:15].

## MVP loop definition
The MVP loop should support:
- a match/expedition start state,
- a defined number of anchors for the run,
- anchor discovery clues or reveal conditions,
- artifact interaction/destruction,
- escalation after each anchor event,
- a final completion condition,
- and win/loss resolution.

The MVP loop does **not** require:
- many anchor archetypes,
- deep narrative quest logic,
- crafting,
- extensive loadout simulation,
- or full multi-day campaign structure.

## Loop model

### Phase 1 — Preparation
The player starts at a safe boundary or low-pressure staging area. This can be the van, porch, tent perimeter, or another liminal home-base space. The purpose of this phase is to set loadout, orient to run state, and enter deliberately rather than spawning directly inside the anomaly.

### Phase 2 — Search
The player navigates the house while looking for evidence of where an anchor currently is or how it can be exposed. Search should rely on space-reading, environmental anomaly cues, tools, or simple clue logic rather than obscure puzzle solving.

### Phase 3 — Contact
The player finds the artifact form of an anchor and performs an interaction sequence: inspect, expose, disrupt, collect, burn, break, or counteract. MVP should use one consistent interaction pattern first.

### Phase 4 — Escalation
A successful anchor event increases danger. Examples include:
- more aggressive anomaly mutations,
- reduced safe routing,
- increased stalker activity,
- less reliable lighting,
- or more frequent impossible-space events.

### Phase 5 — Resolution
After the required number of anchors has been completed, the match enters a final state: escape the house, perform one terminal ritual/destruction step, or survive the collapse window. This last phase gives the loop shape and prevents “destroy final anchor, instant fade out.”

## Anchor model

### `AnchorDefinition`
Static design-time data for an anchor archetype.

Stores:
- anchor id,
- display name,
- artifact type,
- reveal rules,
- interaction type,
- destruction requirements,
- escalation value,
- allowed spawn/context tags,
- optional audiovisual tags.

### `AnchorRuntimeState`
Per-match runtime state for a specific spawned anchor.

Stores:
- anchor instance id,
- assigned graph node or domain,
- current phase (`Hidden`, `Revealed`, `Accessible`, `Disabled`, `Destroyed`),
- clue state,
- whether it has been interacted with,
- whether its completion effect has fired.

### `ArtifactPresenter`
World-facing representation of the anchor.

Owns:
- spawned world object/prefab,
- interaction surface,
- current presentation state,
- basic visual feedback.

This follows the same runtime-vs-definition separation that earlier architecture docs treat as mandatory for maintainability [file:1][file:12][file:13][file:14][file:15].

## Match ownership

### `ExpeditionManager`
Owns:
- run state,
- active anchor set,
- progress count,
- escalation tier,
- final resolution trigger,
- win/loss state.

Does not own:
- low-level graph mutation logic,
- player interaction raycasts,
- or entity AI details.

### `AnchorManager`
Owns:
- selecting anchors for the run,
- spawning or binding anchor instances,
- reveal-state progression,
- validating anchor completion,
- dispatching anchor-completed events.

### `EscalationDirector`
Owns:
- reacting to anchor progress,
- raising tension tier,
- notifying anomaly and threat systems of tier change.

### `AnomalyDirector`
Owns:
- space mutation behavior,
- anomaly pattern selection,
- mutation pacing.

### `ThreatDirector` or `EntityDirector`
Owns:
- stalker/entity aggression state,
- entity spawn or hunt pressure changes linked to escalation.

This mirrors the earlier recommendation to avoid giant managers and instead let a few clear systems own narrow concerns [file:13][file:14][file:15].

## System interactions
- `ExpeditionManager` starts the run and asks `AnchorManager` to initialize the required anchor set.
- `AnchorManager` selects valid anchor placements based on current graph/domain state.
- `PlayerInteractor` or equivalent sends interaction attempts to `ArtifactPresenter`.
- `ArtifactPresenter` delegates validation to `AnchorManager`.
- `AnchorManager` marks the anchor complete and emits `OnAnchorCompleted`.
- `EscalationDirector` raises the escalation tier.
- `AnomalyDirector` and `ThreatDirector` respond to the new tier.
- `ExpeditionManager` checks whether the final completion condition has been met.

## MVP counts
Recommended MVP values:
- 2–3 anchors per run.
- 1 anchor archetype.
- 1 interaction pattern.
- 3 escalation tiers: baseline, stressed, critical.
- 1 final resolution step.

These counts are small enough to playtest quickly while still producing a recognizable session arc, which matches the earlier milestone philosophy of finding the smallest meaningful loop first [file:1][file:12][file:15].

## Anchor discovery methods
For MVP, use one or two discovery methods only:
- proximity or directional tool signal,
- environmental anomaly tell,
- room-level clue marker,
- or graph-triggered exposure after entering the correct region.

Avoid large puzzle chains first. The loop should emphasize risky navigation, not opaque puzzle logic.

## Artifact interaction methods
Start with one simple, testable interaction style:
- hold-to-destroy,
- use correct tool on artifact,
- perform short multi-step interaction in place,
- or carry the artifact to a neutralization point.

For MVP, **hold-to-destroy or use-tool-on-artifact** is best. This keeps the interaction cost low while letting the surrounding danger and space do the interesting work.

## Escalation model
Each completed anchor should increase one or more of:
- anomaly frequency,
- anomaly severity,
- route instability,
- stalker aggression,
- resource drain,
- or safe-zone shrinkage.

Important rule: escalation must be legible. Players should feel that progress has consequences rather than perceiving danger spikes as random noise.

## Final resolution options
Recommended MVP final state:
- destroy the final anchor, then escape to the safe boundary before collapse completes.

This is the clearest structure because it naturally turns the final stretch of the match into a retreat through a now-more-hostile house. It also leverages all the spatial systems you are building better than an instant win state would [file:1][file:12].

## Failure states
The loop should support at least one clear fail path in MVP:
- player death or capture,
- total expedition wipe in co-op,
- house collapse before escape,
- or irreversible resource failure if you choose to prototype expedition supplies.

For the first playable implementation, keep only one hard fail state and one win state.

## Data assets
Recommended data assets:
- `AnchorDefinition`
- `AnchorSpawnRuleDefinition`
- `ArtifactInteractionDefinition`
- `EscalationTierDefinition`
- `ExpeditionRulesDefinition`

This matches the larger project preference that tunable rules live in data assets and reusable systems interpret them [file:12][file:13][file:14][file:15].

## Suggested file structure

### Data
- `Assets/_Project/Data/Match/Anchors/AnchorDefinition.cs`
- `Assets/_Project/Data/Match/Anchors/AnchorSpawnRuleDefinition.cs`
- `Assets/_Project/Data/Match/Anchors/ArtifactInteractionDefinition.cs`
- `Assets/_Project/Data/Match/Expedition/ExpeditionRulesDefinition.cs`
- `Assets/_Project/Data/Match/Escalation/EscalationTierDefinition.cs`

### Runtime
- `Assets/_Project/Scripts/Match/Objectives/AnchorRuntimeState.cs`
- `Assets/_Project/Scripts/Match/Objectives/AnchorManager.cs`
- `Assets/_Project/Scripts/Match/Objectives/ArtifactPresenter.cs`
- `Assets/_Project/Scripts/Match/Flow/ExpeditionManager.cs`
- `Assets/_Project/Scripts/Match/Rules/EscalationDirector.cs`

### Interaction / UI / debug
- `Assets/_Project/Scripts/Interaction/Components/ArtifactInteractable.cs`
- `Assets/_Project/Scripts/UI/HUD/ObjectiveHUDController.cs`
- `Assets/_Project/Scripts/UI/Debug/AnchorDebugOverlay.cs`

## Contracts

### `IAnchorQuery`
```csharp
public interface IAnchorQuery
{
    int GetTotalAnchorCount();
    int GetCompletedAnchorCount();
    bool AreAllRequiredAnchorsComplete();
    IReadOnlyList<string> GetActiveAnchorInstanceIds();
}
```

### `IAnchorInteractionService`
```csharp
public interface IAnchorInteractionService
{
    bool CanInteractWithAnchor(string anchorInstanceId, string actorId);
    bool TryBeginAnchorInteraction(string anchorInstanceId, string actorId);
    bool TryCompleteAnchorInteraction(string anchorInstanceId, string actorId);
    IReadOnlyList<string> GetBlockingReasons(string anchorInstanceId);
}
```

### `IEscalationQuery`
```csharp
public interface IEscalationQuery
{
    int GetCurrentEscalationTier();
    string GetCurrentEscalationTierId();
    float GetAnomalyFrequencyMultiplier();
    float GetThreatAggressionMultiplier();
}
```

### `IExpeditionStateQuery`
```csharp
public interface IExpeditionStateQuery
{
    MatchState GetCurrentMatchState();
    bool IsFinalResolutionActive();
    bool IsEscapeObjectiveActive();
    string GetCurrentObjectiveText();
}
```

These contracts keep objective logic inspectable and composable without forcing other systems to know about specific anchor implementations, which follows the same decoupling logic used in the earlier clone system docs [file:13][file:14][file:15].

## Event flow
Recommended events:
- `OnExpeditionStarted`
- `OnAnchorRevealed`
- `OnAnchorInteractionStarted`
- `OnAnchorCompleted`
- `OnEscalationTierChanged`
- `OnFinalResolutionStarted`
- `OnExpeditionWon`
- `OnExpeditionLost`

Use these to avoid hard references and make debug/event tracing easier, which is consistent with the broader architecture guidance already established [file:13].

## Debug requirements
This loop is not done unless the debug layer can show:
- total anchors,
- completed anchors,
- active anchor ids,
- each anchor’s runtime phase,
- assigned node/domain,
- current escalation tier,
- final resolution active/inactive,
- and the current objective string.

As in the earlier hidden-state frameworks, this is mandatory rather than optional [file:1][file:12][file:13][file:15].

## Acceptance criteria
The Anchor / Artifact Loop epic is complete when all of the following are true:
- The run can start in a valid expedition state.
- A required set of anchors is selected or spawned for the run.
- The player can discover and complete at least one anchor interaction.
- Completing an anchor advances progress and escalates danger.
- The loop supports repeated anchor completion until the final threshold is reached.
- The final objective state activates correctly.
- The run resolves through a stable win or fail condition.
- All important anchor, escalation, and expedition states are visible in debug.
- Repeated runs are playable without blocker bugs.

## Recommended sprint breakdown
Following the earlier “prove one major question at a time” roadmap style [file:1][file:12][file:15]:

### Sprint 5A — Anchor Core State
- create anchor data assets,
- create runtime state,
- create anchor manager,
- create objective HUD/debug surface,
- support one static anchor in scene.

Question answered:
- can the game represent anchor objectives cleanly?

### Sprint 5B — Artifact Interaction Slice
- add artifact presenter/interactable,
- support one complete interaction pattern,
- validate completion path,
- update run progress.

Question answered:
- can players complete an anchor in a satisfying and understandable way?

### Sprint 5C — Escalation Response
- add escalation tiers,
- connect anchor completion to anomaly/threat response,
- update debug and HUD.

Question answered:
- does progress create meaningful rising pressure?

### Sprint 5D — Final Resolution
- add final anchor threshold,
- add escape/final state,
- add win/fail resolution flow,
- run repeated expedition tests.

Question answered:
- does the full loop produce a real match arc?

## Deferred from the epic
- multi-day campaign structure,
- advanced loadout economy,
- multiple anchor archetypes,
- faction/narrative quest systems,
- protector/protégé replacement loop,
- complex cooperative role specialization,
- and post-run progression systems.

## Epic done
Mark complete when:
- [ ] players have a clear objective loop built around finding and neutralizing anchors/artifacts,
- [ ] each completed objective meaningfully escalates the run,
- [ ] the final objective triggers a strong end-state,
- [ ] and the system is modular, debuggable, and stable enough to support new anchor types later.
