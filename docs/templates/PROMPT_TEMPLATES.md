# PROMPT_TEMPLATES -- Index

This is the index for reusable handoff prompt skeletons stored in `templates/`. If you're writing a handoff and not sure which skeleton to grab, start here.

## The templates

| Template | Direction | When to use |
|---|---|---|
| `templates/default.md` | Cowork → Claude Code | Sub-sprint briefs. The 80% case. Opens a `handoff-prompts/current/` entry. |
| `templates/claude-code.md` | Claude Code → Cowork | Sub-sprint wrap-up. Closes an `active/` entry and parks it for archive. |
| `templates/cowork.md` | Cowork → Mike (user) | User-facing report. Things Mike personally needs to decide, install, or act on. Parks in `mike/current/`. |
| `templates/perplexity.md` | Cowork → Perplexity (scheduled research) | Research query. Dropped in `perplexity/outbox/`, scheduled task picks it up. |
| `templates/session-start.md` | Paste at top of any fresh session | Tier-1 context loading boilerplate. Forces CLAUDE.md read order. |
| `templates/post-mortem-primer.md` | End-of-sprint retro | Scaffolding for post-sprint retrospective. Invoked via `/retro`. |

## Routing rules

- **Agent ↔ Agent handoffs** → `cowork/`, `subagent/`, `alt-cli-ai/` (Codex, Gemini). Keep these aggressive and terse -- the recipient is another LLM that reads fast.
- **Agent → Mike handoffs** → `mike/current/`. Keep this surface SMALL. Only things that require a human: install a tool, approve a design call, pick between options, etc.
- **Agent → Mike → Perplexity handoffs** → `perplexity/incoming/`. Keep this a strategic and comprehensive LLM prompt -- Mike copy/pastes into Perplexity Deep Research, and saves report in `perplexity/outgoing`
- **Long-lived sub-sprint briefs** → `active/` at the top level (not under `cowork/`). These are the canonical in-flight handoffs that either side reads.
- **Completed handoffs** → move to `archive/` (or the appropriate sub-archive). Do NOT delete.

## File naming

`YYYY-MM-DD-{slug}.md`, optionally `-{agent-name}` suffix.

Examples:
- `2026-04-19-sub-sprint-b-wrap.md`
- `2026-04-19-mppm-install-cowork.md`
- `2026-04-19-pcg-room-research-perplexity.md`

## Iron rules

1. **Templates are skeletons, not scripture.** Cut sections that don't apply. Don't pad.
2. **Every handoff names its sender, recipient, and the question/ask in the first 3 lines.** A reader should know in 5 seconds why this file exists.
3. **When a handoff is resolved, MOVE it to archive.** Never delete.
4. **Never edit `docs/workspace.md` or `user/user.md`** -- those are Mike's scratchpads. Drop new prompts in `mike/current/` instead.
5. **If a handoff requires Mike specifically, it goes in `mike/current/` -- not `cowork/incoming/`.** Cowork's inbox is for other agents, not humans.
6. **Every Claude-Code wrap-up includes:** what works, what's blocked, ARCH decisions to record, TODO stubs created thi