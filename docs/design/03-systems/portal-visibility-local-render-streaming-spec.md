# Portal Visibility / Local Render Streaming Spec

## Document objective
Define how impossible-space topology is presented believably from first-person camera views. This spec establishes the runtime rules for portal visibility, node activation, local render streaming, sightline continuity, and contradiction prevention so that graph mutations remain convincing rather than visually exposing the trick [file:12][file:13][file:14][file:15].

## Why this document exists
The project now has a conceptual stack for structural generation, authoritative topology, observation locks, and legal spatial mutation. The next problem is presentation. A house can mutate legally in graph space and still fail as horror if the player camera catches impossible contradictions, unloaded spaces, or inconsistent door views. This document exists to ensure the impossible house feels coherent from where the player stands, even when it is globally impossible [file:12][file:13].

## High-level thesis
The player should only ever see a locally consistent slice of the house: the node they occupy, a controlled set of adjacent/visible nodes, and any remote spaces exposed through authorized portal sightlines. Rendering should follow authoritative graph truth, but it should be constrained by local visibility rules so the player experiences believable continuity rather than omniscient contradiction [file:12][file:13][file:15].

## Design goals
This system should optimize for:
- believable impossible space from first-person view,
- strong portal/threshold continuity,
- stable local navigation readability,
- support for graph mutation without visual popping,
- and debug-first visibility into what is currently active and why [file:12][file:13][file:15].

## Questions this spec answers
- What parts of the house should be active/rendered at any moment?
- How do portals reveal remote or remapped spaces believably?
- How should local node activation respond to movement and mutation?
- What contradictions must never be shown to the player?
- What debug tooling is required to tune the system?

## Out of scope
This spec does **not** fully define:
- final art direction for portals or VFX,
- final occlusion-culling implementation details,
- advanced GPU optimization work,
- final multiplayer rendering authority,
- or all dimension-layer rendering rules.

It defines the local presentation harness that those later systems will use.

## Core design statement
**Render only the player’s locally valid world.** The player should inhabit one active node context, see a curated set of neighboring spaces, and view remote spaces only through graph-authorized openings and visibility rules. The system should hide impossible global truth while preserving convincing local truth [file:12][file:13].

## Relationship to prior docs
This spec assumes:
- the house builder produces graph-ready artifact data,
- the house graph owns authoritative topology,
- observation locks control mutation eligibility,
- and the mutation runtime can apply legal topology changes [file:12][file:13][file:14][file:15].

This document defines how those systems become camera-believable in first-person play.

## Primary concepts

### Active node
The graph node currently occupied by the player’s body/camera context.

### Warm node
A nearby node kept loaded or semi-loaded because it may become visible or traversable imminently.

### Portal-visible node
A node that is not locally occupied but is currently visible through a valid portal/threshold sightline.

### Active set
The set of nodes and presentation elements currently enabled for rendering/simulation at local scope.

### Streaming policy
The rule set that decides whether a node is cold, warm, active, or portal-visible.

### Contradiction
Any camera-visible state that exposes impossible spatial inconsistency in an uncontrolled way, such as a door showing one room from the hallway and a different incompatible room from a nearby side angle without a rule-backed transition.

## Presentation philosophy
This game does not need a globally truthful render of the house. It needs a **locally truthful illusion**. That means a player’s current node and its immediate thresholds must feel physically reliable, while deeper structure may be remapped, hidden, or selectively activated as long as the camera never exposes uncontrolled contradictions [file:12][file:13][file:15].

## Ownership model
To preserve narrow ownership and avoid giant scene managers, this spec recommends the following systems [file:13][file:14].

| System | Owns | Does not own |
|---|---|---|
| `LocalVisibilityService` | determining local active/warm/visible node sets | graph mutation decisions |
| `PortalVisibilityResolver` | evaluating portal-based sightlines | mutation scheduling |
| `NodeStreamingController` | activating/deactivating node presentation state | authoritative graph topology |
| `PortalRenderController` | portal window presentation and continuity | graph legality |
| `LocalRenderDebugOverlay` | visualizing active sets and sightlines | render authority decisions |

## Runtime versus definition split
This spec should preserve the same separation discipline used elsewhere in the project [file:12][file:13][file:14].

### Definition-side examples
- `StreamingPolicyDefinition`
- `PortalVisibilityRuleDefinition`
- `NodeActivationProfile`
- `LocalRenderPolicyDefinition`

### Runtime-side examples
- `LocalVisibilityState`
- `NodeStreamingState`
- `PortalVisibilityState`
- `ActiveSetSnapshot`
- `VisibilityRebuildRecord`

## What counts as locally renderable
A node should not be active just because it exists in the graph.

