# Animal companions

Lobby-selectable passive abilities that spawn a friendly animal ally near the player at world entry. Related: [accessories.md](accessories.md), [active-abilities.md](active-abilities.md), [characters.md](characters.md), [ai.md](ai.md), [factions.md](factions.md), [lobby.md](../ui/lobby.md), [depiction.md](depiction.md). Technical: [animal-companions.md](../../../technical/features/gameplay/animal-companions.md).

## Requirements

- Each companion is an **ability** accessory with **`activation.kind: none`** (not in-world equippable). Players **select/own** it in the lobby via `player_selectable` (point cost **1**, neutral domain).
- Shipped companions (each pairs with a character definition of the same id):
  - **Fox** (`fox`)
  - **Squid** (`squid`)
  - **Monkey** (`monkey`)
  - **Penguin** (`penguin`)
  - **Poison dart frog** (`poison_dart_frog`)
- When a player character that owns a companion accessory first ticks in the world, a **same-faction** AI character of that type spawns on a nearby grass hex (same nearby-grass rules as zombie spawns). Spawn runs once per accessory instance; if placement cannot find a hex, retry on later ticks until success.
- The spawned ally records the spawning player as its **owner** (`OwnerActorId`). Companions use the shared **`AiController`**: energy upkeep, movement energy, Eat, Swing; default aggression (**0.45**); nearest-hostile combat. They do **not** attack the owning player (same faction).
- **Owner follow** (see [ai.md](ai.md)): no roam; stay near the owner (still when within 1 hex); aggression may still pull them toward hostiles. When seriously injured and fleeing, they run to the owner.
- **Prototype art:** world depiction and lobby icons use game-icons SVGs as `texture` depictions until dedicated Kenney SpriteFrames exist (same exception class as placed vegetables).

## Non-goals (for now)

- Per-species unique combat kits
- Dedicated Kenney pawn frames for each animal
- Resummon / replace after companion death
