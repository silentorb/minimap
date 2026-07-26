# Missiles and damage

Missile and Swing simulation implementing [combat.md](../../../game/features/gameplay/combat.md), [damage.md](../../../game/features/gameplay/damage.md), and [health.md](../../../game/features/gameplay/health.md).

## Requirements

- **`Missile`**: position, velocity, radius, damage (**int**), owner faction id, optional owner character id, **`FriendlyFire`** (default **true**).
- Defaults from game docs: speed **200**, damage **25**, fire interval **1.25** s, friendly fire **on**.
- Actor ids (characters and cell placeables) share one `GameWorld` allocator so owner-id skip cannot collide across kinds.
- Each tick: integrate position; destroy on wall contact; on overlap with a living destructible actor other than the owner:
  - If `FriendlyFire` **or** `FactionRules.AreHostile(ownerFaction, target.FactionId)`, apply damage and remove missile; otherwise ignore (pass through without damaging). Applies to characters and factioned cell actors.
- Character / cell-actor hit radius uses movement radius (`PlayerRadius`) unless a smaller missile radius is used (missile radius ~ half character radius is fine). Cell actors are tested at their cell world center.
- **`SwingArc`**: origin, facing, radius, arc degrees, damage, owner faction/character id, friendly fire, remaining visual lifetime. Spawned on Swing fire; hits resolved **once** at spawn (half-disk overlap); lifetime only drives client VFX.
- **`GameWorld.ApplyDamage(Actor, int)`**: no-op when target is not destructible or amount ≤ 0; otherwise subtract health and clamp at 0.
- After damage, if a destructible actor’s health ≤ 0: remove characters (and controllers) from the roster; remove cell actors from occupancy.

## Client

Sync missiles and Swing arcs by id for simple visuals; remove nodes when they despawn. Swing arcs render as a brief translucent half-disk.
