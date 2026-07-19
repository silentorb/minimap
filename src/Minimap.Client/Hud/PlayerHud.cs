using Godot;

namespace Minimap.Client;

/// <summary>Single player HUD slot: display name and health.</summary>
public partial class PlayerHud : Control
{
    private Label? _nameLabel;
    private Label? _healthLabel;

    public override void _Ready()
    {
        _nameLabel = GetNode<Label>("VBox/NameLabel");
        _healthLabel = GetNode<Label>("VBox/HealthLabel");
    }

    public void Apply(PlayerHudModel model)
    {
        if (_nameLabel is null || _healthLabel is null)
        {
            _nameLabel = GetNodeOrNull<Label>("VBox/NameLabel");
            _healthLabel = GetNodeOrNull<Label>("VBox/HealthLabel");
        }

        if (_nameLabel is not null)
            _nameLabel.Text = model.DisplayName;
        if (_healthLabel is not null)
            _healthLabel.Text = $"{FormatHp(model.Health)}/{FormatHp(model.MaxHealth)}";
    }

    private static string FormatHp(float value) =>
        Mathf.IsEqualApprox(value, Mathf.Round(value)) ? ((int)Mathf.Round(value)).ToString() : value.ToString("0.#");
}
