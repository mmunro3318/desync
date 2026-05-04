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

### R1.3: XZ inset
Separator XZ edges must be inset **0.05m inside** the inner face of enclosing exterior walls. For a 0.15m-thick exterior wall with inner face at X=0.15, the separator edge is at X=0.20.

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

### R3.1: Extend into host
Where an internal wall meets an exterior wall, the internal wall must extend **0.05m past** the exterior wall's inner face, into the exterior wall's volume. The internal wall's end is buried inside the exterior wall.

### R3.2: Extend into separator
Where an internal wall meets a horizontal separator (floor/ceiling), the wall's top/bottom extends **0.05m into** the separator per R1.2. (In practice, the separator is positioned to achieve this — the wall height stays at the nominal room height.)

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
Where a railing meets a wall (exterior or internal), the railing extends **0.05m into** the wall volume. Never terminate flush with a wall face.

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
| Interior wall inner faces | X: 0.15 / 13.85, Z: 0.15 / 9.85 |
| GF height | 0 to 2.70m |
| SF height | 2.70 to 5.40m |
| Floor/ceiling thickness | 0.10m |
| Safety overlap | 0.05m |

## Violation Examples

| Violation | What's wrong | Fix |
|-----------|-------------|-----|
| Ceiling top flush with wall top (Y=2.700 = Y=2.700) | Coplanar face, causes banding | Raise ceiling top to Y=2.750 (R1.2) |
| Internal wall ends at exterior wall inner face (X=0.15) | Coplanar face at T-junction | Extend internal wall to X=0.10 (R3.1) |
| Thin-plane ceiling (Y-scale < 0.01) | No volume for walls to terminate into | Replace with 0.1m thick rect (R1.1) |
| Railing base sits exactly on floor top (Y=2.750) | Coplanar face | Lower railing base to Y=2.700 (R4.1) |
| Roof ceiling inset to wall inner edge only | Float precision causes sub-pixel protrusion | Inset additional 0.05m past inner edge (R1.3 + R1.5) |
