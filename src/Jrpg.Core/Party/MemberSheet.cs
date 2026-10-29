using Jrpg.Core.Stats;

namespace Jrpg.Core.Party;

/// <summary>
/// Builds a member's <see cref="StatSheet"/> from their class's numbers at their level and what they wear. It lives
/// here, not in <c>Stats</c>, so <c>Stats</c> stays a leaf that never sees the class (the plan's folder dependencies).
/// </summary>
public static class MemberSheet
{
    // The least max HP anyone has: gear that takes HP or Constitution away never leaves a member unable to stand.
    private const int LeastMaxHp = 1;

    /// <summary>
    /// Gear's primaries add before the conversions, so a Power ring raises damage and crit damage too; gear on a
    /// derived stat adds after them, its whole percents as permille (ADR 0026). Dodge and resistances are capped once,
    /// after gear.
    /// </summary>
    /// <param name="atLevel">The class's numbers at the member's level.</param>
    /// <param name="worn">What everything worn adds together.</param>
    /// <param name="weaponDamage">The worn weapon's damage; null without one, for the rules' unarmed damage.</param>
    /// <param name="rules">The conversions' rates and bases.</param>
    public static StatSheet Of(ClassStats atLevel, StatBonuses worn, int? weaponDamage, StatRules rules)
    {
        var primaries = atLevel.Primaries.Plus(PrimariesOf(worn));
        return new StatSheet(
            Primaries: primaries,
            MaxHp: Math.Max(LeastMaxHp, StatFormula.MaxHp(atLevel.MaxHp, primaries.Constitution, rules) + worn.Of(StatKind.MaxHp)),
            Speed: atLevel.Speed + worn.Of(StatKind.Speed),
            AttackDamage: weaponDamage ?? rules.UnarmedDamage,
            AccuracyPermille: StatFormula.AccuracyPermille(atLevel.Accuracy + worn.Of(StatKind.Accuracy), rules),
            CritChancePermille: StatFormula.CritChancePermille(primaries.Agility, rules) + PermilleOf(worn, StatKind.CritChance),
            CritDamagePermille: StatFormula.CritDamagePermille(primaries.Power, rules) + PermilleOf(worn, StatKind.CritDamage),
            DodgePermille: StatFormula.DodgePermille(primaries.Agility, worn.Of(StatKind.Dodge), rules),
            CrowdControlResistancePermille:
                StatFormula.CrowdControlResistancePermille(rules) + PermilleOf(worn, StatKind.CrowdControlResistance),
            DamagePermille: StatFormula.DamagePermille(primaries.Power, rules),
            Resistances: StatFormula.ResistancesFrom(primaries, ResistancesOf(worn), rules),
            StatusPermille: StatFormula.StatusPermille(primaries.Intelligence, rules),
            ExtraStatusTurns: StatFormula.ExtraStatusTurns(primaries.Intelligence, rules))
        {
            LifeStealPermille = PermilleOf(worn, StatKind.LifeSteal),
            ArmorPenetrationPermille = PermilleOf(worn, StatKind.ArmorPenetration),
        };
    }

    private static PrimaryStats PrimariesOf(StatBonuses worn) => new(
        worn.Of(StatKind.Power),
        worn.Of(StatKind.Agility),
        worn.Of(StatKind.Intelligence),
        worn.Of(StatKind.Constitution),
        worn.Of(StatKind.Will));

    private static Resistances ResistancesOf(StatBonuses worn) =>
        Resistances.From(type => PermilleOf(worn, StatKinds.ResistanceTo(type)));

    // Gear gives percent kinds in whole percents (content), the sheet holds them in permille.
    private static int PermilleOf(StatBonuses worn, StatKind kind) => StatFormula.PermilleOf(worn.Of(kind));
}
