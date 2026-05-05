# ARCH — Key Architectural Decisions

This is the **load-bearing decision log** for DESYNC. Record every decision here that future AI agents (or future Mike) might otherwise re-litigate, and explain *why* — the why is the part that prevents drift.

This file supersedes the prior `OLD_ARCHITECTURAL_DECISIONS.md` (Phasmo-Clone era). Decisions below are either **carried forward** (still load-bearing for DESYNC) or **new** (DESYNC-specific).

### Records of decisions go here, not into commit messages alone
**What:** Any decision a future agent might re-litigate gets an entry in this file with a clear *what / why / how to apply / future review*.
**Why:** Commit messages are cheap to lose; a decision log is the artifact AI agents can re-read at session start.
**How to apply:** When reviewing a PR, if a non-obvious choice was made and not captured here, push back and ask for an `ARCH.md` entry before approving.

How to use this doc:
- Before changing a decision recorded here, read it. If you still want to change it, add a new dated entry that supersedes the old one — do not delete the old reasoning.
- Decisions about *Unity engine choices* (URP defaults, NGO authority model, etc.) live in `docs/design/98-unity-research/`. This doc records project-specific decisions that go beyond the engine defaults.

---

## Stack

- **Unity 6, URP 17.4.0, C#.** Confirmed in `unity-DESYNC/Packages/manifest.json`. URP chosen over HDRP for jam-scope tractability and NGO/MPM compatibility.
- **Netcode for GameObjects (NGO) 2.11.2.** First-party multiplayer, intended Relay/Lobby integration path. Mirror/FishNet ruled out for official-support and longevity reasons.
- **New Input System 1.19.0** with the generated C# wrapper from `PlayerInputActions.inputactions`. Type-safe action references; no legacy `Input.GetAxis`.
- **ProBuilder 6.0.9** for graybox geometry. Replaced by authored assets later.
- **Multiplayer Play Mode 2.0.2** for in-Editor multi-instance testing.

---

## Design philosophy

- **Multiplayer-first.** Every player-facing `MonoBehaviour` that owns gameplay state extends `NetworkBehaviour` with explicit `if (!IsOwner) return;` / `if (!IsServer) return;` guards. No system assumes a single local source of truth.
- **Data-driven content.** Tunable values and content definitions live on ScriptableObjects (`GameplaySettings.asset` is the existing pattern). Adding new content = new asset + new field, not new code.
- **Namespaces from day one.** All first-party code is under `Desync.*` (`Desync.Core`, `Desync.Player`, `Desync.Items`, `Desync.Audio`, `Desync.UI`, future system namespaces). Tests under `Desync.Tests.*`.
- **Thin scene objects.** Logic lives in reusable components, not scene-specific scripts. Scene `MonoBehaviour`s are wiring/composition only.
- **Deep modules.** New systems are organized as self-contained modules with a small public interface and complex hidden internals. See `docs/DEEP_MODULES_SPEC.md` for the canonical pattern; `CLAUDE.md` enforces it.
- **Vertical-slice-first.** Build the smallest end-to-end playable thing, then deepen. See `docs/design/05-debug-and-testing/impossible-house-graybox-vertical-slice-plan.md`.

---

## Carried-forward decisions (from migration)

These decisions originated in the Phasmo-Clone era but remain load-bearing because the code carrying them was migrated into `unity-DESYNC/`. Source: `docs/handoff-prompts/current/01-migration/work-done/phasmo-clone-carry-forward-manifest.md`.

### Client-authoritative player movement (`ClientNetworkTransform`)
**What:** Player position synced client-authoritative, not server-authoritative.
**Why:** Co-op PvE with no anti-cheat needs. Simpler and more responsive (no input prediction needed).
**Future review:** Revisit if PvP or competitive modes are added — switching to server-auth requires client-side prediction.

### Camera pitch via `NetworkVariable<float>`
**What:** `PlayerLook` stores pitch in a `NetworkVariable<float>` and remote clients rotate the `CameraRoot` child transform from it.
**Why:** `ClientNetworkTransform` only syncs the root transform (position + yaw). Without the explicit pitch sync, a remote player's flashlight aim would diverge from what the owner sees.
**Future review:** If more child transforms need syncing (head bob, lean), consider a custom `NetworkBehaviour` that batches them.

