# Run 6 — AI-Assisted Development Guardrails and Unity/C# Anti-Patterns

## Overview

This report covers Checklist F and Checklist G from the project brief: how to use Claude Code safely and effectively for a Unity C# project, how to structure repo instructions so AI output stays inside project constraints, and which Unity-specific code patterns Claude should avoid generating. It also translates common Unity engine pitfalls into explicit guardrails suitable for a `CLAUDE.md` or equivalent repo instruction file.

The central recommendation is: **treat Claude as a fast implementation partner, not an autonomous architecture authority**. Give it narrow tasks, enforce repo-specific rules, require it to preserve existing architecture, and explicitly forbid the Unity patterns that generate hidden coupling, editor-only success, GC churn, and multiplayer desync.

## Executive Summary

AI assistance is most valuable in Unity when the model is constrained by a clear architecture, explicit coding rules, and small-task boundaries. It becomes actively dangerous when allowed to invent systems, rename files broadly, rewrite unrelated code, or generate idiomatic C# that ignores Unity's lifecycle and serialization model. The correct guardrail posture is not “trust but verify”; it is “specify, constrain, inspect, and test.”[cite:414][cite:426]

For this project, the most important repo-level guardrails are:
- Claude must preserve the bootstrap-scene + composition-root architecture.
- Claude must never introduce new global singletons without explicit approval.
- Claude must not use raw `SceneManager.LoadScene` in NGO gameplay code.
- Claude must initialize NGO code in `OnNetworkSpawn`, not `Awake`/`Start`.
- Claude must not generate `FindObjectOfType`, `GameObject.Find`, `Camera.main`, or other scene-wide lookup calls inside gameplay hot paths.[cite:422][cite:419][cite:453]

## Part 1 — Guardrails for AI-Assisted Unity Development

### Why Unity needs stronger AI guardrails than ordinary app code

Unity code is unusually sensitive to hidden lifecycle rules, serialization behavior, scene wiring, inspector references, and editor/runtime differences. A code assistant can easily generate syntactically correct C# that still fails in subtle Unity-specific ways: initialization order bugs, broken serialized references, editor-only object lookup success, hidden GC allocation, and network-state desynchronization. That means the repo instructions need to encode engine rules, not just style preferences.[cite:404][cite:453]

General AI-assisted development guidance also recommends constraining scope, making expected output explicit, and avoiding broad autonomous refactors unless the system has enough repo context to act safely.[cite:414][cite:426]

### Recommended operating model for Claude Code

Use Claude in four modes, each with different permissions:

| Mode | Allowed scope | Typical tasks | Human review level |
|---|---|---|---|
| **Research mode** | Read-only | Compare approaches, explain APIs, propose architecture | Medium |
| **Spec mode** | Write docs only | Draft design notes, task plans, pseudocode | Medium |
| **Implementation mode** | Small focused diff | Add one component, one method, one system slice | High |
| **Refactor mode** | Existing files only, narrow target | Rename methods, split classes, reduce duplication | Very high |

For this project, Claude should spend most of its time in **research mode** and **small-scope implementation mode**, not free-form refactor mode.

### Prompting rules that reduce bad output

Best-practice guidance for Claude Code emphasizes giving explicit constraints, expected file boundaries, and acceptance criteria instead of open-ended requests.[cite:414][cite:417]

**Good prompt pattern:**
- State the target file(s).
- State what must not change.
- State the architecture to preserve.
- State the Unity-specific constraints.
- Ask for a brief plan first when the task is medium or large.

Example:

```text
Modify only Assets/_Project/Scripts/Gameplay/Doors/DoorInteractable.cs.
Do not change public serialized fields or rename classes.
Keep the existing NGO server-authoritative pattern.
Initialize all network subscriptions in OnNetworkSpawn and unsubscribe in OnNetworkDespawn.
Do not use FindObjectOfType, GameObject.Find, or new singletons.
First: explain your plan in 5 bullets. Then implement.
```

