# Accessories (technical)

Accessory definitions, instances, and character effect cache. Implements [../../game/features/accessories.md](../../game/features/accessories.md). Related: [characters.md](characters.md), [controllers.md](controllers.md), [extensions.md](extensions.md).

## Requirements

- Types in **`Minimap.Simulation.Types`**:
  - **`AccessoryEffect`** — base for independent effects; behavior-specific data lives on effect types, not on `Accessory`.
  - **`ShootEffect`** — fire interval, missile speed, damage, and runtime `CooldownRemaining` (no intrinsic aim).
  - **`AccessoryDefinition`** — `Id` + effect templates.
  - **`Accessory`** — definition ref + effect instances only.
- **`IExtensionRegistry`** catalogs accessory definitions in **registration order** (`AddAccessoryDefinition` / `AccessoryDefinitions`).
- **`Character.AddAccessory` / `RemoveAccessory`**: append or drop the accessory; sync the same effect **object references** onto / off of **`Character.Effects`**.
- Simulation systems (e.g. shoot) read **`character.Effects`**, not accessory trees.
- CompuQuest registers **Gun** (`ShootEffect` with combat doc values: interval **1.25** s, speed **200**, damage **25**).
