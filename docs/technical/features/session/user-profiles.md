# User profiles (technical)

Implements [user-profiles.md](../../../game/features/session/user-profiles.md). Related: [lobby.md](../ui/lobby.md), [main-menu.md](../ui/main-menu.md), [local-play-context.md](local-play-context.md), [../platform/core-settings.md](../platform/core-settings.md), [players.md](players.md).

## Requirements

- **Persistence**: `user://player_profiles.json` (Client globalizes; **Minimap.App** `PlayerProfileStore` loads/saves absolute paths). Distinct from shipped `config/core.json`. Schema:

```json
{
  "profiles": [
    { "id": "<guid>", "name": "Alex", "deaths": 3 }
  ]
}
```

- Missing file → empty store. Validate unique ids, non-empty unique names (case-insensitive), `deaths >= 0`. Mutations save immediately.
- **Host hooks**: `WorldHostHooks` load/save delegates registered by `AppHostRegistration`. Client does not open the profiles path with App types directly.
- **Profiles scene**: `res://scenes/profiles.tscn` — root `ProfilesApp` (`Minimap.Client.Profiles`). Pure `ProfilesScreenModel` for list selection and create/rename/delete outcomes (unit-tested); Godot UI mirrors it. Rename/create use `LineEdit` with temporary exclusive keyboard focus.
- **Main menu**: **Profiles** control between **New** and **Quit** → profiles scene. Popup unchanged.
- **Lobby**: slot modes `Available` → `SelectingProfile` → `SelectingAccessories` → `Ready`. `LobbyProfileSelectionState` holds carousel index / confirmed profile id. `ProfileSelectionPanel` is select-only (no CRUD). Carousel omits profiles already chosen by other slots. Roster build includes `ProfileId` + `DisplayName` on `LocalPlayerEntry`.
- **Deaths**: `GameSession.HumanDeathsThisTick` lists player indices that transitioned alive → dead after `World.Tick`. Client maps to roster profile ids and increments via the store. `DropHumanPlayer` is not a death.

## Non-goals (for now)

- Broader user settings file beyond this profiles store
- Mid-match profile editing
