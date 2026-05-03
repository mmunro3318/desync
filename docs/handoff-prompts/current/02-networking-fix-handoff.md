# Handoff: Networking/Multiplayer Fix Complete

## Branch: `debug/networking-multiplayer-fix`

## What Was Done

Fixed 4 code bugs + identified 1 environment issue that together broke multiplayer:

### Code Fixes (all in `unity-DESYNC/Assets/_Project/Scripts/`)

1. **`Core/GameBootstrap.cs`** -- Host was binding to `127.0.0.1` (localhost only). Fixed to call `transport.SetConnectionData("0.0.0.0", port, "0.0.0.0")` before `StartHost()`, binding to all network interfaces. Added `OnConnectionFailed` event, `OnClientDisconnectCallback`/`OnTransportFailure` handlers for connection lifecycle. Added diagnostic debug logs for transport state.

2. **`UI/LobbyUI.cs`** -- Subscribed to new `OnConnectionFailed` event so the UI resets buttons and shows error status when connection fails/drops (previously stuck on "Connecting..." forever).

3. **`Audio/FootstepAudio.cs`** -- `PlayFootstepRemoteClientRpc()` was called directly by the owning client, but only the server can send ClientRpcs. Changed to proper `ServerRpc -> ClientRpc` relay pattern (`RequestFootstepServerRpc()` -> `PlayFootstepRemoteClientRpc()`).

4. **`Player/PlayerMotor.cs`** -- Added `OnNetworkSpawn()` that disables `CharacterController` on non-owner instances, preventing it from fighting `NetworkTransform` position updates on remote player representations.

### Environment Issue (not a code fix)

5. **Windows Firewall doesn't reliably prompt for UDP-only apps.** The built game .exe on the joining machine must be manually added to Windows Firewall allowed apps. Without this, the join hangs silently. This needs to be documented in player-facing setup/README.

## Verification Status

- [x] MPPM (same machine): Host + Join works, both players visible, movement replicates
- [x] Cross-machine (LAN WiFi): Host on desktop, join from laptop build -- works after firewall .exe allowance
- [ ] `NetworkBootstrapConsistencyTests` -- should still pass (ConnectionApproval remains false)
- [ ] Flashlight replication between players
- [ ] Footstep audio heard on remote player

## Docs to Update

- **CLAUDE.md**: Smoke test step 6 can now be checked. The multiplayer caveat ("Cross-machine multiplayer is not solved") can be softened to note LAN works with firewall setup.
- **README.md** (if/when created): Add "Multiplayer Setup" section noting the firewall requirement for builds.
- **`docs/design/05-debug-and-testing/`**: Update graybox test plan to note the firewall .exe requirement.

## Key Architectural Decisions

- Host binds to `0.0.0.0` (all interfaces) rather than a specific adapter IP. Simpler; works for any LAN topology.
- FootstepAudio uses ServerRpc->ClientRpc relay (standard NGO pattern for owner-initiated effects that need to reach all clients).
- CharacterController disabled on non-owner rather than using a custom NetworkTransform override. Simpler; NetworkTransform handles interpolation directly on the Transform.
- No Relay/Lobby/NAT traversal added. LAN-only scope preserved per jam timeline.
