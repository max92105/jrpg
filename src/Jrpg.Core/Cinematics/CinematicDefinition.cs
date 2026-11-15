namespace Jrpg.Core.Cinematics;

/// <summary>
/// One cinematic as content (ADR 0046): its stills in order and the track under them, played by an event's
/// <c>cinematic</c> step.
/// </summary>
public sealed record CinematicDefinition
{
    public required string Id { get; init; }

    /// <summary>
    /// A track in the music folder, its folder included (<c>town/loop.wav</c>); null keeps the map's (decision 614).
    /// </summary>
    public string? MusicFile { get; init; }

    /// <summary>The stills in the order they show, at least one.</summary>
    public required IReadOnlyList<CinematicStill> Stills { get; init; }

    /// <summary>
    /// The stills' times summed; Data keeps the total a time the game can count. A loop, not LINQ: the game reads it on
    /// every frame of a cinematic.
    /// </summary>
    public TimeSpan Duration
    {
        get
        {
            var total = TimeSpan.Zero;
            for (var index = 0; index < Stills.Count; index++)
            {
                total += Stills[index].Duration;
            }

            return total;
        }
    }

    /// <summary>
    /// Which still shows at a time into the cinematic and how far into it: the game and the editor's preview ask it, so
    /// a still's place in time has one rule (decision 629). A still's end is the next one's start.
    /// </summary>
    /// <returns>Null at or past the last still's end.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="elapsed"/> is negative.</exception>
    public StillMoment? MomentAt(TimeSpan elapsed)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(elapsed, TimeSpan.Zero);
        var left = elapsed;
        for (var index = 0; index < Stills.Count; index++)
        {
            var duration = Stills[index].Duration;
            if (left < duration)
            {
                return new StillMoment(index, left);
            }

            left -= duration;
        }

        return null;
    }

    /// <summary>
    /// The still whose voice line speaks while this one shows (decision 613): itself when it has a voice, else the
    /// latest earlier one with a voice, which plays on; null when none has. Scans back without allocating.
    /// </summary>
    public int? VoicedStillAt(int stillIndex)
    {
        for (var index = stillIndex; index >= 0; index--)
        {
            if (Stills[index].VoiceFile is not null)
            {
                return index;
            }
        }

        return null;
    }

    // A record compares a list by reference; two cinematics read from the same content are one cinematic.
    public bool Equals(CinematicDefinition? other) =>
        other is not null && (Id, MusicFile) == (other.Id, other.MusicFile) && Stills.SequenceEqual(other.Stills);

    public override int GetHashCode() => HashCode.Combine(Id, MusicFile, Stills.Count);
}
