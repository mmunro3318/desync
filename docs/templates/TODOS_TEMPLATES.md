# TODOS Templates and *Iron* Rules

## Summary

During development, TODOs are recorded at the **lowest fidelity tier that preserves signal**, and promoted to higher tiers only at natural stage boundaries. Never burn context on full TODO entries mid-flow. Never let signal die because the format felt too heavy.

**Three tiers of TODO fidelity:**

| Tier | Name | When | Format | Where |
|---|---|---|---|---|
| **T1** | Stub | Mid-flow (coding, planning, brainstorm) | 1-line, `~TYPE \| title \| why \| date` | temp memory file |
| **T2** | Draft | Scope locked / sprint boundary | Partial template, `TD????` placeholder | temp file → `TODOS.md` |
| **T3** | Full | Document upkeep stage | Complete template with `TD####` | `TODOS.md` |

The core rule: **write at the lowest tier that preserves the signal**. Promote upward only at natural stage boundaries (scope lock, sprint end, `/ship`, `/retro`). Never promote mid-flow.

---

## Workflows

### Brainstorming, Planning, Design Docs
*Triggered by: `/brainstorm`, `/office-hours`, `/plan-{eng|ceo|design}-review`*

**Rule: Do NOT write to `TODOS.md` during this stage.**

If decisions are being made fast in order to scope out the features for a Sprint or Sub-sprint, we may be deferring certain features to later sprints, deferring tech stack/dependency implementation until later to push out a POC, etc.. As these decisions are made, we *do not* want to waste tokens/context on side-quests researching every rabbit trail to keep `TODOS.md` updated. *But keeping `TODOS.md` updated is critical to team success — we've floundered here in the past.*

Brainstorming and ideation is the info dump session where we know we *won't* keep everything. We slap together the uber product/feature, then *cull scope ruthlessly*. Do **not** write directly to `TODOS.md` related to new ideated features until the brainstorming session is over and you've **locked in scope**.

**Workflow:**

1. Open (or append to) `{YYYY-MM-DD}-{plan|design|implement}-temp-memory.md` (wherever your skill typically stores them — e.g. `~/.gstack-dev/plans/`, `docs/superpowers/plans/`)
2. As items are culled or deferred, immediately drop a **T1 stub** — one line, keep moving
3. When scope is **locked**, write a marker line in the temp file:
    ```md
    ## [SCOPE LOCKED: {YYYY-MM-DD}]
    ```
    Everything above is pre-lock noise. Everything below is promotable.
4. Scan survivor stubs, promote to T2 in the same temp file (still no ID)
5. Items that didn't survive: leave as stubs or delete — never promote to `TODOS.md`

### In medias res
*Triggered by: `/writing-plans`, `/subagent-driven-development`, `/implement`, `/qa`*

**Rule: Never stop mid-flow to write full TODOs. Stubs only.**

When coding on multiple features at once, you would burn through all your context window updating every doc. Don't.

**Workflow:**

1. Keep your current temp memory file open as a running list. If none is open, create `{YYYY-MM-DD}-implement-temp-memory.md`
2. On discovery of a bug / tech debt item / deferred feature mid-code: drop a one-line **T1 stub**, keep moving
3. Do **not** touch `TODOS.md` directly during active implementation

**Context compaction / usage-cap warning protocol:**

If you detect approaching context limits, receive a compaction warning, or approach a usage cap:

1. Immediately flush all T1 stubs to T2 (partial expansion) in the temp file
2. Write a brief handoff note: what you were doing, what's unfinished, list of stub titles
3. Stop cleanly — do not try to complete work first

