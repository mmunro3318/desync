# Phasmo-Clone → Spatial-Horror Carry-Forward Manifest

**Date:** 2026-05-02
**Source root:** `~/Desktop/Projects/Unity/phasmo-clone/phasmo-clone/`
**Target root:** `~/Desktop/Projects/Unity/spatial-horror/unity-DESYNC/My project/`
**Companion doc:** `docs/05-migration/phasmo-clone-archaeology-and-extraction-report.md`

Paths below are relative to each project's root. Bucket A = copied (verbatim minus namespace), Bucket B = copied with refactor flagged for follow-up.

## Scripts (`_Project/Scripts/`)

| Source | Destination | Bucket | Treatment | Why kept |
|---|---|---|---|---|
| `Assets/_Project/Scripts/Audio/AmbientAudioManager.cs` | `Assets/_Project/Scripts/Audio/AmbientAudioManager.cs` | A | Copied; namespace `GhostHunt.Audio` → `Desync.Audio` | Singleton drone + procedural fallback — useful pattern for the atmosphere bed. |
| `Assets/_Project/Scripts/Audio/FootstepAudio.cs` | `Assets/_Project/Scripts/Audio/FootstepAudio.cs` | B | Copied; namespace renamed | NGO audio broadcast pattern is good; coupling to the player motor needs to be split before non-player footsteps exist. |
| `Assets/_Project/Scripts/Core/GameBootstrap.cs` | `Assets/_Project/Scripts/Core/GameBootstrap.cs` | B | Copied; namespace renamed | Host/client entry; hardcoded scene name + port `7777` to be promoted into a settings SO later. |
| `Assets/_Project/Scripts/Core/GameplaySettings.cs` | `Assets/_Project/Scripts/Core/GameplaySettings.cs` | A | Copied; namespace renamed | ScriptableObject of player + audio knobs — exactly the runtime-vs-definition split the new docs want. |
| `Assets/_Project/Scripts/Items/FlashlightController.cs` | `Assets/_Project/Scripts/Items/FlashlightController.cs` | A | Copied; namespace renamed | Owner-input → `NetworkVariable<bool>` → mirror; clean, generalizes to any toggleable item. |
| `Assets/_Project/Scripts/Player/PlayerInputRouter.cs` | `Assets/_Project/Scripts/Player/PlayerInputRouter.cs` | A | Copied; namespace renamed | New Input System dispatch; no clone-specific assumptions. |
| `Assets/_Project/Scripts/Player/PlayerLook.cs` | `Assets/_Project/Scripts/Player/PlayerLook.cs` | B | Copied; namespace renamed | First-person camera + `NetworkVariable<float>` pitch sync. Contains a "destroy scene main camera" branch to revisit when the spawn flow is defined. |
| `Assets/_Project/Scripts/Player/PlayerMotor.cs` | `Assets/_Project/Scripts/Player/PlayerMotor.cs` | A | Copied; namespace renamed | CharacterController walk/sprint/gravity, driven by `GameplaySettings`. |
| `Assets/_Project/Scripts/UI/LobbyUI.cs` | `Assets/_Project/Scripts/UI/LobbyUI.cs` | A | Copied; namespace renamed | Host/Join/IP UI for graybox sessions. |

## Tests (`_Project/Tests/`)

| Source | Destination | Bucket | Treatment | Why kept |
|---|---|---|---|---|
| `Assets/_Project/Tests/EditMode/NetworkBootstrapConsistencyTests.cs` | `Assets/_Project/Tests/EditMode/NetworkBootstrapConsistencyTests.cs` | A | Copied; namespace renamed | Behavioral regression for NGO `ConnectionApprovalCallback`/`ConnectionApproval` consistency — also serves as a template for further "did the NGO config drift?" tests. |
| `Assets/_Project/Tests/EditMode/PhasmoClone.Tests.EditMode.asmdef` | `Assets/_Project/Tests/EditMode/Desync.Tests.EditMode.asmdef` | A | **Renamed** file + `name`/`rootNamespace` fields (`PhasmoClone` → `Desync`) | Required for the test to load under the renamed namespace. |

## Settings (`_Project/Settings/`)

| Source | Destination | Bucket | Treatment | Why kept |
|---|---|---|---|---|
| `Assets/_Project/Settings/AtmosphereVolumeProfile.asset` | `Assets/_Project/Settings/AtmosphereVolumeProfile.asset` | A | Copied verbatim | Tuned URP Volume profile — mood baseline. |
| `Assets/_Project/Settings/GameplaySettings.asset` | `Assets/_Project/Settings/GameplaySettings.asset` | A | Copied verbatim | Tuned ScriptableObject instance of `GameplaySettings`. |
| `Assets/_Project/Settings/Input/PlayerInputActions.inputactions` | `Assets/_Project/Settings/Input/PlayerInputActions.inputactions` | A | Copied verbatim | Move/Look/Sprint/ToggleFlashlight bindings. |
| `Assets/_Project/Settings/Input/PlayerInputActions.cs` | `Assets/_Project/Settings/Input/PlayerInputActions.cs` | A | Copied verbatim (auto-generated wrapper) | Strongly-typed input actions wrapper. |

## Prefabs (`_Project/Prefabs/`)

