using Godot;
using Minimap.Client.Achievements;
using Minimap.Client.LocalPlay;

namespace Minimap.Client.PostSession;

/// <summary>Full-screen post-session summary: per-player session achievements + ready gate.</summary>
public partial class PostSessionOverlay : CanvasLayer
{
    private Control? _playerRow;
    private Label? _titleLabel;
    private PostSessionReadyModel? _readyModel;
    private LocalPlayRoster? _roster;
    private SessionAchievementLedger? _ledger;
    private readonly List<Label> _readyLabels = new();

    public event Action? AllReadyRequested;

    public bool OverlayVisible => Visible;
    public PostSessionReadyModel? ReadyModel => _readyModel;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        Visible = false;
        _titleLabel = GetNode<Label>("Root/Margin/VBox/Title");
        _playerRow = GetNode<Control>("Root/Margin/VBox/PlayerRow");
    }

    public void ShowSummary(
        LocalPlayRoster roster,
        SessionAchievementLedger ledger,
        string title = "Session complete")
    {
        ArgumentNullException.ThrowIfNull(roster);
        ArgumentNullException.ThrowIfNull(ledger);

        _roster = roster;
        _ledger = ledger;
        var count = Math.Max(1, roster.PlayerCount);
        _readyModel = new PostSessionReadyModel(count);
        if (_titleLabel is not null)
            _titleLabel.Text = title;

        RebuildPlayerPanels();
        Visible = true;
    }

    public void HideOverlay()
    {
        Visible = false;
        _readyModel = null;
        _roster = null;
        _ledger = null;
        ClearPlayerPanels();
    }

    public bool TryHandleReadyInput(InputDeviceId device)
    {
        if (!Visible || _readyModel is null || _roster is null)
            return false;

        for (var i = 0; i < _roster.PlayerCount; i++)
        {
            if (!_roster.Players[i].HasDevice(device))
                continue;
            if (!_readyModel.TryToggleReady(i))
                return false;
            RefreshReadyLabels();
            if (_readyModel.AllReady)
                AllReadyRequested?.Invoke();
            return true;
        }

        return false;
    }

    private void RebuildPlayerPanels()
    {
        ClearPlayerPanels();
        if (_playerRow is null || _roster is null || _ledger is null || _readyModel is null)
            return;

        _ledger.EnsurePlayerCount(_roster.PlayerCount);
        for (var i = 0; i < _roster.PlayerCount; i++)
        {
            var panel = new VBoxContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            };
            panel.AddThemeConstantOverride("separation", 8);

            var name = _roster.Players[i].DisplayName ?? $"Player {i + 1}";
            panel.AddChild(new Label
            {
                Text = name,
                HorizontalAlignment = HorizontalAlignment.Center,
            });

            panel.AddChild(new Label { Text = "Achievements this session:" });

            var earns = _ledger.GetEarns(i);
            if (earns.Count == 0)
            {
                panel.AddChild(new Label { Text = "(none)" });
            }
            else
            {
                foreach (var earn in earns)
                {
                    var def = AchievementCatalog.Find(earn.AchievementId);
                    var title = def?.Title ?? earn.AchievementId;
                    var suffix = earn.FirstTime ? "  (first time!)" : "  (earned again)";
                    panel.AddChild(new Label { Text = title + suffix });
                }
            }

            var readyLabel = new Label
            {
                Text = "Press Enter / Start / A to Ready",
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            _readyLabels.Add(readyLabel);
            panel.AddChild(readyLabel);
            _playerRow.AddChild(panel);
        }

        RefreshReadyLabels();
    }

    private void RefreshReadyLabels()
    {
        if (_readyModel is null)
            return;
        for (var i = 0; i < _readyLabels.Count && i < _readyModel.PlayerCount; i++)
        {
            _readyLabels[i].Text = _readyModel.IsReady(i)
                ? "Ready"
                : "Press Enter / Start / A to Ready";
        }
    }

    private void ClearPlayerPanels()
    {
        _readyLabels.Clear();
        if (_playerRow is null)
            return;
        foreach (var child in _playerRow.GetChildren())
            child.QueueFree();
    }
}
