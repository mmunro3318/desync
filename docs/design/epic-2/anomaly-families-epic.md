# Anomaly Families Epic

## Purpose
This epic defines the major spatial anomaly families for the project, the player-facing effect each family is meant to create, the technical shape each family takes inside the graph runtime, and which families belong in MVP versus post-MVP development. Its job is to give later sprint docs stable semantics so “loop,” “substitution,” “Tardis,” and “shadow bridge” do not drift into vague or overlapping meanings as implementation expands [file:12][file:13][file:14][file:15].

This document also exists to preserve the design principles already established across prior planning: prove one major question at a time, keep runtime state separate from authored definitions, keep scene objects thin, make hidden systems debuggable from the start, and prefer new data over new engine-level branching whenever possible [file:12][file:13][file:14][file:15].

## Design goal
Anomalies are the house’s main expressive language. They should not feel like random tricks layered on top of a normal level. They are the way the game communicates instability, refusal of certainty, and the sensation that physical rules only hold so long as attention can keep them coherent.

At the same time, anomalies must remain readable. The player should usually feel “the house changed because I stopped being able to hold it steady,” not “the game cheated.” Readability and emotional trust are higher priorities than maximum complexity, especially in MVP [file:12][file:15].

## Core anomaly principles

### 1. Anomalies are graph transforms first
An anomaly should be defined primarily as a transform on runtime house graph state, not as a one-off scene hack. Visual presentation can vary, but the underlying semantic change should be expressible in terms of nodes, edges, portal routing, subgraph composition, or visibility domains.

### 2. Anomalies obey observation legality
An anomaly may only apply when affected nodes and edges are mutation-eligible under the observation lock system. This preserves the project’s central rule that space changes when certainty collapses, not at arbitrary moments.

### 3. Anomalies must be debuggable
Every anomaly family must expose:
- family id,
- active pattern id,
- affected nodes,
- affected edges,
- preconditions,
- blocking reasons,
- and reset state.

This follows the hidden-state debug-first requirement repeated in the earlier project docs [file:12][file:13][file:14][file:15].

### 4. New anomaly content should mostly mean new data
Adding a new anomaly variant should mostly mean authoring new definitions, patterns, and tags rather than rewriting the graph runtime from scratch [file:12][file:13].

### 5. One active question per sprint
Implementation should keep following the earlier milestone logic: one family or one vertical slice at a time, each answering a specific technical and experiential question before the next family is attempted [file:12][file:15].

## Taxonomy overview

| Family | Player-facing effect | Graph/runtime shape | MVP priority |
|---|---|---|---|
| Loop anomalies | Paths repeat, continue incorrectly, or trap the player in recursive traversal | Portal-edge remap | High |
| Substitution anomalies | A familiar room or corridor resolves into a different authored variant | Node swap or node presentation/domain substitution | High |
| Tardis anomalies | Interior space contains more volume or sequence depth than the exterior shell implies | Subgraph insertion or interior-only graph expansion | Medium |
| Shadow-house anomalies | Player crosses into or perceives an alternate layer/domain of the same house | Domain swap, parallel graph overlay, or bridge edges between graphs | Medium |
| Overworld separation anomalies | Players occupy the “same” house but in different active layers with asymmetric visibility/interactions | Multi-domain runtime state with per-player layer membership | Post-MVP |
| Topology corruption anomalies | The graph itself destabilizes broadly as win/lose pressure escalates | Higher-order mutation orchestration across families | Post-MVP |

## Family 1 — Loop anomalies

### Player effect
Loop anomalies create the feeling that progress has stopped mapping cleanly onto movement. A hall may continue when it should end, an exit may resolve back into an earlier route, or a threshold may preserve local continuity while violating global expectation.

### Technical shape
Loop anomalies are primarily **portal-edge remaps**. The geometry the player sees remains locally coherent, but route resolution changes at traversal boundaries.

### Use cases
- Hall repeats with subtle variation.
- Door at the end of a corridor returns the player to an earlier segment.
- Stair landing resolves to the same floor again.

### Strengths
- High horror payoff for relatively controlled runtime complexity.
- Strong fit for the existing graph, portal, and observation framework.
- Easy to stage as a first real vertical slice.

### Risks
- Can feel cheap if overused.
- Needs careful presentation so players sense continuity rather than obvious teleporting.

### MVP status
**MVP family.** Already the correct first implemented anomaly family.

## Family 2 — Substitution anomalies

### Player effect
Substitution anomalies create the feeling that a familiar place is no longer the place the player thought it was. A bedroom may become a variant bedroom, a safe corridor may become hostile, or a remembered landmark may silently resolve into a different authored space.

