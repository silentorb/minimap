# Agent notes — Minimap.Simulation

## Purpose

Authoritative game logic and state for the playthrough.

## What may live here

- World/sim APIs (`GameWorld`, systems, controllers that are simulation-owned)
- Rules, ticks, spawn, combat helpers that operate on sim state
- Types from **Minimap.Simulation.Types** consumed as contracts

## What must not live here

- **Godot** references or engine types
- User input or output APIs (Client captures input; App feeds it)
- Settings / extension **file I/O** (App loads JSON and extension DLLs)
- Sealed content effects that belong in an extension (default: CompuQuest)

Depends on **Minimap.Simulation.Types** only for shared contracts.
