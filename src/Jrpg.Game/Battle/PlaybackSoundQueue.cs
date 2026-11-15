using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>Asks a turn's sounds, the wind-up's or the playback's, as its time reaches each, once.</summary>
internal sealed class PlaybackSoundQueue
{
    private ISoundTimeline? _followed;
    private int _next;

    /// <param name="timeline">The wind-up or the playback on show; a new one starts again from its first sound.</param>
    /// <param name="sounds">Where the sounds due now are asked.</param>
    public void Follow(ISoundTimeline? timeline, AskedSounds sounds)
    {
        if (!ReferenceEquals(timeline, _followed))
        {
            _followed = timeline;
            _next = 0;
        }

        while (timeline is not null && _next < timeline.Sounds.Count && timeline.Sounds[_next].At <= timeline.Elapsed)
        {
            sounds.Ask(timeline.Sounds[_next].File);
            _next++;
        }
    }
}
