The right move here is to define a starter implementation spec that is stable enough to support the later phase roadmaps, but not so overdesigned that it locks you in too early. The main goal of this spec is to establish **what exists**, **what owns what**, and **what “done” looks like** for the project foundation before we break it into phase-by-phase development plans.
## Project intent

This Unity project should be built as a modular, data-driven first-person ghost investigation game prototype that can first clone the core deduction loop, then expand cleanly into custom ghost logic, evidence, items, and mechanics. The architecture should prioritize reusable runtime systems, ScriptableObject-driven content definitions, and minimal hardcoded ghost-specific branching so future phase roadmaps can add features incrementally without refactoring the entire project.

## Rules of the architecture

These are the project rules everything else should follow:

- Runtime state and content definition must be separate, for example `GhostController` handles the current match ghost while `GhostDefinition` describes what a ghost type is.
- Scene objects should stay thin; reusable logic should live in components and systems.
- Adding a new ghost type should mostly mean creating or editing data assets, not writing new engine-level code.
- Adding a new evidence type should mostly mean defining a new evidence rule, emitter behavior, and tool interaction path.
- All hidden systems must expose debug data early, because this genre becomes impossible to tune without observability.
- Menus, progression, account systems, and polish flow are out of scope for the starter implementation.

## Target stack

Assume:

- Unity 6.x.
- URP.
- New Input System.
- C# only for the starter implementation.
- Multiplayer-first architecture throughout; all systems must be written with network ownership in mind from day one. Early phases may be tested locally using Unity's Multiplayer Play Mode, but no system should assume a single local source of truth.
- Graybox art and placeholder audio at first.

Unity’s data-driven asset workflow and ScriptableObject model make this a good fit for ghost/evidence/tool definitions that need to be iterated rapidly.

## Directory structure

Use this folder layout inside `Assets/_Project/`:

```txt
Assets/
  _Project/
    Art/
      Materials/
      Models/
      Audio/
      VFX/
      Animations/
      Prefabs/
        Environment/
        Interactables/
        Items/
        Ghost/
        UI/
        Debug/

    Data/
      Ghosts/
      Evidence/
      Tools/
      Match/
      Rooms/
      UI/

    Scenes/
      Bootstrap/
      Test/
      House_Graybox/
      House_Prototype/

    Scripts/
      Core/
        Bootstrap/
        Events/
        Utilities/
        Constants/
      Player/
        Input/
        Movement/
        Camera/
        Interaction/
        Inventory/
      Interaction/
        Interfaces/
        Components/
      World/
        Doors/
        Lights/
        Rooms/
        Props/
        Spawners/
      Items/
        Base/
        Tools/
        Runtime/
      Ghost/
        Runtime/
        States/
        Behaviors/
        Evidence/
      Match/
        Flow/
        Rules/
        Objectives/
      UI/
        HUD/
        Journal/
        Debug/
      Audio/
      Editor/

    Settings/
      Input/
      RenderPipeline/
      Physics/
```

This structure is intentionally boring. That is a good thing. It gives each later phase a predictable place to land.

## Scenes

Define four scenes now, even if only two are used immediately:

|Scene|Purpose|
|---|---|
|`Bootstrap`|Loads the target gameplay scene and initializes persistent systems.|
|`Test`|Tiny developer sandbox for isolated script testing.|
|`House_Graybox`|First playable movement/interactions scene.|
|`House_Prototype`|Main evolving prototype scene after Phase 0 stabilizes.|

`Bootstrap` exists to prevent manager sprawl in your actual gameplay scene. `Test` exists because otherwise every tiny script change gets tested in the full house, which slows iteration badly.

## Namespaces

Use namespaces from the beginning:

```csharp
GhostHunt.Core
GhostHunt.Player
GhostHunt.Interaction
GhostHunt.World
GhostHunt.Items
GhostHunt.Ghost
GhostHunt.Match
GhostHunt.UI
GhostHunt.Audio
GhostHunt.Editor
```

