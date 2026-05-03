# Deep Modules Design Specification

> **Source:** ["Your codebase is NOT ready for AI (here's how to fix it)"](https://www.youtube.com/watch?v=uC44zFz7JSM) — Matt Pocock (Feb 26, 2026)  
> **Reference:** *A Philosophy of Software Design* — John Ousterhout

---

## Overview

**Deep Modules** is an architectural pattern that organizes a codebase into large, self-contained feature clusters — each exposing a *simple, intentional public interface* while hiding complex implementation details inside. This design has always been considered good software practice, but it has become **essential** in the age of AI-assisted development.

The core premise: **your codebase is the biggest influence on AI output** — more than the prompt, more than your `agents.md` file. A well-structured codebase enables AI to work effectively; a poorly structured one creates confusion, feedback gaps, and cognitive burnout.

---

## The Problem: Shallow Modules

Most codebases consist of many small, shallow modules with unrestricted cross-imports. This creates:

- **Navigation failure** — AI has no memory of your codebase. Each new conversation is like a new developer stepping in with no context ("the guy from Memento").
- **Slow feedback loops** — AI can't tell if its changes worked because there's no clear test boundary.
- **Cognitive overload** — The developer must manually hold the entire inter-module relationship graph in their head.
- **Invisible structure** — The mental map of the codebase exists only in the developer's head; it is not reflected in the file system.

---

## The Solution: Deep Modules

A deep module has:

- **Large implementation surface** — significant internal complexity and logic
- **Simple public interface** — a small, intentional API surface (types, exported functions, components)
- **Clear boundaries** — imports go *through* the interface, not around it

Instead of many small interconnected modules, the codebase becomes a set of well-named feature services — e.g., `VideoEditorService`, `AuthService`, `ThumbnailService` — each owning their own folder and exposing only what callers need.

---

## Three Core Benefits

### 1. Navigability (Progressive Disclosure of Complexity)

- The file system directly reflects the mental map of the codebase
- AI can scan top-level folder names to orient itself immediately
- Public types/interfaces sit at the entry point of each module — AI reads those first
- Implementation details are available on demand, not forced upfront
- AI behaves like a capable new hire who reads the API docs before diving into source

### 2. Reduced Cognitive Burnout

- Developer only needs to hold 7–8 "lumps" of functionality in mind
- AI manages internals; developer manages interfaces and how modules fit together
- No need to track dozens of cross-cutting import relationships
- Clean mental model = faster, more confident decision-making

### 3. Locked, Testable Behavior (Graybox Modules)

- Each module is a "graybox": you *can* look inside, but you don't *have* to
- Tests lock down module behavior at the interface level
- AI can freely refactor internals as long as tests pass
- This enables a clear division of labor: developer owns interface design, AI owns implementation

---

## Key Design Principles

| Principle | Description |
|-----------|-------------|
| **Interface First** | Design the public API before writing implementation |
| **Boundary Taste** | Apply human judgment at module boundaries — what goes in, what stays out |
| **One Folder, One Service** | Each deep module lives in its own directory with a clear entry point |
| **Types as Contracts** | Export types at the top of each module so AI (and humans) can understand behavior without reading code |
| **Test at the Interface** | Write tests against the public API, not internal implementation details |
| **File System = Mental Map** | The directory structure should mirror the logical architecture of the system |

---

## What This Is NOT

- **Not vibe coding.** Deep modules require significant architectural taste and discipline.
- **Not a new idea.** This is 20-year-old software practice (see Ousterhout's *A Philosophy of Software Design*). It maps to patterns like hexagonal architecture, ports & adapters, vertical slice / feature slice design, and facade/wrapper patterns.
- **Not about AI only.** What works for humans — clean interfaces, clear boundaries, testable units — works equally well (or better) for AI.

---

## Applying This in Practice

**Every coding session should include:**

1. **Plan phase** — Identify which deep modules are affected before writing any code
2. **Interface design** — Define or update the public interface/types for each affected module
3. **Test design** — Write or update tests that lock down expected behavior at the interface
4. **Implementation** — Delegate internal changes to AI with confidence that tests will catch regressions
5. **PRD / Issue writing** — When converting PRDs to implementation tasks, scope each issue to a specific module and its interface contract

**File structure pattern:**
```
src/
  services/
    auth/
      index.ts          ← Public interface (exports only)
      types.ts          ← Exported types/contracts
      auth.service.ts   ← Implementation (AI-managed)
      auth.test.ts      ← Tests locked to interface behavior
    video-editor/
      index.ts
      types.ts
      ...
    thumbnail/
      ...
```

---

## Reference

- **Video:** [Matt Pocock — "Your codebase is NOT ready for AI"](https://www.youtube.com/watch?v=uC44zFz7JSM)
- **Book:** *A Philosophy of Software Design* — John Ousterhout
- **Newsletter:** [AI Hero](https://aihero.dev/s/j6UNFU)
