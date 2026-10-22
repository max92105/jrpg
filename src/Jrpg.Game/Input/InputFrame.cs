using Jrpg.Core.Exploration;

namespace Jrpg.Game.Input;

/// <summary>The player's input for one update, in game terms.</summary>
internal readonly record struct InputFrame
{
    private readonly ButtonSet _held;
    private readonly ButtonSet _pressed;

    /// <param name="move">Where the player pushes: the stick, else the direction buttons.</param>
    /// <param name="held">Buttons down this update.</param>
    /// <param name="pressed">Buttons down this update that weren't on the previous one.</param>
    public InputFrame(MoveIntent move, ButtonSet held, ButtonSet pressed)
    {
        Move = move;
        _held = held;
        _pressed = pressed;
    }

    public MoveIntent Move { get; }

    /// <summary>The device the player last used; the keyboard until a controller is touched.</summary>
    public InputDevice Device { get; init; }

    /// <summary>True only on the update the button goes down, so holding Confirm doesn't repeat an action.</summary>
    public bool IsPressed(GameButton button) => _pressed.Contains(button);

    public bool IsHeld(GameButton button) => _held.Contains(button);
}
