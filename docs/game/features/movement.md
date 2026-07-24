# Movement

Realtime character motion and collision. Related: [local-input.md](local-input.md), [ai.md](ai.md), [map-layout.md](map-layout.md).

## Requirements

- Characters move in **realtime** with **cartesian** motion driven by **screen-axis** input (or AI move intent using the same motion rules).
- Hex walls and other characters are **solid colliders** with **slide** (characters do not pass through walls or each other).
