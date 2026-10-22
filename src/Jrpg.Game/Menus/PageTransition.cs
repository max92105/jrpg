using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>
/// A menu page entering: it slides in from the right while it fades in, eased so it lands softly. Input isn't held back
/// meanwhile.
/// </summary>
internal sealed class PageTransition
{
    /// <summary>How far right of its place the page starts, in pixels (the demo's value; placeholder).</summary>
    public const int SlideDistance = 24;

    /// <summary>The demo's 220 ms (placeholder).</summary>
    public static readonly TimeSpan Duration = TimeSpan.FromMilliseconds(220);

    // Starts finished, so a page that never opened draws in place.
    private TimeSpan _elapsed = Duration;

    /// <summary>Pixels right of the page's place: <see cref="SlideDistance"/> at the start, 0 at the end.</summary>
    public int Offset => (int)Math.Round(SlideDistance * (1 - Eased));

    /// <summary>0 for transparent to 1 for opaque.</summary>
    public float Opacity => Eased;

    private float Eased => Easing.CubicOut(Easing.ShareOf(_elapsed, Duration));

    public void Start() => _elapsed = TimeSpan.Zero;

    public void Update(TimeSpan delta) => _elapsed += delta;
}
