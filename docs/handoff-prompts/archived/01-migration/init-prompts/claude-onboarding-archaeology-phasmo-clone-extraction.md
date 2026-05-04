# Claude Onboarding / Archaeology Prompt — Review Existing Phasmo-Clone and Extract What We Should Keep

## Purpose
This prompt is for a **fresh Claude session** operating in a **new repo** that currently contains documentation but not the old Unity implementation. Claude’s job is to review the prior Unity project (the earlier `Phasmo-Clone` codebase), identify what is worth carrying forward into the new impossible-house prototype, and help seed the new Unity repo with only the relevant code, assets, settings, and project structure.

The goal is **selective salvage**, not a blind port.

## High-level instruction to Claude
You are reviewing an older Unity project called `Phasmo-Clone` as an archaeology and extraction task for a new Unity game prototype. The new project has pivoted away from a direct Phasmophobia clone into a spatial-horror impossible-house co-op prototype. Your job is to:

1. inspect the old Unity project,
2. identify the parts worth reusing,
3. identify the parts that should **not** be copied over,
4. extract or recreate the useful foundation in the new repo,
5. and produce a concise archaeology report plus a recommended carry-forward list.

Be critical. Do **not** assume old code is correct just because it exists.

## New project context
The new project is a Unity-based first-person co-op spatial horror prototype built around:
- an impossible house / graph-based house runtime,
- portal and hallway anomalies,
- observation locks,
- multiplayer authority and reconciliation,
- graybox-first development,
- debug-first hidden-state systems,
- modular runtime architecture,
- and narrow, testable vertical slices.

This is **not** a straight continuation of the old clone design. The old repo is a source of potentially useful implementation work, assets, and project setup — not the new product definition.

## Your mission
When reviewing the old repo, sort findings into four buckets:

### Bucket A — Copy over directly
Things that are already good, small, relevant, and low-risk to reuse with little or no change.

Examples might include:
- Unity project settings worth preserving,
- useful input actions,
- simple first-person controller pieces,
- flashlight systems,
- basic interactable interfaces,
- graybox materials/prefabs,
- debug helpers,
- test house geometry or modular pieces,
- generally reusable utility code.

### Bucket B — Copy, but refactor first
Things that contain useful ideas but are too coupled, bloated, verbose, clone-specific, or awkwardly structured to bring over unchanged.

Examples might include:
- managers that do too much,
- clone-specific scripts with reusable sub-parts,
- interaction systems with hardcoded assumptions,
- networking code that partially works locally but not across machines,
- overgrown prefabs,
- scene-dependent logic.

### Bucket C — Summarize only
Things worth learning from, but **not** worth copying verbatim.

Examples:
- experimental systems,
- dead-end prototypes,
- overly verbose code with one useful concept inside,
- clone-specific ghost/evidence logic that no longer fits the game,
- one-off hacks that solved a temporary problem.

### Bucket D — Leave behind
Anything irrelevant, misleading, broken in a dangerous way, obsolete, or too tied to the old Phasmophobia-clone direction.

Examples:
- ghost/evidence clone systems that are no longer structurally useful,
- dead assets,
- broken networking experiments that are not worth preserving as code,
- throwaway scenes,
- menu/progression remnants,
- duplicate or unused packages,
- prefab clutter,
- generated or imported junk.

## Critical review expectations
Do **not** merely inventory files. Review them critically.

Specifically:
- Flag code that is bloated, overly verbose, or more complex than the problem requires.
- Flag scene or prefab setups that appear fragile or tightly coupled.
- Flag clone-specific logic that should not infect the new architecture.
- Flag utility code that is only “reusable” in theory but would cause architectural drag.
- Prefer small, clean, teachable building blocks over giant inherited systems.

If something is salvageable only after simplification, say so explicitly.

## Known legacy issues to watch for
Please review the old repo with these known problems in mind:

### 1. Multiplayer/networking issue
The previous prototype supports a kind of local multiplayer / virtual-player test setup, but **true multiplayer across separate computers is not working yet**.

That means:
- networking code is **suspect**,
- networking assumptions may be wrong,
- transport/setup/configuration may be incomplete,
- and local “it sort of works” behavior should not be treated as production-ready.

Treat old networking code as something to analyze and possibly salvage in parts, **not** as trusted foundation.

### 2. Lighting issue
There is a known **light leak artifact between floors** in the test house.

That means:
- inspect lighting setup,
- inspect mesh overlap / ceiling-floor construction,
- inspect baked vs realtime assumptions,
- inspect URP/light/shadow settings if relevant,
- and identify whether the issue comes from geometry, lighting config, material setup, probe setup, or scene authoring.

Do not over-focus on fixing it in the old repo unless the cause helps determine what geometry/settings should be carried into the new project.

## Documentation context in the new repo
The new repo already contains a docs stack describing the new direction. You should use those docs to decide what is relevant from the old repo.

Read these first in the new repo before doing extraction work:
1. `repo-docs-index-claude-file-map.md`
2. `spatial-horror-gdd.md`
3. `spatial-runtime-framework.md`
4. `networked-house-runtime-interfaces-contracts.md`
5. `networked-house-vertical-slice-integration-checklist.md`
6. `claude-code-implementation-prompt-pack-vertical-slice.md`

