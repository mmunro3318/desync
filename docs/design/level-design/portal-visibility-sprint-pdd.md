# Portal Visibility Sprint / PDD

## Sprint title
Portal Visibility Sprint — Local Render and Threshold Continuity

## Document objective
Turn the Portal Visibility / Local Render Streaming Spec into concrete implementation work. This sprint defines the classes, interfaces, activation states, milestone tasks, debug tools, and acceptance criteria needed to make impossible-space presentation believable from the first-person camera while remaining grounded in authoritative graph truth [file:12][file:13][file:14][file:15].

## Why this sprint exists
The project now has structural truth, legal mutation rules, and an observation-based system for when space may change. The next step is to make those systems camera-safe. This sprint exists to ensure that threshold views, local streaming, and node activation all support the illusion of a coherent nearby world even when the global topology is impossible [file:12][file:13].

## Sprint thesis
This sprint should prove that the game can render a locally consistent active subset of the house, resolve portal views from authoritative graph bindings, preserve believable threshold continuity while the player moves, and avoid obvious visual contradictions during local movement and legal topology changes [file:12][file:13][file:15].

## Questions this sprint answers
- Can the game decide which nodes are locally active, warm, portal-visible, or cold?
- Can portals reveal graph-authorized remote spaces without exposing contradictory topology?
- Can threshold crossing feel continuous even when the graph underneath is strange?
- Can developers inspect why a node is active or unloaded [file:12][file:13][file:15]?

## Out of scope
This sprint does **not** include:
- final portal VFX,
- advanced GPU optimization,
- final multiplayer render authority,
- all dimension-layer rendering rules,
- or a large library of bespoke camera tricks.

This sprint is about the core local render harness, not full production polish.

## Sprint goals
By the end of this sprint, the project should support:
- building a local active-set snapshot from current node and camera context,
- applying node streaming states to scene presentation roots,
- resolving portal destinations from authoritative graph bindings,
- handling threshold crossing with controlled source/destination overlap,
- protecting visible nodes from premature unload,
- and exposing the whole process through graybox debug tooling [file:12][file:13][file:14].

## Architectural rules carried into implementation
This sprint must preserve the same foundational rules used elsewhere in the project.

- Runtime state stays separate from data definitions [file:12][file:13][file:14].
- Scene objects stay thin and bind to graph identity instead of inventing topology [file:12][file:13][file:14].
- Hidden render-selection logic must be inspectable in debug [file:12][file:13][file:15].
- New presentation behaviors should mostly mean new policy data or focused classes, not giant scene-specific manager hacks [file:12][file:13].

## Relationship to prior docs
This sprint depends conceptually on:
- authoritative house graph runtime,
- observation lock/mutation legality rules,
- graph mutation execution,
- and the portal visibility/render streaming rules spec [file:12][file:13][file:14][file:15].

### Outputs from this sprint
- local visibility service,
- portal visibility resolver,
- node streaming controller,
- threshold transition policy,
- local render debug overlay,
- and one graybox workflow proving portal continuity.

## Recommended folder targets
This sprint should land cleanly inside the existing structure and namespace conventions [file:13][file:14].

### Suggested folders
- `Assets/_Project/Data/Rooms/Rendering/`
- `Assets/_Project/Scripts/World/Graph/Visibility/`
- `Assets/_Project/Scripts/World/Graph/Rendering/`
- `Assets/_Project/Scripts/World/Doors/`
- `Assets/_Project/Scripts/UI/Debug/`
- `Assets/_Project/Prefabs/Debug/`
- `Assets/_Project/Scenes/Test/`

## Suggested namespaces
- `GhostHunt.World.Graph.Visibility`
- `GhostHunt.World.Graph.Rendering`
- `GhostHunt.World.Doors`
- `GhostHunt.UI.Debug`
- `GhostHunt.Core.Events`

## Milestone framing
This sprint breaks into two concrete milestones.

### PV-1
Local visibility and node streaming foundation.

