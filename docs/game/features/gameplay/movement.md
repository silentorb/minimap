# Movement

Realtime actor motion and collision. Related: [actors.md](actors.md), [local-input.md](../session/local-input.md), [ai.md](ai.md), [map-layout.md](../session/map-layout.md).

## Requirements

- Locomotion is granted by the passive **`move`** accessory (activation **none**; not lobby-selectable). Actors without **`move`** do not integrate move intent.
- Actors with **`move`** move in **realtime** with **cartesian** motion driven by **screen-axis** input (or AI move intent using the same motion rules). Speed comes from the move effect (default **120** world units per second).
- Hex walls and other free-moving actors are **solid colliders** with **slide** (they do not pass through walls or each other).
