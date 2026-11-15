namespace Jrpg.Game.Battle;

/// <summary>
/// How the impact's light looks as time passes since damage landed (decision 660): the warm flash over the battlefield,
/// the white wash on the battlers near the hit and the glow on the floor around the target, all at their height for 0.1
/// s and gone by 0.2 s, as in the reference clip's frames #77-#82; then the warm halo on the target, rising as the
/// flash fades, held to 0.43 s and gone by 0.5 s (frames #83-#90). Placeholder heights and times, tuned by eye.
/// </summary>
internal static class ImpactLight
{
    // The flash is held at its height for the first half, then let go in the second, while the halo rises.
    private static readonly TimeSpan _heldFor = TimeSpan.FromSeconds(0.1);
    private static readonly TimeSpan _goneAt = TimeSpan.FromSeconds(0.2);

    // The halo takes over from the flash: it rises while the flash falls, so the light never drops out between them.
    private static readonly TimeSpan _haloHeldTo = TimeSpan.FromSeconds(0.43);
    private static readonly TimeSpan _haloGoneAt = TimeSpan.FromSeconds(0.5);

    // Warm enough to read as light over the battlefield, not so strong that the battlers' colors drown.
    private const float SceneFlashHeight = 0.45f;

    // The target's whiteness at the hit; a battler farther away gets less, none past the reach.
    private const float WashHeight = 0.6f;
    private const float WashReach = 400;

    // The floor is lit brighter than the air: the clip's ground glows near white under the target.
    private const float FloorGlowHeight = 0.8f;
    private const float HaloHeight = 0.5f;

    /// <param name="sinceImpact">From the impact on; before it, nothing is lit.</param>
    public static ImpactLightLook At(TimeSpan sinceImpact)
    {
        var flash = FlashShareAt(sinceImpact);
        return new ImpactLightLook(
            SceneFlashHeight * flash, WashHeight * flash, FloorGlowHeight * flash, HaloHeight * HaloShareAt(sinceImpact));
    }

    /// <summary>
    /// The wash on a battler at <paramref name="distance"/> virtual px from the hit target, falling linearly.
    /// </summary>
    /// <param name="distance">From the hit target's center to the battler's.</param>
    /// <param name="wash">The wash at the target now (<see cref="ImpactLightLook.Wash"/>).</param>
    public static float WashAt(float distance, float wash) => wash * Math.Max(0, 1 - (distance / WashReach));

    // Full while held, then down in a straight line to none.
    private static float FlashShareAt(TimeSpan sinceImpact)
    {
        if (sinceImpact < TimeSpan.Zero || sinceImpact >= _goneAt)
        {
            return 0;
        }

        return sinceImpact <= _heldFor ? 1 : (float)((_goneAt - sinceImpact) / (_goneAt - _heldFor));
    }

    // Up in a straight line while the flash falls, held, then down in a straight line to none.
    private static float HaloShareAt(TimeSpan sinceImpact)
    {
        if (sinceImpact <= _heldFor || sinceImpact >= _haloGoneAt)
        {
            return 0;
        }

        if (sinceImpact < _goneAt)
        {
            return (float)((sinceImpact - _heldFor) / (_goneAt - _heldFor));
        }

        return sinceImpact <= _haloHeldTo ? 1 : (float)((_haloGoneAt - sinceImpact) / (_haloGoneAt - _haloHeldTo));
    }
}
