## Problem Statement

The house graph runtime (S1A) and portal visibility system (S1B) are complete with 125 passing tests, but players walk through invisible rooms. All 5 Room_* prefabs are trigger-volume-only with no mesh geometry. The only visual feedback is debug gizmos (F3/F4 overlays). The game cannot be shown to anyone outside the dev team because there is nothing to see.

## Solution

Add visible ProBuilder graybox geometry to all 5 room prefabs so the activation system becomes tangible. Rooms appear and disappear as the player moves. Looking through a doorway shows the adjacent room. The house graph stops being an abstraction and becomes something you can walk through and show someone.

## User Stories

1. As a player, I want to see walls, floors, and ceilings in each room, so that I can orient myself in the house.
2. As a player, I want rooms to appear when I walk toward them, so that the spatial horror activation system is visible.
3. As a player, I want rooms I leave behind to disappear, so that only nearby rooms are rendered.
4. As a player, I want to look through a doorway and see the connected room, so that I can navigate the house visually.
5. As a player, I want doorway openings to match the actual portal positions, so that navigation feels consistent.
6. As a player, I want the room I am standing in to always be visible, so that I never fall into void space.
7. As a developer, I want the presentation hierarchy separated from tracking infrastructure, so that deactivating a room visuals never kills its trigger volumes.
8. As a developer, I want the streaming controller to bind to the local player automatically on spawn, so that the resolve/present loop activates without manual wiring.
9. As a developer, I want the local player binding to clean up on despawn, so that reconnect/disconnect does not leave stale references.
10. As a developer, I want debug overlays (F3 graph topology, F4 visibility) to work against visible geometry, so that I can tune activation behavior.
11. As a developer, I want all 125+ existing tests to pass after the refactor, so that the hierarchy change does not regress prior work.
12. As a developer, I want a "root stays active when presentation deactivates" test, so that the self-lockout fix is proven and regression-protected.
13. As a developer, I want the forceAllActive debug toggle to show all rooms with geometry, so that I can inspect the full house layout.
14. As a tester, I want a single-room proof gate (Gate 0) before bulk authoring, so that hierarchy and activation issues surface early.
15. As a multiplayer tester, I want host and client to see consistent room activation, so that the spatial experience is shared.

## Implementation Decisions

### Module 1: NodePresentationHandle Refactor (deep module)

The core architectural change. `SetPresentation(bool)` interface stays the same but internals change:
- Add `[SerializeField] Transform presentationRoot` field with `[Header("Presentation")]` attribute
- `SetPresentation(bool)` toggles `presentationRoot.gameObject.SetActive()` instead of `this.gameObject.SetActive()`
- Add `OnValidate()` warning when `presentationRoot` is null
- Add runtime warning in `SetPresentation()` when `presentationRoot` is null (warn + return, no throw)
- Remove the existing WARNING comment about self-lockout (the coupling is resolved)
- This matches the existing authoring pattern: `RoomNodeAuthoring.roomVolume` and `PortalAnchorAuthoring.crossingTrigger` are both serialized references with OnValidate warnings

### Module 2: Prefab Hierarchy Migration (asset work)

Structural migration of 5 Room_* prefabs and House_Prototype scene:
- Add empty `Presentation` child GameObject to each Room_* prefab root
- Wire `NodePresentationHandle.presentationRoot` to the new Presentation child on each prefab
- Remove 5 legacy scene-level NodePresentationHandle components from House_Prototype (added during S1B as scene overrides)
- **Invariant: portal anchors (PortalAnchorAuthoring) and room triggers (BoxCollider) must NOT be children of Presentation.** Only renderable geometry goes under Presentation. Tracking/authoring infrastructure stays on the always-active root.
- **Post-migration invariant:** Exactly 5 NodePresentationHandle instances discovered in House_Prototype at runtime. One per room. Prefab-owned. Each targeting a Presentation child. Zero legacy scene-level handles remaining.

