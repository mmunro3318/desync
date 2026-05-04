# Triforce — A Multi-Agent Development Coordination Framework

**Version:** 1.0 (codified 2026-04-22)
**Status:** Portable. Repo-agnostic. Copy this file and its companion templates into a fresh repo to bootstrap the workflow.
**One-line pitch:** A coordination pattern for solo developers working with Claude Code, Claude Cowork, and compute-offload CLI tools, where a filesystem-based handoff layer (markdown docs) mediates all work between a human operator and N AI agents of varying capability, cost, and context-window size.

---

## Table of Contents

0. [When to use this framework](#when-to-use)
1. [Core concept](#core-concept)
2. [The three principals](#the-three-principals)
3. [Extended compute tiers](#extended-compute-tiers)
4. [Handoff-prompts — the coordination layer](#handoff-prompts)
5. [Compute economy — routing decisions](#compute-economy)
6. [Verification gates](#verification-gates)
7. [Setting up Triforce in a new repo](#setup)
8. [Operational patterns](#operational-patterns)
9. [Appendix A — Brief (CC fork doc) template](#appendix-a)
10. [Appendix B — Wrap-up template](#appendix-b)
11. [Appendix C — Compute-offload CLI prompt template](#appendix-c)
12. [Appendix D — Subagent prompt guidelines](#appendix-d)
13. [Appendix E — Scheduled-task prompt guidelines](#appendix-e)
14. [Appendix F — Operator-facing dispatch menu template](#appendix-f)
15. [Appendix G — Session-continuity doc (pre-compaction handoff)](#appendix-g)

---

<a id="when-to-use"></a>
## 0. When to use this framework

Use Triforce when **all** of the following are true:

- You are a solo (or very small team) developer.
- You use Claude Code (or equivalent agentic CLI) AND Claude Cowork (or equivalent long-thread web/desktop agent) in the same repo.
- The work is non-trivial enough that context windows matter — individual sessions compact, and you want continuity across compactions.
- You have access to at least one compute-offload option (GitHub Copilot, Aider, Cursor, Gemini CLI, Codex CLI, etc.) — or you want to keep that option open.
- You prefer explicit, auditable filesystem state over in-app memory / chat history.

Don't use Triforce if you're working single-session on a throwaway script, or if the human operator doesn't mind re-explaining context every session.

---

<a id="core-concept"></a>
## 1. Core concept

**Triforce is named for its three primary corners**, but it is actually a multi-tier system. The "tri" names the humans-and-strategic-agents layer; in practice there are up to six distinct agent tiers coordinating through a single shared filesystem.

### 1.1 The six tiers

```
┌─────────────────────────────────────────────────────────────┐
│ TIER 0: OPERATOR (human)                                     │
│   Decides, vetoes, reviews. Authors the first prompt.        │
├─────────────────────────────────────────────────────────────┤
│ TIER 1: ORCHESTRATOR (Cowork / long-thread strategic agent)  │
│   Strategy. Synthesis. Audits. Doc authorship.               │
│   Multi-MCP surface. Persists across sessions via disk.      │
├─────────────────────────────────────────────────────────────┤
│ TIER 2: ENGINEER (Claude Code / agentic CLI)                 │
│   Execution in the repo. IDE-native tooling.                 │
│   Fresh session per task. Branches + wraps.                  │
├─────────────────────────────────────────────────────────────┤
│ TIER 3: COMPUTE-OFFLOAD CLI (Copilot / Aider / Cursor / etc.) │
│   Mechanical labor. Git ops, renames, sweeps.                │
│   Paid by a different / same usage plan.                     │
├─────────────────────────────────────────────────────────────┤
│ TIER 4: SUBAGENTS (in-session Task-tool spawns)              │
│   Parallel workstreams within ONE session.                   │
│   Model-tiered (Opus/Sonnet/Haiku). Ephemeral.               │
├─────────────────────────────────────────────────────────────┤
│ TIER 5: SCHEDULED TASKS (autonomous background workers)      │
│   Fire at absolute times. Stateless. Output via dir.         │
└─────────────────────────────────────────────────────────────┘
```

### 1.2 The single shared state

All tiers coordinate through **a single directory tree of markdown files** living at `docs/handoff-prompts/` (path configurable). No in-app memory, no vendor-specific chat history, no session state. Files are the source of truth.

This is the core discipline: **if a fact isn't written to disk, it doesn't exist for the next agent**. Anything you care about must be in a markdown file.

### 1.3 Why filesystem over memory

- **Compaction-resistant.** When a long thread compacts, its context is lost but the filesystem survives.
- **Vendor-portable.** The same markdown works whether the next session is Claude, GPT, or a future model. No lock-in.
- **Auditable.** Git history gives you a free audit trail of every coordination event.
- **Human-readable.** The operator can grep, diff, and review without tooling.
- **Parallelizable.** Multiple agents can read the same file simultaneously; writes are rare enough that conflicts are manageable.

---

<a id="the-three-principals"></a>
## 2. The three principals (Tiers 0–2)

### 2.1 Tier 0 — The operator (human)

**Scope:** vision, priorities, final veto, human-in-the-loop review.

**Doesn't do:** execution at scale, multi-file implementation work, repetitive synthesis.

**Primary artifacts:** answers inline in dispatch menus, merges on branches, decisions captured in ARCH docs.

**Key behaviors:**
- Writes an initial "what I want" prompt to the orchestrator.
- Reviews orchestrator-authored briefs before dispatching engineers.
- Dispatches engineers (opens fresh sessions, pastes brief paths).
- Reviews wrap-ups, merges branches, or sends back for revision.
- Answers blocking questions inline in handoff docs.

### 2.2 Tier 1 — The orchestrator (Cowork / strategic long-thread)

**Scope:** strategy, synthesis, audits, doc authorship, multi-MCP operations, scheduled-task orchestration.

**Doesn't do:** repo-local code editing (no IDE), long-running unattended work (session compaction).

**Primary artifacts:** briefs (fork docs) in `handoff-prompts/cowork/outgoing/`, synthesis docs, dispatch menus, audits.

**Key behaviors:**
- Reads the operator's initial intent.
- Authors briefs for engineers (one brief per fresh engineer session).
- Reads engineer wrap-ups and synthesizes them into project state.
- Authors operator-facing dispatch menus ("here's what's in flight, here's what's parked").
- Runs audits (MCP surface, skill catalog, doc freshness, drift detection).
- Registers scheduled tasks for recurring/autonomous work.
- Emits continuity docs before compaction (see Appendix G).

**Key strength:** multi-MCP surface. The orchestrator can talk to calendar, email, chat, project management, research APIs, and the web simultaneously. This breadth is what justifies the tier — the engineer typically has narrower tool access.

### 2.3 Tier 2 — The engineer (Claude Code / agentic CLI)

**Scope:** execution in the repo. Branch-per-task. Deep access to local tooling (language servers, test runners, compilers, project-specific MCPs).

**Doesn't do:** enumerate orchestrator's tool surface (invisible), strategic project management, recurring synthesis across many sessions.

**Primary artifacts:** code commits on `<namespace>/<slug>` branches, wrap-ups in `handoff-prompts/cowork/incoming/`.

**Key behaviors:**
- Reads a brief from `handoff-prompts/cowork/outgoing/`.
- Runs pre-dispatch checks (verify repo state, branch from main).
- Executes on a new branch named `<namespace>/<slug>` (convention: `cowork/` namespace).
- Writes a wrap-up in `handoff-prompts/cowork/incoming/<date>-<slug>-wrap.md`.
- Reports L1/L2 verification results; defers L3 to operator.
- Spawns its own subagents (Tier 4) for parallel sub-tasks within the session.

**Key strength:** deep repo context + compilation feedback loop. The engineer sees what actually works.

---

<a id="extended-compute-tiers"></a>
## 3. Extended compute tiers (Tiers 3–5)

The three principals above are sufficient for many sessions. But as complexity grows, three more tiers become essential.

### 3.1 Tier 3 — Compute-offload CLI tools

**What counts:** GitHub Copilot (in IDE), Aider, Cursor, Gemini CLI, Codex CLI, local LLMs via llama.cpp, or any other agentic coding tool available on a different (or same) billing plan.

**Purpose:** offload *mechanical* labor from the orchestrator/engineer. The goal is to reduce the Claude-specific compute burden (context window, rate limits, usage cost) by routing work that doesn't require Claude's particular strengths (reasoning depth, multi-MCP orchestration) elsewhere.

**Typical work:**
- Git operations (renames, merges, branch cleanup)
- File/directory moves
- Search-and-replace sweeps
- Bulk doc-header updates (e.g., "bump `Last-updated:` on every file matching glob")
- Running verification scripts
- Encoding-fix passes (mojibake, NUL-byte stripping, line-ending normalization)

**Coordination pattern:** the orchestrator writes a prompt in a specific format; the operator pastes it into the offload tool; the offload tool executes; the operator reviews the diff before committing.

**Prompt format ("Run this / Does / Why"):** see Appendix C.

**Why this tier exists:** mechanical work is cheap to describe, expensive to execute in a high-capability agent. A Copilot-tier tool completes a 200-file rename in seconds without burning orchestrator context.

### 3.2 Tier 4 — In-session subagents

**What counts:** subagents spawned via the Task tool (or equivalent) within a single orchestrator or engineer session.

**Purpose:** parallel workstreams. Model-tiered by cost/cognitive load.

**Tiering (Claude-specific; adapt for other ecosystems):**

| Model | Use for | Don't use for |
|---|---|---|
| **Opus** | Complex reasoning, architectural trade-offs, adversarial review, ADR drafting, multi-file refactor planning, "second opinion" reviews | Mechanical work (waste), simple lookups (waste) |
| **Sonnet** | Day-to-day workhorse. Feature implementation inside a defined spec, doc synthesis across ≤10 files, code review in a known scope, moderate refactors | Trade-offs that need deep reasoning (use Opus) |
| **Haiku** | Mechanical / bulk / low-judgement. Renames, find-and-replace sweeps, batch grep surveys, glob enumeration | Anything with judgement calls |

**Parallel dispatch:** if work streams are genuinely independent (no shared state, no ordering dependency), dispatch multiple subagents in a single message. They execute concurrently; you synthesize after all return.

**Prompt rules (Appendix D):**
- Self-contained. Subagents have NO prior conversation context.
- Explicit "done" definition. What proves the work is complete?
- Short-report cap unless the work requires long output. Keeps parent context clean.
- Never delegate understanding. "Based on your findings, fix the bug" is a fail mode — you synthesize, they execute.
- Treat the summary as input, not truth. Verify by reading actual files/diffs.

### 3.3 Tier 5 — Scheduled tasks

**What counts:** autonomous Cowork-style sessions that fire at absolute times and write to a known output location. Registered via a scheduled-task MCP (or OS-level cron wrapping a headless agent).

**Purpose:** recurring research, reporting, maintenance that should run without the operator being present.

**Typical work:**
- Daily deadline-check scan (upcoming calendar + course platform + project tool)
- Weekly research deep-dive (indie game dev, domain of interest, etc.)
- Outbox watcher (reads a queue, processes items, writes results)
- Morning digest (overnight notifications, flagged channels)

**Key constraints:**
- **Absolute-time scheduling only.** No daisy-chaining (one task cannot spawn another mid-run reliably).
- **Stateless between runs.** Each invocation gets a fresh prompt; continuity is via the output dir.
- **Output-dir rendezvous.** If task A must inform task B, task A writes to a known dir and task B reads it. Use staggered absolute times, not dependencies.

**Prompt rules (Appendix E):**
- Fully self-contained. No prior context assumed.
- Explicit output path. The task must know exactly where to write.
- Graceful degradation. If expected inputs are missing, say so and do what's possible — don't block.
- Never-delete rule always applies.

---

<a id="handoff-prompts"></a>
## 4. Handoff-prompts — the coordination layer

This is the heart of Triforce. Every agent tier coordinates through this one directory tree.

### 4.1 Directory structure

```
docs/handoff-prompts/
├── README.md                   # minimal overview + link to this TRIFORCE.md
├── templates/                  # reusable prompt skeletons
│   ├── brief.md                # orchestrator → engineer (Appendix A)
│   ├── wrap-up.md              # engineer → orchestrator (Appendix B)
│   ├── offload-prompt.md       # orchestrator → compute-offload CLI (Appendix C)
│   ├── subagent-prompt.md      # parent → subagent (Appendix D)
│   ├── scheduled-task.md       # registration prompt body (Appendix E)
│   ├── dispatch-menu.md        # orchestrator → operator (Appendix F)
│   ├── continuity.md           # pre-compaction handoff (Appendix G)
│   └── post-mortem.md          # end-of-sprint retro scaffolding
├── cowork/                     # orchestrator ↔ engineer channel
│   ├── outgoing/               # briefs authored by orchestrator for engineer
│   ├── incoming/               # wrap-ups authored by engineer for orchestrator + operator
│   └── archive/                # superseded items (NEVER deleted)
├── offload/                    # orchestrator → compute-offload CLI (optional)
│   ├── outgoing/               # offload prompts (for operator to paste into the CLI)
│   └── incoming/               # operator-pasted-back results / diffs (optional)
├── scheduled/                  # orchestrator ↔ scheduled-task outputs
│   ├── inbox/                  # scheduled tasks deposit here
│   └── outbox/                 # operator's followup prompts / queries for next run
├── operator/                   # agent → operator channel (operator-facing only)
│   ├── current/                # READ-FIRST dispatch menus, audits, status docs
│   └── archive/                # superseded
└── post-mortems/               # sprint-end retros
```

**Naming convention for channel subdirectories:** pick ONE meaningful name for the operator (e.g., `operator/`, or the operator's first name). Don't use `user/` — ambiguous with generic "user" references in CLAUDE.md and other docs. This has been a real source of drift.

### 4.2 Naming convention for files

```
YYYY-MM-DD-<slug>[-<role-tag>].md
```

- **Absolute date, not relative.** "today" decays; "2026-04-22" doesn't.
- **Slug is kebab-case, descriptive.** `mcp-audit` not `audit` (too generic) or `audit-of-all-the-mcps-we-have` (too long).
- **Role tag (optional suffix):**
  - `-cc` — fork doc intended for a Claude Code / engineer session.
  - `-cowork` — Cowork-internal (orchestrator doing the work itself).
  - `-wrap` — wrap-up doc after work lands.
  - `-audit` — an audit doc.
  - `-dispatch` — operator-facing dispatch menu.
  - `-continuity` — pre-compaction handoff.

Example lifecycle of one task:
```
cowork/outgoing/2026-04-22-mcp-audit-cc.md          # brief (orchestrator authors)
cowork/incoming/2026-04-22-mcp-audit-wrap.md        # wrap (engineer authors)
operator/current/2026-04-22-mcp-audit-dispatch.md   # menu entry (optional, orchestrator)
cowork/archive/2026-04-22-mcp-audit-cc.md           # archived (after merge)
cowork/archive/2026-04-22-mcp-audit-wrap.md         # archived (after review)
```

### 4.3 Document types (and which tier writes them)

| Type | Writer | Reader | Lives in |
|---|---|---|---|
| Brief (fork doc) | Orchestrator (Tier 1) | Engineer (Tier 2) | `cowork/outgoing/` |
| Wrap-up | Engineer (Tier 2) | Orchestrator + Operator | `cowork/incoming/` |
| Dispatch menu | Orchestrator (Tier 1) | Operator (Tier 0) | `operator/current/` |
| Audit | Any tier; usually Orchestrator | Operator + future agents | `operator/current/` or `cowork/incoming/` |
| Offload prompt | Orchestrator (Tier 1) | Operator pastes into Tier 3 CLI | `offload/outgoing/` |
| Subagent brief | Parent agent (Tier 1 or 2) | Subagent (Tier 4) | Usually inline in prompt, not disk |
| Scheduled-task prompt | Orchestrator (Tier 1) | Scheduled-task MCP | Registered with the MCP; backup copy in `scheduled/outbox/` |
| Scheduled-task output | Scheduled task (Tier 5) | Orchestrator + Operator | `scheduled/inbox/` |
| Continuity doc | Orchestrator (Tier 1) pre-compaction | Fresh orchestrator session | `cowork/outgoing/` (next-session-directed) |
| Post-mortem | Orchestrator + Operator jointly | Everyone | `post-mortems/` |

### 4.4 Lifecycle

1. **Operator intent** → operator expresses a desire in chat with orchestrator ("I want to add X", "audit our Y").
2. **Orchestrator authors brief** → writes to `cowork/outgoing/YYYY-MM-DD-<slug>-cc.md`. Brief is self-contained; see Appendix A.
3. **Operator dispatches** → opens fresh engineer session, hands it the brief path (or contents).
4. **Engineer executes** → branches `cowork/<slug>`, does work, spawns subagents if useful.
5. **Engineer wraps up** → writes `cowork/incoming/YYYY-MM-DD-<slug>-wrap.md`, pushes branch (does NOT merge unless explicitly authorized).
6. **Operator reviews** → merges the branch, sends back for revision, or parks.
7. **Orchestrator synthesizes** → reads the wrap-up, updates project state docs (PROGRESS, TODOS index, dispatch menu), surfaces drift/followups.
8. **Archive** → at sprint-end or after N days, move completed items to `archive/` subdirs. **Never delete.**

### 4.5 Channels (who writes where)

The directory structure maps to **channels**, each connecting a specific pair of tiers:

| Channel | From | To | Examples |
|---|---|---|---|
| `cowork/outgoing/` | Orchestrator | Engineer | Fork docs, audit briefs |
| `cowork/incoming/` | Engineer | Orchestrator + Operator | Wrap-ups, engineer-authored audits |
| `offload/outgoing/` | Orchestrator | Operator (who pastes into Tier 3 CLI) | Copilot prompts, Aider tasks |
| `scheduled/outbox/` | Operator | Scheduled-task (Tier 5) | Followup prompts for next run |
| `scheduled/inbox/` | Scheduled-task (Tier 5) | Orchestrator + Operator | Research dumps, daily briefs |
| `operator/current/` | Orchestrator | Operator | Dispatch menus, status docs |

**The operator reads every channel.** The orchestrator reads every channel. The engineer reads `cowork/outgoing/` and writes `cowork/incoming/`. Subagents read their parent's prompt only.

---

<a id="compute-economy"></a>
## 5. Compute economy — routing decisions

The routing question: *which tier should do this piece of work?*

### 5.1 The decision matrix

| Work characteristic | Route to |
|---|---|
| Involves deciding / deep reasoning | Orchestrator (Tier 1), possibly with Opus subagent |
| Involves repo-local code / tests / builds | Engineer (Tier 2) |
| Mechanical, well-specified, repetitive | Compute-offload CLI (Tier 3) |
| Independent parallel tasks within a session | Subagents (Tier 4), dispatched in a single message |
| Recurring on a schedule without operator present | Scheduled task (Tier 5) |
| Requires human judgement / veto | Operator (Tier 0) — with an operator-facing doc to support the decision |

### 5.2 Rules of thumb

- **Understanding vs. executing.** If the work is *understanding* and *deciding*, do it yourself (orchestrator). If the work is *executing a defined plan*, route down the tiers.
- **Never delegate understanding.** Prompts to lower tiers must specify the work concretely. "Based on your findings, fix the bug" is a fail mode at every tier.
- **Pure I/O** (read, list, grep) → Haiku subagent or Tier 3 offload.
- **Pattern-matching / code in a known spec** → Sonnet subagent or Tier 2 engineer.
- **Trade-offs / architecture** → Opus subagent or Tier 1 orchestrator.
- **Mechanical git** → Tier 3 offload CLI (fastest + cheapest).
- **If you'll quote from a subagent's response** → ask for a word cap.
- **If you'll scan for a fact** → ask for structured format (table/JSON/bullets).

### 5.3 Anti-patterns

- **Using the orchestrator for mechanical git sweeps.** Burns the long-thread context. Route to Tier 3.
- **Using the engineer for strategic project management.** Engineer sessions compact fast; strategic context belongs in the orchestrator.
- **Ignoring Tier 3 because it's "not as smart."** Mechanical work doesn't need smart. It needs fast and cheap.
- **Daisy-chaining scheduled tasks.** Doesn't work reliably. Use staggered absolute times + directory rendezvous.
- **Spawning subagents serially when they could run in parallel.** Waste of wall-clock time.
- **Live-testing MCPs during audits.** Creates noise. Enumerate from config + session context.

---

<a id="verification-gates"></a>
## 6. Verification gates

Every deliverable has a gate. Gates prevent "done in theory" slipping into main.

### 6.1 The three levels

- **L1 — automated.** The work compiles / lints / passes a smoke-test script. Can be checked by any agent with shell access.
- **L2 — reproducible.** The work runs end-to-end in a sanity harness. A playtest scene for a game, a `curl` round-trip for an API, a seeded replay for a simulation.
- **L3 — human review.** The operator eyeballs the result. Required for UX work, subjective-quality docs, or anything where "looks right" is part of the spec.

### 6.2 When each applies

| Work type | L1 | L2 | L3 |
|---|---|---|---|
| Code change | Required | Required if behavior-visible | Required if UX |
| Pure docs | N/A | N/A | Required |
| Audit | Data-gather validation | N/A | Required |
| Refactor | Required | Required | Optional |
| Infrastructure / config | Required | Required | Required if production |
| Research synthesis | N/A | N/A | Required |

### 6.3 How to invoke the gate

In every brief, under `## Verification Gate`:

```
- L1: <what the automated check is, or "N/A">
- L2: <what the reproducible check is, or "N/A">
- L3: <what the operator reviews, or "N/A">
```

In every wrap-up, under `## Verification Gate`:

```
- L1: passed / failed / N/A [with evidence]
- L2: passed / failed / N/A [with evidence]
- L3: deferred to operator
```

---

<a id="setup"></a>
## 7. Setting up Triforce in a new repo

### 7.1 Day-zero checklist

1. **Create `docs/handoff-prompts/`** with the directory structure in § 4.1.
2. **Copy this `TRIFORCE.md`** to `docs/triforce/TRIFORCE.md`.
3. **Create `docs/handoff-prompts/README.md`** with a minimal overview + pointer to `TRIFORCE.md`.
4. **Copy templates** from Appendices A–G into `docs/handoff-prompts/templates/`.
5. **Author `CLAUDE.md`** at repo root with:
   - A one-paragraph project description.
   - A "Triforce setup" section that points to `TRIFORCE.md`.
   - Load-order guidance for context (Tier 1/2/3 docs).
   - The operator's personal non-negotiables (e.g., "never delete X", style preferences).
6. **Commit the scaffold.** First commit is "chore: scaffold Triforce workflow".

### 7.2 First smoke test

- Operator asks orchestrator: "Author a trivial brief so we can verify the engineer channel works."
- Orchestrator writes `cowork/outgoing/<today>-smoke-test-cc.md` — brief is something small like "add a one-line README comment".
- Operator dispatches a fresh engineer session on the brief.
- Engineer executes, wraps up in `cowork/incoming/<today>-smoke-test-wrap.md`.
- Operator merges. Sprint begins.

### 7.3 First real dispatch menu

After the smoke test, the operator + orchestrator together draft the first real dispatch menu in `operator/current/<today>-dispatch-menu-v1.md` listing the initial N tasks they want done. This becomes the rolling status doc the operator refers to.

### 7.4 Ongoing docs to maintain

- `docs/PROGRESS.md` — current milestone + sprint state.
- `docs/ARCHITECTURAL_DECISIONS.md` — locked decisions. Don't re-debate.
- `docs/TODOS/TODOS_INDEX.md` — glanceable TODO index (counts match body).
- `docs/PLANNING_DOCS_HEALTH.md` — doc-freshness registry.
- `docs/MCP_AUDIT.md` (optional) — connector inventory if you have several MCPs.
- `docs/SKILLS_REFERENCE.md` (optional) — skill-triage map if you use multiple skill suites.

---

<a id="operational-patterns"></a>
## 8. Operational patterns

### 8.1 Handling context compaction

Long orchestrator threads hit compaction eventually. Compaction summarizes aggressively; detail is lost.

**Pattern:** the orchestrator emits a **continuity doc** before it senses compaction (or proactively, at natural stage boundaries). Written to `cowork/outgoing/<date>-<slug>-continuity.md` or `operator/current/<date>-handoff-continuity.md`.

**Continuity doc contents:** see Appendix G. Summary:
- Operational state (what's landed, what's in flight, what's parked).
- Recent wraps to read, in priority order.
- Open questions for the operator.
- Specific assignments for the next session.
- Protocol reminders (never-delete, verification gates, etc.).

**Operator feeds the continuity doc into a fresh orchestrator session.** The new orchestrator reads the doc + canonical repo files (CLAUDE.md, PROGRESS, etc.) and picks up where the prior thread left off.

**Project spaces / named threads** (Cowork's Projects feature, equivalent in other tools) make this smoother — the directory mount is persistent, and the continuity doc is the only session-level state to transfer.

### 8.2 Sandbox / git corruption guardrails

Sandboxed filesystems sometimes return corrupted git state — truncated HEAD refs, unbreakable `index.lock`, ghost "staged delete" claims on files that are actually clean.

**Mitigation:**
- **Audit-before-execute.** Before authoring a fork doc based on git state, run `git status`, `git log --oneline -10`, `git diff --stat HEAD~5..HEAD` and include the raw output in the doc.
- **Verify with a second agent.** If a finding is surprising, route it to a Tier 3 CLI for independent check before acting.
- **Log incidents.** Keep `docs/<your-ai>-troubleshooting/README.md` with observed symptoms + confirmed fallout + guardrails.

### 8.3 MCP connector drift

The orchestrator's available MCPs can change silently between sessions — a connector lapses, a new one gets added, a plugin registers different tools.

**Mitigation:**
- Run a periodic MCP audit (`docs/MCP_AUDIT.md`). Enumerate from the session's deferred-tool list + config files, don't live-test.
- Flag drift explicitly in wrap-ups: "CLAUDE.md claims X; not visible this session."
- Trust the session's deferred-tool list for the current session, not claims in CLAUDE.md.

### 8.4 Operator preference injection

Operators have personal non-negotiables (language rules, delete policies, aesthetic preferences, monitored environments, accessibility needs). These live in the global CLAUDE.md (user-level) so every session inherits them.

**Discipline:** when a pattern recurs ("every time I'm about to commit, I want Claude to ..."), promote it from scratchpad notes into CLAUDE.md. The framework works because the rules are encoded, not remembered.

### 8.5 Decide, don't question

When an agent has enough context to decide, it should decide — not ask the operator to coin-flip. This is especially a risk with orchestrators: they're in long threads, they know the context, they can decide.

**Anti-pattern:** "Do you want me to use option A or option B?" when the orchestrator already has the information to pick.

**Pattern:** "Picking option A because [reason]. Let me know if you want B instead." Make the call; make it reversible.

Questions should appear in handoffs only when the orchestrator genuinely lacks information the operator alone has (preferences, intent, context the orchestrator can't see).

### 8.6 Pre-dispatch checks

Every brief includes a **pre-dispatch checklist** that the engineer runs BEFORE starting work. Catches drift between brief-authoring time and dispatch time.

Example checks:
- `git fetch origin && git log origin/main..HEAD` — am I actually up to date?
- Does the file the brief references still exist at the expected path?
- Has another branch already started this work?
- Is the state assumed by the brief actually present?

If checks fail: the engineer reports back immediately in chat (not a wrap-up), operator decides whether to amend the brief or proceed with adjustments.

### 8.7 Never-delete meta-rule

Many operators will have "never delete X" rules (emails, calendar events, messages, etc.). The framework itself should respect this universally:

- Never delete files from handoff channels. Move to `archive/`.
- Never delete branches without explicit operator approval.
- Never call `delete_*` MCP tools without explicit approval.
- If a tool surface EXPOSES deletion, document it but gate it.

This is stricter than "don't delete what the operator cares about" — the rule is "don't delete by default, ever," because the operator cares about provenance more than storage economy.

---

<a id="appendix-a"></a>
## Appendix A — Brief (CC fork doc) template

Save as `docs/handoff-prompts/templates/brief.md`.

```markdown
# <Title> — <One-sentence pitch>

**From:** Orchestrator (Cowork)
**To:** Fresh engineer session (Claude Code)
**Priority:** TIER 1 / 2 / 3 — <one-line rationale>
**Branch:** `cowork/<slug>` (fresh from main)

---

## Operator's Instructions (where to mount, what to bring)

**Mount at:** <absolute path to repo>
**Pre-dispatch checklist:**
- [ ] `git fetch origin && git log origin/main..HEAD` — branch not accidentally diverged
- [ ] <any state assumptions the brief makes>
- [ ] <any tooling / service that must be running>

**What the operator wants in chat when done:** <one sentence — e.g., "done + link to doc, no summary">

---

## Goal (one sentence)

## Why

<2-4 sentences of context. What problem does this solve? Why now?>

## Research pre-reqs (files the engineer should read before starting)

1. <path> — <why>
2. <path> — <why>

## Scope / structure

<Bulleted scope. For each item: what to do, what NOT to do, where to put output.>

## What NOT to do

- Never delete anything. Rename / archive instead.
- <task-specific negative-space constraints>
- Do NOT invoke MCPs outside scope just to probe them.

## Verification Gate

- L1: <automated check, or "N/A">
- L2: <reproducible check, or "N/A">
- L3: <operator review scope>

## Wrap-up deliverable

Write to `docs/handoff-prompts/cowork/incoming/<date>-<slug>-wrap.md`:
- Paths / artifacts created.
- Counts or summary stats.
- Any decisions made inside the brief's scope.
- Any drift / surprises found.
- Open questions for the operator.
- L1 / L2 results.

## Suggested commit

```
<type>(<scope>): <subject>

<2-3 line body explaining why>
```
```

---

<a id="appendix-b"></a>
## Appendix B — Wrap-up template

Save as `docs/handoff-prompts/templates/wrap-up.md`.

```markdown
# Wrap-up: <Title>

**From:** Engineer session (Claude Code, <date>)
**To:** Orchestrator + Operator
**Branch:** `cowork/<slug>` (<N> commits, <pushed / not pushed>)
**Fork doc executed:** `docs/handoff-prompts/cowork/outgoing/<date>-<slug>-cc.md`

---

## Status: done / partial / blocked.

## Files changed per commit

1. `<sha>` — **<commit subject>**
   - <path> (new / modified / renamed, <+/-lines>)

2. <as above>

## Verification answers

- **L1:** <passed / failed / N/A> — <evidence>
- **L2:** <passed / failed / N/A> — <evidence>
- **L3:** deferred to operator

## Deviations from fork doc

- <any places the engineer deviated from spec, with reason>

## Drift / surprises

- <anything the engineer noticed that contradicts CLAUDE.md, ARCH, or a prior doc>

## Skipped / deferred

- <anything the brief asked for that the engineer did NOT complete, with reason>

## Open questions for operator

1. <question>
2. <question>

## Next

- <what the engineer recommends next, if anything>
```

---

<a id="appendix-c"></a>
## Appendix C — Compute-offload CLI prompt template

Used when routing mechanical work to GitHub Copilot, Aider, Cursor, Gemini CLI, Codex, etc.

Save as `docs/handoff-prompts/templates/offload-prompt.md`.

```markdown
# Offload Task: <title>

**For:** <tool name, e.g., GitHub Copilot>
**Context:** <one paragraph of what the operator is doing + why this is offloaded>

---

## Run this

```<language>
<the exact command / diff / script>
```

## Does

<in plain English, what the above command does>

## Why

<why this is the right mechanical operation — not "because the orchestrator said so", but the actual rationale>

## Pre-conditions

- [ ] <state the tool can verify before running>
- [ ] <another verifiable condition>

## Post-conditions

- [ ] <what the operator verifies after the tool runs>
- [ ] <another>

## If it fails

<one-line fallback: "report back; don't retry with a different approach">
```

**Why this format:** the operator pastes this into the CLI. The CLI reads the `Run this` block as the action, the `Does` block as a sanity gate, the `Why` block as the rationale (often unnecessary but useful for the operator). The tool can verify pre-conditions before running and surface post-condition status after.

---

<a id="appendix-d"></a>
## Appendix D — Subagent prompt guidelines

When a parent (orchestrator or engineer) spawns a subagent via the Task tool, the prompt must be self-contained because the subagent has NO prior context.

### Template

```
<1-paragraph context: what the parent is trying to accomplish, and why this subagent exists>

<1-paragraph task: exactly what the subagent should do>

<explicit "done" definition: what proves the work is complete>

<output format: word cap, structured bullets, table, JSON, etc.>

<anti-scope: what NOT to do>
```

### Rules

1. **Self-contained.** No "based on our earlier discussion". The subagent sees only the prompt.
2. **Explicit done.** "Report under 200 words with a table" is better than "give me an overview".
3. **Word cap when you'll quote.** "Under 300 words" keeps parent context clean.
4. **Structured output when you'll scan.** Tables/JSON/bullets > prose.
5. **Never delegate understanding.** The parent synthesizes; the subagent executes.
6. **Verify by reading files.** The subagent's summary describes intent, not outcome.

### Model routing (Claude)

| Work | Model |
|---|---|
| Architecture / trade-offs / adversarial review | Opus |
| Feature implementation in defined spec / doc synthesis / moderate refactor | Sonnet (default workhorse) |
| Renames / grep sweeps / glob surveys / ballpark triage | Haiku |

### Parallel dispatch

For independent workstreams: spawn multiple subagents in a SINGLE message. They run concurrently.

Don't parallelize when there's shared state, ordering dependencies, or when the tasks will be cross-referenced mid-work.

---

<a id="appendix-e"></a>
## Appendix E — Scheduled-task prompt guidelines

Scheduled tasks are autonomous Cowork sessions firing at absolute times. The prompt body is stateless — no session-level memory, no continuation.

### Template

```
# Scheduled Task: <name>

## Fires at: <cron or absolute time>
## Output target: <absolute file path — exact>
## Input sources: <files the task reads, or "none">

## Persona

<who is the task "acting as" — a domain expert, a researcher, a brief writer?>

## Scope

<what the task does in 3-6 bullets>

## Output format

<structured requirements — word count, sections, headers, tone>

## Graceful degradation

<what to do if an expected input is missing — don't block, do what's possible, note gaps>

## Never-delete rule applies. No credentials in output. Cite sources if applicable.
```

### Rules

1. **Absolute times only.** No "every Tuesday at noon unless..."
2. **No daisy-chain.** If task B needs task A's output, stagger them by a safe margin and use a shared dir.
3. **Graceful degradation.** Missing inputs is the common failure mode; plan for it.
4. **Idempotent by design.** If the task fires twice (operator re-triggers), the output should be well-defined.
5. **Self-contained.** Every scheduled-task prompt reads like day-zero onboarding.

### Scheduling patterns

- **Staggered same-night:** three 7pm tasks + one 9:30pm synthesis reading all three. Synthesis has a 2.5h buffer for the 7pm tasks to complete + graceful degradation if any fails.
- **Morning digest:** 7am scan of overnight state, deposit in `scheduled/inbox/`.
- **Outbox watcher:** daily check of a queue directory; process any items; write results back; mark processed.

---

<a id="appendix-f"></a>
## Appendix F — Operator-facing dispatch menu template

A dispatch menu is the operator's status dashboard. Lives in `operator/current/<date>-dispatch-menu-vN.md`.

```markdown
# Dispatch Menu vN — READ FIRST

**From:** Orchestrator
**To:** Operator
**Date:** <YYYY-MM-DD>
**Status:** <draft / ready-to-dispatch / in-flight / parking>

---

## What changed since vN-1

<1-3 bullets — if v1, skip this section>

---

## TIER 1 — Unblocks active work

### 1. <Title>
- **Brief:** `cowork/outgoing/<date>-<slug>-cc.md`
- **What:** <one sentence>
- **Why first:** <rationale>
- **Parallel-safe with:** <other tier-1 items it doesn't conflict with>
- **Audit verdict:** <GREEN / minor-patch / must-rewrite>
- **Status:** [ ] / [DONE] / [IN FLIGHT] / [SKIPPED]

### 2. <Title>
<as above>

---

## TIER 2 — Workflow infrastructure

<as above>

---

## TIER 3 — Next-sprint prep

<as above>

---

## Scheduled tasks live

| Task ID | Schedule | Output dir |
|---|---|---|
| <id> | <cron> | <path> |

---

## Recommended dispatch order

1. <which to fire first and why>
2. <which next>
3. <parallel pairs>
4. <park-until-x items>

---

## Housekeeping

- <stale items cleanup>
- <doc-drift items to patch>
- <superseded items to archive>
```

The operator annotates this file directly — `[DONE]`, `[SKIPPED]`, `[MIKE: ...]`, etc. — as work progresses. When the doc gets messy, the orchestrator issues a v(N+1).

---

<a id="appendix-g"></a>
## Appendix G — Session-continuity doc (pre-compaction handoff)

Written by an orchestrator just before its thread compacts (or proactively at natural stage boundaries). Fed to a fresh orchestrator session to bootstrap state.

**Target location:** `cowork/outgoing/<date>-<slug>-continuity.md` or `operator/current/<date>-handoff.md`.

### Contents (in order)

1. **Orientation block** — identity, role, who the operator is (compressed), what this doc is.
2. **Repo state** — directory layout, gotchas, key file paths.
3. **Workflow state** — Triforce setup, handoff channels, current sub-sprint.
4. **Operational state (THIS IS THE VALUE)** — what's landed recently, what's in flight with which engineer, what's parked for the fresh session.
5. **Recent wraps to read** — ordered by priority.
6. **Open questions for the operator** — things the prior thread couldn't decide.
7. **Assignments for the fresh session** — specific tasks with scope.
8. **Protocol rules** — never-delete, verification gates, monitoring constraints.
9. **First actions** — load order + template for first message to operator.
10. **Appendices** — file-path cheatsheet, connector state, recent quirks/drift.

### Tone

- Dense. This is bootstrapping a new context, not conversational.
- Decisive about state. "Branch X is at commit Y as of T" not "I think branch X might be...".
- Candid about uncertainty. Flag what you don't know.
- Operator-aware. Repeat the operator's communication preferences.

### What NOT to include

- Summary of the prior thread's conversation. The fresh session doesn't need meta; it needs state.
- Apologies or acknowledgements of compaction. Wastes tokens.
- Generic advice. Point at the canonical docs instead.

---

## Version history

| Version | Date | Changes |
|---|---|---|
| 1.0 | 2026-04-22 | Initial codification. Six-tier model. Handoff-prompts mechanism. Seven appendices with templates. |

---

## License / reuse

This framework is offered as a working pattern, not a product. Fork it, adapt it, discard parts you don't need. The only hard dependency is the filesystem-as-coordination-layer idea; everything else can be tuned to your tools and operator preferences.