### Technical shape
Substitution anomalies are primarily **node substitutions**. The runtime swaps one node instance, node presentation, or node-domain association for another while preserving compatible graph connections.

### Use cases
- Same doorway now leads into a mirror-variant room.
- A room revisited later has different props, dimensions, or exits.
- “Safe” room becomes “wrong” room after observation collapses.

### Strengths
- Expands replayability without requiring entirely new house layouts.
- Strong thematic fit for unstable identity and shifting social rules.
- Pairs well with art-direction passes and sparse environmental storytelling.

### Risks
- Requires stable identity rules so code can distinguish “same logical node with new skin” from “actually different node.”
- Can become muddy if too many variants share weak visual contrast.

### MVP status
**MVP family.** Best second anomaly family after loop remapping.

## Family 3 — Tardis anomalies

### Player effect
Tardis anomalies create the feeling that the house contains more depth than its exterior can justify. A closet can become a hallway system, a pantry can contain a wing, or a staircase can imply volume that the outside shell cannot hold.

### Technical shape
Tardis anomalies are primarily **interior-only subgraph insertions**. A small threshold on the baseline graph resolves into a larger composed interior graph that does not need to obey the baseline exterior shell.

### Use cases
- Small side room unfolds into extended service corridors.
- Attic hatch opens into impossible architecture.
- Basement access descends into a larger-than-house understructure.

### Strengths
- Extremely strong spatial-horror payoff.
- Supports expedition depth and the fantasy of “going too far in.”
- Good foundation for later home-base/tent/return-path tension.

### Risks
- Higher rendering and mental-model complexity.
- Needs strong node activation and streaming discipline to avoid exposing impossible exteriors.
- More likely to demand art/content support than loop anomalies.

### MVP status
**Stretch MVP / early post-MVP.** Worth designing early, but implementation should likely trail loop and substitution.

## Family 4 — Shadow-house anomalies

### Player effect
Shadow-house anomalies create the sense that the player has slipped into another version of the house: familiar structure, altered mood, altered routes, altered rules, and potentially altered visibility to other players.

### Technical shape
Shadow-house anomalies are primarily **parallel graph overlays or domain swaps**. The runtime treats the shadow house as a separate but structurally related graph, with bridge edges or transition triggers between domains.

### Use cases
- Hall enters a dark parallel layer with muted sound and altered exits.
- One player sees another player one-way through a domain boundary.
- Safe rooms in baseline become compromised in the shadow domain.

### Strengths
- Excellent thematic expression for dissociation, alienation, and split experience.
- Strong support for multiplayer asymmetry later.
- Offers a controllable way to escalate beyond local anomalies into systemic instability.

### Risks
- Significant content and UX complexity.
- Easy to confuse players without deliberate visual language and debug support.
- Can sprawl into “entire second game” if not constrained.

### MVP status
**Early post-MVP or tightly constrained stretch.** Design now, implement later unless a very small bridge slice proves cheap.

## Family 5 — Overworld separation anomalies

### Player effect
Players occupy different active versions of “the same” house. They may hear, glimpse, or infer one another across layers but cannot fully share space or interactions.

### Technical shape
These are **multi-domain per-player layer assignments** on top of the graph runtime. Each player’s current domain determines routing, visibility, and interaction permissions.

### Use cases
- Two players stand “in the same room” but only one can see the current exit.
- One player can see another through a wall or threshold the other cannot cross.
- Co-op requires synchronizing perception rather than simply sharing a position.

### Strengths
- Potentially the most original multiplayer expression of your concept.
- Deeply aligned with fractured social reality and layered identity themes.

### Risks
- Networking and UX complexity spike sharply.
- Hard to communicate cleanly in a jam MVP.
- Requires mature domain, interaction, and replication rules.

### MVP status
**Post-MVP.** This should remain a design target, not a near-term implementation commitment.

## Family 6 — Topology corruption anomalies

### Player effect
The house stops feeling like a collection of isolated tricks and begins feeling globally unstable. Routes fail, safe assumptions erode, and multiple anomaly families begin interacting as the house destabilizes under pressure.

### Technical shape
Topology corruption is a **director-level orchestration family**, not one primitive mutation type. It coordinates pacing, escalation, family weighting, and legal transform selection across multiple anomaly families.

### Use cases
- Each destroyed anchor increases anomaly frequency and severity.
- Repeated failed navigation pushes the house into more aggressive states.
- Late-stage runs mix loops, substitutions, and shadow intrusions.

### Strengths
- Excellent late-run escalation.
- Creates a path from eerie uncertainty to full panic.

