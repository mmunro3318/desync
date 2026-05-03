# Run 1 — Unity Project Architecture and C# Code Organization for a Small-Team Co-op Horror Prototype

## Overview
This report covers the architecture and code-organization layer for a Unity 6.4 / C# project using the classic GameObject and MonoBehaviour workflow. It focuses on maintainability, script boundaries, ScriptableObject patterns, naming and file structure, composition-root thinking, and practical ways to avoid the Unity-specific anti-patterns that turn small prototypes into brittle scene-driven messes.[cite:62][cite:121][cite:122]

The central recommendation is to build the project around **small, narrowly scoped MonoBehaviours, data definitions separated from runtime state, explicit bootstrap/composition entry points, limited service registration, and repo-level naming/folder conventions that stay boring and predictable**. Unity’s official guidance on project organization, naming conventions, ScriptableObject-based architecture, and advanced code architecture strongly supports that direction, while community experience mainly adds warnings about overusing ScriptableObjects, over-abstracting too early, and allowing manager sprawl to replace deliberate system boundaries.[cite:62][cite:87][cite:121][cite:122][cite:159]

## Executive Summary
For this prototype, the correct architecture is not “enterprise Unity,” but it also should not be “throw scripts on prefabs until it works.” The best structure for a programmer-led horror prototype is a restrained middle path: enough organization to keep multiplayer, impossible-space logic, and hidden-state systems understandable, but not so much abstraction that every feature requires navigating six indirection layers.[cite:121][cite:143][cite:144]

The most important Unity architecture decision is where different kinds of truth live. Scene-authored objects, prefab defaults, ScriptableObject definitions, and runtime state each serve different roles, and bugs multiply when those boundaries are blurred.[cite:62][cite:85][cite:87][cite:159]

C# organization should follow normal .NET naming discipline where possible, but adapted to Unity’s editor-centric model. Unity explicitly recommends consistent naming rules and style conventions, and Microsoft’s C# naming guidance remains a useful baseline for identifiers, type names, and member conventions.[cite:122][cite:125]

For bootstrapping, the project should use an explicit startup path rather than relying on random Awake ordering across persistent objects. Unity community practice strongly favors a bootstrap or composition-root style for scene initialization in larger projects because it makes startup dependencies visible and reduces “why did this singleton initialize first?” bugs.[cite:134][cite:138][cite:140]

ScriptableObjects should be used for reusable data definitions, editor-authored configuration, and selected event/data patterns, but not as a universal replacement for every system or all runtime state. Unity itself promotes ScriptableObjects for separating data and logic, yet practical community experience consistently warns that they are easy to overuse and can create opaque coupling if treated as a magic architecture solution.[cite:85][cite:87][cite:159][cite:166]

## Architectural principles for this repo
### 1. Separate definitions from runtime state
This should be the foundational rule for the repo. Unity’s official ScriptableObject guidance emphasizes using asset-based data to separate reusable game data from logic and reduce duplication, which is valuable precisely because Unity scenes and prefabs otherwise make it easy to hide critical data in many places at once.[cite:85][cite:87][cite:159]

For this project, the cleanest split is:
- **Definitions**: ScriptableObjects and other asset data that define things such as room archetypes, interactable config, item types, puzzle rules, audio events, or observation rule definitions.[cite:85][cite:159]
- **Runtime state**: spawned object state, networked state, current room graph instance, player inventory state, lock flags, door states, and hidden-state transitions that live in runtime objects or dedicated state models.[cite:38][cite:72][cite:159]
- **Scene composition**: where references are wired and test content is placed, not where hidden system truth quietly accumulates.[cite:62][cite:159]

This matters even more in a multiplayer impossible-house game than in a simple single-player prototype. Runtime authority and synchronization become much easier to reason about if the team can answer, for every system, “What is a reusable definition?” and “What is the live authoritative state right now?”[cite:38][cite:72][cite:159]

### 2. Prefer composition over giant inheritance trees
Unity’s component model is built around attaching modular behaviors to GameObjects, and Unity’s own component documentation makes that model the default mental framework.[cite:61][cite:63] That means most gameplay design should favor composition—multiple small components cooperating on one object—rather than deep inheritance hierarchies or monolithic “master gameplay base classes.”[cite:61][cite:121]

