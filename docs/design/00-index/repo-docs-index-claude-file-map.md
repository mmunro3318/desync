# Repo Docs Index / Claude File Map

## Purpose
This document is the repo-native index for the design, architecture, sprint, spec, checklist, and prompt-pack docs that support the impossible-house prototype in Unity. It exists so Claude Code can navigate the project documentation using stable filenames and purposes instead of temporary chat-local references such as `[file:12]` or `[file:14]`.

## How Claude should use this file
Read this file first when entering the repo or when asked to work on a new feature. Use it to identify:
- which documents define overall intent,
- which documents define system rules,
- which documents define sprint scope,
- which documents define implementation contracts,
- and which documents define validation/test expectations.

When a task is narrow, Claude should read only the smallest relevant subset of docs rather than the entire library.

## Document type legend
- **Vision**: high-level game direction and design goals.
- **Epic**: large thematic/system roadmap.
- **Sprint/PDD**: bounded implementation slice.
- **Spec**: rules, contracts, or behavior definitions.
- **Checklist**: integration or acceptance checklist.
- **Prompt Pack**: bounded Claude task prompts.
- **Plan**: cross-system execution plan.

## Recommended reading order for a new Claude session
1. `spatial-horror-gdd.md`
2. `spatial-runtime-framework.md`
3. `networked-house-runtime-interfaces-contracts.md`
4. `repo-docs-index-claude-file-map.md`
5. The sprint/spec/checklist docs directly related to the current task

## Core design and vision docs

| Filename | Type | Purpose | Read when | Depends on |
|---|---|---|---|---|
| `spatial-horror-gdd.md` | Vision | Main game concept, pillars, MVP shape, milestone framing | starting any feature work | none |
| `player-experience-pillars.md` | Vision | Defines emotional/player-experience goals | tuning feel, pacing, horror readability | `spatial-horror-gdd.md` |
| `match-loop-and-session-flow.md` | Spec | Defines round/session structure and likely objective loop | implementing session flow or progression slice | `spatial-horror-gdd.md` |
| `navigation-and-orientation-ux.md` | Spec | Navigation, readability, and orientation rules in impossible space | building traversal, readability, signposting | `spatial-horror-gdd.md` |
| `room-identity-environmental-legibility-spec.md` | Spec | Rules for keeping rooms distinct/legible under anomaly pressure | implementing room variants or environmental readability | `navigation-and-orientation-ux.md` |
| `ux-principles-and-interaction-spec.md` | Spec | Core interaction and usability principles | implementing player interaction or UX glue | `spatial-horror-gdd.md` |
| `spatial-horror-reference-board.md` | Vision | Creative anchor -- digest Mike's (dev) vision and inspiration for the game project | planning out new features, unsure about dev direction or Mike's intent for game/features, deciding on next steps | none |

## Runtime architecture docs

| Filename | Type | Purpose | Read when | Depends on |
|---|---|---|---|---|
| `spatial-runtime-framework.md` | Spec | High-level framework/harness for spatial runtime systems | implementing core runtime systems | `spatial-horror-gdd.md` |
| `networked-house-runtime-interfaces-contracts.md` | Spec | Explicit interfaces/contracts for networked house runtime | implementing service APIs or contracts | `spatial-runtime-framework.md` |
| `house-graph-core-epic.md` | Epic | Canonical graph-based house runtime roadmap | building graph systems | `spatial-runtime-framework.md` |
| `house-graph-integration-spec.md` | Spec | Integration rules for graph systems in runtime/scene | wiring graph into scene/runtime | `house-graph-core-epic.md` |
| `room-node-authoring-spec.md` | Spec | How room/node authoring should work | building authoring components/tools | `house-graph-core-epic.md` |
| `house-layout-dsl-spec.md` | Spec | DSL and structure for house layout definitions | implementing layout serialization/import | `house-graph-core-epic.md` |
| `importer-contracts-and-data-model.md` | Spec | Import contracts and data models for content pipeline | building importers or format bridges | `house-layout-dsl-spec.md` |

## Observation, mutation, and portal docs

