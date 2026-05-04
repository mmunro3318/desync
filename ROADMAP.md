# ROADMAP — DESYNC

> A first-person co-op spatial horror game where an ordinary house folds into a looping shadow-structure whenever nobody is looking, and players must destroy anchors to make reality hold still long enough to escape.

**Jam:** Pride Jam 2026 | **Deadline:** 2026-06-12 | **Theme:** "Asylum"

---

## Status Dashboard

| Sprint | Status | Blocked By | Unblocks | Release |
|--------|--------|------------|----------|---------|
| S0.1 Multiplayer Fix | ✅ Complete | — | S1A, S1B | POC |
| S0.2 Light Leak Fix + Graphics Deep Dive | ✅ Complete | — | S1A, S1B | POC |
| S1A House Graph Authoring | 🔲 Not Started | S0.1, S0.2 | S1B, S2, S3, S5A | POC |
| S1B Portal Visibility + Node Activation | 🔲 Not Started | S1A | S2 | POC |
| S2 Observation Lock System | 🔲 Not Started | S1B | S3, S4A, S4B | POC |
| S3 Loop Anomaly Vertical Slice | 🔲 Not Started | S2 | S4A, S4B, S6 | POC |
| S4A Substitution Anomaly | 🔲 Not Started | S3 | S6 | Jam |
| S4B Tardis Anomaly | 🔲 Not Started | S3 | S6 | Jam |
| S5A Anchor Core State | 🔲 Not Started | S1A | S6 | Jam |
| S6 Match Flow + Game Loop | 🔲 Not Started | S3, S5A | M3 | Jam |
| M3 Atmosphere + Assets | 🔲 Not Started | S6 | M4 decision gate | Jam |
| M4 Stalker Entity | 🔲 Not Started | M3 decision gate | — | Stretch |

**Critical path:** `S0.1/S0.2 → S1A → S1B → S2 → S3 → S6 → M3`

**Parallel opportunities:**
- S0.1 and S0.2 can run concurrently
- S4A, S4B, and S5A can all run in parallel (after their respective blockers clear)
- S5A can begin as soon as S1A is done (does not need S2 or S3)

---

## Dependency Graph

```
S0.2 Light Leak ───────┐
                       ├──► S1A ──► S1B ──► S2 ──► S3 ──┬──► S4A ──┐
S0.1 Multiplayer fix ──┘         │                      ├──► S4B ──┼──► S6 ──► M3 ──► M4?
                                 │                      │          │
                                 └──────────────────────┼──► S5A ──┘
                                                        │
                                                   (parallel)
```

---

## Release Milestones

### Release: POC (Proof of Concept) — Target: Weeks 1–3

**Contains:** M0 cleanup (S0.1, S0.2) + M1 spatial core (S1A, S1B, S2, S3)

**Gate question:** *Is looping impossible continuity fun and readable before any content breadth?*

**Done when:**
- [ ] Player walks a hand-authored house graph
- [ ] Loop anomaly fires when unobserved, producing impossible but rule-bound continuity
- [ ] Observation lock system visibly constrains mutations
- [ ] Debug overlay renders full graph state, mutation eligibility, and observation locks
- [ ] Multiplayer validated: both players see consistent graph state, no drift

---

### Release: Jam Submission — Target: 2026-06-12

**Contains:** M2 game loop (S4A, S4B, S5A, S6) + M3 atmosphere

**Gate question:** *Does the full round work as a coherent horror game, not just a tech demo?*

**Done when:**
- [ ] Complete match flow: lobby → load → explore → anchor destruction → collapse → victory/defeat
- [ ] At least one anchor destroyable in a run (expand to 3 for full loop)
- [ ] Timer-based lose mechanic as MVP fallback
- [ ] Geometry collapse sequence on final anchor destruction
- [ ] House looks and sounds like a house (textures, ambient audio, footsteps)
- [ ] Mutation nodes visually distinct from baseline (texture treatment replaces color filters)
- [ ] Player models visible to each other (will-o-wisp minimum)
- [ ] House "as a character" — audible groaning/shuddering during mutations
- [ ] Multiplayer validated end-to-end: full round with 2 players on LAN

