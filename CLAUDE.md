# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project identity

- **Working title:** `DESYNC`
- **Genre:** first-person co-op spatial / liminal horror
- **Premise:** a house occupied by a Lovecraftian entity expands its interior into an impossible labyrinth in real time; players hunt 1–3 anchoring artifacts to collapse the anomaly and escape, while a stalking Entity hunts them.
- **Jam constraint:** Pride Jam 2026, due **2026-06-12**, theme **"Asylum"**.
- **Engine:** Unity 6, URP 17.4.0, Netcode for GameObjects (NGO) 2.11.2, Input System 1.19.0.

## Current code state (as of 2026-05-09)

The repo has a working house graph runtime (S1A complete), portal visibility / node activation system (S1B complete), ProBuilder graybox geometry for all 5 rooms (S1C complete), and observation lock system (S2 complete) built on a migrated Unity foundation. Mutation and anchor systems are not yet implemented. All first-party code lives under `unity-DESYNC/Assets/_Project/`:

- **Graph system** (`Scripts/World/Graph/`, namespace `Desync.World.Graph.*`): `Definitions/HouseGraphDefinition.cs` (ScriptableObject + definition structs), `Runtime/SpatialGraphRuntime.cs` (O(1) query engine), `Runtime/PortalResolver.cs`, `Runtime/PlayerNodeTracker.cs`, `Runtime/{RuntimeNodeState,RuntimeEdgeState}.cs`, `Authoring/{RoomNodeAuthoring,PortalAnchorAuthoring}.cs`, `GraphRuntimeHost.cs`, `Debug/{SpatialDebugOverlay,SpatialDebugGizmos}.cs`. Deep module boundary via `Desync.World.Graph.asmdef`.
- **Node activation** (`Scripts/World/Graph/Runtime/`): `ViewContext.cs`, `NodeActivationReason.cs`, `NodeActivationResolver.cs`, `NodeStreamingController.cs`, `NodePresentationHandle.cs`, `PortalVisibilityContracts.cs`, `PortalVisibilityController.cs`, `PortalVisibilityEvaluator.cs`, `Debug/SpatialVisibilityDebugOverlay.cs`. Camera/tracker bound via `NodeStreamingController.BindLocalPlayer()` (no `Camera.main`). Resolver and evaluator return fresh collections per call (no aliased mutable state).
- **Observation lock** (`Scripts/World/Graph/Runtime/`): `ObservationLockSystem.cs` (lock state engine with grace timers and visibility polling), `NodeObservationState.cs`/`EdgeObservationState.cs` (per-target lock state), `LockReason.cs` (enum: Occupied, AdjacentOccupiedEdge, PortalVisible, DebugForced, ProtectedByRule, GracePeriod), `IObservationLockQuery.cs` (read-only query interface), `IObservationInputSource.cs` (input abstraction), `LocalObservationInputSource.cs` (portal visibility adapter), `Definitions/ObservationRulesDefinition.cs` (ScriptableObject for grace durations/refresh interval), `Debug/ObservationDebugOverlay.cs` (F6 IMGUI overlay), `Debug/ObservationDebugGizmos.cs` (scene gizmos for lock state). `GraphRuntimeHost` owns observation lifecycle (`InitializeObservation`, `BindObservationTracker`, `ResetObservation`). `NodeStreamingController` feeds portal results via `BuildPortalProbes()`. `PlayerMotor` wires `BindObservationTracker` on network spawn/despawn.
- **Core** (`Scripts/Core/`): `GameBootstrap.cs`, `GameplaySettings.cs` (ScriptableObject).
- **Player** (`Scripts/Player/`): `PlayerInputRouter.cs`, `PlayerLook.cs`, `PlayerMotor.cs`.
- **Other** (`Scripts/`): `Items/FlashlightController.cs`, `Audio/{AmbientAudioManager,FootstepAudio}.cs`, `UI/LobbyUI.cs`.
- **Scenes** (`Scenes/`): `Bootstrap.unity` (lobby + NetworkManager, loads `House_Prototype`), `House_Prototype.unity` (5-node graph scene with debug overlay + gizmos + activation system + observation lock + ProBuilder graybox geometry), `House_Graybox.unity` (legacy two-floor graybox, safe as lighting reference).
- **Tests** (`Tests/EditMode/`): 189 tests (all pass). Covers graph definitions, runtime queries, resolver, state, host, player tracker, network bootstrap, geometry grammar compliance, node activation contracts, resolver, evaluator, streaming controller, visibility overlay, observation lock system (32 tests), observation state (11 tests), observation rules (3 tests), local observation input source (7 tests), host observation binding (5 tests), and portal probe building (4 tests).
- **Data/Prefabs**: `Data/HouseGraphDefinition.asset` (5-node graph SO), `Data/ObservationRulesDefinition.asset` (observation tuning SO), `Prefabs/Rooms/Room_*.prefab` (x5 with RoomNodeAuthoring + trigger volumes + NodePresentationHandle + ProBuilder geometry under Presentation child), `PF_Player`, `Railing_Graybox`.
- **Network**: `DefaultNetworkPrefabs.asset` registry referencing `PF_Player`.

