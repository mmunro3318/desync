# TODOS Index Template

**Glance-only summary of active TODOs.** Read this before major planning or scoping decisions so you can see what is already on the books without loading the full TODO document.

- Full details -> `[path/to/TODOS.md]`
- Format rules / tier system -> `[path/to/TODOS_TEMPLATES.md]`
- Closed items -> `[path/to/ARCHIVED_TODOS.md]`

---

## How to use this file

When you **add**, **archive**, or materially change a TODO in the main TODO document, update the corresponding row here. This file stays a one-line-per-item summary only; do not expand rows into full entries.

At release, retro, sprint boundary, or other document-upkeep checkpoints: reconcile this index against the main TODO document end-to-end.

---

## Active (by milestone, sprint, track, or bucket)

| ID | Milestone/Track | Title | Depends on | Added |
|---|---|---|---|---|
| TD0001 | M1 | Example: fix stale cache after reconnect | — | YYYY-MM-DD |
| TD0002 | M1 | Example: document architectural decisions | TD0001 | YYYY-MM-DD |
| TD0003 | M2 | Example: add export flow | auth refactor, API readiness | YYYY-MM-DD |

Replace the example rows with live items. Keep titles short and scannable.

## Counts

- **[Bucket A] open:** 0
- **[Bucket B] open:** 0
- **[Bucket C+] open:** 0
- **LAST_USED_ID:** TD0000 (mirror of the main TODO document header, if your system uses incremental IDs)

If you do not use milestone buckets, replace this section with whatever rollup is most useful for scanning, such as by priority, owner, status, or sprint.

## Archived / closed

Point this section at the archive document that holds completed, canceled, or superseded items. If you do not maintain a separate archive, note that here instead.
