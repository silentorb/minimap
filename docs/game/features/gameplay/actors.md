# Actors

General runtime entities in the world. Related: [accessories.md](accessories.md), [health.md](health.md), [farming.md](farming.md), [movement.md](movement.md), [interaction.md](interaction.md), [cell-placement.md](cell-placement.md), [depiction.md](depiction.md), [factions.md](factions.md). Technical: [actors.md](../../../technical/features/gameplay/actors.md), [characters-and-factions.md](../../../technical/features/gameplay/characters-and-factions.md).

## Requirements

- An **actor** is a runtime entity with identity, accessories/effects, resources, facing, depiction (including an optional runtime depiction override), cartesian **position**, move intent, and ability loadout.
- An **actor definition** describes what to spawn: id, optional display name / depiction / icon, optional **tags** (e.g. `human` / `animal`), ordered accessory definitions applied at instantiation, optional **starting resources** (e.g. health / max health / energy), and optional **`size`** (base collision radius for projectiles).
- All actor definitions ship as JSON under the content extension (`src/CompuQuest.Minimap/config/actors/`). There is no separate character catalog.
- **Projectile actors** (e.g. shipped **`missile`**) are free actors with flight state assigned when a gun fires; they have no combat accessories and are not locomotion pawns (see [combat.md](combat.md)).
- **Locomotion** is not intrinsic: actors that should move include the passive **`move`** accessory (see [movement.md](movement.md)).
- **Cell occupancy** is optional: cell-anchored actors also appear in the world’s actor collection and in a cell → actor map. Free actors are only in the collection. Placement snaps to cell centers.
- Destructible actors die and are removed when health reaches 0 (quiet removal; controllers unpossess when attached).
- Playthrough content supplies a **default actor** definition (`generic`) used for player-faction humans. That definition is tagged **`human`** and has **no** starting combat accessories — players obtain abilities (e.g. **Gun**, **Swing**, **Farm**, **Geek**, **Heal**) only via lobby selection (see [accessories.md](accessories.md), [lobby.md](../ui/lobby.md)).
- Spawner / AI defs list accessories on the actor definition:
  - **zombie** — **move**, **Swing**, energy upkeep, movement energy, Eat (AI does not use Eat). Untagged for medical classify.
  - **zombie_farmer** — same as zombie plus **Farm**; AI harvests ripe plants and eats food when energy is low (see [ai.md](ai.md)).
- **Animal companions** (`fox`, `squid`, `monkey`, `penguin`, `poison_dart_frog`) are tagged **`animal`**, use the same accessory set as **zombie** for now, and spawn as same-faction AI when the matching lobby ability is owned (see [animal-companions.md](animal-companions.md)).
- **Crazed carrot** (`crazed_carrot`) is a rival-faction vegetable monster (same faction as zombies) with **move**, **Swing**, energy upkeep, movement energy, Eat, high-aggression AI (**0.9**), and a death drop of a picked carrot. It emerges from planted crazed carrot growing plants (see [farming.md](farming.md)).

## Non-goals (for now)

- Occupancy blocking free-actor movement
- Player-facing actor selection or customization
- Actor stats beyond resources (future; accessories/effects are shaped to support them later)