### PV-2
Portal resolution and threshold continuity.

## Core runtime model
This sprint should formalize a definition/runtime split consistent with the rest of the architecture [file:12][file:13][file:14].

### Definition-side examples
- `StreamingPolicyDefinition`
- `PortalVisibilityRuleDefinition`
- `NodeActivationProfile`
- `ThresholdTransitionProfile`
- `LocalRenderPolicyDefinition`

### Runtime-side examples
- `LocalRenderState`
- `ActiveSetSnapshot`
- `NodeStreamingStateRecord`
- `PortalVisibilityState`
- `ThresholdTransitionState`
- `VisibilityRebuildRecord`

## Suggested activation states
Use a small explicit state vocabulary.

### Required states
- `Cold`
- `Warm`
- `Active`
- `PortalVisible`
- `Transitioning`

### Rule
Graph nodes should never infer their own streaming state from scene distance alone. State should be assigned by the visibility/streaming services.

## Suggested core classes

| Class | Responsibility |
|---|---|
| `LocalVisibilityService` | builds the active local node set from graph and camera context |
| `PortalVisibilityResolver` | resolves what each visible portal should show |
| `NodeStreamingController` | applies activation state changes to node presentation targets |
| `NodePresentationBinder` | binds graph node ids to scene/prefab presentation roots |
| `ThresholdTransitionController` | manages source/destination overlap during portal crossing |
| `PortalRenderController` | coordinates portal-facing visual state |
| `LocalRenderState` | stores current active set and presentation flags |
| `LocalRenderDebugOverlay` | visualizes node states, portal states, and unload guards |

This preserves narrow ownership and avoids giant mixed-purpose systems, which follows your existing design rules [file:12][file:13][file:15].

## Suggested interfaces
The exact signatures can evolve, but the sprint should lock down narrow contracts rather than direct inter-system reach-throughs [file:13][file:14].

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

## Suggested first implementation slice
The sprint should stay narrow and graybox-first [file:12][file:13][file:15].

### Prototype policy
- one local player camera,
- current node + neighbor depth 1,
- portal-visible remote nodes only through bound doorway portals,
- one threshold transition controller for standard doorway crossing,
- debug overlay for node state and portal mapping.

## PV-1 overview
PV-1 proves that the game can determine and apply local node visibility correctly.

### PV-1 answers
Can the game compute a locally consistent active node set and apply it to presentation targets without exposing hidden global topology?

## PV-1 tasks

### PV1-T1 — Create render/runtime state models
Implement:
- `LocalRenderState`
- `ActiveSetSnapshot`
- `NodeStreamingStateRecord`
- `VisibilityRebuildRecord`

#### Acceptance criteria
- Runtime types compile and remain separate from definition assets.
- Types capture current node, active set, warm set, unload-guard reasons, and rebuild metadata.

### PV1-T2 — Create render policy assets and enums
Implement:
- `StreamingPolicyDefinition`
- `LocalRenderPolicyDefinition`
- `NodeActivationProfile`
- `NodeStreamingState` enum

#### Acceptance criteria
- Neighbor depth, warm-state policy, and unload policy can be tuned without rewriting core logic [file:12][file:13][file:14].

### PV1-T3 — Implement `NodePresentationBinder`
Create the binding layer that maps graph node ids to scene presentation roots.

#### Responsibilities
- bind node id to presentation root,
- validate missing or duplicate bindings,
- expose binding mismatch info to debug.

#### Acceptance criteria
- Scene/prefab roots do not invent node identity independently.
- Missing bindings are visible and fail loudly when necessary [file:12][file:13][file:14].

### PV1-T4 — Implement `LocalVisibilityService`
Build the service that computes local active sets.

#### Minimum rules
- include occupied node,
- include graph neighbors within policy depth,
- include nodes needed by active transitions,
- reserve portal-visible handling for resolver integration.

#### Acceptance criteria
- Active-set construction is deterministic for a given graph state and camera/node context.
- Service does not rely on ad hoc scene proximity hacks.

