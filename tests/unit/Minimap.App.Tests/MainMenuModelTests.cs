using Minimap.Client.LocalPlay;
using Minimap.Client.MainMenu;
using Xunit;

namespace Minimap.App.Tests;

public class MainMenuModelTests
{
    [Fact]
    public void ScreenOptions_AreNewAndQuit()
    {
        Assert.Equal(
            new[] { MainMenuAction.New, MainMenuAction.Quit },
            MainMenuModel.ScreenOptions);
    }

    [Fact]
    public void PopupOptions_WithActiveGame_IncludeContinueFirst()
    {
        Assert.Equal(
            new[] { MainMenuAction.Continue, MainMenuAction.New, MainMenuAction.Quit },
            MainMenuModel.PopupOptions(activeGame: true));
    }

    [Fact]
    public void PopupOptions_WithoutActiveGame_OmitContinue()
    {
        Assert.Equal(
            new[] { MainMenuAction.New, MainMenuAction.Quit },
            MainMenuModel.PopupOptions(activeGame: false));
    }
}

public class MainMenuOwnershipTests
{
    [Fact]
    public void Open_AcceptsOnlyOwner_UntilClosed()
    {
        var ownership = new MainMenuOwnership();
        var owner = InputDeviceId.Keyboard;
        var other = InputDeviceId.Joypad(0);

        Assert.False(ownership.IsOpen);
        ownership.Open(owner);
        Assert.True(ownership.IsOpen);
        Assert.True(ownership.Accepts(owner));
        Assert.False(ownership.Accepts(other));

        ownership.Close();
        Assert.False(ownership.IsOpen);
        Assert.False(ownership.Accepts(owner));
    }
}
