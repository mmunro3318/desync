# README -- Research Requests

## 🔬 Deep Research Handoff Protocol — Spatial Horror Game (Unity/C#)

You are developing a **spatial/liminal horror game in Unity using C#**. Throughout development, you will encounter questions, design decisions, and technical rabbit holes where deep external research would meaningfully improve your work. Your job is to **proactively identify those moments** and write research request files to the `handoff-prompts/research-requests/` directory so Michael can copy them into Perplexity's Deep Research mode.

Fed into Perplexity's Deep Research mode to get comprehensive reports and guides for Unity development (or *anything*, really) and how to handle 3D graphics, rendering, debugging graphics artifacts; and level generation/management... your doc can include several separate prompts I'll feed one-at-a-time: specify which prompts to feed sequentially, which build/feed off prior context/results, and which prompts (prompt batches) should be separate threads (clean context). 

You're not restricted to graphics/levels questions -- generate **_any_** research requests that you think will empower the AI coding tools now, or later in development as well -- where are the dark corners and unknowns that I can address and gather context on now and while the AIs work on the first steps/M1 (after our graphics fix). You can see the full picture, so think of all the right questions we should be asking (or that future Claude should/would be asking) that would come up during those sprints (or might sabotage us if we miss the right answers to those questions, fail to ask, and Claude makes wrong assumptions...). I get 50 Deep Research requests/month, and never use them all... Let's use them... 

---

## 📁 File Naming Convention

Use this format:

```
handoff-prompts/research-requests/YYYY-MM-DD_<type>_<slug>.md
```

**Types:**
- `sequential` — A series of prompts meant to be fed one after another into the **same Deep Research thread** (builds context)
- `oneshot` — A standalone prompt for a **fresh context window**
- `basecamp` — A context-priming prompt (no research activation needed) to be sent *before* activating Deep Research
- `qa-loop` — Perplexity will ask Mike questions; he copies answers back into the thread before continuing
- `follow-up` — Dive deeper on topics/concepts after you've read new reports/research and want to learn more

**Examples:**
```
2026-05-03_oneshot_liminal-space-psychology.md
2026-05-03_sequential_procedural-horror-audio.md
2026-05-03_basecamp_spatial-horror-design-primer.md
2026-05-03_qa-loop_enemy-behavior-design.md
```

---

## 📄 File Format

Each file must follow this template:

```markdown
# [Short Title]

**Type:** [oneshot | sequential | basecamp | qa-loop]
**Thread:** [new | continue: <slug-of-prior-file>]  ← only for sequential/qa-loop
**Priority:** [high | medium | low]
**Context:** [1–2 sentences on why this matters right now in development]

---

## Prompt(s)

### Prompt 1 [of N]
[The actual prompt text Michael will paste into Perplexity]

### Prompt 2 [of N] ← sequential only, paste after Prompt 1 response
[Next prompt that builds on the previous response]
```

---

## 🧠 When to Write a Request

Write a research request when you encounter **any of the following**:

### Design & Narrative
- Questions about horror genre conventions (liminal space theory, analog horror, dread mechanics)
- Level/environment design decisions (spatial disorientation, architecture that feels "wrong")
- Narrative structure choices (non-linear storytelling, environmental storytelling, unreliable narrator)
- Player psychology questions (fear response, tension vs. terror, when to withhold vs. reveal)
- Tone or atmosphere questions ("what makes a space feel uncanny?")

### Technical (Unity / C#)
- Shader or rendering techniques for unsettling visuals (fog, screen distortion, lighting tricks)
- Audio design patterns (binaural audio, diegetic sound design for horror, dynamic ambience)
- Procedural generation research (level generation that feels "wrong-but-coherent")
- AI behavior systems for non-hostile, stalker, or ambiguous enemies
- Performance profiling / LOD strategies for atmospheric environments
- Occlusion culling in liminal/corridor spaces
- Unity HDRP/URP decisions relevant to horror aesthetics

### Game Feel & Mechanics
- Movement and camera choices that induce unease (head bobbing, slow turn speed, restricted FOV)
- Inventory / interaction systems in horror games
- Sanity / perception mechanic precedents
- Save system design in horror (autosave vs. manual, permadeath considerations)

### Reference & Competitive
- Specific games to study as reference (mechanics, art, design)
- Academic or design theory research (architecture, psychology, philosophy)
- Sound design or music composition techniques used in horror

---

## 🔁 Thread Structure Rules

Follow these rules to help Michael manage context efficiently:

### `oneshot`
- Self-contained. No prior context assumed.
- Use for: reference research, historical/theory topics, game comparisons, single technical questions
- Example trigger: "I need to know how fog of war was implemented in Amnesia"

### `sequential`
- A 2–5 prompt chain. Each prompt assumes the previous response was just given.
- Always label prompts `### Prompt 1 of 3`, `### Prompt 2 of 3`, etc.
- The first prompt establishes context; each subsequent one narrows or deepens it
- Use for: multi-part technical deep dives, iterative design exploration
- Example trigger: "I want to understand procedural horror level design from theory → Unity implementation → specific patterns"

### `basecamp`
- A plain-language context dump with no question — just background for Perplexity to absorb
- Paste this *before* activating Deep Research or beginning a sequential thread
- Format: project description + current implementation state + vocabulary/terminology
- Use for: anytime a new major thread is being started and context is complex

