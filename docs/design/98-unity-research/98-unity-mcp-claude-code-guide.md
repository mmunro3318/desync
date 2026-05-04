# Unity MCP (Coplay) — Claude Code Developer Guide

> **Purpose:** This guide helps Claude Code understand how to effectively write code and features for a Unity project integrated with the Coplay `unity-mcp` server. All prompts, scripts, and agentic actions should be informed by this document.

---

## Architecture Overview

The Coplay Unity MCP system uses a **two-component bridge** between your AI client (Claude) and the Unity Editor:

| Component | Language | Role | Where It Runs |
|-----------|----------|------|---------------|
| **Unity Bridge** | C# | Executes commands inside Unity Editor — the "hands" | Inside the Unity project |
| **MCP Server** | Python | Translates AI requests into Unity Bridge instructions — the "brain" | Local server on your machine |

The protocol is **JSON-RPC 2.0** over standard I/O (stdio). Claude speaks to the Python MCP server, which speaks to the C# Unity Bridge.

---

## Prerequisites & Setup

Before any code can be written or tools invoked, verify these are in place:

- **Unity Hub & Editor:** Version 2021.3 LTS or newer
- **Python:** 3.12 or newer
- **uv (Python package manager):**
  ```bash
  # macOS/Linux
  curl -LsSf https://astral.sh/uv/install.sh | sh

  # Windows PowerShell
  powershell -c "irm https://astral.sh/uv/install.ps1 | iex"
  ```
- **Unity Package installed** via `Window > Package Manager > Add from Git URL`:
  ```
  https://github.com/CoplayDev/unity-mcp.git
  ```
- **AI Client configured** via `Window > MCP for Unity > Auto-Setup` (or manual JSON config)
- **Claude Desktop config path:** `%APPDATA%\Claude\claude_desktop_config.json`
- **Verification:** In the Unity MCP window, both server and bridge indicators should show green

---

## Available MCP Tool Calls

When writing code or features, Claude Code will use these MCP tools via the server. **Always prefer these over manual file creation when in an MCP session:**

| Tool | Purpose | Example Use |
|------|---------|-------------|
| `manage_scene` | Create/modify scenes, set up lighting | `"Create a new scene with basic directional lighting"` |
| `manage_gameobject` | Create, position, scale, parent GameObjects | `"Create a capsule at position (0, 1, 0) named 'Player'"` |
| `manage_script` | Create new C# script files | `"Create a script named 'PlayerController.cs'"` |
| `script_apply_edits` | Write or modify C# code inside scripts | Write movement logic, jump code, collision handlers |
| `manage_component` | Attach/detach components on GameObjects | `"Attach 'PlayerController' to the Player object"` |

---

## Agentic Workflow Pattern

When given a complex task (e.g., "build a platformer level"), Claude Code should follow this **plan-first, execute-second** pattern:

1. **Generate a markdown to-do list** before making any tool calls
2. **Outline all sub-steps** (create scene → place objects → write scripts → attach scripts → verify)
3. **Execute tool calls sequentially**, one logical step at a time
4. **Report back after each major step** before proceeding

> ✅ **Example prompt that works well:**
> `"Build me a simple 2D platformer using 3D objects. Create a level with a player, 5 platforms, and a goal object."`

---

## C# Scripting Guidelines

### Input System — Critical Gotcha

Unity projects may use either the **legacy Input Manager** or the **new Input System package**. Always verify which is active:

- **Check:** `Project Settings > Player > Other Settings > Active Input Handling`
- **Safe option:** Set to `"Both"` to support code generated for either system
- **When writing scripts**, prefer the new Input System for projects on Unity 2021+:
  ```csharp
  using UnityEngine.InputSystem;
  // Use InputAction references instead of Input.GetKey()
  ```

### Boilerplate to Include in All Player Scripts

```csharp
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 8f;

    private Rigidbody rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Movement
        float h = Input.GetAxis("Horizontal");
        Vector3 move = new Vector3(h, 0, 0) * moveSpeed;
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, 0);

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    void OnCollisionExit(Collision col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }
}
```

### Camera Follow Script

```csharp
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 5, -10);
    public float smoothSpeed = 0.125f;

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 desired = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed);
    }
}
```

---

## Prompt Engineering for Unity MCP

### High-Signal Prompt Patterns

These prompt structures consistently produce good results in Unity MCP sessions:

```
# Scene setup
"Create a new scene called 'Level_01'. Add a directional light, set sky color to light blue."

# Player creation
"Create a 3D capsule named 'Player' at (0, 1, 0). Add a Rigidbody (freeze Z rotation). Create and attach a PlayerController.cs with WASD movement and spacebar jump."

# Level geometry
"Create 8 cube platforms of scale (3, 0.5, 3) arranged in a rising staircase pattern from (0,0,0) to (20, 8, 0). Tag them all as 'Ground'."

# Goal object
"Create a golden sphere at (22, 10, 0) named 'Goal'. Add a trigger collider. Create a GoalTrigger.cs script that prints 'Level Complete!' when the Player enters."

# Shader/material
"Create a new material named 'PlatformMat', set albedo color to (0.3, 0.6, 0.9). Apply it to all platform objects."
```

