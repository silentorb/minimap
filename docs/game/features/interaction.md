# Environment interaction

Player characters interacting with actors in the map. Related: [actors.md](actors.md), [accessories.md](accessories.md), [active-abilities.md](active-abilities.md), [farming.md](farming.md), [local-input.md](local-input.md). Technical: [../../technical/features/interaction.md](../../technical/features/interaction.md).

## Requirements

- Players interact with certain actors by **facing** them and being in **close proximity** (the map cell in front of the player).
- Whether a target is valid depends on **interaction effects** on the player’s **currently selected** ability (modal accessory). Effects expose whether an actor is a valid target.
- When a valid interact target exists, that actor is **highlighted** so the player knows interact is available.
- Interact invoke: gamepad **`JoyButton.A`** / keyboard **E**. The selected ability’s interaction effects perform the action (e.g. Farm harvest).
- Modal activate (`X` / Space) remains separate and is not used for environment interact.

## Non-goals (for now)

- Multiple simultaneous interact targets or a radial chooser
- Interacting with mobile characters
- Ability-specific interact chrome beyond highlight
