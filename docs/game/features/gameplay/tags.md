# Tags

Named labels on game data (e.g. which accessories players may choose, which resource types a character holds, which **domain** themes apply). Technical: [tags.md](../../../technical/features/gameplay/tags.md). Related: [resources.md](resources.md), [domains.md](domains.md).

## Requirements

- Content may mark accessories (and later other records) with tags such as **`player_selectable`** or domain tags (`gardening`, `computing`).
- Players only see accessories tagged as player-selectable in the lobby picker.
- Tag names are authored as strings; the game resolves them at load time.
- **Resource type** ids are tags; character resource amounts are keyed by those tags (see [resources.md](resources.md)).
- **Domain** ids are tags; accessories listed with those tags get domain-colored UI icons (see [domains.md](domains.md)).

## Non-goals (for now)

- Player-facing tag browser UI
