# Multiplayer Architecture for Co-Op Horror Games

Use a **server-authoritative world model** with client-owned intent, and treat topology mutation as replicated state plus a small set of atomic mutation events; that gives you the best tradeoff for a 2–4 player co-op horror game in NGO. Unity’s own guidance is to use `NetworkVariable`s for persistent state that late joiners must inherit, and RPCs for short-lived events, which maps cleanly onto a mutable house graph, observation coverage, and stalker actions. [docs.unity3d](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.11/manual/learn/rpcnetvarexamples.html)

## Authority split

Your proposed split is basically right: the server should own the canonical house graph, room/door/anchor states, mutation scheduler, and stalker AI, while each client owns only local input streams such as movement input, camera/look direction, “I am observing these rooms,” and interaction requests. Persistent topology and AI state are exactly the kind of data NGO says should be replicated as state rather than sent only as one-off events, because late joiners need the latest world, not the history of how it got there. [docs.unity3d](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.11/manual/learn/rpcnetvarexamples.html)

A practical pattern is:
- Client authoritative for raw **input**, never for outcomes.
- Server authoritative for room connectivity, mutation legality, interaction resolution, line-of-sight validation, and stalker behavior.
- Hybrid for movement: client-side locomotion feel with server validation/correction if you need responsiveness, but do not let clients decide room membership or topology consequences. That keeps cheating and accidental divergence out of the system while preserving decent feel on LAN and acceptable feel online. [docs.unity3d](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.11/manual/learn/rpcnetvarexamples.html)

## Observation mechanic

Compute shared observation coverage on the **server** from per-player observation contributions, not as a fully client-trusted union. A client can cheaply precompute what it thinks it sees for UX, but the server should validate or recompute room visibility from player pose, facing, room occlusion rules, and observation anchors before deciding whether a room or edge is mutation-eligible. [docs.unity3d](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.11/manual/learn/rpcnetvarexamples.html)

The best model is:
- Each client sends observation intent/state at a fixed cadence or only on change: current room, camera forward, focal target, maybe a compact visible-room bitset.
- Server derives authoritative observed set per player.
- Server computes `ObservedUnion = OR(all player observed bitsets)`.
- Mutation planner only considers nodes/edges outside `ObservedUnion`.
- Server broadcasts either the new graph state or a compact “mutation committed” payload plus updated eligibility metadata for nearby rooms. Because this eligibility affects gameplay over more than an instant, it behaves more like replicated state than a fire-and-forget event. [docs.unity3d](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.11/manual/learn/rpcnetvarexamples.html)

For your specific mechanic, I would separate:
- **Coverage state:** which rooms are currently protected by observation.
- **Mutation event:** the atomic graph rewrite that just happened.
- **Presentation hints:** local shaders, UI pulses, audio creaks, “unstable” warnings clients can predict cosmetically.

That avoids the worst failure mode, where clients disagree about whether a room was safe to mutate at the instant the server committed the change.

## Consistency patterns

Do **not** use lock-step for this genre; it is too latency-sensitive and unnecessary for 2–4 player exploration horror. Also avoid full rollback for the world graph unless you are building a much more simulation-heavy game, because topology changes are rare, high-impact, and easier to serialize as authoritative commits than to rewind continuously. [docs.unity3d](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.11/manual/learn/rpcnetvarexamples.html)

The safer pattern is “authoritative commit with guarded traversal”:
1. Server decides a mutation at time \(t\).
2. Server marks affected doors/thresholds/anchors as `TransitionLocked`.
3. Server sends one atomic mutation message or updates a versioned graph state.
4. Clients receiving it update geometry/navigation and clear local speculative visuals.
5. Any player whose predicted movement crossed an invalidated threshold gets server-corrected to the nearest safe anchor in their pre-mutation room or the destination room according to a deterministic rule.

Unity notes that RPCs are useful when multiple values must arrive together, while `NetworkVariable`s are eventually consistent and may not be observed simultaneously on clients. For a topology mutation, that means the mutation itself should often be packaged as a single event payload or versioned transaction, even if the resulting steady-state graph lives in replicated state. [docs.unity3d](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.11/manual/learn/rpcnetvarexamples.html)

For “I walked into a room that just mutated,” use these safeguards:
- **Doorway reservation:** a room edge cannot mutate while any player capsule overlaps its threshold volume.
- **Traversal tokens:** when a client begins crossing a doorway, the server grants a short-lived crossing token and excludes that edge from mutation until resolved.
- **Safe snap rules:** if correction is needed, snap to a pre-authored anchor with a horror-friendly effect, such as blackout, blink, or camera hitch, instead of a naked teleport.
- **Version checks:** every room trigger and interactable should know the current graph version so stale client interactions can be rejected cleanly.