This matters because later phase roadmaps will refer to concrete classes, and consistent namespaces reduce confusion and class collisions.

## Starter scene hierarchy

For `House_Graybox`, use this first-pass hierarchy:

```tx
House_Graybox
  Systems
    MatchManager
    RoomRegistry
    EvidenceManager
    DebugManager
    UIManager
  PlayerSpawn
  Player
    CameraRoot
      PlayerCamera
      InteractionRayOrigin
      HeldItemAnchor
  Environment
    Structure
      Floors
      Walls
      Ceilings
    Doors
    Lights
    Furniture
    PickupProps
  Rooms
    EntryRoom
    Hallway
    LivingRoom
    Kitchen
    Bedroom
  Ghost
    GhostRoot
    GhostAnchorPoints
    EvidenceAnchors
  Canvas_HUD
    Crosshair
    InteractionPrompt
    HeldItemLabel
    DebugPanel
```

Even before ghost logic exists, keep the `Ghost` root in the scene so placement and anchor concepts are established early.

## Core prefabs

Create these prefabs first:

## Environment prefabs

- `PF_Door_Single`
- `PF_LightSwitch_Basic`
- `PF_CeilingLight_Basic`
- `PF_Table_Basic`
- `PF_Shelf_Basic`

## Interactable prefabs

- `PF_Pickup_Base`
- `PF_Inspectable_Base`
- `PF_InteractPromptAnchor`

## Item prefabs

- `PF_Item_Flashlight`
- `PF_Item_TestDetector`
- `PF_Item_JournalPlaceholder`

## Debug prefabs

- `PF_DebugRoomLabel`
- `PF_DebugGhostInfoPanel`

These prefab names matter because the later roadmap can refer to them directly.

## First 15 scripts

These are the first implementation scripts I would consider part of the starter spec:

|Order|Script|Ownership|
|---|---|---|
|1|`GameBootstrap.cs`|Scene and startup flow|
|2|`PlayerInputRouter.cs`|Input abstraction|
|3|`PlayerMotor.cs`|Character movement|
|4|`PlayerLook.cs`|Camera look|
|5|`PlayerInteractor.cs`|Raycast interaction|
|6|`PlayerInventory.cs`|Held/equipped item state|
|7|`HeldItemPresenter.cs`|Visual representation of held item|
|8|`IInteractable.cs`|Base interaction contract|
|9|`InteractableBase.cs`|Shared interactable logic|
|10|`DoorInteractable.cs`|Door open/close|
|11|`LightSwitchInteractable.cs`|Switch toggling|
|12|`PickupItem.cs`|Pickup behavior|
|13|`HUDController.cs`|Prompt/crosshair/item text|
|14|`DebugOverlay.cs`|Debug state visibility|
|15|`RoomVolume.cs`|Room metadata and bounds|

Only after these should ghost and match scripts start being implemented.

## Second-wave scripts

These come immediately after the foundational 15:

|Script|Purpose|
|---|---|
|`MatchManager.cs`|Current round state and win/loss ownership|
|`GhostDefinition.cs`|Data asset for ghost type|
|`GhostController.cs`|Runtime ghost brain|
|`GhostStateMachine.cs`|Ghost state transitions|
|`EvidenceDefinition.cs`|Data asset for evidence type|
|`EvidenceManager.cs`|Evidence eligibility/emission|
|`ToolDefinition.cs`|Data asset for tools|
|`JournalController.cs`|Journal tracking and ghost guess flow|
|`RoomRegistry.cs`|Tracks all `RoomVolume` instances|
|`GhostSpawnPoint.cs`|Defines ghost spawn locations|
|`EvidenceAnchor.cs`|Defines evidence-capable world positions|

These scripts define the skeleton that later phases will flesh out.

## Foundational data assets

Create the following ScriptableObject classes early, even if some are mostly placeholders at first:

## `GhostDefinition`

Fields:

