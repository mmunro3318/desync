# Run 0 — Unity 6.4 Landscape and Tooling Taxonomy for a Small-Team Co-op Horror Prototype

## Overview
This report maps the Unity 6.4 ecosystem for a programmer-led team coming from web development, with the goal of making later design and implementation research more legible. Unity 6.4 is a current Unity 6 release with documented engine updates, and Unity’s ecosystem now spans classic GameObject/MonoBehaviour workflows, data-oriented ECS/Entities workflows, multiple rendering pipelines, Unity Package Manager packages, Unity Gaming Services, and several multiplayer networking options.[cite:2][cite:106][cite:90]

For this project, the recommended default is **GameObject + MonoBehaviour + URP + new Input System + NGO + UGS support services where needed**, not ECS-first, HDRP-first, or custom-engineering-heavy. That recommendation follows Unity’s current docs, Unity’s own multiplayer documentation, and the practical fit of those tools for a 2–4 player first-person co-op prototype with portals, shifting house logic, graybox iteration, and AI-assisted development.[cite:27][cite:31][cite:38][cite:41][cite:88]

## Executive Summary
The core mental shift from MERN to Unity is that Unity is not primarily a request/response application framework; it is a real-time simulation environment built around scenes, serialized assets, GameObjects, components, and frame-by-frame update loops.[cite:61][cite:63][cite:81] The closest web analogy is not React, Express, or a database ORM, but a live runtime where data, objects, rendering, physics, and networking all co-exist inside one editor-driven application model.[cite:61][cite:62][cite:81]

For a small co-op horror prototype, the most practical path is the classic Unity stack rather than an ECS-first or DOTS-first architecture. Unity’s Entities package exists and is documented as a different workflow centered on data-oriented entity/component/system patterns, but it adds conceptual and implementation overhead that is usually not the best starting point for a first Unity production unless the game’s scale or performance profile clearly demands it.[cite:106][cite:107][cite:25]

On rendering, Unity’s current strategy strongly favors URP going forward, and Unity has published both a render-pipeline comparison and a render-pipeline strategy update for 2026. For a horror prototype that needs flexible lighting, acceptable performance, and sane team complexity, URP is the default recommendation unless a later visual target clearly forces a different decision.[cite:27][cite:31]

On networking, Unity’s own stack for GameObject-based multiplayer is Netcode for GameObjects, supported by Unity multiplayer docs, quickstarts, and Multiplayer Services integrations. Community alternatives such as Mirror, FishNet, and Photon each have strengths, but if the team is already leaning toward NGO and wants the most direct path through official docs and Unity-supported workflows, NGO is a reasonable default for a 2–4 player co-op prototype.[cite:38][cite:41][cite:37][cite:8][cite:11]

## Unity’s Mental Model for a Web Developer
### What Unity is actually made of
Unity projects are built from **Assets**, **Scenes**, **GameObjects**, and **Components**. A GameObject is the container placed in a scene, and Components are the attached behaviors or data-bearing pieces that define what that object does, how it renders, how it collides, or how it behaves at runtime.[cite:61][cite:63]

That means the closest mental model is: a scene is not a webpage route, and a GameObject is not a React component. A Unity scene is more like an entire loaded simulation space, while a GameObject is a runtime object node, and Components are modular capabilities attached to it.[cite:61][cite:63]

Unity’s runtime is frame-driven. Scripts often respond to engine lifecycle events such as initialization, per-frame update, physics update, collision callbacks, and scene load/unload transitions, which is very different from the event-loop and request-lifecycle emphasis of MERN applications.[cite:61][cite:81]

### What feels familiar vs unfamiliar from MERN
Some concepts will feel familiar:
- Prefabs are somewhat analogous to reusable component templates, but with serialization and editor-authored defaults rather than JSX composition.[cite:62]
- ScriptableObjects are somewhat analogous to reusable data assets or config documents, but they live as Unity assets and can be referenced directly inside scenes and prefabs.[cite:85][cite:87][cite:89]
- Packages installed through Unity Package Manager feel a bit like npm packages, though they are integrated into Unity’s editor/package ecosystem rather than a Node runtime.[cite:21][cite:90]