---

### Release: Stretch / Post-POC

**Contains:** M4 stalker entity + Track B (pipeline/DSL/importer)

**Decision gate after M3:** Assess whether to pursue stalker entity or polish existing content for jam submission.

**M4 done when:**
- [ ] Primitive entity with FSM (idle/patrol/flee/hunt) lives in the house
- [ ] Entity navigates graph, repaths on mutations
- [ ] Entity behavior scales with anchor progression
- [ ] Player death on entity contact (simple message + despawn for MVP)
- [ ] Gate question answered: *Does entity presence add to or detract from the spatial horror?*

**Track B (deferred to post-POC, not necessarily post-jam):**
- House layout DSL and importer pipeline
- Automated graph generation from DSL definitions
- Content validator and debug viz for imported layouts
- Scalable room variant authoring

---

## Sprint Detail

---

### S0.1 — Light Leak Fix + Graphics Deep Dive

**Milestone:** M0 (Foundation cleanup) | **Release:** POC
**Blocked by:** — | **Unblocks:** S1A, S1B
**Parallel with:** S0.2

**Source:** `docs/design/98-unity-research/03-unity-urp-graphics-lighting-horror-report.md`
**Scene:** `unity-DESYNC/Assets/_Project/Scenes/House_Graybox.unity`

**Objective:** Fix the known floor-to-floor light leak in House_Graybox. Use this as a forcing function to deeply understand URP lighting, shadow cascades, and light probe behavior in Unity 6 — preventing compounding graphics ignorance in later milestones.

**Acceptance Criteria:**
- [x] Light leak identified, root-caused, and fixed (2026-05-04)
- [x] Document findings in `docs/ARCH.md` (URP lighting decisions) (2026-05-04)
- [ ] Validate fix doesn't break AtmosphereVolumeProfile mood (visual check in Play mode — pending)
- [x] Developer confidence: can explain how Unity handles light boundaries between floors (2026-05-04)

---

### S0.2 — Research Spike: Unity Geometry & Streaming

**Milestone:** M0 (Foundation cleanup) | **Release:** POC
**Blocked by:** — | **Unblocks:** S1A
**Parallel with:** S0.1

**Objective:** Deep research on how Unity handles runtime geometry loading/unloading, additive scene management, and room-based streaming. Answer: can we reference the abstract graph and load only room nodes that are occupied + adjacent? What are the performance characteristics of load/unload cycles?

**Acceptance Criteria:**
- [ ] Research report written (Unity geometry loading patterns, additive scenes vs prefab instantiation vs addressables)
- [ ] Recommendation locked: which approach for room node loading in DESYNC
- [ ] Performance envelope documented (how many room nodes before lag)
- [ ] Decision recorded in `docs/ARCH.md`

