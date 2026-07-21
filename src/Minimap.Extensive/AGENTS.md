# Agent notes — Minimap.Extensive

## Purpose

Extension contracts, in-memory registry, and the default integrator—the extensibility capability assembly (not a bag of content instances).

## What may live here

- `IExtension`, `IExtensionRegistry`, `IIntegrator`, and related registration APIs
- In-memory catalogs (integrators, effect factories, definitions)
- Built-in **`DefaultIntegrator`**

## What must not live here

- **Godot** references
- **File I/O** (App loads config and DLLs; see [extensions.md](../../docs/technical/features/extensions.md))
- Shared simulation contracts / definition bags (those live in **Minimap.Simulation.Types**)
- Concrete accessory effects or shipped game content (extensions such as CompuQuest)

Depends on **Minimap.Simulation.Types**.
