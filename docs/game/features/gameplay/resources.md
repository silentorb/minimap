# Resources

Actor-owned amounts keyed by tags, plus registered resource types. Related: [tags.md](tags.md), [health.md](health.md), [hunger.md](hunger.md), [medical.md](medical.md), [accessories.md](accessories.md), [farming.md](farming.md), [player-hud.md](../ui/player-hud.md). Technical: [resources.md](../../../technical/features/gameplay/resources.md).

## Requirements

- Each **actor** (including characters) has a map of **tag → integer** amounts (resources the actor owns).
- **Resource types** are content definitions registered by extensions. A type’s **id** is also a **tag** (same tag system as elsewhere).
- Type data may include **display name**, **icon**, optional **limit** (another resource type that stores the max), **visible** (whether the type appears in UI resource lists), and **uiPriority** (HUD sort; higher first; may be negative).
- When a type has a **limit**, current amount is clamped to that limit resource’s value. A resource used **as** a limit must not itself be limited by another resource. (A future fixed numeric limit on types is out of scope and may later constrain limit resources.)
- Types with **`visible: false`** are omitted from HUD enumerations. Limit types (e.g. max health) ship hidden; the limited resource shows **`VALUE / MAX`** using the limit amount.
- **Health** is the `health` resource, limited by `max_health`. Any actor may have these; characters default max (and starting) health is **100**. Placeables may set starting amounts via actor definition `resources` (see [health.md](health.md)). Actors without positive max health are indestructible.
- **Energy** is the `energy` resource, limited by `max_energy`. Default max (and starting) energy is **100**. See [hunger.md](hunger.md).
- Effect **use costs** (optional resource tag + amount on an activatable effect; default free) gate activation; successful use consumes the cost. Shipped activations that had no cost now cost **≥1 energy** (Swing, harvest, use-computer, pickup); effects that already cost another resource keep that cost only. Acquire grants use a `modify_resource` effect when the accessory is added. There is no way yet to refill ammo / seeds / electronics / medkits after the acquire grant (food accumulates via harvest; **Eat** spends food to restore energy; **Heal** spends medkits to restore health; computer turrets start with **10** ammo from `computer_gun`).
- Shipped CompuQuest types: `health`, `max_health`, `energy`, `max_energy`, `electronics`, `seeds`, `ammo`, `food`, `medkits`. Acquire grants: **Geek** → electronics (1), **Farm** → seeds (3), **Gun** → ammo (6), **computer gun** → ammo (10), **Heal** → medkits (3). Harvest yields food (carrot 1 / corn 2 / melon 3). Eating costs **1 food** and restores **+5 energy** (see [hunger.md](hunger.md)). Healing costs **1 medkit** and restores full health (see [medical.md](medical.md)).

## Non-goals (for now)

- Fixed numeric limits on resource types
- Ways to refill ammo / seeds / electronics / medkits after grant
- Resource trading or shared party pools
