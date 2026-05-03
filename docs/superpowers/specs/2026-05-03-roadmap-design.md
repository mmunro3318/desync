# Design Spec: DESYNC ROADMAP.md

**Date:** 2026-05-03
**Status:** Approved and written

## Summary

Created `ROADMAP.md` at repo root — the scheduling and progress-tracking layer for DESYNC development. Consolidates GDD milestones, sprint PDDs, and epic docs into a single glanceable document with dependency analysis.

## Key Decisions

1. **Release milestones:** POC (M0+M1) → Jam Submission (M2+M3) → Stretch (M4)
2. **Track A (vertical slice) is the critical path.** Hand-authored house graph, not pipeline-imported.
3. **Track B (DSL/importer/pipeline) deferred to post-POC.**
4. **Sprint 5A (anchors) reordered to run parallel with S4A/S4B** — only needs graph runtime, not anomaly completion.
5. **Co-op Observation Sprint deferred** — 2-player LAN sufficient for jam.
6. **Jam target:** M1+M2+M3 (full MVP minus stalker). M4 decision gate after M3.
7. **Hybrid progress tracking:** status dashboard table at top + checkbox tasks per sprint.

## Structure

1. Status Dashboard (one-row-per-sprint table)
2. Dependency Graph (ASCII)
3. Release Milestones (time-boxed targets with gate questions)
4. Sprint Detail Blocks (doc criteria + personal gates as checkboxes)
5. Constraints & Principles (multiplayer-first, scope creep management, debug-first)
6. Deferred Work (Track B and post-POC items)
7. Document Lineage (links to source-of-truth docs)

## Files Created/Modified

- `ROADMAP.md` — new, repo root
- `CLAUDE.md` — added Progress Tracking section
- `docs/TODO.md` — added Creative Backlog section with categorized ideas
- `docs/handoff-prompts/current/02-s0.1-light-leak-fix-handoff.md` — next session handoff
- `docs/handoff-prompts/perplexity-research-requests/01-unity-research-batch-for-desync.md` — 8 research batches, 13 prompts
