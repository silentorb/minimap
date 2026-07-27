# Players (technical)

Simulation `Player` records and client controller association. Implements [players.md](../../../game/features/session/players.md). Related: [controllers.md](../gameplay/controllers.md), [lobby.md](../ui/lobby.md), [core-settings.md](../platform/core-settings.md), [accessories.md](../gameplay/accessories.md).

## Requirements

- **`Player`** (Simulation): slot `Id`, starting `AccessoryPoints`, selected `AccessoryDefinition`s, optional `Character` pawn link.
- **`GameSession`** owns `Players` (1–4). Create applies lobby-selected accessories onto each pawn after spawning from `GameContent.DefaultActor`.
- **`PlayerController`** (Client) is constructed with a `Player` and possesses that player’s character.
- Device bindings remain on client **`LocalPlayRoster` / `LocalPlayerEntry`** (including selected accessory defs for spawn).
- Core setting **`player.accessoryPoints`** (default **2**) seeds each `Player.AccessoryPoints` and the lobby selection budget.

## Non-goals (for now)

- Networked player replication
