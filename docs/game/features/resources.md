# Resources

Actor-owned amounts keyed by tags, plus registered resource types. Related: [tags.md](tags.md), [health.md](health.md), [hunger.md](hunger.md), [accessories.md](accessories.md), [farming.md](farming.md), [player-hud.md](player-hud.md). Technical: [../../technical/features/resources.md](../../technical/features/resources.md).

## Requirements

- Each **actor** (including characters) has a map of **tag → integer** amounts (resources the actor owns).
- **Resource types** are content definitions registered by extensions. A type’s **id** is also a **tag** (same tag system as elsewhere).
- Type data may include **display name**, **icon**, optional **limit** (another resource type that stores the max), **visible** (whether the type appears in UI resource lists), and **uiPriority** (HUD sort; higher first; may be negative).
- When a type has a **limit**, current amount is clamped to that limit resource’s value. A resource used **as** a limit must not itself be limited by another resource. (A future fixed numeric limit on types is out of scope and may later constrain limit resources.)
- Types with **`visible: false`** are omitted from HUD enumerations. Limit types (e.g. max health) ship hidden; the limited resource shows **`VALUE / MAX`** using the limit amount.
- **Health** is the `health` resource, limited by `max_health`. Default max (and starting) health is **100**.
- **Energy** is the `energy` resource, limited by `max_energy`. Default max (and starting) energy is **100**. See [hunger.md](hunger.md).
- Effect **use costs** (optional resource tag + amount on an activatable effect; default free) gate activation; successful use consumes the cost. Acquire grants use a `modify_resource` effect when the accessory is added. There is no way yet to refill ammo / seeds / computers after the acquire grant (food accumulates via harvest; **Eat** spends food to restore energy).
- Shipped CompuQuest types: `health`, `max_health`, `energy`, `max_energy`, `computers`, `seeds`, `ammo`, `food`. Acquire grants: **Geek** → computers (1), **Farm** → seeds (3), **Gun** → ammo (6). Harvest yields food (carrot 1 / corn 2 / melon 3). Eating costs **1 food** and restores **+5 energy** (see [hunger.md](hunger.md)).

## Non-goals (for now)

- Fixed numeric limits on resource types
- Ways to refill ammo / seeds / computers after grant
- Resource trading or shared party pools