The carried-forward player/audio/UI code is a **fixture for the runtime, not the architectural template**. New systems must be built per the design docs, not by extending carried code.

## Repo layout

```
unity-DESYNC/                   # Unity project root (Assets/, Packages/, ProjectSettings/, Library/)
  Assets/_Project/              # all first-party content lives here
    {Scripts, Scenes, Prefabs, Data, Settings, Art, Audio, Tests}/
docs/
  design/
    00-index/                   # repo-docs-index-claude-file-map.md (canonical doc map)
    01-vision/                  # GDD, pillars, reference board, UX specs
    02-architecture/            # spatial runtime framework, contracts, graph specs
    03-systems/                 # observation, mutation, portal, anchor, builder epics+specs
    04-sprints/                 # sprint/PDD docs
    05-debug-and-testing/       # debug overlay, graybox test plan, integration checklist
    06-claude-prompts/          # bounded prompt packs
    98-unity-research/          # Unity 6 / URP / NGO / testing / AI-guardrails research reports
    99-legacy/                  # earlier clone-phase reference docs
  handoff-prompts/
    current/                    # active session briefs (incl. 01-migration/)
    archived/                   # session pipeline
  templates/                    # TODO/prompt templates
  ARCH.md
  TODO.md, TODO_INDEX.md, TODO_ARCHIVE.md
  UNITY_MCP_LESSONS.md, DEEP_MODULES_SPEC.md, SKILLS_REFERENCE.md
  workspace.md (Mike's scratchpad — ignore / do not read)
.claude/skills/                 # mattpocock skills (symlinks; see skills-lock.json)
CLAUDE.md, CLAUDE-draft.md, README.md, skills-lock.json
```

## Progress tracking

`ROADMAP.md` (repo root) is the scheduling and progress-tracking layer. It wraps the GDD milestones in release targets, shows sprint dependencies/parallelism, and has checkboxes for acceptance criteria + personal gates. Read the Status Dashboard table at the top for a 5-second progress snapshot. Sprint PDDs remain the source of truth for detailed specs.

## Doc routing

The canonical doc map is `docs/design/00-index/repo-docs-index-claude-file-map.md`. Read it first when scoping any feature. Do not duplicate its routing table here.

**Reading order for a new feature:** GDD (`docs/design/01-vision/spatial-horror-gdd.md`) → spatial runtime framework (`docs/design/02-architecture/spatial-runtime-framework.md`) → networked-house contracts (`docs/design/02-architecture/networked-house-runtime-interfaces-contracts.md`) → the relevant epic/sprint docs (loaded narrowly, not in bulk).

For Unity engine-side conventions, the source of truth is `docs/design/98-unity-research/`:
- `00-unity-landscape-taxonomy-report.md` — engine stack baseline
- `01-unity-architecture-code-organization-report.md` — folder/code organization, ScriptableObject boundaries, bootstrap patterns
- `03-unity-urp-graphics-lighting-horror-report.md` — URP/lighting diagnostics, shadow cascades, interior light containment
- `04-ngo-multiplayer-architecture-report.md` — NGO authority/ownership rules
- `05-testing-profiling-debug-overlay-report.md` — testing & profiling
- `06-ai-guardrails-and-unity-antipatterns-report.md` — directly informs the rules below
- `07-urp-lighting-architecture-in-unity-6.md` — URP lighting internals/containment behavior for interiors.
- `08-room-based-level-streaming-in-unity-6.md` — scene building strategies for streaming many room nodes.
- `09-multiplayer-room-loading-with-ngo.md` — NGO patterns for authoritative graph-state sync and client-side room loading.
- `10-performance-budgets-for-room-based-horror-games.md` — frame, memory, loading, physics, and networking budget thresholds.
- `11-graph-based-level-representation-in-mutable-horror-games.md` — mutable house-graph data models, invariants, and serialization strategies.
- `12-player-visibility-and-observation-systems.md` — observation-lock visibility heuristics and multiplayer observation-set handling.
- `13-impossible-geometry-techniques-in-real-games.md` — shipped techniques for portals, loops, substitutions, and Tardis-style spaces.
- `14-ai-navigation-in-mutable-graph.md` — stalker/entity navigation on runtime-mutating topology.
- `15-multiplayer-architecture-for-co-op-horror-games.md` — authority, consistency, pacing, and late-join patterns for 2–4 player co-op horror.
- `16-scriptable-object-patterns-for-runtime-game-systems.md` — definition-vs-runtime SO architecture, tooling, validation, and pitfalls.
- `98-unity-mcp-claude-code-guide.md` — Unity MCP + Claude Code operational workflow and guardrails for editor-orchestrated development.

