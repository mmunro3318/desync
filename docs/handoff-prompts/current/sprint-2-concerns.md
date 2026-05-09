# Sprint 2 Concerns Log

Discoveries during Sprint 2 implementation that may need follow-up as TODOs.

---

## C1: PortalVisible fires for all adjacent nodes regardless of camera direction

**Discovered:** 2026-05-09, Phase 0 (TD0018 probe wiring)

**Observation:** After wiring real `PortalProbeData` from `PortalAnchorAuthoring` transforms, the F4 Node Visibility overlay shows `PortalVisible` for ALL adjacent nodes simultaneously, even when the player camera is facing away from those portals. Expected behavior: only portals within the camera's view cone should evaluate as visible.

**Hypotheses (ranked by likelihood):**

1. **Portal-crossing guard too permissive.** `PortalVisibilityEvaluator.EvaluateSingle()` returns `true` whenever `planeDot < 0` (player is "behind" the portal plane). If `PortalAnchorAuthoring` transforms have forward vectors that don't precisely face outward from the room interior, this guard triggers from inside the room and short-circuits the facing test. This is the most likely cause — the anchors were authored for crossing detection (trigger volumes), not for visibility evaluation.

2. **Dot threshold too wide.** The default `dotThreshold` is 0.5 (~60-degree half-angle cone). In a room like `v_hall_a` with 3 doorways, portals may all fall within a 120-degree forward cone from center-room positions. This would only explain the issue when standing near the center of a room, not when facing a wall.

3. **Anchor forward vectors defaulting to identity.** If `PortalAnchorAuthoring` GameObjects were placed without explicitly setting rotation, `transform.forward` defaults to `Vector3.forward` (0,0,1). If all anchors share the same forward, the evaluator can't distinguish which direction the portal faces.

**Impact:** Currently cosmetic — the activation system already uses `Adjacent` as a reason, so rooms appear regardless. But when the observation lock system (Phase 3) consumes visibility results for mutation gating, false-positive `PortalVisible` will over-lock nodes that should be mutation-eligible. This would make the grace timer meaningless (nodes never become eligible because they're always "visible").

**Severity for Sprint 2:** Medium. Must be investigated before Phase 3 visibility lock testing. If portal visibility always returns true for all adjacent nodes, the visibility lock adds no information beyond what occupancy + adjacency already provide.

**Likely fix directions:**
- Audit `PortalAnchorAuthoring` transform orientations in all 5 room prefabs — forwards should point outward from the room interior through the doorway.
- Consider whether the portal-crossing guard (`planeDot < 0`) should be tightened or removed for visibility evaluation (it was designed for "keep destination visible after player steps through" but may fire too eagerly from inside the room).
