# Agent notes — Minimap.Client

## Purpose

Godot-facing presentation and local play: rendering, device input, HUD, world/lobby scene roots. Also a class library for tests. Sources under this tree are compiled into the Godot host assembly as well.

## What may live here

- Godot nodes/scripts for world visuals, world scene root (`WorldApp`), lobby scene root (`LobbyApp`), lobby panels, HUD (`PlayerHud*`)
- Lobby / world boot sequencing (`LobbySceneBoot`, `WorldSceneBoot`) and pure lobby state (`LobbyStateMachine`)
- Local session adapter (`ClientSession`): attach `PlayerController`s, feed input, map HUD models over Simulation `GameSession`
- Input capture helpers (`LocalInputAggregator`, device binding UI)
- Godot navigation host upgrade for AI (via **Minimap.Simulation.Navigation**)
- Automation RPC host (`GodotRpcHost`) implementing contracts from **Minimap.Automation.Contracts** (see [testing.md](../../docs/technical/features/testing.md))
- `ExtensionPreflight` / `WorldHostHooks` (Client invokes; App registers the real loaders)

## What must not live here

- Authoritative game rules or world mutation owned by Simulation
- Settings / extension **file** loading implementation (App owns that; call hooks only)
- Playbook implementations (those live under `tests/`)

## I/O

Light device/engine I/O is expected (keyboard/joypad, display, scene tree). Do **not** open shipped JSON/DLL config paths—use App-registered hooks.

Depends on Simulation (+ Simulation.Navigation for crowd steering); also Automation + Automation.Contracts for the RPC surface. HUD types stay Simulation-free.
