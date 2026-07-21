using Minimap.Client.Lobby;
using Xunit;

namespace Minimap.App.Tests;

public class LobbySceneBootTests
{
    [Fact]
    public void Starts_with_no_panels_extensions_or_input()
    {
        var boot = new LobbySceneBoot();
        Assert.False(boot.PanelsBound);
        Assert.False(boot.ExtensionsLoaded);
        Assert.False(boot.AcceptsInput);
        Assert.False(boot.IsAborted);
        Assert.False(boot.TryAcceptInput());
        Assert.False(boot.CanRefreshPanels());
    }

    [Fact]
    public void MarkPanelsBound_allows_refresh_but_not_input()
    {
        var boot = new LobbySceneBoot();
        boot.MarkPanelsBound();
        Assert.True(boot.PanelsBound);
        Assert.True(boot.CanRefreshPanels());
        Assert.False(boot.TryAcceptInput());
    }

    [Fact]
    public void MarkExtensionsLoaded_requires_panels_first()
    {
        var boot = new LobbySceneBoot();
        var ex = Assert.Throws<InvalidOperationException>(boot.MarkExtensionsLoaded);
        Assert.Contains("panels", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False(boot.AcceptsInput);
    }

    [Fact]
    public void Successful_order_enables_input()
    {
        var boot = new LobbySceneBoot();
        boot.MarkPanelsBound();
        boot.MarkExtensionsLoaded();
        Assert.True(boot.ExtensionsLoaded);
        Assert.True(boot.AcceptsInput);
        Assert.True(boot.TryAcceptInput());
        Assert.True(boot.CanRefreshPanels());
    }

    [Fact]
    public void Abort_after_panels_disables_input_and_refresh()
    {
        var boot = new LobbySceneBoot();
        boot.MarkPanelsBound();
        boot.Abort("Extension library missing");
        Assert.True(boot.IsAborted);
        Assert.Equal("Extension library missing", boot.AbortReason);
        Assert.False(boot.TryAcceptInput());
        Assert.False(boot.CanRefreshPanels());
        Assert.True(boot.PanelsBound);
    }

    [Fact]
    public void Abort_before_panels_never_accepts_input()
    {
        var boot = new LobbySceneBoot();
        boot.Abort("scene wiring failed");
        Assert.False(boot.TryAcceptInput());
        Assert.False(boot.CanRefreshPanels());
        Assert.Throws<InvalidOperationException>(boot.MarkPanelsBound);
    }

    [Fact]
    public void Abort_after_ready_revokes_input()
    {
        var boot = new LobbySceneBoot();
        boot.MarkPanelsBound();
        boot.MarkExtensionsLoaded();
        Assert.True(boot.TryAcceptInput());
        boot.Abort("late failure");
        Assert.False(boot.TryAcceptInput());
        Assert.False(boot.CanRefreshPanels());
    }

    [Fact]
    public void Simulates_extension_preflight_failure_like_LobbyApp_Ready()
    {
        var boot = new LobbySceneBoot();
        boot.MarkPanelsBound();
        try
        {
            throw new FileNotFoundException("Extension library 'CompuQuest.Minimap.dll' not found.");
        }
        catch (Exception ex)
        {
            boot.Abort(ex.Message);
        }

        Assert.True(boot.IsAborted);
        Assert.False(boot.TryAcceptInput());
        Assert.False(boot.CanRefreshPanels());
        Assert.Throws<InvalidOperationException>(boot.MarkExtensionsLoaded);
    }
}