Recommended usage shorthand:
- Use `07`–`10` when de-risking `M1` runtime foundations.
- Use `11`–`13` when implementing graph, observation, and spatial anomaly systems.
- Use `14`–`16` when shaping `M4` AI and cross-system data architecture.
- Use `98` whenever executing Unity work through MCP-driven Claude workflows.

### **WHEN IN DOUBT** 

*If ever not 100% sure how to proceed (design, development, or debugging) and answers aren't clear in repo Unity research* -- with Sonnet web_search subagents:
- Consult the official Unity 6.4 documentation: `https://docs.unity3d.com/6000.4/Documentation/Manual/UnityManual.html`
- Consult the Game Patterns and SOLID Design ebook: `docs/design/98-unity-research/UNITY-TEXTBOOK-Level_up_your_code_with_design_patterns_and_SOLID.pdf`

**Architectural Design Decisions:** Record key, major architectural decisions (and *WHY*) during development to prevent future AI agents from straying, in `docs/ARCH.md`. Especially as it comes to Unity development. 

## Architecture rules

These are non-negotiable. Surface concerns immediately if a task forces you to violate one.

- **Lean code.** Choose the simplest implementation first; tag hardening/robustness as TODOs rather than building speculative complexity.
- **Single-purpose functions.** ~50 LoC budget per function. Exceeding it requires an inline justification comment and a refactor tag — do not silently expand.
- **Meaningful naming.** Functions/variables read on their own. Comments explain *why*, not *what* or *how*.
- **Deep modules / graybox interfaces.** Each system is a self-contained module with a small public surface and complex internals. Work through public interfaces. **Do not import internal files across modules.** If a change requires modifying a public interface, flag it for human review before proceeding. Read `docs/DEEP_MODULES_SPEC.md` if designing new modules or refactoring for guidance.
- **Runtime state vs. definition.** Tunable knobs go on ScriptableObjects (see `_Project/Scripts/Core/GameplaySettings.cs` as the pattern); per-instance runtime state stays on components. Do not collapse the two.
- **Thin scene objects, no manager-god classes.** Scene `MonoBehaviour`s should be wiring/composition. New singletons or "Manager" classes require explicit trade-off discussion before introduction.
- **Debug-first for hidden state.** Any system whose state is invisible to the player gets a debug overlay/gizmo from day one (see `docs/design/05-debug-and-testing/networked-house-debug-overlay-spec.md`).
- **Vertical-slice-first execution.** Build the smallest end-to-end playable thing, then deepen — see `docs/design/05-debug-and-testing/impossible-house-graybox-vertical-slice-plan.md`.
- **Surface concerns immediately.** If you discover conflicting decisions or documentation, immediately surface to Mike for hard decision and alignment.

## Unity MCP Dev: Claude Code Behavioral Directives

When working in a Unity project with MCP enabled, Claude Code should:

1. **Always plan before executing** — output a numbered task list before making tool calls
2. **Prefer `manage_*` tools over raw file I/O** when the MCP server is connected
3. **Write idiomatic C# for Unity** — use `MonoBehaviour`, `[Header]` attributes, `[SerializeField]` for inspector exposure
4. **Never hardcode magic numbers** — use public/serialized fields for speeds, forces, distances
5. **Add error guards** — null-check `GetComponent<>()` calls; log warnings instead of silent failures
6. **Tag GameObjects semantically** — use tags like `"Ground"`, `"Player"`, `"Enemy"`, `"Collectible"`
7. **Prefer prefabs** for any object that appears more than once in a scene
8. **Keep one concern per script** — `PlayerController` handles input/movement; a separate `PlayerHealth` handles damage
9. **After any batch of tool calls**, instruct the user to hit Play and check the Console before continuing
10. **When a script error occurs**, read the full error message and fix the root cause — don't mask with try/catch

## NGO + multiplayer guardrails