This is dramatically safer than “make the door system multiplayer.”

### What should go in CLAUDE.md

A Unity-focused `CLAUDE.md` should act as a project constitution, not a tutorial. The most useful patterns in repo instruction files are concise rules that shape output quality, file targeting, and architectural consistency rather than long prose explanations.[cite:414][cite:442][cite:447]

Recommended sections:
- **Project identity** — genre, platform, architecture summary.
- **Non-negotiable architecture rules** — bootstrap scene, composition root, no manager sprawl.
- **Networking rules** — NGO authority model, scene loading, ownership conventions.
- **Code generation rules** — file naming, folder boundaries, serialization rules, no broad rewrites.
- **Forbidden patterns** — raw `SceneManager.LoadScene`, `FindObjectOfType`, singleton creation, logic in Update when event-driven is possible, allocations in hot paths.
- **Testing expectations** — MPPM smoke test, host + client validation, no code considered done without multiplayer verification.
- **Response format** — ask Claude to propose plan first, list modified files, note assumptions, and flag uncertainty.

### Recommended CLAUDE.md rules for this repo

The following rule set is appropriate for the impossible-house project:

```text
# Architecture Rules
- Preserve the bootstrap scene + persistent NetworkManager + composition root setup.
- Prefer small single-purpose MonoBehaviours over large manager classes.
- Do not introduce new global singletons without explicit approval.
- Keep ScriptableObjects as static/config data, not mutable runtime state.
- Do not rename folders, namespaces, or serialized field names unless explicitly asked.

# Unity Rules
- Prefer [SerializeField] private over public fields unless a field is part of the public API.
- Do not use GameObject.Find, FindObjectOfType, FindAnyObjectByType, or Camera.main in gameplay hot paths.
- Avoid work in Update if event-driven, coroutine, timer, or custom update manager patterns are more appropriate.
- Do not allocate new collections, strings, or lambdas in per-frame code.
- Cache component references in Awake or initialization methods.

# NGO Rules
- Use NetworkBehaviour when NGO features are needed.
- Initialize NGO subscriptions in OnNetworkSpawn and clean them up in OnNetworkDespawn.
- Use NetworkVariables for persistent shared state and RPCs for transient events.
- Use [ServerRpc(RequireOwnership = false)] for shared interactables.
- Never use SceneManager.LoadScene during an active NGO session; use NetworkSceneManager.

# Workflow Rules
- For medium or large tasks: explain plan first, then implement.
- Modify only the requested files unless additional file changes are justified explicitly.
- If assumptions are needed, list them before coding.
- If a requested change conflicts with project rules, say so instead of improvising.
```

## Part 2 — Unity/C# Anti-Patterns Claude Should Avoid

### 1. Manager sprawl and singleton addiction

This is one of the most common Unity anti-patterns. Developers start with a `GameManager`, then add `AudioManager`, `UIManager`, `DoorManager`, `RoomManager`, `EnemyManager`, and eventually everything depends on everything else. Singletons hide dependencies, make order-of-initialization brittle, and become especially dangerous across scene loads and NGO sessions.[cite:418][cite:421]

**Bad pattern:**

```csharp
public class DoorController : MonoBehaviour
{
    void OpenDoor()
    {
        GameManager.Instance.AudioManager.Play("door_open");
        UIManager.Instance.ShowPrompt("Door opened");
        RoomManager.Instance.MarkVisited(currentRoomId);
    }
}
```

This class secretly depends on three global objects and becomes impossible to test in isolation.

**Preferred pattern:** explicit references or composition-root wiring.

```csharp
public class DoorController : MonoBehaviour
{
    [SerializeField] private DoorAudioPresenter _audioPresenter;
    [SerializeField] private DoorStateService _doorStateService;

    public void OpenDoor()
    {
        _doorStateService.MarkDoorOpen(this);
        _audioPresenter.PlayOpen();
    }
}
```

### 2. Scene-wide lookup calls in gameplay code

