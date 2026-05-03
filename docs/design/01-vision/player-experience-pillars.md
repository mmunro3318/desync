# Player Experience Pillars

## Document objective
Define the intended emotional, social, and perceptual experience of play so later UX, systems, content, and sprint docs all optimize toward the same player-facing outcome. This document is the high-level player-experience contract for the multiplayer-first spatial horror prototype [file:12][file:13][file:14][file:15][cite:31][cite:32].

## Why this doc exists
The project has moved away from a ghost-hunting clone and toward a multiplayer-first prototype built around unstable geometry, navigation stress, and partial information. Earlier planning already established that the project should optimize for loop clarity, readable interaction, debug visibility, and modular systems; this document preserves those architectural values while redefining what the player should actually feel during play [file:12][file:13][file:14][file:15].

## Core player fantasy
Players enter an impossible house knowing that the environment obeys some hidden logic, even if they do not yet understand it. The fantasy is not mastery through perfect knowledge, but survival through fragile pattern recognition, teamwork, courage, and self-made orientation inside a reality that threatens to detach them from safety forever [cite:31][file:12][file:15].

The player should feel three things at once:
- vulnerable to the house,
- overwhelmed by growing spatial complexity,
- yet convinced that the house is solvable if they stay attentive and coordinated.

## Pillar 1 — Rule-bound uncertainty
The house should feel unstable, but never arbitrary. Players may not know *which* rule is active, *when* a shift will occur, or *what* changed while they were not looking, but they should gradually infer that space changes according to consistent systemic logic rather than random developer cruelty [file:12][file:15][cite:31].

### Player feeling
- “I do not understand this place yet.”
- “Something changed when I was not observing it.”
- “There must be a rule here.”
- “If I stay calm, I can work out enough of it to survive.”

### Design implication
Every major spatial anomaly should preserve some recoverable logic, even if the player cannot fully decode it in one run. The game should produce paranoia, not hopelessness.

## Pillar 2 — Spatial dread over jump-scare dependence
The dominant fear should come from navigation drift, geometry mistrust, and the terror of stepping into a path that may no longer connect back to safety. Threats, entities, and hunts may produce acute panic spikes, but they should sit on top of the deeper ongoing dread that the house itself is unstable [cite:31][file:12].

### Player feeling
- “I do not trust this hallway anymore.”
- “If I go deeper, I may not be able to return.”
- “I need a reference point before this place changes again.”

### Design implication
Level structure, visibility, sound, breadcrumb tools, and anomaly pacing should do more work than cinematic jump scares. The most memorable moments should come from realizing the space has betrayed expectation.

## Pillar 3 — Multiplayer witness and mutual orientation
The game is multiplayer-first, so fear should be shaped by other people being present, absent, separated, or differently informed. Co-op is not just practical support; it is part of how truth is established. Another player seeing a space may stabilize it, warn you, contradict you, or fail to arrive when expected [cite:32][cite:31].

### Player feeling
- “Did you see that too?”
- “Stay there and keep watching that door.”
- “If we split up, we may lose the route.”
- “Your presence might be the only thing keeping this corridor stable.”

### Design implication
Multiplayer UX should favor verbal coordination and shared witnessing over explicit shared UI markers. Co-op should feel like social navigation under pressure, not like a networked puzzle game with omniscient interface tools.

## Pillar 4 — Minimal information, intentional exposure
The player should not be wrapped in explanatory UI. The screen should feel exposed and vulnerable, with the house occupying most of the player’s attention. Information should appear only when needed for immediate action, and the absence of constant guidance is part of the intended affect [file:13][file:15][cite:31].

### Player feeling
- “I have to pay attention to the space itself.”
- “The game is not going to narrate every danger for me.”
- “I only get tiny clues; the rest is on me.”

### Design implication
The in-run interface should remain sparse, with player-facing information mostly delivered through environment, voice, sound, objects, and intermittent prompts rather than persistent HUD layers. Clarity should come from interaction quality and reliable rule communication, not from interface density [file:12][file:13].

## Pillar 5 — Clear mission, unclear route
The macro-goal should be understandable before the run begins, likely through menu/tutorial framing, but the local route through the house should remain unstable and difficult to trust. This preserves overall loop clarity while allowing the navigation layer to remain frightening and disorienting [file:12][file:1][cite:31].

### Player feeling
- “I know why I’m here.”
- “I do not know whether this path is still good.”
- “The goal is understandable, but getting there is not safe.”

### Design implication
The game should communicate the mission cleanly before deployment, then largely stop explaining itself in the field. Players should be able to state the run objective in one sentence, even if they still fear every step of execution.

