
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

**Frontier Coding Agents Can Now Implement an AlphaZero Self-Play Machine Learning Pipeline For Connect Four That Performs Comparably to an External Solver**

  S1A Skill Pipeline

  Here's the suggested workflow for S1A (House Graph Authoring), with session breakpoints marked:

  Session 1: Architecture + Plan

  /office-hours           → Scope the sprint, challenge premises, design doc
  /plan-eng-review        → Lock architecture (graph data model, module boundaries, network sync)
                          ↓
  📋 Output: approved design doc + eng plan
  🔄 COMPACT/NEW SESSION — plan is on disk, context can be clean

  Session 2: Core Graph Implementation

  /context-restore        → Pick up the plan
  /tdd                    → HouseGraphDefinition (SO) + HouseGraphInstance (runtime)
                            Vertical slice: author 3-node graph, load, query adjacency
  /unity-mcp-skill        → Materialize graph as scene objects (room nodes)
                          ↓
  📋 Output: graph loads, queries work, rooms appear in scene
  🔄 COMPACT/NEW SESSION — heavy Unity MCP work fills context fast

  Session 3: Network Sync + Debug Overlay

  /context-restore        → Pick up from Session 2
  /tdd                    → NetworkVariable sync of graph state, 2-player validation
  /unity-mcp-skill        → Debug overlay (graph topology viz, node states)
                          ↓
  📋 Output: graph replicates, debug overlay functional
  🔄 COMPACT/NEW SESSION

  Session 4: Ship + Integration

  /context-restore        → Pick up from Session 3
  Manual                  → Quick restart support, edge case cleanup
  /ship                   → PR with full test suite
  /review                 → Final review gate
                          ↓
  📋 Output: S1A PR merged to main

  Key notes:
  - Sessions 2-3 are the heaviest. Unity MCP tool calls + TDD loops eat context fast. Plan for compaction between them.
  - The /context-restore → /context-save pattern preserves state across sessions.
  - Read the S1A sprint PDD (docs/design/04-sprints/sprint-1a-house-graph-authoring.md) and contracts doc (docs/design/02-architecture/networked-house-runtime-interfaces-contracts.md) at
  Session 1 start, not Session 2.

  ---