Unity-wide lookup calls like `GameObject.Find`, `FindObjectOfType`, `FindAnyObjectByType`, and repeated `Camera.main` access are convenient but dangerous. They are slower than cached references, hide dependencies, and often succeed in the editor while failing in actual gameplay transitions or pooled object contexts.[cite:419][cite:422]

JetBrains Rider specifically flags `Camera.main` as expensive because it searches for the camera tagged `MainCamera` each call rather than returning a cached reference.[cite:422]

**Rule**: never generate these in hot paths, and prefer not to generate them at all outside bootstrap or one-time initialization code.

### 3. Everything public for inspector access

A common beginner pattern is making fields public just so they appear in the Inspector. This weakens encapsulation and exposes implementation details unnecessarily.[cite:434][cite:439]

**Prefer:**

```csharp
[SerializeField] private Transform _viewRoot;
[SerializeField] private float _openDuration = 0.4f;
```

Use `public` only when something is intentionally part of the external API of the component.

### 4. Doing too much in Update

Unity's own performance guidance recommends reducing the number of per-frame managed callbacks and using a custom update manager when many objects need conditional or infrequent updates.[cite:453] Large numbers of `Update()` methods create native-to-managed transition overhead even when each one appears cheap.

**Bad uses of Update:**
- Polling for state changes that could use events.
- Recomputing values every frame when they only change occasionally.
- Repeated object lookups every frame.
- Empty or nearly empty Update methods on many components.

**Better alternatives:**
- C# events / UnityEvents for state-driven logic.
- Coroutines for timed sequences.
- Timers invoked at lower frequency.
- A custom update manager when many objects need selective ticking.[cite:453]

### 5. Hidden GC allocations in hot paths

Unity's garbage collection documentation explicitly warns against per-frame string concatenation, array-returning APIs, boxing, closures, and repeated temporary object creation in gameplay loops.[cite:404]

**High-risk allocation sources:**[cite:404][cite:407][cite:454]
- `new List<T>()` in Update / FixedUpdate.
- String concatenation in UI refresh loops.
- Boxing of value types when passing into `object` or non-generic interfaces.
- LINQ in gameplay hot paths.
- Capturing lambdas inside repeated callbacks.
- APIs that return arrays each call.

**Bad pattern:**

```csharp
void Update()
{
    var nearby = new List<Collider>();
    statusText.text = "Fear: " + currentFear;
}
```

**Better:**

```csharp
private readonly List<Collider> _nearby = new();
private readonly StringBuilder _sb = new(64);

void RefreshUI()
{
    _sb.Clear();
    _sb.Append("Fear: ").Append(_currentFear);
    _statusText.text = _sb.ToString();
}
```

### 6. Inspector-driven runtime state

Serialized fields are excellent for configuration, references, and defaults. They are a poor place to store mutable runtime state that should be initialized explicitly, reset deterministically, or synchronized over NGO. When runtime state is hidden in serialized fields, prefab overrides and scene state can produce inconsistent startup behavior.

**Rule**: use serialized fields for configuration, not for authoritative mutable game state.

### 7. Coroutines, Tasks, and async misuse

Unity supports both coroutines and async/await patterns, but they solve different problems. Coroutines are still the simplest tool for sequencing gameplay over frames or seconds inside the main thread. `Task`/`async` are better suited for real asynchronous work (I/O, web requests, background operations) and can cause thread/context confusion if used casually in gameplay code.[cite:435][cite:441]

**Rule for Claude**:
- Prefer coroutines for gameplay sequencing and delays.
- Prefer async/await for external I/O or clearly isolated async services.
- Do not mix both in a single feature without a strong reason.
- Never touch Unity objects off the main thread.

### 8. Broad refactors that break serialization

In Unity, renaming fields, moving scripts, changing namespaces, and splitting classes can break serialized references, prefab links, and scene bindings even if the C# code still compiles. This is an area where AI tools are especially dangerous because they tend to normalize codebases according to general software style rather than Unity's serialization constraints.