## Pillar 6 — Self-made orientation
Players should not receive an in-game map. Orientation is something they build through observation, memory, speech, and tools they physically place or use, such as chalk marks, spray paint, chemlights, and other breadcrumb devices [cite:31].

### Player feeling
- “If I do not mark this route now, I may never trust it again.”
- “These traces are the only proof I was here.”
- “The house is rewriting itself faster than my memory can hold it.”

### Design implication
Player-placed markers are not flavor props; they are core navigation aids and part of the emotional identity of the game. Their utility should be strong enough that players naturally invent route-maintenance habits.

## Pillar 7 — Pattern-recognition strain
Cognitive stress should come from comparing remembered layout against present layout under uncertainty. This is different from pure sensory overload. The game should push players to notice mismatches, loops, omissions, repeated thresholds, and false familiarity [cite:31].

### Player feeling
- “This room is almost the same, but not exactly.”
- “I have been here before... I think.”
- “Something is wrong with the sequence of spaces.”

### Design implication
The game’s horror should repeatedly exploit near-recognition. Subtle differences, repeated spaces, and ambiguous continuity should matter more than loud visual chaos.

## Pillar 8 — Tragic but legible failure
Failure should feel serious and sad, but understandable. The player should believe they were defeated by pressure, misjudgment, bad luck within a rule-bound system, or overwhelming escalation—not by unreadable interface failure [file:1][file:12][cite:31].

### MVP death stance
For MVP, death should resolve simply and cleanly. A dead player loses direct agency, their body remains in the world, and the session continues for surviving players.

### Roadmap stance
The preferred expansion path is a ghost-camera spectator mode with continued voice participation. That future state should preserve involvement after death while still feeling eerie and limited rather than empowering [cite:31].

## Pillar 9 — Queer/neurodivergent instability as mechanics
The project’s thematic lens should live in play structure, not just narrative framing. The player experience should express instability of norms, mismatch between expectation and environment, the danger of relying on external rules that refuse to remain fixed, and the need to build meaning through adaptation and mutual support [cite:31].

### Design implication
This should not become random confusion or ableist “madness” shorthand. The point is not that the player is broken; the point is that the world is unreliable, coercive, and unstable, and survival depends on developing alternative orientation strategies.

## Pillar 10 — Solo-readable, multiplayer-native
Although you will often test alone, the game should be designed from the start as multiplayer-native. Solo play should still function and remain legible, but co-op should feel like the full intended expression of the design rather than an add-on mode [cite:31][cite:32].

### Design implication
Mechanics, pacing, and communication assumptions should never depend on a single omniscient player. Systems should naturally support split attention, partial information, and unequal spatial knowledge.

## Emotional arc of a run
A strong session should move through this emotional curve:

1. **Brief certainty** — players know the mission and have a starting point.
2. **Controlled exploration** — the house seems navigable, though uneasy.
3. **Doubt** — players notice inconsistencies and begin using marks or verbal checks.
4. **Separation anxiety** — routes feel less trustworthy; distance between players becomes dangerous.
5. **Escalation** — objectives near completion, but the house and threat push back harder.
6. **Extraction panic** — players try to return or finish while mistrusting every familiar path.
7. **Aftermath** — success feels earned; failure feels mournful and analyzable.

## What the game should not feel like
The game should avoid becoming:
- a jump-scare delivery system,
- a lore puzzle that interrupts spatial play,
- a fully HUD-led objective game,
- a random maze with no decipherable structure,
- or a co-op experience that can be solved by UI pings instead of conversation.

## Testing questions
Use these questions in playtests:
- Did players understand the mission before entering the run [file:12]?
- Did they believe the house had rules, even when they could not articulate them?
- Did they begin making self-directed orientation habits?
- Did co-op conversation feel necessary and useful?
- Did fear come more from spatial uncertainty than from enemy scripting?
- When players died or got lost, did the outcome feel tragic but understandable [file:1]?
- Did the minimal interface increase attention without causing basic interaction confusion [file:13][file:15]?

## Acceptance criteria
This document is serving the project correctly when:
- New mechanics are evaluated partly by whether they increase spatial dread and rule-bound uncertainty.
- UX decisions preserve a sparse player-facing interface while keeping interactions readable [file:13][file:15].
- Multiplayer design privileges witnessing and verbal coordination over explicit shared markers [cite:31][cite:32].
- Navigation tools focus on player-authored breadcrumbs instead of map revelation [cite:31].
- Death, escalation, and extraction continue to reinforce tragic spatial pressure rather than arcade spectacle [file:1][cite:31].

## Relationship to later docs
This document defines *what the player should feel*. The next UX/interaction document should define *how controls, prompts, HUD, marking tools, and post-death states support that feeling* [file:13][file:14].
