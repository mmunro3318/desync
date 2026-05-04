# Phasmo-Clone Migration QA Checklist

## Purpose
This checklist is for reviewing Claude’s archaeology, extraction, and migration work after it inspects the old `Phasmo-Clone` Unity project and seeds the new impossible-house repo. The goal is to verify that the migration preserved the useful foundation while avoiding clone baggage, networking confusion, scene bloat, and architecture drift.

This is a **human review checklist** for accepting or rejecting Claude’s migration decisions.

## What this checklist is checking
Use this checklist to confirm that Claude:
- brought over only the assets/code/settings that help the new vertical slice,
- critically reviewed old code instead of copying everything,
- preserved the new repo’s architecture rules,
- documented what it imported and why,
- and did **not** silently drag old clone assumptions into the new prototype.

## Core acceptance principle
The migration succeeds only if the new repo becomes a **cleaner starting point** than the old one.

If the result feels like “the old clone, but copied into a new folder,” the migration failed.

## Section 1 — Repo-level sanity check

### Checklist
- [ ] The new repo still reads like the impossible-house prototype, not like a Phasmophobia clone.
- [ ] The migrated Unity project structure matches the documented architecture direction rather than the accidental shape of the old repo.
- [ ] The docs remain the source of truth for the new direction.
- [ ] The imported foundation is selective and understandable.
- [ ] There is no obvious flood of unnecessary assets, scenes, packages, or experimental junk.

## Section 2 — Archaeology report quality
Claude should have produced an archaeology report, not just copied files.

### Checklist
- [ ] A markdown archaeology/extraction report exists.
- [ ] The report summarizes the old repo rather than dumping a giant raw file tree.
- [ ] The report clearly classifies findings into keep/refactor/summarize/leave-behind style buckets.
- [ ] The report explains **why** each important category was kept or excluded.
- [ ] The report flags known risk areas, especially networking and lighting.
- [ ] The report is concise enough to read, but specific enough to act on.

## Section 3 — Carry-forward manifest quality
Claude should have created a useful manifest of what moved.

### Checklist
- [ ] A carry-forward manifest exists.
- [ ] The manifest lists source path and destination path.
- [ ] The manifest states whether each item was copied verbatim or refactored.
- [ ] The manifest includes a short reason each item was kept.
- [ ] The manifest is selective rather than exhaustive noise.

## Section 4 — Architecture preservation
This is one of the most important review sections.

### Checklist
- [ ] Imported code still respects runtime-vs-definition separation where relevant.
- [ ] Scene objects remain thin instead of gaining giant logic blobs.
- [ ] There are no obvious manager-god classes brought over “because they already existed.”
- [ ] Clone-specific logic is not masquerading as general architecture.
- [ ] Debug/observability support was preserved or improved for hidden-state systems.
- [ ] The migration aligns with the vertical-slice-first approach rather than broad incomplete breadth.

### Red flags
- [ ] Giant all-purpose manager copied over with minimal scrutiny.
- [ ] Scene-dependent code that only works because a specific old scene hierarchy exists.
- [ ] Reused code still named after ghost/evidence/clone assumptions that do not fit the new prototype.
- [ ] Old implementation shortcuts now shaping the new architecture.

If any red flag is true, stop and review the migration critically.

## Section 5 — Code quality review
Claude was specifically asked to be critical about bloat and verbosity.

### Checklist
- [ ] Reused scripts are reasonably small and understandable.
- [ ] Obvious bloat or excessive verbosity was reduced.
- [ ] Clone-specific branches or hardcoded assumptions were removed where appropriate.
- [ ] Utility code brought forward is actually reusable, not merely familiar.
- [ ] There are no huge unexplained code dumps copied “just in case.”
- [ ] Refactored imports include enough notes to explain what changed.

### Questions to ask
- Could this script be smaller?
- Is this code still useful if the game is no longer a clone?
- Would this be easier to rewrite cleanly than to inherit?

## Section 6 — Relevance of imported systems
The old prototype had a house, physics, lighting, flashlight, and partial local multiplayer. Only the relevant foundation should survive [cite:31][cite:68].

### Checklist
- [ ] Player movement/look code kept only if it is genuinely useful for the new first-person prototype [cite:31].
- [ ] Flashlight or held-item logic kept only if it is clean and reusable [cite:31].
- [ ] Interaction systems kept only if they fit the new architecture and are not heavily clone-specific [cite:63].
- [ ] Test house geometry/materials kept only if they help graybox or atmosphere work [cite:31].
- [ ] Input actions/assets kept only if they save time and still fit the new controls model.
- [ ] Simple debug helpers were preserved if useful.

### Red flags
- [ ] Entire ghost/evidence systems copied over even though they do not serve the current vertical slice.
- [ ] Old menus/progression/lobby systems copied without a clear new-project purpose.
- [ ] Large unrelated prefab trees imported because they were attached to one useful object.

## Section 7 — Networking review
The old project has known multiplayer limitations: local/virtual-player behavior exists, but true cross-computer multiplayer does not work yet [cite:68].

