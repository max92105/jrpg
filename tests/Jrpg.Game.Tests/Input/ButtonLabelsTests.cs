using Jrpg.Game.Input;

namespace Jrpg.Game.Tests.Input;

public sealed class ButtonLabelsTests
{
    [Fact]
    public void Of_ConfirmOnKeyboard_IsItsFirstKey()
    {
        var label = ButtonLabels.Of(GameButton.Confirm, InputDevice.Keyboard);

        Assert.Equal("E", label);
    }

    [Fact]
    public void Of_ConfirmOnController_IsA()
    {
        var label = ButtonLabels.Of(GameButton.Confirm, InputDevice.Controller);

        Assert.Equal("A", label);
    }

    // The tab buttons gained their keys with the Shop's categories (decision 384).
    [Fact]
    public void Of_PreviousTabOnKeyboard_IsQ()
    {
        Assert.Equal("Q", ButtonLabels.Of(GameButton.PreviousTab, InputDevice.Keyboard));
    }

    [Fact]
    public void Of_NextTabOnKeyboard_IsR()
    {
        Assert.Equal("R", ButtonLabels.Of(GameButton.NextTab, InputDevice.Keyboard));
    }
}
