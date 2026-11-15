namespace Jrpg.Game.Battle;

/// <summary>
/// How the impact's light looks as time passes since damage landed (decision 660): the warm flash over the battlefield
/// and the white wash on the battlers near the hit, both at their height for 0.1 s and gone by 0.2 s, as in the
/// reference clip's frames #77-#82. Placeholder heights and times, tuned by eye against the clip.
/// </summary>
internal static class ImpactLight
{
    // Held at its height for the first half, then let go in the second.
    private static readonly TimeSpan _heldFor = TimeSpan.FromSeconds(0.1);
    private static readonly TimeSpan _goneAt = TimeSpan.FromSeconds(0.2);

    // Warm enough to read as light over the battlefield, not so strong that the battlers' colors drown.
    private const float SceneFlashHeight = 0.45f;

    // The target's whiteness at the hit; a battler farther away gets less, none past the reach.
    private const float WashHeight = 0.6f;
    private const float WashReach = 400;

    /// <param name="sinceImpact">From the impact on; before it, nothing is lit.</param>
    public static ImpactLightLook At(TimeSpan sinceImpact)
    {
        var share = ShareAt(sinceImpact);
        return new ImpactLightLook(SceneFlashHeight * share, WashHeight * share);
    }

    /// <summary>
    /// The wash on a battler at <paramref name="distance"/> virtual px from the hit target, falling linearly.
    /// </summary>
    /// <param name="distance">From the hit target's center to the battler's.</param>
    /// <param name="wash">The wash at the target now (<see cref="ImpactLightLook.Wash"/>).</param>
    public static float WashAt(float distance, float wash) => wash * Math.Max(0, 1 - (distance / WashReach));

    // Full while held, then down in a straight line to none.
    private static float ShareAt(TimeSpan sinceImpact)
    {
        if (sinceImpact < TimeSpan.Zero || sinceImpact >= _goneAt)
        {
            return 0;
        }

        return sinceImpact <= _heldFor ? 1 : (float)((_goneAt - sinceImpact) / (_goneAt - _heldFor));
    }
}
