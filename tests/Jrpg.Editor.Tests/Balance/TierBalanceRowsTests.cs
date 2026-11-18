using Jrpg.Editor.Balance;

namespace Jrpg.Editor.Tests.Balance;

// The balance table's tier sheet (decisions 704, 992): each tiered effect record at the typed Powers with the typed
// modifier, by Core's own rule for a skill naming the record.
public sealed class TierBalanceRowsTests
{
    // The developer's sheet: Power 1 x a Low tier's 2 x (1 + 4.73) is 11.46, kept as 11.
    [Fact]
    public void Of_SheetExample_PowerOneLowWithFourHundredSeventyThreePercent()
    {
        var rows = TierBalanceRows.Of(BalanceFixtures.World, [1], modifierPermille: 4730);

        Assert.Equal([11], RowOf("fire_1", rows).Amounts);
    }

    // heal_1 gives 3 per Power: 30 at Power 10, the 473 % left out as the battle leaves it out of healing.
    [Fact]
    public void Of_HealRecordWithAModifier_ModifierIgnored()
    {
        var heal = RowOf("heal_1", TierBalanceRows.Of(BalanceFixtures.World, [10], modifierPermille: 4730));

        Assert.Equal((30, true), (Assert.Single(heal.Amounts), heal.IsHeal));
    }

    // The world lists heal_1 first.
    [Fact]
    public void Of_TwoTiers_InIdOrder()
    {
        var rows = TierBalanceRows.Of(BalanceFixtures.World, [1], modifierPermille: 0);

        Assert.Equal(["fire_1", "heal_1"], rows.Select(row => row.TierId));
    }

    [Fact]
    public void Of_DamageTier_KindLineNamesItsType()
    {
        var fire = RowOf("fire_1", TierBalanceRows.Of(BalanceFixtures.World, [1], modifierPermille: 0));

        Assert.Equal(("damage, fire", 2, false), (fire.KindLine, fire.PerPower, fire.IsHeal));
    }

    [Fact]
    public void Of_HealTier_KindLineAlone()
    {
        var heal = RowOf("heal_1", TierBalanceRows.Of(BalanceFixtures.World, [1], modifierPermille: 0));

        Assert.Equal("restoreHp", heal.KindLine);
    }

    // fire_1's 2 per Power at Powers 1, 10 and 50.
    [Fact]
    public void Of_ThreePowers_OneAmountEachInTheirOrder()
    {
        var fire = RowOf("fire_1", TierBalanceRows.Of(BalanceFixtures.World, [1, 10, 50], modifierPermille: 0));

        Assert.Equal([2, 20, 100], fire.Amounts);
    }

    private static TierBalanceRow RowOf(string tierId, IReadOnlyList<TierBalanceRow> rows) =>
        Assert.Single(rows, row => row.TierId == tierId);
}
