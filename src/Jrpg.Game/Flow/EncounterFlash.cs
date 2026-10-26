namespace Jrpg.Game.Flow;

/// <summary>
/// The white flash over the frozen field when an encounter starts (decision 258): up to 85 % in 0.04 s, back down in
/// 0.08 s, before the fade to the battle. A sharp rise and a softer fall read as a hit, not a blink.
/// </summary>
internal sealed class EncounterFlash
{
    /// <summary>The flash at its brightest, 0 to 1.</summary>
    public const float PeakWhiteness = 0.85f;

    private static readonly TimeSpan _rise = TimeSpan.FromSeconds(0.04);
    private static readonly TimeSpan _fall = TimeSpan.FromSeconds(0.08);

    // Done until started, so a flash never started plays nothing.
    private TimeSpan _elapsed = _rise + _fall;

    public bool IsPlaying => _elapsed < _rise + _fall;

    /// <summary>From the flash's peak on, when the frame cracks; also true when no flash plays.</summary>
    public bool IsPastPeak => _elapsed >= _rise;

    /// <summary>0 clear to <see cref="PeakWhiteness"/>.</summary>
    public float Whiteness
    {
        get
        {
            if (!IsPlaying)
            {
                return 0;
            }

            return _elapsed < _rise
                ? PeakWhiteness * (float)(_elapsed / _rise)
                : PeakWhiteness * (1 - (float)((_elapsed - _rise) / _fall));
        }
    }

    public void Start() => _elapsed = TimeSpan.Zero;

    public void Advance(TimeSpan delta) => _elapsed += delta;
}
