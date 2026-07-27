# Environment interaction

Player characters interacting with actors in the map. Related: [actors.md](actors.md), [accessories.md](accessories.md), [active-abilities.md](active-abilities.md), [farming.md](farming.md), [local-input.md](../session/local-input.md). Technical: [interaction.md](../../../technical/features/gameplay/interaction.md).

## Requirements

- Players interact with certain actors by **facing** them and being in **close proximity** (the map cell in front of the player).
- Interaction is **object-relative**, not gated solely by the equipped ability:
  1. Is the player in position to interact with an actor (front cell occupied)?
  2. If yes, determine whether there is a **default** interaction for that subject actor and object actor.
  3. If the player has an **equipped** modal ability with an interaction option that matches the current situation, that option **overrides** the default.
- Modal activate (`X` / Space) remains ability-tied and is separate from environment interact. Interact (E / A) can succeed with **no** modal equipped when a default exists (e.g. picked carrot pickup).
- When a valid interact target exists, that actor is **highlighted** so the player knows interact is available.
- Interact invoke: gamepad **`JoyButton.A`** / keyboard **E**.
- Interaction use costs: Farm **harvest** and Geek **use computer** cost **1 energy**; picked carrot **pickup** costs **1 energy** and grants **+1 food** (no ability required); **Heal** costs **1 medkit** (see [medical.md](medical.md)).
- Front-cell targets include **cell-anchored** placeables and **mobile** (free) actors whose position maps to that hex. When several candidates share the cell, the selected modal’s interaction effect is tried on each until one matches; otherwise the first valid **default** interaction wins.
- Examples: Farm **harvest** overrides on mature crops; Geek **use computer** when Geek is selected; **Heal** restores an injured human/animal in front; picked carrots use a **default** pickup.

## Non-goals (for now)

- Multiple simultaneous interact targets or a radial chooser
- Continuous (non-hex) proximity radii for interact
- Ability-specific interact chrome beyond highlight
