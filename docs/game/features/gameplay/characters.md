# Characters

Character definitions vs runtime instances. Related: [actors.md](actors.md), [factions.md](factions.md), [health.md](health.md), [accessories.md](accessories.md), [combat.md](combat.md). Technical: [characters.md](../../../technical/features/gameplay/characters.md), [characters-and-factions.md](../../../technical/features/gameplay/characters-and-factions.md).

## Requirements

- A **character** is an **actor** specialized as a possessable mobile pawn (resources including health, position, faction, accessories/effects, ability loadout) plus a **character definition** that describes what to spawn. See [actors.md](actors.md).
- Character definitions list **accessory definitions** applied when the character is instantiated, and may include a **depiction** (see [depiction.md](depiction.md)).
- Short-term: one **generic** character definition (`src/CompuQuest.Minimap/config/characters/generic.json`) is used for player-faction humans. That definition has **no** starting combat accessories — players obtain abilities (e.g. **Gun**, **Swing**, **Farm**, **Geek**) only via lobby selection (see [accessories.md](accessories.md), [lobby.md](../ui/lobby.md)).
- Playthrough content supplies a **default character** definition used when populating the world.
- Spawner enemies list accessories on their character definition:
  - **zombie** — **Swing**, energy upkeep, movement energy, Eat (AI does not use Eat).
  - **zombie_farmer** — same as zombie plus **Farm**; AI harvests mature crops and eats food when energy is low (see [ai.md](ai.md)).
- **Crazed carrot** (`crazed_carrot`) is a rival-faction vegetable monster (same faction as zombies) with **Swing**, energy upkeep, movement energy, Eat, high-aggression AI (**0.9**), and a death drop of a free-loot carrot. It emerges from planted crazed carrot crops (see [farming.md](farming.md)).


## Non-goals (for now)

- Player-facing character selection or customization
- Character stats beyond resources (future; accessories/effects are shaped to support them later)
