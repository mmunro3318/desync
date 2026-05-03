Below is a concrete Unity project structure you can use as a starting blueprint for the clone. The guiding principle is to keep runtime behavior in focused systems and keep ghost, tool, and evidence tuning in data assets so you can add content without rewriting core code.unity+1

## Folder layout

Use a structure like this inside `Assets/`:


```text
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


This kind of separation keeps content, data, runtime code, and scenes from turning into one giant pile. ScriptableObject-heavy categories like ghosts, evidence, and tools belong in `Data/`, while scene instances and reusable object setups belong in `Prefabs/`.

## Core scenes

Start with only four scenes:

- `Bootstrap`: initializes systems, loads the active gameplay scene, and can later become the place for menu flow.
- `Test`: tiny sandbox scene for fast script validation.
- `House_Graybox`: your first playable movement/interactions scene.
- `House_Prototype`: the evolving real prototype house.

You do not need menu scenes yet. The main value of `Bootstrap` is that it gives you a stable place to initialize persistent managers later without hacking them into the prototype house.  [epicgames](https://dev.epicgames.com/documentation/en-us/unreal-engine/overview-of-blueprints-visual-scripting-in-unreal-engine)

## Key scripts

Here’s the first-pass class map I’d recommend:

|Script/Class|Purpose|
|---|---|
|`GameBootstrap`|Initializes shared services and loads the target scene.|
|`PlayerInputRouter`|Wraps Unity Input System actions and exposes clean game-facing events.|
|`PlayerMotor`|Handles movement, gravity, sprint, crouch if needed, and grounding.|
|`PlayerLook`|Mouse/controller look and camera rotation.|
|`PlayerInteractor`|Raycasts for interactables, shows prompts, triggers use/pickup.|
|`PlayerInventory`|Tracks held/equipped item(s).|
|`HeldItemPresenter`|Displays the currently held item in-hand.|
|`InteractableBase`|Shared component base for world interactions.|
|`DoorInteractable`|Open/close behavior.|
|`LightSwitchInteractable`|Toggles linked lights.|
|`PickupItem`|Lets world items be collected and held.|
|`MatchManager`|Owns current round state, win/loss, active ghost, and session flow.|
|`GhostController`|Runtime ghost brain, state machine host, and event scheduler.|
|`GhostStateMachine`|Controls roam, manifest, evidence, hunt, idle transitions.|
|`EvidenceManager`|Owns evidence spawning/validation logic for the current match.|
|`JournalController`|Tracks player discoveries and ghost guesses.|
|`HUDController`|Crosshair, prompts, held-item label, sanity/debug readout.|
|`DebugOverlay`|Shows ghost room, current state, evidence cooldowns, and tuning values.|
|`RoomVolume`|Defines room bounds and metadata for ghost activity.|

This keeps responsibilities narrow. For example, `GhostController` should not own UI, inventory, or room authoring logic.

## Data assets

Your modularity lives here. I’d define these ScriptableObject types early:

|Data asset|What it stores|
|---|---|
|`GhostDefinition`|Ghost name, allowed evidence, baseline behavior parameters, hunt thresholds, roam tendencies.|
|`EvidenceDefinition`|Evidence type, spawn rules, visibility rules, required tool, cooldown logic.|
|`ToolDefinition`|Tool name, detection mode, range, update rate, UI behavior, evidence compatibility.|
|`MatchRulesDefinition`|Global round timing, sanity drain, ghost event pacing, fail rules.|
|`RoomDefinition`|Optional metadata for room type, spawn tags, evidence affinities.|
|`GhostBehaviorModifier`|Small reusable behavior deltas for later unique ghost personalities.|

This is the heart of “easy expansion and tweaking.” A ghost type should mostly be a `GhostDefinition` asset, not a new giant subclass every time.

## Interfaces and contracts

Use interfaces aggressively for interactions and detection. I’d start with these:

```csharp
public interface IInteractable {     
	string GetPrompt();    
	void Interact(PlayerInteractor interactor); 
} 

public interface IPickupable {     
	ItemInstance CreateItemInstance(); 
} 

public interface IEvidenceEmitter {     
	EvidenceType EvidenceType { get; }    
	bool CanEmit();    
	void Emit();
}