### `GameBootstrap` is a plain `MonoBehaviour`, not a `NetworkBehaviour`
**What:** Configures and starts `NetworkManager` but has no network identity.
**Why:** It runs in the lobby phase before any network session exists. Adding a `NetworkObject` would serve no purpose.

### Defensive scene loading in `GameBootstrap`
**What:** `StartHost()` validates both the `NetworkManager.StartHost()` return value and that the gameplay scene exists in build settings before calling `LoadScene`. Fires `OnHostStartFailed` so the UI can recover.
**Why:** NGO's `SceneManager.LoadScene` fails silently on a missing scene, leaving the UI stuck. This is a permanent safety pattern — do not strip the validation when "simplifying."

### Bootstrap scene unloads on gameplay load
**What:** Bootstrap unloads when the gameplay scene loads via `NetworkManager.SceneManager.LoadScene()`. `NetworkManager` persists via `DontDestroyOnLoad`.
**Why:** Standard NGO pattern; no reason to keep lobby UI loaded during gameplay.

### Networked footstep audio (`FootstepAudio` is a `NetworkBehaviour`)
**What:** Owner plays 2D audio; remote clients receive a `ClientRpc` and play 3D spatial audio.
**Why:** Hearing other players' footsteps is core to horror atmosphere — "was that Kayden, or something else?"
**Carry-forward note:** Per the migration manifest this is **Bucket B** — the player-coupling needs to be split before non-player footsteps exist.

### Ambient audio is client-local (`AmbientAudioManager` is a plain `MonoBehaviour`)
**What:** Each client generates its own drone and one-shot sounds independently.
**Why:** Sync would add bandwidth for zero gameplay benefit; slightly different random creaks per player is more immersive, not less.
**Future review:** When ambient audio needs to react to Entity state, the Entity system will trigger it via local events — still client-local.

### `FlashlightController` lives in `Desync.Items` (not `Desync.Player`)
**What:** The flashlight is in the Items namespace despite being permanently attached to the player today.
**Why:** Design intent is for it to become a pickup/ground item; the namespace pre-positions for that without a future move.

### `GameplaySettings` ScriptableObject as the single tuning surface
**What:** One ScriptableObject (`Assets/_Project/Settings/GameplaySettings.asset`) holds movement, look, flashlight, and audio knobs; player and item scripts reference it instead of carrying their own `[SerializeField]` fields.
**Why:** One asset to tweak feel; trivial to clone for variants ("Debug_FastMove"). Concrete instantiation of the *runtime state vs. definition* rule.
**Future review:** If it grows past ~30 fields, split into category-specific SOs (`MovementSettings`, `FlashlightSettings`, etc.).

### URP Volume drives global atmosphere
**What:** Atmosphere is configured on `Assets/_Project/Settings/AtmosphereVolumeProfile.asset` (Vignette, Color Adjustments, etc.) alongside `RenderSettings` fog.
**Why:** Editing one profile beats hunting per-scene volumes. Entity events can dynamically adjust the profile during anomalies.

### NGO connection-approval consistency (TD0002)
**What:** `NetworkManager.NetworkConfig.ConnectionApproval` and `NetworkManager.ConnectionApprovalCallback` must be consistent. M1 ships with both **disabled** (no lobby auth scope yet). The behavioral regression test `NetworkBootstrapConsistencyTests` enforces this against `Bootstrap.unity`.
**Why:** Past drift where the callback was wired but the flag was off produced a silent NGO warning and confusing connection behavior. Keep the test green; if you intentionally enable approval, decide on the auth model first and update the test.

### NGO hard rules (camera/audio/scene/spawn)
- **Camera/AudioListener gating:** `OnNetworkSpawn` must disable Camera and AudioListener on non-owner instances and explicitly enable them for the owner. The scene's main camera must be **destroyed** (not just disabled) via a `SceneManager.sceneLoaded` callback — players spawn in `Bootstrap` before NGO transitions to the gameplay scene, so any `FindWithTag` in `OnNetworkSpawn` targets the wrong scene's camera. Player camera depth is set to 1 as defense in depth.
- **Scene loading:** Use `NetworkManager.SceneManager.LoadScene()` after the network session starts. Never use raw `SceneManager.LoadScene()` for gameplay scenes — NGO must control scene transitions to sync clients.
- **Player spawning:** `NetworkManager` spawns the player prefab automatically. Spawn-point selection is `GameBootstrap`'s responsibility (round-robin from a list).

