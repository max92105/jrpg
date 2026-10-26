using Jrpg.Game.Ui;

namespace Jrpg.Game.Flow;

/// <summary>
/// How far the shards of an encounter's broken frame have flown (decision 258): nowhere until the fade to the battle
/// starts, then all the way over its fade-out, fast at first and settling, as glass thrown outward would.
/// </summary>
/// <param name="length">The fade-out the shards fly over.</param>
internal sealed class EncounterShatter(TimeSpan length)
{
    private bool _isFlying;
    private TimeSpan _elapsed;

    /// <summary>0 at the break to 1 at the fade-out's end, eased out.</summary>
    public float Share => _isFlying ? Easing.CubicOut(Easing.ShareOf(_elapsed, length)) : 0;

    /// <summary>The time the shards have flown, eased like <see cref="Share"/>, in seconds.</summary>
    public float FlightSeconds => Share * (float)length.TotalSeconds;

    public void Start()
    {
        _isFlying = true;
        _elapsed = TimeSpan.Zero;
    }

    public void Advance(TimeSpan delta) => _elapsed += delta;
}
