using Minimap.Simulation;
using Minimap.Simulation.Types;

namespace Minimap.Client.World;

/// <summary>
/// Local client adapter over a Simulation <see cref="GameSession"/>:
/// PlayerControllers, input feed, and HUD model mapping.
/// </summary>
public sealed class ClientSession
{
    private readonly GameSession _session;
    private readonly IReadOnlyList<DomainDefinition> _domains;
    private readonly List<PlayerController> _players = new();

    public ClientSession(
        GameSession session,
        IReadOnlyList<DomainDefinition>? domains = null)
    {
        ArgumentNullException.ThrowIfNull(session);
        _session = session;
        _domains = domains ?? Array.Empty<DomainDefinition>();
        AttachHumanPlayers();
    }

    public GameSession Simulation => _session;
    public IReadOnlyList<PlayerController> Players => _players;

    public void SetMoveInput(int playerIndex, SimVec2 direction)
    {
        if (playerIndex < 0 || playerIndex >= _players.Count)
            return;
        _players[playerIndex].SetMoveInput(direction);
    }

    public void SetAimInput(int playerIndex, SimVec2 direction)
    {
        if (playerIndex < 0 || playerIndex >= _players.Count)
            return;
        _players[playerIndex].SetAimInput(direction);
    }

    public void SetFireHeld(int playerIndex, bool held)
    {
        if (playerIndex < 0 || playerIndex >= _players.Count)
            return;
        _players[playerIndex].SetFireHeld(held);
    }

    public void SetAbilityActivatePressed(int playerIndex, bool pressed)
    {
        if (playerIndex < 0 || playerIndex >= _players.Count)
            return;
        _players[playerIndex].SetAbilityActivatePressed(pressed);
    }

    public void SetAbilityBackPressed(int playerIndex, bool pressed)
    {
        if (playerIndex < 0 || playerIndex >= _players.Count)
            return;
        _players[playerIndex].SetAbilityBackPressed(pressed);
    }

    public void SetInteractPressed(int playerIndex, bool pressed)
    {
        if (playerIndex < 0 || playerIndex >= _players.Count)
            return;
        _players[playerIndex].SetInteractPressed(pressed);
    }

    public void SetModalSelect(int playerIndex, int? slotIndex)
    {
        if (playerIndex < 0 || playerIndex >= _players.Count)
            return;
        _players[playerIndex].SetModalSelect(slotIndex);
    }

    public void OnLevelRegenerated() => ReattachHumanPlayers();

    public IReadOnlyList<PlayerHudModel> BuildHudModels()
    {
        var models = new List<PlayerHudModel>(_players.Count);
        for (var i = 0; i < _players.Count; i++)
        {
            var pawn = _players[i].Pawn;
            models.Add(new PlayerHudModel
            {
                DisplayName = $"Player {i + 1}",
                Resources = BuildResourceModels(pawn, _session.Content),
                SelectedAbility = BuildSelectedAbilityModel(pawn, _domains),
            });
        }

        return models;
    }

    /// <summary>Unpossess, drop from simulation, and remove local controller.</summary>
    public bool DropHumanPlayer(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= _players.Count)
            return false;

        var controller = _players[playerIndex];
        controller.Unpossess();
        _players.RemoveAt(playerIndex);
        return _session.DropHumanPlayer(playerIndex);
    }

    private static IReadOnlyList<PlayerHudResourceModel> BuildResourceModels(
        Character? pawn,
        GameContent content)
    {
        var rows = new List<PlayerHudResourceModel>();
        foreach (var definition in content.Resources
                     .Where(r => r.Visible)
                     .OrderByDescending(r => r.UiPriority)
                     .ThenBy(r => r.Id, StringComparer.Ordinal))
        {
            var amount = 0;
            int? maxAmount = null;
            if (pawn is not null)
            {
                amount = pawn.GetResource(definition.Tag);
                if (definition.LimitTag is { } limitTag)
                    maxAmount = pawn.GetResource(limitTag);
            }
            else if (definition.LimitTag is { } limitTag &&
                     content.TryGetResource(limitTag, out var limitDef) &&
                     limitDef is not null)
            {
                // Dead / missing pawn: still show health/energy as 0 / default max when possible.
                if (definition.Tag == content.HealthTag)
                {
                    amount = 0;
                    maxAmount = CombatTuning.DefaultMaxHealth;
                }
                else if (definition.Tag == content.EnergyTag)
                {
                    amount = 0;
                    maxAmount = CombatTuning.DefaultMaxEnergy;
                }
            }

            var isHealth = definition.Tag == content.HealthTag;
            var isEnergy = definition.Tag == content.EnergyTag;
            if (!isHealth && !isEnergy && amount <= 0 && maxAmount is null)
                continue;

            if (isHealth && pawn is { IsAlive: false })
                amount = 0;

            rows.Add(new PlayerHudResourceModel
            {
                Id = definition.Id,
                DisplayName = definition.DisplayName ?? definition.Id,
                IconPath = definition.IconConfig?.ResourcePath,
                Amount = amount,
                MaxAmount = maxAmount,
            });
        }

        return rows;
    }

    private static PlayerHudAbilityModel? BuildSelectedAbilityModel(
        Character? pawn,
        IReadOnlyList<DomainDefinition> domains)
    {
        var selected = pawn?.AbilityLoadout.SelectedModal;
        if (selected is null)
            return null;

        var definition = selected.Definition;
        return new PlayerHudAbilityModel
        {
            Id = definition.Id,
            DisplayName = definition.DisplayName ?? definition.Id,
            IconPath = definition.IconConfig?.ResourcePath,
            DomainColors = DomainColorResolver.Resolve(definition, domains),
        };
    }

    private void AttachHumanPlayers()
    {
        foreach (var player in _session.Players)
        {
            var controller = new PlayerController(player, _session.Rng);
            if (player.Character is not null)
                _session.World.AttachController(controller, player.Character);
            _players.Add(controller);
        }
    }

    private void ReattachHumanPlayers()
    {
        for (var i = 0; i < _players.Count && i < _session.Players.Count; i++)
        {
            var controller = _players[i];
            var character = _session.Players[i].Character;
            if (character is null)
                continue;

            if (controller.Pawn is not null)
                controller.Unpossess();

            _session.World.AttachController(controller, character);
        }
    }
}
