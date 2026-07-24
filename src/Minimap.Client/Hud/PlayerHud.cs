using Godot;

namespace Minimap.Client;

/// <summary>Single player HUD slot: display name and visible resources.</summary>
public partial class PlayerHud : Control
{
    public const int MaxVisibleResources = 4;

    private Label? _nameLabel;
    private HBoxContainer? _resourcesRow;

    public override void _Ready()
    {
        _nameLabel = GetNode<Label>("VBox/NameLabel");
        _resourcesRow = GetNode<HBoxContainer>("VBox/ResourcesRow");
    }

    public void Apply(PlayerHudModel model)
    {
        if (_nameLabel is null || _resourcesRow is null)
        {
            _nameLabel = GetNodeOrNull<Label>("VBox/NameLabel");
            _resourcesRow = GetNodeOrNull<HBoxContainer>("VBox/ResourcesRow");
        }

        if (_nameLabel is not null)
            _nameLabel.Text = model.DisplayName;

        if (_resourcesRow is null)
            return;

        foreach (var child in _resourcesRow.GetChildren())
            child.QueueFree();

        var resources = model.Resources;
        var visibleCount = Math.Min(resources.Count, MaxVisibleResources);
        for (var i = 0; i < visibleCount; i++)
            _resourcesRow.AddChild(CreateResourceEntry(resources[i]));

        var overflow = resources.Count - visibleCount;
        if (overflow > 0)
        {
            var overflowLabel = new Label
            {
                Text = $"+{overflow}",
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            _resourcesRow.AddChild(overflowLabel);
        }
    }

    private static Control CreateResourceEntry(PlayerHudResourceModel resource)
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 2);

        var icon = TryLoadIcon(resource.IconPath);
        if (icon is not null)
        {
            row.AddChild(new TextureRect
            {
                Texture = icon,
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                CustomMinimumSize = new Vector2(16, 16),
            });
        }

        var amountText = resource.MaxAmount is { } max
            ? $"{resource.Amount}/{max}"
            : resource.Amount.ToString();
        row.AddChild(new Label
        {
            Text = amountText,
            TooltipText = resource.DisplayName,
        });
        return row;
    }

    private static Texture2D? TryLoadIcon(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;
        if (!ResourceLoader.Exists(path))
            return null;
        return ResourceLoader.Load<Texture2D>(path);
    }
}
