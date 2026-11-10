using Jrpg.Core.Maps;

namespace Jrpg.Game.Exploration;

/// <summary>
/// Which map's name and level the entry banner shows, and how opaque it is now (decisions 528, 529): entering a map
/// other than the one entered last starts it when that map shows the banner; it waits, fades in, holds and fades out.
/// Its clock is the scene's, moved only while no event runs, so a warp's fade from black and an arrival cutscene are
/// over before it shows.
/// </summary>
internal sealed class MapBanner
{
    /// <summary>A beat after the field is free, before the fade in (placeholder).</summary>
    public static readonly TimeSpan WaitTime = TimeSpan.FromSeconds(0.5);

    /// <summary>From transparent to opaque (placeholder).</summary>
    public static readonly TimeSpan FadeInTime = TimeSpan.FromSeconds(0.4);

    /// <summary>Fully shown (placeholder).</summary>
    public static readonly TimeSpan HoldTime = TimeSpan.FromSeconds(3);

    /// <summary>From opaque to transparent (placeholder).</summary>
    public static readonly TimeSpan FadeOutTime = TimeSpan.FromSeconds(0.8);

    private static readonly TimeSpan _wholeTime = WaitTime + FadeInTime + HoldTime + FadeOutTime;

    private string? _lastEnteredId;
    private TimeSpan _clock;

    /// <summary>
    /// The map the banner shows, from its wait to the end of its fade out; null while it shows nothing.
    /// </summary>
    public MapDefinition? Shown { get; private set; }

    /// <summary>0 for transparent to 1 for opaque, linear in and out.</summary>
    public float Opacity => Shown is null ? 0 : OpacityAt(_clock);

    /// <summary>
    /// Restarts the banner on a map other than the one entered last, or stops it when that map doesn't show the banner;
    /// the same map again (a warp within it) changes nothing. The first map always counts as another.
    /// </summary>
    public void Enter(MapDefinition map)
    {
        if (map.Id == _lastEnteredId)
        {
            return;
        }

        _lastEnteredId = map.Id;
        Shown = map.Settings.ShowsBanner ? map : null;
        _clock = TimeSpan.Zero;
    }

    /// <summary>Moves the clock; past the fade out the banner shows nothing.</summary>
    public void Advance(TimeSpan delta)
    {
        if (Shown is null)
        {
            return;
        }

        _clock += delta;
        if (_clock >= _wholeTime)
        {
            Shown = null;
        }
    }

    private static float OpacityAt(TimeSpan clock)
    {
        var sinceWait = clock - WaitTime;
        if (sinceWait < TimeSpan.Zero)
        {
            return 0;
        }

        if (sinceWait < FadeInTime)
        {
            return (float)(sinceWait / FadeInTime);
        }

        var sinceHold = sinceWait - FadeInTime - HoldTime;
        return sinceHold < TimeSpan.Zero ? 1 : 1 - (float)(sinceHold / FadeOutTime);
    }
}
