# Tags

Named labels on game data (e.g. which accessories players may choose, which resource types a character holds). Technical: [../../technical/features/tags.md](../../technical/features/tags.md). Related: [resources.md](resources.md).

## Requirements

- Content may mark accessories (and later other records) with tags such as **`player_selectable`**.
- Players only see accessories tagged as player-selectable in the lobby picker.
- Tag names are authored as strings; the game resolves them at load time.
- **Resource type** ids are tags; character resource amounts are keyed by those tags (see [resources.md](resources.md)).

## Non-goals (for now)

- Player-facing tag browser UI