**Rule**: Claude should never rename serialized fields, move MonoBehaviour scripts, or change namespaces broadly unless the user explicitly requests it and understands the serialization consequences.

## Part 3 — Unity Multiplayer-Specific Anti-Patterns for Claude

These overlap with Run 4 but belong in the guardrails document because AI tools often generate them by default.

### Never do these in NGO code

- Put NetworkVariable subscriptions in `Awake` or `Start` instead of `OnNetworkSpawn`.
- Use raw `SceneManager.LoadScene` during an active NGO session.
- Assume host mode proves correctness for remote clients.
- Let clients directly mutate shared world state without a server-authoritative path.
- Introduce broad static access patterns that bypass ownership/authority boundaries.
- Generate client-side-only visual fixes that actually need synchronized state.

These are not “style issues”; they are correctness issues that create desyncs.[cite:404]

## Part 4 — Review Checklist for AI-Generated Unity Code

Use this checklist on every non-trivial Claude diff:

### Architecture
- Does the change preserve bootstrap/composition-root architecture?
- Did Claude introduce a new singleton or manager unnecessarily?
- Are dependencies explicit rather than hidden behind globals?

### Unity lifecycle
- Are component references cached once rather than searched repeatedly?
- Is `Update()` actually necessary?
- Did Claude misuse `Awake`, `Start`, `OnEnable`, or `OnDisable`?

### Serialization
- Did Claude rename serialized fields or move scripts?
- Are inspector references still valid?
- Are public fields truly public API, or should they be `[SerializeField] private`?

### Performance
- Any per-frame allocations?
- Any LINQ in hot paths?
- Any string concatenation in frequent loops?
- Any repeated `Camera.main`, `FindObjectOfType`, or array-returning API calls?

### NGO
- Is network initialization in `OnNetworkSpawn`?
- Are subscriptions removed in `OnNetworkDespawn`?
- Are stateful values using NetworkVariables and transient events using RPCs?
- Are scene transitions going through `NetworkSceneManager`?

## Part 5 — Recommended “Always / Never” Rules for Claude

### Claude should always do

- Preserve architecture and existing scene/bootstrap patterns.
- Prefer small focused diffs over sweeping rewrites.[cite:414][cite:417]
- Use `[SerializeField] private` for inspector references unless a field is true public API.[cite:434]
- Cache references instead of repeated scene-wide lookups.[cite:419][cite:422]
- Minimize `Update()` usage and prefer events/coroutines/timers where appropriate.[cite:453]
- Avoid per-frame allocations and boxing in hot paths.[cite:404][cite:454]
- Explain assumptions before coding when repo context is incomplete.[cite:414]
- Flag when a request conflicts with project rules instead of silently improvising.[cite:426]

### Claude should never do

- Create new singletons casually.[cite:418][cite:421]
- Use `GameObject.Find`, `FindObjectOfType`, `FindAnyObjectByType`, or repeated `Camera.main` in gameplay code.[cite:419][cite:422]
- Make fields public only for inspector visibility.[cite:434][cite:439]
- Put broad logic in `Update()` without justification.[cite:453]
- Allocate lists, strings, arrays, or closures in hot paths.[cite:404][cite:407]
- Rename serialized fields or move Unity scripts without explicit permission.
- Assume normal C# refactor rules are safe in Unity serialization contexts.
- Override project architecture because a generic software pattern looks cleaner on paper.

## Conclusion

The best use of Claude Code in this Unity project is not autonomous architecture invention; it is **constrained acceleration**. The repo should encode explicit engine-aware rules so that Claude generates code aligned with Unity's lifecycle, serialization model, NGO authority rules, and performance constraints. If the guardrails in this report are turned into a project-level `CLAUDE.md`, the quality of generated code will improve substantially and the frequency of “compiles but fails in play” bugs should drop sharply.[cite:414][cite:426][cite:453]