Follow the authority/ownership rules in `docs/design/98-unity-research/04-ngo-multiplayer-architecture-report.md`. Key points:

- Server is the authority on game state; clients are owners only of their player input + cosmetic state.
- Use `NetworkVariable<T>` for synced state, ServerRpc for client→server intent, ClientRpc for server→client effects. Do not invert these.
- Spawn networked prefabs through the `DefaultNetworkPrefabs` registry; do not `Instantiate` them locally.
- The `NetworkBootstrapConsistencyTests` regression exists because `ConnectionApprovalCallback`/`NetworkConfig.ConnectionApproval` previously drifted apart (TD0002). If you touch connection approval in `Bootstrap.unity` or `GameBootstrap.cs`, that test must still pass.

**The carried-forward networking is graybox-grade only.** Local LAN host/join works against a hardcoded port (7777). **Cross-machine LAN multiplayer is confirmed working** (host binds `0.0.0.0`; joining machine's built `.exe` must be manually added to Windows Firewall allowed apps — UDP is not auto-prompted). No Relay, no lobby auth, no NAT traversal. Do not yet claim multiplayer "works" beyond the local LAN graybox case until a Relay/Lobby integration lands.

## URP + lighting guardrails

Reference: `docs/design/98-unity-research/03-unity-urp-graphics-lighting-horror-report.md` and `docs/design/03-systems/lighting-and-visibility-spec.md`.

- `_Project/Scenes/House_Graybox.unity` coplanar geometry artifacts are **fixed** (canonically called the "light leak" fix — actual cause was IEEE 754 z-fighting at shared faces, not lighting). Scene is safe to use as a lighting reference. **Construction rules for new rooms:** follow `docs/handoff-prompts/current/GEOMETRY_GRAMMAR.md` — key points: separators extend to wall midpoint (not just inner face), separator tops 0.05m above wall tops (not flush), internal walls trim inward 0.05m. See `docs/ARCH.md` for diagnostic history and S0.3 decision rationale.
- Atmosphere is driven by the tuned `_Project/Settings/AtmosphereVolumeProfile.asset` — modify the profile, not per-scene volumes, when adjusting global mood.
- Lighting communicates state; do not reach for ambient-fill solutions that erase the readability tiers defined in the lighting spec.

## Git workflow

- **Branch for any non-doc, non-trivial change.** Prefixes: `feat/`, `fix/`, `debug/`, `mini-sprint/`.
- **PR flow:** create PR via `/ship` (runs tests, reviews diff, creates PR) → review via `/review` (AI code review) → squash merge to `main` via GitHub.
- **Merge strategy:** squash merge to main for clean linear history. Note: squash merges leave the source branch tip unrecognized by `git branch -d`; use `git branch -D` after merge.
- **Commit messages:** concise, imperative, prefixed (`feat:`, `fix:`, `docs:`, `test:`, `chore:`).
- **Doc-only changes** (markdown, comments, CLAUDE.md updates) can go direct to main if trivial.

## Don'ts

- **Don't** treat the carried Phasmo-Clone code as the architectural template. It is a fixture, not a foundation.
- **Don't** use/defer to Phasmo-Clone era naming schemes (ie, `GhostHunt.*` vs `Desync.*`) as canon over our living, Source-of-Truth design docs `ARCH.md` and `ROADMAP.md`
- **Don't** import internal files across modules. Work through the public interface.
- **Don't** assume cross-machine multiplayer works beyond local LAN. LAN works with the Windows Firewall `.exe` allowance; internet/NAT traversal is not solved.
- **Don't** silently expand a function past the LoC budget. Justify in a comment + tag for refactor, and surface to Mike.
- **Don't** introduce new managers/singletons or new ScriptableObject categories without surfacing the trade-off.
- **Don't** edit Unity `.meta` files manually unless you are explicitly fixing a GUID issue. Let the editor regenerate them.
- **Don't** modify `Library/`, `Temp/`, `Logs/`, or `UserSettings/` — they are editor-local and not source.
- **Don't** read `docs/workspace.md` unsolicited — Mike's scratchpad.
- **Don't** duplicate doc content in code comments or in this file. Link to the source-of-truth doc.
- **Don't** make aspirational claims about systems that don't exist yet (the house graph runtime, observation lock, portals, anchors — if not yet implemented).
- **Don't** ignore discrepencies in docs/code (naming schemes, tech/architectural choices), errors (failed/errored CLI commands, tool calls, MCP requests, fizzled skill invocations) -- **surface them for doc/code alignment**

## Skill routing

When the user's request matches an available skill, **invoke it via the Skill tool as your first action**. Verify skill names against the session's available-skills list — drop any that aren't present.

- **Brainstorming / new feature ideation** → `superpowers:brainstorming`
- **Plan-then-implement workflows** → `superpowers:writing-plans` → `superpowers:executing-plans`
- **TDD on a unit of work** → `tdd` (mattpocock) or `superpowers:test-driven-development`
- **Hard bug / unexpected behavior** → `superpowers:systematic-debugging`, `diagnose` (mattpocock), `investigate` (gstack), or `systematically-debug`
- **Pre-completion verification** → `superpowers:verification-before-completion`
- **Pre-PR review** → `superpowers:requesting-code-review` or `review` (gstack)
- **Adversarial second opinions** → `codex` and `gemini` wrappers for cross-model review
- **Unity Editor orchestration over MCP** → `unity-mcp-skill` (see `docs/UNITY_MCP_LESSONS.md` for known gotchas)
- **Doc/PRD/issue authoring** → `to-prd`, `to-issues`, `write-a-skill` (mattpocock)
- **Researching (External) Documentation** → use the `context7` plugin
- **Post-Sprint: Learning and Retros** → `document-release`, `retro`, `learn` (gstack) for updating documentation, and recording key insights and learning

Local skill suites live under `.claude/skills/` (mattpocock) and are pinned in `skills-lock.json`. Superpowers and gstack are environment-installed.

### Plans/Document Storage
- Claude Code (native): `~/.claude/projects/C--Users-admin-Desktop-Projects-Unity-spatial-horror/memory/`
- gstack: `~/.gstack-dev/plans/` and `~/.gstack/projects/spatial-horror/`
- superpowers: `docs/superpowers/plans/` and `docs/superpowers/specs/`
- speckit: `.specify/{memory,scripts,specs,templates}/` (specs nested per feature: `specs/001-<name>/{spec,plan,tasks,data-model,research}.md` + `contracts/`)

## Subagent delegation (Opus / Sonnet / Haiku + parallel Agents)

Dispatch subagents to conserve the main context window and parallelize work. Give each a self-contained brief with file paths and an explicit "done" definition. Ask for short reports (<300 words) unless long output is required. Never delegate understanding — verify by reading the actual diff/file before marking work done.

- **Opus** — architectural trade-offs, adversarial review, multi-file refactor planning, second opinions.
- **Sonnet** — feature implementation inside a defined spec, doc synthesis (≤10 files), straightforward refactors.
- **Haiku** — mechanical/bulk work: renames, find-and-replace sweeps, batch globs, quick file surveys
- **Parallel Agent dispatch** — multiple Agent tool calls in a single message for independent reads/probes/test runs. Not for anything with ordering or shared state.
- **Codex / Gemini wrappers** — frequently used for adversarial second opinions on plans and diffs and code reviews.
- **Perplexity: Deep Research** — *to offload context/compute/search* proactively ask and write a prompt for Mike in `docs/handoff-prompts/perplexity-research-requests/` if you'd like a comprehensive deep dive research report on a complex topic or problem (distilling over 200-300 sources).

Routing rule of thumb: pure I/O → Haiku; pattern-matching/coding within a known spec → Sonnet; trade-offs/architecture → Opus.

## Imperatives

- **No sycophancy** -- do not praise my ideas: I'm very ignorant of C# and Unity Development. I rely on you to honestly assess ideas and ensure we run a clean, tight ship and follow best practices. 
- **Rule of Threes** -- *FIGHT ME ON SCOPE CREEP* if we're deviating, or I'm attempting to do/add too much during a sprint, push back hard. Note my drift with a timestamp and single sentence comment appended to my `workspace.md` scratchpad file (ie, surface your concern). Suggest (in chat) a forked discussion (to get it out of my system) in separate context/session (offer me seed prompt and suggested skills). Define the issue:
  - *WHAT* are we working on right now, and need to focus on to ship our work;
  - *WHERE* am I drifting, and attempting to lead us to (eg, working on textures and I'm talking about Entity AI);
  - *WHY* you think I'm offtrack, and should defer/recenter;
  - *WHEN* would this feature/expansion be appropriate to address/revist (what sprint/milestone/stage);
  - *HOW* this drift/scope creep should be handled later (validate my idea/drift with a creative `todo` item to add to `TODO.md`)
  - **RITUAL**: *The Rule of Threes* -- if I push back on you **THREE** times, assume I'm serious: relent, and trust my judgement -> /zoom out, rescope, and help me orient for new direction without losing current work (plan out expansion/drift as a TODO item, decide if we need to finish/ship current work, or immediately pivot).