**Remember to clean up your temp docs (if they've been ingested), or leave instructions in your design docs / handoff-prompt docs for later doc-update stages.**

### Sprint Boundary / Scope Lock
*Triggered by: end of planning session, end of `/brainstorm`, start of `/ship`*

**Rule: Promote T1 → T2. Batch-write drafts to `TODOS.md`. No IDs yet.**

**Workflow:**

1. Collect all T1 stubs from temp memory file(s)
2. For each stub, expand to T2 format (see below):
    - Fill: `What`, `Why`, `Priority`, `Added`
    - Leave as `{TBD}`: `How`, `Effort`, `Verification`, `Regression risk`
3. Append all T2 entries to `TODOS.md` using `TD????` as ID placeholder
4. In your handoff prompt, note: `{N} TODO drafts need ID assignment and full expansion`
5. Do **not** run `generate_next_todo_id.js` yet — that happens at document upkeep

### Upon (Sub)Sprint Completion
*Triggered by: `/document-release`, `/ship`*

**Rule: Promote T2 → T3. Assign all IDs. Clean up temp files.**

**Workflow:**

1. Find all `TD????` entries in `TODOS.md`
2. Read `**LAST_USED_ID:**` at the top of `TODOS.md`
3. Run `node ./docs/TODOS/generate_next_todo_id.js <LAST_USED_ID> <count>` to get the next batch of IDs
4. Replace each `TD????` with its assigned `TD####`, in order of `Added` date
5. Expand all `{TBD}` fields now that sprint context is fresh
6. Update `**LAST_USED_ID:**` at the top of `TODOS.md` with the highest ID just assigned
7. Archive or delete temp memory files that have been fully ingested
8. Leave a note in any temp file that has NOT been fully ingested

### Post-Mortem
*Triggered by: `/review`, `/retro`*

**Rule: Audit only. No new stubs here — surface gaps.**

**Workflow:**

1. Scan both `TODOS.md` and `ARCHIVED_TODOS.md` for:
    - Remaining `TD????` placeholders (promotion was missed)
    - Remaining `~TYPE` stubs that were never promoted (signal loss risk)
    - Closed TODOs missing resolution notes (Outcome / Root cause / Fix / Verification)
2. For each gap: either promote immediately or flag for next document upkeep session
3. If new items are discovered during retro: write directly as T2 (not T1 — retro is a low-velocity stage with context to spare)
4. Sprint post-mortem audit: move **completed** TODO items from `TODOS.md` to `ARCHIVED_TODOS.md` using the appropriate completion template. If a TODO item was completed, atomized, or spawned child/compounding TODO items, reassess (and if necessary recalculate) Priority Scores based on the retro/sprint review.

---

## Priority Scoring

### Calculate Priority Scores Based On:

**First Pass:** A first pass priority score (you're quickly adding TODO items as they arise during design or implementation, without time/context to thoroughly analyze) should have `P[~N]` (ie, `P[~2]`), until a more thorough analysis or calculation can take place (CC personal estimates, not exact). **Any** doc review (`/document-release`, `/office-hours`, `/review`, etc.) that encounters a priority value of the `~` format `P[~N]` (or `P[?]` = was never calculated) **must**:

1. **pause**
2. **dispatch** a Sonnet subagent (for estimates `P[~2-4]`) or an Opus subagent (for estimates `P[~0-1]`)
    - For `P[?]` values, dispatch **Haiku** to provide a bare-bones estimation in `P[~N]` format, then feed TODO and priority score into a second, appropriate Sonnet/Opus subagent
3. **document** the score in `TODOS.md`, and consolidate info from the research report into the TODO item, making sure to note the path to the research report in the **Added:** field

*Approximate best you can — do not attempt high precision numbers, only informed estimates based on CC timelines.*

**Formula:** Priority score = (Impact + Urgency + Dependency Weight + Risk) - Effort

**Where:**
- **Impact:** How bad if we do nothing?
- **Urgency:** Time sensitivity, deadline pressure, decay of value.
- **Dependency Weight:** How many other systems / TODOs / planned features depend on this, or are blocked until it lands? (Replaces the old "Reach" metric — in a solo-dev game project, "how many users" is always 1; what matters is how much downstream work is gated by this item.)
- **Risk:** Security, reliability, compliance, or operational risk reduced by doing it. For a game project, this includes architecture lock-in risk and multiplayer/networking correctness.
- **Effort:** Relative implementation cost/complexity.

### Suggested mapping:

- 15+ → P0
- 12-14 → P1
- 9-11 → P2
- 6-8 → P3
- 5 or less → P4

### Best-practice guardrails

- **Cap P0s hard**: Keep P0 rare, reserved for true emergencies.
- **Separate severity from priority**: A severe bug can still be P2 if it affects very few systems with a good workaround.
- **Re-score at sprint boundaries**: Priority should move with context.
- **Record rationale**: Always include one sentence why a task got its P level.

### Priority Override Badges

To distinguish high-priority (but not high-severity) items that both you and Mike decide on locking in for next sprint / immediate work, add an `O[n]` badge for priority override:

- `O[0]` = this sprint, before wrap-up
- `O[n+1]` = next sprint
- `O[M]` = this milestone, before wrap-up
- `O[MVP]` = before MVP ship
- `O[D]` = deferred / iced indefinitely or until after MVP and other features

---

## TODO ID System

### Generating consistent, unique TODO ids

- *!* `[{TODO_ID}]`: A TODO ID needs to be generated for *each* new T3 TODO item. The last used ID is stored at the top of `TODOS.md`: `**LAST_USED_ID:** TD0032`
    - To generate the next new id, call in terminal:
        - Single ID generation: `node ./docs/TODOS/generate_next_todo_id.js TD0032` → `TD0033`
        - Batch ID generation: `node ./docs/TODOS/generate_next_todo_id.js TD0032 10` → `["TD0033","TD0034","TD0035","TD0036","TD0037","TD0038","TD0039","TD0040","TD0041","TD0042"]`
        - Function signature: `function* generateNextTodoId(resumeFrom = null, count = 1, width = 4)`
    - *!* **Note on first init:** If you see an uninitiated `**LAST_USED_ID:** [NOT INITIATED YET]`, you will first need to run the script with `null` (or no arg) to pull `TD0001`, store it in the **LAST_USED_ID:** field, replacing `[NOT INITIATED YET]` with the first id value. Then call it every time you need a new `TODO_ID`.

**IDs are only assigned at T3 (document upkeep stage).** Do not assign real IDs at T1 or T2.

---

## TODO Item Naming Conventions and Types

- *!* `[TODO] = [GENERAL | FEATURE | REFACTOR | TESTING | TECH_DEBT | KNOWN_BUG | IMPROVEMENT | BUG]`
    - `[FEATURE]` – net-new user-visible capability
    - `[IMPROVEMENT]` – enhance an existing feature (perf, UX, robustness)
    - `[BUG]` – errors/warnings, defect, visibly incorrect behavior
    - `[KNOWN_BUG]` – acknowledged bug you're intentionally not fixing yet
    - `[REFACTOR]` – non-behavioral code restructuring
    - `[TESTING]` – test authoring, coverage, harness improvements
    - `[TECH_DEBT]` – cleanup / structural work deferred earlier (non-feature, non-bug)
    - `[GENERAL]` – meta work (docs, infra, coordination) that doesn't fit others
    - *!* Note: `Title [{TYPE}]` only gets *one* primary `TYPE`. Secondary type tags are allowed in the body: (e.g., `**Types:** [{PRIMARY_TYPE}, BUG, TECH_DEBT]`), where the *primary* type is always first (for easy scanning/regex).
    - *!* **Tags:** An additional `Tags` field is present for arbitrary classification of an issue with tags to aid organization, prioritization, scope analysis — and later, self-evolved learning and repo/dev mapping. **MAX = 10**.
- *!* Naming Convention: `## [{TODO_ID}] M{milestone}: [{TYPE}] {Short clear title} [PARTIAL Y%]`
    - `M{N}` Examples: `M0`, `M1`, `M1.2`, `M2-a`; sprints or tracks: `M1S2` or `M1.API`
    - `{Short clear title}` Example: `{"Verb or affected component" + ":" + " behavior"}`
        - `Dashboard: fix stale balance cache when reconnecting`
        - `Agent planner: split large work items before execution`
    - Full Title Examples:
        - `## [TD0012] M1 [FEATURE] Add transaction export to CSV`
        - `## [TD0013] M1 [BUG] Login fails with 500 on special chars [PARTIAL 40%]`
        - `## [TD0014] M2 [TECH_DEBT] Replace ad-hoc JSON schema checks with zod`
        - `## [TD0015] M2.1 [KNOWN_BUG] Rare double-charge when retried in under 3s`

### Error Signature for [BUG] Types with Errors or Warnings

- *!* **Error signature:** `{SOURCE}|{LEVEL}|{SYSTEM}|{CODE}|{MESSAGE}`
    Where:
    - `[SOURCE]`: TERMINAL, SERVER, BROWSER, TEST, BUILD, LINTER, APP, UNITY_EDITOR, UNITY_PLAY
    - `[LEVEL]`: FATAL, ERROR, WARN, INFO
    - `[SYSTEM]`: short emitter name, e.g. vite, nextjs, playwright, jest, postgres, nginx, wallet-sync, ngo, urp, probuilder
    - `[CODE]`: stable error code if known, otherwise `NO_CODE`
    - `[MESSAGE]`: normalized, lowercase-safe or sentence-safe canonical message with volatile values stripped
- *!* _Error Normalization Rules_:
    - To maximize grouping and regex usefulness, normalize the signature text with a few hard rules:
    - Strip timestamps.
    - Strip file line numbers when they vary.
    - Replace dynamic IDs, UUIDs, hashes, ports, and durations with placeholders.
    - Replace quoted values with placeholders if the value is not semantically important.
    - Keep the component and failure mode intact.
    - Examples:
        - Example raw lines:
            ```
            Error: connect ECONNREFUSED 127.0.0.1:5432
            Error: connect ECONNREFUSED 127.0.0.1:6432
            ```
        - Normalized signature:
            ```
            SERVER|ERROR|postgres-client|ECONNREFUSED|connect econnrefused {host}:{port}
            ```
        - Regex-friendly syntax:
            ```
            **Error signature:** [ERRSIG]
            `SERVER|ERROR|api|500|post /v1/chat returned internal server error`
            ```
        - Regex Commands:
            ```
            ^\*\*Error signature:\*\* \[ERRSIG\]\s*\n`([^`]+)`$

            **Error signature:** [ERRSIG] `SERVER|ERROR|api|500|post /v1/chat returned internal server error`

            ^\*\*Error signature:\*\* \[ERRSIG\]\s*`([^`]+)`$
            ```

### Rules for citing source documents

- *!* A *first-order relevance* doc is the first recorded occurrence of (1) the initial issue (a bug report, report from adversarial review, the active design/plan doc at the time the deferment decision was made), and (2) the initial handoff-prompt doc *if* the issue was escalated (pushed to a second session **before** insertion to `TODOS.md`) (if simply "added to TODOS" — doc).
    - A *second-order relevance* doc could be (1) research into hypothesizing the root cause or impact/severity of an issue, (2) researching and proposing solutions/fixes/high-level feature-specs as handoff-prompt docs.
    - A *third-order or higher relevance* doc might include higher-order (removed from Source) docs such as debugging, `/qa` bug reports and handoff-prompts, or an agent's design doc/report escalating the issue/bug in scope/complexity/effort. These docs should be cited under **Triage:**, and not bloat the top-level context pathing attached to **Added:**.

### [BUG] Triage Attempts (failed or partial solution attempts)

- *!* *--Note--* **Triage:** files/docs may live at (skill suite memory dirs) `~/.gstack-dev/plans/`, `docs/superpowers/plans/`, `.specify/`; or (Mike repo-native session/memory management for active sprint) `docs/handoff-prompts/{active|archive}/`
- *!* **Try-Count:** and **Triage:** MAX count — if the count value, or number of docs *ever* exceeds **MAX = 5**, it needs to be surfaced as `P[0]` issue for review at next post-mortem. When discovered, *immediately* save a *brief* handoff-prompt doc called `docs/handoff-prompts/current/{YYYY-MM-DD}-P0-triage-escalation.md`, pasting the full TODO item text at the top, and outlining the `P[0]` escalation in 2-5 bullet points of your (or your subagent's) assessment on blocking or regression risks if ignored.
    - Bullet item format: `* []`

---

## Tier Formats

### T1 — Stub Format

One-line format. Lives only in temp memory files. **Never written directly to `TODOS.md`.**

```md
- [ ] ~{TYPE} | {short title} | {why deferred or discovered} | {YYYY-MM-DD}
```

**Valid TYPE values:**
`GENERAL` `FEATURE` `IMPROVEMENT` `BUG` `KNOWN_BUG` `REFACTOR` `TESTING` `TECH_DEBT`

**Examples:**
```md
- [ ] ~BUG | camera not following player in FPV | blocker, defer until after M1 merge | 2026-04-19
- [ ] ~TECH_DEBT | replace ad-hoc JSON schema checks with zod | deferred post-POC | 2026-04-19
- [ ] ~FEATURE | CSV export for transaction history | descoped Sprint 3, post-MVP | 2026-04-19
- [ ] ~KNOWN_BUG | rare double-charge on retry under 3s | low freq, investigate post-launch | 2026-04-19
```

**Rules:**
- `~` prefix marks it as a stub (not yet a full TODO)
- Pipe-delimited — no markdown formatting inside fields
- No ID assigned at this stage (use `TD????` only at T2)
- Do not use commas as delimiters — they appear naturally in descriptions

**Regex to find all stubs in a temp file:**
```regex
^- \[ \] ~(GENERAL|FEATURE|IMPROVEMENT|BUG|KNOWN_BUG|REFACTOR|TESTING|TECH_DEBT)\s*\|\s*(.+)\|\s*(.+)\|\s*(\d{4}-\d{2}-\d{2})$
```

### T2 — Draft Format

Partial template. Written at scope lock or sprint boundary. Uses `TD????` placeholder.

```md
## [TD????] M{milestone}: [{TYPE}] {short clear title}
**What:** {1–2 sentence description}
**Why:** {impact or reason deferred}
**How:** {TBD}
**Priority:** P[~N]
**Effort:** {TBD}
**Regression risk:** {TBD}
**Depends on:** {Nothing OR known dependency}
**Added:** {YYYY-MM-DD} ({sprint or context source})
**Lineage:** {parent stub title or None}
```

**Rules:**
- `TD????` is a literal placeholder — never assign a real ID at this stage
- `{TBD}` fields are expected and acceptable — do not block on them
- Include enough to reconstruct a T3 entry without re-reading sprint context

### T3 — Full TODO Format

Complete entry with real `TD####`. Written at document upkeep stage only.

#### T3 General TODO

```md
## [{TODO_ID}] M{milestone}: [{TYPE}] {short clear title}
**What:** {1–2 sentence description of the task / change.}
**Why:** {Impact, risk, or value; why it matters now or later.}
**How:** {High-level approach; mention key files/classes/components/dependencies.}

**Priority:** {P0 Critical | P1 High | P2 Medium | P3 Low | P4 Trivial}
**Effort:** {time estimate text} (Size: {XS|S|M|L|XL|Full Sprint}; Human: ~{mins/hours/days}, CC: ~{mins/hours/days})
**Regression risk:** {Low|Medium|High} — {short rationale *1-3 sentences*}
**Depends on:** {Nothing | M1 / PR-123 / other TODO id}
**Types:** {optional all-caps comma-separated tags for secondary `TODO` [{TYPE}]'s}
**Tags:** {optional all-caps comma-separated tags for arbitrary issue classifiers: e.g., `[AUTH, INFRA, BILLING]`}

**Added:** {YYYY-MM-DD}
**Context Reference:**
- Parent: {parent TODO id or "None"}
- Source docs:
  - {path/to/original/context/doc.md}
  - {path/to/bug/report/doc.md}   <!-- most recent fully-resolving report -->
  - {other relevant design/spec docs}
```

#### T3 Bug TODO

- If a root cause is provided by the agent(s), assign a confidence value `[CONFIDENCE] = X%/Y%` where `X%` is the agent's own confidence in root cause identification, and `Y%` is an adversarial subagent (*preferably Codex to avoid same-model bias*) confidence score based on its assessment of any provided bug reports/context docs.
    - *!* If no confidence score is provided, assign MIN `15%` (some faith it might work, but needs careful review before we burn tokens coding).
- If bug TODO is `[PARTIAL]` fix:
    - **Triage:** {include 2 sentences for partial solution, prefixed with `[Y%]` for partial fix score}

```md
## [{TODO_ID}] M{milestone}: [BUG] {short clear title} [PARTIAL Y%]
**Priority:** {P0 Critical | P1 High | P2 Medium | P3 Low | P4 Trivial | P? Not Calculated}
**Problem:** {1-sentence description of the observable issue.}
**Error signature:** {If an error or warning is observed, include} [ERRSIG]
`{SOURCE}|{LEVEL}|{SYSTEM}|{STABLE_CODE}|{NORMALIZED_MESSAGE}`
    **Observed evidence:**
    - `{raw line 1}`
    - `{raw line 2}`
**Root cause:** [CONFIDENCE X%/Y%] {2–5 sentence technical cause + scope.}

**Why:** {Impact, risk, or reason it matters now/later. 1-2 sentences.}
**How:** {High-level fix plan; key files/classes/components. 1-2 sentences.}

**Try-Count:** {INT — number of times the issue has been attempted, default to `0`, **MAX = 5**}
**Triage:** {None OR [Y%] {YYYY-MM-DD} {partial solution statement. 1-3 sentences.}}
    **Artifacts:** (**MAX 5** filtered by most recent edit date)
    - {~/.gstack/path/to/bug/report/doc.md}
    - {~/.gstack/path/to/solution/design/doc.md} (if any)
    - {~/.gstack/path/to/previous/fix/attempts/doc.md} (if any)

**Effort:** {time estimate text} (Size: {XS|S|M|L|XL|Full Sprint}; Human: ~{mins/hours/days}, CC: ~{mins/hours/days})
**Verification:** [GATE] {The plan to validate the fix and guard against regressions.}
**Regression risk:** {Low|Medium|High} — {why. 1-3 sentences.}
**Depends on:** {Nothing OR prerequisite milestone/task/dependencies/PR}

**Types:** {optional all-caps comma-separated tags for secondary `TODO` [{TYPE}]'s}
**Tags:** {optional all-caps comma-separated tags for arbitrary issue classifiers: e.g., `[AUTH, INFRA, BILLING]`}
**Follow-ups:** {None OR list of blocked TODOs/sprints/milestones, or deferred hardening/tests/docs}
**Added:** {YYYY-MM-DD}
**Context Reference:**
- Parent: {parent TODO id or "None"}
- Source docs:
  - {path/to/original/context/doc.md}
  - {path/to/bug/report/doc.md}   <!-- most recent fully-resolving report -->
  - {other relevant design/spec docs}
```

---

## Iron Rules

1. **Never write a full T3 entry mid-flow.** Stubs exist so you don't have to.
2. **Never let a stub die without promotion.** A stub in a temp file that never reaches `TODOS.md` is signal loss — as bad as not writing it.
3. **Never assign real IDs at T1 or T2.** IDs are assigned in batch at document upkeep only.
4. **One temp file per session/stage.** Don't scatter stubs across multiple files with no index.
5. **Handoff prompts must list stub counts.** Any handoff prompt must include: how many T1 stubs are unpromoted, how many T2 drafts are in `TODOS.md` without IDs.
6. **Temp files are not permanent.** Archive or delete after ingestion. A temp file that outlives its sprint becomes noise.
7. **Priority score `P[?]` or `P[~N]` always gets promoted at the next doc review.** Never ship a post-mortem with unresolved priority estimates.
8. **Try-Count ≥ 5 is an automatic P[0] escalation.** Write the escalation handoff-prompt *before* attempting a 6th fix.

---

## Populated Examples

### Example: T3 General Feature TODO

```md
## [TD0012] M1 [FEATURE] Add CSV export for monthly transaction history

**What:** Allow users to export their monthly transaction history as a CSV file from the dashboard.
**Why:** Improves bookkeeping workflows and reduces manual data entry for power users.
**How:** Add "Export CSV" button to `/dashboard/transactions`; implement CSV generation in `transactions/exporter.ts` with pagination and date-range filters.

**Priority:** P1 High
**Effort:** 1–2 days (Size: M; Human: ~10–12h, CC: ~2–3h)
**Regression risk:** Medium — touches query layer and pagination logic.

**Depends on:** M0 [FEATURE] Implement transactions filtering
**Tags:** dashboard, exports, billing

**Added:** 2026-04-19 (Sprint S3 review)

**Context Reference:**
- Parent: None
- Source docs:
  - ./docs/product/transactions-reporting.md
```

### Example: T3 Bug TODO with Partial Fix

```md
## [TD0013] M1.2 [BUG] API returns stale balance after wallet reconnection [PARTIAL 40%]

**Problem:** After reconnecting a wallet, the dashboard balance sometimes shows an outdated value for up to 60 seconds.
**Error signature:** [ERRSIG] `SERVER|ERROR|express|500|post api messages returned internal server error`
    - **Observed evidence:**
        - POST /api/messages 500 182ms
        - TypeError: Cannot read properties of undefined (reading 'id')

**Root cause:** [CONFIDENCE 70%/55%]
The balance query is reading from a cached snapshot in `walletCache.ts` that is only invalidated on login, not on wallet reconnection. In high-latency environments, a reconnect event doesn't trigger a downstream `invalidateBalance` call, so clients continue to see the stale snapshot until the periodic refresh.

**Why:** Users may make spending decisions based on incorrect balances, especially after changing wallets or switching networks.

**How:** On wallet reconnection, explicitly call `invalidateBalance(walletId)` and ensure balance subscriptions are re-subscribed. Add a reconnection-specific integration test in `tests/integration/wallet-reconnect.spec.ts`.

**Triage:** [40%] 2026-04-19 Partial mitigation by reducing cache TTL from 60s → 10s in `walletCache.ts`.

**Artifacts:**
- ~/.gstack/bugs/2026-04-19-stale-balance-report.md
- ~/.gstack/bugs/2026-04-19-partial-fix-attempt-1.md

**Priority:** P1 High
**Effort:** 0.5–1 day (Size: S; Human: ~4–6h, CC: ~1–2h)
**Verification:** [GATE] Add automated test ensuring reconnect invalidates cache; run load test with forced reconnects; verify no stale reads in logs.

**Regression risk:** Medium — cache changes can affect performance and other callers.

**Depends on:** Nothing
**Follow-ups:** Add guardrails for cache usage in high-traffic paths; improve observability around cache invalidations.

**Added:** 2026-04-19 (QA regression suite)
**Context Reference:**
- Parent: M1 [FEATURE] Add wallet reconnection support
- Source docs:
  - ./docs/architecture/wallet-sync.md
  - ~/.gstack/bugs/2026-04-19-stale-balance-report.md
```

---
