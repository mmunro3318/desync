# Phasmo-Clone Archaeology & Extraction Report

**Date:** 2026-05-02
**Source repo:** `~/Desktop/Projects/Unity/phasmo-clone/phasmo-clone/`
**Target repo:** `~/Desktop/Projects/Unity/spatial-horror/unity-DESYNC/My project/`
**Old project namespace:** `GhostHunt.*`
**New project namespace:** `Desync.*` (matches the `unity-DESYNC` Unity folder)

## TL;DR

The old "Phasmo-Clone" repo was much smaller and cleaner than its name implies. There is **no actual ghost / evidence / hunt logic** in it — it is essentially a thin networked first-person foundation:

- 9 scripts, ~695 LoC total, all under 130 lines each
- Modular folder layout (`_Project/{Scripts,Scenes,Prefabs,Settings,Art,Audio,Tests}`)
- NGO 2.11 multiplayer scaffolding (host/join lobby, scene load, per-player NetworkObject)
- ScriptableObject-based gameplay tunables (`GameplaySettings`)
- New Input System with a small player action map
- One graybox two-floor house scene + a Bootstrap scene
- One EditMode regression test for NGO bootstrap consistency

Because the codebase was already lightweight and clone-naming was confined to the namespace, the migration carried over almost the full `_Project/` tree with one mechanical change: `GhostHunt.*` → `Desync.*` across all scripts, asmdef, prefabs, and scene YAML.

**Salvage decision summary:** kept ~95% of `_Project/` content; left behind Unity sample assets in the new project, the throwaway `Test.unity` scene, the empty `Editor/` folder, the empty `NuGet` Plugins folder, the local-path `com.coplaydev.unity-mcp` package, and the unused `com.unity.visualscripting` package.

## What the old repo actually contained

```
Assets/
├── _Project/                          ← the only meaningful folder
│   ├── Art/Materials/                 4 graybox materials
│   ├── Audio/                         empty (no .wav assets)
│   ├── Prefabs/
│   │   ├── Environment/Railing_Graybox.prefab
│   │   └── Player/PF_Player.prefab    NetworkObject + motor + look + flashlight + footstep
│   ├── Scenes/
│   │   ├── Bootstrap.unity            NetworkManager + LobbyUI
│   │   ├── House_Graybox.unity        ~115 GameObjects, modular two-floor house
│   │   └── Test.unity                 minimal placeholder (DROPPED)
│   ├── Scripts/                       9 scripts, 695 LoC
│   │   ├── Audio/AmbientAudioManager.cs (111L)
│   │   ├── Audio/FootstepAudio.cs (114L)
│   │   ├── Core/GameBootstrap.cs (66L)
│   │   ├── Core/GameplaySettings.cs (49L)
│   │   ├── Items/FlashlightController.cs (61L)
│   │   ├── Player/PlayerInputRouter.cs (55L)
│   │   ├── Player/PlayerLook.cs (127L)
│   │   ├── Player/PlayerMotor.cs (57L)
│   │   └── UI/LobbyUI.cs (55L)
│   ├── Settings/
│   │   ├── AtmosphereVolumeProfile.asset    URP volume (post-fx mood)
│   │   ├── GameplaySettings.asset           tuned ScriptableObject instance
│   │   └── Input/PlayerInputActions{.inputactions,.cs}
│   └── Tests/EditMode/
│       ├── PhasmoClone.Tests.EditMode.asmdef    (renamed)
│       └── NetworkBootstrapConsistencyTests.cs  (regression for NGO config drift)
├── DefaultNetworkPrefabs.asset        registers PF_Player with NGO
├── Editor/                            EMPTY (LEFT BEHIND)
├── Plugins/NuGet/                     EMPTY (LEFT BEHIND)
├── Scenes/                            top-level — just default
├── Screenshots/                       debug captures (LEFT BEHIND)
├── Settings/                          URP renderer + RP assets (used new project's defaults)
├── TextMesh Pro/                      auto-imports on first TMP usage (LEFT BEHIND)
└── _Recovery/                         Unity recovery folder (LEFT BEHIND)
```

## Bucket assessments

### Bucket A — copied directly (verbatim minus namespace rename)

