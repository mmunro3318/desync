
Next steps for docs break into three tracks: (1) finish the spatial-anomaly suite, (2) define the actual “play the game” loop, and (3) give Claude rich reference/context beyond our own docs.

Below is a concrete order that stays aligned with your existing architecture principles: narrow slices, data-driven systems, and debug-first hidden-state design. [ppl-ai-file-upload.s3.amazonaws](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/collection_3e24517e-f748-4f28-a2e8-ab6a8ad3dec7/8b3132e3-6a7e-42a3-8862-f3d6d706519c/Clone-MVP-Vision.md)

***

## 1. Finish the spatial anomaly doc stack

You now have:

- House Graph Core epic  
- Spatial Runtime Framework  
- Sprint 1A: Graph authoring/runtime  
- Sprint 1B: Node activation & portal visibility  
- Sprint 2: Observation Lock System  
- Sprint 3: Loop Mutation vertical slice  

The next **context/design** docs here should round out the anomaly families, but still one question at a time.

1. **Anomaly Families Epic (“Spatial Mutation Suite”)**  
   High-level design doc that:
   - Categorizes anomaly types: loop, substitution, Tardis (interior bigger than exterior), shadow-house bridges.  
   - States their intended player-facing effect and relative intensity.  
   - Tags which families are MVP vs post-MVP.  
   Purpose: so later sprints (4A, 4B, etc.) don’t reinvent semantics each time, and “what counts as a legal substitution vs loop” is fixed up front. [ppl-ai-file-upload.s3.amazonaws](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/collection_3e24517e-f748-4f28-a2e8-ab6a8ad3dec7/a9c7ca86-9c45-4e90-8d35-899089ac9029/1-Phasmo-Clone-MVP.md)

2. **Sprint 4A — Node Substitution / Room Swap Vertical Slice**  
   Similar structure to Sprint 3 but for “same doorway, completely different room”:
   - Design a small, controlled substitution pattern (e.g., Bedroom A ↔ Bedroom B variant).  
   - Precondition: only allowed when both nodes and connecting edges are eligible under observation rules. [ppl-ai-file-upload.s3.amazonaws](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/collection_3e24517e-f748-4f28-a2e8-ab6a8ad3dec7/8b3132e3-6a7e-42a3-8862-f3d6d706519c/Clone-MVP-Vision.md)
   - Clear debug showing before/after mapping and why it was legal.  
   Goal: prove you can “re-skin” or swap chunks of the house without breaking the graph or confusing the systems that treat nodes as stable identity.

3. **Sprint 4B — Tardis / Non-Euclidean Interior Slice**  
   Design-doc level first; possibly implement after jam:
   - Define what it means for an interior graph to have more volume than the exterior implies.  
   - Write explicit constraints so the exterior shell remains canonical while interior subgraphs can be larger/different. [ppl-ai-file-upload.s3.amazonaws](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/collection_3e24517e-f748-4f28-a2e8-ab6a8ad3dec7/8b3132e3-6a7e-42a3-8862-f3d6d706519c/Clone-MVP-Vision.md)

For the jam, you may not implement Tardis fully, but having the design doc lets you keep hooks clean in earlier code.

***

## 2. Anchor / Artifact loop and match structure

Right now your spatial core is strong, but the **player’s goal loop** is still mostly in your head. You want an expedition/anchor arc similar-in-spirit to Phasmophobia’s investigation loop but tuned for spatial horror.

Next docs here:

1. **Anchor / Artifact Loop Epic**  
   This should be a GDD-style doc focused on progression, not systems:
   - Define “anchor” types: seals, artifacts, nodes that stabilize or destabilize the anomaly.  
   - Clarify win/fail conditions: destroy N anchors, complete a ritual, return to van, etc.  
   - Decide whether anchors live on nodes, portals, or abstract “rooms” in the graph.  
   - Establish how anchor state interacts with anomaly intensity (e.g., fewer anchors → more aggressive anomalies).  
   This keeps mutations and goal state tied together logically instead of feeling like separate minigames. [ppl-ai-file-upload.s3.amazonaws](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/collection_3e24517e-f748-4f28-a2e8-ab6a8ad3dec7/a9c7ca86-9c45-4e90-8d35-899089ac9029/1-Phasmo-Clone-MVP.md)

