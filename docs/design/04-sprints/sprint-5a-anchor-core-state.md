# Sprint 5A — Anchor Core State

## Sprint objective
Implement the first clean runtime objective layer for the new game loop: define anchors as real match objects with stable data, runtime state, debug visibility, and HUD readout, even before full artifact interaction is complete. Sprint 5A is successful when a run can initialize a required anchor set, track their states, expose that information to debug/UI, and survive restart/reset cleanly in `House_Prototype` [file:1][file:12][file:13][file:14][file:15].

## Sprint question
Can the game represent anchor objectives as modular runtime state, rather than as ad hoc scene scripting, in a way that is clear enough to support later interaction, escalation, and final escape logic [file:1][file:12][file:15]?

## Why this sprint exists
The Anchor / Artifact Loop epic defined the player-facing objective spine, but the system still needs its first narrow implementation slice. Sprint 5A exists to prove the most basic architectural question: can anchors exist as stable match entities with clean ownership, runtime-vs-definition separation, and useful debug/UI surfaces. This follows the same “one major question at a time” milestone discipline used in the earlier project roadmap and clone-loop docs [file:1][file:12][file:15].

## In scope
- `AnchorDefinition` data asset.
- `AnchorRuntimeState` runtime model.
- `AnchorManager` initialization flow.
- `ExpeditionManager` objective state integration.
- One static or semi-static anchor placement path.
- Objective HUD text/readout.
- Debug overlay for anchor state.
- Restart/reset stability.

## Out of scope
- Full artifact interaction.
- Escalation response logic.
- Final escape state.
- Multiple anchor archetypes.
- Procedural anchor placement.
- Threat/entity response.
- Audio/FX feedback beyond minimal debug readability.

## Design intent
This sprint should make the player and the developer feel that the game now has an actual objective structure, even if the objective cannot yet be fully completed through final interactions. The output should not be “fake future hooks”; it should be a real runtime objective spine that later sprints can attach to.

## Recommended slice
Start with one static anchor instance in the house and one run rule that says, effectively:
- total anchors required = 1,
- anchor begins in `Hidden` or `Revealed`,
- debug can show where it is and what state it is in,
- HUD can report objective status.

If you want a slightly stronger slice, allow 2 anchors but still only one anchor archetype.

## Core concepts

### Anchor definition
A design-time content asset that describes what an anchor is supposed to be.

Stores:
- anchor id,
- display name,
- artifact type/tag,
- allowed placement tags,
- default reveal state,
- optional interaction type id,
- escalation value,
- and optional presentation tags.

### Anchor runtime state
A per-match state object representing one active anchor instance.

Stores:
- anchor instance id,
- source definition id,
- assigned node/room/domain,
- current phase,
- whether it is currently discoverable,
- whether it has been completed,
- and any debug labels needed for tuning.

### Expedition state
A lightweight match-level state that tracks:
- total required anchors,
- current completed count,
- whether final resolution should be unlocked,
- and current objective string.

This structure directly follows the earlier architecture rule that definition data, runtime state, and scene-authored placements should be separate concerns [file:1][file:12][file:13][file:14][file:15].

## Match phases for Sprint 5A
Keep phase flow minimal:
- `Booting`
- `Exploring`
- optional `ObjectiveReady`

Do not add win/loss expansion here unless you need a placeholder state to preserve match flow structure. The goal is objective representation, not full match completion.

## System ownership

### `ExpeditionManager`
Owns:
- total required anchor count,
- completed anchor count,
- current objective string,
- run start/reset,
- future final resolution readiness flag.

Does not own:
- per-anchor interaction logic,
- graph placement rules,
- or anomaly escalation behavior.

### `AnchorManager`
Owns:
- creating runtime anchor instances,
- binding definitions to placements,
- storing anchor runtime state,
- exposing anchor queries,
- and reporting anchor state changes.

Does not own:
- HUD rendering,
- player input,
- or anomaly mutation behavior.

### `ObjectiveHUDController`
Owns:
- displaying current objective text,
- displaying anchor progress count,
- optionally displaying simple status cues.

Does not own:
- anchor truth,
- interaction validation,
- or run-state transitions.

### `AnchorDebugOverlay`
Owns:
- visualizing anchor definitions, placements, current runtime phase, and progress counts.

