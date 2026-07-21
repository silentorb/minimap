# Agent notes — CompuQuest.Minimap

## Purpose

Sample/content **extension** library: default home for concrete accessory effects and shipped accessory/character JSON. Built as a **loadable DLL** under `extensions/`—a **build-only** dependency of the Godot host (**not** linked into the main assembly).

## What may live here

- `IExtension` registration (integrator id **`compuquest`**, effect factories such as **`shoot`**)
- Sealed gameplay implementations (e.g. `ShootEffect`)
- Content JSON under `config/` (copied to `extensions/CompuQuest.Minimap/` on build)

## What must not live here

- Host App/Client/Simulation module boundaries or Godot scene scripts
- Shared contracts that belong in **Minimap.Simulation.Types** or **Minimap.Extensive**
- Assumptions that this assembly is referenced by the Godot main assembly at runtime (it is loaded dynamically)

Depends on **Extensive** + **Simulation**. See [extensions.md](../../docs/technical/features/extensions.md), [definition-config.md](../../docs/technical/features/definition-config.md), [depiction.md](../../docs/technical/features/depiction.md).

CompuQuest-owned Godot art (Kenney 1-Bit Pack tilesheet + SpriteFrames) lives under host **`assets/compuquest/`** so it can be imported as `res://`; definition JSON remains under `config/`.