### PV1-T5 — Implement `NodeStreamingController`
Apply streaming states to presentation targets.

#### Responsibilities
- assign cold/warm/active/transitioning state,
- prevent unsafe unload of guarded nodes,
- track current assigned state by node.

#### Acceptance criteria
- Presentation roots can be moved through streaming states predictably.
- Nodes required by current view or transition do not unload prematurely.

### PV1-T6 — Build local render debug overlay
Show:
- current active node,
- active/warm/cold nodes,
- unload guards,
- last rebuild reason,
- and binding failures.

#### Acceptance criteria
- Developers can understand why a node is active or inactive during graybox play.
- Debug categories can be toggled independently.

### PV1-T7 — Add logic tests for active-set construction
Create tests for:
- occupied node inclusion,
- neighbor depth behavior,
- unload guard behavior,
- binding mismatch detection.

#### Acceptance criteria
- Tests run without full scene gameplay boot.
- Failures point to visibility/streaming logic directly.

## PV-1 completion definition
PV-1 is complete when the project can compute and apply a deterministic local active set, bind nodes to presentation roots, and explain node activation state through debug views [file:12][file:13][file:15].

## PV-2 overview
PV-2 proves that portal views and threshold crossing stay believable.

### PV-2 answers
Can portals reveal graph-authorized destinations and can doorway crossing remain visually consistent during movement and mutation?

## PV-2 tasks

### PV2-T1 — Create portal runtime state models
Implement:
- `PortalVisibilityState`
- `ThresholdTransitionState`
- `PortalRenderMode` enum

#### Acceptance criteria
- Portal runtime state can capture destination node, render mode, transition flags, and visibility reason.

### PV2-T2 — Create portal policy assets
Implement:
- `PortalVisibilityRuleDefinition`
- `ThresholdTransitionProfile`

#### Acceptance criteria
- Portal reveal distance, sightline policy, and transition timing are tunable in data [file:12][file:13][file:14].

### PV2-T3 — Implement `PortalVisibilityResolver`
Build the resolver for what a portal currently reveals.

#### Minimum inputs
- portal id,
- current graph binding,
- camera transform,
- current node context,
- active transition state.

#### Minimum outputs
- destination node id,
- portal-visible bool,
- render mode,
- unload guard requirements.

#### Acceptance criteria
- Portal resolution follows authoritative graph binding only.
- Resolver never returns contradictory destinations for the same portal state.

### PV2-T4 — Implement `PortalRenderController`
Apply resolved portal visibility to presentation logic.

#### Responsibilities
- request destination node warm/portal-visible state,
- coordinate portal-facing render mode,
- guard against destination unload while visible.

#### Acceptance criteria
- Looking through a bound portal reveals its resolved destination consistently.
- Portal-visible nodes remain available while needed.

### PV2-T5 — Implement `ThresholdTransitionController`
Build threshold crossing continuity support.

#### Responsibilities
- activate destination before full crossing commit,
- keep source alive during overlap window,
- finalize source teardown after commitment,
- expose current transition debug data.

#### Acceptance criteria
- Standard doorway crossing avoids obvious pop-in or one-frame contradiction.
- Source and destination do not compete visibly in a broken way.

### PV2-T6 — Add mutation-aware portal protection hooks
Integrate with mutation/render rules so currently visible portal truths do not snap to contradictory states mid-look.

#### Acceptance criteria
- A visible portal destination can be protected, deferred, or transitioned according to current policy.
- Hard contradictions during observed portal views are prevented.

### PV2-T7 — Build portal sightline debug tools
Show:
- portal id,
- resolved destination,
- current portal render mode,
- sightline validity,
- transition state,
- and unload guards.

#### Acceptance criteria
- Developers can inspect what each visible threshold believes it is showing.

### PV2-T8 — Add graybox threshold smoke workflow
Create a test workflow for moving through doorways and observing portal states.

