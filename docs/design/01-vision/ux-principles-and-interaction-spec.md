# UX Principles and Interaction Spec

## Document objective
Define the player-facing interaction rules, HUD behavior, information policy, and control readability standards for the multiplayer-first spatial horror prototype. This document translates the Player Experience Pillars into implementation-facing UX rules for controls, prompts, item handling, objective messaging, death flow, and debug separation [file:12][file:13][file:14][file:15][cite:31][cite:32].

## Why this doc exists
The game’s desired feel depends on doing two things at once: keeping the screen sparse and exposed, while making every core interaction reliable enough that fear comes from the house rather than from clumsy controls. Earlier project docs already emphasized readable interaction, minimal HUD scope, debug visibility, and repeated-run stability; this document adapts those same principles to the new multiplayer-first, no-map, breadcrumb-driven design [file:1][file:12][file:13][file:14][file:15].

## UX core principle
Player-facing UX should be **minimal, legible, and withholding**. The interface should not comfort the player with constant state explanation, but it must also never hide the basic verbs needed to move, touch, mark, carry, communicate, and survive [file:12][file:13][file:15][cite:31].

## UX principles

### 1. Clarity of verb, ambiguity of world
The player should always know what action they are currently able to perform, but not always understand the full meaning of the surrounding space. Interaction clarity must stay high even while environmental certainty remains low [file:12][file:13][file:15].

### 2. Reveal information only at point of need
Persistent HUD should be kept extremely thin. Information should appear when a player targets something, equips a relevant item, changes session phase, or enters a meaningful state transition.

### 3. Environment first, interface second
The house, player marks, sound, voice, and object placement should do more explanatory work than UI panels. The UI exists to support action, not to become the primary surface of play [cite:31][file:12].

### 4. Multiplayer truth is partial
No player should receive a fully authoritative interface view of the entire session. Each player sees local prompts, local surroundings, their own item state, and globally important session cues only when absolutely necessary [cite:31][cite:32].

### 5. Debug is mandatory, but separate
Because the project depends on hidden state, debug tools must be rich and always available in development. However, debug must remain clearly separate from fiction-facing UX so the game can preserve vulnerability and uncertainty during real play [file:13][file:15].

## Player-facing HUD policy
The default player-facing HUD should contain only these core elements:
- context reticle,
- interaction prompt,
- minimal item readout when relevant,
- temporary objective/status cards when a phase changes,
- and optional voice activity indicators only if needed later.

Avoid permanent maps, objective lists, minimaps, compass ribbons, teammate outlines through walls, or constant tutorial text. Those would undermine the intended experience of exposed navigation and partial information [cite:31].

## Reticle and target affordance

### Rule
The reticle should appear **only** when the player is hovering a valid interactable target. It should not remain visible at all times [cite:31].

### Why
This keeps the screen visually sparse while still giving the player a clean non-invasive cue that something can be touched, opened, picked up, or used. It also avoids the problem of players not knowing what in the environment is actually actionable.

### Behavior spec
- No default center reticle when looking at empty space.
- When `PlayerInteractor` detects a valid interactable, a small reticle fades in at screen center [file:13][file:14].
- The reticle should remain visually restrained: small, neutral, and not arcade-like.
- The reticle may subtly change state for categories such as usable, pickupable, blocked, or unavailable, but these distinctions should remain low-noise.
- If an object is targetable but currently invalid, the reticle may still appear in a muted form to communicate “yes, but not now.”

## Interaction prompt

### Rule
Prompt text appears only when an interactable is in focus and should be anchored close to the reticle or in a minimal center-lower screen zone.

### Prompt content
Prompt strings should be short and verb-first:
- `Open Door`
- `Pick Up Chalk`
- `Turn On Light`
- `Place Chem Light`
- `Read Note`
- `Cannot Carry More`

### Prompt behavior
- Prompts should vanish immediately when focus is lost.
- Prompts should never stack into a cluttered list.
- Only the highest-priority valid target should surface a prompt.
- If an interaction is blocked, the prompt should explain why in as few words as possible.

This remains consistent with the earlier `PlayerInteractor` ownership model, where interaction logic is object-specific but target detection and prompt surfacing are centralized [file:13][file:14].