### Module 3: PlayerMotor Local Binding Lifecycle (thin wiring)

Spawn + despawn lifecycle wiring for TD0021:
- In `PlayerMotor.OnNetworkSpawn()`, after the existing `IsOwner` check, add a `BindLocalStreamingContext()` private helper that finds the scene NodeStreamingController via `FindAnyObjectByType` and calls `BindLocalPlayer(tracker, cam)` where tracker = `GetComponent<PlayerNodeTracker>()` and cam = `GetComponentInChildren<Camera>()`
- Add `OnNetworkDespawn()` override that calls `BindLocalPlayer(null, null)` to clear stale refs on disconnect/respawn
- Only the locally-owned player binds (IsOwner gate). Non-owners skip binding entirely.
- `BindLocalPlayer(null, null)` is already a supported no-op clear path in NodeStreamingController
- Add ARCH.md entry documenting this concern-mixing decision and the extraction trigger: "extract to dedicated component when 3+ local-player bootstrap responsibilities accumulate in PlayerMotor"

### Module 4: Graybox Geometry Authoring (asset work)

ProBuilder room geometry under Presentation children:
- **Entry first as proof gate (Gate 0):** Sleeve Room_Entry with floor, 4 walls, ceiling, doorway opening(s) aligned to portal anchors. Validate clean activation/deactivation and doorway transition before proceeding.
- **Remaining 4 rooms after Gate 0:** Sleeve Hall_A, Living, Kitchen, Corridor_B with identical approach.
- Room dimensions: walls align with inner faces of existing BoxCollider trigger volumes (size 6). Ceiling height 2.70m. Doorway openings 1.0m wide x 2.1m tall.
- Follow GEOMETRY_GRAMMAR.md rules: separators extend to wall midpoint, separator tops 0.05m above wall tops, internal walls trim inward 0.05m.
- Default URP lit material only. No textures, no lighting beyond defaults.

### Phase sequencing

- [~] **Phase 0:** Module 1 (refactor) + Module 2 (migration) + run all tests
- [ ] **Phase 1:** Module 3 (binding wiring) + verify gizmo activation colors return (TD0022)
- [ ] **Phase 2:** Module 4 first room (Entry) + Gate 0 doorway test
- [ ] **Phase 3:** Module 4 remaining rooms
- [ ] **Phase 4:** Validation gate (all tests, full walkthrough, portal sightlines, overlays, multiplayer spot-check)

### Additional decisions from eng review

- **TD0013 (trigger overlap race) is conditional.** If Gate 0 doorway test reproduces the race, promote to sprint scope. Otherwise stays S2.
- **Portal sightlines in S1C are adjacency-based.** NSC currently passes empty portal probes. "Looking through a doorway shows the connected room" works via 1-hop adjacency activation, not true line-of-sight. This is correct for S1C.
- **Scene controller references remain discovery-based.** House_Prototype NodeStreamingController uses `FindAnyObjectByType` at runtime (S1B design decision, commit c9fe81a). Not changing this.
- **TODO: Add in-game pause/quit path** for builds (Application.Quit). Not S1C scope but captured as TD0023 for pre-demo landing.

## Testing Decisions

### What makes a good test here

Tests should verify the **observable contract**, not internal wiring. For NodePresentationHandle, the contract is: "SetPresentation toggles the presentation child active state while the root GameObject remains always-active." Tests should not care about how the toggle happens internally, only that the right things are active/inactive afterward.

### Automated tests (EditMode)

**NodePresentationHandle** -- update 4 existing tests + add 2-3 new:
- `SetPresentation_True_ActivatesPresentationChild` (updated from root GO assertion)
- `SetPresentation_False_DeactivatesPresentationChild` (updated from root GO assertion)
- `SetPresentation_RootStaysActive_WhenPresentationDisabled` (NEW, headline invariant)
- `SetPresentation_NullPresentationRoot_WarnsAndDoesNotThrow` (NEW)
- `SetPresentation_AfterDestroy_DoesNotThrow` (existing, verify guard logic still works)
- `NodeId_ExposesSerializedValue` (existing, unchanged)

