using Jrpg.Game.Audio;
using Jrpg.Game.Dialogue;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Exploration;

/// <summary>
/// The message box read over the map: a step's pages typed out with their ticks (<see cref="TypingSound"/>), turned by
/// Confirm, and closed after the last page.
/// </summary>
/// <param name="fonts">The message box's fonts.</param>
/// <param name="voice">The voice-over playing, which mutes the ticks.</param>
internal sealed class MessageReading(FontLibrary fonts, VoiceChannel voice)
{
    private readonly DialogueBox _box = new(fonts);
    private readonly TypingSound _typing = new(voice);

    // The pages being read; null when no message is on screen.
    private SpeechPlayback? _pages;

    public bool IsShowing => _pages is not null;

    /// <summary>The page on screen; null while none shows.</summary>
    public string? ShownLine => _pages?.CurrentPage;

    /// <summary>The typing ticks of the last update.</summary>
    public IReadOnlyList<string> Sounds => _typing.Sounds;

    public void Show(SpeechPlayback pages)
    {
        _pages = pages;
        _box.Show(pages);
    }

    /// <summary>Types the page, or completes or turns it on Confirm; after the last page the message closes.</summary>
    public void Update(TimeSpan delta, InputFrame input)
    {
        _typing.Clear();
        var pages = _pages ?? throw new InvalidOperationException("No message is showing.");

        // Confirm is judged on the page as the player last saw it, so a press on the update the last character would
        // appear completes the page rather than turning it (decision 54).
        if (!input.IsPressed(GameButton.Confirm))
        {
            var before = pages.VisibleLength;
            pages.Update(delta);
            _typing.Follow(pages.CurrentPage, before, pages.VisibleLength, delta);
            return;
        }

        var shownPage = pages.PageIndex;
        pages.Confirm();
        if (pages.IsFinished)
        {
            _pages = null;
        }
        else if (pages.PageIndex != shownPage)
        {
            _box.Show(pages);
        }
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        if (_pages is not null)
        {
            _box.Draw(batch, textures, _pages);
        }
    }
}
