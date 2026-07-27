# Tags

Runtime tag ids for classifying definitions and content. Related: [accessories.md](accessories.md), [resources.md](resources.md), [domains.md](domains.md), [extensions.md](../platform/extensions.md), [definition-config.md](../platform/definition-config.md).

## Requirements

- Tags have **string** names in file data; at runtime they resolve to **`TagId`** (`int`-backed, process-local).
- **`TagRegistry`** is an **instance** (not static/global). Create-if-not-exists via `GetOrCreate(string)`. Ids are **not** stable across app runs.
- **`IExtensionRegistry`** owns a `TagRegistry` and exposes `RegisterTags(IEnumerable<string>)` for extensions.
- Tags are provided only through **definition JSON** and the **extension API** (not ad-hoc strings in Minimap.* gameplay code). Shipped tag strings live in **CompuQuest**.
- Shipped tags include **`player_selectable`** (CompuQuest; accessory filter), classify tags **`human`** / **`animal`** (actor definitions; see [medical.md](medical.md)), **domain ids** (`gardening`, `computing`, `medical`; see [domains.md](domains.md)), and **resource type ids** (`health`, `max_health`, `electronics`, `seeds`, `ammo`, `medkits`, …) resolved when loading resource definitions.
- When loading accessory or actor JSON, optional `"tags": ["…"]` resolves each string through `GetOrCreate`. Resource definition `id` values likewise resolve through `GetOrCreate`.
- Character resource amounts are keyed by **`TagId`** (see [resources.md](resources.md)).

## Non-goals (for now)

- Persisting tag ids to disk or network
- Tag hierarchies / wildcards
