# User profiles (technical)

Implements [user-profiles.md](../../../game/features/session/user-profiles.md). Related: [lobby.md](../ui/lobby.md), [main-menu.md](../ui/main-menu.md), [local-play-context.md](local-play-context.md), [../platform/core-settings.md](../platform/core-settings.md), [players.md](players.md), [player-hud.md](../ui/player-hud.md).

## Requirements

- **Persistence**: `user://player_profiles.json` (Client globalizes; **Minimap.App** `PlayerProfileStore` loads/saves absolute paths). Distinct from shipped `config/core.json`. Schema:

```json
{
  "profiles": [
    {
      "id": "<guid>",
      "name": "Alex",
      "deaths": 3,
      "avatarFile": "<guid>.png",
      "unlockedAchievements": ["survive_5_minutes"]
    }
  ]
}
```

- Missing file → empty store. Validate unique ids, non-empty unique names (case-insensitive), `deaths >= 0`. Optional `unlockedAchievements` (string ids; missing → empty). Optional `avatarFile` (filename only under the avatars directory; missing → none). Reject absolute paths, directory separators, or empty strings for `avatarFile`. Mutations save immediately.
- **Avatar files**: `user://profile_avatars/{profileId}{ext}` via **Minimap.App** `PlayerAvatarStore`. Client globalizes `WorldHostHooks.DefaultPlayerAvatarsResPath`. Import copies a chosen filesystem image into that directory; replacing an avatar deletes the previous file for that profile. Deleting a profile deletes its avatar file when present.
- **Import rules** (expected failures → `Try*` + Profiles error label): extensions `.png`, `.jpg`, `.jpeg`, `.webp`; max source size **8 MiB**; destination name `{profileId}` + lowercase original extension.
- **Host hooks**: `WorldHostHooks` load/save profile delegates and avatar import/delete delegates registered by `AppHostRegistration`. Client does not open profile/avatar paths with App types directly.
- **Profiles scene**: `res://scenes/profiles.tscn` — root `ProfilesApp` (`Minimap.Client.Profiles`). Pure `ProfilesScreenModel` for list selection and create/rename/delete outcomes (unit-tested); Godot UI mirrors it. Rename/create use `LineEdit` with temporary exclusive keyboard focus. Detail panel shows avatar preview (`TextureRect`), **Change picture** (`FileDialog`: `OpenFile`, `Access.Filesystem`, image filters), and **Clear picture**. Missing/corrupt image → placeholder (do not fail boot). **Achievements** → set profile focus on `LocalPlayContext` and load `achievements.tscn` (see [achievements.md](achievements.md)).
- **Main menu**: **Profiles** control between **New** and **Quit** → profiles scene. Popup unchanged.
- **Lobby**: slot modes `Available` → `SelectingProfile` → `SelectingAccessories` → `Ready`. `LobbyProfileSelectionState` holds carousel index / confirmed profile id. `ProfileSelectionPanel` is select-only (no CRUD) and shows the profile avatar when present. Slot title shows a small avatar after a profile is confirmed. Carousel omits profiles already chosen by other slots. Roster build includes `ProfileId` + `DisplayName` + `AvatarFile` on `LocalPlayerEntry`.
- **Deaths**: `GameSession.HumanDeathsThisTick` lists player indices that transitioned alive → dead after `World.Tick`. Client maps to roster profile ids and increments via the store. `DropHumanPlayer` is not a death.

## Non-goals (for now)

- Broader user settings file beyond this profiles store
- Mid-match profile editing
- Automated FileDialog / OS file-picker tests
