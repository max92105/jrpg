using Jrpg.Game.Input;

namespace Jrpg.Game.Exploration;

/// <summary>
/// How far the hold to skip a cinematic has come (decision 623): it grows while Confirm stays held after a press, falls
/// back to nothing on release, and is done once held for <see cref="HoldTime"/>. A single press does nothing, and a
/// Confirm already held when the cinematic began never counts.
/// </summary>
internal sealed class SkipHold
{
    /// <summary>
    /// How long Confirm must stay held to skip. A named Game constant, as other presentation timings are: content holds
    /// game rules only, and has no place for UI timings (decision 624).
    /// </summary>
    public static readonly TimeSpan HoldTime = TimeSpan.FromSeconds(2);

    // Armed by a press made while the cinematic plays, so a hold carried over from the press that started the event
    // never skips it.
    private bool _isArmed;
    private TimeSpan _held;

    /// <summary>0 to 1: how much of <see cref="HoldTime"/> Confirm has been held.</summary>
    public float Progress => (float)(_held / HoldTime);

    public bool IsDone => _held >= HoldTime;

    /// <summary>The press only arms the hold; each update after it that Confirm stays held counts its time.</summary>
    public void Update(TimeSpan delta, InputFrame input)
    {
        if (!input.IsHeld(GameButton.Confirm))
        {
            _isArmed = false;
            _held = TimeSpan.Zero;
        }
        else if (input.IsPressed(GameButton.Confirm))
        {
            _isArmed = true;
        }
        else if (_isArmed)
        {
            _held = _held + delta < HoldTime ? _held + delta : HoldTime;
        }
    }
}
