# Domains

Thematic tags that group accessories (and later other content) into strong visual/logic themes. Technical: [domains.md](../../../technical/features/gameplay/domains.md). Related: [tags.md](tags.md), [accessories.md](accessories.md), [ui-icons.md](../ui/ui-icons.md), [resources.md](resources.md).

## Terminology

- **Domain** — a named theme (e.g. gardening, computing) with a display color. Represented by a **tag** and a domain data record.
- **Neutral accessory** — an accessory with no domain tags (no domain-colored icon background).

## Requirements

- Extensions may register **domain** records. Each domain has an **id** that is also its **tag**, optional display name, and a **color**.
- Accessories may list one or more domain tags (alongside other tags such as `player_selectable`). Most abilities are domain-tagged; some accessories stay **neutral**.
- Shipped CompuQuest domains:
  - **`gardening`** — green (`#3A8F4B`). Tagged on **Farm** and vegetable **grow_*** accessories.
  - **`computing`** — light gray with a hint of blue (`#B4BEC8`). Tagged on **Geek**.
  - **`medical`** — crimson (`#C44B5A`). Tagged on **Heal**.
- Domain tag **`computing`** is distinct from the **`electronics`** resource and from the **computer** actor.
- When an accessory has domain tags, its **UI icon** uses those domain colors as the icon background (game-icons white glyph on a colored swatch):
  - one domain → solid color square
  - two domains → diagonal split (two triangles)
  - three or more → equal vertical stripes
- Neutral accessories keep the default baked black icon background.

## Non-goals (for now)

- Domain picker UI or exclusive domain loadouts
- Domain affinity on characters/actors beyond accessory tags
- Recoloring world depiction (Kenney / SpriteFrames)