For example, an interactable door should not start life as a massive `NetworkedObservedInteractableDoorPuzzleObjectBase`. It should more likely be composed from smaller responsibilities such as interactability, open/closed state, authority checks, animation trigger, sound playback, and optional observation-lock integration.[cite:121][cite:143][cite:149]

### 3. Optimize for inspectability and debugging
Because this project includes hidden-state systems, multiplayer authority, and shifting-space logic, architecture should optimize for “Can a tired developer understand this at 2 AM?” rather than for abstract elegance alone. Unity’s maintainability guidance and best-practice materials repeatedly push toward clearer code architecture because debugging cost is usually the real killer in medium-sized projects.[cite:121][cite:153][cite:143]

That means:
- short scripts,
- explicit naming,
- explicit ownership,
- visible serialized references,
- minimal magic initialization,
- and low ambiguity about where state changes occur.[cite:121][cite:122][cite:143]

## Recommended project architecture shape
### The practical layer model
A useful architecture for this repo is a lightweight layered model rather than a formal enterprise architecture. The project can stay in Unity’s normal workflow while still giving Claude and future human contributors clear boundaries.

A practical structure is:
- **Core domain/data definitions**: ScriptableObjects, enums, serializable config data, stable identifiers, and rule definitions.[cite:85][cite:87][cite:159]
- **Runtime systems/services**: house runtime orchestration, state registries, save/debug helpers, networking facades, spawn coordinators, event dispatchers.[cite:121][cite:38]
- **Scene/prefab behaviors**: MonoBehaviours attached to scene or prefab objects that bridge runtime systems into actual gameplay objects.[cite:61][cite:62]
- **Feature modules**: interaction, items, doors, puzzles, observation locks, room graph, portals, player controller, audio cues, debug overlay.[cite:121][cite:143]

This is intentionally not a hard technical architecture with strict package-enforced isolation on day one. It is a **conceptual architecture** that later can be tightened with assembly definitions and clearer internal APIs as the repo matures.[cite:151][cite:66]

## Folder and file organization
Unity’s project-organization guidance recommends deliberate folder structure, naming consistency, and avoiding chaotic asset placement. The specific folder shape can vary, but the main goal is that a developer should be able to predict where code, prefabs, scenes, ScriptableObjects, and test assets belong without guessing.[cite:62][cite:66]

### Recommended top-level folder strategy
A good default for this repo is to organize primarily by **feature domain**, with a few cross-cutting technical roots. Unity’s own organization guidance supports consistent, scalable organization, and community practice for larger Unity projects often converges on feature-based grouping once a project becomes more than a toy.[cite:62][cite:66]

Suggested structure:

```text
Assets/
  _Project/
    Art/
    Audio/
    Materials/
    Prefabs/
    Scenes/
      Bootstrap/
      Test/
      Gameplay/
    ScriptableObjects/
    Scripts/
      Core/
      Networking/
      Debug/
      Features/
        Player/
        Interaction/
        Items/
        Doors/
        Puzzles/
        Observation/
        SpatialRuntime/
    UI/
    Settings/
```

A stronger long-term variation is to nest assets by feature more aggressively:

```text
Assets/_Project/Features/Doors/
  Scripts/
  Prefabs/
  Data/
  Tests/
```

That approach works especially well when each gameplay feature becomes a mini-subsystem. The tradeoff is that shared assets and cross-feature dependencies need discipline so the repo does not become a maze of tiny folders.[cite:62][cite:66]

### Folder rules that should become repo standards
- Keep all first-party project content under a single root such as `Assets/_Project/`.[cite:62][cite:66]
- Separate experimental or throwaway content from production paths using `Scenes/Test`, `Sandbox`, or `Prototypes` folders.[cite:62]
- Never let package examples, imports, and asset-store content spill into core project folders unchanged; isolate them clearly.[cite:62]
- Keep scene files, prefabs, materials, and scripts in predictable locations so search friction stays low.[cite:62][cite:66]

## Naming and code style
Unity has an official naming and style guide for C# scripting, and Microsoft’s broader C# naming conventions remain the best baseline for identifiers. The project should adopt that guidance with minimal customization instead of inventing a house style from scratch.[cite:122][cite:125]

