# Accessories (technical)

Accessory definitions, instances, and character effect cache. Implements [../../game/features/accessories.md](../../game/features/accessories.md). Related: [characters.md](characters.md), [controllers.md](controllers.md), [active-abilities.md](active-abilities.md), [cell-placement.md](cell-placement.md), [extensions.md](extensions.md), [definition-config.md](definition-config.md), [depiction.md](depiction.md), [tags.md](tags.md), [players.md](players.md).

## Requirements

- Contracts in **`Minimap.Simulation.Types`** (see that project’s `AGENTS.md`):
  - **`AccessoryEffect`** — abstract base; concrete sealed effects live in content extensions (default: CompuQuest).
  - **`IShootEffect`** — shoot params + runtime cooldown (no intrinsic aim). Simulation fire logic depends on this contract.
  - **`AccessoryActivation`** / **`AccessoryActivationKind`** — dedicated / modal / none metadata on definitions.
  - **`AccessoryDefinition`** — `Id` + effect templates + optional depiction / icon / tags / point cost / display fields + **Activation**.
  - **`Accessory`** — definition ref + effect instances only.
- **`ICellPlacementEffect`** lives in **Simulation** (needs `GameWorld` / `HexAxial`); CompuQuest implements `PlaceRandomObjectEffect`.
- **`IExtensionRegistry`** catalogs accessory / character / placed-object definitions and accessory effect factories.
- **`IIntegrator.GetPlayerSelectableAccessories`** returns accessories that carry the content tag `player_selectable` (CompuQuest policy).
- **`Character.AddAccessory` / `RemoveAccessory`**: append or drop the accessory; sync effect refs onto **`Character.Effects`**; rebuild **`AbilityLoadout`**.
- Simulation systems read **`character.Effects`** / loadout, not accessory trees; shoot uses **`IShootEffect`**.
- Shipped CompuQuest accessories under **`src/CompuQuest.Minimap/config/accessories/`**: `gun` (dedicated `primary_fire`), `plant_vegetable` (modal `place_random_object`), `use_computer` (stub).