### House_Graybox movement geometry (carried with caveats)
**Status:** The Phasmo-era D-series fixes (`stepOffset = 0.05`, ghost-ramp pattern, disabled `GF_Ceiling` collider, repitched `RampCollider`) live inside the migrated `House_Graybox.unity`. They are still load-bearing for movement to work without wedging the capsule in lintels.
**Minimum overhead clearance rule:** `floor_top + capsule_height + stepOffset + skinWidth + 0.10 safety` ≈ **2.25m** interior ceiling for new graybox rooms.
**Caveats:**
- The **floor-to-floor light leak** has been fixed (see *URP lighting: modular graybox floor/ceiling construction* below). The scene is now safe to use as a lighting reference.
- Any new geometry must respect the ceiling rule and the ghost-ramp pattern (visible mesh + invisible sloped collider) for stairs.

---

## DESYNC-specific decisions (post-migration)

### The migrated code is a fixture, not the architectural template
**What:** Carried Phasmo-Clone scripts/scenes/prefabs are kept verbatim (modulo namespace) so the project boots, hosts, and renders. **Do not extend them as if they define the architecture.**
**Why:** The new spatial-horror runtime (house graph, observation lock, mutation, portal, anchor) must be designed against the docs in `docs/design/02-architecture/` and `03-systems/`. Building those systems by attaching to `PlayerLook` or `GameBootstrap` would entrench Phasmo-shaped assumptions that don't match the new design.
**How to apply:** New systems get their own modules. The migrated code is allowed to be edited where the manifest's "Bucket B" notes call for it, but new system architecture is not negotiated against it.

### Local LAN graybox is the only multiplayer claim until Relay/Lobby lands
**What:** `GameBootstrap.StartClient` uses a user-entered IP and port `7777`. There is no Relay, no Lobby, no auth, no NAT traversal. Cross-machine LAN is confirmed working; internet play is not.
**Why:** The migration brought the graybox networking forward but explicitly did not solve cross-machine internet play. Claiming otherwise creates a credibility gap with playtesters.
**How to apply:** Until a Relay/Lobby integration lands, all multiplayer claims in docs, PR descriptions, and playtest invites are scoped to "local LAN graybox." The Windows Firewall caveat must be communicated — UDP-only apps are not auto-prompted, so the built `.exe` must be manually allowed.

### Host binds to `0.0.0.0` (all network interfaces)
**What:** `GameBootstrap` calls `transport.SetConnectionData("0.0.0.0", port, "0.0.0.0")` before `StartHost()`, binding to all interfaces rather than localhost.
**Why:** Binding to `127.0.0.1` (previous behavior) prevented any cross-machine connection. `0.0.0.0` works for any LAN topology without requiring the host to know its own IP.
**How to apply:** Do not revert to a specific IP in `SetConnectionData`. If the host IP ever needs to be surfaced to the UI (for the joining player to type), read it from `Dns.GetHostEntry` — do not hardcode it.

### FootstepAudio uses ServerRpc → ClientRpc relay (owner-initiated effects)
**What:** `FootstepAudio.PlayFootstepRemoteClientRpc()` is sent via a `RequestFootstepServerRpc()` → `PlayFootstepRemoteClientRpc()` relay rather than calling the ClientRpc directly from the owning client.
**Why:** Only the server can send ClientRpcs. Calling a ClientRpc from the owning client triggers an NGO ownership error. This is the standard NGO pattern for effects that originate on the owner and must reach all clients.
**How to apply:** Any new "owner plays something, everyone hears it" pattern must use this relay shape. Direct owner → ClientRpc calls are always wrong.

### CharacterController disabled on non-owner player instances
**What:** `PlayerMotor.OnNetworkSpawn()` calls `GetComponent<CharacterController>().enabled = false` on non-owner instances.
**Why:** An enabled `CharacterController` fights `NetworkTransform` on remote player representations — two systems writing to `Transform.position` in the same frame produces jitter and wrong positions.
**How to apply:** Any physics/movement component on a networked prefab that should only drive the owner must be disabled for non-owners in `OnNetworkSpawn`. Do not rely on `if (!IsOwner) return;` inside `Update` alone — the component must be fully disabled to prevent physics interference.

