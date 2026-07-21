using Minimap.Automation.Contracts;
using Xunit;

namespace Minimap.Functional.Godot.Tests;

/// <summary>Runs in-Godot playbooks against the live headless process via a thin gRPC control plane.</summary>
[Collection("GodotAutomation")]
public class WorldRootSceneFunctionalTest(GodotAutomationFixture fixture)
{
    [Fact]
    public async Task World_scene_bootstraps_hex_and_player_layers()
    {
        var result = await fixture.Client.RunPlaybookAsync(new RunPlaybookRequest
        {
            PlaybookId = GodotAutomationFixture.WorldBootstrapId,
        });
        Assert.True(result.Ok, $"{result.Error} diagnostics={result.Diagnostics}");
    }

    [Fact]
    public async Task Hold_D_moves_player_visual_after_frames()
    {
        var result = await fixture.Client.RunPlaybookAsync(new RunPlaybookRequest
        {
            PlaybookId = GodotAutomationFixture.HoldDMovesPlayerId,
        });
        Assert.True(result.Ok, $"{result.Error} diagnostics={result.Diagnostics}");
    }
}

[Collection("GodotAutomation")]
public class LobbyFunctionalTest(GodotAutomationFixture fixture)
{
    [Fact]
    public async Task Lobby_main_scene_shows_four_available_panels()
    {
        var result = await fixture.Client.RunPlaybookAsync(new RunPlaybookRequest
        {
            PlaybookId = GodotAutomationFixture.LobbyBootstrapId,
        });
        Assert.True(result.Ok, $"{result.Error} diagnostics={result.Diagnostics}");
    }

    [Fact]
    public async Task Lobby_keyboard_claim_ready_starts_world()
    {
        var result = await fixture.Client.RunPlaybookAsync(new RunPlaybookRequest
        {
            PlaybookId = GodotAutomationFixture.LobbyClaimReadyStartId,
        });
        Assert.True(result.Ok, $"{result.Error} diagnostics={result.Diagnostics}");
    }

    [Fact]
    public async Task Lobby_back_unclaims_slot()
    {
        var result = await fixture.Client.RunPlaybookAsync(new RunPlaybookRequest
        {
            PlaybookId = GodotAutomationFixture.LobbyBackUnclaimsId,
        });
        Assert.True(result.Ok, $"{result.Error} diagnostics={result.Diagnostics}");
    }

    [Fact]
    public async Task Lobby_joypad_claim_ready_starts_world()
    {
        var result = await fixture.Client.RunPlaybookAsync(new RunPlaybookRequest
        {
            PlaybookId = GodotAutomationFixture.LobbyJoypadClaimReadyId,
        });
        Assert.True(result.Ok, $"{result.Error} diagnostics={result.Diagnostics}");
    }

    [Fact]
    public async Task Reconnect_overlay_allows_drop_player()
    {
        var result = await fixture.Client.RunPlaybookAsync(new RunPlaybookRequest
        {
            PlaybookId = GodotAutomationFixture.ReconnectOverlayDropId,
        });
        Assert.True(result.Ok, $"{result.Error} diagnostics={result.Diagnostics}");
    }
}
