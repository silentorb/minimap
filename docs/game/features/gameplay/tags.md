# Tags

Named labels on game data (e.g. which accessories players may choose, which resource types a character holds, which **domain** themes apply). Technical: [tags.md](../../../technical/features/gameplay/tags.md). Related: [resources.md](resources.md), [domains.md](domains.md).

## Requirements

- Content may mark accessories **and actors** with tags such as **`player_selectable`**, domain tags (`gardening`, `computing`, `medical`), or classify tags (`human`, `animal`).
- Players only see accessories tagged as player-selectable in the lobby picker.
- Tag names are authored as strings; the game resolves them at load time.
- **Resource type** ids are tags; character resource amounts are keyed by those tags (see [resources.md](resources.md)).
- **Domain** ids are tags; accessories listed with those tags get domain-colored UI icons (see [domains.md](domains.md)).
- **Actor** classify tags: shipped CompuQuest marks **`generic`** with **`human`**, and companion actors (`fox`, `squid`, `monkey`, `penguin`, `poison_dart_frog`) with **`animal`**. Used by **Heal** (see [medical.md](medical.md)).

## Non-goals (for now)

- Player-facing tag browser UI