| Item | Why kept |
|---|---|
| `Scripts/Audio/AmbientAudioManager.cs` | Singleton + drone loop + procedural sine fallback. Useful pattern for the eventual "atmosphere" bed; trivially small. |
| `Scripts/Audio/FootstepAudio.cs` | Owner-local trigger + ClientRpc broadcast — clean NGO audio pattern. Bound to CharacterController; will need a refactor when "observation lock" players become non-moving observers, but kept as-is for now. |
| `Scripts/Core/GameBootstrap.cs` | Minimal host/client entry. Hardcoded port `7777` and scene name `House_Graybox` — both are config concerns, not code smells. |
| `Scripts/Core/GameplaySettings.cs` | ScriptableObject of player + audio knobs with `[Tooltip]`s — exactly the runtime-vs-definition separation the new docs want. |
| `Scripts/Items/FlashlightController.cs` | Owner-input → `NetworkVariable<bool>` → all-clients mirror. Small, clean, generalizes to any toggleable held item. |
| `Scripts/Player/PlayerInputRouter.cs` | New Input System dispatch into events the motor + flashlight subscribe to. No clone-specific logic. |
| `Scripts/Player/PlayerLook.cs` | First-person camera with `NetworkVariable<float>` pitch sync. Worth refactoring later (see Bucket B note). |
| `Scripts/Player/PlayerMotor.cs` | CharacterController walk/sprint/gravity. |
| `Scripts/UI/LobbyUI.cs` | Host/Join/IP UI. Good enough for graybox testing. |
| `Tests/EditMode/NetworkBootstrapConsistencyTests.cs` | Behavioral regression test for the historical TD0002 `ConnectionApprovalCallback ≠ null ⟹ ConnectionApproval = true` bug. Excellent pattern to keep as a template. |
| `Settings/Input/PlayerInputActions.{inputactions,cs}` | Move/Look/Sprint/ToggleFlashlight — clean baseline. Will be extended for portal/observation interactions. |
| `Settings/GameplaySettings.asset` | Tuned values are a useful starting point. |
| `Settings/AtmosphereVolumeProfile.asset` | Custom URP Volume profile — mood-tuning baseline. |
| `Prefabs/Player/PF_Player.prefab` | Composes the player from Motor + Look + Input + Flashlight + Footstep + NetworkObject. |
| `Prefabs/Environment/Railing_Graybox.prefab` | Trivial mesh prefab; reusable. |
| `Art/Materials/MAT_{Ceiling,Floor_Dark,Stairs,Walls}.mat` | Graybox material set; replace later but useful for first-pass scenes. |
| `Scenes/Bootstrap.unity` | Lobby/networking entry scene. |
| `Scenes/House_Graybox.unity` | Two-floor modular house — useful as the first graybox testing target while the house-graph runtime is built. **Carries the known floor-to-floor light leak risk** (see §Lighting). |
| `DefaultNetworkPrefabs.asset` | NGO network-prefab registry referencing `PF_Player`. |

### Bucket B — kept, refactor flagged

These were copied because the cost of refactor-on-import is higher than "rename namespace, then refactor when the system around them is built." Each has a follow-up note for the next implementation pass.

- **`PlayerLook.cs`** — contains a "destroy scene main camera" branch that only makes sense for a clone-style spawn flow. Re-evaluate once the spatial-horror spawn flow is defined; in particular, the new observation-lock model may want non-owner cameras kept around for spectator/observation views.
- **`FootstepAudio.cs`** — tightly coupled to the player motor. When non-player footsteps or "phantom" footsteps appear (a likely spatial-horror feature), split into a generic `NetworkedFootstepEmitter` + a movement-driver adapter.
- **`GameBootstrap.cs`** — hardcoded scene name and port. Promote to the `Settings/GameplaySettings.asset` (or a new `NetworkSettings` SO) when the scene-load flow grows.
- **`House_Graybox.unity`** — see §Lighting and §Geometry. Geometry is fine for graybox; lighting/seal authoring should be re-validated before being treated as ground truth.
- **`PF_Player.prefab`** — `FlashlightController` and `FootstepAudio` should not stay welded to the player root once a generic item-attach / audio-emitter system exists.

### Bucket C — summarized only (not copied)

Nothing met this bar. The codebase was small enough that "summarize but don't copy" was rarely the right call — every script either belonged in A/B or was already absent. The closest case is the `GameBootstrap`'s implicit assumption that the host is also the gameplay scene loader; that is not copied as a "rule," but it is documented here so the new architecture can decide whether to keep, generalize, or replace it (e.g., a server-only authority that loads scenes independent of host topology).

### Bucket D — left behind