| Source | Destination | Bucket | Treatment | Why kept |
|---|---|---|---|---|
| `Assets/_Project/Prefabs/Player/PF_Player.prefab` | `Assets/_Project/Prefabs/Player/PF_Player.prefab` | B | Copied; `m_EditorClassIdentifier` hint strings updated for namespace rename | Composes Motor + Look + Input + Flashlight + Footstep + NetworkObject. Splitting flashlight/footstep off the player root is a follow-up. |
| `Assets/_Project/Prefabs/Environment/Railing_Graybox.prefab` | `Assets/_Project/Prefabs/Environment/Railing_Graybox.prefab` | A | Copied verbatim | Trivial mesh prefab. |

## Scenes (`_Project/Scenes/`)

| Source | Destination | Bucket | Treatment | Why kept |
|---|---|---|---|---|
| `Assets/_Project/Scenes/Bootstrap.unity` | `Assets/_Project/Scenes/Bootstrap.unity` | A | Copied; YAML `m_EditorClassIdentifier` updated for namespace rename | Lobby + NetworkManager entry. |
| `Assets/_Project/Scenes/House_Graybox.unity` | `Assets/_Project/Scenes/House_Graybox.unity` | B | Copied; YAML `m_EditorClassIdentifier` updated for namespace rename | Two-floor modular graybox house. **Carries known light-leak risk between floors** — see archaeology report §Lighting. |
| `Assets/_Project/Scenes/Test.unity` | *(not copied)* | D | DROPPED | Placeholder scene with no value. |

## Art (`_Project/Art/`)

| Source | Destination | Bucket | Treatment | Why kept |
|---|---|---|---|---|
| `Assets/_Project/Art/Materials/MAT_Ceiling.mat` | `Assets/_Project/Art/Materials/MAT_Ceiling.mat` | A | Copied verbatim | Graybox material. |
| `Assets/_Project/Art/Materials/MAT_Floor_Dark.mat` | `Assets/_Project/Art/Materials/MAT_Floor_Dark.mat` | A | Copied verbatim | Graybox material. |
| `Assets/_Project/Art/Materials/MAT_Stairs.mat` | `Assets/_Project/Art/Materials/MAT_Stairs.mat` | A | Copied verbatim | Graybox material. |
| `Assets/_Project/Art/Materials/MAT_Walls.mat` | `Assets/_Project/Art/Materials/MAT_Walls.mat` | A | Copied verbatim | Graybox material. |

## Top-level assets

| Source | Destination | Bucket | Treatment | Why kept |
|---|---|---|---|---|
| `Assets/DefaultNetworkPrefabs.asset` | `Assets/DefaultNetworkPrefabs.asset` | A | Copied verbatim | NGO network-prefab registry referencing `PF_Player`. |

## Packages (`Packages/manifest.json`)

| Package | Bucket | Treatment | Why |
|---|---|---|---|
| `com.unity.netcode.gameobjects` 2.11.0 | A | **Added** to new manifest | Required by all carried networking code + tests. |
| `com.unity.multiplayer.playmode` 2.0.2 | A | **Added** to new manifest | Multi-instance Editor testing for graybox co-op sessions. |
| `com.unity.probuilder` 6.0.9 | A | **Added** to new manifest | Used by the carried `House_Graybox.unity` geometry; needed for graybox iteration. |
| `com.unity.visualscripting` 1.9.10 | D | **Removed** from new manifest | Unused in old repo, no plans for visual scripting in new repo. |
| `com.coplaydev.unity-mcp` (file:…) | D | **Not added** | Old repo referenced a local-machine path; not portable. Re-add manually if MCP integration is desired. |
| All other shared packages (URP, Input System, Test Framework, etc.) | — | Already present in new project | No action needed. |

## Items deliberately left behind

| Source | Bucket | Why excluded |
|---|---|---|
| `Assets/Editor/` | D | Empty folder. |
| `Assets/Plugins/NuGet/` | D | Empty / not used. |
| `Assets/Screenshots/` | D | Local debug captures, not source. |
| `Assets/_Recovery/` | D | Unity recovery folder — must never be copied. |
| `Assets/TextMesh Pro/` | D | Auto-imports on first TMP usage. |
| `Assets/Scenes/` (top-level, old repo) | D | Sample scaffolding; meaningful scenes live in `_Project/Scenes/`. |
| `Assets/Settings/` (old repo's URP renderer/RP assets) | D | New project already has equivalent URP-template defaults; not overwritten to avoid silently changing render settings. |
| New project: `Assets/Readme.asset`, `Assets/TutorialInfo/`, `Assets/InputSystem_Actions.inputactions`, `Assets/Scenes/SampleScene.unity` | D | Unity URP-template scaffolding; deleted from the new project to avoid clutter and conflict with the carried input asset. |
| Old `ProjectSettings/*` | D | Not touched. New project's freshly-initialized `ProjectSettings/` was kept as-is to avoid silently importing untracked physics layers, tag manager, graphics settings, etc. Specific settings can be promoted later if needed. |

## Summary counts

- Scripts copied: **9** (all under `Desync.*` namespace)
- Tests copied: **1** + 1 renamed asmdef
- Prefabs copied: **2**
- Scenes copied: **2** (1 dropped)
- Materials copied: **4**
- Setting assets copied: **3** (custom volume profile, gameplay settings instance, input actions)
- Top-level network assets copied: **1**
- Packages added to manifest: **3** (NGO, multiplayer-playmode, ProBuilder)
- Packages removed from manifest: **1** (visualscripting)
- Sample assets stripped from new project: **4** (Readme, TutorialInfo, default InputSystem_Actions, SampleScene)
