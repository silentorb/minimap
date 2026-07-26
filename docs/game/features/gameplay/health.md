# Health

Actor hit points as resources. Related: [resources.md](resources.md), [damage.md](damage.md), [combat.md](combat.md), [actors.md](actors.md), [factions.md](factions.md), [hunger.md](hunger.md).

## Requirements

- Any **actor** may have **current health** (`health` resource) and **max health** (`max_health` resource). Health is limited by max health.
- An actor with **no positive max health** is **indestructible**: it has no health bag for combat, and damage does not apply.
- An actor with positive max health is **destructible**. When health reaches **0 or below**, the actor **dies**.
- **Characters** always start destructible. Default **max health** (and starting health) is **100**.
- **Cell-anchored placeables** (CompuQuest): computers start with max/start health **40**; planted vegetables (carrot / corn / melon) with **25**; **zombie spawners** with **400**. Starting amounts come from actor definition `resources`.
- Death **quietly removes** the actor from the map: no death animation, VFX, or UI for now. Characters (and their controllers) are gone on the next simulation update; cell-anchored actors are removed from occupancy the same way. When a human player’s character dies and that player has a [user profile](../session/user-profiles.md), the profile’s death count increments (disconnect drop does not).
- Some characters declare a **death drop** (`death_drop` effect): before removal, if the hex under the corpse is empty, place that cell actor (e.g. crazed carrot → free-loot carrot). See [farming.md](farming.md).
- **Level transition** (after all waves in a level, when the parked wave runner is enabled): all human players are **healed to max health** and **resurrected** if they had died during the level. Inactive in normal sandbox play (see [scenarios.md](../session/scenarios.md)).
- **Game over** (all human players dead): see [game-over.md](../ui/game-over.md) for the pause overlay exception.

## Non-goals (for now)

- Armor or invulnerability frames

Vitality from [hunger.md](hunger.md) can heal or hurt character health over time based on energy bands. Player health is shown on the local HUD as part of the resource list; see [player-hud.md](../ui/player-hud.md) and [resources.md](resources.md).