#### Could include
- forced node selection,
- portal destination overlay labels,
- manual portal visibility refresh,
- threshold transition step-through mode.

#### Acceptance criteria
- A developer can reproduce and inspect doorway/portal behavior reliably.

### PV2-T9 — Add contradiction regression tests
Create targeted tests for common failure cases.

#### Suggested tests
- portal destination mismatch rejected,
- visible node blocked from unload,
- threshold crossing keeps destination active before commit,
- mutation-visible portal change deferred or guarded.

#### Acceptance criteria
- Known contradiction patterns are covered by regression tests.

## PV-2 completion definition
PV-2 is complete when portals resolve authoritative destinations consistently, threshold crossing preserves local continuity, visible nodes are protected from unsafe unload, and graybox debug tools make portal behavior inspectable in play [file:12][file:13][file:15].

## Suggested ownership map
To keep Claude effective, each subsystem should stay narrow.

| Area | Owns | Does not own |
|---|---|---|
| Local visibility service | active-set construction | portal visual styling |
| Node streaming controller | node state application | graph truth |
| Node presentation binder | node-to-scene binding | streaming policy |
| Portal visibility resolver | portal destination logic | graph mutation decisions |
| Portal render controller | portal-facing presentation state | topology authority |
| Threshold transition controller | crossing continuity | portal legality |
| Debug overlay | hidden render-state visibility | render truth decisions |

## Suggested Claude Code workflow
This sprint should be implemented through tightly scoped prompts with exact classes and acceptance checks, following the broader project lesson that AI work is strongest when ownership and done-state are explicit [file:12][file:13][file:15].

### Good prompt shape
- 1–3 classes at a time,
- explicit folder targets,
- required methods and state fields,
- acceptance tests,
- and required debug outputs.

### Bad prompt shape
- “Make portal rendering work.”
- “Implement impossible-space visuals.”
- “Fix the whole streaming system.”

## Suggested Claude task order
A practical implementation order is:
1. runtime state models,
2. policy assets/enums,
3. node presentation binder,
4. local visibility service,
5. node streaming controller,
6. local render debug overlay,
7. active-set logic tests,
8. portal runtime state models,
9. portal policy assets,
10. portal visibility resolver,
11. portal render controller,
12. threshold transition controller,
13. mutation-aware portal protection hooks,
14. portal sightline debug tools,
15. graybox threshold workflow,
16. contradiction regression tests.

## Testing strategy
Use three layers of testing.

### 1. Logic tests
Verify active-set construction, portal resolution, unload guards, and threshold transition rules.

### 2. Graybox movement tests
Walk doors, corners, and short corridors to verify local continuity.

### 3. Mutation interaction tests
Trigger legal nearby mutations and verify that currently visible thresholds do not expose uncontrolled contradictions.

## Risks and mitigations

### Risk 1
The local active set is too broad and exposes contradictory geometry.

### Mitigation
Start with compact active sets and explicit portal authorization.

### Risk 2
The local active set is too narrow and causes obvious pop-in.

### Mitigation
Use warm states, unload guards, and threshold transition overlap.

### Risk 3
Portal resolution drifts from graph truth.

### Mitigation
Require resolver inputs from authoritative graph binding only.

### Risk 4
Scene doors begin owning topology or visibility truth.

### Mitigation
Keep doors and thresholds as thin binders/controllers tied to portal ids, consistent with the broader architecture [file:12][file:13][file:14].

## Acceptance checklist
This sprint is successful when:
- local active sets are computed deterministically from node context and policy rather than ad hoc scene distance [file:12][file:13],
- node presentation roots bind cleanly to graph node ids,
- node streaming states are applied predictably and protect visible nodes from unsafe unload,
- portals resolve authoritative destinations consistently,
- threshold crossing preserves local continuity without obvious contradiction,
- mutation-aware portal protection prevents hard visual breaks during visible topology changes,
- and the whole presentation state is inspectable in debug during graybox play [file:12][file:13][file:14][file:15].