## Core interaction verbs
The MVP interaction model should support a very small, reliable verb set:
- move,
- look,
- interact,
- use held item,
- drop held item,
- inspect/mark if needed later,
- push-to-talk for voice,
- and open pause/settings.

Do not overload the first MVP with too many bespoke verbs. Earlier planning repeatedly favored narrow, stable slices over broad but messy control surfaces [file:12][file:15].

## Pickup, hold, use, drop

### Pickup
A pickup should be fast, readable, and forgiving. The player hovers a valid item, sees the reticle + prompt, presses interact, and the item transitions into inventory/held state with minimal friction [file:13][file:15].

### Hold
Held items should be visible enough to communicate what the player is carrying, but should not dominate the screen. The held model should support atmosphere without obstructing the central field of view [file:13].

### Use
Use behavior should be consistent across tools wherever possible:
- press or hold to activate based on tool type,
- present feedback through the item itself first,
- surface UI readout only when the tool genuinely needs one.

### Drop
Drop should be immediate and predictable. In a navigation-horror game, dropped items may become orientation anchors, emergency caches, or accidental losses, so physical placement needs to feel dependable.

## Item readouts
Item UI should be item-scoped, not globally persistent. A flashlight, chalk, spray can, sensor, or future detector may each expose small local readouts when equipped, but these should disappear when the item is not relevant [file:13][file:15].

### Guidance
- Default to no extra panel.
- Add readout only if the tool is unreadable without it.
- Prefer diegetic or near-diegetic feedback, such as light, sound, mesh state, or animation, before adding HUD widgets.
- If a panel exists, keep it compact and tied to the item state.

## Objective communication

### Pre-run clarity
The macro goal should be explained before the session begins, likely in menu/tutorial text or staging language. Players should begin the run already knowing the broad purpose [cite:31].

### In-run policy
During the run, objective communication should be sparse:
- a brief start card or message,
- short state-change messages when entering a new session phase,
- and no persistent checklist pinned to the screen.

### Example
- `Locate the anchors.`
- `The house is destabilizing.`
- `Return to the exit.`

This preserves loop clarity without undermining the intended vulnerability [file:12][file:1][cite:31].

## No-map policy
Players should not have access to an in-run map. There should be no minimap, automap, route line, or omniscient floor plan [cite:31].

### Allowed substitutes
Navigation support should instead come from:
- player memory,
- voice coordination,
- environmental recognition,
- room naming/signage if present,
- and player-authored breadcrumb tools such as chalk, spray paint, chemlights, or similar markers [cite:31].

### Developer exception
A full graph/debug map is allowed for development and testing only. It should be explicitly gated behind debug tooling and never leak into player-facing UX [cite:31][file:13][file:15].

## Breadcrumb tools UX
Breadcrumb tools are core UX, not side flavor.

### Required qualities
- fast to deploy,
- easy to read in low light,
- visually distinct from ambient clutter,
- cheap enough to use habitually,
- but limited enough to force decision-making.

### Example classes
- chalk or spray marks for walls/doors,
- chemlights for persistent route markers,
- dropped supplies as improvised memory anchors,
- future tech items such as directional probes or mapping aids.

### UX rule
A breadcrumb tool should help with orientation without becoming a disguised minimap. It should preserve local uncertainty while rewarding proactive marking behavior [cite:31].

## Multiplayer communication UX
Communication should be verbal and diegetic through localized in-game voice chat, not through ping systems or explicit tactical markers [cite:31].

### Consequences
- No default ping wheel in MVP.
- No teammate waypoint projection system.
- No shared magical objective pointer.
- Information transfer between players should come primarily from speech and co-presence.

### Why
This keeps witnessing, contradiction, panic, and trust central to co-op play instead of outsourcing teamwork to UI [cite:31][cite:32].

## Session-state messaging
The session flow doc defines clear state changes across staging, exploration, final resolution, escape, win, loss, and restart. UX should acknowledge these transitions, but only briefly and clearly [file:1][file:12].

### Rules
- On state change, show one short message card.
- Avoid cinematic overlays that block vision for long durations.
- Never spam repeated reminders every few seconds.
- End-state messages should be concise and emotionally appropriate.

