# Diagnostic Handoff: Room Geometry Not Rendering in Play Mode

## Problem

When playing `Bootstrap.unity` (which loads `House_Prototype`), the player spawns and controls work, but **no room geometry is visible** — just skybox and ground plane. The S1B node activation system is working correctly (debug overlays confirm `v_entry: Occupied`, `v_hall_a: Adjacent`), but there's nothing to see.

Room geometry IS visible in the Scene view (editor preview) of `House_Prototype.unity`.

## Evidence

Screenshot: `image.png` in repo root shows:
- F3 overlay: `Current Node: v_entry`, 5 nodes, 4 edges — graph is initialized
- F4 overlay: `v_entry: Occupied`, `v_hall_a: Adjacent` — resolver + controller running
- Game view: empty (skybox + flat brown ground)

## What changed (S1B sprint)

Branch: `feat/s1b-shared-contracts` (7 commits on top of main)

1. Added `NodePresentationHandle` component to all 5 `Room_*` GOs in `House_Prototype.unity`
2. Created `VisibilityController` GO with `NodeStreamingController` + `PortalVisibilityController`
3. Created `SpatialVisibilityDebugOverlay` GO
4. `NodeStreamingController.Update()` now calls `SetPresentation(bool)` on handles each frame
5. `SetPresentation(bool)` calls `gameObject.SetActive(active)` on the Room_* root GO

## Hypotheses (ranked by likelihood)

### H1: Room_* GOs have no mesh renderers — they're trigger volumes only
The CLAUDE.md says: "Prefabs/Rooms/Room_*.prefab (x5 with RoomNodeAuthoring + trigger volumes)". The Room_* GOs may contain only `BoxCollider` (trigger) and `RoomNodeAuthoring` — no `MeshRenderer`, no visible geometry. The visible geometry in the Scene view may be separate static objects not parented to the Room_* hierarchy.

**Check:** In House_Prototype Scene view, inspect Room_Entry's children. Do any have `MeshRenderer`? Or is the visible geometry (walls, floors) separate root-level GOs?

### H2: Room_* GO deactivation hides trigger volumes → PlayerNodeTracker can't fire
If `SetPresentation(false)` deactivates a Room_* GO, its `BoxCollider` trigger goes inactive. If the player hasn't entered a room trigger yet, `PlayerNodeTracker.CurrentNodeId` stays null. The controller skips the frame (`if string.IsNullOrEmpty return`). Chicken-and-egg.

**Against:** The F3 overlay shows `Current Node: v_entry`, so the tracker DID detect entry. This hypothesis is currently disproven by the evidence, but could become an issue if the player spawns outside all triggers.

### H3: Camera is positioned outside/above the room geometry
The player spawns via NGO at the `PF_Player` prefab's default position. If the room geometry exists but is at a different Y level or world position than the player spawn, the player sees sky/ground instead of walls.

**Check:** Compare player spawn position vs Room_Entry world position and bounds.

### H4: SetActive toggling is rapid/flickering
If `SetPresentation` is called every frame and something causes the active state to toggle rapidly, the renderer might not have time to draw. Unlikely given the stable F4 output.

### H5: Bootstrap scene loads House_Prototype additively — geometry from House_Prototype may not be at expected position
The scene loading chain is `Bootstrap.unity` → loads `House_Prototype.unity` additively. If there's a position offset or the loaded scene's objects don't align with the player spawn position, geometry could be far away.

**Check:** In play mode, check Scene view (not Game view) — are the room objects visible? Where is the player relative to them?

## Diagnostic Steps

1. **Enter Play mode from Bootstrap.unity**
2. **Switch to Scene view** (not Game view) — can you see the Room_* GOs? Where is the player camera relative to them?
3. **Select Room_Entry in hierarchy during Play** — is it active? Does it have children with MeshRenderers?
4. **Check if room geometry is separate from Room_* GOs** — are there other root-level GOs in House_Prototype with mesh renderers (walls, floors, etc.) that aren't parented under Room_*?
5. **If geometry IS separate:** that's the root cause. `SetPresentation` toggles Room_* GOs but not the actual visible geometry. The fix would be to either parent the geometry under Room_* GOs, or change the presentation handle to target the geometry GOs instead.

## Files to inspect

- `unity-DESYNC/Assets/_Project/Scenes/House_Prototype.unity` — scene hierarchy, what's parented where
- `unity-DESYNC/Assets/_Project/Prefabs/Rooms/Room_*.prefab` — what components are on the prefabs
- `unity-DESYNC/Assets/_Project/Scripts/World/Graph/Runtime/NodePresentationHandle.cs` — the SetPresentation logic
- `unity-DESYNC/Assets/_Project/Scripts/World/Graph/Runtime/NodeStreamingController.cs` — the frame loop

## Context for fix

If the room geometry is separate from the Room_* GOs (H1), the fix approach depends on how the scene is structured:
- **If geometry can be parented under Room_*:** reparent and the existing system works
- **If geometry must stay separate:** `NodePresentationHandle` needs a serialized reference to the geometry root GO instead of toggling `this.gameObject`
- **If there's no geometry at all in House_Prototype** (it's purely a graph test scene): we need to add graybox geometry (cubes for walls/floor/ceiling) to make the activation system visually testable