### Tests are EditMode-first, behavioral, and namespace-portable
**What:** The single existing test (`NetworkBootstrapConsistencyTests`) runs in EditMode and asserts on scene-serialized state, not source strings. The test asmdef is `Desync.Tests.EditMode`.
**Why:** EditMode tests run without entering Play mode (cheap CI-friendly), and behavioral assertions survive refactors that only change identifier names. PlayMode infra is heavier; reach for it only when a behavior genuinely requires it.
**How to apply:** New regression guards should follow the same pattern (open the scene, query the serialized state). PlayMode tests need explicit justification.

### Modular graybox geometry construction grammar (2026-05-04, S0.3) [SUPERSEDES: "How to apply" rules in S0.1 entry below]
**What:** Systematic coplanar-face artifact fix across all of `House_Graybox.unity`. Root cause was IEEE 754 float precision: modular pieces sharing exact face positions caused renderer z-fighting visible as banding artifacts on the building exterior and at floor/ceiling junctions. Canonically called the "light leak" bug/fix (that was the original working hypothesis), but the actual cause was coplanar geometry, not lighting.
**The canonical construction rules are codified in `docs/handoff-prompts/current/GEOMETRY_GRAMMAR.md`.** All new room geometry must follow that grammar. Key rules that supersede the S0.1 "inset to wall inner edges" approach:
- **Separator XZ edges extend to wall MIDPOINT** (R1.3) — 0.075m from outer face for a 0.15m wall. Not just to the inner face; that left a visible gap. Not to the outer face; that caused z-fighting.
- **Separator tops extend 0.05m ABOVE wall tops** (R1.2) — not flush. Walls terminate INTO the separator volume.
- **Internal walls trim INWARD** (R3.1) — ends sit 0.05m inside the exterior wall inner face. Not extending to the outer face (previous approach caused z-fighting at the building exterior).
- **Safety overlap constant is 0.05m everywhere** (R5.3).
**S0.3 scene changes (House_Graybox.unity):**
- 6 horizontal separators: XZ edges extended to wall midpoint (0.075m), tops raised 0.05m above wall tops.
- 17 internal walls: trimmed so exterior-facing ends are 0.05m inside the exterior wall inner face.
- 4 railings: trimmed at exterior walls (per R4.2), bases lowered 0.05m into floor slabs (per R4.1).
- Hall_W3: fixed envelope violation (was extending 1m past building back wall).
**Known limitation:** The grammar is a hand-authored fixture. As house-graph room nodes land (S1A), new geometry will need to follow this grammar or the validator tests will catch violations. A procedural room builder that generates grammar-compliant geometry automatically is tracked as TD0004.
**References:** `docs/handoff-prompts/current/GEOMETRY_GRAMMAR.md` (canonical rules), `docs/handoff-prompts/current/04-geometry-validator-tdd-handoff.md` (TDD seed for EditMode validator), S0.1 entry below (diagnostic history).

### URP lighting: modular graybox floor/ceiling construction (2026-05-04, S0.1) [SUPERSEDED by S0.3 entry above for "How to apply" rules — diagnostic history preserved]
**What:** Floor/ceiling rects in `House_Graybox.unity` must be inset to wall inner edges so their side faces never protrude through exterior walls. The fix also lowered ceilings so their top faces are flush with wall tops, offset SF floors to overlap with ceilings (eliminating coplanar faces), and set `shadowCastingMode = TwoSided` on all floor/ceiling MeshRenderers.
**Root cause:** The Phasmo-Clone graybox floor/ceiling cubes spanned the full building footprint (X[0,14] Z[0,10]), matching or exceeding the exterior wall outer edges. The 0.1m-tall side faces of these cubes were visible from outside — lit by interior point lights, they produced bright horizontal bands on the building exterior. A secondary issue was coplanar faces between GF_Ceiling and SF_Floor at Y=2.7.
**What changed in the scene (S0.1 — partial fix, superseded by S0.3):**
1. All floor/ceiling rects inset to wall inner edges: X[0.15, 13.85], Z[0.15, 9.85] (SF_Floor_A/B/C inset per-piece based on which edges touch exterior walls).
2. GF_Ceiling lowered to Y=2.65 (top=2.70, flush with wall tops). SF_Ceiling lowered to Y=5.35 (top=5.40, flush with SF wall tops).
3. SF_Floor pieces at Y=2.70, overlapping GF_Ceiling by 0.05m — no coplanar faces, no gap.
4. All 6 floor/ceiling MeshRenderers set to `Cast Shadows = Two Sided`.
**Hypotheses tested and ruled out during diagnosis:**
- Shadow bias (zeroed on all lights — no change)
- Directional light bleed (disabled — no change)
- Coplanar Z-fighting alone (moved floors — bands persisted)
- The root cause was geometry protrusion, not lighting configuration.
**Reference:** `docs/handoff-prompts/current/DEBUG-RESEARCH-debugging-light-leak-urp-issue.md` and `docs/design/98-unity-research/03-unity-urp-graphics-lighting-horror-report.md`.

