# Missiles and damage

Missile and Swing simulation implementing [combat.md](../../../game/features/gameplay/combat.md), [damage.md](../../../game/features/gameplay/damage.md), and [health.md](../../../game/features/gameplay/health.md).

## Requirements

- **Projectile actors**: free `Actor` instances with optional **`ProjectileFlight`** state (velocity, damage, friendly fire, owner actor id, effective size/radius, range, origin, distance traveled). Marked by non-null flight state (`IsProjectile`).
- Spawn via shoot: resolve `IShootEffect.ProjectileActorId` to an `ActorDefinition` that has **`Size`**; effective radius = `definition.Size * MissileSizeScale` (scale default **1**). Assign velocity = aim × `MissileSpeed`, damage / friendly fire / range from the effect. If the definition is missing or has no size, do not fire and do not consume the use cost.
- Defaults from game docs: speed **400** (shipped guns), damage **25**, fire interval **1.25** s, range **800** (distance), friendly fire **on**, shipped missile base size ≈ **12.285**.
- Actor ids share one `GameWorld` allocator so owner-id skip cannot collide across kinds.
- Each tick (`TickProjectiles`): integrate position; add `|velocity| * dt` to distance traveled; destroy when distance ≥ range; destroy on wall contact; on overlap with a living destructible **non-projectile** actor other than the owner:
  - If `FriendlyFire` **or** `FactionRules.AreHostile(ownerFaction, target.FactionId)`, apply damage and remove projectile; otherwise ignore (pass through without damaging). Applies to free actors and factioned cell actors.
- Target hit radius uses movement radius (`PlayerRadius`) plus the projectile’s effective size. Cell actors are tested at their cell world center.
- Projectiles are excluded from movement obstacles, nearest-hostile aim, and being damage targets of other projectiles.
- **`SwingArc`**: origin, facing, radius, arc degrees, damage, owner faction/character id, friendly fire, remaining visual lifetime. Spawned on Swing fire; hits resolved **once** at spawn (half-disk overlap); lifetime only drives client VFX. Swing skips projectile actors as free-body targets.
- **`GameWorld.ApplyDamage(Actor, int)`**: no-op when target is not destructible or amount ≤ 0; otherwise subtract health and clamp at 0.
- After damage, if a destructible actor’s health ≤ 0: remove characters (and controllers) from the roster; remove cell actors from occupancy.

## Client

Sync projectile actors (and Swing arcs) by id for simple visuals; scale projectile visuals from effective size; remove nodes when they despawn. Swing arcs render as a brief translucent half-disk. Free non-projectile actors use the normal character sync path (skip `IsProjectile`).
