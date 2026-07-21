# Agent notes — Minimap.Simulation

## Purpose

Authoritative game logic and state for the playthrough. Treat as a **headless server/engine**: neighbors feed constructed data in and read state out.

## What may live here

- World/sim APIs (`GameWorld`, `GameSession`, systems, controllers that are simulation-owned)
- Rules, ticks, spawn, combat helpers that operate on sim state
- Navigation **interfaces** and headless steering (`IMoveSteering`, `DirectMoveSteering`) — Godot implementations live in **Minimap.Simulation.Navigation**
- Types from **Minimap.Simulation.Types** consumed as contracts

## What must not live here

- **Any I/O** — no filesystem, JSON, DLL load, Godot `OS`/`ProjectSettings`, or device polling
- **Godot** references or engine types (including NavigationServer / NavigationAgent)
- User input or output APIs (Client captures input and feeds controller intents)
- Settings / extension **file I/O** (App loads JSON and extension DLLs)
- Sealed content effects that belong in an extension (default: CompuQuest)
- Client types (`PlayerController`, HUD models)

Depends on **Minimap.Simulation.Types** only for shared contracts.