### Recommended conventions
- **PascalCase** for classes, structs, enums, public methods, and public properties.[cite:122][cite:125]
- **camelCase** for private fields and local variables, with a consistent private-field prefix only if the repo explicitly commits to one style and uses it everywhere.[cite:122][cite:125]
- Descriptive type names that encode role, not implementation trivia; e.g. `DoorInteractor`, `ObservedRoomDefinition`, `NetworkSessionCoordinator`.[cite:122]
- MonoBehaviour filenames should match class names exactly, which is effectively required by Unity workflow expectations.[cite:122]

### Style rules worth standardizing
- One public class per file unless there is a compelling tightly coupled exception.[cite:122][cite:125]
- Keep methods short enough that their purpose can be described in a single sentence.[cite:121][cite:143]
- Prefer verbs for command methods (`OpenDoor`, `RequestPickup`, `ApplyObservationRule`) and nouns for data holders (`RoomDefinition`, `ItemConfig`).[cite:122][cite:125]
- Avoid abbreviations unless they are standard and obvious in context (`UI`, `ID`, `RPC`, `NGO`).[cite:122][cite:125]

### Guidance for a MERN-background developer
The biggest mental adjustment is that Unity C# rewards **explicitness** more than JavaScript often does. Type names, serialized fields, access modifiers, and concrete responsibility boundaries carry more weight because the Editor, Inspector, serializer, and runtime lifecycle all rely on stable structure.[cite:61][cite:122][cite:125]

In practice, that means code that feels slightly “verbose” compared with JavaScript is often actually easier to debug in Unity. Short clever patterns that would feel fine in TypeScript can become opaque when mixed with serialized references, inspector wiring, scene loading, and frame-based state transitions.[cite:121][cite:122]

## How to split code responsibly
### The right size for a MonoBehaviour
Unity’s component model works best when each MonoBehaviour owns a narrow role. A common architecture failure is the “god script”: one large class that handles input, state, animation, audio, networking, UI prompts, and scene references all at once.[cite:149][cite:152]

A useful rule for this repo is that a MonoBehaviour should usually answer **one main question**. For example:
- “How does this object expose interaction?”
- “How does this object replicate a specific state?”
- “How does this object animate its visual response?”

When a script answers four different kinds of questions, it is a candidate for splitting.[cite:149][cite:152][cite:177]

### Practical split examples
Bad direction:
- `DoorController.cs` handles interaction, authority checks, animation, sound, debug labels, lock-state rules, and save/load serialization.

Better direction:
- `DoorInteractable`
- `DoorStateModel`
- `DoorAnimatorBridge`
- `DoorAudioFeedback`
- `DoorNetworkSync`
- `DoorObservationLockAdapter`

This does **not** mean every class should be microscopic. The goal is not “as many files as possible”; the goal is “a future reader can understand why this class exists without opening six others first.”[cite:121][cite:149][cite:177]

## ScriptableObjects: where they help and where they are overused
Unity explicitly promotes ScriptableObjects as a way to architect games, separate data from logic, reduce duplicated values, and enable more designer-friendly workflows. That guidance is valid and especially useful in content-driven systems.[cite:85][cite:87][cite:159]

### Good uses in this project
ScriptableObjects are a strong fit for:
- item definitions,
- room archetype definitions,
- interaction prompt config,
- audio event/config data,
- puzzle definitions,
- observation-rule definitions,
- tuning values for movement, stamina, fear, or UI timing.[cite:85][cite:87][cite:159]

### Bad uses in this project
They should **not** become the default place for mutable live gameplay state such as:
- current door open state,
- current player inventory contents,
- current network authority decisions,
- currently active runtime room graph instance,
- or any state that must be per-session, per-instance, or per-match.[cite:159][cite:166]

### Why overuse happens
ScriptableObjects are seductive because they are easy to create, inspect, reference, and share. But community experience repeatedly points out that teams can accidentally turn them into global mutable state containers, hidden dependency hubs, or pseudo-singletons that obscure ownership and make runtime behavior harder to reason about.[cite:166][cite:163][cite:165]

### Repo rule
Use ScriptableObjects for **definitions and selected event/config channels**, not as a universal architecture pattern. Every time one is introduced, the author should be able to answer: “Why is this asset data rather than runtime instance state?”[cite:85][cite:87][cite:166]

