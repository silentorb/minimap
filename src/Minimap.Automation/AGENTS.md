# Agent notes — Minimap.Automation

## Purpose

Standalone in-process Godot automation helpers (frame wait, movement keys, scene lookup) used by playbooks inside the live Godot process.

## What may live here

- GodotSharp-only helpers shared by playbook libraries

## What must not live here

- References to **Minimap.Automation.Contracts**, playbook interfaces, or test projects
- Game Simulation rules or App composition
- gRPC server hosting (Client’s `GodotRpcHost` owns that)

Package dependency: **GodotSharp** only. See [testing.md](../../docs/technical/features/testing.md).
