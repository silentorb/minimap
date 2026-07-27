# Definition config

JSON configuration for accessory, actor, resource, and domain definitions. Loaded at the **Minimap.App** surface into the extension registry. Implements [accessories](../gameplay/accessories.md) / [actors](../gameplay/actors.md) / [resources](../gameplay/resources.md) / [domains](../gameplay/domains.md); related: [extensions.md](extensions.md), [depiction.md](../gameplay/depiction.md), [ui-icons.md](../ui/ui-icons.md), [tags.md](../gameplay/tags.md), [farming.md](../gameplay/farming.md), [hunger.md](../gameplay/hunger.md).

## Requirements

- Shipped definitions are **extension content**, authored next to the extension project and **mirrored** beside the loadable DLL on build (destination content tree wiped, then all `*.json` copied — so gitignored pull survivors cannot keep retired files):
  - Source: `src/CompuQuest.Minimap/config/accessories/*.json`, `src/CompuQuest.Minimap/config/actors/*.json`, `src/CompuQuest.Minimap/config/resources/*.json`, `src/CompuQuest.Minimap/config/domains/*.json`
  - Runtime: `extensions/CompuQuest.Minimap/accessories/`, `actors/`, `resources/`, `domains/` (directory named after the assembly, next to `CompuQuest.Minimap.dll`; see `extensions/README.md`)
- One definition per file. **Minimap.App** `DefinitionConfig` loads every `*.json` in each directory (sorted by filename for stable registration order). Missing directories are treated as empty.
- Load order inside `ExtensionLoader` (for each configured extension DLL, after that DLL’s `Register`, before `CreateGameContent`):
  1. Register resource definitions from `{dllDir}/{assemblyName}/resources/` (id → `TagRegistry.GetOrCreate`; optional `limit` resolved in a second pass; then validate limit rules)
  2. Register domain definitions from `{dllDir}/{assemblyName}/domains/` (id → `TagRegistry.GetOrCreate`; required `#RRGGBB` color)
  3. Register accessory definitions from `{dllDir}/{assemblyName}/accessories/` (effect `cost` / `modify_resource` ids must resolve to registered resource types)
  4. Register actor definitions from `{dllDir}/{assemblyName}/actors/` (accessory ids resolve against the registry)
- Effect JSON `type` values resolve via **`IExtensionRegistry` accessory effect factories** registered by the extension (e.g. CompuQuest registers `"shoot"`, `"place_random_actor"`, `"modify_resource"`, `"grow"`, `"spawn"`, `"harvest"`, `"use_computer"`, `"drain_resource"`, `"modify_resource_by_ratio_bands"`, `"modify_resource_on_use"`). The host does **not** hardcode concrete effect classes.
- Simulation and Client do not perform file I/O for definitions. Extensions may still register definitions in C#; shipped CompuQuest content is JSON.
- Invalid JSON, missing required fields, unknown effect `type`, unresolved accessory / resource / actor ids, malformed `depiction` / `icon` / domain `color`, duplicate ids, or invalid resource limit graphs fail fast (same boot/preflight boundary as extensions).
- Optional **`depiction`** / **`icon`** on accessory and actor JSON map to **`DepictionConfig`** / **`IconConfig`**. App does not validate that Godot resources exist at load time.

### Domain schema

```json
{
  "id": "gardening",
  "displayName": "Gardening",
  "color": "#3A8F4B"
}
```

- `id` and `color` (`#RRGGBB`) are required. `displayName` is optional. `id` becomes a tag (same as resource ids). See [domains.md](../gameplay/domains.md).

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
      "projectileActorId": "missile",
      "fireIntervalSeconds": 1.25,
      "missileSpeed": 400,
      "missileDamage": 25,
      "missileRange": 800,
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

- `id` and `effects` are required. `activation`, `depiction`, `icon`, `tags`, `pointCost` (default **0**), `displayName`, `description`, and optional **`enabledWhen`** (`{ "id", "atLeast" }` resource gate) are optional.
- Optional `"cost": { "id", "amount" }` on activatable effects (default free; when present, `amount` must be ≥ **1**).
- `activation.kind`: `none` | `dedicated` | `modal`. Dedicated requires `bind` (e.g. `primary_fire`, `secondary_fire`).

