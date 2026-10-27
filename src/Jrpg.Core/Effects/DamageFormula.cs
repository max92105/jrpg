using Jrpg.Core.Stats;

namespace Jrpg.Core.Effects;

/// <summary>
/// Defense subtracts straight from power, the placeholder ported from the demo (plan 2026-10-17, guiding rule); the
/// target's resistance to the hit's type and its statuses, such as Exposed's +50 % (question 215), then scale what gets
/// through (ADR 0027). A minimum keeps every hit counting.
/// </summary>
internal static class DamageFormula
{
    public const int MinimumDamage = 1;

    private const long Whole = 1000;

    /// <returns>
    /// <c>max(1, (power − defense) × (1000 − resistance) × (1000 + extra taken) ÷ 1000²)</c>, rounded down
    /// once. Both shares come after defense, so a weakness, like Exposed, multiplies what actually gets through
    /// rather than the raw power. The resistance is held within <see cref="Resistances.FloorPercent"/>..
    /// <see cref="Resistances.CapPercent"/>.
    /// </returns>
    public static int Calculate(int power, Mitigation mitigation)
    {
        var resistance = Math.Clamp(
            mitigation.ResistancePermille,
            StatFormula.PermilleOf(Resistances.FloorPercent),
            StatFormula.PermilleOf(Resistances.CapPercent));
        var throughDefense = (long)power - mitigation.Defense;
        var damage = throughDefense * (Whole - resistance) * (Whole + mitigation.ExtraTakenPermille) / (Whole * Whole);
        return (int)Math.Max(MinimumDamage, damage);
    }
}
