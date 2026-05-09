
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


---

● SECOND OPINION (Codex):            
  ════════════════════════════════════════════════════════════
  1) Coolest version: A "stagecraft graph" -- only 2-3 physical room shells are ever live; the runtime quietly rebinds which semantic node occupies which shell as players turn, cross           
  thresholds, or lose line of sight. The graph becomes the illusion rig itself.                                                                                                                  
                                                                                                                                                                                                 
  2) Key insight: "Walking through a portal feels seamless -- no loading hitch or frame stutter." The real excitement is invisible handoff, not "we have a graph."                               
                                                                                                                                                                                                 
  3) 50% solution: xNode (ScriptableObject-backed graph editor). The other 50%: binding graph IDs to scene anchors, runtime queries, NGO sync, reset, and spatial debug.                        

  4) Weekend build: One end-to-end illusion. HouseGraphDefinition for 5 nodes, authoring components on House_Prototype, SpatialGraphRuntime with 5 queries, one server-owned snapshot, one clean
  threshold handoff, one ugly debug overlay. "If that hallway transition does not feel clean by Sunday, the rest is architecture theater."

  5) Premise challenge: Premise 4 (network sync scope). The authored graph is static -- every client loads the same asset. Syncing the whole graph via NetworkVariable is probably wrong for a
  5-week jam. Only sync authoritative deltas (occupancy, mutation state), or S1A becomes NGO serialization work instead of proving the magic trick.
  ════════════════════════════════════════════════════════════

Cross-model synthesis:

  - Agree: The "if the hallway transition doesn't feel clean, the rest is architecture theater" line nails it. The vertical slice philosophy in your CLAUDE.md says the same thing.
  - Agree: The xNode callout is interesting but I'd skip it. Adding a dependency for graph editing when the graph is 5 nodes feels like yak-shaving. ScriptableObject arrays are sufficient for
  the jam.
  - Partially agree on premise 4: Codex is right that syncing the full graph definition is wasteful -- the asset is identical on all clients. But the ROADMAP acceptance criteria explicitly
  require "graph state replicates over network." The resolution: sync only runtime state (player occupancy, node active/inactive, topology version counter) via NetworkVariable, not the
  definition data itself. Clients load the same SO, server syncs only mutable runtime deltas.
  - Disagree on stagecraft graph: The pooled-shell rebinding idea is architecturally elegant but out of scope for S1A. It's a S1B concern (node activation/deactivation). S1A should prove the
  data model, not the rendering trick.                                                                                                                                                           
                  
