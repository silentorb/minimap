# Missiles and damage

Missile simulation implementing [../../game/features/combat.md](../../game/features/combat.md), [../../game/features/damage.md](../../game/features/damage.md), and [../../game/features/health.md](../../game/features/health.md).

## Requirements

- **`Missile`**: position, velocity, radius, damage, owner faction id, optional owner character id.
- Defaults from game docs: speed **200**, damage **25**, fire interval **1.25** s.
- Each tick: integrate position; destroy on wall contact; on overlap with a living character, if `FactionRules.AreHostile(ownerFaction, target.FactionId)`, apply damage and remove missile; otherwise ignore (or pass through same-faction without damaging).
- Character radius for hit tests matches movement radius unless a smaller missile radius is used (missile radius ~ half character radius is fine).
- After damage, if `Health <= 0`, remove character and controller from the world.

## Client

Sync missiles by id for simple visuals; remove nodes when missiles despawn.
