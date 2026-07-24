# Resources

Character-owned amounts keyed by tags, plus registered resource types. Related: [tags.md](tags.md), [health.md](health.md), [accessories.md](accessories.md), [player-hud.md](player-hud.md). Technical: [../../technical/features/resources.md](../../technical/features/resources.md).

## Requirements

- Each **character** has a map of **tag → integer** amounts (resources the character owns).
- **Resource types** are content definitions registered by extensions. A type’s **id** is also a **tag** (same tag system as elsewhere).
- Type data may include **display name**, **icon**, optional **limit** (another resource type that stores the max), **visible** (whether the type appears in UI resource lists), and **uiPriority** (HUD sort; higher first; may be negative).
- When a type has a **limit**, current amount is clamped to that limit resource’s value. A resource used **as** a limit must not itself be limited by another resource. (A future fixed numeric limit on types is out of scope and may later constrain limit resources.)
- Types with **`visible: false`** are omitted from HUD enumerations. Limit types (e.g. max health) ship hidden; the limited resource shows **`VALUE / MAX`** using the limit amount.
- **Health** is the `health` resource, limited by `max_health`. Default max (and starting) health is **100**.
- Placement / shoot accessories may declare a **resource** they consume and a **starting amount** granted when the accessory is added. Successful use consumes **1**; the accessory cannot be used at **0**. There is no way yet to gain more of those stocks after grant.
- Shipped CompuQuest types: `health`, `max_health`, `computers`, `seeds`, `ammo`. Shipped accessory stocks: **Geek** → computers (1), **Plant Vegetable** → seeds (3), **Gun** → ammo (6).

## Non-goals (for now)

- Fixed numeric limits on resource types
- Ways to refill ammo / seeds / computers after grant
- Resource trading or shared party pools
