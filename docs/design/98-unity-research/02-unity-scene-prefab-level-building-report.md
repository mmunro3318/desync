# Run 2 — Unity Scene, Prefab, and Level-Building Discipline for a Small-Team Co-op Horror Prototype

## Overview
This report covers scene architecture, prefab strategy, graybox level-creation workflows, and the discipline needed to keep Unity's content model from becoming a source of hidden runtime truth. It addresses Checklist B from the project's research brief, with all recommendations tailored to a Unity 6.4 co-op horror prototype using the classic GameObject/MonoBehaviour + NGO + URP stack established in Runs 0 and 1.[cite:62][cite:193][cite:233]

The central recommendation is a **multi-scene architecture anchored by a persistent bootstrap scene, with gameplay content organized into additive scenes, all interactables and reusable elements expressed as prefabs, and graybox geometry built in ProBuilder from the start**. Scene-authored transforms, room geometry, and inspector-wired references should be treated as composition tools, not hidden sources of live gameplay state.[cite:202][cite:203][cite:211][cite:233]

## Executive Summary
Unity's content model—scenes, prefabs, and assets—is powerful but silently punishing when ownership rules blur. The two most common failure modes in growing Unity projects are: first, scenes that quietly accumulate runtime truth alongside world composition (nobody can tell what is a layout artifact vs. a live gameplay contract); and second, prefabs that grow ungoverned until a change breaks six connected behaviors with no clear ownership boundary.[cite:62][cite:203][cite:225]

For this project, those failure modes are especially dangerous because the impossible-house prototype depends on a graph-based spatial runtime, portals, observation locks, and multiplayer synchronization—systems that are hard enough to debug without also wondering whether some critical room-state datum is hiding in a scene GameObjects's serialized field.[cite:233][cite:230]

The solution is consistent discipline, not exotic architecture: scenes own composition, prefabs own reusable structure, ScriptableObjects own definitions, runtime systems own live state, and graybox geometry stays iteration-friendly rather than becoming permanent load-bearing structure.[cite:62][cite:193][cite:202][cite:211]

## Unity scene architecture fundamentals
### What scenes are for
A Unity scene is not just a level file. It is a container for composed object hierarchies, serialized component values, and world-space relationships. Unity's multi-scene editing documentation, which has been part of Unity's official guidance since Unity 5, describes scenes as owning their section of the hierarchy and being loadable, unloadable, and additive—meaning multiple scenes can be open and active simultaneously.[cite:211][cite:227]

For any project beyond a simple single-scene prototype, scenes should carry a limited scope: this scene owns the persistent services, this scene owns the UI, this scene owns one environment section, this scene owns test content. That separation prevents merge conflicts between team members, keeps the hierarchy readable, and makes additive loading natural.[cite:203][cite:211][cite:225]

### Multi-scene workflow and additive loading
Unity officially recommends using additive scene loading rather than abusing `DontDestroyOnLoad` for persistence. The Unity multi-scene manual explicitly states: "It is recommended to avoid using DontDestroyOnLoad to persist manager GameObjects that you want to survive across scene loads. Instead, create a manager scene that has all your managers and use SceneManager.LoadScene in Additive mode."[cite:211]

Additive loading gives the project a clean persistence model: a bootstrap or manager scene stays resident for the session, and gameplay or environment scenes are loaded and unloaded additively alongside it.[cite:202][cite:203][cite:208] This approach also makes it natural to test individual gameplay scenes without the full game loading path—a significant benefit for rapid iteration.[cite:203][cite:208]

The practical pattern for this repo:
- **Bootstrap Scene**: NetworkManager, session coordination, core services, persistent UI. Never unloaded.
- **Gameplay Scene**: Player spawning, environment layout, room graph, test triggers.
- **Environment Sections (optional)**: Separate room cluster scenes that can stream in and out if the spatial runtime demands it.
- **Test/Development Scenes**: Isolated prefab test beds, lighting experiments, interaction checks.[cite:202][cite:203][cite:208]

