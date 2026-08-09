# Agent notes — CompuQuest.Minimap

## Purpose

**CompuQuest** surface-game **content extension**: default home for concrete accessory effects and shipped accessory/actor JSON. Built as a **loadable DLL** under `extensions/`—a **build-only** dependency of the Godot host (**not** linked into the main assembly). Minimap is the engine layer; this assembly is the CompuQuest content on top.

## What may live here

- `IExtension` registration (integrator id **`compuquest`**, effect factories such as **`shoot`** / **`swing`** / **`move`** / **`place_random_actor`** / **`modify_resource`** / **`grow`** / **`spawn`** / **`spawn_nearby_ally`** / **`harvest`** / **`use_computer`** / **`heal`**, tags such as **`player_selectable`**, **`human`**, **`animal`**)
- Sealed gameplay implementations (e.g. `ShootEffect`, `SwingEffect`, `MoveEffect`, `SpawnEffect`, `HealEffect`)
- Content JSON under `config/` (**mirrored** to `extensions/CompuQuest.Minimap/` on build — wipe then copy): accessories, actors, resources, domains
- Integrator policy: default actor **`generic`**, world spawner pool of **zombie spawners** (marker path for tests / future wave events), intrinsic **`zombie_spawner`** actor for normal play (with wave/level countdown), player-selectable accessories via tag filter, resource catalog into `GameContent`
- Effect factories: `shoot`, `swing`, `move`, `place_random_actor`, `modify_resource`, `grow`, `spawn`, `spawn_nearby_ally`, `harvest`, `pickup_resource`, `death_drop`, `use_computer`, `heal`, `drain_resource`, `drain_resource_by_distance`, `modify_resource_by_ratio_bands`, `modify_resource_on_use`

## What must not live here

- Host App/Client/Simulation module boundaries or Godot scene scripts
- Shared contracts that belong in **Minimap.Simulation.Types** or **Minimap.Extensive**
- Assumptions that this assembly is referenced by the Godot main assembly at runtime (it is loaded dynamically)

Depends on **Extensive** + **Simulation**. See [extensions.md](../../docs/technical/features/platform/extensions.md), [definition-config.md](../../docs/technical/features/platform/definition-config.md), [depiction.md](../../docs/technical/features/gameplay/depiction.md), [ui-icons.md](../../docs/technical/features/ui/ui-icons.md).

CompuQuest-owned Godot art lives under host **`assets/compuquest/`** so it can be imported as `res://`; definition JSON remains under `config/`:

- **Depiction (world):** Kenney 1-Bit Pack tilesheet + SpriteFrames under `assets/compuquest/kenney-1bit/`
- **UI icons (data records):** [game-icons.net](https://game-icons.net/) SVGs under `assets/compuquest/game-icons/`
