using Jrpg.Core.Battle;

namespace Jrpg.Game.Battle;

/// <summary>
/// How strongly white lies over a battler as it is drawn: a fallen battler's, still fading, or the actor's blink as its
/// action winds up (decisions 648, 649).
/// </summary>
internal static class BattlerWhiteness
{
    // Bright, with the battler's own colors and lines still showing through.
    private const float Strength = 0.6f;

    /// <param name="combatant">The battler drawn.</param>
    /// <param name="windUp">The action winding up or last wound up, whose actor blinks; null before any.</param>
    /// <returns>From 0, none, to <see cref="Strength"/>.</returns>
    public static float Of(Combatant combatant, ActionWindUp? windUp)
    {
        if (!combatant.IsAlive)
        {
            return Strength;
        }

        return windUp is null ? 0 : Strength * windUp.BlinkOf(combatant);
    }
}
