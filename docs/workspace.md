
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
