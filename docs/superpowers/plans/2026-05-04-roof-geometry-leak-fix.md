# Roof Geometry Leak Fix — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix the light leak on the roof of `House_Graybox.unity` caused by SF_Ceiling edges being exactly coplanar with wall inner faces, producing visible bright bands at the roofline.

**Architecture:** Same geometry-inset approach as the original floor/ceiling light leak fix (ARCH.md, S0.1). The SF_Ceiling scale is reduced to create a 0.05m overlap with the wall thickness on each edge — matching the 0.05m overlap used for floor-ceiling junctions in the original fix. No code changes; scene-only fix via MCP.

**Tech Stack:** Unity 6 scene editing via MCP tools (`manage_components` for transform property changes, `manage_camera` for verification screenshots).

---

## Root Cause Analysis

The original light leak fix inset all floor/ceiling cubes to the wall inner edges: X[0.15, 13.85], Z[0.15, 9.85]. This works for lower ceilings because SF_Floor pieces overlap GF_Ceiling by 0.05m from above, covering any edge artifacts.

**SF_Ceiling (the roof) has no overlapping geometry from above.** Its edges are exactly coplanar with the wall inner faces. IEEE 754 float imprecision in the scale value (13.7 is not exactly representable — `13.7/2 ≈ 6.84999990`, making the edge ≈ `0.15000009`) causes a sub-pixel protrusion past the wall inner face. Interior lights illuminate this sliver of the ceiling underside, producing visible bright bands on the building exterior at the roofline.

**Fix:** Reduce SF_Ceiling XZ scale from (13.7, 0.1, 9.7) to (13.6, 0.1, 9.6), placing edges at X[0.2, 13.8] Z[0.2, 9.8] — a clear 0.05m inside the wall inner faces. This matches the 0.05m safety overlap used elsewhere in the original fix.

**Trade-off:** A 0.05m gap will be visible at the very top of interior walls where the ceiling doesn't reach the wall face. At the top of 2.7m walls in a dark horror game, this is effectively invisible in gameplay.

## Current SF_Ceiling State (pre-fix)

| Property | Current Value | Target Value |
|----------|--------------|--------------|
| Position | (7, 5.35, 5) | (7, 5.35, 5) — unchanged |
| Scale X | 13.7 | **13.6** |
| Scale Y | 0.1 | 0.1 — unchanged |
| Scale Z | 9.7 | **9.6** |
| X edges | [0.15, 13.85] | **[0.2, 13.8]** |
| Z edges | [0.15, 9.85] | **[0.2, 9.8]** |
| Wall overlap | 0.00m (flush) | **0.05m** (safe) |
| shadowCastingMode | TwoSided (2) | TwoSided (2) — unchanged |

---

### Task 1: Capture before-screenshot for comparison

**Files:** None (read-only verification)

- [ ] **Step 1: Take a screenshot from above the building showing the roof leak**

Use MCP `manage_camera` with `screenshot_multiview` or `screenshot` action to capture the scene view from above the building, matching the angle in `docs/handoff-prompts/current/geometry-bug-repeat-on-roof.png`. Frame the building roof so the bright bands at the edges are visible.

Save as: `docs/handoff-prompts/current/geometry-bug-roof-BEFORE.png`

- [ ] **Step 2: Verify the leak is visible in the screenshot**

Confirm the bright bands are visible at the roof edges. If not visible, adjust the scene view angle and retake.

---

### Task 2: Apply the ceiling scale fix

**Files:**
- Modify: `House_Graybox.unity` (via MCP — SF_Ceiling transform, instance ID 108240)

- [ ] **Step 1: Set SF_Ceiling scale X to 13.6**

```
MCP: manage_components
  action: set_property
  target: 108240
  component_type: Transform
  property: localScale
  value: {"x": 13.6, "y": 0.1, "z": 9.6}
```

This changes the ceiling edges from X[0.15, 13.85] Z[0.15, 9.85] to X[0.2, 13.8] Z[0.2, 9.8], creating 0.05m of wall overlap on each edge.

- [ ] **Step 2: Verify the transform was applied**

Read back the SF_Ceiling transform via MCP resource `mcpforunity://scene/gameobject/108240/components` and confirm:
- Position is still (7, 5.35, 5)
- Scale is now (13.6, 0.1, 9.6)

---

### Task 3: Verify the fix visually

**Files:** None (read-only verification)

- [ ] **Step 1: Take an after-screenshot from the same angle**

Use MCP `manage_camera` screenshot from the same angle as the before-screenshot, showing the roof.

Save as: `docs/handoff-prompts/current/geometry-bug-roof-AFTER.png`

- [ ] **Step 2: Confirm the bright bands are gone**

Compare before/after. The bright bands at the roof edges should be eliminated. If any bands remain, the overlap may need to be increased, or the issue has a different secondary cause.

- [ ] **Step 3: Check interior view**

Frame the scene view from inside the second floor, looking up at the ceiling. Confirm the 0.05m gap at the wall-ceiling junction is not visually distracting. In the dark interior with point lighting, it should be imperceptible.

---

### Task 4: Save scene and commit

**Files:**
- Modify: `House_Graybox.unity` (save via MCP)
- Modify: `docs/ARCH.md` (update construction rules)

- [ ] **Step 1: Save the scene**

```
MCP: manage_scene
  action: save
```

- [ ] **Step 2: Update ARCH.md construction rules**

In the "URP lighting: modular graybox floor/ceiling construction" section, add a note about the roof-specific fix. After the existing rule "Floor/ceiling rects must be inset to the inner edges of the enclosing walls", add:

```markdown
- The topmost ceiling (roof) must be inset an additional 0.05m past the wall inner edges (total 0.20m from outer wall) to prevent floating-point coplanar artifacts — unlike lower ceilings, the roof has no overlapping floor piece from above to mask edge seams.
```

- [ ] **Step 3: Commit**

```bash
git add unity-DESYNC/Assets/_Project/Scenes/House_Graybox.unity
git add docs/ARCH.md
git commit -m "fix: inset SF_Ceiling to eliminate roof light leak (geometry coplanar artifact)"
```

---

## Verification Checklist

- [ ] Roof bright bands eliminated (before/after screenshot comparison)
- [ ] Interior ceiling appearance acceptable (no visible gap from player height)
- [ ] Scene saves cleanly (no console errors)
- [ ] Existing `NetworkBootstrapConsistencyTests` still pass (unrelated but sanity check)