- `tags` is an array of strings resolved via the registry `TagRegistry` (create-if-not-exists).
- Effect `type` is a discriminator resolved by a registered factory. CompuQuest ships:
  - **`modify_resource`** — on acquire: add `amount` of resource `id` (amount may be negative).
  - **`shoot`** → `ShootEffect` (`IShootEffect` + `IWorldPassiveEffect` for cell-actor auto-fire) — required `projectileActorId`, `fireIntervalSeconds`, `missileSpeed`, `missileDamage`, `missileRange` (max distance traveled); optional `friendlyFire` (default true), optional `missileSizeScale` (default **1**), optional `cost`.
  - **`swing`** → `SwingEffect` (`ISwingEffect`) — melee Swing params (damage, interval, radius, arc, visual duration, friendly fire) + optional `cost`.
  - **`place_random_actor`** → `PlaceRandomActorEffect` (`ICellPlacementEffect`) — weighted `pool` of `{ "id", "weight" }` actor definition ids + optional `cost`.

  - **`grow`** — duration, mature depiction, optional `harvestYield`, optional `emergeActorId` + `emergeAfterMatureSeconds` (passive; on vegetable actors).
  - **`harvest`** → `IInteractionEffect` — harvest mature food actors (or emerge ambush crops) + optional `cost`.
  - **`pickup_resource`** → `IDefaultInteractionEffect` — object-side pickup (resource `id` + `amount`) + optional `cost`.
  - **`death_drop`** → `IDeathDropEffect` — on character death, place actor definition `id` if the corpse hex is free.

  - **`drain_resource`** → `IPassiveEffect` — drain resource `id` at `amountPerSecond` (default **1**).
  - **`drain_resource_by_distance`** → `IPassiveEffect` — drain resource `id` by traveled distance (`unitsPerAmount` world units per **1** resource).
  - **`modify_resource_by_ratio_bands`** → `IPassiveEffect` — every `periodSeconds`, read source/max ratio and apply a banded delta to a target resource (vitality).
  - **`modify_resource_on_use`** → `IInstantUseEffect` — on activate: add `amount` of resource `id` + optional `cost`.
  - **`spawn_nearby_ally`** → `IWorldActorPassiveEffect` — one-shot spawn of actor `actorId` on the owner’s faction near the owner (optional `aggression`; see [animal-companions.md](../gameplay/animal-companions.md)).
  - **`move`** → `IMoveEffect` — locomotion enable + `speed` (world units/sec); see [movement.md](../../game/features/gameplay/movement.md).
- CompuQuest also ships generic (non–lobby-selectable) accessories such as **`energy_upkeep`**, **`movement_energy`**, **`eat`**, and **`computer_gun`** (see [hunger.md](../gameplay/hunger.md) / combat docs); they use the same schema. Lobby-selectable animal companions (`fox`, `squid`, `monkey`, `penguin`, `poison_dart_frog`) use `spawn_nearby_ally`.

### Actor schema

```json
{
  "id": "carrot_growing",
  "displayName": "Carrot",
  "accessories": ["grow_carrot"],
  "resources": [
    { "id": "max_health", "amount": 25 },
    { "id": "health", "amount": 25 }
  ],
  "depiction": {
    "kind": "texture",
    "path": "res://assets/compuquest/game-icons/delapouite/seedling.svg"
  }
}
```

- `id` required. `accessories` optional (default empty). `displayName`, `depiction`, `icon` optional. Optional **`size`** (`> 0`): base collision radius for projectile actors (see [combat.md](../../game/features/gameplay/combat.md)); omit for non-projectiles. Optional **`resources`**: array of `{ "id", "amount" }` starting amounts (`amount` ≥ **0**); applied in order at actor construction (set `max_health` before `health`).
- Mobile pawns (e.g. **`generic`**, **`zombie`**) live in the same catalog and typically include **`move`**, hunger accessories, and starting health/energy resources. Lobby-selectable abilities are still chosen in the lobby. Shipped **`missile`** is a projectile actor with `size` and no accessories/resources.


### Later similar catalogs

For a new definition kind in an extension: add `config/<plural-kind>/` under the extension project, copy it to `extensions/{AssemblyName}/` on build, and add a loader step in `DefinitionConfig` / `ExtensionLoader` after its dependencies. New effect types: register an `AddAccessoryEffectFactory` in the extension’s `Register`.

## Non-goals (for now)

- Hot-reload of definition JSON during play
- Host-root `config/` accessory catalogs (those remain for core/scenario/extensions settings only)
