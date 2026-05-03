# Unity MCP Lessons -- Insights in Proper Use of MCP Tool

**Scope:** How to use Unity MCP tools in this repo without wasting tokens on schema errors, wrong URIs, or dead-end tool calls.
**Maintained by:** Whichever CC session most recently hit a new failure mode. Append, don't rewrite.

---

## Rule 0 — Read This First

If you are about to reach for a Unity MCP tool, pause and read the relevant rule below. The most common time sinks in this project have been:

1. Calling deferred tools before loading their schema (→ InputValidationError)
2. Wrong MCP resource URIs (→ Resource not found -32002)
3. Wrong tool param case (→ schema rejected)
4. Trusting probe results when the wrong scene is loaded (→ silent empty results)
5. Calling `execute_code` with blocking `while` loops or long waits (→ hangs)

---

## Rule 1 — Load Deferred Tool Schemas Before First Use

Most MCP and task tools are **deferred** — their JSONSchema is NOT in the prompt until you call `ToolSearch`. Calling them before loading produces:

```
InputValidationError ... This tool's schema was not sent to the API
```

**Pre-flight:** At session start, bulk-load every deferred tool you expect to need in a single `ToolSearch` call:

```
ToolSearch query="select:TaskCreate,TaskUpdate,ReadMcpResourceTool,ListMcpResourcesTool"
```

Use the `select:` form — it's the only way to load schemas deterministically. Keyword search may miss. `select:` takes a comma-separated list of exact tool names.

**Unity MCP tools** (all deferred, all prefixed `mcp__coplay-mcp__`):

```
ToolSearch query="select:mcp__coplay-mcp__execute_script,mcp__coplay-mcp__create_game_object,mcp__coplay-mcp__add_component"
```

Load in one message before the first use.

---

## Rule 2 — Tool Params Are camelCase, Not snake_case

All task tools in this harness use camelCase:

| Wrong | Right |
|---|---|
| `task_id` | `taskId` |
| `active_form` | `activeForm` |
| `add_blocked_by` | `addBlockedBy` |

When in doubt, re-read the schema output from `ToolSearch` before calling.

---

## Rule 3 — Unity MCP Resource URIs Use `mcpforunity://` Scheme with `/` Separators

Common mistake: using `unity://editor_state` (wrong scheme, wrong separator).

**Always verify URIs by listing first:**

```
ListMcpResourcesTool server="UnityMCP"
```

### Verified URIs (as of 2026-04-26)

| Resource | URI |
|---|---|
| Editor readiness snapshot | `mcpforunity://editor/state` |
| Active tool / handle settings | `mcpforunity://editor/active-tool` |
| Selection details | `mcpforunity://editor/selection` |
| Open editor windows | `mcpforunity://editor/windows` |
| Prefab stage (isolation mode) | `mcpforunity://editor/prefab-stage` |
| Scene cameras | `mcpforunity://scene/cameras` |
| Scene volumes | `mcpforunity://scene/volumes` |
| GameObject API docs | `mcpforunity://scene/gameobject-api` |
| Project info (version, paths) | `mcpforunity://project/info` |
| Project layers | `mcpforunity://project/layers` |
| Project tags | `mcpforunity://project/tags` |
| Tests | `mcpforunity://tests` |
| Tool groups | `mcpforunity://tool-groups` |
| Menu items | `mcpforunity://menu-items` |
| Running Unity instances | `mcpforunity://instances` |
| URP renderer features | `mcpforunity://pipeline/renderer-features` |
| Rendering stats | `mcpforunity://rendering/stats` |
| Prefab API docs | `mcpforunity://prefab-api` |

---

## Rule 4 — TaskCreate Is Single-Task (not a todos array)

The native Claude Code CLI has `TodoWrite` that takes an array. **This harness's `TaskCreate` is one task per call.** Schema:

```json
{ "subject": "...", "description": "...", "activeForm": "..." }
```

