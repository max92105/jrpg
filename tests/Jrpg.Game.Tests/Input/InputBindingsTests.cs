using Jrpg.Game.Input;
using Microsoft.Xna.Framework.Input;

namespace Jrpg.Game.Tests.Input;

// Q and R are the keyboard's tab buttons (decision 384), the shoulders' twins.
public sealed class InputBindingsTests
{
    [Fact]
    public void KeyBindings_Q_IsPreviousTab()
    {
        Assert.Contains((Keys.Q, GameButton.PreviousTab), InputBindings.KeyBindings);
    }

    [Fact]
    public void KeyBindings_R_IsNextTab()
    {
        Assert.Contains((Keys.R, GameButton.NextTab), InputBindings.KeyBindings);
    }
}