| Filename | Type | Purpose | Read when | Depends on |
|---|---|---|---|---|
| `co-op-observation-and-sync-rules-spec.md` | Spec | Rules for multiplayer observation truth and sync behavior | implementing observation or sync logic | `spatial-runtime-framework.md`, `networked-house-runtime-interfaces-contracts.md` |
| `observation-lock-spatial-mutation-rules-spec.md` | Spec | Allow/reject rules linking observation protection to mutations | implementing mutation legality checks | `co-op-observation-and-sync-rules-spec.md` |
| `portal-visibility-local-render-streaming-spec.md` | Spec | Portal visibility, local render truth, and streaming rules | implementing portal render/stream/visibility | `house-graph-core-epic.md`, `co-op-observation-and-sync-rules-spec.md` |
| `anomaly-families-epic.md` | Epic | Taxonomy and roadmap of anomaly types | planning or implementing anomaly families | `spatial-horror-gdd.md` |
| `anchor-artifact-loop-epic.md` | Epic | Anchor/artifact objective loop and related systems | implementing objective/win-con systems | `spatial-horror-gdd.md`, `match-loop-and-session-flow.md` |
| `environmental-prop-taxonomy-asset-kit-spec.md` | Spec | Prop families, authoring tags, prefab kit rules, and placement metadata for room population system | implementing room population, prop placement, or world-layer variants | `spatial-horror-gdd.md`, `room-identity-environmental-legibility-spec.md` |
| `lighting-and-visibility-spec.md` | Spec | Lighting as atmosphere, navigation, interaction readability, and state communication; visibility tiers and room light signatures | implementing lighting systems, URP setup, or readability tuning | `spatial-horror-gdd.md`, `navigation-and-orientation-ux.md`, `environmental-prop-taxonomy-asset-kit-spec.md` |


## Pipeline and builder docs

| Filename | Type | Purpose | Read when | Depends on |
|---|---|---|---|---|
| `house-builder-pipeline-epic.md` | Epic | Roadmap for the house-builder/content pipeline | implementing content generation pipeline | `house-layout-dsl-spec.md`, `importer-contracts-and-data-model.md` |
| `house-builder-pdd-sprint-hb1-hb2.md` | Sprint/PDD | Sprint breakdown for house-builder work | executing HB sprint tasks | `house-builder-pipeline-epic.md` |
| `room-population-rules-spec.md` | Spec | Rules for populating spaces with props/content | implementing room dressing/content placement | `house-builder-pipeline-epic.md` |

## Sprint docs

| Filename | Type | Purpose | Read when | Depends on |
|---|---|---|---|---|
| `house-graph-sprint-pdd.md` | Sprint/PDD | Sprint plan for core graph work | executing graph sprint | `house-graph-core-epic.md` |
| `sprint-1-spatial-core-pdd.md` | Sprint/PDD | Broad spatial-core sprint document | implementing the earliest spatial runtime slice | `spatial-runtime-framework.md` |
| `sprint-1a-house-graph-authoring.md` | Sprint/PDD | First slice: graph authoring/runtime basics | implementing initial graph authoring/runtime | `house-graph-core-epic.md`, `house-graph-sprint-pdd.md` |
| `sprint-1b-portal-visibility-node-activation.md` | Sprint/PDD | Second slice: portal visibility and node activation | implementing node activation and portal visibility | `sprint-1a-house-graph-authoring.md`, `portal-visibility-local-render-streaming-spec.md` |
| `sprint-2-observation-lock-system.md` | Sprint/PDD | Observation-lock system implementation sprint | implementing observation protection systems | `co-op-observation-and-sync-rules-spec.md` |
| `co-op-observation-sprint-pdd.md` | Sprint/PDD | More focused co-op observation sprint planning | implementing or refining co-op observation | `sprint-2-observation-lock-system.md` |
| `spatial-mutation-sprint-pdd.md` | Sprint/PDD | Mutation system sprint plan | implementing mutation systems | `observation-lock-spatial-mutation-rules-spec.md` |
| `portal-visibility-sprint-pdd.md` | Sprint/PDD | Portal system sprint plan | implementing portal system slice | `portal-visibility-local-render-streaming-spec.md` |
| `sprint-3-loop-mutation-vertical-slice.md` | Sprint/PDD | Loop anomaly vertical slice sprint | implementing looping corridor anomaly | `spatial-mutation-sprint-pdd.md`, `anomaly-families-epic.md` |
| `sprint-4a-substitution-anomaly-vertical-slice.md` | Sprint/PDD | Substitution anomaly vertical slice sprint | implementing room substitution anomaly | `spatial-mutation-sprint-pdd.md`, `anomaly-families-epic.md` |
| `sprint-4b-tardis-anomaly-vertical-slice.md` | Sprint/PDD | Tardis/interior-bigger anomaly vertical slice sprint | implementing nested/interior-bigger anomaly | `spatial-mutation-sprint-pdd.md`, `anomaly-families-epic.md` |
| `sprint-5a-anchor-core-state.md` | Sprint/PDD | Anchor/artifact core-state sprint | implementing objective/anchor core state | `anchor-artifact-loop-epic.md` |

## Debug, testing, and integration docs

