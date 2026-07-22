# Definition config

JSON configuration for accessory and character definitions (and the pattern for later similar game-data catalogs). Loaded at the **Minimap.App** surface into the extension registry. Implements [accessories](accessories.md) / [characters](characters.md); related: [extensions.md](extensions.md), [depiction.md](depiction.md), [ui-icons.md](ui-icons.md).

## Requirements

- Shipped definitions are **extension content**, authored next to the extension project and copied beside the loadable DLL on build:
  - Source: `src/CompuQuest.Minimap/config/accessories/*.json`, `src/CompuQuest.Minimap/config/characters/*.json`
  - Runtime: `extensions/CompuQuest.Minimap/accessories/`, `extensions/CompuQuest.Minimap/characters/` (directory named after the assembly, next to `CompuQuest.Minimap.dll`)
- One definition per file. **Minimap.App** `DefinitionConfig` loads every `*.json` in each directory (sorted by filename for stable registration order). Missing directories are treated as empty.
- Load order inside `ExtensionLoader` (for each configured extension DLL, after that DLL’s `Register`, before `CreateGameContent`):
  1. Register accessory definitions from `{dllDir}/{assemblyName}/accessories/`
  2. Register character definitions from `{dllDir}/{assemblyName}/characters/` (accessory ids resolve against the registry, including any C#-registered defs and earlier extensions)
- Effect JSON `type` values resolve via **`IExtensionRegistry` accessory effect factories** registered by the extension (e.g. CompuQuest registers `"shoot"`). The host does **not** hardcode concrete effect classes.
- Simulation and Client do not perform file I/O for definitions. Extensions may still register definitions in C#; shipped CompuQuest content is JSON.
- Invalid JSON, missing required fields, unknown effect `type`, unresolved accessory ids, malformed `depiction` or `icon`, or duplicate ids fail fast (same boot/preflight boundary as extensions).
- Optional **`depiction`** object on accessory and character JSON maps to **`DepictionConfig`** (see [depiction.md](depiction.md)). App does not validate that Godot resources exist at load time.
- Optional **`icon`** object on accessory and character JSON maps to **`IconConfig`** (see [ui-icons.md](ui-icons.md)). App does not validate that Godot resources exist at load time.

### Accessory schema

```json
{
  "id": "gun",
  "displayName": "Gun",
  "description": "Fire missiles at foes.",
  "pointCost": 1,
  "tags": ["player_selectable"],
  "effects": [
    {
      "type": "shoot",
      "fireIntervalSeconds": 1.25,
      "missileSpeed": 200,
      "missileDamage": 25,
      "friendlyFire": true
    }
  ],
  "depiction": {
    "kind": "sprite_frames",
    "path": "res://assets/compuquest/kenney-1bit/depict/gun.tres",
    "animation": "default"
  },
  "icon": {
    "path": "res://assets/compuquest/game-icons/john-colburn/pistol-gun.svg"
  }
}
```

- `id` and `effects` are required. `depiction`, `icon`, `tags`, `pointCost` (default **0**), `displayName`, and `description` are optional.
- `tags` is an array of strings resolved via the registry `TagRegistry` (create-if-not-exists).
- Effect `type` is a discriminator resolved by a registered factory. CompuQuest ships:
  - **`shoot`** → CompuQuest `ShootEffect` (`IShootEffect`) — requires `fireIntervalSeconds`, `missileSpeed`, `missileDamage`; `friendlyFire` optional (default **true**).

### Character schema

```json
{
  "id": "generic",
  "accessories": [],
  "depiction": {
    "kind": "sprite_frames",
    "path": "res://assets/compuquest/kenney-1bit/depict/generic.tres",
    "animation": "default"
  },
  "icon": {
    "path": "res://assets/compuquest/game-icons/delapouite/person.svg"
  }
}
```

- `id` and `accessories` are required. Each accessories entry is an already-registered accessory definition id (order preserved). `depiction` and `icon` are optional.
- CompuQuest also ships **`zombie`** with `"accessories": ["gun"]` for wave spawns.

### Later similar catalogs

For a new definition kind in an extension: add `config/<plural-kind>/` under the extension project, copy it to `extensions/{AssemblyName}/` on build, and add a loader step in `DefinitionConfig` / `ExtensionLoader` after its dependencies. New effect types: register an `AddAccessoryEffectFactory` in the extension’s `Register`.

## Non-goals (for now)

- Hot-reload of definition JSON during play
- Host-root `config/` accessory/character catalogs (those remain for core/scenario/extensions settings only)
