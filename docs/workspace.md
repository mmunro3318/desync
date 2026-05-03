
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

--- 

Use the /ultraplan skill to review the various design docs in `docs/` and help me generate a ROADMAP.md which will be our canonical tracker of progress and completed work, so we don't get 
 lost during development. We'll start high level, identifying the MVP, core epics with features to be implemented. After the first pass, we'll back in and draft/flesh out the sprint         
designs, identifying the relevant user stories for each. After the second, we'll go back through and break each sprint in atomic tasks and subtasks. We need a hierarchy of tasks in a        
checklist mode to check off as we work.

---

Okay -- whatever you did broke something, and the game won't start anymore, crashing with these errors in console:

```
Failed to bind UDP socket because the address is already in use. Likely because there is another process using port 7777.
UnityEngine.Debug:LogError (object)

Server failed to bind. This is usually caused by another process being bound to the ame port.
UnityEngine.Debug:LogError (object)

[Netcode] Host is shutting down due to network transport start failure of UnityTransport!
UnityEngine.Logger:Log (UnityEngine.LogType,object,UnityEngine.Object)

[GameBootstrap] Transport failure.
UnityEngine.Debug:LogError (object)

[GameBootstrap] StartHost failed.
UnityEngine.Debug:LogError (object)
```
---

You may also provide a handoff-prompt doc in `docs/handoff-prompts/perplexity-research-requests` that I can feed into Perplexity's Deep Research mode to get comprehensive reports and guides for Unity development (anything really) and how to handle 3D graphics, rendering, debugging graphics artifacts; and level generation/management... your doc can include several separate prompts I'll feed one-at-a-time: specify which prompts to feed sequentially, which build/feed off prior context/results, and which prompts (prompt batches) should be separate threads (clean context). 

You're not restricted to the graphics/level question -- generate any research requests that you think will empower the AI coding tools later in development as well -- where are the dark corners and unknowns that I can address gather context on now and while the AIs work on the first steps/M1 (after our graphics fix). You can see the full picture, so think of all the right questions we should be asking (or that future Claude should/would be asking) that would come up during those sprints (or might sabotauge us if we miss the right answers to those questions, fail to ask, and Claude makes wrong assumptions...). I get 50 Deep Research requests/month, and never use them all (unless I'm doing big deep dives on random topics, like my crypto trading bot). Let's use them... 

# Research Requests

## What it does

The file establishes a complete **research handoff protocol** Claude Code can follow autonomously. Here's how the pieces fit:

## The 4 prompt types

| Type | When to use | Thread behavior |
|---|---|---|
| `oneshot` | Self-contained reference research (e.g., "what makes liminal spaces scary?") | Fresh window every time |
| `sequential` | Multi-part technical deep dives | Prompts 1→N in the same thread, each building on the last |
| `basecamp` | Complex new threads needing priming | Paste *before* activating Deep Research, no question yet |
| `qa-loop` | Design decisions that depend on your specifics | Perplexity asks you Qs → you copy answers back → then researches |

## How to use it

1. **Drop the file** as `CLAUDE.md` or append to your existing project instructions in VS Code
2. Claude Code will write `.md` files to `handoff-prompts/research-requests/` as it hits natural research inflection points during development
3. You check the folder periodically, open a file, and follow the `Type` + `Thread` instructions to know whether to start a new Perplexity session or continue an existing one
4. For `qa-loop` files, paste the prompt → copy Perplexity's questions into a new message to Claude Code → paste its answers back into Perplexity, *then* ask for the full report

## A few things to tweak

- If you already have a `CLAUDE.md`, paste the content after your existing instructions rather than replacing them
- You can update the **"When to Write a Request"** trigger list as the game's scope evolves — Claude will follow it literally
- The three full examples in the file (liminal space psychology, horror audio, enemy AI) double as **actual ready-to-use prompts** you can paste into me right now if any of those are live design questions