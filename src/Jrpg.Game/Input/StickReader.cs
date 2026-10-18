using Jrpg.Core.Exploration;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Input;

/// <summary>Turns a thumbstick or the direction buttons into one <see cref="MoveIntent"/>, so both move the same way.</summary>
internal static class StickReader
{
    /// <summary>Below this tilt the stick counts as released, so a worn stick doesn't drift (placeholder, part 2).</summary>
    public const float Deadzone = 0.2f;

    /// <param name="stick">MonoGame's thumbstick, whose Y points up; the map's Y points down.</param>
    public static MoveIntent FromStick(Vector2 stick) =>
        stick.Length() < Deadzone ? MoveIntent.None : MoveIntent.FromAxes(stick.X, -stick.Y);

    /// <summary>Opposite directions cancel out; a diagonal moves at full speed, not faster.</summary>
    public static MoveIntent FromDirectionButtons(ButtonSet held) =>
        MoveIntent.FromAxes(
            AxisOf(held, GameButton.Left, GameButton.Right),
            AxisOf(held, GameButton.Up, GameButton.Down));

    private static int AxisOf(ButtonSet held, GameButton negative, GameButton positive) =>
        (held.Contains(positive) ? 1 : 0) - (held.Contains(negative) ? 1 : 0);
}
