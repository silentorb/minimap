# Accessories

System for attaching game logic to characters. Related: [characters.md](characters.md), [combat.md](combat.md). Technical: [../../technical/features/accessories.md](../../technical/features/accessories.md).

## Requirements

- Each **character** has a list of **accessories**. Each **character definition** lists accessory definitions assigned at instantiation.
- Accessories may be beneficial (abilities, equipment) or harmful (debuffs). Equipment is modeled as accessories.
- An accessory does not carry behavior-specific fields of its own. It holds a list of **accessory effects** (modifiers). One accessory can mix independent effects.
- Each accessory instance has an **accessory definition**. Definitions ship as extension content.
- When an accessory is **added** to a character, its effects are added to a flat **effect list on the character**. When it is **removed**, those effects are removed. Simulation queries the character’s effects, not accessory trees.
- First shipped accessory: **Gun**, which provides a shoot effect (see [combat.md](combat.md)).

## Non-goals (for now)

- Inventory UI, equip slots, or buff/debuff presentation
- Character stats and stat-modifier apply/revert (future; motivates the character effect cache)
- Multiple weapon types beyond Gun