This narrow ownership model keeps the new loop aligned with the existing anti-sprawl design philosophy: a few focused systems, not giant game managers [file:13][file:14][file:15].

## Recommended folders and files

### Data
- `Assets/_Project/Data/Match/Anchors/AnchorDefinition.cs`
- `Assets/_Project/Data/Match/Expedition/ExpeditionRulesDefinition.cs`

### Runtime
- `Assets/_Project/Scripts/Match/Objectives/AnchorRuntimeState.cs`
- `Assets/_Project/Scripts/Match/Objectives/AnchorManager.cs`
- `Assets/_Project/Scripts/Match/Flow/ExpeditionManager.cs`

### Authoring / scene
- `Assets/_Project/Scripts/World/Spawners/AnchorSpawnPoint.cs`

### UI / debug
- `Assets/_Project/Scripts/UI/HUD/ObjectiveHUDController.cs`
- `Assets/_Project/Scripts/UI/Debug/AnchorDebugOverlay.cs`

This file placement follows the existing project structure principles of boring, predictable folders with data in `Data/` and runtime logic in `Scripts/` [file:13][file:14].

## Authoring model
For Sprint 5A, use a very simple scene-authored placement flow:
- place one `AnchorSpawnPoint` in `House_Prototype`,
- assign placement tags or room id,
- assign one `AnchorDefinition`,
- let `AnchorManager` instantiate an `AnchorRuntimeState` for it on run start.

This avoids procedural placement complexity while still proving the runtime model.

## Contracts

### `IAnchorQuery`
```csharp
public interface IAnchorQuery
{
    int GetTotalAnchorCount();
    int GetCompletedAnchorCount();
    IReadOnlyList<string> GetActiveAnchorInstanceIds();
    AnchorPhase GetAnchorPhase(string anchorInstanceId);
    string GetAnchorNodeId(string anchorInstanceId);
}
```

### `IExpeditionObjectiveQuery`
```csharp
public interface IExpeditionObjectiveQuery
{
    string GetCurrentObjectiveText();
    bool IsFinalResolutionReady();
    int GetRequiredAnchorCount();
    int GetCompletedAnchorCount();
}
```

### `IAnchorPlacementQuery`
```csharp
public interface IAnchorPlacementQuery
{
    IReadOnlyList<AnchorSpawnPoint> GetAllAnchorSpawnPoints();
    AnchorSpawnPoint GetSpawnPointForAnchor(string anchorInstanceId);
    bool IsPlacementValid(string anchorId, AnchorSpawnPoint spawnPoint);
}
```

These interfaces keep other systems dependent on stable objective queries rather than concrete implementation details, which fits the broader project goal of safe iterative expansion [file:13][file:14][file:15].

## Enums
Define an early anchor phase enum now:

```csharp
public enum AnchorPhase
{
    None,
    Hidden,
    Revealed,
    Accessible,
    Disabled,
    Destroyed
}
```

Even if Sprint 5A only uses `Hidden`, `Revealed`, and `Destroyed`, having the fuller progression now keeps later sprints coherent.

## Tasks

### 1. Create anchor definition data asset
- [ ] Create `AnchorDefinition.cs`.
- [ ] Add inspector fields for id, display name, artifact type/tag, placement tags, default reveal phase, escalation value, and presentation tags.
- [ ] Author first anchor asset.

#### Acceptance tests
- [ ] Anchor definition asset exists and is editable in inspector.
- [ ] A designer can create a second anchor asset without code changes.
- [ ] Definition data is distinct from runtime state.

### 2. Create anchor runtime state model
- [ ] Create `AnchorRuntimeState.cs`.
- [ ] Store definition id, instance id, assigned node/spawn point, phase, completed flag, and debug labels.
- [ ] Ensure runtime state is created fresh on run start.

#### Acceptance tests
- [ ] Runtime anchor state is not stored directly on the definition asset.
- [ ] Each run gets valid runtime state for each initialized anchor.
- [ ] Runtime anchor ids remain stable during a single run.

### 3. Create anchor spawn point authoring component
- [ ] Create `AnchorSpawnPoint.cs`.
- [ ] Add optional room/node id, placement tags, and anchor definition reference.
- [ ] Place at least one spawn point in `House_Prototype`.

#### Acceptance tests
- [ ] Spawn point is visible and editable in scene.
- [ ] Spawn point can reference an anchor definition.
- [ ] AnchorManager can discover spawn points on run start.

