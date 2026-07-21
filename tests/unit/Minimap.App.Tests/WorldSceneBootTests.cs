using Minimap.Client.World;
using Xunit;

namespace Minimap.App.Tests;

public class WorldSceneBootTests
{
    [Fact]
    public void Starts_with_no_settings_session_views_or_tick()
    {
        var boot = new WorldSceneBoot();
        Assert.False(boot.SettingsLoaded);
        Assert.False(boot.SessionBound);
        Assert.False(boot.ViewsBound);
        Assert.False(boot.IsAborted);
        Assert.False(boot.TryTick());
    }

    [Fact]
    public void MarkSettingsLoaded_alone_does_not_enable_tick()
    {
        var boot = new WorldSceneBoot();
        boot.MarkSettingsLoaded();
        Assert.True(boot.SettingsLoaded);
        Assert.False(boot.TryTick());
    }

    [Fact]
    public void MarkSessionBound_requires_settings_first()
    {
        var boot = new WorldSceneBoot();
        var ex = Assert.Throws<InvalidOperationException>(boot.MarkSessionBound);
        Assert.Contains("settings", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False(boot.TryTick());
    }

    [Fact]
    public void MarkViewsBound_requires_session_first()
    {
        var boot = new WorldSceneBoot();
        boot.MarkSettingsLoaded();
        var ex = Assert.Throws<InvalidOperationException>(boot.MarkViewsBound);
        Assert.Contains("session", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False(boot.TryTick());
    }

    [Fact]
    public void Successful_order_enables_tick()
    {
        var boot = new WorldSceneBoot();
        boot.MarkSettingsLoaded();
        boot.MarkSessionBound();
        boot.MarkViewsBound();
        Assert.True(boot.ViewsBound);
        Assert.True(boot.TryTick());
    }

    [Fact]
    public void Abort_after_settings_disables_tick()
    {
        var boot = new WorldSceneBoot();
        boot.MarkSettingsLoaded();
        boot.Abort("Extension library missing");
        Assert.True(boot.IsAborted);
        Assert.Equal("Extension library missing", boot.AbortReason);
        Assert.False(boot.TryTick());
        Assert.True(boot.SettingsLoaded);
    }

    [Fact]
    public void Abort_before_settings_never_ticks()
    {
        var boot = new WorldSceneBoot();
        boot.Abort("scene wiring failed");
        Assert.False(boot.TryTick());
        Assert.Throws<InvalidOperationException>(boot.MarkSettingsLoaded);
    }

    [Fact]
    public void Abort_after_ready_revokes_tick()
    {
        var boot = new WorldSceneBoot();
        boot.MarkSettingsLoaded();
        boot.MarkSessionBound();
        boot.MarkViewsBound();
        Assert.True(boot.TryTick());
        boot.Abort("late failure");
        Assert.False(boot.TryTick());
    }

    [Fact]
    public void Simulates_settings_load_failure_like_WorldApp_Ready()
    {
        var boot = new WorldSceneBoot();
        try
        {
            throw new FileNotFoundException("Core settings file not found.");
        }
        catch (Exception ex)
        {
            boot.Abort(ex.Message);
        }

        Assert.True(boot.IsAborted);
        Assert.False(boot.TryTick());
        Assert.Throws<InvalidOperationException>(boot.MarkSettingsLoaded);
    }
}