| Filename | Type | Purpose | Read when | Depends on |
|---|---|---|---|---|
| `debug-and-visualization-spec.md` | Spec | Broader debug/visualization guidance | implementing general debug tools | `spatial-runtime-framework.md` |
| `networked-house-debug-overlay-spec.md` | Spec | Debug overlay panels, HUD, gizmos, and read models | implementing networked house debug UI | `debug-and-visualization-spec.md`, `networked-house-runtime-interfaces-contracts.md` |
| `networked-house-graybox-test-plan.md` | Plan | Scenario matrix and regression rules for graybox testing | validating the vertical slice | `networked-house-debug-overlay-spec.md`, `networked-house-runtime-interfaces-contracts.md` |
| `networked-house-vertical-slice-integration-checklist.md` | Checklist | Concrete wiring checklist for the first playable slice | integrating systems in Unity | `networked-house-graybox-test-plan.md`, `networked-house-runtime-interfaces-contracts.md` |
| `impossible-house-graybox-vertical-slice-plan.md` | Plan | Higher-level plan for the graybox slice | orienting a new implementation pass | `spatial-horror-gdd.md`, `spatial-runtime-framework.md` |
| `docs/handoff-prompts/current/GEOMETRY_GRAMMAR.md` | Spec | Canonical geometry construction grammar — inset, stagger, separator, and coplanar-avoidance rules for graybox rooms; source of truth for the procedural room builder and geometry validator | authoring new room geometry, implementing the procedural room builder (TD0004), running geometry validator TDD (TD0005–TD0008) | `docs/ARCH.md` (S0.3 entry), `docs/handoff-prompts/current/04-geometry-validator-tdd-handoff.md` |

## Claude execution docs

| Filename | Type | Purpose | Read when | Depends on |
|---|---|---|---|---|
| `claude-code-task-pack-networked-house-runtime.md` | Prompt Pack | Earlier bounded task pack for runtime work | breaking runtime work into tasks | `networked-house-runtime-interfaces-contracts.md` |
| `claude-code-implementation-prompt-pack-vertical-slice.md` | Prompt Pack | Sequenced bounded prompts for vertical slice implementation | actively driving Claude task-by-task | `networked-house-vertical-slice-integration-checklist.md`, `networked-house-graybox-test-plan.md` |
| `repo-docs-index-claude-file-map.md` | Index | Stable documentation map for Claude | first file to read in repo docs context | none |

## Unity Research Docs

| Filename | Type | Purpose | Read when | Depends |
| --- | --- | --- | --- | --- |
| `99-perplexity-deep-research-seed-prompt-unity-csharp-best-practices.md` | Checklist / Research brief | Defines the research goals, run structure, deliverables, and topic checklist for the Unity/C# deep research series | starting, extending, or auditing the research program | `99-perplexity-deep-research-seed-prompt-unity-csharp-best-practices.md` |
| `00-unity-technology-baseline-report.md` | Research report | Establishes the baseline Unity 6 stack, recommended defaults, and core technology choices for the prototype | deciding engine stack, rendering path, networking baseline, and overall technical posture | `99-perplexity-deep-research-seed-prompt-unity-csharp-best-practices.md` |
| `01-project-architecture-and-code-organization-report.md` | Research report | Documents project architecture, folder/code organization, ScriptableObject boundaries, bootstrap patterns, and service/manager discipline | setting up project structure, composition root, and coding conventions | `99-perplexity-deep-research-seed-prompt-unity-csharp-best-practices.md`, `00-unity-technology-baseline-report.md` |
| `02-scenes-prefabs-and-level-building-report.md` | Research report | Covers scene layout, prefab strategy, graybox workflow, and level-building discipline for a maintainable Unity project | defining gameplay scene structure, prefabs, additive loading, and environment-building workflow | `99-perplexity-deep-research-seed-prompt-unity-csharp-best-practices.md`, `01-project-architecture-and-code-organization-report.md` |
| `03-unity-urp-graphics-lighting-horror-report.md` | Research report | Covers URP configuration, lighting, shadows, post-processing, reflection probes, and horror-specific visual guidance | tuning rendering, fixing light leaks, improving atmosphere, or setting URP defaults | `99-perplexity-deep-research-seed-prompt-unity-csharp-best-practices.md`, `00-unity-technology-baseline-report.md` |
| `04-ngo-multiplayer-architecture-report.md` | Research report | Defines NGO architecture, authority model, ownership rules, state sync patterns, and multiplayer implementation guardrails | implementing multiplayer gameplay, interactables, spawning, authority, or scene synchronization | `99-perplexity-deep-research-seed-prompt-unity-csharp-best-practices.md`, `00-unity-technology-baseline-report.md`, `01-project-architecture-and-code-organization-report.md` |
| `05-testing-profiling-debug-overlay-report.md` | Research report | Covers local multiplayer testing workflow, profiling tools, runtime debug overlays, observability, and pooling guidance | testing co-op sessions, diagnosing performance issues, or adding runtime developer tooling | `99-perplexity-deep-research-seed-prompt-unity-csharp-best-practices.md`, `04-ngo-multiplayer-architecture-report.md` |
| `06-ai-guardrails-and-unity-antipatterns-report.md` | Research report | Defines Claude/AI coding guardrails, Unity-specific anti-patterns, and repo-level rules for safe implementation assistance | writing CLAUDE.md, reviewing AI-generated code, or enforcing project coding constraints | `99-perplexity-deep-research-seed-prompt-unity-csharp-best-practices.md`, `01-project-architecture-and-code-organization-report.md`, `04-ngo-multiplayer-architecture-report.md`, `05-testing-profiling-debug-overlay-report.md`|

