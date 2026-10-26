using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>
/// The game menu's opening and closing over time (decision 262): the bars and the picture fade in while the tabs slide
/// in from the left one after another; closing fades everything out over the map. The opening is timed from the first
/// drawn frame, so a slow first frame can't use it up (BG1), the closing from the moment it starts.
/// </summary>
internal sealed class MenuMotion
{
    /// <summary>How far left of its place a tab starts, in pixels (placeholder).</summary>
    public const float TabSlideDistance = 32;

    // Placeholder timing (plan 2026-10-25-ui-second-pass, design.md).
    private static readonly TimeSpan _frameFade = TimeSpan.FromSeconds(0.15);
    private static readonly TimeSpan _tabSlide = TimeSpan.FromSeconds(0.2);
    private static readonly TimeSpan _tabDelay = TimeSpan.FromSeconds(0.03);
    private static readonly TimeSpan _closingFade = TimeSpan.FromSeconds(0.12);

    private TimeSpan _sinceFirstDrawn;
    private bool _isDrawn;
    private TimeSpan? _sinceClosing;

    public bool IsClosing => _sinceClosing is not null;

    /// <summary>Whether the closing fade has run its whole length; false before it starts.</summary>
    public bool IsClosingDone => _sinceClosing >= _closingFade;

    /// <summary>The bars, the picture, the page and the footer: 0 for clear to 1 for opaque.</summary>
    public float FrameOpacity => Easing.ShareOf(_sinceFirstDrawn, _frameFade) * ClosingOpacity;

    private float ClosingOpacity => 1 - Easing.ShareOf(_sinceClosing ?? TimeSpan.Zero, _closingFade);

    /// <summary>How far left of its place the tab at <paramref name="index"/> is drawn, in pixels.</summary>
    public float TabOffset(int index) => TabSlideDistance * (1 - TabShare(index));

    /// <summary>0 for clear to 1 for opaque: a tab fades in as it slides, and out with the rest.</summary>
    public float TabOpacity(int index) => TabShare(index) * ClosingOpacity;

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

    // Each tab starts its slide a little after the one above it, and settles gently.
    private float TabShare(int index) => Easing.CubicOut(Easing.ShareOf(_sinceFirstDrawn - (index * _tabDelay), _tabSlide));
}
