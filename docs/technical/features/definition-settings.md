# Definition settings

JSON configuration for accessory and character definitions (and the pattern for later similar game-data catalogs). Loaded at the **Minimap.App** surface into the extension registry. Implements [accessories](accessories.md) / [characters](characters.md); related: [extensions.md](extensions.md).

## Requirements

- Shipped definitions live under the config tree (Godot `res://config/...`):
  - **Accessories:** `config/accessories/*.json` (e.g. `gun.json`)
  - **Characters:** `config/characters/*.json` (e.g. `generic.json`)
- One definition per file. **Minimap.App** `DefinitionSettings` loads every `*.json` in each directory (sorted by filename for stable registration order). Missing directories are treated as empty.
- Load order inside `ExtensionLoader` (after extension DLLs `Register`, before `CreateGameContent`):
  1. Register accessory definitions from `config/accessories/`
  2. Register character definitions from `config/characters/` (accessory ids resolve against the registry, including any C#-registered defs)
- Simulation and Client do not perform file I/O for definitions. Extensions may still register definitions in C#; shipped content is JSON.
- Invalid JSON, missing required fields, unknown effect `type`, unresolved accessory ids, or duplicate ids fail fast (same boot/preflight boundary as extensions).

### Accessory schema

```json
{
  "id": "gun",
  "effects": [
    {
      "type": "shoot",
      "fireIntervalSeconds": 1.25,
      "missileSpeed": 200,
      "missileDamage": 25,
      "friendlyFire": true
    }
  ]
}
```

- `id` and `effects` are required.
- Effect `type` is a discriminator. Supported today:
  - **`shoot`** → `ShootEffect` — requires `fireIntervalSeconds`, `missileSpeed`, `missileDamage`; `friendlyFire` optional (default **true**).

### Character schema

```json
{
  "id": "generic",
  "accessories": ["gun"]
}
```

- `id` and `accessories` are required. Each accessories entry is an already-registered accessory definition id (order preserved).

### Later similar catalogs

For a new definition kind: add `config/<plural-kind>/`, a loader that builds types in `Minimap.Simulation.Types` and registers on `IExtensionRegistry`, then wire that step into `ExtensionLoader` after its dependencies.

## Non-goals (for now)

- Hot-reload of definition JSON during play
- Per-extension content directories (host `config/` is the authoring surface)
- Effect types beyond `shoot`