### Candidate local categories
- currently occupied node,
- adjacent reachable node,
- portal-visible node,
- mutation-transition support node,
- debug-forced visible node.

### Recommendation
Start with a compact active set: current node, direct neighbors, and portal-visible remote nodes only. Expand only when required by a specific presentation problem.

## Node activation states
The system should use a small finite set of activation modes.

### Suggested states
- `Cold`: inactive and not renderable,
- `Warm`: not fully active but prepared for fast activation,
- `Active`: fully present in local play space,
- `PortalVisible`: not traversed locally but currently rendered through a portal view,
- `Transitioning`: temporarily managed during node handoff or mutation.

## Local visibility rules
Local visibility should be resolved from graph truth and camera context rather than scene proximity hacks.

### Minimum local activation rule
The active set should include:
- the player’s current node,
- graph neighbors within configured depth,
- nodes visible through a portal with valid sightline continuity,
- and any node temporarily required to maintain transition continuity.

## Portal visibility model
Portals are the main way the game sells impossible space. They should not behave like ordinary hole-in-the-wall culling shortcuts; they are explicit visual contracts between graph-connected spaces.

### A portal may reveal
- its linked destination node,
- an intermediate staged view for continuity,
- or nothing beyond threshold if policy blocks visibility.

### A portal should never reveal
- an unloaded contradictory destination,
- a destination that disagrees with current graph binding,
- or multiple incompatible remote truths at once from the same resolved state.

## Portal continuity rule
When the player looks through a portal, the visible destination must match the current authoritative portal binding. If mutation changes that binding, the presentation must either defer the visible change until the sight rule allows it or perform a controlled transition that does not expose two incompatible truths simultaneously [file:12][file:13].

## Local traversal handoff
Crossing a threshold should feel continuous even if graph topology behind the scenes is strange.

### Handoff goals
- no obvious popping while crossing,
- no camera frame where both old and new local truths compete,
- destination node active before commitment,
- source node retained long enough to avoid visible teardown.

### Recommendation
Use a short threshold transition state where source and destination nodes remain active until the camera fully commits to the new node context.

## Streaming policy
Streaming should follow graph-local relevance, not only Euclidean distance.

### Inputs to streaming policy
- current node id,
- neighbor depth,
- portal sightlines,
- active mutation transition,
- player velocity/direction,
- debug override state.

### Design principle
A nearby but graph-disconnected room may matter less than a remote room visible through a portal. Graph truth should dominate over raw spatial distance.

## Contradictions to avoid
This system exists largely to prevent the player from seeing the trick at the wrong time.

### Hard contradictions
- a doorway shows a destination that does not match the current graph binding,
- a room unloads while still in direct view,
- the player can see geometry overlap from two mutually exclusive node states,
- mutation visibly rewires a currently visible corridor without a rule-backed transition,
- portal destination changes while still actively observed without a controlled policy.

### Soft contradictions
- noticeable pop-in near thresholds,
- warm-to-active lighting mismatches,
- slight temporal mismatch in portal preview,
- one-frame absence of far geometry during transition.

### Priority
Hard contradictions must be eliminated first. Soft contradictions can be tuned iteratively.

## Mutation-aware rendering
When the graph mutates, presentation must respond carefully.

### Rule
If a node or portal is currently visible in the player’s local view and the mutation system changes related topology, the render layer should either:
- defer visible presentation change until visibility conditions no longer expose it,
- or perform a controlled transition policy designed for that mutation family.

### Implication
The render layer needs awareness of current observation and active portal visibility state, even if it does not own mutation authority.

## Portal visibility resolution
A dedicated service should resolve what each portal currently shows.

### Inputs
- current `HouseGraphInstance`,
- active node,
- camera transform,
- portal binding,
- sightline state,
- mutation transition state.

### Outputs
- destination node id,
- portal-visible flag,
- render mode,
- continuity/transition requirement.

## Suggested render modes
- `ClosedOccluded`
- `LocalOpen`
- `PortalPreview`
- `TransitionBlend`
- `DebugForced`

These modes give you a controllable vocabulary without overcommitting to specific shader or camera tricks too early.

## Local simulation scope
Not every loaded node needs full simulation.

### Recommendation
Split simulation scope from render scope when useful.

### Example
- occupied node: full simulation,
- adjacent active node: partial simulation,
- portal-visible node: render-priority, reduced simulation,
- cold node: no local simulation.

This helps performance and keeps node presentation policy from becoming identical to all gameplay simulation rules.

## Suggested core classes
The exact names may evolve, but the following class map should be implementation-friendly.

