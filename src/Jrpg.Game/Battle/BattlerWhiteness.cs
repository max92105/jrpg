using Jrpg.Core.Battle;

namespace Jrpg.Game.Battle;

/// <summary>
/// How strongly white lies over a battler as it is drawn, the strongest of three: a fallen battler's, still fading, the
/// actor's blink as its action winds up (decisions 648, 649), and the impact's wash on the battlers near where damage
/// landed (decision 660).
/// </summary>
internal static class BattlerWhiteness
{
    // Bright, with the battler's own colors and lines still showing through.
    private const float Strength = 0.6f;

    /// <param name="combatant">The battler drawn.</param>
    /// <param name="windUp">The action winding up or last wound up, whose actor blinks; null before any.</param>
    /// <param name="wash">
    /// The impact's wash on this battler now (<see cref="ImpactLight.WashAt"/>); 0 when unlit.
    /// </param>
    /// <returns>From 0, none, to <see cref="Strength"/>.</returns>
    public static float Of(Combatant combatant, ActionWindUp? windUp, float wash)
    {
        if (!combatant.IsAlive)
        {
            return Strength;
        }

        var blink = windUp is null ? 0 : Strength * windUp.BlinkOf(combatant);
        return Math.Max(blink, wash);
    }
}
