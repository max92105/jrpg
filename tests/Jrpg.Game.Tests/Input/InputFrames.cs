using Jrpg.Core.Exploration;
using Jrpg.Game.Input;

namespace Jrpg.Game.Tests.Input;

/// <summary>Input for tests that drive scenes and menus.</summary>
internal static class InputFrames
{
    /// <summary>The button goes down this update, with no movement.</summary>
    public static InputFrame Pressing(GameButton button) =>
        new(MoveIntent.None, ButtonSet.Empty.With(button), ButtonSet.Empty.With(button));

    /// <summary>Esc goes down: Cancel and Menu at once, as <see cref="InputBindings"/> maps it.</summary>
    public static InputFrame PressingEscape { get; } = new(
        MoveIntent.None,
        ButtonSet.Empty.With(GameButton.Cancel).With(GameButton.Menu),
        ButtonSet.Empty.With(GameButton.Cancel).With(GameButton.Menu));
}