### `qa-loop`
- Perplexity will generate questions. Michael pastes the Q&A back as a follow-up.
- Include the instruction: *"Before researching, ask me 3–5 clarifying questions about my game's specific context, then wait for my answers before generating a report."*
- Use for: design decisions where the right answer depends heavily on project specifics
- Example trigger: "I'm designing the enemy AI but I haven't decided on its core behavior loop yet"

### `follow-up`
- After reading/reviewing research reports and you have follow-up questions or want deeper dives or to follow a rabbit hole
- Specifiy originating research request prompt, and whether you want the new prompt(s) you prove to be entered into the same/originating context, or a new thread
- Use for: clarifaction, deepening understanding, pursing new ideas sparked by results, etc.

---

## ✍️ Example Research Requests

---

### Example 1: `oneshot` — Liminal Space Psychology

```markdown
# Liminal Space Psychology & Horror Design

**Type:** oneshot
**Thread:** new
**Priority:** high
**Context:** Designing the game's core environments and need psychological grounding for what makes liminal spaces feel threatening.

---

## Prompt

I'm designing a spatial horror game set in liminal environments (empty hallways, transitional architecture, backrooms-style spaces). Please research the psychology and theory behind why liminal spaces feel unsettling to humans. Cover:

1. The psychological concept of liminality (van Gennep, Victor Turner) and how it maps to spatial experience
2. Why certain architectural spaces (hotel lobbies at 3am, empty malls, stairwells) feel uncanny — reference cognitive and evolutionary theories
3. How horror game designers (e.g., Kitamura, Thomas Grip of Frictional Games, the creators of Anatomy or Haunted PS1 games) have articulated their design philosophy around this
4. Specific environmental design principles that maximize discomfort without relying on jump scares

Generate a structured report with citations I can use as a design reference document.
```

---

### Example 2: `basecamp` + `sequential` — Horror Audio Design

```markdown
# Horror Audio Design — Basecamp Primer

**Type:** basecamp
**Thread:** new
**Priority:** medium
**Context:** Starting a research thread on dynamic audio for horror. Paste this before activating Deep Research.

---

## Prompt (paste first, no Deep Research yet)

I'm building a spatial horror game in Unity (C#) using HDRP. The game is set in liminal/transitional interior spaces — think empty office buildings, hospitals, brutalist architecture at night. The player is alone; there are no traditional enemies yet, though I'm considering a stalker entity. The game relies heavily on atmosphere rather than action. I'm currently using Unity's audio mixer and AudioSource components but haven't implemented any adaptive audio systems yet. I use FMOD or am considering it. My game has no score yet — I want to understand whether I should compose one, generate procedural audio, or use field recordings + synthesis. Please hold any questions until after I activate Deep Research mode.
```

```markdown
# Horror Audio Design — Sequential Thread

**Type:** sequential
**Thread:** continue: horror-audio-basecamp
**Priority:** medium
**Context:** Continuation after basecamp primer above.

---

## Prompt 1 of 3

Research the state of the art in horror game audio design. Cover:
- The distinction between diegetic and non-diegetic sound in horror and when to use each
- How games like Alien: Isolation, Silent Hill 2, and Hellblade use adaptive/reactive audio
- The psychological mechanics of sound that induce dread vs. fear vs. unease
- Field recording vs. synthesis vs. procedural generation for horror ambience

---

## Prompt 2 of 3
(Paste after receiving response to Prompt 1)

Now focus specifically on **Unity implementation**:
- Comparing FMOD, Wwise, and Unity's built-in audio for adaptive horror audio
- How to architect a dynamic ambience system that reacts to player state (heart rate simulation, proximity to anomalies, time in darkness)
- Specific Unity/C# patterns for triggering audio events based on spatial conditions
- Any open-source or Asset Store tools worth examining

---

## Prompt 3 of 3
(Paste after receiving response to Prompt 2)

Finally, give me a **prioritized implementation roadmap** for adding atmospheric audio to an early-stage Unity horror game. Assume a solo developer with intermediate C# skills. What do I implement first, what can wait, and what common mistakes should I avoid?
```

---

### Example 3: `qa-loop` — Enemy AI Design

```markdown
# Enemy/Entity AI Design — QA Loop

**Type:** qa-loop
**Thread:** new
**Priority:** high
**Context:** Haven't finalized the enemy behavior loop. Need research tailored to my specific design decisions.

---

## Prompt

I'm designing an entity/enemy for a spatial horror Unity game. Before you research anything, please ask me 4–5 clarifying questions about my game's specific design context — things like: Is the enemy visible? What triggers it? What is the player's goal when encountering it? What emotional response am I designing for? 

Wait for my answers before generating any report or recommendations. Once I answer, produce a research report on:
- Horror game enemy AI archetypes (stalker, ambient, reactive, scripted illusion)
- Academic and GDC research on AI behavior in horror
- Unity NavMesh and behavior tree patterns best suited to my specific entity type
- Player perception tricks to make simple AI feel threatening and unpredictable
```

---

## 🚫 Don't Write a Request When...

- The answer is in Unity documentation or C# reference (just look it up)
- It's a bug you can reproduce and fix without design context
- It's a question about your own game's internal logic (ask Michael directly)
- It's something you can resolve with a quick Stack Overflow or GitHub search

---

## 📌 Reminder

Michael checks `handoff-prompts/research-requests/` regularly. Write clear, copy-paste-ready prompts — he pastes them directly into Perplexity. The better the prompt, the better the research report he brings back. Think of these files as **briefs to a research assistant who is very capable but knows nothing about your specific project** unless you tell them.
