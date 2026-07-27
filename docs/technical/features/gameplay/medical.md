# Medical (technical)

Medkits resource and Heal effect. Implements [medical.md](../../../game/features/gameplay/medical.md). Related: [resources.md](resources.md), [accessories.md](accessories.md), [active-abilities.md](active-abilities.md), [interaction.md](interaction.md), [domains.md](domains.md), [tags.md](tags.md), [actors.md](actors.md), [definition-config.md](../platform/definition-config.md).

## Requirements

- Domain JSON `medical` under CompuQuest `config/domains/`; resource JSON `medkits` under `config/resources/`.
- Accessory **`heal`**: modal; `player_selectable` + `medical`; `enabledWhen` `{ "id": "medkits", "atLeast": 1 }`; acquire grant `modify_resource` medkits **3**; effect type **`heal`** with use cost **1** medkit.
- CompuQuest **`HealEffect`**: implements **`IInstantUseEffect`**, **`IInteractionEffect`**, and **`IEffectUseCost`**.
  - Instant `TryUse`: gate injured + definition has `human` or `animal` + afford → consume → set `Health = MaxHealth`.
  - Interact `CanInteract` / `TryInteract`: same gates on **target**; cost consumed from the **caster**.
- Actor classify tags **`human`** / **`animal`** on `ActorDefinition` (see [tags.md](tags.md)); Heal reads `target.Definition.HasTag`.
- Front-cell resolve includes free actors (see [interaction.md](interaction.md)) so companions and other pawns can be healed.

## Non-goals (for now)

- Partial heal effect variants
- Server-side medical simulation distinct from accessory effects
