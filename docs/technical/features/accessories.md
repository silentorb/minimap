# Accessories (technical)

Accessory definitions, instances, and actor effect cache. Implements [../../game/features/accessories.md](../../game/features/accessories.md). Related: [actors.md](actors.md), [characters.md](characters.md), [controllers.md](controllers.md), [active-abilities.md](active-abilities.md), [cell-placement.md](cell-placement.md), [farming.md](farming.md), [resources.md](resources.md), [domains.md](domains.md), [extensions.md](extensions.md), [definition-config.md](definition-config.md), [depiction.md](depiction.md), [tags.md](tags.md), [players.md](players.md), [hunger.md](hunger.md).

## Requirements

- Contracts in **`Minimap.Simulation.Types`** (see that project’s `AGENTS.md`):
  - **`AccessoryEffect`** — abstract base; concrete sealed effects live in content extensions (default: CompuQuest).
  - **`IShootEffect`**, **`IOnAccessoryAcquired`**, **`IEffectUseCost`** (optional cost tag + amount; default free), **`IInteractionEffect`** (Simulation; needs world/actors), **`IPassiveEffect`** (character/world tick), **`IInstantUseEffect`** (modal activate without preview).
  - **`AccessoryActivation`** / **`AccessoryActivationKind`** — dedicated / modal / none metadata on definitions.
  - **`AccessoryDefinition`** — `Id` + effect templates + optional depiction / icon / tags / point cost / display fields + **Activation** + optional **`EnabledWhen`** (no accessory-level consumed resource fields).
  - **`Accessory`** — definition ref + effect instances; runtime **`IsEnabled`** from actor resources vs `EnabledWhen` (orthogonal to use cost).
- **`ICellPlacementEffect`** lives in **Simulation** (needs `GameWorld` / `HexAxial`); CompuQuest implements `PlaceRandomActorEffect` (`place_random_actor`).
- **`IExtensionRegistry`** catalogs accessory / character / actor / resource / domain definitions and accessory effect factories.
- **`IIntegrator.GetPlayerSelectableAccessories`** returns accessories that carry the content tag `player_selectable` (CompuQuest policy).
- **`Actor.AddAccessory` / `RemoveAccessory`**: append or drop; sync effect refs; invoke `IOnAccessoryAcquired`; rebuild `AbilityLoadout` on characters (also rebuild when enable state flips).
- Simulation systems read flat effects / loadout; shoot/place/interact/instant-use gate and consume via **`IEffectUseCost`** on the activating effect. Passive effects tick via **`IPassiveEffect`**.
- Shipped CompuQuest accessories under **`src/CompuQuest.Minimap/config/accessories/`**: `gun`, `farm`, `geek` (place + `use_computer` interact), `energy_upkeep`, `eat`, plus passive `grow_*` on vegetable actors. See [hunger.md](hunger.md).
