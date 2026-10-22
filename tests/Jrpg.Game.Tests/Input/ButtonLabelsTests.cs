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

    [Fact]
    public void Of_TabButtonOnKeyboard_ThrowsNamingIt()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => ButtonLabels.Of(GameButton.PreviousTab, InputDevice.Keyboard));

        Assert.Contains("PreviousTab", exception.Message);
        Assert.Contains("Keyboard", exception.Message);
    }
}
