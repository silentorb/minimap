# Game feature docs (read on demand)

**Features index** for CompuQuest (the surface game on Minimap). **Source of truth** for design, feel, and player-facing rules beyond the high-level pillars in [game-design.md](../game-design.md). Do **not** open every file for general engineering tasks.

1. Skim the **trigger** lines below.
2. If a trigger matches your current task, read **only** that markdown file.

| File | Read when… |
|------|------------|
| [../game-design.md](../game-design.md) | Reading **gameplay vision**, genre pillars, co-op scope, or world/evolution feel. **Do not edit** unless the user explicitly instructed changes to that file. |
| [resources.md](resources.md) | Changing **actor/character resources**, resource types, limits, HUD resource lists, or effect resource cost/grant. |
| [health.md](health.md) | Changing **hit points**, max health, or **death / removal** on zero health. |
| [hunger.md](hunger.md) | Changing **energy** / max energy, energy drain, vitality bands, **Eat**, or food-gated ability enable. |
| [movement.md](movement.md) | Changing **realtime cartesian movement**, screen-axis move input, or wall/character **slide** collision. |
| [damage.md](damage.md) | Changing how **damage** applies, missile damage amounts, or friendly-fire rules. |
| [depiction.md](depiction.md) | Changing **depiction** on character/accessory/actor definitions, Kenney / SpriteFrames presentation, or how pawns look. |
| [ui-icons.md](ui-icons.md) | Changing **UI / data-record icons**, game-icons art under `assets/compuquest/game-icons/`, or definition `icon` fields. |
| [actors.md](actors.md) | Changing **actors**, actor definitions, cell-anchored actors, or the actor vs character split. |
| [characters.md](characters.md) | Changing **character definitions**, CompuQuest `config/characters/` JSON, default character, or definition vs instance rules. |
| [accessories.md](accessories.md) | Changing **accessories**, **abilities**, accessory effects, Gun, Farm, Geek, CompuQuest `config/accessories/` JSON, or effect-cache rules. |
| [active-abilities.md](active-abilities.md) | Changing **dedicated vs modal** ability activation, preview/confirm vs immediate activate, D-pad / 1–4 select, or ability activate binds. |
| [interaction.md](interaction.md) | Changing **environment interact**, target highlight, A/E binds, or ability interaction effects. |
| [farming.md](farming.md) | Changing **Farm**, grow/harvest, seedlings, food yields, or vegetable actors. |
| [cell-placement.md](cell-placement.md) | Changing **cell occupancy**, placing actors on cells, placement preview, or Farm/Geek place rules. |
| [tags.md](tags.md) | Changing **tags** on definitions (e.g. `player_selectable`), resource type ids as tags, or tag naming policy. |
| [domains.md](domains.md) | Changing **domains** (gardening / computing themes), domain tags on accessories, or domain-colored icon swatches. |
| [players.md](players.md) | Changing **player** records, accessory points, or lobby→spawn accessory choices. |
| [ai.md](ai.md) | Changing **AI** wander / grass goals, AI combat behavior, or AI spawn counts per faction. |
| [combat.md](combat.md) | Changing **missiles**, shoot effect, fire rate, fire button, mouse/stick aim, or AI nearest-hostile aim. |
| [factions.md](factions.md) | Changing **faction** membership, hostility, or surface faction ids / spawn mix. |
| [map-layout.md](map-layout.md) | Changing **map shape or size** (rectangle extents, single-screen arena). |
| [player-hud.md](player-hud.md) | Changing **player HUD** slots, names, or on-screen resource / health display. |
| [lobby.md](lobby.md) | Changing **local player lobby**, join panels, or lobby → world start flow. |
| [scenarios.md](scenarios.md) | Changing **scenario** JSON, wave pacing, spawner counts, level transitions, or single-map sandbox session rules. |
| [waves.md](waves.md) | Changing **wave timing**, spawner placement, per-wave enemy volume, or the parked/disabled wave stance. |
| [game-over.md](game-over.md) | Changing **game over** detection, overlay, or continue navigation. |
| [local-input.md](local-input.md) | Changing **gamepad/keyboard** binding, per-player devices, disconnect/reconnect, or in-world input. |