| Item | Reason |
|---|---|
| `Assets/Editor/` | Empty folder. |
| `Assets/Plugins/NuGet/` | Empty / not in use. |
| `Assets/Screenshots/` | Local debug captures, not source. |
| `Assets/_Recovery/` | Unity auto-recovery folder. Never copy. |
| `Assets/TextMesh Pro/` | Re-imports automatically on first TMP usage. |
| `Assets/Scenes/` (top-level) | Only contained Unity sample scaffolding; the meaningful scenes live in `_Project/Scenes/`. |
| `_Project/Scenes/Test.unity` | Placeholder scene; no purpose. |
| `Packages: com.coplaydev.unity-mcp` (file path) | Local-machine path to a beta download; not portable, and unrelated to the spatial-horror runtime. Re-add manually if MCP integration is desired. |
| `Packages: com.unity.visualscripting` | Not used; explicitly removed from the new manifest. |
| `Packages: com.unity.netcode.gameobjects 2.11.0` | KEPT — added to the new project's manifest. |
| `Packages: com.unity.multiplayer.playmode 2.0.2` | KEPT — added to the new project's manifest (multi-instance Editor testing). |
| `Packages: com.unity.probuilder 6.0.9` | KEPT — added to the new project's manifest (graybox modeling). |
| Default Unity URP template's `PC_Renderer.asset`, `PC_RPAsset.asset`, `DefaultVolumeProfile.asset`, etc. | Already present in the new project as template defaults; not overwritten. The old project's tuned `AtmosphereVolumeProfile.asset` was carried into `_Project/Settings/`. |
| New project's `Readme.asset`, `TutorialInfo/`, `InputSystem_Actions.inputactions`, `Scenes/SampleScene.unity` | DELETED — Unity URP template scaffolding, not relevant. |

## Networking review

**Stack identified:** Netcode for GameObjects (NGO) **2.11.0**, Unity Transport (UTP), host/client topology, scene management enabled, single `DefaultNetworkPrefabs.asset` registering `PF_Player`.

**What is structurally useful and was kept:**
- `GameBootstrap` host/client entry pattern.
- `LobbyUI` + IP-string flow as a graybox harness.
- `NetworkVariable<T>` usage in `PlayerLook` (pitch sync) and `FlashlightController` (toggle state).
- `ClientRpc`-broadcast footstep pattern in `FootstepAudio`.
- The `NetworkBootstrapConsistencyTests` regression — directly useful as a template for any future "did the NGO config drift?" tests.

**What is incomplete or misleading:**
- No client prediction, no reconciliation, no lag compensation. All movement is owner-authoritative trust. This is a *test harness*, not a multiplayer foundation.
- The hardcoded `7777` port and the hardcoded `"House_Graybox"` scene mean this only works for one well-known route.
- Scene transitions assume host = scene authority; this couples networking topology to gameplay flow.
- `LobbyUI` writes the IP into the transport before `StartClient` — fine for LAN-only, brittle for any NAT/relay situation.

**Likely cause of "doesn't work cross-computer":** with raw UTP and direct IP entry, anything outside the same LAN segment will fail without port-forwarding or a relay (Unity Relay / Lobby service). The local-only "virtual player" mode hides this because it loops back over the host process. Nothing in the carried-forward code addresses this gap; the user-facing fix is a relay/lobby integration, not a code change to the existing scripts.

**Carry-forward stance for networking:** the old code is treated as *graybox-grade* — useful enough to host two players in the same room for early validation, not trustworthy as a production multiplayer foundation. The Unity research docs (`04-ngo-multiplayer-architecture-report.md`) should be the source of truth for the production model, and the carried-forward scripts will likely be partially or fully replaced as the spatial runtime contracts (`networked-house-runtime-interfaces-contracts.md`) come online.

## Lighting & geometry review

**Geometry:** the `House_Graybox` scene contains ~115 GameObjects organized as modular wall/floor/ceiling pieces grouped by room, plus 4 point lights, stair steps, railings, and 3 spawn points. Materials are 4 graybox `.mat` files. ProBuilder is in the dependency list, suggesting some of the geometry is ProBuilder-authored.

**Light-leak hypothesis:** the most likely cause of the floor-to-floor leak is a combination of:
1. Modular ceiling/floor pieces meeting at seams without overlap, allowing real-time point-light influence to bleed through the gap.
2. No baked lighting; everything is real-time, so shadow leakage at seams is amplified by point lights with large attenuation ranges.
3. No "second-floor wall skirts" sealing the perimeter where the upper floor meets the upper exterior walls.

