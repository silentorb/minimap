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
            new[] { MainMenuAction.Continue, MainMenuAction.EndGame, MainMenuAction.Quit },
            MainMenuModel.PopupOptions(activeGame: true));
    }

    [Fact]
    public void PopupOptions_WithoutActiveGame_OmitContinue()
    {
        Assert.Equal(
            new[] { MainMenuAction.EndGame, MainMenuAction.Quit },
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

public class MainMenuPopupControllerTests
{
    [Fact]
    public void JoypadOwner_CanNavigateAndActivateEndGame()
    {
        var controller = new MainMenuPopupController();
        var owner = InputDeviceId.Joypad(0);
        controller.Open(owner);

        Assert.Equal(MainMenuAction.Continue, controller.SelectedAction);
        Assert.True(controller.TryHandle(owner, navigateDelta: 1, activateSelected: false, dismiss: false, out var activated));
        Assert.Null(activated);
        Assert.Equal(MainMenuAction.EndGame, controller.SelectedAction);

        Assert.True(controller.TryHandle(owner, navigateDelta: 0, activateSelected: true, dismiss: false, out activated));
        Assert.Equal(MainMenuAction.EndGame, activated);
    }

    [Fact]
    public void NonOwner_InputIsIgnored()
    {
        var controller = new MainMenuPopupController();
        controller.Open(InputDeviceId.Joypad(0));

        Assert.False(
            controller.TryHandle(
                InputDeviceId.Keyboard,
                navigateDelta: 1,
                activateSelected: false,
                dismiss: false,
                out var activated));
        Assert.Null(activated);
        Assert.Equal(MainMenuAction.Continue, controller.SelectedAction);
    }

    [Fact]
    public void Dismiss_AlwaysActivatesContinue()
    {
        var controller = new MainMenuPopupController();
        var owner = InputDeviceId.Joypad(0);
        controller.Open(owner);
        controller.TryHandle(owner, navigateDelta: 1, activateSelected: false, dismiss: false, out _);

        Assert.True(controller.TryHandle(owner, navigateDelta: 0, activateSelected: false, dismiss: true, out var activated));
        Assert.Equal(MainMenuAction.Continue, activated);
    }

    [Fact]
    public void Navigate_WrapsAroundOptions()
    {
        var controller = new MainMenuPopupController();
        var owner = InputDeviceId.Keyboard;
        controller.Open(owner);

        Assert.True(controller.TryHandle(owner, navigateDelta: -1, activateSelected: false, dismiss: false, out _));
        Assert.Equal(MainMenuAction.Quit, controller.SelectedAction);
    }
}
