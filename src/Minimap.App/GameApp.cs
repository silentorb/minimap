using Godot;
using Minimap.Client;
using Minimap.Simulation;

namespace Minimap.App;

/// <summary>Godot entry: owns <see cref="GameSession"/>, drives WorldView + player HUD panel.</summary>
public partial class GameApp : Node2D
{
    [Export] public int GridRadiusX { get; set; } = 8;
    [Export] public int GridRadiusY { get; set; } = 6;
    [Export] public int WorldSeed { get; set; } = 42;
    [Export] public float HexSize { get; set; } = HexLayout.DefaultHexSize;
    [Export] public int PlayerFactionId { get; set; } = 1;
    [Export] public int RivalFactionId { get; set; } = 2;
    [Export] public int AiPerFaction { get; set; } = 3;
    [Export] public int LocalPlayerCount { get; set; } = 1;

    private GameSession? _session;
    private WorldView? _worldView;
    private PlayerHudPanel? _hudPanel;

    public WorldView? WorldView => _worldView;

    public override void _Ready()
    {
        var count = Math.Clamp(LocalPlayerCount, 1, 4);
        var spawn = new SpawnConfig
        {
            PlayerFactionId = PlayerFactionId,
            RivalFactionId = RivalFactionId,
            AiPerFaction = AiPerFaction,
            HumanPlayerCount = count,
        };

        _session = GameSession.Create(
            GridRadiusX,
            GridRadiusY,
            WorldSeed,
            HexSize,
            spawn,
            count);

        _worldView = GetNode<WorldView>("WorldView");
        _worldView.HexSize = HexSize;
        _worldView.Bind(_session.World, _session.Rng, _session.HumanPawns);

        _hudPanel = GetNode<PlayerHudPanel>("PlayerHudPanel");
        _hudPanel.Apply(_session.BuildHudModels());
    }

    public override void _Process(double delta)
    {
        if (_session is null || _worldView is null || _hudPanel is null)
            return;

        _session.SetMoveInput(0, _worldView.ReadMoveInput());
        for (var i = 1; i < _session.Players.Count; i++)
            _session.SetMoveInput(i, SimVec2.Zero);

        _session.Tick((float)delta);
        _worldView.SyncFrame();
        _hudPanel.Apply(_session.BuildHudModels());
    }
}
