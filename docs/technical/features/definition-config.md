# Definition config

JSON configuration for accessory, character, actor, and resource definitions. Loaded at the **Minimap.App** surface into the extension registry. Implements [accessories](accessories.md) / [characters](characters.md) / [actors](actors.md) / [resources](resources.md); related: [extensions.md](extensions.md), [depiction.md](depiction.md), [ui-icons.md](ui-icons.md), [tags.md](tags.md), [farming.md](farming.md).

## Requirements

- Shipped definitions are **extension content**, authored next to the extension project and copied beside the loadable DLL on build:
  - Source: `src/CompuQuest.Minimap/config/accessories/*.json`, `src/CompuQuest.Minimap/config/characters/*.json`, `src/CompuQuest.Minimap/config/actors/*.json`, `src/CompuQuest.Minimap/config/resources/*.json`
  - Runtime: `extensions/CompuQuest.Minimap/accessories/`, `characters/`, `actors/`, `resources/` (directory named after the assembly, next to `CompuQuest.Minimap.dll`)
- One definition per file. **Minimap.App** `DefinitionConfig` loads every `*.json` in each directory (sorted by filename for stable registration order). Missing directories are treated as empty.
- Load order inside `ExtensionLoader` (for each configured extension DLL, after that DLL’s `Register`, before `CreateGameContent`):
  1. Register resource definitions from `{dllDir}/{assemblyName}/resources/` (id → `TagRegistry.GetOrCreate`; optional `limit` resolved in a second pass; then validate limit rules)
  2. Register accessory definitions from `{dllDir}/{assemblyName}/accessories/` (effect `cost` / `modify_resource` ids must resolve to registered resource types)
  3. Register actor definitions from `{dllDir}/{assemblyName}/actors/` (accessory ids resolve against the registry)
  4. Register character definitions from `{dllDir}/{assemblyName}/characters/` (accessory ids resolve against the registry)
- Effect JSON `type` values resolve via **`IExtensionRegistry` accessory effect factories** registered by the extension (e.g. CompuQuest registers `"shoot"`, `"place_random_actor"`, `"modify_resource"`, `"grow"`, `"harvest"`). The host does **not** hardcode concrete effect classes.
- Simulation and Client do not perform file I/O for definitions. Extensions may still register definitions in C#; shipped CompuQuest content is JSON.
- Invalid JSON, missing required fields, unknown effect `type`, unresolved accessory / resource / actor ids, malformed `depiction` or `icon`, duplicate ids, or invalid resource limit graphs fail fast (same boot/preflight boundary as extensions).
- Optional **`depiction`** / **`icon`** on accessory, character, and actor JSON map to **`DepictionConfig`** / **`IconConfig`**. App does not validate that Godot resources exist at load time.

### Resource schema

```json
{
  "id": "health",
  "displayName": "Health",
  "limit": "max_health",
  "visible": true,
  "uiPriority": 1000,
  "icon": {
    "path": "res://assets/compuquest/game-icons/sbed/health-normal.svg"
  }
}
```

- `id` is required (also becomes a tag). `displayName`, `icon`, `limit`, `visible` (default **true**), and `uiPriority` (default **0**, may be negative) are optional.
- After all resource files in the directory are registered, resolve each `limit` string to a `TagId` and fail if the target type is missing or if a type used as a limit itself has a resource limit.

### Accessory schema

No accessory-level `resource` block. Grants and costs live on effects:

```json
{
  "id": "gun",
  "displayName": "Gun",
  "description": "Fire missiles at foes.",
  "pointCost": 1,
  "tags": ["player_selectable"],
  "activation": {
    "kind": "dedicated",
    "bind": "primary_fire"
  },
  "effects": [
    {
      "type": "modify_resource",
      "id": "ammo",
      "amount": 6
    },
    {
      "type": "shoot",
      "fireIntervalSeconds": 1.25,
      "missileSpeed": 200,
      "missileDamage": 25,
      "friendlyFire": true,
      "cost": { "id": "ammo", "amount": 1 }
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

- `id` and `effects` are required. `activation`, `depiction`, `icon`, `tags`, `pointCost` (default **0**), `displayName`, and `description` are optional.
- Optional `"cost": { "id", "amount" }` on activatable effects (default free; when present, `amount` must be ≥ **1**).
- `activation.kind`: `none` | `dedicated` | `modal`. Dedicated requires `bind` (e.g. `primary_fire`).
- `tags` is an array of strings resolved via the registry `TagRegistry` (create-if-not-exists).
- Effect `type` is a discriminator resolved by a registered factory. CompuQuest ships:
  - **`modify_resource`** — on acquire: add `amount` of resource `id` (amount may be negative).
  - **`shoot`** → `ShootEffect` (`IShootEffect`) — fire params + optional `cost`.
  - **`place_random_actor`** → `PlaceRandomActorEffect` (`ICellPlacementEffect`) — weighted `pool` of `{ "id", "weight" }` actor definition ids + optional `cost`.
  - **`grow`** — duration, mature depiction, harvest yield (passive; on vegetable actors).
  - **`harvest`** → `IInteractionEffect` — harvest mature food actors.

### Actor schema

```json
{
  "id": "carrot",
  "displayName": "Carrot",
  "accessories": ["grow_carrot"],
  "depiction": {
    "kind": "texture",
    "path": "res://assets/compuquest/game-icons/delapouite/seedling.svg"
  }
}
```

- `id` required. `accessories` optional (default empty). `displayName`, `depiction`, `icon` optional.

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
- CompuQuest also ships **`zombie`** with `"accessories": ["gun"]` for wave spawns. Generic has **empty** accessories — players choose abilities in the lobby.

### Later similar catalogs

For a new definition kind in an extension: add `config/<plural-kind>/` under the extension project, copy it to `extensions/{AssemblyName}/` on build, and add a loader step in `DefinitionConfig` / `ExtensionLoader` after its dependencies. New effect types: register an `AddAccessoryEffectFactory` in the extension’s `Register`.

## Non-goals (for now)

- Hot-reload of definition JSON during play
- Host-root `config/` accessory/character catalogs (those remain for core/scenario/extensions settings only)
