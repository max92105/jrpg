using Jrpg.Game.Ui;

namespace Jrpg.Game.Flow;

/// <summary>
/// The white flash when an encounter starts (decision 258): up to 85 % white in 0.04 s, back down in 0.08 s, over the
/// field's frozen frame and before the fade into battle.
/// </summary>
internal sealed class EncounterFlash
{
    private const float Peak = 0.85f;
    private static readonly TimeSpan _rise = TimeSpan.FromSeconds(0.04);
    private static readonly TimeSpan _fall = TimeSpan.FromSeconds(0.08);

    // Null until a flash starts; it keeps counting after the flash ends, so the frame stays broken through the fade.
    private TimeSpan? _elapsed;

    public bool IsPlaying => _elapsed is { } elapsed && elapsed < _rise + _fall;

    /// <summary>Whether the flash has reached its peak, when the frozen frame cracks (decision 258).</summary>
    public bool IsPastPeak => _elapsed >= _rise;

    /// <summary>0 clear to <c>0.85</c> at the flash's peak.</summary>
    public float Whiteness => _elapsed is { } elapsed && IsPlaying
        ? Peak * (elapsed < _rise ? Easing.ShareOf(elapsed, _rise) : 1 - Easing.ShareOf(elapsed - _rise, _fall))
        : 0;

    public void Start() => _elapsed = TimeSpan.Zero;

    public void Advance(TimeSpan delta)
    {
        if (_elapsed is { } elapsed)
        {
            _elapsed = elapsed + delta;
        }
    }
}