What is unfamiliar is more important:
- Unity serializes scene and asset state into files the editor understands, so “where truth lives” is a major architectural issue.[cite:62][cite:85]
- Rendering, physics, animation, audio, input, and networking are not separate service layers; they are all part of one running engine.[cite:27][cite:81][cite:88]
- Performance decisions are often shaped by CPU frame time, GPU frame time, memory churn, draw calls, physics cost, and network replication cost rather than by API latency or bundle size alone.[cite:2][cite:81][cite:84]

## The Core Stack Taxonomy
### 1. Runtime object model: GameObject/MonoBehaviour vs ECS/Entities
Unity currently supports two major programming paradigms.

**GameObject + MonoBehaviour** is the traditional and still most common workflow for general Unity gameplay. It aligns with scenes, prefabs, the Inspector, classic first-person gameplay patterns, and Netcode for GameObjects.[cite:61][cite:38][cite:41]

**Entities / ECS / DOTS** is Unity’s data-oriented workflow. Unity’s docs describe ECS as organizing logic around entities, components as pure data, and systems that process groups of entities, with the goal of performance and scalability.[cite:106][cite:107][cite:25]

For this project, ECS should be treated as a **specialized optimization-oriented ecosystem**, not the default foundation. The prototype’s main challenges are architectural clarity, scene/prefab discipline, multiplayer authority, hidden-state observability, and rapid iteration—not massive entity counts or simulation throughput that obviously require DOTS from day one.[cite:106][cite:107][cite:38]

### Recommended choice
Use **GameObject/MonoBehaviour as the primary architecture**. Only introduce ECS later for isolated subsystems if profiling and concrete bottlenecks justify it.[cite:106][cite:107]

## Rendering and graphics taxonomy
### Built-in vs URP vs HDRP
Unity documents multiple render pipelines and provides an official feature comparison. In current Unity strategy guidance, URP is the main forward-looking universal pipeline, while Unity’s 2026 render-pipeline strategy clarifies how Unity sees URP and HDRP moving forward.[cite:27][cite:31]

- **Built-in Render Pipeline**: legacy, widely understood, but not the strategic default for new work in Unity 6.[cite:27][cite:31]
- **URP**: the general-purpose modern default, designed to work broadly across platforms and project types.[cite:27][cite:31]
- **HDRP**: aimed at high-end visual targets, but with higher complexity and a narrower best-fit profile.[cite:27][cite:31]

For a small-team spatial horror prototype, URP is the recommended default because it offers modern rendering support without pushing the project into an unnecessarily expensive graphics pipeline. It is the best match for graybox-first development, indoor lighting iteration, and later horror-specific polish without overcommitting the team too early.[cite:27][cite:31]

### What matters most for this game
For this project, rendering decisions should be framed around:
- indoor lighting control,
- shadows and light leakage,
- iteration speed,
- acceptable performance during co-op play,
- and readability of graybox spaces before final art direction.[cite:27][cite:31][cite:2]

That means the graphics question is not “what looks most impressive?” but “what lets the team debug and ship believable space, mood, and player readability without drowning in pipeline complexity?” URP best fits that framing.[cite:27][cite:31]

## Package and services taxonomy
### Unity Package Manager
Unity Package Manager is Unity’s package ecosystem for adding official and third-party packages into a project. It fills the role closest to npm in a Unity workflow, but packages are editor/runtime integrations rather than normal JavaScript libraries.[cite:21][cite:90]

Important categories for this project include:
- Multiplayer packages such as Netcode for GameObjects.[cite:38]
- Addressables for asset loading and content management.[cite:44]
- Input System for modern input mapping and rebinding workflows.[cite:71]
- Cinemachine for camera workflows when needed.[cite:99]
- Entities only if a later subsystem truly needs data-oriented architecture.[cite:106][cite:107]

### Unity Gaming Services and multiplayer-adjacent services
Unity’s services documentation covers a broader services layer around gameplay systems. For multiplayer projects, the relevant categories include Multiplayer Services, Relay, Lobby, and Matchmaker, depending on how players discover and connect to sessions.[cite:90][cite:88][cite:52][cite:43]