---

## S1A — House Graph Authoring and Runtime Shell (2026-05-04)

These decisions were locked during S1A Session 1 (architecture + plan). Design doc approved, eng review cleared. Cross-model review (Codex) ran twice.

### Room geometry strategy: Prefab rooms, not additive scenes
**What:** Each room node is a prefab instantiated by `SpatialGraphRuntime` at authored positions. Not additive scene loading.
**Why:** For a 5-node authored graph, prefab instantiation is simpler, faster, and avoids the NGO scene-sync complexity of additive scenes. Additive loading becomes relevant when room count exceeds ~15 (memory/loading budget from research report `98-unity-research/08`).
**Future review:** Revisit when room count exceeds 15 or when S3 mutations require swapping room geometry at runtime. The prefab approach supports activation/deactivation cleanly for the "stagecraft" pooling pattern (S1B).

### Portal anchors: Transform + BoxCollider trigger
**What:** Each doorway has a `PortalAnchorAuthoring` component carrying a `Transform` (position/orientation of the passage) and a `BoxCollider` (trigger mode, for crossing detection). Each room has a room-volume `BoxCollider` trigger for `GetNodeForPosition` (player-to-node resolver).
**Why:** Transform gives portal rendering and teleport destination data. BoxCollider trigger gives physics-based crossing detection without raycasts. Room-volume triggers give cheap spatial queries without manual bounds math.
**How to apply:** Portal anchors live on child GameObjects of the room prefab. Room-volume triggers are on the room root. Both are `isTrigger = true`.

### Connector IDs: Stable doorway IDs, not cardinal directions
**What:** Doorway connections use stable string IDs like `door_a`, `door_b` per room. NOT cardinal directions (`north`, `south`, `east`, `west`).
**Why:** Cardinal IDs break when rooms rotate or when a room has multiple exits on the same wall. Stable IDs are rotation-proof and mutation-proof (S3 can rebind `door_a` to a different destination without renaming).
**How to apply:** `HouseEdgeDefinition` references source/target nodes by node ID and source/target anchors by stable anchor ID within their respective rooms.

### Network sync: Full snapshot struct for S1A (temporary)
**What:** Graph runtime state (occupancy, active nodes, topology version) syncs via `NetworkVariable<HouseSnapshot>` — a single struct containing the full state.
**Why:** For 5 nodes, the snapshot is ~50 bytes. Full snapshot is simpler to implement and debug than delta-based sync. Premature optimization toward deltas would add complexity with no measurable benefit at this scale.
**Future review:** **Evolve to delta-based sync in S3** when mutations arrive and the graph may change shape mid-round. The snapshot struct will grow linearly with node count; deltas grow with mutation frequency instead.
**This is explicitly a temporary decision.** Do not treat snapshot sync as the permanent architecture.