| Class | Responsibility |
|---|---|
| `LocalVisibilityService` | computes the active local node set |
| `PortalVisibilityResolver` | determines what portals currently reveal |
| `NodeStreamingController` | applies activation state changes to node presentation |
| `NodePresentationBinder` | binds graph node ids to scene/prefab presentation roots |
| `PortalRenderController` | coordinates threshold visuals and remote node presentation |
| `LocalRenderState` | stores current active set and render-mode state |
| `LocalRenderDebugOverlay` | visualizes active, warm, portal-visible, and cold nodes |
| `PortalSightlineDebugView` | shows current portal visibility rays and destination mapping |

## Suggested interfaces
The system should expose narrow contracts and avoid direct dependency webs, consistent with the broader project rules [file:13][file:14].

```csharp
public interface ILocalVisibilityService
{
    ActiveSetSnapshot BuildActiveSet(string currentNodeId, Camera activeCamera);
}

public interface IPortalVisibilityResolver
{
    PortalVisibilityState Resolve(string portalId, Camera activeCamera, HouseGraphInstance graph);
}

public interface INodePresentationTarget
{
    string NodeId { get; }
    void ApplyStreamingState(NodeStreamingState state);
}
```

## Suggested data assets
To keep render policy tunable, the project should define a few focused assets rather than hardcoding every threshold rule [file:12][file:13][file:14].

### Suggested assets
- `StreamingPolicyDefinition`
- `PortalVisibilityRuleDefinition`
- `NodeActivationProfile`
- `LocalRenderPolicyDefinition`
- `ThresholdTransitionProfile`

## Recommended first implementation slice
The first pass should be aggressively narrow.

### Prototype policy
- one local player camera,
- one active node,
- neighbor depth of 1,
- portal-visible remote nodes allowed only through bound doorways,
- threshold handoff support for one doorway class,
- debug overlay showing active/warm/cold state and current portal destinations.

This fits the project’s broader milestone discipline of proving one major question at a time [file:12][file:13][file:15].

## Rules for threshold crossing
Thresholds are where the illusion will succeed or fail.

### Threshold crossing policy
- source node remains active through crossing commit,
- destination node becomes active before full commitment,
- portal view must remain consistent during the crossing window,
- source teardown may occur only after the player has clearly entered destination local context.

## Rules for node unloading
Unloading should be conservative around visible thresholds.

### Node unload policy
A node should not be unloaded if it is:
- the occupied node,
- directly visible through a currently valid portal sightline,
- part of an active transition window,
- or debug-forced active.

## Rules for door and portal objects
Doors, arches, and threshold props should remain thin scene objects that bind to graph portal identity rather than deciding visibility truth on their own, which keeps them aligned with the wider architecture [file:12][file:13][file:14].

### Door/portal objects should own
- local trigger/handoff hooks,
- animation state,
- optional sound/VFX hooks,
- reference to portal id.

### Door/portal objects should not own
- topology truth,
- remote node choice,
- mutation legality,
- local active-set policy.

## Debug requirements
This system is not done unless the hidden render-selection state is visible in debug, because invisible streaming logic becomes impossible to tune cleanly [file:12][file:13][file:15].

### Required debug views
- current active node,
- full active set snapshot,
- node streaming states,
- current portal destination mapping,
- visible portal rays or threshold links,
- nodes blocked from unloading and why,
- last visibility rebuild reason,
- current transition state.

## Testing strategy
Use three layers of testing.

### 1. Logic tests
Validate active-set construction, portal resolution, and unload-guard rules.

### 2. Graybox threshold tests
Walk doorways and corners to ensure continuity and absence of obvious contradictions.

### 3. Mutation interaction tests
Trigger legal mutations near and beyond line of sight to verify that the player never sees uncontrolled contradictory topology during or after change.

## Risks and mitigations

### Risk 1
The system renders too much and exposes contradictions.

### Mitigation
Start with a compact local active set and explicit portal authorization.

### Risk 2
The system renders too little and creates obvious pop-in.

### Mitigation
Use warm states and threshold transition windows.

### Risk 3
Portal visibility logic drifts from graph truth.

### Mitigation
Require portal render resolution to consume authoritative graph binding only.

### Risk 4
Mutation changes become visible mid-look in a way that feels buggy.

### Mitigation
Defer or transition visible portal updates when observation/visibility policy requires stability.

## Acceptance checklist
This spec is successfully implemented when:
- the player’s local view renders a graph-consistent active subset rather than the entire impossible house [file:12][file:13],
- portal views resolve from authoritative graph bindings,
- threshold crossing preserves local continuity without obvious popping,
- nodes do not unload while still visibly required,
- mutation-aware rendering avoids hard contradictions during visible topology changes,
- and debug tools make local render truth inspectable enough to tune during graybox development [file:12][file:13][file:14][file:15].

