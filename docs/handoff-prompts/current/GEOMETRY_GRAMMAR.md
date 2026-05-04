# Geometry Construction Grammar

> Codified rules for constructing modular graybox geometry in DESYNC.
> All rules exist to prevent coplanar-face artifacts (light leaks, shadow banding) caused by IEEE 754 float imprecision when two surfaces share an edge.

## Core Principle

**No two geometry pieces may share a coplanar face.** Every junction between pieces must have one piece extending INTO the other by at least **0.05m** (the safety overlap). The piece that is "continuous" (the structural host) receives the terminating piece into its volume.

## Hierarchy of Dominance

When two pieces meet, one is the **host** and the other **terminates into** it. The host's surface is never modified; the terminating piece extends past the host's surface into its volume.

**Dominance order (host wins):**
1. **Horizontal separators** (floors, ceilings, roof caps) — always host
2. **Exterior walls** — host for internal walls and railings
3. **Internal walls** — host for railings, fixtures, and trim
4. **Railings / fixtures** — always terminate into something else

When two pieces of the same rank meet (e.g., two internal walls at a corner), either may be the host — pick one consistently and document it.

---

## Rule 1: Horizontal Separators

Horizontal separators are floors, ceilings, and roof caps. They are the dominant structural element.

### R1.1: Minimum thickness
Every horizontal separator must be a **solid box with Y-scale >= 0.1m**. Never use a flat plane or thin quad.

### R1.2: Wall burial
The separator's top face must extend **0.05m above** the top of any walls it contacts. Walls terminate INTO the separator — their top edges are buried inside the slab volume. This means:
- Separator top = wall top + 0.05m
- Separator bottom = separator top - thickness (>= 0.1m)

### R1.3: XZ extent to wall midpoint
Separator XZ edges must extend to the **midpoint** of enclosing exterior walls (50% of wall thickness from the outer face). For a 0.15m-thick exterior wall, the separator edge is at **0.075m** from the outer face. This ensures the separator is visually flush with the wall interior (no gap) while staying safely inside the wall volume (no z-fighting at the outer face).

- Left wall (outer X=0.0): separator X-min = **0.075**
- Right wall (outer X=14.0): separator X-max = **13.925**
- Front wall (outer Z=0.0): separator Z-min = **0.075**
- Back wall (outer Z=10.0): separator Z-max = **9.925**

### R1.4: Inter-floor overlap
Where a floor separator sits above a ceiling separator (multi-story), the floor's bottom must extend **0.05m below** the ceiling's top. Never coplanar, never gapped.

### R1.5: Roof cap
The topmost ceiling of the building is a **roof cap** — a horizontal separator following all the same rules. It must be a thick rect (R1.1), extend above wall tops (R1.2), and be XZ-inset (R1.3). There is no geometry above to mask its edges, so this rule is critical.

### R1.6: Shadow casting
All horizontal separator MeshRenderers must have `Cast Shadows = Two Sided`.

---

## Rule 2: Exterior Walls

Exterior walls form the building envelope. They are continuous along each face.

### R2.1: Full-height coverage
Each exterior wall piece covers the full height of its floor (Y=0 to Y=wall_top for GF, Y=floor_top to Y=wall_top for SF). No vertical gaps.

### R2.2: Inter-floor continuity
GF and SF exterior wall pieces on the same face do NOT need to overlap each other — the horizontal separator between them (R1.2, R1.4) bridges the junction and prevents coplanar artifacts.

### R2.3: Consistent thickness
All exterior walls use a uniform thickness (currently 0.15m). Inner face positions derive from this.

---

## Rule 3: Internal Walls (T-Junctions)

Internal walls partition rooms. They terminate at other walls or at horizontal separators.

**Key insight:** The original graybox construction has internal walls extending to the exterior wall **outer face** (X=0.0, Z=0.0), passing through the full wall thickness. This creates z-fighting at the building exterior. The fix is to **trim inward**, not extend outward.

### R3.1: Trim to inner face overlap
Where an internal wall meets an exterior wall, the internal wall must be **trimmed** so its exterior-facing end penetrates **0.05m into the wall volume past the exterior wall's inner face** (toward the outer face). The end is embedded in the exterior wall, but does NOT reach the outer face.

For a 0.15m-thick exterior wall:
- Left wall (outer X=0.0, inner X=0.15): internal wall X-min = **0.10**
- Right wall (outer X=14.0, inner X=13.85): internal wall X-max = **13.90**
- Front wall (outer Z=0.0, inner Z=0.15): internal wall Z-min = **0.10**
- Back wall (outer Z=10.0, inner Z=9.85): internal wall Z-max = **9.90**

### R3.2: Wall-separator junction
Where an internal wall meets a horizontal separator (floor/ceiling), the separator is positioned so the wall top is **0.05m below** the separator top per R1.2. Wall heights stay at the nominal room height — the separator handles the burial.

### R3.3: Internal T-junctions
Where two internal walls meet at a T-junction, the terminating wall extends **0.05m into** the continuous wall. Document which wall is continuous in ambiguous cases.

### R3.4: Height
Internal wall heights match their floor's nominal height (GF: 2.7m, SF: 2.7m). The horizontal separator above handles the burial (R1.2).

---

## Rule 4: Railings and Fixtures

Railings, trim, and decorative geometry are always the lowest-priority pieces.

### R4.1: Base burial
Railing bases must extend **0.05m into** the floor slab they sit on. Position the railing so its bottom is 0.05m below the floor's top face.

### R4.2: Wall contact
Where a railing meets an **internal** wall, the railing extends **0.05m into** the wall volume. Where a railing meets an **exterior** wall, the railing is **trimmed** to 0.05m inside the inner face per R3.1 — never extend to the outer face.

---

## Rule 5: General Constraints

### R5.1: No piece may extend beyond the building envelope
All internal geometry must stay within the exterior wall outer faces. (Exception: horizontal separator overlap into wall volume per R1.3 is INTO, not past, the wall.)

### R5.2: Grid-snap
All modular pieces should be grid-snapped during placement to prevent sub-millimeter gaps.

### R5.3: Consistent overlap constant
The overlap/inset constant is **0.05m** everywhere. Do not use different values for different junction types.

---

## Reference: Current House_Graybox Dimensions

| Parameter | Value |
|-----------|-------|
| Building envelope | X[0, 14], Z[0, 10] |
| Exterior wall thickness | 0.15m |
| Exterior wall midpoint | X: 0.075 / 13.925, Z: 0.075 / 9.925 |
| Interior wall inner faces | X: 0.15 / 13.85, Z: 0.15 / 9.85 |
| GF height | 0 to 2.70m |
| SF height | 2.70 to 5.40m |
| Floor/ceiling thickness | 0.10m |
| Safety overlap | 0.05m |

## Violation Examples

| Violation | What's wrong | Fix |
|-----------|-------------|-----|
| Ceiling top flush with wall top (Y=2.700 = Y=2.700) | Coplanar face, causes banding | Raise ceiling top to Y=2.750 (R1.2) |
| Internal wall extends to exterior outer face (X=0.0) | Z-fighting at building exterior | Trim internal wall to X=0.10 (R3.1) |
| Thin-plane ceiling (Y-scale < 0.01) | No volume for walls to terminate into | Replace with 0.1m thick rect (R1.1) |
| Railing base sits exactly on floor top (Y=2.750) | Coplanar face | Lower railing base to Y=2.700 (R4.1) |
| Separator edge at wall inner face (X=0.15) | Visible gap between floor and wall | Extend to wall midpoint X=0.075 (R1.3) |
