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
- Interaction use costs: Farm **harvest** and Geek **use computer** cost **1 energy**; picked carrot **pickup** costs **1 energy** and grants **+1 food** (no ability required).
- Examples: Farm **harvest** overrides on mature crops; Geek **use computer** when Geek is selected; picked carrots use a **default** pickup.

## Non-goals (for now)

- Multiple simultaneous interact targets or a radial chooser
- Interacting with mobile characters
- Ability-specific interact chrome beyond highlight
