using Jrpg.Core.Stats;

namespace Jrpg.Core.Party;

/// <summary>
/// Builds a member's <see cref="StatSheet"/> from their class's numbers at their level. It lives here, not in
/// <c>Stats</c>, so <c>Stats</c> stays a leaf that never sees the class (the plan's folder dependencies).
/// </summary>
internal static class MemberSheet
{
    public static StatSheet Of(ClassStats atLevel, StatRules rules)
    {
        var primaries = atLevel.Primaries;
        return new StatSheet(
            Primaries: primaries,
            MaxHp: StatFormula.MaxHp(atLevel.MaxHp, primaries.Constitution, rules),
            Speed: atLevel.Speed,
            AccuracyPermille: StatFormula.AccuracyPermille(atLevel.Accuracy, rules),
            CritChancePermille: StatFormula.CritChancePermille(primaries.Agility, rules),
            CritDamagePermille: StatFormula.CritDamagePermille(primaries.Power, rules),
            DodgePermille: StatFormula.DodgePermille(primaries.Agility, rules),
            CrowdControlResistancePermille: StatFormula.CrowdControlResistancePermille(rules),
            DamagePermille: StatFormula.DamagePermille(primaries.Power, rules));
    }
}