- `ghostId`
- `displayName`
- `description`
- `allowedEvidence`
- `baseRoamInterval`
- `baseEventInterval`
- `baseHuntThreshold`
- `preferredRoomBias`
- `behaviorModifiers`

Purpose: describes ghost type identity and tunable defaults.

## `EvidenceDefinition`

Fields:

- `evidenceId`
- `displayName`
- `toolTypeRequired`
- `minEmitInterval`
- `maxEmitInterval`
- `visibilityMode`
- `spawnRuleType`

Purpose: defines what evidence is and how it can be surfaced.

## `ToolDefinition`

Fields:

- `toolId`
- `displayName`
- `toolMode`
- `scanRange`
- `cooldown`
- `supportedEvidenceTypes`
- `uiReadoutStyle`

Purpose: defines detection tools independently of runtime item instances.

## `MatchRulesDefinition`

Fields:

- `startingSanity`
- `passiveSanityDrain`
- `ghostEventCooldownMin`
- `ghostEventCooldownMax`
- `deathEnabled`
- `guessEnabled`

Purpose: lets you change round pacing without editing code.

## `RoomDefinition`

Fields:

- `roomId`
- `displayName`
- `tags`
- `isGhostSpawnEligible`
- `evidenceAffinityTypes`

Purpose: optional metadata layer for authoring rooms beyond just trigger volumes.

## Runtime classes and responsibilities

This is the most important section for future roadmap writing: each runtime class needs a stable ownership boundary.

## `GameBootstrap`

Owns:

- persistent startup initialization
- scene loading order
- dev/test scene target selection

Does not own:

- match state
- player state
- ghost state

## `PlayerInputRouter`

Owns:

- wrapping Unity Input System actions
- exposing movement/look/interact/use/drop events

Does not own:

- movement physics
- item logic

## `PlayerMotor`

Owns:

- movement velocity
- gravity
- grounding
- sprint state if used

Does not own:

- input bindings
- look rotation
- interactions

## `PlayerLook`

Owns:

- camera pitch/yaw
- sensitivity
- cursor lock

Does not own:

- movement
- item use

## `PlayerInteractor`

Owns:

- forward raycast
- current interactable target
- prompt text source
- dispatching interaction calls

Does not own:

- actual object-specific interaction logic

## `PlayerInventory`

Owns:

- currently held item
- equip/drop/use routing
- inventory slots, if any later

Does not own:

- item visuals
- item world pickup behavior

## `HeldItemPresenter`

Owns:

- showing current held item model in view
- swapping view models or placeholders

Does not own:

- item data
- item behavior logic

## `MatchManager`

Owns:

- current round state
- active ghost definition
- win/loss resolution
- current enabled evidence set

Does not own:

- detailed ghost AI
- UI rendering details

## `GhostController`

Owns:

- current runtime ghost state
- timers
- room choice
- event requests
- hunt requests

Does not own:

- authoring data
- journal logic

## `EvidenceManager`

Owns:

- when evidence can appear
- which emitters/anchors are valid
- matching active ghost to evidence opportunities

Does not own:

- player interpretation
- journal UI

## `JournalController`

Owns:

- player-recorded evidence
- ghost guess
- deduction UI data model

Does not own:

- evidence spawning
- ghost state

## `DebugOverlay`

Owns:

- visualizing hidden state
- toggled debug panels
- current ghost state, room, timers, evidence flags

Does not own:

- actual game logic

## Interfaces

Lock in these contracts now:

```csharp
public interface IInteractable
{
    string GetPromptText();
    bool CanInteract(PlayerInteractor interactor);
    void Interact(PlayerInteractor interactor);
}

public interface IPickupItem
{
    ItemRuntimeData CreateRuntimeData();
}

public interface IUsableItem
{
    void OnUseStart();
    void OnUseStop();
}

public interface IEvidenceEmitter
{
    bool CanEmit(EvidenceDefinition evidence);
    void Emit(EvidenceDefinition evidence);
}

public interface IToolTarget
{
    void OnScanned(ToolScanContext context);
}
```

