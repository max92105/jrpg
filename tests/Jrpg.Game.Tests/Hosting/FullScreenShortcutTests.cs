using Jrpg.Game.Hosting;
using Microsoft.Xna.Framework.Input;

namespace Jrpg.Game.Tests.Hosting;

public sealed class FullScreenShortcutTests
{
    [Fact]
    public void IsPressed_AltHeldEnterGoesDown_IsTrue()
    {
        var pressed = FullScreenShortcut.IsPressed(new KeyboardState(Keys.LeftAlt), new KeyboardState(Keys.LeftAlt, Keys.Enter));

        Assert.True(pressed);
    }

    [Fact]
    public void IsPressed_RightAlt_IsTrue()
    {
        var pressed = FullScreenShortcut.IsPressed(new KeyboardState(), new KeyboardState(Keys.RightAlt, Keys.Enter));

        Assert.True(pressed);
    }

    [Fact]
    public void IsPressed_EnterWithoutAlt_IsFalse()
    {
        var pressed = FullScreenShortcut.IsPressed(new KeyboardState(), new KeyboardState(Keys.Enter));

        Assert.False(pressed);
    }

    [Fact]
    public void IsPressed_EnterHeldTwoUpdates_OnlyFirst()
    {
        var before = new KeyboardState(Keys.LeftAlt);
        var held = new KeyboardState(Keys.LeftAlt, Keys.Enter);

        var first = FullScreenShortcut.IsPressed(before, held);
        var second = FullScreenShortcut.IsPressed(held, held);

        Assert.True(first);
        Assert.False(second);
    }
}