2. **Sprint 5 — Anchor Core Loop (MVP)**  
   PDD for a minimal playable run:
   - Spawn 1–3 anchors based on the current graph.  
   - Let players locate and interact with them (destroy, cleanse, seal).  
   - On success, collapse or calm the anomaly and trigger extraction/win.  
   - On fail (time, corruption, death threshold), lock the house or “consume” the player.  
   Acceptance tests similar to the Minimal Ghost Loop checklist: one complete run, repeatable, with clear win/loss and debug visibility into anchor and anomaly state. [ppl-ai-file-upload.s3.amazonaws](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/collection_3e24517e-f748-4f28-a2e8-ab6a8ad3dec7/c0fd6c0f-2601-40a9-8f9c-7119efeb6283/4-Minimal-Ghost-Loop-Checklist.md)

3. **Match Loop & Session Flow Doc**  
   A small design doc that:
   - Defines match states: Setup → Explore → AnchorStage → Collapse/Extraction → Summary.  
   - Decides whether the van/outside is a separate scene or a zone in the same graph.  
   - Specifies how many days/nights, how restocking works, and whether there are “protégé” extra lives.  
   This is your “how does a session feel front-to-back” doc, separate from anomalies.

***

## 3. Player experience and UX docs

Once the core loop is drawn, it’s time to make it empathic and legible, especially given the jam’s asylum/queer-neurodivergent theme.

1. **Experience Pillars & Mood Doc**  
   A short, high-level design doc that:
   - States 3–4 experience pillars (e.g., “Unstable Certainty,” “Shared Madness,” “Asylum as Refuge/Threat”).  
   - Maps each pillar to concrete mechanics: loops, over-worlds, one-way visibility between layers, safe van/tent boundaries, etc.  
   - Includes a short “tone bible” for writing, art, and audio.  
   That doc keeps every feature scoped against the same emotional target rather than “cool tech” alone. [ppl-ai-file-upload.s3.amazonaws](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/collection_3e24517e-f748-4f28-a2e8-ab6a8ad3dec7/8b3132e3-6a7e-42a3-8862-f3d6d706519c/Clone-MVP-Vision.md)

2. **Player Abilities & Tools Concept Doc**  
   Not implementation, but semantics:
   - List baseline verbs: walk, observe, mark, camp, seal, carry.  
   - Define one or two starting tools (e.g., “anchor compass,” “topology scanner”) and what they should feel like.  
   - Tie tools to the spatial systems: how do they reveal or interact with loops, locks, or anchors?  
   This will later feed into concrete Tool PDDs similar to your old ghost/evidence/tool docs.

3. **UX / Accessibility & Comfort Guidelines**  
   Given the spatial horror and neurodivergent-identity theme, you’ll want:
   - Rules for motion (slow acceleration, strong foothold, comfort settings; no cheap flickers).  
   - Clear options for visual intensity, HUD clutter, text readability.  
   - Guardrails against mechanics that mimic real-world psychiatric harm in ways you don’t want.  

1. Player Experience Pillars
Emotional goals, fantasy, dread profile, co-op social feel, thematic translation, death feel, and what the player should feel at each phase of a run.

2. UX Principles and Interaction Spec
Reticle, prompts, item pickup/use/drop, HUD rules, objective communication, post-death camera/spirit mode, breadcrumb tools, multiplayer readability, and what information is intentionally withheld.

3. Navigation and Orientation UX
A narrower systems doc for marking tools, room naming/readability, environmental legibility, no-map policy, debug map policy, and how players maintain orientation in a mutating graph.