These services are **not** the same thing as gameplay networking code. NGO handles synchronized gameplay objects and networked runtime behavior; services like Lobby and Relay help players meet and connect, while Matchmaker helps automate session assignment in more structured flows.[cite:38][cite:41][cite:43][cite:52]

For this project, the likely near-term rule is simple:
- Use **NGO** for gameplay networking.[cite:38][cite:41]
- Add **Relay/Lobby** if internet-based play and friend-session flows become necessary.[cite:43][cite:90]
- Delay Matchmaker unless the project actually needs automated public matchmaking.[cite:52]

## Asset and content taxonomy
### Scenes, prefabs, ScriptableObjects, and Addressables
A Unity project’s content model is easier to understand when these are kept distinct.

- **Scenes** hold placed objects and world composition.[cite:62]
- **Prefabs** are reusable object templates instantiated in scenes or at runtime.[cite:62]
- **ScriptableObjects** are asset-based data containers that Unity recommends for reducing duplicated data and separating data from behavior where appropriate.[cite:85][cite:87]
- **Addressables** provide an asset management and loading system for content that may be loaded dynamically or organized more flexibly than direct scene references allow.[cite:44][cite:50]

The practical architectural lesson is that each content type should have a clear responsibility. Scenes should describe composition, prefabs should describe reusable object structure, ScriptableObjects should describe reusable data definitions, and runtime state should not be silently hidden in whichever one happened to be convenient.[cite:62][cite:85][cite:87]

For your prototype, this matters because impossible-space logic, room graph definitions, interactable definitions, and observation rules will become brittle fast if editor-authored content and runtime truth are mixed carelessly. That is one of the biggest Unity-specific architecture traps to avoid in later runs.[cite:62][cite:85][cite:87]

## Input, camera, and player-control taxonomy
Unity has both an older legacy input approach and the newer Input System package. Current Unity practice increasingly centers the newer Input System for action maps, device abstraction, and more maintainable control configuration.[cite:71][cite:101][cite:103]

For a first-person co-op prototype, the important distinction is that input should be treated as **named gameplay actions**, not raw key polling scattered across scripts. That becomes especially important once local testing, rebinding, held-item logic, and UI-state-specific controls appear.[cite:71][cite:101][cite:103]

Cinemachine is a separate camera tooling layer in Unity’s ecosystem, useful when advanced camera behavior is needed. For a straightforward first-person horror prototype, it may not be central at first, but it is part of the broader tooling map.[cite:99][cite:105]

### Recommended choice
Use the **new Input System**, even if it adds a little early learning overhead, because it aligns better with future maintainability and multiplayer-adjacent control complexity than the legacy input approach.[cite:71][cite:101]

## Physics taxonomy
Unity’s built-in 3D physics system is a core engine subsystem documented in the Unity manual and tied to colliders, rigidbodies, triggers, and physics queries. For a first-person spatial-horror prototype, physics is less about advanced simulation and more about dependable collision, triggers, interaction checks, player movement support, and deterministic-enough gameplay rules for networked play.[cite:81][cite:84]

This means the team should think of physics as a gameplay infrastructure layer rather than as spectacle. Doors, held objects, triggers, room volumes, observation zones, and interaction checks will likely lean on standard physics concepts long before any advanced custom simulation is necessary.[cite:81][cite:84]

## Multiplayer taxonomy
### The main options
Unity’s officially supported GameObject-based multiplayer stack is **Netcode for GameObjects (NGO)**. Unity provides the package docs, manual, quickstarts, and service integrations around it.[cite:38][cite:41][cite:48][cite:88]

In the broader community, commonly discussed alternatives include **Mirror**, **FishNet**, and **Photon/Fusion**. Community comparisons generally frame these as trade-offs among official support, maturity, performance patterns, feature set, hosting assumptions, and learning curves rather than as a single universal “best” answer.[cite:8][cite:11][cite:110]

Unity now also documents direct comparisons in some service contexts, including NGO vs Mirror for Relay-related workflows. That matters because Unity is not pretending alternatives do not exist; it is explicitly situating NGO inside a broader multiplayer ecosystem.[cite:37]