● User answered Claude's questions:    
  Yes, narrow to runtime deltas -- I had been thinking something similar, but had no idea how to implement it in Unity. I   
     can easily create this construct for a 2D JavaScript game (drawing to Canvas), but hesitate at 3D objets (and not grid based... could it be grid based? could we cheat a little with        
     enforcing consistency with, like, 1m/1.5m grids/cells? what do we simply with this, and what gets complicated?).                                                                            
                                                                                                                                                                           
     But to circle back -- I don't want to load the entire graph (ever). A fully synced state (for graphics, specifically) would require each client load/render or handle all scene/graph data 
     that's visible to the players on other computers... ridiculous. I should be able to track only the active nodes for that player and leverage the deltas for quick sync/load... it's probably
      fine if the client has the full house graph synced/loaded, as long as we're not cramming petabytes of graphics data down their little network cable (well, WiFi... but didn't have an      
     analogy on hand). But if the server holds the True state of the house graph at all times, and is performing the mutations and transforming the graph... **to clarify** -- are we discussing:
      "The player gets a full house graph on load/init, and then as the server mutates the house graph, we push only the deltas to update the client's graph? or are we only giving each client  
     the house node data observable to them on their machine/instance (this was my initial thought, but now I'm thinking you meant the former)? The former seems most sensible, and how I would  
     do it -- there's no need to shove all graph data onto each client when nothing (too) dramatic is happening to the house graph.                                                              
                               
     I don't think I understand the "stagecraft graph" pitch... does he mean 3 high-level shells that get repurposed/painted? I'm not sure if that approach could handle complex geometries above
      basic boxy shapes...                                                                                                                                                                                                                                                                                                                          
     If xNode is a solution to future scaling problems we'd hit with ScriptableObject arrays, then we should note it for review/research post MVP.

  Phase 4: Approaches

  APPROACH A: "Vertical Slice First" (Recommended)
    Summary: Build the smallest end-to-end path first: 3-node graph (Entry→Hall→Living),
             runtime loads it, one query works, debug shows it. Then expand to 5 nodes
             and full query set.
    Effort:  M
    Risk:    Low
    Pros:    - Testable artifact after Session 2 day 1 (matches Mike's stated preference)
             - If any architectural assumption is wrong, you find out on 3 nodes, not 16 files
             - Codex's "if the hallway doesn't feel clean by Sunday" philosophy
    Cons:    - Slightly more refactoring as you expand from 3→5 nodes
             - Definition SO shape might change once you add portal anchors
    Reuses:  GameBootstrap scene-loading pattern, existing player prefab, GameplaySettings SO pattern

  APPROACH B: "Full Sprint PDD as Written"
    Summary: Implement all 16 files, 5 tasks, and acceptance criteria from the sprint PDD
             in order. Definitions first, then runtime, then debug, then match flow.
    Effort:  L
    Risk:    Medium
    Pros:    - PDD is well-thought-out and complete -- no design decisions mid-implementation
             - All 5 queries land together, no partial API surface
             - Debug overlay sees the full graph from the start
    Cons:    - First testable play-mode artifact comes late (after ~8 files are written)
             - If the SO shape is wrong, you've built 4 definition files on a bad foundation
             - Higher risk of context exhaustion in a single session
    Reuses:  Same as A

  APPROACH C: "Data-First, Scene-Last"
    Summary: Build all definition SOs and runtime queries as pure C# with EditMode tests
             only. No scene work, no MonoBehaviours, no debug overlay until the data layer
             is proven. Then wire it to a scene in Session 2.
    Effort:  M
    Risk:    Medium
    Pros:    - Fastest path to tested graph logic (no Unity scene complexity)
             - EditMode tests catch data model bugs before scene wiring adds confusion
             - Clean separation: Session 1 = data, Session 2 = scene
    Cons:    - No play-mode "whoa" moment until Session 2
             - Contradicts Mike's preference for iteratively testable phases in play mode
             - Authoring components (RoomNodeAuthoring) deferred, so scene setup is delayed
    Reuses:  Existing EditMode test pattern (NetworkBootstrapConsistencyTests)

  RECOMMENDATION: Approach A ("Vertical Slice First") because it matches both your stated preference for iteratively testable phases and Codex's "prove the hallway t

Hmm... I'm very hesitant to deviate from the design docs at this stage. I want to push for Approach B -- full sprint PDD (with small pivots based on discussions). But I *do* want to see a quick playable vertical slice. It's just that in the past when I prioritize that quick playable POC, we often end up with desperate, duct-taped decisions to ship something... temporary solutions that get baked in and codified indefinitely, and turn downstream work into a mess... and that's where I usually give up and jump ship on a project. The moment I have what feels like 90% done, and I watch that last 10% dilate toward infinity for the refactors, bug fixes, TODOs, and major brain surgery on our game systems to add a single feature... I'd rather start over or start a new project.

So let's stick to Approach B. However... if we can isolate an Approach A vertical slice as an exploratory prototype to test Unity implementations (both testing our knowledge/research, understanding, and assumptions of Unity and it's MCP framework; and testing the limits and capacity of C# and the Unity framework/engine itself) -- I would call that a valuable sprint that would empower future dev work. But it has to be an experiment, and not approached as the root of iteration. We can iterate with experiments, trying new features, new approaches, new algorithms, new loading/saving schemes... whatever -- perhaps leveraging subagents (or external CLI agents, like Gemini in Antigravity, or Codex) to sync the living experiment with true work and ARCH decisions. The vertical slice test(s) inform our decisions and design choices going forward, but doesn't define them. When decisions get locked in, we shred the experiment down to those core features/decisions. Then use it as the live testing ground for the next experimental feature. 

I bring this up because *I'm* ignorant of Unity/C# and **can't** help you if you get lost in the weeds... I'm operating at the edge of my abilities/knowledge (which thrills me -- I aim to live on that frontier at all times), but I'm not one of those bungie-jumping thrill seekers tempting fate... I'm actively pushing that frontier outward to know more -- to see the *solution space* generally, and grab as many insights/patterns as I can, and throw them at everything else in life to see what sticks/transfers. I feel I *really* need to repeat: **I. Can. Not. Help. You.**... when it comes to writing or debugging C#, or literally *any* Unity features/tools/frameworks/patterns... you **do** have my logic/systems and high level expertise, my human vision to assess assets/behavior on screen (you've been putting a lot of faith in still screenshots -- those kept missing the banding artifacts because the player needed to be moving), my ability to search/research online for you, click on shit.... You do have access to my informed insights and expertise in:

- Game Design: high-level, pillars, mechanics, economies, algorithms (levels, AI... all of it), marketing (I know what's good, the psychographic bins of Gamers (Big 5, 1:1), who my target gamer is for any given project: ICP, and what would build clout and momentum)
   - **Note:** I have *zero* passion for marketing... it's just one of those weird savant superpowers I inherited from a systems-obsessed, Autistic world-view.
- General coding principals: I can write code. Maybe not for NASA (but I expect your code to pass NASA standards). It takes me a long time, and I fall into a lot of rabbit holes chasing specific obscure features that no one will care about but me (in direct violation of that screaming voice of the Marketer in my head), and experimenting with weird architectures that don't ship products. That's why I'm offloading most of that to *you*. The less shiny code artifacts I stumble upon that make me want to deviate or pivot hard -- the less chance I will. But **one** of us has to be driving this ship...
   - But I can write specific functions or design certain algorithms -- that's my specialty. Not intricate networking code, or scene loading, or all the horse labor for making a 3D game work in Unity. But game AI, pathfinding, level generation, PCG algorithms, game systems (logic) and economies... those I can do. You can offload "need clever human creativity" to me if you ever need more than 100 LOC to manifest a behavior or response/artifact in the game.
- Game systems: again... I mention it again. Because for this project I will ask you to write X, Y, Z for AI behavior, level generation, graph mutations, etc... and likely discard them to write myself (I don't even know other humans that do game systems well), especially for the game AI, behaviors, and FSMs. I might see something clever in your code/solution, and steal it (I'll tell you), or give another 49 page info dump like I am right now.
- Math: I'm decent at math. 
   - Zen master at algebra -> first year-calc, and can quickly onboard to tackle anything through undergrad diff-eq, ODE, etc.. 
   - Higher than that without clear, tangible buyin/engagement of topic (like Quantum Mechanics... took twice, failed once... I *still* don't get the Dirac Notation -- was too embarressed to ask at the time, and it never came back up) you'll lose me. 
   - I'm very familiar with Graph Theory, though a zen-master with notation. But this sprint will likely see me getting the graph structure designs from you to see where you're at... and then writing my own (in psuedocode (maybe Python or JS or a bastardization of the two) and pictures). 
- Art: I can draw what I can see... and that's about it. No graphical design skills on the computer, just tight sketches and graphite work. No skill or patience for pixel art, or textures... but I have clever algorithmic ideas for generating (noise functions, filters, etc.) some textures automatically (batching them, picking the good ones, maybe dropping those in a GA loop...). I can probably prompt other AIs to generate really good concept art. But I will be no help when it comes to 3D modeling -- I'm hoping between you, Blender MCP/plugin and Unity MCP... we'll figure that out together. But animations, if I can script them... I might have some advantage there. 
 

 So... to wrap up a barely coherant info dump:
 - Default to Approach B. 
 - Plan out a vertical slice side quest sprint as an isolated, quick-and-dirty, playable experiment with the sole purpose to move fast, break things, and learn.
 
 Give me your honest, critical assessment of our options at this fork in the road. If Approach A seems viable as a separate side quest sprint, to be done in a separate worktree... then dispatch a subagent (it can be Opus for depth and ponderous analysis of the situation) to review this sprint S1A, the ROADMAP, the Approach A Vertical Slice pitch, and any relevant research or future sprint/planning docs she wants -- to distil into a Sprint 1 Side Quest [S1.SQA] handoff prompt doc in docs/handoff-prompts/current/. 
   - **Note:** If a subagent can spawn other subagents, encourage Opus sub to spawn Sonnet subagents to crunch docs/research for targeted context. Otherwise, *you* should dispatch a few Sonnet agents to read docs/research to pull the important insights into a final handoff report for the Opus subagent to reference as they design/plan out the sprint side quest. 

---

### Sprint 2 Review:

  Defer (already tracked or low-risk):
  - Node/EdgeObservationState duplication (works, refactor when a third state type appears)
  - Per-frame allocations (C3, already documented)
  - ObservationInput concrete type (changing now breaks the portal wiring we just validated)
  - lockDebugVerbose unused (cosmetic)
  - Mutable struct risk (safe today, flag for co-op sprint)
  - Null graph path untested (defensive edge case)

### Review Response:

● This is the correct approach. Three specific affirmations:

  [DefaultExecutionOrder] over push model is the right call. The push model (NodeStreamingController calling GraphRuntimeHost.TickObservation()) would solve ordering but          
  introduces a dependency direction that doesn't exist today and adds a new public method to the host's surface. The attributes are Unity's idiomatic solution, zero coupling      
  change, and the ordering constraint is visible at the class declaration rather than buried in a call chain. Two lines, done.                                                     
                                                                                                                                                                                   
  ForceNodeLock/ForceEdgeLock for test setup is better test design. Testing "after Reset, previously locked nodes are unlocked" via real lock state is an observable-behavior      
  assertion. The alternative -- mocking internals or using FakeObservationInputSource to tick state into existence -- would test implementation details and break on refactors that
   preserve behavior. This avoids the "fake coverage" trap where tests pass but don't prove anything about the code path that actually runs in production.

  Deferrals are correctly scoped for jam. C5 (debug overrides unguarded): the F6 overlay requires deliberate keyboard navigation (F6 toggle, arrow keys, L/U/C) -- no player
  stumbles into this accidentally. Near-zero risk for a jam build. C6 (mutable struct shared List): the only consumer today is OnGUI, which reads within a single frame. The risk
  materializes when a co-op authority model holds references across tick boundaries -- that's a co-op sprint concern, not Sprint 2.