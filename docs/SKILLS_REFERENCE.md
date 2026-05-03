# SKILLS_REFERENCE — Which Skill, When

**Last updated:** 2026-04-21
**Owner:** Mike
**Audience:** Mike + any session agent doing skill triage
**Source brief:** `docs/handoff-prompts/cowork/outgoing/2026-04-19-skills-reference-deepdive-cc.md`

## Install status (checked 2026-04-21)

| Suite | Location | Count | Health |
|---|---|---|---|
| **Native Claude Code** | bundled into CC binary (no on-disk files) | ~55 commands + ~6 bundled skills | OK |
| **gstack** | `~/.claude/skills/gstack/` | 41 skills | OK — but `/browse` and its downstream stack is **disabled** in this repo per `CLAUDE.md` (Playwright only) |
| **superpowers** | `.claude/sources/superpowers/skills/` | 14 real + 3 deprecated aliases | OK |
| **speckit** | `.claude/skills/speckit-*/` + `.specify/` | 9 skills, v0.7.4.dev0 | **Blocked on constitution** — see §Speckit below |
| **Custom / stray** | `~/.claude/skills/unity-mcp-skill/`, `.claude/skills/logging-file-metadata/` | 2 | OK |

**Not installed (expected by brief):** `engineering:*` plugin skills. Brief assumed they were present; they are not.

---

## TL;DR decision table

| Signal | Primary skill | Fallback / notes |
|---|---|---|
| "Is this worth building?" | gstack `/office-hours` | superpowers `brainstorming` for design-level exploration |
| Vague feature idea → approved spec | superpowers `brainstorming` | gstack `/office-hours` for YC-style "should this exist?" pass |
| Bug triage w/ stack trace | gstack `/investigate` (fresh triage) | superpowers `systematic-debugging` (mid-session discipline enforcement) |
| CC-internal issue (session misbehaving) | native `/debug` | `/doctor` if install-level |
| Pre-commit polish | gstack `/qa` or native `/simplify` | `/simplify` for quality of *recently changed* files; `/qa` for project-wide |
| Pre-merge code review | gstack `/review` → native `/ultrareview` | `/simplify` mid-work; gstack for judgment; `/ultrareview` for cloud multi-agent deep pass |
| Ready-to-ship final check | gstack `/ship` (after `/qa`) | `/ship` before `/land-and-deploy` — sequential, not alternates |
| Merge + prod monitor | gstack `/land-and-deploy` → `/canary` | `/canary` blocked for this repo (browser stack disabled) |
| Security | native `/security-review` (per PR) + gstack `/cso` (periodic) | Complementary, not alternates |
| Post-sprint retro | gstack `/retro` | native `/insights` for raw session stats |
| Mid-session save state | gstack `/context-save` (formerly `/checkpoint`) | `/recap` for in-session re-orientation (lightweight, ephemeral) |
| Repo/doc drift | gstack `/health` | native `/doctor` for CC install drift |
| New feature spec (multi-day, multi-file, external interface) | speckit `/speckit-specify` → `/speckit-plan` → `/speckit-tasks` | superpowers `writing-plans` for in-session plans; `/autoplan` for CEO-lens gauntlet |
| Implementation plan (mid-ceremony, in-repo) | superpowers `writing-plans` | gstack `/autoplan` for quicker multi-lens review |
| Plan-review gauntlet (CEO/design/eng/DX) | gstack `/autoplan` | individual `/plan-*-review` skills for single-lens pass |
| Execute a written plan | superpowers `subagent-driven-development` | `executing-plans` if you want a fresh session instead |
| Parallel independent work streams | superpowers `dispatching-parallel-agents` | native `/batch` for worktree-per-agent large migrations |
| Worktree isolation | superpowers `using-git-worktrees` | no gstack equivalent |
| Before creating/editing a skill | superpowers `writing-skills` | requires `test-driven-development` as prerequisite reading |
| Scheduled recurring task | native `/schedule` | native `/loop` for in-session polling |
| Design — net new | gstack `/design-consultation` | produces `DESIGN.md`; run before `/design-shotgun` |
| Design — explore variants | gstack `/design-shotgun` | after consultation, before finalization |
| Design — audit live site | gstack `/design-review` | visual QA; NOT for a plan (use `/plan-design-review` instead) |
| Design — finalize to HTML | gstack `/design-html` | requires approved mockup |
| External-model second opinion | gstack `/codex` (OpenAI) or `/gemini` (Google) | parallel wrappers; use both if you have both CLIs |

