using Jrpg.Core.Stats;

namespace Jrpg.Core.Effects;

/// <summary>
/// Whether a damaging hit lands (decision 297): the attacker's accuracy against the target's dodge. Accuracy already
/// holds the rules' base, so the chance is their difference, kept between a floor, so no target is out of reach, and a
/// ceiling of 100 %, so a hit that can't miss draws nothing (ADR 0026).
/// </summary>
internal static class HitFormula
{
    /// <summary>
    /// The least chance any hit has: 20 % keeps a fight against a nimble foe a fight, not a wall (placeholder).
    /// </summary>
    public const int FloorPercent = 20;

    /// <summary>A sure hit, which draws nothing (ADR 0026).</summary>
    public const int CeilingPercent = 100;

    /// <returns><c>clamp(accuracy − dodge, floor, ceiling)</c>, in permille.</returns>
    public static int ChancePermille(int accuracyPermille, int dodgePermille) => Math.Clamp(
        accuracyPermille - dodgePermille,
        StatFormula.PermilleOf(FloorPercent),
        StatFormula.PermilleOf(CeilingPercent));
}