Do NOT pass `todos: [...]` — the schema rejects it. Fire one `TaskCreate` per task; they can be parallelized in a single message.

---

## Rule 5 — Verify the Right Scene Is Loaded Before Running Probes

`OverlapBox`, `CapsuleCast`, `FindGameObjectsWithTag`, and any other runtime probe using `execute_code` will **return empty/wrong results if the wrong scene is active**.

Before trusting probe output:

```csharp
// Always check scene state at the top of any probe script
Debug.Log($"[PROBE] sceneCount={SceneManager.sceneCount}");
for (int i = 0; i < SceneManager.sceneCount; i++)
    Debug.Log($"[PROBE] scene[{i}]={SceneManager.GetSceneAt(i).name} loaded={SceneManager.GetSceneAt(i).isLoaded}");
```

If `House_Graybox` is not listed as loaded, open it via `manage_scene` before probing. This bit D-series agents multiple times — Bootstrap was active post-play-mode-exit, so probes targeted the wrong scene.

---

## Rule 6 — Never Retry the Exact Same Failed Tool Call

If you get `InputValidationError` or `MCP error -32002`, the error message contains the fix:

- `InputValidationError` → re-read the schema. The diff is in the error.
- `-32002 Resource not found` → wrong URI. Run `ListMcpResourcesTool` to get the real one.
- `null reference` in `execute_code` → scene not loaded or GameObject not found. Check scene state first.

Do not retry the identical call. Diagnose → fix → retry.

---

## Rule 7 — execute_code Patterns That Work vs. Patterns That Hang

### Works
```csharp
// Inline probe — runs, collects results, logs them, returns
var go = GameObject.Find("GF_Ceiling");
var bc = go?.GetComponent<BoxCollider>();
Debug.Log($"[PROBE] enabled={bc?.enabled} bounds={bc?.bounds}");
```

### Hangs or errors
```csharp
// DON'T: blocking loop
while (!Application.isPlaying) { /* spin-wait */ }

// DON'T: yield in non-coroutine execute_code
yield return null;

// DON'T: EditorApplication.EnterPlayMode() inside execute_code (triggers domain reload, kills the script mid-run)
```

For play-mode operations, use `manage_editor` with `action="enter_play_mode"` instead of `execute_code`.

---

## Rule 8 — `read_console` After Every Script or Scene Edit

Always call `read_console` after:
- Creating or modifying a script (check for compilation errors before using the new type)
- Entering play mode (check for runtime exceptions)
- Any `execute_code` that modifies scene state (check for MissingReferenceException)

Filter by `logType="Error"` first. Only expand to `Warning` if errors are clean.

```
read_console logType="Error" maxLines=20
```

---

## Rule 9 — Verify APIs Exist Before Writing Code

Use `unity_reflect` to check that a method/property exists before writing C# code that calls it. Saves a full compilation round-trip on typos or version differences.

```
unity_reflect query="CharacterController.stepOffset"
unity_reflect query="BoxCollider.enabled"
```

---

## Rule 10 — Probe-Then-Edit Pattern for Scene Changes

For any scene modification that isn't trivially obvious:

1. **Probe first** — use `execute_code` or `find_gameobjects` to confirm the target exists and read its current state
2. **Edit** — use `manage_gameobject`, `manage_components`, or `execute_code`
3. **Verify** — re-probe immediately after to confirm the edit landed

```
# Example: disabling a BoxCollider
# Step 1: probe
execute_code → Debug.Log($"BC enabled={go.GetComponent<BoxCollider>().enabled}")
# Step 2: edit
manage_components action="modify" → { "BoxCollider": { "enabled": false } }
# Step 3: verify
execute_code → Debug.Log($"BC enabled={go.GetComponent<BoxCollider>().enabled}")
```

This is how D7's implementation was independently verified without entering play mode.

---

## Rule 11 — Subagent Dispatch for Mechanical Unity-MCP Tasks