These are not sacred, but the concept is: interaction, pickup, item use, evidence emission, and tool scanning must be decoupled.

## Item model

Use three layers for items:

|Layer|Meaning|
|---|---|
|`ToolDefinition`|Static design-time item data|
|`ItemRuntimeData`|Runtime state such as active, battery, last scan|
|`ItemView` / prefab|World/view representation|

This separation is useful later when tools become more complex and possibly networked.

## Ghost model

Use three layers for ghosts too:

|Layer|Meaning|
|---|---|
|`GhostDefinition`|Static ghost type data|
|`GhostRuntimeState`|Current room, timers, hunt status, active modifiers|
|`GhostController`|Behavior execution and transitions|

This is one of the key modularity decisions. It prevents the project from turning into “every ghost is a subclass with custom spaghetti.”

## State enums

Define these early:

```csharp
public enum MatchState
{
    Booting,
    Exploring,
    Hunting,
    PlayerDead,
    RoundWon,
    RoundLost
}

public enum GhostState
{
    Dormant,
    Idle,
    Roaming,
    Interacting,
    EmittingEvidence,
    Hunting
}

public enum EvidenceType
{
    None,
    EMF,
    Temperature,
    Writing,
    Orb,
    SpiritBox,
    UV
}

public enum ToolType
{
    None,
    Flashlight,
    EMFReader,
    Thermometer,
    Journal,
    UVLight,
    SpiritBox
}
```

You may rename them later, but a stable enum set helps keep early systems coherent.

## Event flow

Use a lightweight event pattern for cross-system communication. Examples:

- `OnItemPickedUp`
- `OnItemDropped`
- `OnDoorOpened`
- `OnLightToggled`
- `OnGhostStateChanged`
- `OnEvidenceEmitted`
- `OnGhostGuessed`
- `OnPlayerDied`

This avoids giant direct references between systems and makes later debugging easier.

## Initial HUD scope

The starter HUD should only include:

- crosshair
- interaction prompt
- held item label
- optional tool readout panel
- debug toggle panel

Do not build a full production journal UI in the first pass. A temporary vertical list of found evidence and a ghost guess dropdown is enough.

## Initial room system

Each room should have:

- `RoomVolume` trigger collider
- `RoomDefinition` reference
- optional list of linked lights
- optional list of evidence anchors
- optional ghost affinity weight

`RoomRegistry` should collect all room volumes at scene start and provide helper queries like:

- get player current room
- get random ghost-eligible room
- get room by id
- get neighboring rooms later if needed

## Initial ghost authoring setup

For the very first ghost pass, the scene should include:

- one `GhostRoot`
- one `GhostController`
- several `GhostAnchorPoint` transforms
- several `EvidenceAnchor` transforms
- one `GhostDefinition` asset assigned through `MatchManager`

Do not give the ghost a visible body unless needed for testing. A hidden controller plus debug gizmos is enough at first.

## Debug requirements

Your starter implementation is not done unless the debug layer can show:

- current room player is in
- current ghost room
- current ghost state
- current ghost timers
- active evidence set
- current guessed ghost
- whether death is enabled
- whether evidence emission is currently allowed

This is mandatory, not optional. In hidden-state games, debug visibility is part of the architecture.

## Acceptance baseline for the starter spec

Before phase roadmaps begin, the starter implementation spec should support a project state where:

- the project loads into `Bootstrap`, then into `House_Graybox`
- the player can move, look, and interact
- at least one door, one light switch, and one pickup object work
- the HUD displays prompts correctly
- room volumes detect player presence
- debug overlay can be toggled
- match systems exist as placeholders even if ghost logic is not fully active yet
- `GhostDefinition`, `EvidenceDefinition`, and `ToolDefinition` asset types exist
- scene hierarchy, folder structure, and namespaces match the spec

That creates the foundation for writing precise subphases and acceptance criteria in the next step.
