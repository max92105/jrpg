namespace Jrpg.Game.Battle;

/// <summary>
/// Sounds due at times along their own clock, for <see cref="PlaybackSoundQueue"/>: an action's wind-up, from its
/// plaque, and its playback, from its impact (decision 647).
/// </summary>
internal interface ISoundTimeline
{
    /// <summary>Sorted by time.</summary>
    IReadOnlyList<TimedSound> Sounds { get; }

    /// <summary>The time on the timeline's clock.</summary>
    TimeSpan Elapsed { get; }
}
