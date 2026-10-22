using Jrpg.Core.Dialogue;

namespace Jrpg.Game.Dialogue;

/// <summary>
/// The pages of one message being read, one message box each, typed out one character at a time. Confirm completes a
/// page still typing, then turns it (decision 54).
/// </summary>
internal sealed class SpeechPlayback
{
    // The marker is shown for half a second, then hidden for half a second (developer decision 54).
    private static readonly TimeSpan _markerBlinkHalf = TimeSpan.FromSeconds(0.5);

    private readonly IReadOnlyList<string> _pages;
    private readonly TimeSpan _characterTime;
    private Typewriter _typewriter;
    private TimeSpan _sinceComplete;

    /// <param name="speech">Its lines are the pages, at least one (content validation); its speaker and portrait show with them.</param>
    /// <param name="characterTime">Per character typed; zero shows each page whole.</param>
    public SpeechPlayback(Speech speech, TimeSpan characterTime)
    {
        Speech = speech;
        _pages = speech.Lines;
        _characterTime = characterTime;
        _typewriter = new Typewriter(_pages[0].Length, characterTime);
    }

    /// <summary>Who speaks and with which portrait; a found item has neither.</summary>
    public Speech Speech { get; }

    /// <summary>Which page is on screen, so the box lays a page out only when it turns.</summary>
    public int PageIndex { get; private set; }

    /// <summary>The page on screen; read it only while the playback isn't finished.</summary>
    public string CurrentPage => _pages[PageIndex];

    /// <summary>How many characters of the current page are typed out.</summary>
    public int VisibleLength => _typewriter.VisibleLength;

    /// <summary>The page is complete and waits for Confirm, in the visible half of the blink.</summary>
    public bool IsMarkerVisible => _typewriter.IsComplete && (_sinceComplete.Ticks / _markerBlinkHalf.Ticks) % 2 == 0;

    public bool IsFinished => PageIndex >= _pages.Count;

    public void Update(TimeSpan delta)
    {
        if (_typewriter.IsComplete)
        {
            _sinceComplete += delta;
        }
        else
        {
            _typewriter.Update(delta);
        }
    }

    /// <exception cref="InvalidOperationException">The last page was already read.</exception>
    public void Confirm()
    {
        if (IsFinished)
        {
            throw new InvalidOperationException("Every page was already read.");
        }

        if (!_typewriter.IsComplete)
        {
            _typewriter.RevealAll();
            return;
        }

        PageIndex++;
        _sinceComplete = TimeSpan.Zero;
        if (!IsFinished)
        {
            _typewriter = new Typewriter(CurrentPage.Length, _characterTime);
        }
    }
}
