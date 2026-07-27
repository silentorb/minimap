# AI

AI-controlled characters. Related: [combat.md](combat.md), [factions.md](factions.md), [health.md](health.md), [farming.md](farming.md), [hunger.md](hunger.md), [animal-companions.md](animal-companions.md). Technical control: [controllers.md](../../../technical/features/gameplay/controllers.md), [navigation.md](../../../technical/features/gameplay/navigation.md).

## Requirements

- AI characters use a single **`AiController`** with an **aggression** property in **[0, 1]**:
  - **0** — pure roam (random grass goals / pause) when unowned. They still aim and Swing/shoot when a hostile is already in range, and farmers still harvest/eat when a viable target is in interact range.
  - **1** — always beeline toward the nearest **significant goal**.
  - Mid values — blend an anchor (roam when unowned; owner when owned) with a gentle pull toward that goal (`lerp`).
- **Significant goals** depend on the AI:
  - Regular zombies / crazed carrots: nearest living hostile ([factions.md](factions.md)).
  - **Zombie farmers**: nearest living hostile **or** ripe plant (equal pull — nearest by distance across both sets).
- Default rival AI aggression is **0.45**. Emerged **crazed carrots** use **0.9**.
- AI engage hostiles with the same combat rules as a player from the character’s point of view ([combat.md](combat.md)): aim at the nearest hostile each tick; fire `IShootEffect` / swing `ISwingEffect` when in reach. Wave / spawner zombies use Swing (not Gun).
- **Zombie farmers** also select Farm to harvest ripe plants via environment interact, and select Eat to restore energy when they hold food and are missing at least **5** energy.
- AI does **not** hard-code “attack the player.” Hostiles are defined by [factions.md](factions.md).
- Emerged crazed carrots use the rival faction and high-aggression AI (see [farming.md](farming.md)). Legacy default roster still places a small amount of ally/rival AI in tests.

### Injury flee

- When remaining health is **≤ 30%** of max health and the AI is not already fleeing, each goal retarget rolls flee with probability **`1 − aggression`** (aggression **1** never flees; **0** always flees when the check runs).
- Flee lasts **3 seconds**, then normal goals resume (another roll may start a new flee while still seriously injured).
- **Flee destination:**
  - Living **owner** ([animal-companions.md](animal-companions.md)): run toward the owner.
  - Otherwise: run **away** from the nearest living hostile for the flee duration.

### Owner follow

- When the AI has a living **owner** and is not fleeing, it does **not** roam. The movement anchor is the owner’s position; when within **1 hex** of the owner, the AI stays still (clears the move goal) unless aggression pulls it toward a significant goal.
- Aggression still blends the owner anchor toward hostiles / ripe crops, so owned AI may leave the owner to fight.

## Non-goals (for now)

- Cover or coordinated group tactics
- Sticky combat target locks (aggression blend replaces the old chase-controller lock)
- Farmer planting AI