This gives you responsiveness without pretending the client can author world truth.

## Separation design

If Player A and Player B end up in areas that become disconnected, that is usually a **feature**, not a bug, in co-op horror. The stronger question is when separation is allowed versus forbidden. Shipped co-op horror games generally preserve team tension by allowing separation but making it risky through information asymmetry, vulnerability, and extraction/regroup incentives rather than hard tethering players together. [steamcommunity](https://steamcommunity.com/app/1966720/discussions/0/4034727918503255629/)

A good rule stack is:
- Prevent mutations that would create an impossible state, such as trapping a player in a room with no valid exits, no line-of-retreat, or no reachable rescue path.
- Allow mutations that create soft separation, such as forcing a longer route, closing a shortcut, or isolating sightlines.
- Escalate warnings before severe separation, for example audio cues, lights flicker, floorplan instability, or radio static.
- Cap isolation duration by design, e.g. no player can remain fully unreachable for more than \(N\) seconds unless this is an intentional set-piece.

For implementation, classify candidate mutations into:
- **Safe:** no players in affected nodes/edges, no pathing hazards.
- **Risky but valid:** separates the team but preserves at least one reconnect path.
- **Illegal:** strands, overlaps geometry, invalidates active traversal, or breaks scripted encounters.

That lets horror emerge from world instability without creating unfair soft-locks.

## Pacing patterns

The common pacing pattern in successful co-op horror is not “keep everyone together,” but “let people split because the task economy encourages it, then punish overextension with uncertainty.” Lethal Company is widely understood to use host-based peer-to-peer networking through Steam/Unity-era tooling rather than dedicated servers, which fits its small-party, chaos-first design, and its structure pushes players apart for loot efficiency while using communication, danger, and extraction pressure to keep the experience coherent. [reddit](https://www.reddit.com/r/gamedev/comments/1e8mb4m/how_was_the_networking_side_of_the_game_lethal/)

Across Phasmophobia, Lethal Company, Devour, and Content Warning, the proven design patterns are:
- Distinct **roles** emerge naturally, not through hard classes: scout, objective runner, support, camera/radio watcher.
- Players are rewarded for splitting temporarily, but information becomes noisier and survival odds worse when isolated.
- Re-group points exist: truck/van, extraction ship, ritual site, objective hand-in, or safe rooms.
- Global threat beats periodically re-synchronize the group experience, such as hunts, quota deadlines, possession phases, or monster spikes. [steamcommunity](https://steamcommunity.com/app/1966720/discussions/0/4034727918503255629/)

For your game, that suggests:
- Let players explore independently for efficiency.
- Use topology mutation and the stalker to make separation progressively more dangerous, not instantly fatal.
- Add periodic “coherence beats” that pull attention back together, such as a whole-house groan, blackout, stalker reveal, or a mutation pulse everyone hears.
- Ensure at least one shared information channel matters, such as map fragments, breaker state, observation coverage, or ritual progress.

That preserves agency while keeping the horror legible to the full party.

## Bandwidth choices

For 50 nodes, the graph itself is small if represented well. A node-state bitset for 50 rooms is only 50 bits, and even if you track several booleans per node or edge, you are still usually talking about tens to low hundreds of bytes per full snapshot before transport overhead; the expensive part is not the raw topology, but frequency, per-object replication overhead, and any associated transforms, nav updates, or visual state churn. NGO also notes that `NetworkVariable`s only sync changed latest values, while RPCs send every call, so high-frequency event spam is where you get into trouble. [docs.unity3d](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.11/manual/learn/rpcnetvarexamples.html)

Use this rule:
- **NetworkVariables / replicated state** for current room states, graph version, door open/closed/locked, stalker phase, player current room, and any state late joiners need. [docs.unity3d](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.11/manual/learn/rpcnetvarexamples.html)
- **RPCs** for mutation commit events, jump-scare triggers, one-shot audio cues, temporary hallucinations, and batched atomic updates that must be observed together. [docs.unity3d](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.11/manual/learn/rpcnetvarexamples.html)

A solid indie-scale pattern is:
- One replicated `GraphVersion`.
- One compact replicated snapshot structure for room/edge persistent flags.
- One RPC `ApplyMutation(mutationId, fromVersion, toVersion, affectedNodes, affectedEdges, safeAnchors...)`.
- Interest management by proximity/floor/zone for cosmetic details, while core topology stays global because every player may care about connectivity.

If mutations are “frequent,” define that carefully. A few per second is already a lot for topology in a horror game. If you mutate every frame, the mechanic is wrong for networking; if you mutate every few seconds or on triggered beats, bandwidth is trivial and readability improves.

## Testing stack

Unity recommends several local multiplayer workflows: player builds for platform validation, Multiplayer Play Mode for fast local simulation up to four players on one device, and third-party tools such as ParrelSync for running multiple editor instances during iteration. That matches a sensible horror-netcode workflow almost exactly. [docs.unity3d](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.11/manual/learn/rpcvnetvar.html)

A good testing pyramid is:
- **Minute-to-minute iteration:** Multiplayer Play Mode for fast repros of observation coverage and mutation timing. Unity says it can simulate up to four players locally in the Editor. [docs.unity3d](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.11/manual/learn/rpcvnetvar.html)
- **Two-editor debugging:** ParrelSync for side-by-side host/client debugging and inspecting divergent state. Normcore’s local testing guide explicitly recommends ParrelSync for this style of iteration because each instance keeps full debugging access. [docs.normcore](https://docs.normcore.io/guides/testing-multiplayer-locally)
- **Nightly validation:** dedicated server or host build plus 2–4 clients, with artificial latency/jitter.
- **Automated harness:** headless server simulation with scripted bots that roam rooms, look at anchors, and trigger interactions while asserting graph invariants.

The automated tests that matter most for your game are:
- Observation union correctness: no observed room mutates.
- Traversal safety: no mutation invalidates an in-progress doorway crossing.
- Reconnection path guarantees: legal mutations never strand a player unless the design explicitly allows it.
- Stalker coherence: all clients agree on stalker room, target phase, and kill windows.
- Version monotonicity: clients never process graph version \(n+1\) before resolving \(n\).

## Late join and reconnect

Late join should use a **full snapshot**, not replaying an entire mutation log, unless you later need demo/replay features. Unity’s NGO documentation explicitly frames `NetworkVariable`s as the mechanism that lets late joiners catch up to the current world state, which is why the canonical mutable graph, door states, and other long-lived values should live in replicated state. [docs.unity3d](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.11/manual/learn/rpcnetvarexamples.html)

A robust late-join flow:
1. Client connects.
2. Server sends current graph version, room/edge persistent state, active stalker state, objective state, and player-safe spawn candidates.
3. Client loads/activates the right room topology and nav blockers.
4. Server spawns the player at a safe node, usually a hub, a teammate-adjacent safe anchor, or a designated recovery room.
5. After snapshot apply, the client begins receiving live mutation RPCs from the current version onward.

For reconnects, if the server can identify the returning player:
- If their last known room still exists and is safe, restore nearby.
- If not, respawn at nearest safe anchor in the same connected component as the team leader or the designated regroup node.
- If the graph version gap is small, deltas are fine; if not, send a fresh snapshot. At your scale, a fresh snapshot is usually simpler and safer than clever delta chains. [docs.unity3d](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.11/manual/learn/rpcnetvarexamples.html)

## Recommended architecture

For your exact project, I would build it like this:
- **Server authoritative:** house graph, mutation scheduler, room eligibility, stalker AI, anchor validity, interaction resolution.
- **Client sends intent:** movement input, look vector/camera, interact requests, “focus/observe” state.
- **Observation pipeline:** client proposes, server validates, server computes union, server decides mutation legality.
- **State sync:** replicated persistent graph snapshot plus version number; RPC for atomic mutation commits and one-shot horror events. [docs.unity3d](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.11/manual/learn/rpcnetvarexamples.html)
- **Traversal safety:** doorway reservations, threshold locks during mutation, deterministic safe-anchor correction.
- **Separation policy:** permit soft separation, forbid hard soft-locks.
- **Testing:** Multiplayer Play Mode first, ParrelSync second, nightly latency-injected build tests third. [docs.normcore](https://docs.normcore.io/guides/testing-multiplayer-locally)
- **Late join:** always snapshot current world, then stream live deltas/events. [docs.unity3d](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.11/manual/learn/rpcnetvarexamples.html)

If you want the cleanest mental model, think of the house as a **server-owned transactional graph**: clients can request, observe, and traverse it, but only the server commits graph rewrites. That model is simple enough for indie scope, resilient enough for internet play later, and matches NGO’s own distinction between persistent replicated state and transient events. [docs.unity3d](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.11/manual/learn/rpcnetvarexamples.html)

Would you like a concrete Unity-side design next, with suggested data structures, message schemas, and a room-graph sync model for NGO 2.11?