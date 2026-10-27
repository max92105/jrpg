using Jrpg.Core.Stats;

namespace Jrpg.Core.Tests.Builders;

/// <summary>Stat rules for tests.</summary>
internal static class TestStatRules
{
    /// <summary>
    /// Every rate 0, accuracy 100, crit 0 and no extra status turn: primaries change nothing, every hit lands and none
    /// crits, so a member's numbers are their class's and seeded battles keep their sequences (ADR 0026).
    /// </summary>
    public static StatRules Neutral { get; } = new(
        HpPercentPer100Constitution: 0,
        BaseAccuracy: 100,
        BaseCritChance: 0,
        BaseDodge: 0,
        BaseCrowdControlResistance: 0,
        DamagePercentPer100Power: 0,
        CritDamagePercentPer100Power: 0,
        CritChancePer100Agility: 0,
        DodgePer100Agility: 0,
        PhysicalResistancePer100Constitution: 0,
        MagicalResistancePer100Will: 0,
        StatusPercentPer100Intelligence: 0,
        IntelligencePerExtraStatusTurn: int.MaxValue,
        UnarmedDamageType: DamageType.Blunt);
}
