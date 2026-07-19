using Godot;

namespace Minimap.Client;

/// <summary>Bottom panel hosting 1–4 <see cref="PlayerHud"/> slots.</summary>
public partial class PlayerHudPanel : CanvasLayer
{
    private HBoxContainer? _slots;
    private PackedScene? _hudScene;
    private readonly List<PlayerHud> _huds = new();

    public override void _Ready()
    {
        _slots = GetNode<HBoxContainer>("Panel/Margin/Slots");
        _hudScene = GD.Load<PackedScene>("res://ui/player_hud.tscn");
    }

    public void SetSlotCount(int count)
    {
        count = Math.Clamp(count, 1, 4);
        EnsureReady();
        if (_slots is null || _hudScene is null)
            return;

        while (_huds.Count < count)
        {
            var hud = _hudScene.Instantiate<PlayerHud>();
            _slots.AddChild(hud);
            _huds.Add(hud);
        }

        while (_huds.Count > count)
        {
            var last = _huds[^1];
            _huds.RemoveAt(_huds.Count - 1);
            last.QueueFree();
        }
    }

    public void Apply(IReadOnlyList<PlayerHudModel> models)
    {
        EnsureReady();
        SetSlotCount(models.Count);
        for (var i = 0; i < models.Count && i < _huds.Count; i++)
            _huds[i].Apply(models[i]);
    }

    private void EnsureReady()
    {
        if (_slots is not null && _hudScene is not null)
            return;
        _slots ??= GetNodeOrNull<HBoxContainer>("Panel/Margin/Slots");
        _hudScene ??= GD.Load<PackedScene>("res://ui/player_hud.tscn");
    }
}
