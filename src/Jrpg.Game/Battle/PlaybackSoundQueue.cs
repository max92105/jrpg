using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>Asks the shown playback's sounds as its time reaches each, once.</summary>
internal sealed class PlaybackSoundQueue
{
    private ActionPlayback? _followed;
    private int _next;

    /// <param name="playback">The turn on show; a new one starts again from its first sound.</param>
    /// <param name="sounds">Where the sounds due now are asked.</param>
    public void Follow(ActionPlayback? playback, AskedSounds sounds)
    {
        if (!ReferenceEquals(playback, _followed))
        {
            _followed = playback;
            _next = 0;
        }

        while (playback is not null && _next < playback.Sounds.Count && playback.Sounds[_next].At <= playback.Elapsed)
        {
            sounds.Ask(playback.Sounds[_next].File);
            _next++;
        }
    }
}