The subagent-driven-development pattern works well for mechanical Unity MCP work:

- **Implementer subagent** (Sonnet/Haiku): gets full task text + Unity MCP tool list + verification probes inline. Executes the change.
- **Spec-reviewer subagent** (Sonnet): re-runs the same probes independently for verification.
- **Key:** dispatch with `unity_instance` pre-set in the prompt; include the full list of deferred tools to pre-load; include the exact probe scripts to run for verification.

Worked well in D7 (Tasks 1–3). Pattern: give each subagent a self-contained prompt with "done" definition, specific GameObjects and expected values, and the probe scripts to run.

---

## MCP Server Migration Note (2026-05-03)

The Unity MCP server migrated from `UnityMCP` (prefix `mcp__UnityMCP__`) to `coplay-mcp` (prefix `mcp__coplay-mcp__`, port 6400). Tool names also changed (e.g., `execute_code` → `execute_script`, `manage_gameobject` → `create_game_object`/`set_transform`/etc.). Failure log entries below predate this migration; the patterns they describe still apply but tool names differ.

---

## Failure Log (running — append new incidents)

### 2026-04-26 — D6 lintel-fix session start

**1. `TaskCreate` with `todos` array**
- Tried: `TaskCreate({todos: [...]})`
- Error: schema rejected — required `subject`/`description`
- Fix: one `TaskCreate({subject, description, activeForm?})` per task

**2. `TaskUpdate` with `task_id` (snake_case)**
- Tried: `TaskUpdate({task_id: "1", status: "completed"})`
- Error: schema rejected — required `taskId`
- Fix: `TaskUpdate({taskId, status})`

**3. `ReadMcpResourceTool` with `unity://editor_state`**
- Tried: `ReadMcpResourceTool({server: "UnityMCP", uri: "unity://editor_state"})`
- Error: `MCP error -32002: Resource not found`
- Fix: scheme is `mcpforunity://`, separator is `/`. Correct: `mcpforunity://editor/state`

### 2026-04-26 — D7 implementation session

**4. `OverlapBox` probe returning empty with Bootstrap active**
- Context: House_Graybox not reloaded after exiting play mode; Bootstrap was active scene
- Symptom: OverlapBox at known-occupied coordinates returned no colliders
- Cost: ~10 minutes of confused re-probing
- Fix: Always log `SceneManager.sceneCount` + scene names at top of every probe script (Rule 5)

### 2026-04-27 — E2 implementation session

**5. `manage_scene action="load"` rejecting `scene_path` param**
- Tried: `manage_scene({action: "load", scene_path: "Assets/_Project/Scenes/House_Graybox.unity"})`
- Error: schema rejected — unknown property `scene_path`
- Root cause: param name is `path`, not `scene_path` (also note camelCase rule — `path` happens to be one word so case-irrelevant here)
- Fix: `manage_scene({action: "load", path: "Assets/_Project/Scenes/House_Graybox.unity"})`

**6. `execute_code` failing compilation when script begins with `using` directives**
- Tried: pasting a normal C# snippet starting with `using UnityEngine;` / `using System.Text;` at the top
- Error: compilation error — `using` not valid at this position
- Root cause: `execute_code` runs the body as a method body, NOT a full file. `using` directives at file scope are illegal inside a method.
- Fix: omit `using` directives entirely and fully-qualify types (`UnityEngine.GameObject`, `System.Text.StringBuilder`, `UnityEngine.SceneManagement.SceneManager`, etc.). Or rely on the implicit usings the harness injects (verify per-call by attempting unqualified first; if it errors, switch to fully-qualified).

---

## Append Template

```
### YYYY-MM-DD — <session description>

**N. <tool/pattern> with <bad pattern>**
- Tried: `<call or approach>`
- Error: `<exact error or symptom>`
- Cost: <N wasted calls + recovery actions>
- Root cause: <why>
- Fix: <correct pattern>
```
