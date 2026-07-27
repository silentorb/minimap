# Medical

Medkits and the **Heal** ability. Related: [resources.md](resources.md), [health.md](health.md), [accessories.md](accessories.md), [active-abilities.md](active-abilities.md), [interaction.md](interaction.md), [domains.md](domains.md), [tags.md](tags.md), [actors.md](actors.md). Technical: [medical.md](../../../technical/features/gameplay/medical.md).

## Requirements

- **Medical** is a shipped domain (`medical`) with a crimson display color. The **Heal** ability is tagged with it (domain-colored icon).
- **Medkits** (`medkits`) are a visible actor resource. There is no way yet to refill medkits after the acquire grant (same pattern as ammo / seeds / electronics).
- **Heal** is a player-selectable modal ability (lobby point cost **1**). It is **disabled** when the actor has **medkits < 1**, and **enabled** when **medkits ≥ 1**. Acquiring Heal grants **3** medkits. Each successful heal costs **1** medkit.
- **Activate** (modal activate): restores the **caster** to full health (`health` = `max_health`) when the caster is **injured** (`health` < `max_health`), can afford the cost, and is tagged **`human`** or **`animal`**.
- **Interact** while Heal is the selected modal: restores a **front-cell target** under the same injured + human/animal + cost gates. The caster pays the medkit cost. Valid targets include **mobile** (free) actors whose map cell is the hex in front of the player, as well as cell-anchored actors that meet the gates.
- Heal does **not** apply to zombies, placeables, full-health allies, or untagged actors. Self-heal at full health is a no-op (no medkit spent).

## Non-goals (for now)

- Partial heal amounts or overheal
- Healing hostile or rival-faction actors by special rule (faction is not a gate; tags and injured state are)
- Ways to refill medkits after the acquire grant
