# S1.SQA -- Vertical Slice Spike (Disposable)

## What this is

A disposable, single-session spike in a separate git worktree. Its only output is **learnings** recorded in `docs/ARCH.md`. All code is thrown away after the session. Namespace everything `Desync.Spike.*` so it is obviously not production.

## What we are learning (not building)

S1A (House Graph Authoring) is the critical-path sprint that everything else depends on. Before committing to Approach B in the full PDD, we need to burn down three assumptions that have zero existing code coverage:

1. **SO-to-runtime pipeline:** Can a ScriptableObject graph definition hydrate into a runtime dictionary and be queried?
2. **Graph-driven materialization:** Can room prefabs be placed at graph-defined positions with walkable doorway connections?
3. **NGO struct sync:** Can a `NetworkVariable<T>` replicate a simple `INetworkSerializable` struct between host and client on LAN?

If any of these fail or surface unexpected friction, we adjust the S1A approach before writing production code.

## Setup

```bash
# From repo root
git worktree add ../desync-spike-s1sqa main
# Work entirely inside ../desync-spike-s1sqa/unity-DESYNC/
# When done: git worktree remove ../desync-spike-s1sqa
```

Use the existing `Bootstrap.unity` scene pattern and `PF_Player` prefab. Create a new scene `Spike_GraphTest.unity` that Bootstrap loads instead of `House_Graybox`.

---

## Question 1: SO graph definition to runtime dictionary

**Goal:** Author a ScriptableObject with serialized node/edge arrays. At runtime, load it into a `Dictionary<string, SpikeNodeData>` and query adjacency.

**Acceptance (pass/fail):**
- [ ] A `SpikeGraphDefinition` SO asset with 3 nodes and 2 edges exists in the inspector
- [ ] On Play, a MonoBehaviour reads the SO and populates `Dictionary<string, SpikeNodeData>`
- [ ] `Debug.Log` confirms: node lookup by ID works, adjacency query returns correct neighbor IDs
- [ ] Modifying the SO asset (adding a node) is reflected on next Play without code changes

**File:**
- `Scripts/Spike/SpikeGraphDefinition.cs` -- ScriptableObject with `NodeDef[]` and `EdgeDef[]` (serialized structs)
- `Scripts/Spike/SpikeGraphRuntime.cs` -- MonoBehaviour that loads the SO into dictionaries on `Awake`

**Structs (inline in the SO file is fine):**
```
[Serializable] struct NodeDef { string id; Vector3 position; }
[Serializable] struct EdgeDef { string fromNodeId; string toNodeId; }
```

**What to log:**
- Does Unity serialize `string` fields in struct arrays without issues?
- Any friction with `[CreateAssetMenu]` workflow?
- Does the dictionary hydration pattern feel clean or does it fight Unity serialization?

---

## Question 2: Graph-driven room materialization + player traversal

**Goal:** Place 3 simple room prefabs (ProBuilder boxes with one open face each as a doorway) at positions defined by the graph SO. Walk between them through the doorway openings.

**Acceptance (pass/fail):**
- [ ] 3 room prefabs instantiated at positions read from `SpikeGraphDefinition`
- [ ] Doorway openings between connected rooms are aligned (player can walk through without collision)
- [ ] Player spawns in node 0 and can walk to node 2 through node 1
- [ ] No CharacterController wedging in doorway geometry

**File:**
- `Scripts/Spike/SpikeRoomSpawner.cs` -- reads `SpikeGraphRuntime` dictionaries, instantiates room prefabs at node positions
- `Prefabs/Spike/SpikeRoom.prefab` -- a ProBuilder box (4m x 3m x 4m) with one wall removed (doorway face)

**Construction notes:**
- Rooms are just open-face boxes. No doors, no portals, no visibility logic.
- Doorway alignment: edges in the graph define which faces to remove. For the spike, just manually orient the prefabs so open faces meet.
- Use the existing minimum overhead clearance rule: 2.25m interior ceiling height.
- Player spawn point: place a transform at node 0's position + (0, 1, 0).