### Checklist
- [ ] Claude treated old networking as suspicious rather than trusted [cite:68].
- [ ] The archaeology report explicitly explains what networking stack is being used, if identifiable.
- [ ] The report distinguishes “useful structural ideas” from “working multiplayer foundation.”
- [ ] Any ported networking code is minimal, justified, and easy to replace if wrong.
- [ ] The new repo does **not** assume that the old networking implementation is already solved.
- [ ] Likely causes of the cross-computer failure were documented if observable [cite:68].

### Red flags
- [ ] Networking code copied wholesale.
- [ ] Old local test harness mislabeled as real multiplayer support.
- [ ] New repo architecture now constrained by old broken networking assumptions.

## Section 8 — Lighting and environment review
The old prototype also has a known light leak artifact between floors [cite:68].

### Checklist
- [ ] Claude evaluated the old lighting/test-house setup as a special case [cite:68].
- [ ] The report proposes a likely cause of the floor-to-floor light leak if enough evidence exists [cite:68].
- [ ] Geometry worth keeping was separated from lighting/config mistakes.
- [ ] The migration did not blindly preserve broken lighting settings just because they were already configured.
- [ ] Graybox materials, meshes, or modular pieces were retained only if they remain useful.

### Red flags
- [ ] Broken lighting setup copied over without explanation.
- [ ] Bad geometry or scene construction imported even though rebuilding would be cleaner.

## Section 9 — Scene and prefab hygiene
One common migration failure is dragging scene clutter into the new project.

### Checklist
- [ ] Only a minimal number of scenes were brought forward.
- [ ] Imported scenes are clearly named and purposeful.
- [ ] Imported prefabs are not giant nested legacy objects full of irrelevant components.
- [ ] Prefabs brought forward are reusable and understandable.
- [ ] Scene references and dependencies were checked after import.
- [ ] There is no obvious scene-level drift or hidden dependency explosion.

## Section 10 — Unity project hygiene
The migration should leave the Unity repo in a clean state.

### Checklist
- [ ] Packages/settings brought forward are intentional.
- [ ] Unused or suspicious packages were not imported automatically.
- [ ] Input System configuration is coherent if imported.
- [ ] URP/render settings are understandable if imported.
- [ ] Project opens without obvious missing-script chaos.
- [ ] The imported project skeleton supports the documented folder structure.

## Section 11 — Documentation alignment
The new repo already has a dense docs stack. The migration should align with it, not contradict it.

### Checklist
- [ ] Imported structure still fits the `Repo Docs Index / Claude File Map` logic.
- [ ] Migrated systems can be mapped to the vertical slice docs cleanly.
- [ ] No imported code quietly contradicts the networked-house runtime contracts.
- [ ] The archaeology report references the new project direction, not only the old clone history.
- [ ] The migration clearly distinguishes current truth from legacy source material.

## Section 12 — Playable smoke review
After migration, the repo should still be practical to work in.

### Checklist
- [ ] The new Unity project opens successfully.
- [ ] Any intentionally imported sample/test scene loads.
- [ ] Basic player/controller functionality works if it was imported.
- [ ] Flashlight/interactions work if they were imported.
- [ ] Imported utilities compile cleanly.
- [ ] There is no flood of console noise from irrelevant legacy code.

## Section 13 — Decision quality review
This section is about judgment, not just mechanics.

### Checklist
- [ ] Claude showed restraint.
- [ ] Claude preferred smaller reusable foundations over broad inheritance.
- [ ] Claude explicitly left behind things that would create drag.
- [ ] Claude used summaries when full code copies were unnecessary.
- [ ] Claude surfaced uncertainty instead of pretending every legacy choice was correct.

## Section 14 — Final accept/reject gate
Only accept the migration if most of the following are true.

### Accept if
- [ ] The imported foundation is clearly useful.
- [ ] The new repo is cleaner than the old one.
- [ ] Architecture direction was preserved.
- [ ] Networking risk was treated conservatively.
- [ ] Lighting/environment issues were documented sensibly.
- [ ] The archaeology report and manifest are actually useful.
- [ ] You can clearly explain why each major imported category exists.

### Reject or revise if
- [ ] The migration feels like a blind repo copy.
- [ ] Too much clone-specific baggage came over.
- [ ] The imported code is still bloated or tightly coupled.
- [ ] Scene/prefab clutter exploded.
- [ ] The networking story is still muddy.
- [ ] The new vertical slice would be easier to build from scratch than from the migrated result.

## Recommended review workflow
1. Read the archaeology report.
2. Read the carry-forward manifest.
3. Inspect the imported folders/scripts/assets.
4. Open the Unity project.
5. Run a minimal smoke test.
6. Mark this checklist.
7. Approve, revise, or roll back selectively.

## Final heuristic
Ask this question repeatedly during review:

> Did this migration make the new impossible-house prototype easier to build, or did it merely preserve old effort?

Only keep the migration decisions that clearly improve the new project.
