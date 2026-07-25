# Damage

How health is reduced. Related: [health.md](health.md), [resources.md](resources.md), [combat.md](combat.md), [actors.md](actors.md), [factions.md](factions.md).

## Requirements

- Damage subtracts from the target’s current **health** resource (integer; see [health.md](health.md) / [resources.md](resources.md)).
- Damage applies to any **destructible** actor (characters and cell-anchored placeables with positive max health). **Indestructible** actors ignore damage.
- A successful **missile hit** deals **25** damage.
- A successful **Swing** hit deals **30** damage (see [combat.md](combat.md)).
- By default, missile and Swing damage to **characters** applies to any living character except the attacker (**friendly fire on**). Same-faction characters take damage from each other’s attacks. Attacks may opt out (`FriendlyFire = false`) to damage hostile characters only.
- **Cell-anchored actors** have no faction; when hit by a missile or Swing, they always take damage if destructible (friendly-fire filter does not apply).
- Health is clamped at a minimum of 0 after damage; reaching 0 triggers death removal per [health.md](health.md).

## Non-goals (for now)

- Damage types, crits, knockback, or DoTs
- Environmental / hazard damage (terrain hazards may exist visually but are out of scope here)