## Composition root and bootstrap patterns
### Why bootstrap explicitly
Unity lets code initialize in many implicit ways: `Awake`, `OnEnable`, scene load order, persistent objects, and `DontDestroyOnLoad` behaviors. That flexibility becomes dangerous once networking, save systems, debug systems, or service registration start depending on startup order.[cite:134][cite:138][cite:140]

An explicit bootstrap scene or composition-root object reduces that ambiguity. Community practice around larger Unity projects often uses a small bootstrap scene that initializes global systems, loads the next scene intentionally, and makes startup dependencies visible rather than accidental.[cite:138][cite:140][cite:141]

### Recommended bootstrap model for this repo
Use a **Bootstrap scene** containing a minimal startup graph responsible for:
- registering core services,
- creating persistent systems that truly must persist,
- configuring debug/developer systems,
- initializing networking/session flow entry points,
- and then loading the menu, test, or gameplay scene intentionally.[cite:138][cite:140][cite:176]

That scene should stay tiny and boring. If it becomes a second hidden game scene packed with gameplay objects, the architecture has already drifted in the wrong direction.[cite:138][cite:140]

### Composition-root rule
The bootstrap layer should be the only place where “big wiring decisions” are made. Scene objects and prefabs should consume established services or references; they should not each invent their own startup logic.[cite:134][cite:138]

## Services, managers, singletons, and sprawl
Unity projects often drift into manager sprawl because managers appear to solve early coordination problems quickly. The danger is that a project ends up with `GameManager`, `AudioManager`, `UIManager`, `NetworkManager`, `InteractionManager`, `PuzzleManager`, `RoomManager`, and `StateManager`, all cross-referencing each other through globals or scene lookups.[cite:129][cite:130][cite:133]

### Recommended stance
Do not ban services entirely, but make them earn their place. A service should exist when it truly coordinates a cross-scene or cross-feature concern, not because a script felt “important.”[cite:121][cite:129]

Good candidates:
- session coordination,
- runtime house orchestration,
- debug overlay routing,
- save/checkpoint coordination,
- centralized event tracing.[cite:121][cite:38]

Bad candidates:
- managers that merely proxy one object to another,
- managers created to avoid passing references properly,
- “manager” classes with no clear ownership or lifecycle.[cite:129][cite:130]

### Singletons vs service locator vs explicit references
Community discussion does not yield a single perfect answer, but it does strongly converge on one warning: **uncontrolled globals create invisible coupling**.[cite:129][cite:131]

For this repo, a practical stance is:
- Prefer serialized references or constructor/setup injection where reasonable.[cite:121][cite:129]
- Allow a small number of bootstrap-registered services for true app-level concerns.[cite:134][cite:138]
- Avoid reaching for singletons as the default answer to every reference problem.[cite:129][cite:130]

This is less about academic purity and more about preserving debuggability. In a multiplayer horror prototype, hidden coupling is deadly because bugs can already come from scene state, authority, timing, and runtime transitions.[cite:38][cite:129]

## Events, delegates, and UnityEvents
Event-driven decoupling is useful in Unity, but it should be used with discipline. Community discussion around Unity events consistently emphasizes trade-offs between native C# events/delegates and UnityEvents, especially around performance, discoverability, and Inspector wiring.[cite:135][cite:137][cite:139]

### Practical recommendation
- Use **plain C# events/delegates** for internal code-to-code communication where the event contract is part of the implementation model.[cite:135][cite:139]
- Use **UnityEvent** sparingly where Inspector wiring genuinely improves iteration and the event is simple enough to benefit from designer-visible hookups.[cite:137]
- Avoid building core game logic around large webs of Inspector-wired UnityEvents that are hard to trace in code review.[cite:137][cite:139]

For this project specifically, networking, authority, spatial runtime state, and hidden-state transitions should favor explicit code paths over event spaghetti. Debugging those systems will already be hard enough.[cite:38][cite:139]

## Assembly definitions and modularization
Unity’s manual documents assembly definition files (`asmdef`) as a way to control how code is compiled into separate managed assemblies. This can improve compile times and help enforce modular boundaries once a project becomes large enough to benefit from that structure.[cite:151]

