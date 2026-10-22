using Jrpg.Core.Exploration;
using Microsoft.Xna.Framework.Input;

namespace Jrpg.Game.Input;

/// <summary>
/// Reads keyboard and controller into one <see cref="InputFrame"/> per update, remembering the previous update and
/// the device last used.
/// </summary>
internal sealed class InputReader
{
    private ButtonSet _previousKeys;
    private ButtonSet _previousPadButtons;
    private MoveIntent _previousStick;
    private InputDevice _device = InputDevice.Keyboard;

    /// <param name="keyboard">This update's keyboard.</param>
    /// <param name="pad">This update's controller, read without MonoGame's dead zone so <see cref="StickReader"/> applies its own.</param>
    public InputFrame Read(KeyboardState keyboard, GamePadState pad)
    {
        var keys = KeyButtons(keyboard);
        var padButtons = PadButtons(pad);
        var stick = StickReader.FromStick(pad.ThumbSticks.Left);
        var isStickNewlyPushed = stick != MoveIntent.None && _previousStick == MoveIntent.None;
        _device = DeviceInUse(keys.Except(_previousKeys), padButtons.Except(_previousPadButtons), isStickNewlyPushed);

        var held = keys.Union(padButtons);
        var pressed = held.Except(_previousKeys.Union(_previousPadButtons));
        _previousKeys = keys;
        _previousPadButtons = padButtons;
        _previousStick = stick;

        var move = stick == MoveIntent.None ? StickReader.FromDirectionButtons(held) : stick;
        return new InputFrame(move, held, pressed) { Device = _device };
    }

    // Only a change counts, like a key going down: a stick held, or drifting past the dead zone, mustn't take the
    // device back from a keyboard the player has just used.
    private InputDevice DeviceInUse(ButtonSet keysPressed, ButtonSet padButtonsPressed, bool isStickNewlyPushed)
    {
        if (!keysPressed.IsEmpty)
        {
            return InputDevice.Keyboard;
        }

        return padButtonsPressed.IsEmpty && !isStickNewlyPushed ? _device : InputDevice.Controller;
    }

    private static ButtonSet KeyButtons(KeyboardState keyboard)
    {
        var held = ButtonSet.Empty;
        foreach (var (key, button) in InputBindings.KeyBindings)
        {
            if (keyboard.IsKeyDown(key))
            {
                held = held.With(button);
            }
        }

        return held;
    }

    private static ButtonSet PadButtons(GamePadState pad)
    {
        var held = ButtonSet.Empty;
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
