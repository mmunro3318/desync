# ARCH — Key Architectural Decisions

This is the **load-bearing decision log** for DESYNC. Record every decision here that future AI agents (or future Mike) might otherwise re-litigate, and explain *why* — the why is the part that prevents drift.

This file supersedes the prior `OLD_ARCHITECTURAL_DECISIONS.md` (Phasmo-Clone era). Decisions below are either **carried forward** (still load-bearing for DESYNC) or **new** (DESYNC-specific).

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
- The scene also carries the **known floor-to-floor light leak** flagged in the archaeology report. Validate before treating it as a lighting reference.
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

### Records of decisions go here, not into commit messages alone
**What:** Any decision a future agent might re-litigate gets an entry in this file with a clear *what / why / how to apply / future review*.
**Why:** Commit messages are cheap to lose; a decision log is the artifact AI agents can re-read at session start.
**How to apply:** When reviewing a PR, if a non-obvious choice was made and not captured here, push back and ask for an `ARCH.md` entry before approving.

---

## Open questions / decisions deferred

These are the obvious upcoming forks. They are *not* decisions yet — recorded so a future agent recognizes the open state and doesn't silently pick one.

- **Relay / Lobby integration.** Unity Gaming Services Relay vs. self-hosted vs. Steam — not yet chosen. Cross-machine play is blocked until this lands.
- **House graph runtime shape.** `docs/design/02-architecture/house-graph-core-epic.md` defines the intent; no runtime types exist yet.
- **Observation-lock authority model.** Spec exists (`docs/design/03-systems/co-op-observation-and-sync-rules-spec.md`); concrete NGO ownership shape is not chosen.
- **Persistent post-jam evolution.** Whether the post-jam iteration stays in Unity or moves elsewhere is not decided. Avoid baking deep Unity-isms into module *interfaces* (implementations can be Unity-flavored).

---