### Scene and NGO interaction
Unity's NGO documentation explicitly requires that any scene load involving synchronized multiplayer objects must go through `NetworkSceneManager`, not raw `SceneManager.LoadScene`. Bypassing NGO's scene pipeline causes client disconnects, GUID mismatches, and clients failing to load or rejecting scene transitions.[cite:233][cite:230][cite:231]

**Critical NGO scene rule**: Only the server calls `NetworkManager.Singleton.SceneManager.LoadScene(...)` for multiplayer transitions. Clients follow server-driven scene events automatically. NetworkManager must persist across scene loads via the bootstrap scene approach or `DontDestroyOnLoad`, otherwise clients lose their connection when the scene hosting NetworkManager unloads.[cite:233][cite:230]

This is one of the most common NGO implementation bugs. Establishing the bootstrap scene pattern early—where NetworkManager lives in a scene that never unloads—prevents this problem entirely.[cite:230][cite:233]

## Scene discipline rules for this repo
### Scene ownership
- Each scene owns its own hierarchy. Cross-scene references through the Inspector are not supported and cause null references at runtime.[cite:211][cite:203]
- Move GameObjects between scenes explicitly using `SceneManager.MoveGameObjectToScene` when needed, not by accident.[cite:211]
- Scene-authored serialized data (positions, references, inspector values) are composition artifacts, not authoritative runtime state.[cite:62][cite:203]

### Avoiding scene-authored runtime truth
The most important hygiene rule for this project: **do not let the scene become a place where gameplay logic secretly depends on how objects are arranged in the hierarchy**. Specific failure patterns to watch for:
- Room graph topology inferred from the scene hierarchy instead of explicit definition data.
- Observation-lock rules embedded in Inspector fields scattered across scene objects.
- Multiplayer authority decisions hardcoded to scene-instance objects rather than tracked by the runtime.[cite:38][cite:62][cite:203]

Every time a design decision is captured in scene serialization, ask: "If we reload this scene, do we lose this state? Should we?" If the answer is unclear, the datum probably belongs in a ScriptableObject definition, a runtime state model, or an NGO-synchronized component instead.[cite:202][cite:203]

### Source control discipline
Scene files in Unity are YAML and cause merge conflicts when multiple developers edit the same scene simultaneously. Community practice consistently recommends two mitigations:[cite:225][cite:226]
1. Use Unity's built-in SmartMerge (UnityYAMLMerge) and configure it in `.gitattributes`.[cite:226]
2. Break scene content into prefabs so team members can work on prefab files rather than the scene file itself.[cite:225]

A multi-scene workflow naturally limits conflict scope because two developers rarely need to edit different scenes at the same time.[cite:203][cite:225]

## Prefab strategy
### What prefabs are for
A Unity prefab is a saved, reusable object template that can be instantiated in scenes and at runtime, maintains a live link back to its asset, and propagates changes made to the asset to all instances.[cite:194][cite:190] Prefabs are the primary mechanism for reusing object structure and behavior across scenes.

For this project, the vast majority of gameplay elements should be prefabs:
- Player character and controller.
- All interactable objects (doors, drawers, items, switches).
- Props and decorative objects intended for repetition.
- Trigger volumes and observation zones.
- Debug overlay UI elements.
- Network-spawned objects.[cite:194][cite:190][cite:62]

### Nested prefabs
Unity supports nesting prefab instances inside other prefab assets. Nested prefabs maintain their own link to their source asset while also forming part of a parent prefab asset, which means changes to the child prefab propagate to all parents that use it.[cite:194][cite:190]

For this project, nested prefabs are well suited to:
- A door prefab that contains a lockable latch sub-prefab.
- A room prop that contains a common grab interaction sub-prefab.
- A puzzle object composed of reusable trigger and indicator sub-prefabs.[cite:194][cite:190][cite:196]

The key rule is that nested prefab hierarchies should be deliberate: nest because the child is genuinely a reusable subunit with its own logic, not because the hierarchy happened to end up that way.[cite:194][cite:215]

