using Jrpg.Core.Stats;
using Jrpg.Game.Menus;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Menus;

public sealed class StatComparisonTests
{
    [Fact]
    public void Between_PowerBonusOn_ListsPowerAndCritDamage()
    {
        var before = Sheet(power: 10, critDamagePermille: 1550);
        var after = Sheet(power: 15, critDamagePermille: 1575);

        var comparison = StatComparison.Between(before, after);

        Assert.Equal([StatKind.Power, StatKind.CritDamage], comparison.Changes.Select(change => change.Kind));
        Assert.Equal((10, 15), (comparison.Changes[0].Before, comparison.Changes[0].After));
    }

    [Fact]
    public void Between_SameNumbers_IsEmpty()
    {
        var sheet = Sheet(power: 10, critDamagePermille: 1500);

        Assert.Empty(StatComparison.Between(sheet, sheet).Changes);
    }

    [Fact]
    public void Between_WeaponDamageAndPower_ListsWeaponDamageFirst()
    {
        var before = Sheet(power: 10, critDamagePermille: 1500);
        var after = Sheet(power: 12, critDamagePermille: 1500) with { AttackDamage = 18 };

        var comparison = StatComparison.Between(before, after);

        Assert.Equal([MessageIds.StatWeaponDamage, MessageIds.StatPower], comparison.Changes.Select(change => change.NameId));
        Assert.Null(comparison.Changes[0].Kind);
    }

    // A Power ring: the same 10 raw attack at 110 % then 120 % reads 11, then 12.
    [Fact]
    public void Between_DamagePercentAloneRaised_MovesWeaponDamage()
    {
        var before = Sheet(power: 10, critDamagePermille: 1500) with { DamagePermille = 1100 };
        var after = Sheet(power: 10, critDamagePermille: 1500) with { DamagePermille = 1200 };

        var change = Assert.Single(StatComparison.Between(before, after).Changes);

        Assert.Equal((MessageIds.StatWeaponDamage, 11, 12), (change.NameId, change.Before, change.After));
    }

    [Fact]
    public void Between_ResistanceChangeUnderOnePercent_IsNotListed()
    {
        // 2.0 % to 2.7 %: both read "2 %".
        var before = Sheet(power: 10, critDamagePermille: 1500) with { Resistances = Resistances.None with { Light = 20 } };
        var after = Sheet(power: 10, critDamagePermille: 1500) with { Resistances = Resistances.None with { Light = 27 } };

        Assert.Empty(StatComparison.Between(before, after).Changes);
    }

    [Fact]
    public void Between_ResistanceChangeAcrossAPercent_IsListed()
    {
        var before = Sheet(power: 10, critDamagePermille: 1500) with { Resistances = Resistances.None with { Light = 27 } };
        var after = Sheet(power: 10, critDamagePermille: 1500) with { Resistances = Resistances.None with { Light = 30 } };

        Assert.Equal([StatKind.LightResistance], StatComparison.Between(before, after).Changes.Select(change => change.Kind));
    }

    private static StatSheet Sheet(int power, int critDamagePermille) => new(
        new PrimaryStats(power, 0, 0, 0, 0), MaxHp: 100, Speed: 10, AttackDamage: 10, AccuracyPermille: 900, CritChancePermille: 50,
        critDamagePermille, DodgePermille: 0, CrowdControlResistancePermille: 0, DamagePermille: 1000, Resistances.None,
        StatusPermille: 1000, ExtraStatusTurns: 0);
}
