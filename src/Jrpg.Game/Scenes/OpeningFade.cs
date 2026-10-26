namespace Jrpg.Game.Scenes;

/// <summary>
/// A screen fading in from clear, timed from its first drawn frame: after one long frame the fixed step runs several
/// updates before the next draw, and counting those would end the fade before it showed (plan
/// 2026-10-25-ui-second-pass, BG1).
/// </summary>
/// <param name="duration">From clear to opaque.</param>
internal sealed class OpeningFade(TimeSpan duration)
{
    private TimeSpan _sinceFirstDrawn;
    private bool _isDrawn;

    /// <summary>0 for clear to 1 for opaque.</summary>
    public float Opacity => (float)Math.Min(1, _sinceFirstDrawn / duration);

    /// <summary>Called on each drawn frame; time counts from the first.</summary>
    public void MarkDrawn() => _isDrawn = true;

    /// <summary>Counts <paramref name="delta"/> only once a frame has been drawn.</summary>
    public void Update(TimeSpan delta)
    {
        if (_isDrawn)
        {
            _sinceFirstDrawn += delta;
        }
    }
}