### Prefab variants
A prefab variant is a child prefab that inherits from a base prefab and can override specific properties, while propagating base changes to all variants automatically.[cite:217][cite:213] The community analogy is useful: nested prefabs are composition, prefab variants are inheritance.[cite:213][cite:215]

Use prefab variants when:
- A door archetype needs variants with different meshes, materials, or audible locked responses but identical core interaction logic.
- An enemy/NPC type needs several versions with shared base behavior and distinct overrides.
- A prop type has five visual variations but the same Physics/collider/interaction setup.[cite:212][cite:213][cite:217]

Avoid variants when:
- The "variants" are truly independent objects with no shared logic worth propagating.
- The inheritance chain is more than two or three levels deep—it becomes hard to reason about which level owns which override.[cite:213][cite:215]

### Prefab hygiene rules
Prefab hygiene failures turn manageable changes into cascading regressions. Common mistakes include:
- Modifying prefab instances in scenes and never applying the override back to the base, so the scene diverges silently from the asset.[cite:194]
- Creating duplicate copies rather than variants, which prevents propagation of shared changes.[cite:212][cite:213]
- Nesting prefabs so deeply that overrides become untrackable.[cite:215]
- Placing runtime-only prefabs (network-spawned objects, dynamic interactables) in scenes as static layout and blurring the line between layout and spawn system.[cite:62]

**Repo hygiene rules**:
- Always open prefabs in Prefab Mode to edit them; avoid direct scene-instance-only edits that are never applied back.
- Use variants when behavior is shared and differences are overrides; use separate prefabs when they are genuinely different archetypes.
- Keep prefab hierarchies shallow; compose rather than deeply nest.
- Clearly distinguish "scene decoration prefabs" (layout only) from "interactive gameplay prefabs" (logic-bearing) in folder organization.

## Graybox level creation workflow
### What graybox is and why it matters for this project
Graybox (or greybox, or blockout) is the practice of building levels from simple geometry first—often plain white or grey primitives—before investing in art. Unity Learn explicitly defines it as "primitive 3D shapes (which are often grey) used to block out the scene so you can implement the basic functionality" before committing to final art.[cite:193][cite:232]

For the impossible-house prototype, graybox is especially important because:
- Room graph topology, portal placement, and impossible spatial transitions depend on spatial logic that can only be tested inside a real level shape.
- Lighting, light leaks, and shadow behavior in indoor horror environments must be evaluated against actual room proportions.
- Player navigation, interactable reachability, and scale can be validated before a single art asset is created.[cite:189][cite:199][cite:221]

Getting into a playable graybox quickly and iterating on spatial layout before any art pass is one of the highest-leverage things the team can do early in the project.[cite:189][cite:193][cite:221]

### ProBuilder for graybox
The community consensus for Unity interior grayboxing is strongly in favor of ProBuilder rather than primitives-only or external DCC tools. ProBuilder is a Unity-native package for creating and editing 3D meshes directly inside the Unity editor, and it is well suited to interior architectural layout work.[cite:219][cite:207][cite:216]

Unity Learn uses ProBuilder for its official greybox prototype tutorials.[cite:200][cite:193] The community recommendation is consistent: "Just use ProBuilder if you are doing grayboxing."[cite:216]

**Why ProBuilder for this project**:
- Rooms, corridors, and impossible-space transitions require walls, floors, ceilings, and openings that cannot be easily built from scaled cubes alone.
- ProBuilder supports UV editing, grid snapping, and face extrusion, which makes it fast to construct interior architectural shapes without leaving Unity.
- ProBuilder meshes can be exported to external tools or replaced by final art without losing layout logic.
- Grid snapping with ProGrids ensures consistent module sizing—critical when rooms must connect, repeat, or reconfigure at runtime.[cite:207][cite:199][cite:219][cite:221]

### Graybox workflow for modular environments
Community practice distinguishes two approaches to graybox for modular environments.[cite:189][cite:221]

