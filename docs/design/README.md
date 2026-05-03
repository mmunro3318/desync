# README -- Claude Working Agreement / Task Template

## Purpose
This document defines the standard working agreement for Claude Code tasks in this repo. Its goal is to keep Claude operating from a small, stable context set, a predictable task shape, and explicit acceptance criteria.

The core idea is simple: Claude performs best when the project provides stable naming, clear ownership, bounded scope, and concrete definitions of done. That principle was already central to the earlier starter implementation docs and task-board patterns, which emphasized fixed ownership boundaries, consistent structure, and explicit acceptance tests rather than vague implementation requests [file:13][file:6][file:2].

This doc is not a replacement for sprint docs, system specs, or prompt packs. It is the standing agreement that tells Claude how to consume them.

## Why this doc exists
Unity work becomes unreliable when AI is asked to operate against:
- too many files at once,
- unclear architectural ownership,
- hidden assumptions in scenes or prefabs,
- or tasks that mix implementation, design, debugging, and refactoring into one prompt.

The earlier project docs repeatedly converged on a better pattern: use small bounded tasks, stable folder and namespace conventions, explicit ownership, and acceptance checks that can actually be verified in-editor or in play mode [file:13][file:14][file:7].

## Standing working agreement
Claude should follow these rules on every implementation task unless the task explicitly says otherwise.

### 1. Prefer small bounded tasks
One task should usually produce one coherent unit of progress:
- one class,
- one prefab wiring pass,
- one interface contract,
- one debug panel,
- one narrow integration step,
- or one tightly related implementation slice.

Avoid asking Claude to implement an entire system family in one prompt unless a dedicated prompt pack already sequences the work.

### 2. Load the smallest stable context set
Claude should not read the entire docs library by default. It should load:
- the permanent core docs,
- one active sprint/PDD,
- the directly relevant system spec(s),
- and one checklist or prompt-pack only if needed.

This preserves architectural coherence without blowing up task context.

### 3. Respect ownership boundaries
Claude should preserve the project’s architectural rules:
- runtime state stays separate from definitions,
- scene objects stay thin,
- systems own their own logic,
- hidden-state systems expose debug data,
- and new behavior should prefer extending assets/contracts over adding hardcoded branching [file:12][file:13][file:14].

### 4. Treat scenes, prefabs, and inspector wiring as first-class work
Unity tasks are not code-only tasks. Claude should always consider:
- scripts changed,
- prefabs affected,
- scene objects affected,
- inspector fields to wire,
- and test steps needed in the editor.

This mirrors the task-board formats from earlier docs, which treated files, scene/prefab impact, dependencies, and acceptance tests as part of every task card [file:6][file:2].

### 5. Acceptance tests are mandatory
A task is not done because code compiles. A task is done when the requested behavior can be verified in-editor or in play mode through explicit acceptance tests [file:7][file:1].

### 6. Non-goals matter
Every task should say what Claude is **not** allowed to do. This is one of the easiest ways to prevent scope creep, overengineering, and accidental architectural drift.

## Core docs Claude should almost always load
This is the small stable set of documents that should usually be available in a Claude session.

### Permanent core set
Load these unless the task is extremely narrow:
- `repo-docs-index-claude-file-map.md`
- `spatial-horror-gdd.md`
- `spatial-runtime-framework.md`
- `networked-house-runtime-interfaces-contracts.md`

### Usually load one of these
Pick the one most relevant to the current task:
- the active sprint/PDD, for example `sprint-1a-house-graph-authoring.md`
- the active implementation prompt pack, for example `claude-code-implementation-prompt-pack-vertical-slice.md`
- the active integration or smoke-test checklist, for example `networked-house-vertical-slice-integration-checklist.md`

### Optionally load one design anchor
Use one of these when feel/readability matters for the task:
- `player-experience-pillars.md`
- `navigation-and-orientation-ux.md`
- `room-identity-environmental-legibility-spec.md`
- `lighting-and-visibility-spec.md`

### Load task-specific system specs only as needed
Examples:
- `co-op-observation-and-sync-rules-spec.md`
- `portal-visibility-local-render-streaming-spec.md`
- `environmental-prop-taxonomy-asset-kit-spec.md`
- `observation-lock-spatial-mutation-rules-spec.md`

## Minimal doc-loading heuristic
Use this heuristic when deciding what to load.

| Task type | Must load | Usually add |
|---|---|---|
| New runtime system code | core set | active sprint/PDD, relevant system spec |
| Scene/prefab wiring | core set | active sprint/PDD, integration checklist |
| Readability/UX tuning | core set | one design anchor, relevant system spec |
| Debug tooling | core set | relevant system spec, debug checklist/spec |
| Refactor or bug fix | core set | the narrowest owning sprint/spec |
| Vertical slice task execution | core set | prompt pack, integration checklist |

If Claude is unsure whether a doc is necessary, prefer **not** loading it unless it changes implementation truth.

## Standard task template
Use this format for all normal Claude tasks.

```md
# Task: <short task name>

## Objective
What must exist or work after this task is done?

## Files
List the code files, prefab files, scene files, and data assets Claude is allowed or expected to touch.

## Contracts
List the interfaces, ownership rules, invariants, data flows, or system boundaries Claude must preserve.

## Non-goals
List what this task must not do. This is mandatory.

## Acceptance Tests
List explicit editor/play-mode checks that determine whether the task is complete.

## Notes
Optional implementation hints, references, or sequencing reminders.
```

## Expanded task template
For Unity work, the more practical version is usually this:

```md
# Task: <short task name>

## Objective
A 1–3 sentence description of the intended outcome.

## Docs to Load
- core docs
- active sprint/PDD
- relevant system spec(s)

## Files
### Scripts
- path/to/file.cs

### Prefabs
- path/to/prefab.prefab

### Scenes
- path/to/scene.unity

### Data Assets
- path/to/asset.asset

## Contracts
- What owns this behavior?
- What must remain separate?
- What interfaces/events must be respected?
- What data should not move into scene-only logic?

## Non-goals
- What this task must not implement
- What must not be refactored
- What adjacent systems are out of scope

## Acceptance Tests
- Play-mode behavior check 1
- Play-mode behavior check 2
- Debug visibility check
- No-console-error check

## Deliverable
- What Claude should report back: files changed, wiring required, known follow-ups
```

## Required task sections explained

### Objective
This should describe the behavior outcome, not the coding activity.

Good:
- “Crossing a threshold updates the player’s current node and exposes the new node id in debug.”

Weak:
- “Write node traversal code.”

### Files
Always specify allowed touch surface. This prevents accidental repo drift.

Include:
- scripts,
- prefabs,
- scenes,
- ScriptableObject assets,
- and editor tooling if relevant.

### Contracts
This is the architectural spine of the task.

Examples:
- `HouseGraphDefinition` remains immutable authoring data.
- Runtime node occupancy is not stored back into authoring assets.
- Portal traversal must publish through the runtime state service rather than mutating scene references directly.
- Debug overlay reads state; it does not own gameplay truth.

This section matters because earlier architecture docs made it clear that ownership drift is one of the fastest ways to lose modularity in Unity projects [file:12][file:13][file:14].

### Non-goals
This is required.

Examples:
- Do not implement multiplayer synchronization in this task.
- Do not refactor unrelated room-authoring code.
- Do not replace the existing debug overlay framework.
- Do not add new anomaly types.

### Acceptance Tests
Every task should include checks that are actually runnable.

Good acceptance tests usually include:
- a direct gameplay behavior,
- a debug/inspection confirmation,
- and a no-errors or no-regression check [file:7][file:1].

## Recommended Claude response format after a task
After completing a task, Claude should respond with:

```md
## Task Result

### Files Changed
- ...

### What Was Implemented
- ...

### Inspector / Scene Wiring Needed
- ...

### Acceptance Test Status
- Passed:
- Not yet verified:

### Known Risks / Follow-ups
- ...
```

This keeps implementation output consistent and reviewable.

## Standard non-goal patterns
These are useful defaults to copy into many tasks.

### Architecture guardrails
- Do not collapse runtime state into ScriptableObject definitions.
- Do not add giant manager behavior to scene bootstrap classes.
- Do not hardcode ghost/anomaly-specific branching where a system contract already exists.
- Do not move system truth into ad hoc inspector-only state.

### Scope guardrails
- Do not implement adjacent sprint items.
- Do not perform broad cleanup unrelated to the task.
- Do not rename stable files/contracts without explicit instruction.
- Do not replace working systems just because a new abstraction seems cleaner.

### Unity guardrails
- Do not silently require manual scene hierarchy assumptions without documenting them.
- Do not add serialized dependencies that are not named in the task output.
- Do not create hidden prefab coupling without reporting it.

## Task sizing guide
Use these rough rules.

### Good Claude-sized task
- 1–3 scripts,
- maybe 1 prefab and 1 scene wiring pass,
- one clear behavior outcome,
- acceptance tests runnable in under 5 minutes.

### Too large
- a whole new subsystem family,
- code + art + networking + UI + debug all at once,
- open-ended refactor plus new feature,
- or a task that needs half the docs library to explain.

If a task feels too large, split it.

## Example task

```md
# Task: Player current node tracker

## Objective
Implement a runtime service that tracks the player’s current house-graph node and updates it when the player crosses a registered threshold. Expose the current node id in the debug overlay.

## Docs to Load
- `repo-docs-index-claude-file-map.md`
- `spatial-runtime-framework.md`
- `networked-house-runtime-interfaces-contracts.md`
- `sprint-1a-house-graph-authoring.md`

## Files
### Scripts
- `Assets/Project/Scripts/World/Rooms/PlayerNodeTracker.cs`
- `Assets/Project/Scripts/World/Graph/HouseRuntimeState.cs`
- `Assets/Project/Scripts/UI/Debug/DebugOverlay.cs`

### Scenes
- `Assets/Project/Scenes/HouseGraybox.unity`

## Contracts
- Runtime current-node state belongs to runtime state, not graph definition assets.
- Threshold components detect crossings but do not own graph truth.
- Debug overlay reads and displays state but does not compute it.

## Non-goals
- Do not implement multiplayer sync.
- Do not implement anomaly mutation logic.
- Do not refactor unrelated threshold authoring.

## Acceptance Tests
- Entering play assigns a valid starting node.
- Crossing into another connected node updates the current node id.
- Debug overlay shows the current node id updating live.
- No console errors occur during repeated crossings.

## Deliverable
- Report files changed.
- Report any inspector wiring required.
- Report any assumptions about threshold placement.
```

## Relationship to other docs
This doc complements, but does not replace:
- sprint PDDs, which define feature slices,
- system specs, which define truth and ownership,
- prompt packs, which sequence work,
- and checklists, which validate integration.

Its job is to standardize the **shape of the work request** so Claude receives stable, comparable task inputs every time.

## Acceptance criteria for this working agreement
This agreement is successful when:
- Claude tasks consistently use Objective → Files → Contracts → Non-goals → Acceptance Tests,
- core docs loaded per task stay small and stable,
- task prompts preserve architectural ownership boundaries,
- Unity scene/prefab impact is named explicitly,
- and implementation results become easier to review, compare, and recover when things go wrong [file:13][file:6][file:2][file:7].