### 4. Create `AnchorManager`
- [ ] Gather spawn points on run start.
- [ ] Instantiate runtime anchor states.
- [ ] Expose anchor queries.
- [ ] Add a simple state-change method for later sprints, even if only debug-driven for now.

#### Acceptance tests
- [ ] AnchorManager initializes all expected anchors at run start.
- [ ] Query methods return correct counts and ids.
- [ ] AnchorManager survives restart without stale state.

### 5. Create `ExpeditionManager`
- [ ] Track required anchor count.
- [ ] Track completed anchor count.
- [ ] Generate current objective text.
- [ ] Reset expedition state on restart.

#### Acceptance tests
- [ ] Game enters a valid expedition state on start.
- [ ] Objective text reflects current anchor counts.
- [ ] Restart restores a clean expedition state.

### 6. Add objective HUD
- [ ] Create `ObjectiveHUDController.cs`.
- [ ] Show current objective string.
- [ ] Show `completed / required` anchor progress.
- [ ] Ensure it updates live in play mode.

#### Acceptance tests
- [ ] HUD shows objective state when a run begins.
- [ ] HUD reflects anchor counts accurately.
- [ ] HUD remains readable enough for repeated playtesting.

### 7. Add debug overlay
- [ ] Create `AnchorDebugOverlay.cs`.
- [ ] Show anchor instance ids, definition ids, node/spawn point ids, phase, completed flag, and counts.
- [ ] Add expedition objective string and final-resolution-ready flag.

#### Acceptance tests
- [ ] Debug overlay shows all active anchors.
- [ ] Debug overlay shows correct phase and placement for each anchor.
- [ ] Debug values update live during state changes.

### 8. Add reset and replay stability
- [ ] Restart clears runtime anchor states.
- [ ] Restart clears expedition counts and objective text.
- [ ] Run repeated boot/start/restart tests.

#### Acceptance tests
- [ ] No stale anchor instances persist across restart.
- [ ] Objective counts reset correctly every run.
- [ ] No critical console errors occur during repeated restarts.

## Debug requirements
Sprint 5A is not done unless debug can show:
- total required anchors,
- total completed anchors,
- active anchor instance ids,
- source definition ids,
- assigned node or spawn point ids,
- anchor phase,
- completed flag,
- and current objective string.

This remains faithful to the project’s debug-first rule for hidden or indirect systems [file:1][file:12][file:13][file:15].

## Suggested implementation order
Following the same narrow-slice sequencing logic used successfully in earlier docs [file:1][file:12][file:15]:
1. `AnchorDefinition`
2. `AnchorRuntimeState`
3. `AnchorSpawnPoint`
4. `AnchorManager`
5. `ExpeditionManager`
6. `ObjectiveHUDController`
7. `AnchorDebugOverlay`
8. restart/replay stabilization

## Full Sprint 5A smoke test
Run this before marking Sprint 5A complete:
- [ ] Launch game from `Bootstrap`.
- [ ] Enter `House_Prototype`.
- [ ] Confirm `AnchorManager` initializes at least one anchor instance.
- [ ] Confirm `ExpeditionManager` enters a valid expedition state.
- [ ] Confirm HUD shows objective text and `0 / required` progress.
- [ ] Confirm debug overlay shows anchor instance id, definition id, phase, and placement id.
- [ ] Trigger one debug state change, for example `Hidden -> Revealed`, and confirm HUD/debug update live.
- [ ] Restart the round.
- [ ] Confirm a clean runtime anchor state is rebuilt.
- [ ] Confirm objective counts reset correctly.
- [ ] Confirm no stale anchor ids or blocker errors remain.

## Deferred from Sprint 5A
Only defer clearly non-blocking items for Sprint 5B+.

- [ ] Real artifact interaction flow.
- [ ] Escalation tier changes.
- [ ] Final resolution unlock.
- [ ] Dynamic/procedural anchor placement.
- [ ] Multiple anchor archetypes.
- [ ] Threat response to anchor progress.
- [ ] Polished audiovisual feedback.

## Sprint done
Mark complete when:
- [ ] anchors exist as clean runtime match objects,
- [ ] the run tracks objective progress through `AnchorManager` and `ExpeditionManager`,
- [ ] HUD/debug expose anchor truth clearly,
- [ ] and repeated restarts produce stable clean state without blocker bugs.
