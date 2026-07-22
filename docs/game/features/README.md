# Game feature docs (read on demand)

**Source of truth** for design, feel, and player-facing rules. Do **not** open every file for general engineering tasks.

1. Skim the **trigger** lines below.
2. If a trigger matches your current task, read **only** that markdown file.

| File | Read when… |
|------|------------|
| [../game-design.md](../game-design.md) | Changing **gameplay vision**, genre pillars, co-op scope, or world/evolution feel. |
| [health.md](health.md) | Changing **hit points**, max health, or **death / removal** on zero health. |
| [damage.md](damage.md) | Changing how **damage** applies, missile damage amounts, or friendly-fire rules. |
| [depiction.md](depiction.md) | Changing **depiction** on character/accessory definitions, Kenney / SpriteFrames presentation, or how pawns look. |
| [ui-icons.md](ui-icons.md) | Changing **UI / data-record icons**, game-icons art under `assets/compuquest/game-icons/`, or definition `icon` fields. |
| [characters.md](characters.md) | Changing **character definitions**, CompuQuest `config/characters/` JSON, default character, or definition vs instance rules. |
| [accessories.md](accessories.md) | Changing **accessories**, accessory effects, Gun, CompuQuest `config/accessories/` JSON, or character effect-cache rules. |
| [tags.md](tags.md) | Changing **tags** on definitions (e.g. `player_selectable`) or tag naming policy. |
| [players.md](players.md) | Changing **player** records, accessory points, or lobby→spawn accessory choices. |
| [ai.md](ai.md) | Changing **AI** wander / floor goals, AI combat behavior, or AI spawn counts per faction. |
| [combat.md](combat.md) | Changing **missiles**, shoot effect, fire rate, twin-stick aim, or AI nearest-hostile aim. |
| [factions.md](factions.md) | Changing **faction** membership, hostility, or surface faction ids / spawn mix. |
| [map-layout.md](map-layout.md) | Changing **map shape or size** (rectangle extents, single-screen arena). |
| [player-hud.md](player-hud.md) | Changing **player HUD** slots, names, or on-screen health display. |
| [lobby.md](lobby.md) | Changing **local player lobby**, join panels, or lobby → world start flow. |
| [scenarios.md](scenarios.md) | Changing **scenario** JSON, wave pacing, spawner counts, or level transitions. |
| [waves.md](waves.md) | Changing **wave timing**, spawner placement, or per-wave enemy volume. |
| [game-over.md](game-over.md) | Changing **game over** detection, overlay, or continue navigation. |
| [local-input.md](local-input.md) | Changing **gamepad/keyboard** binding, per-player devices, disconnect/reconnect, or in-world input. |