## Legacy source docs from the earlier clone phase
These are older foundation docs that many newer docs implicitly build on. They should be preserved for architecture philosophy, but current implementation work should usually prefer the newer impossible-house docs.

| Legacy reference | Current filename | Why it matters now |
|---|---|---|
| `[file:12]` | `Clone MVP Vision.md` | Source of core architecture philosophy: modularity, runtime-vs-definition separation, debug-first, graybox-first |
| `[file:13]` | `3 - Starter Design Doc.md` | Source of starter implementation structure, ownership rules, scene discipline |
| `[file:14]` | `2 - Project Structure.md` | Source of folder/scene/class organization guidance |
| `[file:15]` | `1 - Phasmo Clone MVP.md` | Source of milestone-slice thinking and build-order philosophy |

## Task-to-doc routing guide
Use this quick routing table to decide what Claude should read for a given task.

| Task type | Start here |
|---|---|
| New feature with no context | `repo-docs-index-claude-file-map.md`, `spatial-horror-gdd.md`, then relevant sprint/spec |
| Graph runtime work | `house-graph-core-epic.md`, `house-graph-integration-spec.md`, `networked-house-runtime-interfaces-contracts.md` |
| Observation/protection work | `co-op-observation-and-sync-rules-spec.md`, `sprint-2-observation-lock-system.md`, `networked-house-runtime-interfaces-contracts.md` |
| Mutation legality work | `observation-lock-spatial-mutation-rules-spec.md`, `spatial-mutation-sprint-pdd.md` |
| Portal/visibility work | `portal-visibility-local-render-streaming-spec.md`, `portal-visibility-sprint-pdd.md`, `sprint-1b-portal-visibility-node-activation.md` |
| Debug UI work | `networked-house-debug-overlay-spec.md`, `debug-and-visualization-spec.md` |
| Graybox validation | `networked-house-graybox-test-plan.md`, `networked-house-vertical-slice-integration-checklist.md` |
| Room geometry authoring / validator TDD | `docs/handoff-prompts/current/GEOMETRY_GRAMMAR.md`, `docs/handoff-prompts/current/04-geometry-validator-tdd-handoff.md` |
| Claude task execution | `claude-code-implementation-prompt-pack-vertical-slice.md` |
| Objective/anchor loop work | `anchor-artifact-loop-epic.md`, `sprint-5a-anchor-core-state.md`, `match-loop-and-session-flow.md` |
| Builder/pipeline work | `house-builder-pipeline-epic.md`, `house-layout-dsl-spec.md`, `importer-contracts-and-data-model.md` |
| Unity code work | `98-unity-research/` |

## Canonical repo convention
These docs now live in the stable folder structure below:

```txt
docs/design
  00-index/
    repo-docs-index-claude-file-map.md
  01-vision/
  02-architecture/
  03-systems/
  04-sprints/
  05-debug-and-testing/
  06-claude-prompts/
  98-unity-research/
  99-legacy/
```

Folder intent:
- `00-index/`: repo-native indexes and navigation docs.
- `01-vision/`: core design direction, player-experience, and UX docs.
- `02-architecture/`: runtime framework, contracts, graph architecture, and authoring specs.
- `03-systems/`: system-specific specs and epics, including observation, mutation, portal, and builder docs.
- `04-sprints/`: sprint and PDD execution docs.
- `05-debug-and-testing/`: debug overlays, plans, checklists, and validation docs.
- `06-claude-prompts/`: Claude-oriented prompt packs and implementation task packs.
- `98-unity-research/`: Claude-oriented research docs for Unity development for our specific game project.
- `99-legacy/`: older clone-phase reference docs retained for architectural context.

## Usage note for Claude
When asked to implement a feature:
1. Read this index.
2. Read the smallest relevant vision/framework/spec doc set.
3. Read the current sprint/checklist/prompt-pack docs.
4. Implement one bounded task.
5. Report files changed, assumptions made, and manual verification steps.

That workflow is the safest way to preserve architectural coherence while still moving quickly.