### Practical comparison for this project
| Option | Best fit | Tradeoff summary |
|---|---|---|
| NGO | Small-to-mid GameObject-based Unity multiplayer projects; strongest official docs path | Best official alignment, but some community developers still view alternatives as stronger in certain advanced or performance-heavy scenarios.[cite:38][cite:41][cite:37] |
| Mirror | Teams that value a long-running community solution and familiar high-level networking concepts | Strong community history, but less directly aligned with Unity’s current official multiplayer stack and services path.[cite:37][cite:8] |
| FishNet | Teams willing to rely more heavily on a community-driven modern alternative | Attractive feature reputation in community discussions, but not the default path through Unity’s own documentation ecosystem.[cite:110][cite:11] |
| Photon / Fusion | Teams prioritizing Photon’s ecosystem or specific netcode model | Powerful, but introduces a more external-platform-centered path and may be overkill for a first Unity co-op prototype.[cite:8][cite:11] |

### Recommended choice
Commit to **NGO** unless a concrete blocker emerges. The reasons are straightforward:
- It is the best-aligned path with Unity’s official docs and onboarding materials.[cite:38][cite:41][cite:48]
- It fits GameObject/MonoBehaviour workflows, which are already the recommended core architecture for this project.[cite:38][cite:41]
- It lowers the number of unknowns for a team already learning Unity fundamentals.[cite:38][cite:41][cite:37]

The project should not spend its earliest phase optimizing for a hypothetical future need that may never materialize. It should optimize for learnability, official support, and a clean path to a working 2–4 player vertical slice.[cite:38][cite:41][cite:37]

## What Unity 6.4 changes that matters here
Unity 6.4 includes documented engine updates, and Unity’s current roadmap framing emphasizes ongoing engine modernization plus changes around rendering strategy and tooling. Not every Unity 6.4 feature matters equally to this prototype; the relevant takeaway is that the project is being built on a current engine branch with active updates rather than on a stale legacy baseline.[cite:2][cite:6][cite:82]

The most important practical implication is not any single flashy feature. It is that later research should assume **Unity 6-era docs, current package versions, and current render/multiplayer guidance**, not older Unity 2019/2020-era blog habits that still dominate many tutorials.[cite:2][cite:31][cite:38]

## Recommended default stack for this repo
The default stack recommendation for the project is:

- **Engine version**: Unity 6.4.[cite:2]
- **Programming model**: GameObject + MonoBehaviour first; no ECS-first architecture.[cite:61][cite:106][cite:107]
- **Language**: C# only, using Unity’s normal scripting workflow.[cite:61]
- **Rendering**: URP.[cite:27][cite:31]
- **Networking**: Netcode for GameObjects.[cite:38][cite:41][cite:48]
- **Support services**: Add Relay/Lobby only when internet session flows require them.[cite:43][cite:90][cite:52]
- **Input**: New Input System.[cite:71][cite:101]
- **Data assets**: ScriptableObjects for reusable data definitions, not as a magical solution to every architecture problem.[cite:85][cite:87][cite:89]
- **Content management**: Start simple; use Addressables deliberately when dynamic asset loading needs justify them.[cite:44][cite:50]

This stack is opinionated but intentionally conservative. It minimizes the number of simultaneous framework bets while preserving a path to sophisticated gameplay systems later.[cite:27][cite:38][cite:85]