***

## 4. Art, audio, and asset pipeline docs

You’re planning a dedicated asset sprint, so you’ll want a doc that makes that efficient.

1. **Art Direction & Asset Bible (Jam Scope)**  
   - Target look: stylized vs realistic, how noisy vs minimal, color language for “safe vs unstable.”  
   - Baseline materials and textures (walls, floors, doors, key props) to support procedural recombination.  
   - Rules for using over-world skins and alternate “dimensions” so art doesn’t fight readability.  

2. **Audio Direction & System Concepts**  
   - Baseline ambience per node type.  
   - Rules for spatial audio tied to anomalies (e.g., subtle Doppler cues in loops).  
   - Minimal system spec for diegetic hints that a loop or substitution has occurred.

3. **Asset Pipeline & Naming Conventions**  
   - How Claude and you will name prefabs, textures, and materials so they plug into graph/node definitions cleanly. [ppl-ai-file-upload.s3.amazonaws](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/collection_3e24517e-f748-4f28-a2e8-ab6a8ad3dec7/186413d8-e11c-428e-a166-596f824425e8/3-Starter-Design-Doc.md)
   - “If you place it more than twice, make it a prefab” and data-backed tags for reuse. [ppl-ai-file-upload.s3.amazonaws](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/collection_3e24517e-f748-4f28-a2e8-ab6a8ad3dec7/871b226d-c480-4c9d-8c92-1bbdc0b47f6c/2-Project-Structure.md)

***

## 5. Claude usage, context, and research docs

To get the most from Claude Code (and other assistants) you’ll want a small set of “meta-docs”:

1. **Claude Working Agreement / Task Template**  
   A doc with:
   - Standard task format (Objective → Files → Contracts → Non-goals → Acceptance Tests).  
   - The small, stable set of core docs that should always be loaded (Spatial Framework, relevant Sprint PDD, maybe Mood/Pillars).  
   This mirrors how your previous docs emphasized stable naming/structure for AI to succeed. [ppl-ai-file-upload.s3.amazonaws](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/collection_3e24517e-f748-4f28-a2e8-ab6a8ad3dec7/186413d8-e11c-428e-a166-596f824425e8/3-Starter-Design-Doc.md)

2. **Unity Reference & Gotcha Cheat Sheet**  
   Curated from Unity docs, tutorials, Reddit/Discord wisdom:
   - Best patterns for additive scenes, ScriptableObjects, addressables (if used).  
   - Known pain points (physics vs triggers vs portals, async scene loads, etc.).  
   This is less a formal spec and more a “constraints & recipes” context doc you can hand to Claude.

3. **Spatial Horror Reference Board (Non-Implementation)**  
   A short text doc enumerating inspirations (PT, House of Leaves, Exit 8, etc.), what you like about them, and what you want to avoid.  
   Good to keep the game’s identity anchored when you’re deep in technical weeds.

***

## 6. Prioritization: what to write next

Given time to Pride Jam and the docs you have, the highest-leverage **next three** context/design docs are:

1) **Anomaly Families Epic (Spatial Mutation Suite)**  
2) **Anchor / Artifact Loop Epic**  
3) **Experience Pillars & Mood Doc**

Those three lock in “what anomalies exist,” “why players are here and how they win/lose,” and “what this game is emotionally about,” which will keep the remaining sprints and implementation from drifting into a generic tech demo. [ppl-ai-file-upload.s3.amazonaws](https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/collection_3e24517e-f748-4f28-a2e8-ab6a8ad3dec7/a9c7ca86-9c45-4e90-8d35-899089ac9029/1-Phasmo-Clone-MVP.md)

Would you prefer to start with the Anomaly Families Epic next, or jump straight into the Anchor / Artifact Loop Epic so the win-condition feels more concrete?


--- 

[GameBootstrap] Scene "House_Graybox" not found in build settings. Shutting down host.
UnityEngine.Debug:LogError (object)

---

