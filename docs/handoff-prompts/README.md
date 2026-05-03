# Handoff Prompts

Handoff prompts are the handshake point between sessions, agents, and humans. The three principals are:

- **Mike** — human in the loop, final call.
- **Cowork** — orchestrator / producer (drafts plans, briefs CC).
- **Claude Code (CC)** — engineer (implements, wraps up).

Secondary agents: Perplexity (scheduled research), subagents spawned by CC (Opus/Sonnet/Haiku for specific jobs), alternate CLIs to offload Claude compute (Claude spec'd, atomic tasks).

---

## Directory structure

```
docs/handoff-prompts/
├── README.md                  # this file
├── PROMPT_TEMPLATES.md        # index of templates + when to use each
├── templates/                 # reusable prompt skeletons
│   ├── default.md             # Cowork → Claude Code sub-sprint handoff
│   ├── claude-code.md         # Claude Code → Cowork wrap-up
│   ├── cowork.md              # Cowork → Mike (user-facing report)
│   ├── perplexity.md          # Scheduled research → Cowork
│   ├── session-start.md       # Paste at top of a fresh session for context loading
│   └── post-mortem-primer.md  # End-of-sprint retro scaffolding
├── current/                   # currently-open Cowork↔Claude Code handoffs
├── archive/                   # completed handoffs (move here after sprint wrap)
├── cowork/
│   ├── incoming/              # Claude Code → Cowork (questions, requests, wrap-ups)
│   └── outgoing/              # Cowork → Claude Code (briefs, sub-sprint prompts)
├── perplexity/
│   ├── incoming/              # Scheduled research output dumped here
│   └── outgoing/              # Queries to research (picked up by scheduled task)
└── mike/
    ├── user.md                # Mike's personal prompt starters (his scratchpad)
    ├── current/               # Cowork → Mike reports (read, act, archive)
    └── archive/               # Archived Mike-facing reports
```

---

## Flow

1. **Cowork drafts a brief** → writes it into `cowork/outgoing/YYYY-MM-DD-<title>.md`.
2. **CC picks it up** in a fresh session, implements, and writes a wrap-up into `cowork/incoming/YYYY-MM-DD-<title>-wrap.md`.
3. **Cowork reads the wrap-up**, decides next step, optionally reports up to Mike in `mike/current/`.
4. **At sprint-end / after consumption**, completed handoffs are moved to `archive/` (or each principal's `archive/` sub-dir).

### Perplexity

- Queries to research (AI → Mike) go in `perplexity/outgoing/` (picked up by a scheduled task).
- (Mike → Perplexity: Deep Research) Output dumps back into `perplexity/incoming/` for Cowork to synthesize.

### Mike-facing

- Cowork → Mike reports go to `mike/current/`. Mike reads, acts, and moves to `mike/archive/`.
- Mike keeps his own scratchpad starters in `mike/user.md`.

---

## Naming convention

- `YYYY-MM-DD-<short-slug>.md` for new handoffs.
- Wrap-ups append `-wrap` to the slug (`2026-04-21-claude-md-refactor-wrap.md`).
- Use the absolute date — relative ("today", "next-sprint") decays fast.

---

## See also

- `PROMPT_TEMPLATES.md` — which template to use when.
- `templates/session-start.md` — paste into a fresh session for context loading.