### Structuring Complex Requests

Break large features into staged prompts:

```
Stage 1: "Set up the scene structure and lighting."
Stage 2: "Create the player character with physics."
Stage 3: "Build the level geometry."
Stage 4: "Add win condition and UI."
```

This prevents context overload and makes debugging easier if a step fails.

---

## The 90/10 Rule — AI's Limits

> **The AI gets you 90% of the way there. The final 10% needs your expertise.**

Common issues that require human review after AI generation:

| Issue | Symptoms | Fix |
|-------|----------|-----|
| Input System mismatch | Player won't move on Play | `Project Settings > Player > Active Input Handling → "Both"` |
| Missing tag | `CompareTag("Ground")` fails silently | Add the tag in `Edit > Project Settings > Tags and Layers` |
| Rigidbody constraints | Player slides/rotates unexpectedly | Freeze axes in Rigidbody component inspector |
| Script not attached | Logic never executes | Verify script is on the correct GameObject in Hierarchy |
| Missing layer collision | Physics triggers don't fire | Check `Edit > Project Settings > Physics > Layer Collision Matrix` |

**Always hit Play and check the Console** after AI-driven changes. Console errors are your guide for follow-up prompts.

---

## Performance

### Performance Notes

- **API Token Cost:** Agentic MCP sessions use ~27.5% more tokens than standard completions (the agent fetches more context per operation). Budget accordingly for long sessions.
- **Local-only:** The MCP Python server runs **100% locally**. Project data is only sent to your chosen LLM API (e.g., Anthropic's API for Claude) — not to Coplay.

---

## Troubleshooting Quick Reference

| Symptom | Cause | Solution |
|---------|-------|----------|
| `uv: command not found` | `uv` not in PATH | Re-run the uv install script and restart terminal |
| `unityMCP` not visible in Claude | Config JSON wrong or in wrong file | Use Manual Setup tab; copy exact JSON block and file path |
| Server status "Not Connected" | Python/uv not installed | Re-run prereqs, restart Unity |
| Package install fails | Git not in PATH | Install Git, restart Unity |
| Bridge status "Not Running" | Unity Editor lost connection | Restart Unity Editor |

---

## Repository Structure Recommendations

```
your-unity-project/
├── Assets/
│   ├── Scripts/
│   │   ├── Player/
│   │   │   ├── PlayerController.cs
│   │   │   └── CameraFollow.cs
│   │   ├── Level/
│   │   │   ├── GoalTrigger.cs
│   │   │   └── LevelManager.cs
│   │   └── UI/
│   │       └── UIManager.cs
│   ├── Scenes/
│   │   └── Level_01.unity
│   ├── Materials/
│   └── Prefabs/
├── Packages/
│   └── manifest.json          ← unity-mcp package reference lives here
└── ProjectSettings/
    └── InputManager.asset
```

---

## Workflow Comparison: Before vs. After MCP

| Task | Manual Workflow | AI-Driven Prompt |
|------|----------------|-----------------|
| Create player character | Create GO → Add Rigidbody → Add Collider → Write script → Attach script | `"Create a player capsule at origin with physics-based movement"` |
| Build a level | Place dozens of prefabs, adjust positions, test layout | `"Generate a platformer level with 10 platforms and a goal at (20, 8, 0)"` |
| Fix a shader | Open shader graph, tweak nodes, recompile, repeat | `"The water shader is too opaque — make it more transparent with a ripple effect"` |

A prompt-to-playable demo (simple platformer) takes **~15 minutes** with AI vs. **1+ hour** manually.

---

## References

- **GitHub Repo:** [CoplayDev/unity-mcp](https://github.com/CoplayDev/unity-mcp)
- **Coplay Blog (Free vs. Premium comparison):** [coplay.dev/blog](https://www.coplay.dev/blog/comparing-coplay-and-unity-mcp)
- **MCP C# SDK (Microsoft-maintained):** [modelcontextprotocol/csharp-sdk](https://github.com/modelcontextprotocol/csharp-sdk)
- **Video Walkthrough:** [Justin P Barnett — Free Unity AI Tool](https://www.youtube.com/watch?v=0ndReIFjV2A)
- **Custom Tools Guide:** See `CUSTOM_TOOLS.md` in the repo for adding your own Python MCP tools

---

*Last updated: May 2026 — Based on CoplayDev/unity-mcp public documentation and Skywork deep-dive analysis.*
