

## Coding Principals

- **LEAN CODE** -- Write *simple*, *clean* code. Do not overengineer. Choose *simplest implementation* first, and note hardening or robust steps as items for TODOS.
- **Single-Purpose Functions** -- religiously *contain scope* of functions and classes. Baseline: all functions should be **less than 50 lines of code**. If a function requires more code, it *must* be justified in a comment, and noted as an architectural decision, and tagged for future refactor/review as the project evolves/grows.
- **Meaningful Naming Schemes** -- me or any other AI agent should be able to read a function and understand its purpose, inputs, and outputs based on the naming of that function and its variables alone. Names should be meaningful enough to stand alone without comments (comments should explain the *WHY* -- not the *WHAT* or *HOW*). 
- **Interfaces** -- This repo will follow a **Deep Module**/**Graybox** design pattern (see blurb below). Review the concept during the planning stage for *all* work. Do *not* compromise on this principal -- and *immediately* surface concerns where it's been violated. 

## Architecture: Deep Modules

This codebase follows the **Deep Module** design pattern. Each feature area is organized as a self-contained service with a simple, intentional public interface (`index.ts` + `types.ts`) and complex implementation kept internal. 

**When making changes:**
- Identify the affected module(s) before writing code
- Work through the public interface — do not import internal files from other modules
- Write or update tests at the interface level to lock in behavior
- You own the implementation; the developer owns the interface design and module boundaries
- If a change requires modifying a public interface, flag it for human review before proceeding

## Repo Structure

**Higher-level**

```
docs/
  design/
  handoff-prompts/
    archived/
    cowork/
      incoming/
      outgoing/
    current/

    README.md
  templates/
    PROMPT_TEMPLATES.md
    TODOS_INDEX.md
    TODOS_TEMPLATES.md

  SKILLS_REFERENCE.md
  TRIFORCE.md
  TODOS.md
  TODOS_INDEX.md
  workspace.md
unity-spatial-horror/
  # Unity project mounted directory

CLAUDE.md
README.md
```


**Unity Project**


## Skill routing

When the user's request matches an available skill, **ALWAYS invoke it via the Skill tool as your FIRST action**. Do not answer directly or use other tools first — skills have specialized workflows that beat ad-hoc answers.

### gstack routing
- Product ideas, "is this worth building", brainstorming → `/office-hours`
- Bug triage, rabbit holes, "why is this happening" → `/investigate`
- Code review requests, PR checks → `/review`
- End-of-session polish, pre-commit verification → `/qa`
- Ready-to-ship checks, final pre-push → `/ship`
- Post-sprint learning capture → `/retro`
- Mid-session commit-worthy checkpoint → `/checkpoint`
- Repo/doc drift detection → `/health`

### Local skill suites
- `speckit-*` from GitHub Spec Kit (in `.claude/skills/`)
- gstack from `.claude/skills/gstack/`
- superpowers from `.claude/sources/superpowers/skills/`

### Plans/Document Storage
- Claude Code (native): `~/.claude/projects/C--Users-admin-Desktop-Projects-Unity-phasmo-clone/memory/`
- gstack: `~/.gstack-dev/plans/` and `~/.gstack/projects/phasmo-clone/`
- superpowers: `docs/superpowers/plans/` and `docs/superpowers/specs/`
- speckit: `.specify/{memory,scripts,specs,templates}/` (specs nested per feature: `specs/001-<name>/{spec,plan,tasks,data-model,research}.md` + `contracts/`)

---

## Subagent delegation (Opus / Sonnet / Haiku + Agent Teams)

Dispatch subagents to conserve the main context window and parallelize work. Route by cost and cognitive load:

**When spawning:** give a self-contained prompt with file paths and an explicit "done" definition. Ask for a short report (<300 words) unless the task requires long output. Treat the summary as input — verify by reading the actual diff/file before marking done.

- **Opus** — complex reasoning, architectural trade-offs, adversarial review. System design, ADR drafting, multi-file refactor plans, `P[~0-1]` triage, "second opinion" reviews.
- **Sonnet** — day-to-day workhorse. Feature implementation inside a defined spec, `P[~2-4]` rescoring, doc synthesis ≤10 files, straightforward refactors.
- **Haiku** — mechanical / bulk / low-judgement. Renames, find-and-replace sweeps, `P[?]` → `P[~N]` ballpark, batch script runs, glob/grep surveys.
- **Agent Teams (parallel dispatch)** — independent work streams via multiple Agent invocations in a SINGLE message. Good for parallel doc reads, parallel research probes, independent test runs. Bad for anything with ordering or shared state.
- **Codex & Gemini CLI** -- Frequently deploy Codex and Gemini instances to review plans and work to provide second opinions, audits, and adversarial code/design reviews. 

**Rules of thumb**
- Pure I/O (read, list, grep) → Haiku. Pattern-matching / code in a known spec → Sonnet. Trade-offs / architecture → Opus.
- If you'll quote from the response → ask for a word cap. If you'll scan for a fact → ask for structured format (table/JSON/bullets).
- Never delegate understanding — prompts must be specific and self-contained, never "based on your findings, continue".

---

## Context loading (session start)

Budget ~5–10k tokens total before touching code. Use **Haiku** subagent to scan and suggest critical context docs for current planning/work. Use **Sonnet** subagents to distill and extract the core knowledge, guidance, and insights from those docs rather than ingesting all content/docs. 

**Tier 1 — always (core orientation):**
1. `CLAUDE.md` (this file)
2. `docs/ARCHITECTURAL_DECISIONS.md` — locked decisions, do not re-debate
3. `docs/PROGRESS.md` — current milestone / sprint / sub-sprint state

**Tier 2 — starting a new sprint or sub-sprint:**
4. `docs/TODOS/TODOS_INDEX.md` — glance-read before scoping decisions; lists all active TODOs with milestone + dependency
5. Most recent wrap-up in `docs/handoff-prompts/archive/` (or `current/` if unarchived)
6. Active session doc in `docs/handoff-prompts/current/` (if any)
7. Relevant `docs/design` doc(s) for your target phase

**Tier 3 — task-specific (load on demand):**
- Adding a TODO → `docs/TODOS/TODOS_INDEX.md` + `docs/TODOS/TODOS_TEMPLATES.md` + `docs/TODOS/TODOS.md`
- Bug fix → `docs/TODOS/ARCHIVED_TODOS.md` (search for prior related fixes)
- Dev-mode work (code/Unity) → `docs/DEV_GATES.md`
- Doc writing → `docs/research/CONTRIBUTING.md` + `docs/research/Sprint {N} Dev Report Template.md`
- Post-mortem → `docs/handoff-prompts/templates/post-mortem-primer.md`

Never read `Library/PackageCache/` or `docs/workspace.md` (Mike's scratchpad) unsolicited. Heavy phase implementation guides (≈900 lines each) — load only when implementing that phase.

### Design Docs

The dir `docs/design/` contains various context documents for you to reference when relevant, broken into `epic-{n}/sprint-{m}/` and higher-level vision docs `vision/`, `player-exp-ux/`. Use subagents to scan/review the available context docs for anything immediately relevant based on your planning / pre-planning to stay aligned with the vision, design pillars, and current work/architectural decisions. Dispatch the agent (likely **Sonnet**) with a short brief on current work and direction for intelligent search. Another agent should always be dispatched to understand the repo structure and what files will be touched.

---

## Handoff prompts

Sessions hand off between Cowork (producer), Claude Code (engineer), and Mike (human in the loop) via `docs/handoff-prompts/`. Cowork drafts briefs into `cowork/outgoing/`; CC picks them up in fresh sessions and wraps up into `cowork/incoming/`. Sprint-end, move to `archive/`.

Full structure, naming convention, and flow → `docs/handoff-prompts/README.md`.

---

## Sandbox Git Corruption — Ghost Filesystem Claims (2026-04-19)

**Symptoms observed:**
- `.git/HEAD` read as a truncated branch name (e.g. `ref: refs/heads/cowor` instead of `cowork/contributing-archive-cleanup`)
- `git rev-parse HEAD` errored
- `.git/index.lock` present and unbreakable
- `git status` showed files as untracked despite `git log main` working
- `git status` reported staged deletions for an entire directory (`.specify/`) that had never been touched

**Current hypothesis:**
- We now suspect it has to do with how OneDrive loads files, and the discrepency between how Claude Cowork can touch files/git through the sandbox, and how Claude Code CLI can (rooted in the repo via VS Code). We've confirmed this is a common issue in the community, and the workarounds are... duct tape. Defer git actions to me or Claude Code CLi in VS Code. 
