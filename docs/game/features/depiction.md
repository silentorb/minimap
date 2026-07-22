# Depiction

How content definitions describe presentation without coupling simulation to Godot sprites. Technical: [../../technical/features/depiction.md](../../technical/features/depiction.md).

## Requirements

- Character and accessory definitions may include an optional **depiction** describing how they appear.
- A depiction is not tied only to sprites: it has a **kind** and a resource reference. Sprite cases use Godot **SpriteFrames** resources (single frame or animation sequences authored there).
- Short-term: CompuQuest ships depictions for the **generic** character and **Gun** accessory using Kenney 1-Bit Pack art under `assets/compuquest/kenney-1bit/`.
- Characters with a sprite-frames depiction render as that sprite in the world; otherwise the placeholder colored square remains.

## Non-goals (for now)

- Accessory visuals attached to the pawn (equip overlay). Data-record / HUD icons are [ui-icons](ui-icons.md), not depiction.
- Player-facing art picker / customization UI
