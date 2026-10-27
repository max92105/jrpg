using Jrpg.Core.Stats;
using Jrpg.Game.Menus;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Menus;

public sealed class StatComparisonTests
{
    private static readonly CombatStats _stats = new(Attack: 10, Defense: 5);

    [Fact]
    public void Between_PowerBonusOn_ListsPowerAndCritDamage()
    {
        var before = Numbers(Sheet(power: 10, critDamagePermille: 1550));
        var after = Numbers(Sheet(power: 15, critDamagePermille: 1575));

        var comparison = StatComparison.Between(before, after);

        Assert.Equal([StatKind.Power, StatKind.CritDamage], comparison.Changes.Select(change => change.Kind));
        Assert.Equal((10, 15), (comparison.Changes[0].Before, comparison.Changes[0].After));
    }

    [Fact]
    public void Between_SameNumbers_IsEmpty()
    {
        var numbers = Numbers(Sheet(power: 10, critDamagePermille: 1500));

        Assert.Empty(StatComparison.Between(numbers, numbers).Changes);
    }

    [Fact]
    public void Between_AttackAndPower_ListsAttackFirst()
    {
        var before = Numbers(Sheet(power: 10, critDamagePermille: 1500));
        var after = new StatComparison.Numbers(Sheet(power: 12, critDamagePermille: 1500), _stats with { Attack = 18 });

        var comparison = StatComparison.Between(before, after);

        Assert.Equal([MessageIds.StatAttack, MessageIds.StatPower], comparison.Changes.Select(change => change.NameId));
        Assert.Null(comparison.Changes[0].Kind);
    }

    [Fact]
    public void Between_ResistanceChangeUnderOnePercent_IsNotListed()
    {
        // 2.0 % to 2.7 %: both read "2 %".
        var before = Numbers(Sheet(power: 10, critDamagePermille: 1500) with { Resistances = Resistances.None with { Light = 20 } });
        var after = Numbers(Sheet(power: 10, critDamagePermille: 1500) with { Resistances = Resistances.None with { Light = 27 } });

        Assert.Empty(StatComparison.Between(before, after).Changes);
    }

    [Fact]
    public void Between_ResistanceChangeAcrossAPercent_IsListed()
    {
        var before = Numbers(Sheet(power: 10, critDamagePermille: 1500) with { Resistances = Resistances.None with { Light = 27 } });
        var after = Numbers(Sheet(power: 10, critDamagePermille: 1500) with { Resistances = Resistances.None with { Light = 30 } });

        Assert.Equal([StatKind.LightResistance], StatComparison.Between(before, after).Changes.Select(change => change.Kind));
    }

    private static StatComparison.Numbers Numbers(StatSheet sheet) => new(sheet, _stats);

    private static StatSheet Sheet(int power, int critDamagePermille) => new(
        new PrimaryStats(power, 0, 0, 0, 0), MaxHp: 100, Speed: 10, AccuracyPermille: 900, CritChancePermille: 50,
        critDamagePermille, DodgePermille: 0, CrowdControlResistancePermille: 0, DamagePermille: 1000, Resistances.None,
        StatusPermille: 1000, ExtraStatusTurns: 0);
}
