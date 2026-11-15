using Jrpg.Core.Battle;

namespace Jrpg.Game.Battle;

/// <summary>An effect strip played over one target.</summary>
/// <param name="Target">Who the effect landed on.</param>
/// <param name="File">The strip, in the effects folder.</param>
/// <param name="IsMirrored">Whether it is drawn mirrored (<see cref="IsMirroredFor"/>).</param>
internal sealed record EffectOnTarget(Combatant Target, string File, bool IsMirrored)
{
    /// <summary>
    /// Whether an attacker's strip is drawn mirrored: effect art faces right, as an enemy attacking the party, so a
    /// party member's attack is drawn the other way (decision 655). The wind-up's lead and the impact both read this
    /// one rule, so they face the same way.
    /// </summary>
    public static bool IsMirroredFor(Combatant attacker) => attacker.Side == BattleSide.Party;
}
