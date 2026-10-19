namespace Jrpg.Game.Dialogue;

/// <summary>The pages of one message being read, one message box each; Confirm turns the page (legacy <c>DialogManager</c>).</summary>
/// <param name="pages">At least one page; content validation guarantees a dialogue step has a line.</param>
internal sealed class SpeechPlayback(IReadOnlyList<string> pages)
{
    private int _index;

    /// <summary>The page on screen; read it only while the playback isn't finished.</summary>
    public string CurrentPage => pages[_index];

    public bool IsFinished => _index >= pages.Count;

    /// <exception cref="InvalidOperationException">The last page was already read.</exception>
    public void Confirm()
    {
        if (IsFinished)
        {
            throw new InvalidOperationException("Every page was already read.");
        }

        _index++;
    }
}