---

## Native Claude Code CLI

These ship with CC itself — no on-disk `SKILL.md`. Source: [code.claude.com/docs/en/commands](https://code.claude.com/docs/en/commands).

### Bundled skills (prompt-based)

| Skill | Purpose | Trigger | Don't use for | Example |
|---|---|---|---|---|
| `/batch <instr>` | Parallel large-scale changes via agents in worktrees | Cross-cutting migrations (rename, API upgrade) | Small single-file edits | `/batch migrate fetch() → apiClient across src/` |
| `/claude-api` | Load Anthropic SDK reference for current language | Writing Claude API / Agent SDK code | Non-Anthropic SDKs | Auto-triggers on `import anthropic` |
| `/debug [desc]` | Enable CC debug logging + analyze session log | CC session itself is misbehaving | Domain bugs — use gstack `/investigate` | `/debug tools returning empty results` |
| `/fewer-permission-prompts` | Scan transcripts, auto-allowlist frequently-used tools | Too many permission prompts per session | One-off tool approvals | `/fewer-permission-prompts` |
| `/loop [int] [prompt]` | Run prompt on interval; autonomous maintenance if no prompt | "keep checking", "every N min", idle-time upkeep | Single-shot tasks | `/loop 5m check deploy status` |
| `/simplify [focus]` | Parallel 3-agent review of recently changed files for quality/reuse, then fixes | Mid-work polish on WIP files | Full PR review — use `/review` or `/ultrareview` | `/simplify focus on error handling` |

### High-value built-in commands (partial — see `/help` for full list)

| Command | Purpose | When to use |
|---|---|---|
| `/ultraplan <prompt>` | Cloud planning session, browser-drafted | Want to plan away from terminal. Prefer gstack `/autoplan` for in-repo four-lens plan critique; `/ultraplan` for fresh browser-drafted planning you'll send back to the terminal |
| `/ultrareview [PR]` | Deep multi-agent cloud PR review | Final pre-merge confidence pass. **3 free Pro/Max runs through 2026-05-05, then ~$5–$20/run.** |
| `/recap` | One-line session summary | Returning after a break; need quick re-orientation. Also auto-fires after an away period (configurable in `/config`) |
| `/autofix-pr [prompt]` | Cloud session watches PR, pushes CI fixes | Let GitHub fix itself while you work |
| `/security-review` | Analyze current branch diff for vulnerabilities | Every PR |
| `/batch` | Parallel worktree-per-agent execution | Large cross-cutting refactors |
| `/plan [desc]` | Enter plan mode | Any non-trivial implementation |
| `/review [PR]` | Local single-pass code review | Quick PR check; see also gstack `/review` |
| `/context` | Colored grid of context usage | "Am I running out of context?" |
| `/compact [instr]` | Summarize conversation to free context | Long session heading toward limit |
| `/rewind` | Rewind conversation/code to a prior point | Undo-by-time-travel |
| `/doctor` | Diagnose CC install, press `f` to auto-fix | CC itself feels broken |
| `/skills` | List available skills (sort by tokens with `t`) | Skill triage, finding what's loaded |
| `/memory` | Edit CLAUDE.md files, manage auto-memory | User memory housekeeping |
| `/insights` | Session analysis report | Self-review your own session |
| `/stats` | Usage visualization | Budget-awareness |
| `/teleport` | Pull claude.ai web session into terminal | Continuing web work locally |
| `/voice` | Push-to-talk voice dictation | Hands-busy dictation |

### Notes on native CC skills

- **Bundled-into-binary.** No directory under `~/.claude/skills/` for them — they appear in `/skills` because CC injects them.
- **Visibility bug:** `/ultraplan` and `/ultrareview` sometimes don't appear in `/help` despite eligibility (GitHub anthropics/claude-code#49510). Fix: `/doctor`, then `/login` to confirm claude.ai auth.
- **Authentication-gated:** `/ultrareview`, `/ultraplan`, `/teleport`, `/voice` require claude.ai login. Not available on Bedrock/Vertex/Foundry/Zero-Data-Retention.

---

## gstack

41 skills. Install: `~/.claude/skills/gstack/<name>/SKILL.md`. Global per CLAUDE.md iron rule.

### Ideation / scoping

| Skill | Purpose | Trigger | Don't use for | Example |
|---|---|---|---|---|
| `/office-hours` | YC-style forcing questions (startup vs builder modes) | "Is this worth building?" | Execution planning — chain to `/plan-eng-review` | "Is the ghost mechanic worth the scope?" |
| `/plan-tune` | Manage AskUserQuestion sensitivity | "Stop asking me about X" | Content of plans | v1: observational only — may not yet suppress |

### Planning (review gauntlet)

| Skill | Purpose | Lens |
|---|---|---|
| `/autoplan` | Runs all four `plan-*-review` skills, auto-decides non-taste choices | **Meta — runs all four** |
| `/plan-ceo-review` | Rethink problem, scope, ambition | CEO / strategic |
| `/plan-eng-review` | Architecture, data flow, edge cases, test coverage | Engineering manager |
| `/plan-design-review` | UX decisions, visual-hierarchy plan | Designer |
| `/plan-devex-review` | Personas, competitor benchmarks, onboarding friction | Developer experience |

### Execution / polish / ship

| Skill | Purpose | Trigger | Don't use for |
|---|---|---|---|
| `/qa` | Full test-fix-verify loop, atomic commits per fix | Pre-commit polish | Report-only — use `/qa-only` |
| `/qa-only` | Structured bug report with screenshots, no fixes | Bug triage before fix decision | Fixing |
| `/review` | Pre-landing PR diff review; SQL/LLM/structural checks | Pre-merge | Full plan architecture — use `/plan-eng-review` |
| `/ship` | Full pre-push: merge base, tests, diff, VERSION, CHANGELOG, PR | "Create a PR" / "push to main" | Post-merge — use `/land-and-deploy` |
| `/land-and-deploy` | Merge PR, wait for CI, verify prod via canary | After `/ship` | Pre-PR prep |
| `/canary` | Post-deploy monitoring via browser | After deploy | Pre-deploy — use `/qa` or `/ship`. **Disabled in this repo.** |
| `/document-release` | Post-ship sync of README, ARCH, CHANGELOG, TODOs | After ship | Docs before shipping |

### Debug / investigate / audit

| Skill | Purpose | Trigger |
|---|---|---|
| `/investigate` | Four-phase debug: investigate → analyze → hypothesize → implement. "No fix without root cause." | "Why is X broken?" |
| `/cso` | Infrastructure-first security audit: secrets, supply chain, OWASP, STRIDE | Periodic security sweep |
| `/health` | Composite 0–10 score across typechecker, linter, tests, dead code | "How healthy is the codebase?" |
| `/retro` | Weekly eng retro from commit history | Sprint end |
| `/learn` | Manage cross-session learnings store | "What have we learned about X?" |

### Safety / guardrails

| Skill | Purpose | When |
|---|---|---|
| `/careful` | PreTool hook that warns before destructive Bash | Touching prod |
| `/freeze` | Block Edit/Write outside a directory for the session | Focused debugging |
| `/guard` | `careful` + `freeze` composite | Full lockdown |
| `/unfreeze` | Clear `/freeze` boundary | Done debugging |

### Session state / context

| Skill | Purpose | Notes |
|---|---|---|
| `/context-save` | Git state + decisions + remaining work snapshot | **Formerly `/checkpoint`** — renamed because CC natively shadows `/checkpoint` |
| `/context-restore` | Load most recent context-save | First action of a resumed session |

### Design family

See TL;DR for the decision row. In short:

| Skill | Role in design pipeline |
|---|---|
| `/design-consultation` | Net-new: research landscape, write `DESIGN.md` |
| `/design-shotgun` | Explore N visual variants (AI-generated comparison board) |
| `/plan-design-review` | Critique a plan's design decisions before coding |
| `/design-review` | Visual QA of a **live site** — finds + fixes polish issues |
| `/design-html` | Finalize approved design into production HTML/CSS |
| `/devex-review` | Live DX audit: actually navigates docs, times TTHW |

### External-model wrappers (parallel, not competing)

| Skill | Wraps |
|---|---|
| `/codex` | OpenAI Codex CLI — review / challenge / consult |
| `/gemini` | Google Gemini CLI — review / challenge / consult |

### Browser automation

| Skill | Notes |
|---|---|
| `/browse` | Headless browser for QA. **Disabled in this repo per CLAUDE.md — use Playwright instead.** |
| `/open-gstack-browser` | Visible AI-controlled Chromium with sidebar |
| `/pair-agent` | Share browser with a remote AI agent |
| `/setup-browser-cookies` | Import real Chromium cookies into headless browse |
| `/connect-chrome` (on disk, no SKILL.md) | Launch real Chrome under gstack control |

**Blocked stack warning:** Because `/browse` is disabled, `/canary`, `/qa`, `/devex-review`, and parts of `/setup-browser-cookies` that depend on the gstack browse daemon are also functionally blocked. Use Playwright directly for browser tasks.

### Utilities

`/make-pdf`, `/setup-deploy`, `/gstack-upgrade`.

### gstack internal overlap rules

- `/qa` vs `/qa-only` → `/qa-only` is report-only; `/qa` fixes in place.
- `/ship` vs `/land-and-deploy` → sequential, not alternates. `/ship` creates PR; `/land-and-deploy` merges + monitors.
- `/review` vs `/plan-*-review` → `/review` is post-code; `/plan-*` is pre-code.
- `/context-save` vs `/learn` → `context-save` is session-level resume data; `/learn` is a persistent cross-session knowledge store.
- `/careful` vs `/freeze` vs `/guard` → `/guard` is just `/careful` + `/freeze` composed.

### gstack strengths (top 5) and weaknesses (top 3)

**Strong:** `/investigate` (Iron Law "no fix without root cause"), `/ship` (comprehensive pre-push), `/cso` (multi-domain security), `/office-hours` (forcing-question discipline), `/autoplan` (orchestrates four-lens gauntlet).

**Weak / confusing:** `/plan-tune` (v1 observational — may not actually suppress), `/open-gstack-browser` vs `/browse` (trigger phrases overlap, easy to fire wrong one), `/pair-agent` (very niche; confusing trigger phrasing).

---

## superpowers

14 real skills + 3 deprecated aliases. Install: `.claude/sources/superpowers/skills/<name>/SKILL.md`.

### Process skills (determine HOW)

| Skill | Purpose | Trigger | Don't use for |
|---|---|---|---|
| `using-superpowers` | Session-start ritual — establishes skill-first behavior | First action of a non-subagent session | Subagent sessions (skill self-skips on `SUBAGENT-STOP`) |
| `brainstorming` | Vague idea → approved design + spec, before any code | Creative work, feature request | Bugs already diagnosed |
| `writing-plans` | Spec → bite-sized (2–5 min) implementation plan with TDD baked in | Approved spec, multi-step feature | Single-step trivial changes |
| `executing-plans` | Load a plan and run it serially in a **separate session** | No subagents available | Prefer `subagent-driven-development` when subagents exist |
| `subagent-driven-development` | Fresh subagent per task + two-stage review (spec then quality) in current session | Plan ready, tasks mostly independent | Tightly coupled tasks |
| `dispatching-parallel-agents` | 2+ independent problem domains → concurrent subagents | Multiple subsystems broken | Shared-state / sequential tasks |
| `systematic-debugging` | Root cause before any fix. "Iron Law: no fixes without RCA." | Any unexplained bug | Known root cause |
| `test-driven-development` | Red-Green-Refactor. "Iron Law: no production code without a failing test first." | Any feature or bugfix | Prototypes, generated code (with explicit sign-off) |
| `verification-before-completion` | Run evidence-gathering commands before claiming "done" | About to say "tests pass" / "fixed" | Mid-task status checks |
| `using-git-worktrees` | Create isolated git worktree for feature work | Before executing a plan | Small hotfixes on main |
| `requesting-code-review` | Dispatch scoped reviewer subagent after completed work | End of task or pre-merge | Mid-implementation |
| `receiving-code-review` | Process inbound review with rigor — verify before implementing | Got review comments | Blindly agreeing |
| `finishing-a-development-branch` | Verify tests, guide merge/PR/cleanup | Work done, tests pass | Mid-feature |
| `writing-skills` | Create/edit skills with TDD-for-docs. Requires `test-driven-development` as background | Adding or fixing a skill | General documentation |

### Deprecated aliases

`superpowers:brainstorm` → `brainstorming`. `superpowers:write-plan` → `writing-plans`. `superpowers:execute-plan` → `executing-plans`. They appear in the skill list but are stubs.

### Superpowers philosophy

Rule-based behavior shaping, not advisory guidance:

- **Iron Laws, not recommendations.** Every load-bearing skill has a named law ("NO PRODUCTION CODE WITHOUT A FAILING TEST FIRST", etc.) with explicit violation language.
- **Context preservation is architectural.** Subagents never inherit session context — the controller curates exactly what they need.
- **TDD is load-bearing.** `writing-skills`, `systematic-debugging`, and `subagent-driven-development` all reference or depend on it. Without TDD compliance the suite loses coherence.
- **Reach for superpowers when you need discipline enforcement, not planning horsepower.** gstack gives you richer planning critique; superpowers gives you a process that is hard to violate accidentally.

### Superpowers strengths and weaknesses

**Strong:** `test-driven-development` (hardest gate in the suite), `subagent-driven-development` (two-stage review after every subagent task — unmatched granularity), `verification-before-completion` (frames unverified claims as "dishonesty, not efficiency").

**Situational:** `using-git-worktrees` (mechanical — arguably a habit, not a skill), `executing-plans` (its own SKILL.md tells you to prefer subagent-driven-development).

---

## speckit

9 skills. Install: `.claude/skills/speckit-*/` + `.specify/`. Version: 0.7.4.dev0, installed 2026-04-17. **Heavier ceremony than the other suites — don't default to it.**

### Per-skill table

| Skill | Purpose | Input | Output |
|---|---|---|---|
| `/speckit-constitution` | Create/update project-wide principles governing all future specs | Interactive or args | `.specify/memory/constitution.md` |
| `/speckit-specify` | Natural-language feature description → structured spec with prioritized user stories | Free text | `.specify/specs/NNN-<name>/spec.md` |
| `/speckit-clarify` | Find underspecified gaps in spec, ask up to 5 targeted questions, encode answers | `spec.md` | Updated `spec.md` in place |
| `/speckit-checklist` | Domain-specific requirement-quality checklist ("unit tests for English") | `spec.md` + domain | `checklist.md` in feature spec dir |
| `/speckit-plan` | Approved spec → architecture + contracts + research artifacts | `spec.md` | `plan.md`, `research.md`, optional `contracts/`, `data-model.md` |
| `/speckit-tasks` | Design artifacts → dependency-ordered, parallelism-annotated task list | `plan.md` + `spec.md` (+ research/data-model/contracts) | `tasks.md` |
| `/speckit-analyze` | Cross-artifact consistency and quality check | `spec.md` + `plan.md` + `tasks.md` | Inline analysis report |
| `/speckit-implement` | Execute `tasks.md` sequentially with hook support | `tasks.md` | Code changes |
| `/speckit-taskstoissues` | Convert tasks into GitHub Issues with labels + ordering | `tasks.md` | GitHub Issues via `gh` |

### Constitution status: **STUB**

`.specify/memory/constitution.md` is the unfilled template — placeholders like `{{PROJECT_NAME}}`, `[PRINCIPLE_1_NAME]`, etc. ~51 lines, no real content. The `plan-template.md` has a "Constitution Check" section that will reference empty governance until this is filled.

**→ Before running any other speckit skill for this project, run `/speckit-constitution` once.**

### Canonical pipeline (from `.specify/workflows/speckit/workflow.yml`)

```
constitution (one-time setup)
  ↓
specify → [review-spec gate] → plan → [review-plan gate] → tasks → implement
```

Review gates are blocking — reject aborts the pipeline.

**Not in the automation graph (manual-invoke only):** `clarify`, `checklist`, `analyze`, `constitution`, `taskstoissues`. These are satellites. `clarify` is optional between `specify` and `plan`. `checklist` is a pre-plan quality gate. `analyze` is typically post-tasks / post-implement. `taskstoissues` is post-tasks when handing off to GitHub.

### Speckit decision flowchart

```
Is this worth formal spec-driven dev?
├── Under a half-day of work, no external contracts, obvious design → SKIP SPECKIT
│   → superpowers writing-plans OR gstack /autoplan
├── Bug fix / refactor / polish → SKIP SPECKIT
│   → gstack /investigate or superpowers systematic-debugging
├── Solo session, no handoff, no audience for the artifacts → SKIP SPECKIT
│   → overhead kills ROI
└── Multi-day work, external interface (network, save format, system contracts),
   multiple independently-testable user stories, or needs traceability
   ├── Is .specify/memory/constitution.md filled?
   │   ├── STUB (current state, 2026-04-21) → /speckit-constitution FIRST
   │   └── Filled → continue
   ├── /speckit-specify "<feature description>"
   ├── [review spec gate] — abort if rejected
   ├── /speckit-clarify (optional — if spec feels ambiguous)
   ├── /speckit-checklist (optional — requirement quality pass)
   ├── /speckit-plan
   ├── [review plan gate] — abort if rejected
   ├── /speckit-tasks
   ├── /speckit-analyze (optional — cross-artifact consistency)
   ├── /speckit-implement     OR hand tasks.md off to
   │                             superpowers:subagent-driven-development
   └── /speckit-taskstoissues (optional — if using GitHub Issues)
```

### When speckit is worth it

- Feature > half-day AND has non-obvious design decisions.
- External interfaces (network protocol, save format, cross-system Unity contracts) where `contracts/` adds real value.
- Multiple independently-testable user stories.
- Traceability needed (handoff to other agent session, post-mortem audit).

### When to skip speckit

- Under 2–3 hours, obvious design, no external contract.
- Fast-iteration / prototype mode (pivoting more likely than following a plan).
- Bug fix / refactor / polish.
- Solo session, no handoff — the artifact trail has no audience.

### Speckit strengths and weaknesses

**Strong:** `/speckit-specify` (vague intent → prioritized, independently-testable spec — pays dividends downstream), `/speckit-tasks` (dependency-ordered with `[P]` parallelism markers — directly consumable by subagent dispatch).

**Situational:** `/speckit-taskstoissues` (only useful with GitHub Issues — Mike uses `docs/TODOS/` instead), `/speckit-checklist` (requirement-quality gate is ceremony-heavy for a solo dev).

### `.specify/` cheat sheet

```
.specify/
├── memory/constitution.md       ← STUB, needs /speckit-constitution
├── templates/                    spec, plan, tasks, constitution, checklist
├── workflows/speckit/workflow.yml   authoritative pipeline
├── integrations/                 claude.manifest.json, speckit.manifest.json, integration.json
└── scripts/powershell/           check-prerequisites, common, create-new-feature, setup-plan
```

No specs have been generated in `.specify/specs/` — speckit has not yet been used on this project.

---

## Stray / custom skills

| Skill | Location | Origin | Notes |
|---|---|---|---|
| `unity-mcp-skill` | `~/.claude/skills/unity-mcp-skill/` | CoplayDev Unity MCP | Unity orchestration via MCP tools; see CLAUDE.md "Unity MCP" section |
| `logging-file-metadata` | `.claude/skills/logging-file-metadata/` | Mike-authored | Standardizes file-metadata logging in Git Bash on Windows when copying assets |

---

## `/plan` disambiguation — which "plan" is which

All four suites use "plan" in different ways. These are **not interchangeable**:

| Invocation | Suite | Does what | Produces |
|---|---|---|---|
| `/plan [desc]` | Native CC | Enters plan mode — an interactive planning state, execution-oriented | In-session plan, not persisted |
| `/ultraplan <prompt>` | Native CC | Cloud planning session drafted in browser, sent back to terminal | Plan artifact returned to session |
| `/autoplan` | gstack | Runs all four `plan-*-review` skills in sequence, auto-decides non-taste choices | Multi-lens critique of an existing plan |
| `/plan-ceo-review`, `/plan-eng-review`, `/plan-design-review`, `/plan-devex-review` | gstack | Single-lens critiques of an existing plan | Review report |
| `writing-plans` | superpowers | Produce a bite-sized (2–5 min step) in-repo plan from an approved spec, TDD-integrated | Markdown plan file in `docs/superpowers/plans/` |
| `executing-plans` | superpowers | Load a plan file and run it serially in a separate session | Code changes per plan |
| `/speckit-plan` | speckit | Spec → architecture + contracts + research artifacts | `plan.md`, `research.md`, `contracts/`, optional `data-model.md` in `.specify/specs/NNN-name/` |

**Rule of thumb:** if you don't have a plan yet and need to write one → `writing-plans` (in-repo, bite-sized) or `/speckit-plan` (ceremony + artifacts). If you have a plan and need it critiqued → `/autoplan` or the individual `plan-*-review` skills. If you want to draft a plan away from the terminal → `/ultraplan`.

---

## Overlap matrix — which to pick when more than one matches

| Task | Native CC | gstack | superpowers | speckit | Winner | Why |
|---|---|---|---|---|---|---|
| Plan an implementation (in-repo, multi-day) | `/plan` (mode) | `/autoplan` | `writing-plans` | `/speckit-plan` | **superpowers** for in-repo engineer-focused TDD-integrated plans; **speckit** if artifacts must survive sessions / contracts matter; **gstack** for four-lens review on someone else's plan | Different output scopes — not interchangeable |
| Debug a bug | `/debug` (CC internals only) | `/investigate` | `systematic-debugging` | — | **gstack `/investigate`** for fresh triage; **superpowers** for mid-session discipline on a known bug | gstack is lighter-weight |
| Pre-merge code review | `/review`, `/ultrareview`, `/simplify` | `/review` | `requesting-code-review` | — | Chain: `/simplify` → gstack `/review` → `/ultrareview` | Different rigor tiers, compose |
| Pre-commit verification | — | `/qa` | `verification-before-completion` | — | Both — they compose | sp = micro-gate per task; gstack `/qa` = macro-gate per session |
| Parallel subsystem work | `/batch` | — | `dispatching-parallel-agents` | — | `/batch` for worktree-per-agent large migrations; **superpowers** for in-session parallel domains | `/batch` spawns isolated worktrees; sp stays in session |
| Schedule recurring task | `/schedule`, `/loop` | — | — | — | `/schedule` for persistent cron; `/loop` for in-session | Only native |
| Session save | `/recap` (ephemeral) | `/context-save` | — | — | `/recap` for quick re-orientation; **gstack** `/context-save` for full state | Different depth |
| Security | `/security-review` | `/cso` | — | — | Both — compose | `/security-review` per-PR diff; `/cso` periodic multi-domain |
| Write a skill | — | — | `writing-skills` | — | **superpowers** | Only one that exists |
| Worktree setup | — | — | `using-git-worktrees` | — | **superpowers** | No gstack/native worktree skill |
| Create GitHub issues from tasks | — | — | — | `/speckit-taskstoissues` | **speckit** | Only one; requires `gh` |
| Validate requirement quality pre-code | — | — | — | `/speckit-checklist` | **speckit** | No peer |
| Ship (create PR + merge + monitor) | `/autofix-pr` (post-merge) | `/ship` → `/land-and-deploy` → `/canary` | `finishing-a-development-branch` | — | **gstack chain**; use sp `finishing-a-development-branch` mid-decision if unclear which gstack stage | Gstack covers full lifecycle |

---

## Multi-suite workflows (worth memorizing)

### Full feature lifecycle (in-repo, multi-day)

```
gstack /office-hours                    (is this worth building?)
→ superpowers brainstorming             (idea → approved spec)
→ superpowers using-git-worktrees       (isolate)
→ superpowers writing-plans             (spec → bite-sized plan)
→ gstack /autoplan                      (four-lens plan critique — optional but valuable)
→ superpowers subagent-driven-development   (execute per-task w/ review)
→ superpowers requesting-code-review    (after each task)
→ gstack /qa OR /simplify               (polish WIP)
→ gstack /review → native /ultrareview  (pre-merge depth)
→ superpowers finishing-a-development-branch
→ gstack /ship → /land-and-deploy → /canary   (canary blocked in this repo)
→ gstack /document-release              (post-ship doc sync)
```

### Speckit-driven feature (when ceremony is worth it)

```
/speckit-constitution                   (one-time, constitution is currently STUB)
→ /speckit-specify → review gate
→ /speckit-clarify (optional)
→ /speckit-plan → review gate
→ /speckit-tasks → /speckit-analyze
→ superpowers subagent-driven-development   (hand tasks.md off)
OR /speckit-implement                  (sequential, with hooks)
→ gstack /review → native /ultrareview → /ship
```

### Bug investigation

```
gstack /investigate                     (fresh triage: four-phase RCA)
→ superpowers systematic-debugging      (mid-session discipline if rabbit-hole)
→ superpowers test-driven-development   (write failing test for the bug)
→ superpowers verification-before-completion  (confirm fix)
→ gstack /review → /ship
```

### Pre-merge confidence ramp

```
native /simplify              (quality fixes on changed files)
→ gstack /review              (judgment: should this merge?)
→ native /ultrareview         (cloud multi-agent bug hunt)
→ gstack /ship
```

### Re-orienting after a break

```
native /recap                 (one-liner re-orientation)
→ gstack /context-restore     (full saved state)
→ native /compact "retain architecture decisions"  (trim to essentials)
→ resume
```

### Large refactor / migration

```
gstack /office-hours          (is this worth the disruption?)
→ speckit /speckit-specify + /speckit-plan   (contracts/data-model matter here)
→ native /batch <instruction>  (worktree-per-agent fleet executes)
→ native /ultrareview          (cloud verify fleet output)
→ gstack /ship
```

---

## Anti-patterns

- **Invoking `/ship` before `/qa`.** Always `/qa` first. `/ship` assumes a clean tree.
- **Using speckit for a <1-day solo task.** Ceremony overhead kills ROI. Use `writing-plans` or `/autoplan`.
- **Running `/speckit-plan` before the constitution is filled.** Produces a plan with an empty "Constitution Check" section — meaningless governance reference.
- **Calling `/review` on your own WIP scratchpad.** Use `/context-save` or `/simplify` instead.
- **Using gstack `/autoplan` AND superpowers `writing-plans` in the same session for the same feature.** Pick one. They produce different artifact shapes; duplicating wastes cycles.
- **Using `/qa-only` when you want fixes.** It's report-only by design — the name is literal.
- **Using gstack `/browse` / `/canary` / `/devex-review` in this repo.** CLAUDE.md forbids `/browse`; Playwright is the replacement. The browse daemon being blocked also functionally blocks `/canary`.
- **Invoking `/ultrareview` on a tiny PR.** It burns cloud budget ($5–$20/run after free allotment) — use gstack `/review` for small diffs.
- **Running `context-save` after forgetting the skill was renamed from `/checkpoint`.** `/checkpoint` is a native CC alias — it will shadow. Use `/context-save` explicitly.
- **Using `superpowers:brainstorm` / `:write-plan` / `:execute-plan`.** These are deprecated aliases — the skill list still shows them but they're stubs pointing to the real names.
- **Using `/speckit-taskstoissues` without `gh` CLI configured.** Produces cryptic failures.

---

## Routing rules for subagents

If you're a subagent dispatched on a task, use these triggers first:

- User said "brainstorm" / "is this worth doing" → gstack `/office-hours` OR superpowers `brainstorming` (office-hours for product-validation lens; brainstorming for design/spec production).
- User said "why is X broken" with a stack trace → gstack `/investigate`.
- User said "ready to commit" → gstack `/qa` first; `/ship` only after.
- User said "spec this out" → check size. Under 1 day → superpowers `writing-plans`. Multi-day + multi-file + external interface → speckit `/speckit-specify`.
- User said "write a plan" → superpowers `writing-plans` unless they explicitly asked for speckit ceremony.
- User said "review my code" → gstack `/review` first, then `/ultrareview` if depth needed.
- User said "I'm stuck / where was I" → native `/recap` first, then `/context-restore` if the recap isn't enough.
- User said "quick polish" → native `/simplify`.
- User said "full audit" → gstack `/qa` (code) or `/cso` (security) or `/health` (overall).

---

## Plan / artifact storage cheat sheet

Pulled from CLAUDE.md "Plans/Document Storage" — restated inline:

| Suite | Plans live at |
|---|---|
| Claude Code (native memory) | `~/.claude/projects/C--Users-admin-Desktop-Projects-Unity-phasmo-clone/memory/` |
| gstack | `~/.gstack-dev/plans/` and `~/.gstack/projects/phasmo-clone/` |
| superpowers | `docs/superpowers/plans/` and `docs/superpowers/specs/` |
| speckit | `.specify/{memory,scripts,specs,templates,workflows,integrations}/` (specs nested per feature: `specs/001-<name>/{spec,plan,tasks,data-model,research}.md` + `contracts/`) |

---

## Outstanding questions for Mike

See `docs/handoff-prompts/cowork/incoming/2026-04-21-skills-reference-wrap.md` for the wrap-up and open questions (engineering plugin, constitution scaffolding, logging-file-metadata classification).
