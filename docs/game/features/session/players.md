# Players

First-class local participants in a playthrough. Related: [lobby.md](../ui/lobby.md), [user-profiles.md](user-profiles.md), [accessories.md](../gameplay/accessories.md), [factions.md](../gameplay/factions.md). Technical: [players.md](../../../technical/features/session/players.md).

## Requirements

- Each human participant is a **player** with an accessory-point budget (from core settings; default **2**).
- In the lobby, after selecting a [user profile](user-profiles.md), a player may spend points on **player-selectable** accessories. They need not spend all points. Choosing moves an accessory to Owned; unchoosing a **this-stage** owned accessory returns it to Available. Accessories from a **previous stage** remain owned and cannot be unchosen (only one selection stage exists for now, but the lock path is wired).
- Selected accessories apply to that player’s character when the match starts.
- Backing out of a lobby slot to Available discards that slot’s profile and accessory selection; Selecting accessories ↔ Ready keeps choices (backing to profile selection also keeps accessory picks until the slot is fully cleared).

## Non-goals (for now)

- Online player accounts