Use those docs as the evaluation filter.

## What success looks like
Success is **not** “the new repo now resembles the old repo.”

Success is:
- the new Unity repo gets only the assets, code, settings, and structure that help the new vertical slice,
- the imported foundation is cleaner than the old repo,
- unnecessary clone baggage is left behind,
- networking assumptions are treated cautiously,
- and the result is a stronger starting point for the impossible-house prototype.

## Specific tasks
Please do the following in order.

### Task 1 — Review the old repo structure
Inspect the old Unity project and produce a concise structural overview:
- folders,
- important scenes,
- major prefabs,
- scripts that seem foundational,
- packages/settings that matter,
- and obvious clutter or dead weight.

Do not dump an enormous raw file tree unless necessary. Summarize intelligently.

### Task 2 — Identify the reusable foundation
Find the old systems/assets most relevant to the new project, especially:
- Unity project settings,
- package setup,
- input system assets,
- player movement/look,
- interaction patterns,
- held-item or flashlight systems,
- minimal HUD/debug elements,
- test-house environment assets,
- materials/textures useful for graybox or atmosphere,
- prefab patterns that are still sane,
- simple scene bootstrap patterns,
- reusable utility code.

For each, classify as:
- copy directly,
- copy after refactor,
- summarize only,
- or leave behind.

### Task 3 — Evaluate networking separately
Audit the old networking/multiplayer code as its own category.

Answer:
- What networking stack is being used?
- What parts seem structurally useful?
- What parts are incomplete or misleading?
- What probably explains why cross-computer multiplayer is not working yet?
- What, if anything, should be ported into the new repo right now?

Be conservative.

### Task 4 — Evaluate lighting/test-house geometry separately
Audit the old test-house scene and lighting setup.

Answer:
- What parts of the house/environment are worth reusing?
- Are any modular pieces worth bringing over?
- What likely causes the floor-to-floor light leak?
- Which geometry/material/lighting settings should be copied?
- Which should be rebuilt cleanly instead?

### Task 5 — Extract only the keepers
Create the new Unity project directory and seed it with the things that should actually carry forward.

This may include:
- folder structure,
- project settings,
- packages manifest guidance,
- selected scripts,
- selected prefabs/materials,
- selected scenes or scene fragments,
- input assets,
- and documentation notes.

Only move/copy what belongs in Buckets A and carefully selected parts of Bucket B.

### Task 6 — Refactor while importing if needed
If a reused script is obviously too verbose, too clone-specific, or too coupled, do not just copy it blindly.

Instead:
- simplify it,
- strip clone-specific naming if appropriate,
- reduce bloat,
- preserve only the reusable core behavior,
- and explain what changed.

### Task 7 — Produce an archaeology report
Create a markdown report in the new repo that includes:
- summary of the old repo,
- reusable findings by category,
- what was imported,
- what was deliberately excluded,
- known risks carried forward,
- networking findings,
- lighting findings,
- and recommended next implementation steps.

Suggested filename:
- `docs/05-migration/phasmo-clone-archaeology-and-extraction-report.md`

## Output format expectations
Please provide:

### 1. A short chat summary
In chat, summarize:
- what you found,
- what you imported,
- what you deliberately left behind,
- and the highest-risk legacy areas.

### 2. A repo report
Write the full archaeology/extraction report as markdown inside the repo.

### 3. A carry-forward manifest
Create a concise machine/human-readable manifest of imported items.

Suggested filename:
- `docs/05-migration/phasmo-clone-carry-forward-manifest.md`

The manifest should list:
- source path in old repo,
- destination path in new repo,
- classification bucket,
- why it was kept,
- and whether it was copied verbatim or refactored.

## Style guidance
Be concise, critical, and useful.

Prefer:
- short code excerpts where they clarify a reusable pattern,
- summaries when full code is unnecessary,
- explicit warnings where code smells or likely bugs appear,
- and clear rationale for every keep/discard decision.

Avoid:
- huge code dumps,
- blind file copying,
- overexplaining Unity basics,
- or preserving clone-era naming that no longer fits.

## Architectural guardrails
While doing this work, preserve these rules:
- runtime state separate from definitions,
- thin scene objects,
- no giant manager gods,
- debug-first hidden-state support,
- graybox-first development,
- modular systems with narrow ownership,
- and vertical-slice-first execution.

If the old repo violates these rules, prefer extracting a smaller cleaner subset instead of importing the violation.

## Decision heuristic
If unsure whether to carry something over, ask:

> Does this directly help the first networked impossible-house vertical slice?

If the answer is not clearly yes, it probably should not be imported yet.

## Suggested final deliverables in the new repo
At the end of this task, the new repo should ideally contain:
- the Unity project skeleton,
- the docs already present,
- selected carried-forward assets/scripts/settings,
- the archaeology report,
- the carry-forward manifest,
- and a cleaner starting point than the original clone repo.

## Final instruction to Claude
Be an **architectural archaeologist**, not a copier.

Extract the smallest high-value foundation from the old Phasmo-Clone that accelerates the new impossible-house prototype without dragging old clone assumptions, networking mistakes, scene bloat, or debug blind spots into the new repo.