## Taxonomy map: what each major tool category is for
| Category | What it is | Why it matters here | Recommendation |
|---|---|---|---|
| GameObjects/Components | Unity’s classic object model.[cite:61] | Core gameplay foundation for a first Unity prototype.[cite:61] | Use as default. |
| MonoBehaviour scripts | Attached C# behavior scripts in classic Unity workflow.[cite:61] | Fastest path to playable systems and debugging. | Use as default. |
| ECS / Entities | Data-oriented architecture focused on scalability/performance.[cite:106][cite:107] | Useful only if later profiling proves the need. | Do not start here. |
| URP | Modern universal rendering pipeline.[cite:27][cite:31] | Best fit for prototype horror visuals and sane complexity. | Use as default. |
| HDRP | Higher-end rendering path.[cite:27][cite:31] | More complexity than this project likely needs early. | Avoid unless later justified. |
| ScriptableObjects | Asset-based data containers.[cite:85][cite:87] | Good for reusable definitions and configuration. | Use carefully, not everywhere. |
| Prefabs | Reusable object templates.[cite:62] | Essential for interactables, props, players, doors, items. | Use heavily but hygienically. |
| Scenes | World composition containers.[cite:62] | Important for bootstrap, test scenes, graybox maps. | Keep disciplined. |
| Addressables | Asset loading/management system.[cite:44] | Helpful later for content organization and dynamic loading. | Introduce when needed. |
| Input System | Modern input mapping package.[cite:71][cite:101] | Better for maintainable first-person and UI controls. | Use as default. |
| Cinemachine | Camera toolset.[cite:99] | Useful for some camera workflows, possibly limited in strict FPS play. | Optional. |
| NGO | Unity’s GameObject multiplayer framework.[cite:38][cite:41] | Best official path for 2–4 player co-op. | Use as default. |
| Relay / Lobby | Connection/session support services.[cite:43][cite:90] | Needed for internet session flows, not local gameplay logic. | Add only when needed. |

## What to ignore for now
A beginner Unity team can lose months by trying to “choose the perfect stack” before learning the engine’s everyday constraints. For this project, the following should stay out of the critical path unless a later run proves otherwise:

- ECS-first architecture.[cite:106][cite:107]
- HDRP-first visual ambition.[cite:27][cite:31]
- Matchmaking infrastructure before the prototype needs real public session flow.[cite:52]
- Addressables complexity before content-loading needs are real.[cite:44]
- Community-framework shopping after already leaning toward NGO without a concrete technical blocker.[cite:37][cite:8][cite:11]

## Guidance tailored to this project
This prototype is not just “a Unity game.” It is a co-op spatial-horror game with impossible-house logic, hidden-state systems, observation rules, multiplayer synchronization needs, and a likely graybox-heavy pipeline. That profile makes **clarity of ownership, runtime-vs-definition separation, observability, and narrow system boundaries** more important than exotic engine choices.[cite:38][cite:85][cite:87]

The best early architecture is therefore not the most clever one. It is the one that makes it obvious where room definitions live, where spawned runtime state lives, who owns authoritative network state, how lighting is debugged, how prefabs are reused, and how Claude can be given bounded implementation tasks without generating giant mystery scripts.[cite:38][cite:85][cite:87]

## Recommended defaults for later runs
The next runs should assume these defaults unless evidence later overturns them:

1. **Use Unity 6-era official docs first.** Older tutorial material should be treated cautiously unless it still matches current Unity guidance.[cite:2][cite:31][cite:38]
2. **Assume GameObject/MonoBehaviour architecture.** Later architectural advice should optimize that path rather than drifting into ECS unless explicitly justified.[cite:61][cite:106]
3. **Assume URP.** Graphics and lighting research should focus on URP configuration, indoor lighting, and horror-friendly iteration.[cite:27][cite:31]
4. **Assume NGO.** Multiplayer runs should focus first on making NGO work well for small co-op rather than reopening the framework choice each time.[cite:38][cite:41][cite:37]
5. **Assume the new Input System.** Controller, interaction, and held-item recommendations should be built around action maps and modern input handling.[cite:71][cite:101]

## Conclusion
Run 0 establishes the taxonomy: Unity 6.4 gives the project a modern engine base; the correct beginner-friendly production path is the classic GameObject/MonoBehaviour workflow; URP is the best rendering default; NGO is the right multiplayer default for this repo; and services such as Relay, Lobby, Addressables, or ECS should be introduced only when they solve specific real problems.[cite:2][cite:27][cite:31][cite:38][cite:43][cite:106]

That framing should reduce future confusion. The purpose of later runs is no longer to decide what Unity *is* in the abstract, but to produce repo-ready implementation guidance for architecture, scenes, prefabs, lighting, multiplayer, debugging, and Claude guardrails on top of this now-chosen baseline.[cite:62][cite:85][cite:38]
