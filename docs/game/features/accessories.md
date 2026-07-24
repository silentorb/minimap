# Accessories

System for attaching game logic to characters. Related: [characters.md](characters.md), [combat.md](combat.md), [tags.md](tags.md), [players.md](players.md), [lobby.md](lobby.md), [active-abilities.md](active-abilities.md), [cell-placement.md](cell-placement.md). Technical: [../../technical/features/accessories.md](../../technical/features/accessories.md).

## Requirements

- Each **character** has a list of **accessories**. Each **character definition** lists accessory definitions assigned at instantiation.
- Accessories may be beneficial (abilities, equipment) or harmful (debuffs). Equipment is modeled as accessories.
- An accessory does not carry behavior-specific fields of its own. It holds a list of **accessory effects** (modifiers). One accessory can mix independent effects.
- Each accessory instance has an **accessory definition**. Definitions ship as JSON with their content extension (CompuQuest: `src/CompuQuest.Minimap/config/accessories/`; see [../../technical/features/definition-config.md](../../technical/features/definition-config.md)); extensions may also register definitions in code.
- Definitions may include **tags**, **pointCost** (default **0**), **displayName**, **description**, depiction, icon, and optional **activation** (dedicated / modal / none).
- When an accessory is **added** to a character, its effects are added to a flat **effect list on the character**. When it is **removed**, those effects are removed. Simulation queries the character’s effects, not accessory trees. The character **ability loadout** rebuilds from accessory activation metadata.
- Shipped player-selectable accessories (each cost **1**): **Gun** (dedicated shoot), **Plant Vegetable** (modal placement), **Use Computer** (stub, no activation yet).
- Lobby accessory selection (see [lobby.md](lobby.md)): players spend accessory points on tagged `player_selectable` accessories.

## Non-goals (for now)

- In-world inventory / equip UI beyond lobby selection and modal ability select
- Character stats and stat-modifier apply/revert (future; motivates the character effect cache)
- Multiple weapon types beyond Gun
