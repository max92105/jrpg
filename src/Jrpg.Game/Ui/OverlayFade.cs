namespace Jrpg.Game.Ui;

/// <summary>
/// A screen over the map (the Forge, the Shop) opening and closing over time, as the menu's frame (decision 370: it
/// fades itself in and out over the map, never through black). The opening is timed from the first drawn frame, so a
/// slow first frame can't use it up, the closing from the moment it starts.
/// </summary>
internal sealed class OverlayFade
{
    // The menu's placeholder timing (Menus/MenuMotion).
    private static readonly TimeSpan _openingFade = TimeSpan.FromSeconds(0.15);
    private static readonly TimeSpan _closingFade = TimeSpan.FromSeconds(0.12);

    private TimeSpan _sinceFirstDrawn;
    private bool _isDrawn;
    private TimeSpan? _sinceClosing;

    public bool IsClosing => _sinceClosing is not null;

    /// <summary>Whether the closing fade has run its whole length; false before it starts.</summary>
    public bool IsClosingDone => _sinceClosing >= _closingFade;

    /// <summary>The whole screen: 0 for clear to 1 for opaque.</summary>
    public float Opacity => Easing.ShareOf(_sinceFirstDrawn, _openingFade) * (1 - Easing.ShareOf(_sinceClosing ?? TimeSpan.Zero, _closingFade));

    /// <summary>Called on each drawn frame; the opening counts from the first.</summary>
    public void MarkDrawn() => _isDrawn = true;

    public void Advance(TimeSpan delta)
    {
        if (_isDrawn)
        {
            _sinceFirstDrawn += delta;
        }

        if (_sinceClosing is { } closing)
        {
            _sinceClosing = closing + delta;
        }
    }

    /// <summary>Starts the closing fade; a second call changes nothing.</summary>
    public void StartClosing() => _sinceClosing ??= TimeSpan.Zero;
}
