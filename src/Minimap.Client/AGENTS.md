# Agent notes — Minimap.Client

## Purpose

Godot-facing presentation: rendering, input capture, HUD, and related scripts. Also a class library for tests. Sources under this tree are compiled into the Godot host assembly as well.

## What may live here

- Godot nodes/scripts for world visuals, lobby scene root (`LobbyApp`), lobby panels, HUD (`PlayerHud*`)
- Lobby boot sequencing (`LobbySceneBoot`) and pure lobby state (`LobbyStateMachine`)
- Input capture helpers (`LocalInputAggregator`, device binding UI)
- Automation RPC host (`GodotRpcHost`) implementing contracts from **Minimap.Automation.Contracts** (see [testing.md](../../docs/technical/features/testing.md))
- `ExtensionPreflight` hook (Client invokes; App registers the real loader)

## What must not live here

- Authoritative game rules or world mutation owned by Simulation
- Heavy Simulation coupling—**minimize** Simulation references; **HUD types stay Simulation-free**
- Settings / extension DLL loading implementation (App owns that; call `ExtensionPreflight` only)
- Playbook implementations (those live under `tests/`)

Depends on Simulation sparingly; also Automation + Automation.Contracts for the RPC surface.
