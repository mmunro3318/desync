# ScriptableObject Patterns for Runtime Game Systems (in Unity)

Use ScriptableObjects as immutable **definitions** and keep all changing play-session data in runtime objects; that is the safest default for a Unity 6 game, especially once multiplayer and large asset graphs enter the picture.  In your case, room definitions, anomaly definitions, anchor definitions, and behavior profiles all fit well as authored assets, while spawned room instances, active anomaly cooldowns, resolved match state, and per-entity behavior memory should live in plain C# classes or MonoBehaviours created from those definitions at runtime. [docs.unity3d](https://docs.unity3d.com/6000.4/Documentation/Manual/class-ScriptableObject.html)

## Definition pattern

Unity describes ScriptableObjects as data containers that exist independently of scene instances, which is exactly why they work well as authored definitions.  The strongest pattern is: asset holds immutable authored data, runtime object holds mutable state plus a reference back to the definition, and a factory or bootstrapper converts one into the other so your gameplay code never mutates the asset. [docs.unity3d](https://docs.unity3d.com/es/2020.1/Manual/class-ScriptableObject.html)

A good baseline shape looks like this: [docs.unity3d](https://docs.unity3d.com/ScriptReference/ScriptableObject.CreateInstance.html)

```csharp
using UnityEngine;

public interface IRuntimeFromDefinition<out TRuntime>
{
    TRuntime CreateRuntime(RuntimeContext context);
}

public sealed class RuntimeContext
{
    public int Seed { get; }
    public RuntimeContext(int seed) => Seed = seed;
}

[CreateAssetMenu(menuName = "Game/Rooms/Room Definition")]
public sealed class RoomDefinition : ScriptableObject, IRuntimeFromDefinition<RoomRuntime>
{
    [SerializeField] private string stableId;
    [SerializeField] private MaterialProfile materialProfile;
    [SerializeField] private LightingPreset lightingPreset;
    [SerializeField] private PropKit[] propKits;

    public string StableId => stableId;
    public MaterialProfile MaterialProfile => materialProfile;
    public LightingPreset LightingPreset => lightingPreset;
    public IReadOnlyList<PropKit> PropKits => propKits;

    public RoomRuntime CreateRuntime(RuntimeContext context)
    {
        return new RoomRuntime(this, context.Seed);
    }
}

public sealed class RoomRuntime
{
    public RoomDefinition Definition { get; }
    public int Seed { get; }
    public bool IsOccupied { get; private set; }
    public float ThreatLevel { get; private set; }

    public RoomRuntime(RoomDefinition definition, int seed)
    {
        Definition = definition;
        Seed = seed;
        IsOccupied = false;
        ThreatLevel = 0f;
    }

    public void MarkOccupied() => IsOccupied = true;
    public void AddThreat(float amount) => ThreatLevel += amount;
}
```

That keeps coupling one-way: runtime depends on the definition contract, but the definition does not know scene objects, network state, or session-specific logic.  If you want even looser coupling, define narrow read-only interfaces like `IRoomDefinitionData` and pass those into constructors so your runtime code can be tested without real assets. [docs.unity3d](https://docs.unity3d.com/Packages/com.unity.test-framework@1.0/manual/edit-mode-vs-play-mode-tests.html)

## Config vs data container

ScriptableObjects are excellent both for simple tuning knobs and for richer authored datasets, but the line is whether the asset is still a stable authored artifact rather than live gameplay state.  A damage curve, ghost speed table, room rarity weights, or audio mix profile is a simple config SO; a room layout template, dialogue graph, behavior tree, anomaly rule graph, or prop spawn graph is a complex data SO. [learn.unity](https://learn.unity.com/tutorial/introduction-to-scriptableobjects)

Use an SO as a complex container when all of these are true: [docs.unity3d](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/ISerializationCallbackReceiver.html)
- Designers need to author and reuse it as an asset. [docs.unity3d](https://docs.unity3d.com/6000.4/Documentation/Manual/class-ScriptableObject.html)
- It is mostly static once the build ships. [docs.unity3d](https://docs.unity3d.com/es/2020.1/Manual/class-ScriptableObject.html)
- Its internal references form a content graph, not a mutable simulation graph. [docs.unity3d](https://docs.unity3d.com/6000.3/Documentation/Manual/assets-avoid-duplication.html)

Stop using SOs as the primary runtime holder when any of these become true: [docs.unity3d](https://docs.unity3d.com/2020.1/Documentation/ScriptReference/ISerializationCallbackReceiver.html)
- The object must change per match, per player, or per networked session. [docs.unity3d](https://docs.unity3d.com/ScriptReference/ScriptableObject.CreateInstance.html)
- You need rollback, save/load snapshots, or deterministic replication of mutable fields. [docs.unity3d](https://docs.unity3d.com/2020.1/Documentation/ScriptReference/ISerializationCallbackReceiver.html)
- You find yourself calling `definition.currentHealth = ...` or tracking timers directly on the asset. [docs.unity3d](https://docs.unity3d.com/ScriptReference/ScriptableObject.CreateInstance.html)

For your horror game, I’d put behavior tree structure, room layout sockets, anomaly trigger rules, and anchor link topology in SOs, but keep blackboard state, active nodes, cooldown timers, resolved matches, and spawned prop occupancy in runtime classes. [docs.unity3d](https://docs.unity3d.com/6000.4/Documentation/Manual/class-ScriptableObject.html)

## Multiplayer identity

In Netcode for GameObjects, `NetworkObjectReference` is for spawned networked scene/prefab objects, not for ScriptableObject assets.  So for shared definitions, the usual pattern is to transmit a stable asset identifier such as a string ID or integer ID, then resolve that ID through a registry on both client and server. [docs.unity3d](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@1.0/api/Unity.Netcode.NetworkObjectReference.html)

Important distinction: Unity asset GUIDs are editor-side AssetDatabase concepts, and `AssetDatabase.GUIDToAssetPath` is an Editor API, so you should not rely on AssetDatabase GUID lookup in player builds.  That makes this a good split: [docs.unity3d](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AssetDatabase.GUIDToAssetPath.html)

| Use case | Recommended ID | Why |
|---|---|---|
| Editor validation and duplicate checks | Asset GUID  [docs.unity3d](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AssetDatabase.GUIDToAssetPath.html) | Great for tooling, not runtime in builds.  [docs.unity3d](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AssetDatabase.GUIDToAssetPath.html) |
| Runtime client/server definition lookup | Stable serialized string or int on the SO, such as `room_abandoned_lab_01`  [docs.unity3d](https://docs.unity3d.com/6000.4/Documentation/Manual/class-ScriptableObject.html) | Works in builds, save files, and packets.  [docs.unity3d](https://docs.unity3d.com/6000.4/Documentation/Manual/class-ScriptableObject.html) |
| Spawned network entities | `NetworkObjectReference`  [docs.unity3d](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@1.0/api/Unity.Netcode.NetworkObjectReference.html) | NGO already supports this for `NetworkObject`s.  [docs.unity3d](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@1.0/api/Unity.Netcode.NetworkObjectReference.html) |
| Network prefab selection | NGO prefab hash / prefab registration  [docs.unity3d](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@1.7/api/Unity.Netcode.NetworkObject.PrefabIdHash.html) | This is for network prefabs, not SO definitions.  [docs.unity3d](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@1.7/api/Unity.Netcode.NetworkObject.PrefabIdHash.html) |

A practical NGO pattern is: server decides `RoomDefinitionId`, sends the ID in an RPC or `NetworkVariable<FixedString64Bytes>`, client resolves it via a local definition database, then instantiates local presentation/runtime from that definition.  Do not send raw asset references or expect a `NetworkObjectReference` to represent a ScriptableObject. [docs.unity3d](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.2/api/Unity.Netcode.NetworkObjectReference.html)

Example: [docs.unity3d](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@1.0/api/Unity.Netcode.NetworkObjectReference.html)

```csharp
using Unity.Collections;
using Unity.Netcode;

public struct RoomSpawnData : INetworkSerializable
{
    public FixedString64Bytes RoomId;
    public int Seed;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref RoomId);
        serializer.SerializeValue(ref Seed);
    }
}
```

Then resolve with a registry: [docs.unity3d](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AssetDatabase.GUIDToAssetPath.html)

```csharp
public interface IDefinitionRegistry<TDef>
{
    bool TryGet(string id, out TDef definition);
}

[CreateAssetMenu(menuName = "Game/Registries/Room Registry")]
public sealed class RoomRegistry : ScriptableObject, IDefinitionRegistry<RoomDefinition>
{
    [SerializeField] private RoomDefinition[] rooms;
    private Dictionary<string, RoomDefinition> _map;

    public void Initialize()
    {
        _map = new Dictionary<string, RoomDefinition>(rooms.Length);
        foreach (var room in rooms)
            _map.Add(room.StableId, room);
    }

    public bool TryGet(string id, out RoomDefinition definition)
        => _map.TryGetValue(id, out definition);
}
```

## Large graphs

Once you have 50+ definitions with cross-references, the main risk is not size but dependency sprawl, duplicate loading, and broken references.  The best pattern is to make leaf assets small and reusable, then point high-level assets at them through registries or clearly bounded subgraphs. [github](https://github.com/njelly/addressables-scriptableobjects-test)

A scalable setup for your room/material/prop/lighting web is: [learn.unity](https://learn.unity.com/course/get-started-with-addressables/tutorial/load-addressable-assets-in-scripts?version=2022.2)
- `RoomDefinition` references only the specific assets it truly owns semantically. [docs.unity3d](https://docs.unity3d.com/6000.4/Documentation/Manual/class-ScriptableObject.html)
- Shared assets like `MaterialProfile`, `LightingPreset`, `PropKit`, and `EntityBehaviorProfile` live in dedicated shared folders and, if using Addressables, dedicated groups to avoid duplication. [docs.unity3d](https://docs.unity3d.com/6000.3/Documentation/Manual/assets-avoid-duplication.html)
- A central `GameDefinitionCatalog` or per-domain registry maps stable IDs to definitions for lookup, validation, save/load, and network resolution. [docs.unity3d](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AssetDatabase.GUIDToAssetPath.html)
- If content is large or streamed, load definition sets through Addressables rather than hard references from always-loaded bootstrap scenes. [learn.unity](https://learn.unity.com/course/get-started-with-addressables/tutorial/load-addressable-assets-in-scripts?version=2022.2)

Unity warns that Addressables and AssetBundles can duplicate dependencies if shared referenced assets are not assigned deliberately, and duplicated dependencies become distinct loaded instances.  That matters a lot for ScriptableObjects because duplicated instances can break assumptions about identity or shared read-only data, so shared SOs should be grouped intentionally and analyzed for duplication. [github](https://github.com/njelly/addressables-scriptableobjects-test)

A good content-loading rule is: if a definition must always exist for gameplay bootstrap, keep it in a built-in startup catalog; if it belongs to streamable content packs, make the definition and its shared dependencies addressable together.  Avoid mixing `Resources` loading with normal references and Addressables for the same definition families unless you want future debugging pain. [reddit](https://www.reddit.com/r/Unity3D/comments/xzr1x4/what_is_the_most_efficient_way_to_load/)

## Editor tooling

For large SO collections, editor tooling matters almost more than runtime code.  The most useful tools are registries, validators, and mass-creation helpers rather than fancy inspectors first. [docs.unity3d](https://docs.unity3d.com/Packages/com.unity.test-framework@1.0/manual/edit-mode-vs-play-mode-tests.html)

I’d prioritize these editor patterns: [docs.unity3d](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AssetDatabase.GUIDToAssetPath.html)
- **Stable ID generator**: custom inspector button that assigns a unique runtime ID if blank. [docs.unity3d](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AssetDatabase.GUIDToAssetPath.html)
- **Registry auto-sync**: editor script that finds all assets of a type and repopulates a registry asset. `AssetDatabase` is ideal for this in editor code. [docs.unity](https://docs.unity.cn/550/Documentation/Manual/AssetDatabase.html)
- **Validation pass**: menu item or `OnValidate` checks for duplicate IDs, null references, cycles where forbidden, and missing Addressable labels. [docs.unity3d](https://docs.unity3d.com/6000.3/Documentation/Manual/assets-avoid-duplication.html)
- **Derived summaries**: custom inspector panels that show resolved dependency counts, inbound references, and warnings for deep null chains. [docs.unity3d](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AssetDatabase.GUIDToAssetPath.html)
- **Create menus / templates**: use `CreateAssetMenu` for every domain asset and prefill sensible defaults. [docs.unity3d](https://docs.unity3d.com/6000.4/Documentation/Manual/class-ScriptableObject.html)

Example registry rebuilder: [docs.unity](https://docs.unity.cn/550/Documentation/Manual/AssetDatabase.html)

```csharp
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class RegistryBuilder
{
    [MenuItem("Tools/Game/Rebuild Room Registry")]
    public static void RebuildRoomRegistry()
    {
        var registry = AssetDatabase.LoadAssetAtPath<RoomRegistry>(
            "Assets/Game/Data/Registries/RoomRegistry.asset");

        var guids = AssetDatabase.FindAssets("t:RoomDefinition");
        var rooms = new List<RoomDefinition>();

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var room = AssetDatabase.LoadAssetAtPath<RoomDefinition>(path);
            if (room != null) rooms.Add(room);
        }

        registry.SetRooms(rooms); // expose an editor-only setter if you want
        EditorUtility.SetDirty(registry);
        AssetDatabase.SaveAssets();
    }
}
#endif
```

## Testing

Unity’s Test Framework supports Edit Mode tests for editor/game code and Play Mode tests for runtime behavior, and Edit Mode is usually enough for SO-consuming logic that does not require scene lifecycle.  That makes your architecture easier to test if runtime systems depend on interfaces or plain constructor inputs instead of directly reaching into project assets. [docs.unity3d](https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/manual/edit-mode-vs-play-mode-tests.html)

Three useful testing strategies: [docs.unity3d](https://docs.unity3d.com/Packages/com.unity.test-framework@1.0/manual/edit-mode-vs-play-mode-tests.html)
- Create temporary SOs in tests with `ScriptableObject.CreateInstance<T>()` for fast unit tests. [docs.unity3d](https://docs.unity3d.com/ScriptReference/ScriptableObject.CreateInstance.html)
- Use test-only asset fixtures when you want to verify serialized references and inspector-authored graphs. [unity](https://unity.com/how-to/automated-tests-unity-test-framework)
- Test pure runtime classes separately from Unity object creation so most behavior is ordinary NUnit code. [unity](https://unity.com/how-to/automated-tests-unity-test-framework)

Example Edit Mode test: [docs.unity3d](https://docs.unity3d.com/Packages/com.unity.test-framework@1.0/manual/edit-mode-vs-play-mode-tests.html)

```csharp
using NUnit.Framework;
using UnityEngine;

public class RoomRuntimeTests
{
    [Test]
    public void CreateRuntime_CopiesDefinitionReference_WithoutMutatingAsset()
    {
        var def = ScriptableObject.CreateInstance<RoomDefinition>();
        def.EditorOnly_SetStableId("room_test"); // test hook or internal setter

        var runtime = def.CreateRuntime(new RuntimeContext(seed: 123));

        Assert.AreEqual("room_test", runtime.Definition.StableId);
        Assert.AreEqual(123, runtime.Seed);
        Assert.IsFalse(runtime.IsOccupied);
    }
}
```

For serializer edge cases, Unity provides `ISerializationCallbackReceiver` so you can manually flatten unsupported structures like dictionaries into serializable lists and rebuild them after deserialize.  Use that sparingly and keep callbacks cheap, because Unity notes the serializer can invoke them on a different thread from most Unity API usage. [docs.unity3d](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/ISerializationCallbackReceiver.html)

## Pitfalls

The biggest pitfall is accidental asset mutation: when you change fields on a referenced ScriptableObject during play, you are often changing the shared asset instance rather than isolated match state.  That creates classic bugs like one anomaly affecting all future sessions in-editor, or multiple entities unexpectedly sharing cooldowns because they all point to the same asset. [docs.unity3d](https://docs.unity3d.com/ScriptReference/ScriptableObject.CreateInstance.html)

Other common failure modes: [github](https://github.com/njelly/addressables-scriptableobjects-test)
- Unity serialization does not natively support every type, especially dictionaries, so custom flattening may be needed. [docs.unity3d](https://docs.unity3d.com/2020.1/Documentation/ScriptReference/ISerializationCallbackReceiver.html)
- Null chains get worse as graphs deepen, so validate transitively, not just direct fields. [docs.unity3d](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AssetDatabase.GUIDToAssetPath.html)
- Addressables or AssetBundle duplication can produce separate instances of what you thought was one shared SO if groups are organized badly. [github](https://github.com/njelly/addressables-scriptableobjects-test)
- Editor-only APIs like `AssetDatabase` work for tooling but not in player builds. [docs.unity](https://docs.unity.cn/550/Documentation/Manual/AssetDatabase.html)

A practical rule set helps:
- Never write mutable gameplay fields onto definition assets at runtime. [docs.unity3d](https://docs.unity3d.com/6000.4/Documentation/Manual/class-ScriptableObject.html)
- Put `[NonSerialized]` runtime caches on systems, not on the asset unless you truly understand domain reload and lifecycle implications. [docs.unity3d](https://docs.unity3d.com/6000.4/Documentation/Manual/class-ScriptableObject.html)
- Prefer runtime wrappers over cloning SOs unless cloning is the explicit design. `CreateInstance` gives you a new SO instance, but that should be a conscious exception, not the baseline architecture. [docs.unity3d](https://docs.unity3d.com/ScriptReference/ScriptableObject.CreateInstance.html)
- Validate IDs and references before build. [docs.unity3d](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AssetDatabase.GUIDToAssetPath.html)

## Folder structure

For 10+ SO types and 50+ instances, organize by domain first, then by asset role, and keep registries explicit.  The goal is that a human can answer “where do room definitions live, where do shared presets live, and where do runtime scripts live?” in one glance. [docs.unity3d](https://docs.unity3d.com/6000.4/Documentation/Manual/class-ScriptableObject.html)

A clean structure would be: [docs.unity3d](https://docs.unity3d.com/6000.4/Documentation/Manual/class-ScriptableObject.html)

```text
Assets/
  Game/
    Data/
      Definitions/
        Rooms/
          RoomDefinition_AbandonedWard.asset
          RoomDefinition_ChildrenHall.asset
        Anomalies/
        Anchors/
        MatchRules/
        BehaviorProfiles/
      Shared/
        MaterialProfiles/
        PropKits/
        LightingPresets/
        AudioPresets/
      Registries/
        RoomRegistry.asset
        AnomalyRegistry.asset
        GlobalDefinitionCatalog.asset
      Addressables/
        LabelsDocsOrProfiles.asset   // optional supporting assets
    Scripts/
      Runtime/
        Rooms/
        Anomalies/
        Networking/
        Match/
      Definitions/
        Rooms/
        Anomalies/
      Editor/
        Validation/
        RegistryBuilders/
        Inspectors/
      Tests/
        EditMode/
        PlayMode/
```

Two naming rules prevent chaos fast: use a clear suffix by type such as `RoomDefinition`, `LightingPreset`, `PropKit`, and give every definition a stable runtime ID separate from file name so renaming assets does not break saves or packets. [docs.unity3d](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AssetDatabase.GUIDToAssetPath.html)

## Recommended architecture

For your Unity 6 + NGO horror game, I’d standardize on this stack: [docs.unity3d](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@1.0/api/Unity.Netcode.NetworkObjectReference.html)
1. ScriptableObjects are immutable authored definitions only. [docs.unity3d](https://docs.unity3d.com/6000.4/Documentation/Manual/class-ScriptableObject.html)
2. Runtime state lives in plain C# classes unless it needs scene lifecycle, then MonoBehaviours. [docs.unity3d](https://docs.unity3d.com/ScriptReference/ScriptableObject.CreateInstance.html)
3. A per-domain registry resolves stable string or int IDs to definitions on both server and client. [docs.unity3d](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AssetDatabase.GUIDToAssetPath.html)
4. NGO syncs IDs and seeds, not SO references. `NetworkObjectReference` is reserved for spawned network objects. [docs.unity3d](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.2/api/Unity.Netcode.NetworkObjectReference.html)
5. Addressables load large content packs, with shared SO dependencies grouped to avoid duplication. [learn.unity](https://learn.unity.com/course/get-started-with-addressables/tutorial/load-addressable-assets-in-scripts?version=2022.2)
6. Editor validation enforces unique IDs, no null chains, and registry completeness before build. [docs.unity3d](https://docs.unity3d.com/6000.3/Documentation/Manual/assets-avoid-duplication.html)

If you want, next step I can turn this into a concrete mini-framework for your project with `RoomDefinition`, `AnomalyDefinition`, `DefinitionRegistry<T>`, NGO sync structs, and test scaffolding in copy-pasteable Unity C# files.