public interface IToolReadable {     
	void OnScanned(ToolContext context); 
}
```

The exact signatures can change, but the point is to avoid hardcoding “player knows how every object works” or “tool knows every ghost type.” Interfaces keep the clone flexible as you add more content. [epicgames](https://dev.epicgames.com/documentation/en-us/unreal-engine/overview-of-blueprints-visual-scripting-in-unreal-engine)

## Prefab strategy

You want prefabs at three levels:

- **Environment prefabs**: doors, switches, lamps, rooms, hiding spots later.
- **Interaction prefabs**: generic pickup base, readable objects, toggles, trigger zones.
- **Gameplay prefabs**: tools, ghost manifestation anchors, evidence emitters, ghost spawn points.

A good rule is: if you place it more than twice, make it a prefab; if you tune it in many scenes, back it with a data asset too. That helps you keep the house scene lightweight and prevents scene-level drift.

## Phase-by-phase ownership

Here’s what each milestone should add to the structure.

## Milestone A

Build only these folders/classes in earnest:

- `Scripts/Player`
- `Scripts/Interaction`
- `Scripts/World/Doors`, `Lights`, `Props`
- `Scripts/UI/HUD`, `Debug`
- `Scenes/House_Graybox`
- `Prefabs/Environment`, `Interactables`, `Items`

Classes to finish first:

- `PlayerInputRouter`
- `PlayerMotor`
- `PlayerLook`
- `PlayerInteractor`
- `PlayerInventory`
- `DoorInteractable`
- `LightSwitchInteractable`
- `PickupItem`
- `HUDController`

That gets you a house, controls, object interaction, and held items.

## Milestone B

Add:

- `Scripts/Ghost/Runtime`
- `Scripts/Ghost/States`
- `Scripts/Match/Flow`
- `Scripts/Items/Tools`
- `Data/Ghosts`
- `Data/Evidence`
- `Data/Tools`

Classes to add:

- `GhostController`
- `GhostStateMachine`
- `GhostDefinition`
- `EvidenceDefinition`
- `ToolDefinition`
- `MatchManager`

That gives you one ghost, one evidence, one tool, and one win/loss loop.

## Milestone C-D

Expand:

- `Scripts/Ghost/Evidence`
- `Scripts/Match/Rules`
- `Scripts/UI/Journal`
- `Data/Match`
- `Data/Rooms`

Classes to add:

- `EvidenceManager`
- `JournalController`
- `MatchRulesDefinition`
- `RoomVolume`
- `RoomRegistry`

This is where the clone becomes properly deductive and content-driven.

## Milestone E

Add:

- `Scripts/Ghost/Behaviors`
- `Data/Ghosts/Modifiers`

Classes to add:

- `GhostBehaviorModifier`
- `BehaviorRuleEvaluator`
- Optional specialized state helpers, not giant ghost subclasses

This is the phase where ghost identity becomes composable instead of hardcoded.

## Recommended namespaces

Use namespaces early so the project stays sane:

csharp

`ProjectName.Core ProjectName.Player ProjectName.Interaction ProjectName.World ProjectName.Items ProjectName.Ghost ProjectName.Match ProjectName.UI ProjectName.Audio ProjectName.Editor`

That makes AI-assisted code generation easier too, because files have clear ownership and class names collide less often.

## First concrete scripts

If I were sequencing your first implementation pass, I’d do it in this order:

1. `PlayerInputRouter`
2. `PlayerMotor`
3. `PlayerLook`
4. `PlayerInteractor`
5. `InteractableBase` + `IInteractable`
6. `DoorInteractable`
7. `LightSwitchInteractable`
8. `PickupItem`
9. `PlayerInventory`
10. `HeldItemPresenter`
11. `HUDController`
12. `DebugOverlay`

Only after that:

1. `MatchManager`
2. `GhostDefinition`
3. `GhostController`
4. `EvidenceDefinition`
5. `ToolDefinition`
6. `EvidenceManager`
7. `JournalController`

That order matches your own instinct: house first, interaction second, ghost third.

## Design rules

A few concrete rules for this structure:

- `GhostDefinition` is immutable match setup data; `GhostController` is per-match runtime state.
- Tools read evidence categories, not ghost classes.
- Evidence emission should be event-driven and schedulable, not hardcoded inside every ghost state.
- `MatchManager` should know round flow, but not the low-level behavior of doors, items, or UI widgets.
- Every major hidden system should expose values in `DebugOverlay`, because tuning a ghost game without debug visibility is brutal.

```txt

```