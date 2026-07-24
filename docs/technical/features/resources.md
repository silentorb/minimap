# Resources (technical)

Tag-keyed actor amounts and extension-registered resource types. Implements [../../game/features/resources.md](../../game/features/resources.md). Related: [tags.md](tags.md), [definition-config.md](definition-config.md), [extensions.md](extensions.md), [actors.md](actors.md), [characters-and-factions.md](characters-and-factions.md), [player-hud.md](player-hud.md), [accessories.md](accessories.md), [hunger.md](hunger.md).

## Requirements

- **`ResourceDefinition`** in **`Minimap.Simulation.Types`**: `Id`, `Tag` (`TagId`), optional `DisplayName` / `IconConfig`, optional `LimitTag`, `Visible` (default **true**), `UiPriority` (default **0**, may be negative; higher sorts first in HUD).
- Loading a resource JSON id uses **`TagRegistry.GetOrCreate`**; the type identity **is** that tag.
- **`IExtensionRegistry`**: `AddResourceDefinition`, list, `TryGet` by id and by `TagId`. Duplicate ids fail fast.
- After a catalog load, validate: every `LimitTag` resolves to a registered resource type; a type **referenced as** someone’s limit must not itself have a `LimitTag`.
- **`GameContent`** includes the resource catalog plus resolved **`HealthTag`** / **`MaxHealthTag`** (from ids `health` / `max_health`) for combat, and **`EnergyTag`** / **`MaxEnergyTag`** (from ids `energy` / `max_energy`) for hunger (see [hunger.md](hunger.md)).
- **`Actor`**: `Dictionary<TagId, int>` bag with `GetResource` / `SetResource` / `AddResource` / `TryConsumeResource` (explicit failure when stock is insufficient). Set/add clamp to `[0, limit]` when the type has a `LimitTag`.
- No dedicated float `Health` / `MaxHealth` / `Energy` fields; combat and death use health / max_health tags from content; hunger uses energy / max_energy.
- **`IEffectUseCost`** on activatable effects; **`IOnAccessoryAcquired`** / `modify_resource` for grants. Helpers afford/consume against the effect cost (not accessory-level fields).
- JSON under extension `config/resources/`; load order **actors → resources → accessories → characters** (see [definition-config.md](definition-config.md)).
- HUD DTOs stay Simulation-free: Client maps visible resources (sorted by `UiPriority` descending) to icon path + amount or `amount / limitAmount`.

## Non-goals (for now)

- Fixed numeric limit property on resource types
- Persisting `TagId` values across process runs