### Recommended stance for this project
Use assembly definitions, but not too early and not everywhere. They are helpful once the repo has stable feature groupings such as `Core`, `Networking`, `Debug`, `Player`, and `SpatialRuntime`, but premature fragmentation can make a young codebase harder to navigate.[cite:151][cite:154]

A good milestone for introducing them is when:
- compile times are starting to hurt,
- feature boundaries are becoming stable,
- and the team wants to prevent casual cross-feature coupling.[cite:151][cite:154]

## Anti-patterns to avoid
### 1. Giant god scripts
These are the fastest path to fragile code. They hide multiple responsibilities in one file and make networking, testing, and refactoring painful.[cite:149][cite:152]

### 2. ScriptableObject-as-everything architecture
This creates hidden global state, unclear ownership, and confusion between editor-time definitions and per-session runtime state.[cite:159][cite:166]

### 3. Manager sprawl
A dozen globally reachable managers create coupling that no one can reason about locally.[cite:129][cite:130]

### 4. Startup by accident
If the project depends on implicit `Awake` ordering or scene-load coincidences, it will become unstable as soon as networking and persistent systems grow.[cite:134][cite:138][cite:140]

### 5. Folder entropy
When assets land “wherever there was room,” everything slows down: onboarding, searching, testing, refactoring, and AI-assisted implementation all become worse.[cite:62][cite:66]

### 6. Over-abstracting too early
Unity’s pattern guides and SOLID materials are useful, but forcing every feature through over-designed interfaces before the game’s actual needs are known can be as damaging as no architecture at all.[cite:143][cite:144][cite:150]

## Recommended defaults for this repo
### What Claude should generally do
- Create small MonoBehaviours with one clear responsibility.[cite:61][cite:121]
- Separate asset definitions from live runtime state.[cite:85][cite:159]
- Put startup wiring in a bootstrap/composition-root path.[cite:138][cite:140]
- Use consistent naming and file conventions aligned with Unity and C# standards.[cite:122][cite:125]
- Prefer explicit references and clear ownership over magic global access.[cite:129][cite:121]
- Introduce ScriptableObjects only when asset-based data is genuinely the right fit.[cite:85][cite:87]

### What Claude should generally avoid
- Giant all-purpose controller scripts.[cite:149][cite:152]
- Implicit singleton-heavy wiring by default.[cite:129][cite:130]
- Treating ScriptableObjects as runtime state buckets.[cite:159][cite:166]
- Creating new managers for local problems that should be solved inside a feature module.[cite:129][cite:130]
- Building abstractions before a concrete need exists.[cite:143][cite:150]
- Naming classes vaguely (`GameSystem`, `UtilityManager`, `Handler`, `Processor`) when the role can be stated directly.[cite:122][cite:125]

## Tailored recommendation for the impossible-house prototype
The project’s unusual features—graph-based spatial runtime, portals/shifting geometry, hidden-state rules, and multiplayer synchronization—mean architecture must prioritize **traceability** over cleverness. A future bug will rarely be “the code was not abstract enough”; it will usually be “nobody could tell where the runtime truth lived or why this state changed.”[cite:38][cite:159][cite:121]

That means the repo should explicitly model:
- definitions versus instances,
- authoritative state versus visual state,
- bootstrap wiring versus gameplay behavior,
- and feature-local logic versus cross-feature orchestration.[cite:159][cite:176][cite:121]

If those distinctions stay crisp, later multiplayer, scene, prefab, and debugging work will be far easier. If they blur early, every later system will be built on uncertain ground.[cite:38][cite:121][cite:159]

## Conclusion
Unity’s official guidance supports a maintainable small-team architecture built on disciplined organization, consistent naming, ScriptableObject-based data separation, and deliberate code structure rather than ad hoc scene wiring.[cite:62][cite:87][cite:121][cite:122] Community practice adds the most value by warning against the failure modes Unity beginners hit repeatedly: god scripts, manager sprawl, accidental startup order, and ScriptableObject overuse.[cite:129][cite:140][cite:149][cite:166]

For this repo, the best default is clear: use MonoBehaviour composition as the gameplay layer, keep runtime state separate from asset definitions, wire the app through an explicit bootstrap path, adopt boring and consistent naming/folder rules, and force every system to make ownership visible. That is the architecture most likely to let Claude generate useful code without turning the project into a mystery box.[cite:61][cite:122][cite:138][cite:159]
