# AI

AI-controlled characters. Related: [combat.md](combat.md), [factions.md](factions.md), [health.md](health.md), [farming.md](farming.md), [hunger.md](hunger.md). Technical control: [controllers.md](../../../technical/features/gameplay/controllers.md), [navigation.md](../../../technical/features/gameplay/navigation.md).

## Requirements

- AI characters use a single **`AiController`** with an **aggression** property in **[0, 1]**:
  - **0** — pure roam (random grass goals / pause). They still aim and Swing/shoot when a hostile is already in range, and farmers still harvest/eat when a viable target is in interact range.
  - **1** — always beeline toward the nearest **significant goal**.
  - Mid values — blend a random roam goal with a gentle pull toward that goal (`lerp`).
- **Significant goals** depend on the AI:
  - Regular zombies / crazed carrots: nearest living hostile ([factions.md](factions.md)).
  - **Zombie farmers**: nearest living hostile **or** ripe plant (equal pull — nearest by distance across both sets).
- Default rival AI aggression is **0.45**. Emerged **crazed carrots** use **0.9**.
- AI engage hostiles with the same combat rules as a player from the character’s point of view ([combat.md](combat.md)): aim at the nearest hostile each tick; fire `IShootEffect` / swing `ISwingEffect` when in reach. Wave / spawner zombies use Swing (not Gun).
- **Zombie farmers** also select Farm to harvest ripe plants via environment interact, and select Eat to restore energy when they hold food and are missing at least **5** energy.
- AI does **not** hard-code “attack the player.” Hostiles are defined by [factions.md](factions.md).
- Emerged crazed carrots use the rival faction and high-aggression AI (see [farming.md](farming.md)). Legacy default roster still places a small amount of ally/rival AI in tests.

## Non-goals (for now)

- Cover, retreat, or coordinated group tactics
- Sticky combat target locks (aggression blend replaces the old chase-controller lock)
- Farmer planting AI