Prior art: existing `ContractTests.cs` in `Tests/EditMode/NodeActivation/`. Same test file, same patterns.

**All 125+ existing tests** must pass without modification (except the 4 being updated above). This proves the hierarchy refactor does not regress S1A/S1B behavior.

### Manual verification gates (play-mode)

**Gate 0 (after Phase 2):**
- Entry room visible with walls/floor/ceiling
- Walk through Entry doorway into Hall_A -- clean transition, no flicker
- PlayerNodeTracker.CurrentNodeId updates correctly
- If TD0013 trigger race reproducible with visible walls -- promote to scope

**PlayerMotor binding (after Phase 1):**
- Owner player spawns -- rooms activate per resolver (not all-gray gizmos)
- Non-owner player spawns -- does NOT bind (no duplicate binding)
- Host shutdown/disconnect -- stale refs cleared, no NRE on next frame
- Gizmo activation colors return (TD0022 auto-resolves)

**Full validation (Phase 4):**
- All tests pass (125+ existing + ~6 new/updated)
- Full 5-node walkthrough: entry -> hall_a -> living -> kitchen -> corridor_b and back
- Portal sightlines: look through doorway, see adjacent room geometry
- Debug overlays F3 and F4 functional against visible geometry
- ForceAllActive toggle: all 5 rooms visible simultaneously
- Multiplayer spot-check: host + client see consistent activation

### What we do NOT test

- OnValidate behavior (editor-only safety net, not worth test infrastructure)
- ProBuilder geometry compliance (manual visual inspection, not automatable in EditMode)
- Portal line-of-sight visibility (probes are S2 scope; adjacency activation covers S1C)

## Out of Scope

- **TD0013 trigger overlap race** -- conditional promotion only if Gate 0 reproduces it. Otherwise stays S2.
- **TD0004 procedural room builder** -- 5 manual rooms do not justify the tooling investment.
- **Textures, materials, or lighting** beyond default URP lit + default directional light. Atmosphere is M3 scope.
- **New room nodes** beyond the existing 5 (Entry, Hall_A, Living, Kitchen, Corridor_B).
- **Observation lock system** -- S2 scope, not required for visible rooms.
- **Portal probe scene wiring** -- NSC passes empty probes by design. Real probe data is S2/S3.
- **Cross-machine multiplayer testing** -- LAN graybox only. No Relay/Lobby.
- **In-game pause/quit menu** -- captured as separate TODO (TD0023), not S1C scope.

## Further Notes

- **Jam deadline: 2026-06-12.** S1C must be small enough to not delay S2 (observation lock), which is the critical path per ROADMAP.md.
- **"Whoa" moment target:** Portal-style sightlines through impossible geometry (seeing yourself through a loop) is the S3 payoff. S1C makes it visually possible by adding the geometry that S3 loop anomaly will eventually exploit.
- **GEOMETRY_GRAMMAR.md** has been archived to `docs/handoff-prompts/archived/03-sprint-1/02-s1b/GEOMETRY_GRAMMAR.md`. Rules still apply for S1C geometry authoring.
- **Prior learning: `presentation-tracking-separation`** (confidence 10/10) -- the hierarchy refactor was identified as necessary across 3 independent sources (NodePresentationHandle WARNING comment, ARCH.md S1B decision, S1B review finding IF-3).
- **Prior learning: `scene-level-handle-migration`** (confidence 9/10) -- scene-level components from prior sprints must be cleaned up during prefab migration to prevent duplicates.
- **Codex outside voice** reviewed this plan and identified the scene-handle cleanup gap, OnNetworkDespawn gap, and portal probe stub reality. All three were incorporated.