### File structure: 9 files, definition structs inlined
**What:** S1A ships ~9 C# files. Definition structs (`HouseNodeDefinition`, `HouseEdgeDefinition`, `PortalAnchorDefinition`) are inlined into `HouseGraphDefinition.cs` rather than separate files.
**Why:** At 5 nodes with simple structs, separate files add navigation overhead for no benefit. The 500 LoC split threshold from `CLAUDE.md` applies — if `HouseGraphDefinition.cs` exceeds 500 LoC, extract structs to their own files.
**File layout:**
- `Scripts/World/Graph/Definitions/HouseGraphDefinition.cs` — SO + inlined structs
- `Scripts/World/Graph/Runtime/SpatialGraphRuntime.cs` — runtime query engine
- `Scripts/World/Graph/Runtime/RuntimeNodeState.cs` — per-node runtime state
- `Scripts/World/Graph/Runtime/RuntimeEdgeState.cs` — per-edge runtime state
- `Scripts/World/Graph/Runtime/PortalResolver.cs` — destination lookup
- `Scripts/World/Graph/Authoring/RoomNodeAuthoring.cs` — scene-to-graph bridge (rooms)
- `Scripts/World/Graph/Authoring/PortalAnchorAuthoring.cs` — scene-to-graph bridge (portals)
- `Scripts/UI/Debug/SpatialDebugOverlay.cs` — IMGUI debug text
- `Scripts/UI/Debug/SpatialDebugGizmos.cs` — in-scene gizmo labels

### Graph population: Hand-authored in Inspector
**What:** The `HouseGraphDefinition` ScriptableObject is populated by hand in the Unity Inspector for S1A's 5-node graph.
**Why:** An editor script that auto-scans `RoomNodeAuthoring` components adds complexity for a 5-node graph. Hand authoring gives direct control and is faster to iterate on during graybox.
**Future review:** When node count exceeds ~20 or when Mike finds Inspector authoring tedious, build an editor tool that scans scene authoring components and populates the SO.

### Debug rendering: IMGUI + OnDrawGizmos
**What:** `SpatialDebugOverlay` uses `OnGUI()` (IMGUI) for overlay text. `SpatialDebugGizmos` uses `OnDrawGizmos()` for in-scene labels and wireframes.
**Why:** IMGUI is the fastest path to a working debug overlay — no canvas setup, no prefabs, no UI Toolkit learning curve. Gizmos are the standard Unity pattern for scene-view debug visualization. Both satisfy the "debug-first for hidden state" architecture rule.
**How to apply:** Debug overlay shows node count, edge count, current node, connected destinations. Gizmos show node positions and edge connections in the Scene view.

### Test strategy: EditMode-first, manual LAN smoke
**What:** All graph logic (queries, validation, initialization) is tested via EditMode tests. Multiplayer validation is a manual 2-player LAN smoke test (10-step checklist from sprint PDD).
**Why:** EditMode tests are cheap, fast, CI-friendly, and sufficient for pure data logic. PlayMode NGO test harness is heavy to set up and flaky — deferred to S3+ when the test infrastructure is worth the investment.
**How to apply:** New graph logic gets an EditMode test. PlayMode tests require explicit justification. The manual smoke checklist is the multiplayer acceptance gate.

### Contract scope: 5 queries + GetNodeForPosition
**What:** S1A implements: `GetNode`, `GetEdge`, `GetConnectedEdges`, `GetDestinationNode`, `GetPortalAnchor`, plus `GetNodeForPosition` (trigger-volume-based player-to-node resolver). The full `IHouseGraphRuntime` interface from the contracts doc is NOT implemented.
**Why:** These 6 operations are what S1B and S2 actually need. Building the full interface now means writing code against requirements that don't exist yet.
**Future review:** Expand the interface when S1B (node activation) or S2 (observation lock) needs additional queries. The runtime's `Dictionary<string,T>` internals make adding queries trivial.

---

## Open questions / decisions deferred

These are the obvious upcoming forks. They are *not* decisions yet — recorded so a future agent recognizes the open state and doesn't silently pick one.

- **Relay / Lobby integration.** Unity Gaming Services Relay vs. self-hosted vs. Steam — not yet chosen. Cross-machine play is blocked until this lands.
- **Observation-lock authority model.** Spec exists (`docs/design/03-systems/co-op-observation-and-sync-rules-spec.md`); concrete NGO ownership shape is not chosen.
- **Persistent post-jam evolution.** Whether the post-jam iteration stays in Unity or moves elsewhere is not decided. Avoid baking deep Unity-isms into module *interfaces* (implementations can be Unity-flavored).

---