**What to log:**
- Does instantiating prefabs at SO-defined positions work cleanly?
- Any CharacterController issues at room boundaries (step offset, seams)?
- How does it feel to walk between the boxes -- is the doorway width sufficient (target: 1.2m minimum)?

---

## Question 3: NGO NetworkVariable struct sync on LAN

**Goal:** Sync a simple `INetworkSerializable` struct between host and client. This tests the narrowest useful network sync pattern for graph versioning.

**Acceptance (pass/fail):**
- [ ] A `NetworkVariable<SpikeHouseState>` on a NetworkBehaviour updates on the host
- [ ] The client receives the updated value via `OnValueChanged`
- [ ] `Debug.Log` on client confirms: received topology version matches what host set
- [ ] Changing the value multiple times on host produces correct sequence on client

**File:**
- `Scripts/Spike/SpikeNetworkSync.cs` -- NetworkBehaviour with `NetworkVariable<SpikeHouseState>` and a keypress to increment version on host

**Struct:**
```
struct SpikeHouseState : INetworkSerializable {
    public int TopologyVersion;
    public int OccupiedNodeCount;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref TopologyVersion);
        serializer.SerializeValue(ref OccupiedNodeCount);
    }
}
```

**Test procedure:**
1. Start as Host (Bootstrap flow)
2. Press a key (e.g., `T`) to increment `TopologyVersion` on server
3. Check client console for `OnValueChanged` log showing new version
4. Repeat 3-4 times to confirm reliable delivery

**What to log:**
- Does `INetworkSerializable` with value types serialize without issues in NGO 2.11.2?
- Any gotchas with `NetworkVariable` of structs (equality check, dirty flags)?
- Latency feel on LAN -- is the update effectively instant?

---

## Execution plan

1. Create the worktree and open `unity-DESYNC` in it with the Unity Editor
2. Create `Assets/_Project/Scripts/Spike/` folder
3. **Q1 first** -- write `SpikeGraphDefinition.cs`, create the SO asset, write `SpikeGraphRuntime.cs`, hit Play, verify logs
4. **Q2 second** -- create the room prefab with ProBuilder, write `SpikeRoomSpawner.cs`, place rooms from graph data, verify player traversal
5. **Q3 third** -- write `SpikeNetworkSync.cs`, attach to a GameObject in Bootstrap scene, test with Multiplayer Play Mode (host + virtual client)
6. Record all findings in `docs/ARCH.md` under a new section (see template below)
7. Remove the worktree: `git worktree remove ../desync-spike-s1sqa`

Total: ~4-5 files of code, 1 prefab, 1 SO asset, 1 scene.

---

## ARCH.md recording template

Add this section to `docs/ARCH.md` when done:

```markdown
### S1.SQA spike findings (YYYY-MM-DD)

**Q1 -- SO graph to runtime dictionary:**
- Result: PASS / FAIL
- Findings: [what worked, what surprised you, any serialization friction]
- Decision: [carry this pattern into S1A / modify because X]

**Q2 -- Graph-driven room materialization:**
- Result: PASS / FAIL
- Findings: [instantiation, doorway alignment, CharacterController behavior]
- Decision: [room prefab strategy for S1A / adjust because X]

**Q3 -- NGO struct sync:**
- Result: PASS / FAIL
- Findings: [INetworkSerializable behavior, dirty flags, latency]
- Decision: [use NetworkVariable<struct> for graph versioning in S1A / use NetworkList because X]
```

---

## Reminders

- **This is disposable.** Do not refactor, do not add error handling, do not build debug overlays. Raw functional tests only.
- **Namespace: `Desync.Spike`** -- not `Desync.Spatial`, not `Desync.Core`.
- **No new singletons or managers.** Plain MonoBehaviours wired in the scene.
- **Use existing infrastructure:** `PF_Player`, `GameBootstrap`, `Bootstrap.unity` scene loading pattern.
- **Do not modify any existing production files.** The spike scene (`Spike_GraphTest`) is the only new scene.
- **Log everything to console.** No UI, no overlays, no polish.
- **When done**, the only permanent artifact is the ARCH.md entry. Delete the worktree.
