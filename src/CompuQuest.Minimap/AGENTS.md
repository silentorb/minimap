# Agent notes — CompuQuest.Minimap

## Purpose

**CompuQuest** surface-game **content extension**: default home for concrete accessory effects and shipped accessory/character JSON. Built as a **loadable DLL** under `extensions/`—a **build-only** dependency of the Godot host (**not** linked into the main assembly). Minimap is the engine layer; this assembly is the CompuQuest content on top.

## What may live here

- `IExtension` registration (integrator id **`compuquest`**, effect factories such as **`shoot`** / **`place_random_actor`** / **`modify_resource`** / **`grow`** / **`harvest`**, tags such as **`player_selectable`**)
- Sealed gameplay implementations (e.g. `ShootEffect`)
- Content JSON under `config/` (copied to `extensions/CompuQuest.Minimap/` on build): accessories, characters, actors, resources
- Integrator policy: default character **`generic`**, world spawner pool of **zombie spawners**, player-selectable accessories via tag filter, resource catalog into `GameContent`
- Effect factories: `shoot`, `place_random_actor`, `modify_resource`, `grow`, `harvest`

## What must not live here

- Host App/Client/Simulation module boundaries or Godot scene scripts
- Shared contracts that belong in **Minimap.Simulation.Types** or **Minimap.Extensive**
- Assumptions that this assembly is referenced by the Godot main assembly at runtime (it is loaded dynamically)

Depends on **Extensive** + **Simulation**. See [extensions.md](../../docs/technical/features/extensions.md), [definition-config.md](../../docs/technical/features/definition-config.md), [depiction.md](../../docs/technical/features/depiction.md), [ui-icons.md](../../docs/technical/features/ui-icons.md).

CompuQuest-owned Godot art lives under host **`assets/compuquest/`** so it can be imported as `res://`; definition JSON remains under `config/`:

- **Depiction (world):** Kenney 1-Bit Pack tilesheet + SpriteFrames under `assets/compuquest/kenney-1bit/`
- **UI icons (data records):** [game-icons.net](https://game-icons.net/) SVGs under `assets/compuquest/game-icons/`