**Approach A: Primitives-first, modular-second**
Block out the full level using scaled cubes and planes to confirm spatial logic and player scale. Once layout is validated, replace primitives with ProBuilder-modeled modular pieces. This approach is recommended when the modular kit is not yet designed, since it avoids committing to module dimensions prematurely.[cite:189][cite:221]

**Approach B: Modular-first**
Establish module dimensions early (e.g., walls 300 cm tall, floors 4x4 m, doors 200 cm) and begin graybox with ProBuilder modules that reflect those final sizes. This is recommended when the modular kit's scale is known and the team wants to catch snapping, grid alignment, and light seam issues from the start.[cite:189][cite:199]

For this project, **Approach A** is likely the better starting point because the spatial runtime, portal logic, and room graph design are still being defined. Build the spatial prototype quickly with primitive cubes, validate movement, scale, and connectivity, then shift to ProBuilder modular pieces once the layout logic is stable.[cite:189][cite:221][cite:193]

### Grid and snapping discipline
Whatever graybox approach is used, snapping discipline is critical for modular indoor levels:
- Enable Unity's grid snapping in the viewport.
- Establish a base module size early and use it consistently (for example, 1 m grid unit, walls 3 m tall, floor tiles multiples of 2 m).
- Label ProBuilder pieces clearly (`Wall_2m`, `Floor_4x4`, `Door_Frame_2m`) from the start to avoid confusion when replacing or reusing them.[cite:189][cite:199]

Snapping failures early become light-leak sources and portal-seam problems later.[cite:189][cite:199]

### From graybox to art pass
Unity community practice consistently describes the graybox-to-art-pass transition as: level designer blocks out with graybox geometry, confirms all spatial, navigational, and gameplay questions are answered, then environment artist replaces graybox pieces with final modular art—usually by deleting graybox geometry section by section and substituting with matching art meshes.[cite:229]

For this project, the graybox should remain the **authoritative spatial reference** for as long as the impossible-house logic is still being iterated. Do not mix graybox and final art in the same scene until spatial design is settled, because art-pass geometry will constrain iteration speed.[cite:189][cite:229]

## Keeping scenes from becoming the source of hidden runtime truth
### The anti-pattern pattern
The most dangerous Unity architecture drift for this project is scenes that silently accumulate runtime contracts. It starts with convenience—"let's just put the room-state flag on this scene object's serialized field for now"—and grows until nobody is sure what is safe to change in a scene without breaking something invisible.[cite:62][cite:203]

Specific patterns to watch for:
- **Hardcoded scene references**: scripts in scenes using `GameObject.Find` or `FindObjectOfType` to locate runtime dependencies by scene-path string.
- **Inspector-wired runtime state**: serialized `bool isLocked` or `int roomIndex` fields that carry live game meaning, not just initial configuration.
- **Gameplay logic in scene-specific scripts**: scripts that only exist because of how this particular scene happens to be organized, rather than as general behaviors.[cite:62][cite:202][cite:203]

### The rule
Scene-authored data should describe how objects are composed and positioned, not what state the game is in. When a serialized field in a scene object begins to influence gameplay behavior meaningfully, it should be moved into a data definition (ScriptableObject), a runtime model, or a networked state variable—somewhere that makes its authority visible.[cite:62][cite:85][cite:202]

## Level design discipline specific to this prototype
### Room graph vs. scene hierarchy
The impossible-house prototype's core is a graph-based spatial runtime. That graph is a runtime system, not a scene hierarchy. The scene should show composed geometry and placed prefabs; the room graph should live in runtime data—whether that is a ScriptableObject definition loaded into a runtime instance, a runtime-built graph structure, or an NGO-synchronized state model.[cite:38][cite:202][cite:203]

If the room graph's topology starts to be implied by scene hierarchy shape—"room B is a child of room A because that is how the graph works"—the scene is doing work it should not do, and changing the graph later will require restructuring the scene hierarchy instead of just modifying the graph data.[cite:202][cite:203]