### Risks
- Not a starter feature.
- Depends on multiple lower-level families already being reliable.

### MVP status
**Post-MVP orchestration layer.** Design now only as a future pacing concept.

## Primitive runtime operations
To keep the family taxonomy concrete, anomaly families should be built from a small set of primitive runtime operations.

| Primitive | Meaning | Used by |
|---|---|---|
| Edge remap | Change which destination a portal-edge resolves to | Loop, corruption |
| Node substitution | Replace or rebind a node’s active room variant/domain | Substitution, corruption |
| Subgraph insertion | Attach an interior-only graph behind a threshold | Tardis |
| Domain swap | Change the active graph domain for player or route | Shadow-house, overworld separation |
| Bridge edge activation | Connect two otherwise separate graph domains | Shadow-house, corruption |
| Director weighting | Alter anomaly family probability and escalation | Corruption |

These primitives matter because they let implementation stay systematic and data-driven instead of devolving into one bespoke script per scare [file:12][file:13][file:14].

## MVP prioritization

### Core MVP families
These are the anomaly families most likely to produce a playable, understandable, and jam-realistic prototype:
- Loop anomalies
- Substitution anomalies
- A very small Tardis-style extension only if loop and substitution stabilize early

### Stretch MVP families
Possible only if the previous families are stable and the content workload stays manageable:
- Tiny shadow-house bridge slice
- One constrained interior-only Tardis branch

### Post-MVP families
Do not schedule these until the core prototype is fun and stable:
- Full shadow-house domain systems
- Per-player overworld separation
- Director-level topology corruption as a broad orchestration layer

This prioritization is intentionally conservative because the earlier docs repeatedly emphasized proving the loop in narrow slices and avoiding overbuild before core value is clear [file:12][file:15].

## Relationship to player loop
Anomalies should not exist as disconnected set pieces. They should serve the broader play loop by:
- obstructing navigation,
- producing uncertainty,
- hiding or exposing anchors/artifacts,
- intensifying danger as the match escalates,
- and making cooperative communication more valuable.

In MVP, anomalies should primarily support navigation horror and route uncertainty. Only later should they become full social/perception asymmetry systems.

## Debug requirements by family
Every family should support the same minimum debug vocabulary:
- family id,
- active pattern id,
- affected nodes,
- affected edges,
- current domain,
- baseline state vs current state,
- legality result,
- and blocking reasons.

Recommended family-specific additions:
- **Loop**: baseline destination vs current destination per edge.
- **Substitution**: baseline node variant vs active node variant.
- **Tardis**: inserted subgraph id and entry threshold.
- **Shadow-house**: current domain and bridge edge status.
- **Overworld separation**: per-player domain/layer assignment.
- **Corruption**: current family weighting and escalation tier.

## Data asset recommendations
Recommended ScriptableObject categories:
- `LoopPatternDefinition`
- `SubstitutionPatternDefinition`
- `TardisSubgraphDefinition`
- `ShadowDomainDefinition`
- `DomainBridgeDefinition`
- `AnomalyFamilyDefinition`
- `AnomalyDirectorRulesDefinition`

This mirrors the earlier project preference for tunable assets and predictable structure rather than hardcoding content into large runtime classes [file:12][file:13][file:14].

## Implementation order
Follow this order unless testing proves a different family is unexpectedly cheaper and clearer:
1. Loop anomaly vertical slice.
2. Substitution anomaly vertical slice.
3. Anchor/artifact loop integration with anomaly intensity.
4. Small Tardis branch.
5. Optional shadow bridge prototype.
6. Broader director-level corruption logic.
7. Overworld/per-player separation experiments.

This preserves the “one major question at a time” roadmap style already used successfully in the earlier milestone docs [file:12][file:15].

## Risks and guardrails

### Major risks
- Too many anomaly families too early.
- Ambiguous semantics between family types.
- Visual confusion outrunning mechanical clarity.
- Networking ambitions consuming MVP time.
- One-off scare implementations bypassing the graph framework.

### Guardrails
- Every new anomaly must declare its family and primitive runtime operation.
- No anomaly family should skip observation legality.
- No sprint should attempt more than one new family at once.
- Debug support ships with the feature, not later.
- If a desired effect cannot be described in graph/runtime terms, treat it as suspect until justified.

## Done condition for this epic
This epic is considered established when:
- anomaly families have clear names and non-overlapping meanings,
- MVP versus post-MVP priority is explicit,
- each family has a defined runtime shape,
- primitive operations are documented,
- and future sprint docs can reference this epic instead of redefining anomaly taxonomy each time.
