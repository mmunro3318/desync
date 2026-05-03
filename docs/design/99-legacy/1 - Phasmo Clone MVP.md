Before iteration and development on new features or new direction with our version of the game, we need to develop a near clone of the original Phasmophobia in core game logic and flow. Then we can build upon it.

Start even **earlier** than “one ghost, one evidence”. For this kind of game, the safest roadmap is: movement and interaction first, then a minimal ghost loop, then evidence, then ghost taxonomy, then the full clone. That sequence lowers risk, gives you playable checkpoints fast, and keeps the architecture modular for later mutation.

## Build order

The project should grow in five layers, each proving one major question before you invest in the next:

|Phase|Goal|Main question answered|
|---|---|---|
|0|Walk around a house and touch things|Does basic first-person feel work?|
|1|One ghost, one evidence, win/loss loop|Is the core hunt/investigate loop fun?|
|2|Three evidence types, baseline ghost behavior|Does evidence gathering create tension and deduction?|
|3|Full core ghost/evidence matrix|Can the clone support modular ghost definitions cleanly?|
|4|Distinct ghost-type behavior|Can you extend the clone without breaking the base systems?|

That ordering matches your stated milestones and also protects you from the classic trap of overbuilding ghost logic before the player controller, interaction model, and room feel are enjoyable. [epicgames](https://dev.epicgames.com/documentation/en-us/unreal-engine/overview-of-blueprints-visual-scripting-in-unreal-engine)

## Foundation first

Before any ghost exists, you need a small set of foundation systems that almost every later milestone depends on. In Unity terms, I’d build these as reusable components plus shared data assets, because ScriptableObjects are specifically designed to store shared data independently from scene instances.unity+1

Build these first:

- First-person controller: move, look, sprint if needed, crouch only if truly necessary later.
- Interaction system: raycast focus, interact prompt, pickup/drop/use, door/light/switch handling.
- Item-in-hand system: player can equip one object, use it, inspect it, drop it.
- Basic world state: rooms, lights, doors, hiding spots later if needed.
- Minimal HUD: crosshair, interaction hint, held item label, debug readouts.
- Match bootstrap: spawn player into house, no menu flow required yet.

The architectural rule should be: scene objects stay dumb; reusable behavior lives in components; tunable rules live in data assets. That will make ghost and evidence expansion much easier later.unity3d+1

## Milestones

## Phase 0

Start with “drop player in house” exactly as you suggested. The whole milestone is successful when movement feels decent, interaction is readable, and the house supports doors, lights, and picking up at least one object.

Systems needed:

- Player controller.
- Camera/look/input layer.
- Interactable interface, for example `IInteractable`.
- Pickup system.
- Simple held-item presentation.
- Core environment prefabs: doors, switches, loose props.
- Debug UI.

You should not add ghost logic here. The milestone ends when the house feels like a place where a ghost **could** exist.

## Phase 1

This is your first real “game.” Add one ghost, one evidence type, and one win/loss condition. The point is not accuracy; the point is proving the emotional loop of entering, detecting, confirming, surviving, and resolving.

Systems needed:

- Ghost controller with a tiny state machine, for example Idle, Roam, EvidenceEvent, Hunt.
- Ghost room/anchor concept.
- One evidence pipeline, such as “ghost can trigger EMF event” or equivalent.
- One investigation tool.
- Death or fail state.
- Guess/resolve interaction, maybe a simple debug terminal or notebook button rather than a full journal UI.
- Match manager tracking win/loss.

This is the first milestone where you learn whether your loop is fun. If this isn’t fun, more ghost types won’t save it.

## Phase 2

Now add three evidence types and flesh out baseline ghost behavior, but still keep ghosts generic. Think “a ghost can produce allowed evidence under certain conditions,” not “this is a Revenant with a bespoke personality.”

Systems needed:

- Evidence manager.
- Evidence event spawn/trigger model.
- Tool detection system, one component per tool or detector mode.
- Ghost activity scheduling, so evidence doesn’t feel purely random.
- Room affinity and movement logic.
- Optional sanity/threat pressure if needed for pacing.
- Better notebook or journal UI for tracking findings.

At this stage, the ghost should be a mostly generic machine with configurable evidence permissions. That gives you the clone’s deduction skeleton without forcing full ghost taxonomy too early.

## Phase 3

This is where the **main clone** takes shape: full evidence logic and ghost types defined by their core evidence combinations. Keep behavior differences minimal at first; ghost identity mainly comes from allowed evidence and shared baseline activity rules.

Systems needed:

- Ghost definition data assets, one per ghost type.
- Evidence combination validation.
- Match generation rules that pick a ghost type and instantiate allowed evidence set.
- Journal deduction UX.
- Difficulty/config tuning layer.
- Better event logging/debugging so you can inspect why a ghost did or didn’t emit evidence.

This phase succeeds when adding a new ghost type mostly means creating a new data asset, selecting evidence rules, and lightly tuning parameters rather than writing new code.unity+1

## Phase 4

Now mutate ghost behavior into specific ghost types. This is where ghosts stop being “same machine, different evidence table” and become “same core framework, different modifiers and edge-case rules.”

Systems needed:

- Ghost behavior traits/modifiers, such as roam bias, hunt threshold bias, interaction frequency bias, deception bias.
- Rule override layer, so a ghost can alter the default evidence/hunt/activity logic.
- Special event hooks for future unique content.
- Balancing tools and debug panels.
- Regression tests and playtest checklist, because this is where complexity starts compounding.

The design goal is composability: ghost type = base ghost rules + evidence set + behavior modifiers + optional special rules. That keeps your system extensible instead of turning into giant `switch` statements.

## System sketch

Here’s the high-level modular architecture I’d aim for in Unity:

|System|Responsibility|Should be data-driven?|
|---|---|---|
|PlayerController|Movement, look, input mapping|Mostly no|
|InteractionSystem|Focus, use, pickup, drop|Mostly no|
|ItemSystem|Held items, item use, tool activation|Partly|
|ToolDefinition|What a tool detects, how it behaves|Yes [unity](https://unity.com/how-to/architect-game-code-scriptable-objects)|
|GhostController|Runtime ghost state machine|No|
|GhostDefinition|Ghost type config, evidence, behavior params|Yes unity3d+1|
|EvidenceSystem|Spawning, validating, exposing evidence|Mixed|
|MatchManager|Start, fail, win, round state|No|
|Journal/UI|Player-facing deduction and readouts|Partly|
|House/RoomSystem|Rooms, anchor points, spawn points, ghost favorite room|Partly|

Two especially important patterns:

- `GhostDefinition` should be a ScriptableObject or equivalent shared data asset.unity3d+1
- Interactions should flow through interfaces/events rather than hard references whenever possible, because that will keep tools, props, and evidence sources decoupled. [epicgames](https://dev.epicgames.com/documentation/en-us/unreal-engine/overview-of-blueprints-visual-scripting-in-unreal-engine)

## Scope guidance

You do **not** need these at the beginning:

- Main menu.
- Level select.
- Truck/lobby flow.
- Progression economy.
- Multiplayer lobby flow, matchmaking UI, and voice chat (network-aware architecture must still be in place from day one — see Unity Research/2 - Unity Setup.md).
- Polished match-end screens.

A totally valid first playable is:

1. Press Play.
2. Spawn in test house.
3. Walk, open doors, toggle lights, pick up one detector.
4. Ghost occasionally emits one evidence.
5. Player identifies it or dies.

That is enough to validate the game’s backbone.

## Recommended roadmap

I’d structure the roadmap like this:

- **Milestone A: House sandbox**
    
    - Player controller
    - Look/input
    - Door/light/object interactions
    - Pickup/drop
    - Tiny HUD/debug overlay
    
- **Milestone B: Minimal haunting**
    
    - One ghost
    - One room anchor
    - One evidence type
    - One tool
    - One win condition
    - One fail/death condition
    
- **Milestone C: Investigation loop**
    
    - Three evidence types
    - Three tools
    - Basic journal
    - Generic ghost evidence scheduling
    - Better pacing/activity rules
    
- **Milestone D: Main clone**
    
    - Full ghost definitions
    - Full evidence matrix
    - Match generator
    - Accurate baseline ghost logic
    - Journal deduction flow
    
- **Milestone E: Ghost identity**
    
    - Per-ghost behavior modifiers
    - Special ghost tendencies
    - Balance pass
    - Content hooks for later original mechanics
    

That gives you a very clean expansion path: each milestone adds one axis of complexity without forcing you to build everything at once.

## Design rules

A few rules will save you pain later:

- Keep ghost **data** separate from ghost **runtime state**. One defines what a ghost type is; the other tracks what this particular ghost is doing in this match.
- Evidence should not be hardcoded into ghost scripts. Ghosts should expose capabilities; the evidence system should orchestrate manifestation.
- Tools should detect categories/events, not individual ghost classes.
- Avoid giant manager gods early; use a few clear systems with narrow roles.
- Build debug UI from the start, because hidden-state games become miserable to tune without visibility.

The most important rule is the one you already identified: design for modular iteration. In practice, that means “new ghost = mostly new data,” “new evidence = new detector/evidence component pair,” and “new mechanic = opt-in system layered onto the existing loop.”

My recommendation is to begin with a **vertical slice of Milestone A only** this week: one graybox house, one player controller, one interaction prompt, one pickup object, one door, one light switch. Once that feels clean, Milestone B becomes dramatically easier.