## Death and post-death UX

### MVP death
For MVP, death should be simple and legible:
- the player dies,
- the body remains in the world,
- direct interaction ends,
- and the surviving session continues [cite:31].

The interface at death should communicate finality clearly, with minimal confusion about whether the player can still act.

### Preferred roadmap
The preferred expansion path is a ghost-camera spectator mode with continued local voice and a spirit-world visual treatment. That future mode should keep dead players engaged without turning death into a power-up [cite:31].

### Future UX constraints for spectator mode
- the dead player should understand they are no longer physically interactive,
- the world presentation should shift enough to feel altered,
- voice should continue in a degraded or spectral way,
- and any future spirit-only threats should be legible within that altered mode.

## Win/loss feedback
Earlier docs already treated win/loss clarity as mandatory. The UX implication is simple: outcomes must be immediately readable, but they do not need to be loud or ornate [file:1][file:12].

### Win
Success should feel tense, earned, and slightly exhausted rather than triumphant.

### Loss
Failure should feel tragic and legible, not arbitrary. The player should be able to say what likely went wrong, even if they lacked full information.

## Multiplayer-first readability rules
Because the game is multiplayer-first from the beginning, interaction UX should obey these constraints [cite:31][cite:32]:
- every core action should remain readable when other players are present,
- no essential action should require solitary perfect information,
- players should be able to infer what others are doing through body language, sound, held items, and speech,
- and no player should receive total strategic certainty through the interface alone.

## Input and control quality
Even though full accessibility is not a first-class doc priority yet, controls must feel fair and low-friction. Earlier docs already emphasized that movement and interaction must feel good before deeper systems matter [file:12][file:15].

### Minimum standards
- interaction raycast must feel generous enough to avoid frustration,
- pickup targets should not require pixel-hunting,
- held-item use should respond consistently,
- and drop behavior should not create accidental loss through sloppy placement.

The original Phasmophobia-style item friction is specifically something to avoid here [cite:31].

## Audio as UX
In this project, audio is not just atmosphere. It is also interface support.

### Audio should help communicate
- whether voice is present or absent,
- whether a space feels occupied or altered,
- whether a nearby marker or tool is active,
- and whether the player’s attention should turn toward a change in the environment.

Audio should support orientation, but not replace it with explicit certainty.

## Debug UX separation
The project requires strong developer-facing debug because hidden-state games are otherwise miserable to tune. That should include graph state, room state, session state, anomaly state, objective progress, and target evaluation, but none of that should bleed into normal play presentation [file:12][file:13][file:15].

### Debug controls should expose
- current session state [file:1],
- player room/node,
- target interactable info,
- held item runtime state,
- anchor/objective progress,
- graph mutation state,
- and any post-death mode flags once they exist.

## System ownership guidance
To stay aligned with earlier architecture docs, keep ownership narrow [file:13][file:14]:
- `PlayerInteractor` owns target acquisition and prompt eligibility.
- `HUDController` owns player-facing prompt/reticle/objective cards.
- `PlayerInventory` owns held item state.
- Item-specific runtime classes own use/readout logic.
- `SessionFlowController` or equivalent owns phase-change messaging triggers.
- `DebugOverlay` owns developer-facing state visualization.

## Acceptance criteria
This spec is implemented correctly when:
- the screen remains mostly clear during ordinary play,
- players can reliably identify when something is interactable through hover-reticle behavior [cite:31],
- prompts are brief, accurate, and never noisy,
- held-item usage is readable without bloated HUD support [file:13][file:15],
- players begin relying on breadcrumb tools rather than waiting for map support [cite:31],
- multiplayer coordination happens mostly through voice and spatial presence [cite:31][cite:32],
- death and end-state transitions are clearly communicated [file:1],
- and debug tools remain rich without contaminating the intended player experience [file:12][file:13][file:15].

## Relationship to next doc
This document defines broad UX rules for interaction and interface. The next navigation/orientation doc should go deeper on breadcrumb tools, route memory, room legibility, false familiarity, and the developer-only graph map policy [cite:31][file:13].
