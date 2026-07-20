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
    public async Task Arrow_right_moves_player_visual_after_frames()
    {
        var result = await fixture.Client.RunPlaybookAsync(new RunPlaybookRequest
        {
            PlaybookId = GodotAutomationFixture.ArrowRightMovesPlayerId,
        });
        Assert.True(result.Ok, $"{result.Error} diagnostics={result.Diagnostics}");
    }
}
