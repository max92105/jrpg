using Jrpg.Core.Exploration;
using Microsoft.Xna.Framework.Input;

namespace Jrpg.Game.Input;

/// <summary>Reads keyboard and controller into one <see cref="InputFrame"/> per update, remembering the previous update.</summary>
internal sealed class InputReader
{
    private ButtonSet _previouslyHeld;

    /// <param name="keyboard">This update's keyboard.</param>
    /// <param name="pad">This update's controller, read without MonoGame's dead zone so <see cref="StickReader"/> applies its own.</param>
    public InputFrame Read(KeyboardState keyboard, GamePadState pad)
    {
        var held = HeldButtons(keyboard, pad);
        var pressed = held.Except(_previouslyHeld);
        _previouslyHeld = held;

        var stick = StickReader.FromStick(pad.ThumbSticks.Left);
        var move = stick == MoveIntent.None ? StickReader.FromDirectionButtons(held) : stick;
        return new InputFrame(move, held, pressed);
    }

    private static ButtonSet HeldButtons(KeyboardState keyboard, GamePadState pad)
    {
        var held = ButtonSet.Empty;
        foreach (var (key, button) in InputBindings.KeyBindings)
        {
            if (keyboard.IsKeyDown(key))
            {
                held = held.With(button);
            }
        }

        foreach (var (padButton, button) in InputBindings.PadBindings)
        {
            if (pad.IsButtonDown(padButton))
            {
                held = held.With(button);
            }
        }

        return held;
    }
}
