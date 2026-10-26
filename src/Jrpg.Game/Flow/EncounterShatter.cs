using Jrpg.Game.Ui;

namespace Jrpg.Game.Flow;

/// <summary>
/// How far the broken glass has flown since the fade into battle started (decision 258): over the fade-out, fast at
/// first and slowing, as the shards shrink a little and fade into the black.
/// </summary>
/// <param name="length">The flight's length: the fade-out's, so the shards fly until the screen is black.</param>
internal sealed class EncounterShatter(TimeSpan length)
{
    // Null until the flight starts: the shards hold their places from the crack until the fade.
    private TimeSpan? _elapsed;

    /// <summary>0 when the shards break apart to 1 when the screen is black, eased out.</summary>
    public float Share => _elapsed is { } elapsed ? Easing.CubicOut(Easing.ShareOf(elapsed, length)) : 0;

    /// <summary>The seconds of flight the shards have covered, eased: <see cref="Share"/> of the length.</summary>
    public float FlightSeconds => Share * (float)length.TotalSeconds;

    public void Start() => _elapsed = TimeSpan.Zero;

    public void Advance(TimeSpan delta) => _elapsed += delta;
}
