using Jrpg.Game.Audio;
using Jrpg.Game.Dialogue;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Exploration;

/// <summary>
/// Asks the typing sound as a page types out (decision 422); muted while the event's voice-over is on. The channel
/// can't tell when a line ends, so it stays muted for the rest of an event once its voice step started.
/// </summary>
internal sealed class TypingSound(VoiceChannel voice)
{
    private readonly AskedSounds _sounds = new();
    private TimeSpan _sinceTick = TypingTicks.MinimumGap;

    public IReadOnlyList<string> Sounds => _sounds.Files;

    public void Clear() => _sounds.Clear();

    public void Follow(string page, int visibleBefore, int visibleNow, TimeSpan delta)
    {
        _sinceTick += delta;
        if (voice.VoiceFile is null && TypingTicks.IsDue(page, visibleBefore, visibleNow, _sinceTick))
        {
            _sounds.Ask(UiSoundFiles.Effects.TextTyping);
            _sinceTick = TimeSpan.Zero;
        }
    }
}