**Key questions to answer:**
- [ ] Can room nodes be loaded/unloaded independently at runtime?
- [ ] What's the cost of instantiate vs addressable load vs additive scene?
- [ ] Can we keep baseline house geometry always-loaded (it's small)?
- [ ] How do we handle portal/threshold visuals during load transitions?

---

### S1A — House Graph Authoring

**Milestone:** M1 (Spatial Core) | **Release:** POC
**Blocked by:** S0.1, S0.2 | **Unblocks:** S1B, S2, S3, S5A
**Parallel with:** Nothing (critical path)

**Source doc:** `docs/design/04-sprints/sprint-1a-house-graph-authoring.md`
**Epic:** `docs/design/02-architecture/house-graph-core-epic.md`
**Contracts:** `docs/design/02-architecture/networked-house-runtime-interfaces-contracts.md`

**Objective:** Author a minimal house graph as data and materialize it as a queryable runtime system. This is the foundation every other system builds on.

**Acceptance Criteria (from docs):**
- [ ] Canonical house graph definition authored (5-8 nodes: entry, halls, rooms, connector)
- [ ] `HouseGraphInstance` runtime loads from authored data
- [ ] Runtime exposes node/edge/portal queries (adjacency, occupancy, active layer)
- [ ] Graph state replicates over network (NetworkVariable-based)
- [ ] Room nodes manifest as geometry in scene (load/unload per S0.2 findings)
- [ ] Debug overlay renders graph topology, node states, edge connections
- [ ] Quick restart supported (graph reset without scene reload)

**Personal Gates (Mike):**
- [ ] Monitor room node load/unload — no visual artifacts, shearing, or rubber-banding
- [ ] Color-coded mutation vs baseline nodes visible (blue/green filter for dev)
- [ ] Graph debug view functional (mini-map or overlay showing node layout)
- [ ] Multiplayer: both players see same graph state, no drift after mutations

---

### S1B — Portal Visibility + Node Activation

**Milestone:** M1 (Spatial Core) | **Release:** POC
**Blocked by:** S1A | **Unblocks:** S2
**Parallel with:** Nothing (critical path)

**Source doc:** `docs/design/04-sprints/sprint-1b-portal-visibility-node-activation.md`
**Spec:** `docs/design/03-systems/portal-visibility-local-render-streaming-spec.md`

**Objective:** Implement node activation (which rooms are "live") and portal visibility (what the player sees through doorways/thresholds). Portals are the visual seam between graph nodes.

**Acceptance Criteria (from docs):**
- [ ] Node activation model: occupied + adjacent nodes are active/rendered
- [ ] Portal rendering: player sees correct destination through doorways
- [ ] Threshold crossing triggers node transition cleanly
- [ ] Inactive nodes unloaded/hidden (no popping, no visual holes)
- [ ] Debug overlay shows active/inactive nodes, portal destinations
- [ ] Multiplayer: both players' active sets computed independently but consistently

**Personal Gates (Mike):**
- [ ] Walking through a portal feels seamless — no loading hitch or frame stutter
- [ ] Looking through a doorway shows the correct connected room (not void)

---

### S2 — Observation Lock System

**Milestone:** M1 (Spatial Core) | **Release:** POC
**Blocked by:** S1B | **Unblocks:** S3, S4A, S4B
**Parallel with:** Nothing (critical path)

**Source doc:** `docs/design/04-sprints/sprint-2-observation-lock-system.md`
**Spec:** `docs/design/03-systems/co-op-observation-and-sync-rules-spec.md`

**Objective:** Implement the rule that observed spaces cannot mutate. This is the core "horror contract" — space only changes when you can't see it.

**Acceptance Criteria (from docs):**
- [ ] Visibility-based lock: nodes in player's validated observation set are locked
- [ ] Occupancy-based lock: occupied nodes are locked
- [ ] Grace timer: recently-observed nodes remain locked for N seconds
- [ ] Mutation eligibility query: runtime answers "can this node/edge mutate right now?"
- [ ] Debug overlay shows lock state per node (locked/eligible/grace-cooling)
- [ ] Multiplayer: each player's observation contributes to shared lock state (server authority)

**Personal Gates (Mike):**
- [ ] Can clearly see in debug that walking away from a room unlocks it after grace period
- [ ] Two players observing different rooms correctly prevents mutation in both

---

### S3 — Loop Anomaly Vertical Slice

**Milestone:** M1 (Spatial Core) | **Release:** POC
**Blocked by:** S2 | **Unblocks:** S4A, S4B, S6
**Parallel with:** Nothing (critical path)

**Source doc:** `docs/design/04-sprints/sprint-3-loop-mutation-vertical-slice.md`
**Epic:** `docs/design/03-systems/anomaly-families-epic.md`

**Objective:** First playable spatial anomaly. A corridor/connector remaps its destination when unobserved, creating impossible continuity (you walk forward but end up where you started, or somewhere unexpected).

**Acceptance Criteria (from docs):**
- [ ] One loop anomaly family implemented as graph edge remap
- [ ] Anomaly only fires when observation lock permits (connector is unobserved)
- [ ] Player experiences impossible continuity that is locally coherent
- [ ] AnomalyDirector pattern established (reusable for future families)
- [ ] Debug overlay shows: active anomaly, affected edges, preconditions, blocking reasons
- [ ] Anomaly resets cleanly on round restart

**Personal Gates (Mike):**
- [ ] Walking a looped corridor genuinely feels disorienting, not glitchy
- [ ] Can reproduce the same loop condition reliably in debug
- [ ] Multiplayer: both players experience consistent loop behavior
- [ ] **POC gate question answered:** *Is this fun?*

---

### S4A — Substitution Anomaly (parallel track)

**Milestone:** M2 (MVP Loop) | **Release:** Jam
**Blocked by:** S3 | **Unblocks:** S6
**Parallel with:** S4B, S5A

**Source doc:** `docs/design/04-sprints/sprint-4a-substitution-anomaly-vertical-slice.md`

**Objective:** A familiar room resolves into a different authored variant when unobserved. Proves the AnomalyDirector can host multiple anomaly families.

**Acceptance Criteria (from docs):**
- [ ] One room node swaps to an authored variant while preserving edge connections
- [ ] Substitution obeys observation lock rules
- [ ] Player notices "this room changed" without visual glitch during transition
- [ ] Debug overlay shows substitution state and variant ID

**Personal Gates (Mike):**
- [ ] Substituted room feels eerie, not broken
- [ ] Multiplayer: substitution consistent for both players

---

### S4B — Tardis Anomaly (parallel track)

**Milestone:** M2 (MVP Loop) | **Release:** Jam
**Blocked by:** S3 | **Unblocks:** S6
**Parallel with:** S4A, S5A

**Source doc:** `docs/design/04-sprints/sprint-4b-tardis-anomaly-vertical-slice.md`

**Objective:** Interior space behind a threshold contains more volume/depth than the exterior shell implies. A small subgraph gets inserted behind a door.

**Acceptance Criteria (from docs):**
- [ ] Subgraph (2-4 nodes) inserted behind a threshold when conditions met
- [ ] Entry/exit portals maintain spatial coherence (entering and leaving feel natural)
- [ ] Interior expansion obeys observation lock on the threshold
- [ ] Debug overlay shows inserted subgraph, entry point, and boundaries

**Personal Gates (Mike):**
- [ ] Walking into an expanded interior is surprising but not confusing
- [ ] Can exit back to the normal house without getting stuck
- [ ] Multiplayer: both players can enter and navigate the expansion together

---

### S5A — Anchor Core State (parallel track)

**Milestone:** M2 (MVP Loop) | **Release:** Jam
**Blocked by:** S1A | **Unblocks:** S6
**Parallel with:** S4A, S4B

**Source doc:** `docs/design/04-sprints/sprint-5a-anchor-core-state.md`
**Epic:** `docs/design/03-systems/anchor-artifact-loop-epic.md`

**Objective:** Implement the anchor/artifact objective system — the "why" of the game loop. Players find and destroy anchors to collapse the anomaly.

**Acceptance Criteria (from docs):**
- [ ] `AnchorManager` owns anchor lifecycle (spawn, active, destroyed)
- [ ] Anchor placed in graph node (static placement for MVP)
- [ ] Player can interact with and destroy an anchor
- [ ] Anchor state synced over network (server-authoritative)
- [ ] HUD readout: anchors remaining
- [ ] Debug overlay shows anchor locations and states

**Personal Gates (Mike):**
- [ ] Single anchor destruction works cleanly in multiplayer
- [ ] Destroying anchor triggers visible feedback (not just a state change)
- [ ] Foundation supports expanding to 3 anchors later (data-driven, not hardcoded)

---

### S6 — Match Flow + Game Loop

**Milestone:** M2 (MVP Loop) | **Release:** Jam
**Blocked by:** S3, S5A | **Unblocks:** M3
**Parallel with:** Nothing (convergence point)

**Source docs:** `docs/design/01-vision/spatial-horror-gdd.md` (match flow section)

**Objective:** Wire the full round: lobby → load → explore → anchor hunt → destruction → collapse → end. This is where individual systems become a game.

**Acceptance Criteria:**
- [ ] Match states: Lobby → Loading → Exploring → AnchorDestroyed → Collapsing → Won/Lost
- [ ] Lobby: host/join (existing), start match triggers load
- [ ] Load: baseline house + initial mutation spawned
- [ ] Explore: house grows/mutates on interval while players search
- [ ] Anchor destruction: triggers collapse sequence
- [ ] Collapse: geometry despawns progressively from anchor outward
- [ ] Win: all mutation nodes collapsed, players at exit = victory
- [ ] Lose: MVP timer (displayed countdown, 10-20 min) — placeholder for richer lose mechanics
- [ ] Player death: if in collapsing node → instant death message + despawn
- [ ] Round restart: return to lobby cleanly
- [ ] Expand to 3 anchors once single-anchor flow is proven
- [ ] Multiplayer: full round works with 2 players on LAN

**Personal Gates (Mike):**
- [ ] Can play a complete round start-to-finish without hitting a blocker
- [ ] Collapse sequence looks intentional, not like a bug
- [ ] Timer is visible and creates urgency
- [ ] **M2 gate:** *Does the full round work as a coherent game?*

---

### M3 — Atmosphere + Assets

**Milestone:** M3 | **Release:** Jam
**Blocked by:** S6 | **Unblocks:** M4 decision gate
**Parallel with:** Nothing (sequential after game loop proven)

**Source docs:** `docs/design/03-systems/lighting-and-visibility-spec.md`, `docs/design/03-systems/environmental-prop-taxonomy-asset-kit-spec.md`

**Objective:** Make the house look, sound, and feel like a horror game. Replace graybox with free-library assets. Establish texture pipeline for room types.

**Acceptance Criteria:**
- [ ] Wall/floor textures: appropriate per room type (kitchen tile, wallpaper, carpet, etc.)
- [ ] Mutation nodes: visually distinct from baseline via texture treatment (replaces color filters)
- [ ] Lighting: dark atmosphere, flashlight as primary light source
- [ ] Player models: visible to each other (will-o-wisp or simple model minimum)
- [ ] Audio: house groaning/shuddering during mutations (localized + migrating sound)
- [ ] Audio: ambient creaking, distant settling sounds on timer
- [ ] Furniture: pre-baked layouts for baseline house rooms (kitchen counters, bathroom items)
- [ ] Furniture gate: if friction/complications arise, defer — empty house is acceptable
- [ ] External: flat infinite landscape acceptable for MVP (trees/bushes experimental only)
- [ ] One mutation shudder animation/visual effect during mutation event

**Personal Gates (Mike):**
- [ ] House looks like a house (empty is fine, but textured)
- [ ] Mutation nodes look like bizarre extensions, not glitched geometry
- [ ] "House as a character" vibe — the house feels alive/reactive
- [ ] Players can see each other and infer orientation (flashlight direction helps)
- [ ] **M3 gate:** *Does atmosphere amplify the spatial loop without obscuring readability?*

**Decision gate after M3:** Assess stalker entity vs content polish for jam submission.

---

### M4 — Stalker Entity (Stretch)

**Milestone:** M4 | **Release:** Stretch
**Blocked by:** M3 decision gate | **Unblocks:** —
**Parallel with:** —

**Objective:** One FSM entity that lives in the impossible space and pressures navigation. It should feel like it understands the house better than the players.

**Acceptance Criteria:**
- [ ] FSM states: idle (dazed/sniffing), patrol (purposeful A* with pauses), flee (startled retreat), hunt (slow predatory stalk)
- [ ] Entity navigates house graph, repaths on mutations (path broken → repath)
- [ ] Primitive model: orb/sphere/wisp (textured beholder-style stretch goal)
- [ ] Behavior scales with anchor progression (3 anchors=passive, 2=defensive, 1=aggressive)
- [ ] Hunt: target player gets blurred vision edges, sluggish movement (Gaze effect)
- [ ] Player death on sufficient entity overlap → death message + despawn
- [ ] Entity sounds: roar/psychic scream (maybe triggers mutation node on flee)

**Personal Gates (Mike):**
- [ ] Entity feels like it *belongs* in the impossible space
- [ ] Presence adds dread, not frustration
- [ ] Entity navigating mutations looks intentional (it knows this world)
- [ ] **M4 gate:** *Does entity presence add to or detract from the spatial horror?*

**Post-MVP entity expansion (backlog):**
- Special movement: melt through walls, blink-step, dissipation into motes
- Richer hunt behaviors: anticipate player path, portal to cut off
- Custom model, rigging, animations
- Multiple entity types/variants

---

## Constraints & Principles

### Multiplayer-First
Every sprint gates on multiplayer validation. Do not develop systems that only work single-player and "will be networked later." Server-authoritative state, NetworkVariable sync, and 2-player LAN testing are non-negotiable at every milestone boundary.

### Scope Creep Management
Creative ideas that expand or complicate the current sprint go to `docs/TODO.md` under a **Creative Backlog** section. They are debated at the next sprint boundary, not mid-sprint. Claude should push back on deviations and make Mike justify inclusions.

### Debug-First
Every hidden system (graph state, observation locks, mutation eligibility, anchor state, entity FSM) gets a debug overlay/gizmo before the sprint is considered done. If you can't see it, you can't tune it.

### Personal Gates Supplement, Never Override
Mike's personal gates are additional success criteria. They do not replace or weaken the acceptance criteria defined in the source sprint docs.

### Decisions in ARCH.md
Key architectural decisions (especially "why" decisions and Unity-specific trade-offs) get recorded in `docs/ARCH.md` to prevent future drift.

---

## Deferred Work (Post-POC)

These items are explicitly not on the critical path. They are deferred until the POC proves the concept, not abandoned.

| Item | Source Doc | Deferred Because |
|------|-----------|-----------------|
| House Layout DSL | `docs/design/02-architecture/house-layout-dsl-spec.md` | Hand-authored graph sufficient for jam |
| Importer/Validator Pipeline | `docs/design/03-systems/house-builder-pipeline-epic.md` | No scale content yet |
| HB-1/HB-2 Builder Sprints | `docs/design/04-sprints/house-builder-pdd-sprint-hb1-hb2.md` | Post-POC pipeline |
| Co-op Observation Sprint | `docs/design/04-sprints/co-op-observation-sprint-pdd.md` | Needs 4 prior systems; 2-player LAN is sufficient for jam |
| Room Population Rules | `docs/design/03-systems/room-population-rules-spec.md` | Furniture is nice-to-have within M3, full system is post-POC |
| Asymmetric Play Mode | (concept only) | Requires debug tooling maturity post-MVP |
| Multiple House Levels | (concept only) | One house proves concept; Rose Manor etc. post-MVP |

---

## Document Lineage

This roadmap consolidates and does not replace:
- **GDD milestones:** `docs/design/01-vision/spatial-horror-gdd.md`
- **Sprint PDDs:** `docs/design/04-sprints/` (13 documents)
- **Epics:** `docs/design/03-systems/` (anomaly families, anchor loop, builder pipeline)
- **Vertical slice plan:** `docs/design/05-debug-and-testing/impossible-house-graybox-vertical-slice-plan.md`
- **Integration checklist:** `docs/design/05-debug-and-testing/networked-house-vertical-slice-integration-checklist.md`

Those documents remain the source of truth for detailed specs, contracts, and implementation guidance. This roadmap is the scheduling and progress-tracking layer on top.
