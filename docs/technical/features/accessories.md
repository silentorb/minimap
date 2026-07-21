# Accessories (technical)

Accessory definitions, instances, and character effect cache. Implements [../../game/features/accessories.md](../../game/features/accessories.md). Related: [characters.md](characters.md), [controllers.md](controllers.md), [extensions.md](extensions.md), [definition-config.md](definition-config.md), [depiction.md](depiction.md).

## Requirements

- Contracts in **`Minimap.Simulation.Types`** (see that project’s `AGENTS.md`):
  - **`AccessoryEffect`** — abstract base; concrete sealed effects live in content extensions (default: CompuQuest).
  - **`IShootEffect`** — shoot params + runtime cooldown (no intrinsic aim). Simulation fire logic depends on this contract.
  - **`AccessoryDefinition`** — `Id` + effect templates + optional `DepictionConfig`.
  - **`Accessory`** — definition ref + effect instances only.
- **`IExtensionRegistry`** catalogs accessory definitions in **registration order** (`AddAccessoryDefinition` / `AccessoryDefinitions`) and **accessory effect factories** (`AddAccessoryEffectFactory`) used when loading definition JSON.
- **`Character.AddAccessory` / `RemoveAccessory`**: append or drop the accessory; sync the same effect **object references** onto / off of **`Character.Effects`**.
- Simulation systems (e.g. shoot) read **`character.Effects`**, not accessory trees; shoot uses **`IShootEffect`**.
- Shipped **Gun** is JSON under **`src/CompuQuest.Minimap/config/accessories/gun.json`** (copied to `extensions/CompuQuest.Minimap/` on build; CompuQuest `ShootEffect` with combat doc values: interval **1.25** s, speed **200**, damage **25**, friendly fire **on**; includes a sprite-frames depiction). Loaded by App into the registry (see [definition-config.md](definition-config.md)).
