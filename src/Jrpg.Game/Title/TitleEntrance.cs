using Jrpg.Game.Ui;

namespace Jrpg.Game.Title;

/// <summary>
/// The title screen coming in from black: the picture, then the game's name, then the entries, each fading in over its
/// own window of time. Timed from the first drawn frame, like the menu's opening (BG1), so a slow first frame can't use
/// the entrance up before it shows.
/// </summary>
internal sealed class TitleEntrance
{
    // Placeholder windows, in seconds since the first drawn frame (plan 2026-10-25-ui-second-pass, design.md).
    private static readonly FadeWindow _picture = new(TimeSpan.Zero, TimeSpan.FromSeconds(0.6));
    private static readonly FadeWindow _name = new(TimeSpan.FromSeconds(0.3), TimeSpan.FromSeconds(0.8));
    private static readonly FadeWindow _entries = new(TimeSpan.FromSeconds(0.8), TimeSpan.FromSeconds(1.2));

    private TimeSpan _sinceFirstDrawn;
    private bool _isDrawn;

    /// <summary>0 for hidden to 1 for fully shown.</summary>
    public float PictureOpacity => _picture.OpacityAt(_sinceFirstDrawn);

    /// <summary>0 for hidden to 1 for fully shown.</summary>
    public float NameOpacity => _name.OpacityAt(_sinceFirstDrawn);

    /// <summary>The entries and the footer line: 0 for hidden to 1 for fully shown.</summary>
    public float EntriesOpacity => _entries.OpacityAt(_sinceFirstDrawn);

    /// <summary>Called on each drawn frame; time counts from the first.</summary>
    public void MarkDrawn() => _isDrawn = true;

    /// <summary>Counts <paramref name="delta"/> only once a frame has been drawn.</summary>
    public void Advance(TimeSpan delta)
    {
        if (_isDrawn)
        {
            _sinceFirstDrawn += delta;
        }
    }

    // One part's fade, from its start to its end.
    private readonly record struct FadeWindow(TimeSpan Start, TimeSpan End)
    {
        public float OpacityAt(TimeSpan elapsed) => Easing.ShareOf(elapsed - Start, End - Start);
    }
}