This was **not fixed in-place** in the old repo, and the geometry was carried forward as-is. Recommended next steps once the new repo is live in Unity:
- Open `House_Graybox.unity`, enable the *Light Influence* gizmo, and visually confirm the leak source.
- Either: (a) add overlap geometry or sealing planes between floors, (b) reduce point-light range and place per-floor lights, or (c) rebuild the upper floor as a single sealed shell.
- See `docs/design/03-systems/lighting-and-visibility-spec.md` and `docs/design/98-unity-research/03-unity-urp-graphics-lighting-horror-report.md` for the new project's intended lighting posture before authoring fixes.

## What was changed during import

1. **Namespace rename** — `GhostHunt.*` → `Desync.*` across all `.cs`, `.asmdef`, `.prefab`, `.unity`, and `.asset` files (the `.prefab`/`.unity` updates are to `m_EditorClassIdentifier` hint strings; Unity-managed GUIDs were not touched, so prefab and scene component bindings are preserved).
2. **Test asmdef rename** — `PhasmoClone.Tests.EditMode.asmdef` → `Desync.Tests.EditMode.asmdef`, with `name` and `rootNamespace` fields updated to match.
3. **Manifest changes** in `Packages/manifest.json`:
   - Added: `com.unity.netcode.gameobjects 2.11.0`, `com.unity.multiplayer.playmode 2.0.2`, `com.unity.probuilder 6.0.9`.
   - Removed: `com.unity.visualscripting 1.9.10` (unused in old repo, no use planned in new repo).
   - **Not added:** `com.coplaydev.unity-mcp` — old repo referenced a local file path; intentionally omitted to keep the new repo machine-portable.
4. **Sample assets stripped from the new project**: deleted `Assets/Readme.asset`, `Assets/TutorialInfo/`, `Assets/InputSystem_Actions.inputactions`, `Assets/Scenes/SampleScene.unity`.
5. **Dropped `Test.unity`** from the carried scenes set.

No script bodies were edited beyond the namespace rename. Refactoring of clone-shaped patterns (e.g., `PlayerLook` camera-destroy branch, `FootstepAudio` coupling) is deferred until the surrounding spatial-horror systems are in place.

## Known risks carried forward

1. **Networking is graybox-grade only.** Cross-machine multiplayer is not solved by the carried code; it requires a relay or LAN-with-port-forwarding setup. Treat the bootstrap as a local-test harness.
2. **Light leak between floors** in `House_Graybox.unity` is unresolved.
3. **`PlayerLook.cs` contains a clone-era branch** (destroy scene main camera) that may or may not match the new spawn flow — re-evaluate before wiring to spawn logic.
4. **`PF_Player.prefab` welds the flashlight + footstep components to the player root** — fine for now, but blocks a generic "held item" or "audio emitter" system if not refactored before feature work expands.
5. **`GameBootstrap` hardcodes the scene name and transport port** — fine for graybox, brittle for anything else.

## Recommended next implementation steps

The carried-forward foundation gets the new repo to *"two players can connect on a LAN, walk around a two-floor graybox house with a flashlight, and hear footsteps."* That maps to the **prerequisites for** but not the substance of the first impossible-house vertical slice. Suggested order:

1. Open the project in Unity, let it import packages, verify `Bootstrap.unity` plays and a host can spawn into `House_Graybox.unity`.
2. Run the `Desync.Tests.EditMode` suite to confirm the NGO bootstrap regression test still passes.
3. Validate the light-leak hypothesis in-Editor; document the actual cause; either fix or formally document as known.
4. Begin the spatial-runtime work per `docs/design/02-architecture/spatial-runtime-framework.md` and the `house-graph-core-epic.md` roadmap. Treat the carried player scripts as fixtures the new runtime *uses*, not as systems the new runtime is *built around*.
5. Plan the relay/lobby integration (Unity Relay, Lobby, or alternative) before treating cross-machine multiplayer as a real target.

## Style + architecture compliance check

| Guardrail (from prompt) | Status |
|---|---|
| Runtime state separated from definitions | ✅ `GameplaySettings` is a SO (definition); per-instance state lives on the components (runtime). |
| Thin scene objects | ✅ Player components are small and single-purpose. |
| No giant manager gods | ✅ `AmbientAudioManager` is a singleton but only ~111 lines and one responsibility. |
| Debug-first hidden-state | ⚠️ Not present yet — old repo had no debug overlay. To be built fresh per `docs/design/05-debug-and-testing/networked-house-debug-overlay-spec.md`. |
| Graybox-first | ✅ Materials, prefabs, and house scene are all graybox-grade. |
| Modular, narrow ownership | ✅ Folder-per-domain layout already matches new architecture intent. |
| Vertical-slice-first | ✅ Carried surface is small enough that a vertical slice can be built on top. |