### Portal and impossible-space level concerns
Portals and shifting geometry add specific level-authoring constraints:
- Portal connection data (which portal connects to which destination) should live in data definitions, not hard-coded to scene object positions.
- Portal trigger volumes should be prefabs with explicit connection components, not scene-unique objects.
- Test levels for spatial-runtime validation should be isolated in test scenes that can be safely deleted or restructured without affecting the main gameplay scenes.[cite:202][cite:203][cite:233]

### Observation zones and trigger volumes
Observation locks, puzzle triggers, and hidden-state zones should each be authored as prefabs with clear component contracts rather than as unique scene-object scripts. This keeps the level designer's composition work separate from the gameplay programmer's logic contracts.[cite:62][cite:189]

## Source of truth summary

| Content type | Where it lives | What it should NOT do |
|---|---|---|
| Room archetypes / definitions | ScriptableObjects[cite:85] | Hold live per-session room state |
| Room graph topology | Runtime data model / ScriptableObject definition[cite:202] | Be implied by scene hierarchy |
| World composition | Scene files[cite:62] | Carry authoritative runtime state |
| Reusable objects | Prefabs[cite:194] | Duplicate instead of variant or nest |
| Graybox geometry | ProBuilder meshes in scene[cite:207] | Block art-pass replacement |
| Live gameplay state | NGO NetworkVariables / runtime models[cite:233] | Be serialized into scene or prefab inspectors |
| Per-player state | NGO-owned runtime data[cite:38] | Scatter across unrelated scene objects |

## Recommended defaults for this repo
### What Claude should generally do
- Use additive multi-scene loading and never fully replace the bootstrap scene.[cite:203][cite:208]
- Keep NetworkManager in a persistent bootstrap scene and always load gameplay scenes additively.[cite:230][cite:233]
- Use `NetworkManager.Singleton.SceneManager.LoadScene` for all multiplayer scene transitions, never raw `SceneManager.LoadScene`.[cite:233][cite:231]
- Build graybox geometry with ProBuilder and grid snapping from day one.[cite:207][cite:216][cite:219]
- Express interactables, triggers, and reusable gameplay objects as prefabs, not unique scene objects.[cite:62][cite:194]
- Use prefab variants for "same thing, different configuration" and nested prefabs for "this sub-object is reused by multiple parents."[cite:213][cite:217]
- Keep scenes clean of gameplay state; move live flags and runtime truth into dedicated state models or NGO-synchronized variables.[cite:203][cite:233]

### What Claude should generally avoid
- `DontDestroyOnLoad` scatter across many objects; use the persistent bootstrap scene instead.[cite:211][cite:208]
- Hardcoded `GameObject.Find` or `FindObjectOfType` calls for runtime dependencies.[cite:62]
- Unique scene-object scripts that encode level-specific logic that should be a component contract.[cite:62][cite:203]
- Allowing the room graph to be implied by scene hierarchy.[cite:202][cite:203]
- Mixing graybox geometry and art-pass geometry in the same scene before spatial logic is stable.[cite:189][cite:229]
- Prefab variants nesting more than two or three levels deep.[cite:215]
- Scene files as the place to resolve merge conflicts by "just taking one person's version"; use SmartMerge and prefab-centric workflow instead.[cite:225][cite:226]

## Conclusion
Scene and prefab discipline is not glamorous architecture work, but for this project it is load-bearing. The impossible-house prototype needs spatial layout, room graph logic, portal definitions, observation rules, and multiplayer authority to each live in their correct home—not bleed together into a scene YAML file that nobody trusts anymore.[cite:62][cite:202][cite:233]

The concrete disciplines are clear: a persistent bootstrap scene hosting NetworkManager, additive gameplay and environment scenes, ProBuilder graybox geometry built to consistent grid dimensions, prefabs for every reusable gameplay object, prefab variants for configuration variation, and an explicit rule that scenes describe composition while runtime state lives elsewhere.[cite:203][cite:208][cite:207][cite:194][cite:233]

Getting these foundations right in the first few weeks means every later run's guidance—lighting, multiplayer, debugging, AI guardrails—lands on stable ground rather than having to fight scene-state ambiguity on top of every other challenge.[cite:62][cite:202][cite:233]
