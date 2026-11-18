using Jrpg.Editor.Balance;

namespace Jrpg.Editor.Tests.Balance;

// The balance table's class skill rows (decision 992): each class's damaging and healing skills at the table's levels,
// by Core's own rule for a member of the class at each level wearing nothing.
public sealed class ClassSkillBalanceRowsTests
{
    private static readonly int[] _startingLevels = [1, 10, 20];

    // The mage's Power is 12, 39 and 69 at levels 1, 10 and 20; fire_1 gives 2 per Power.
    [Fact]
    public void Of_TieredSkill_AmountsAtTheThreeLevels()
    {
        var fireball = RowOf("Fireball", ClassSkillBalanceRows.Of(BalanceFixtures.World, _startingLevels, modifierPermille: 0));

        Assert.Equal(("fire_1 x2", 1), (fireball.TierLine, fireball.Hits));
        Assert.Equal(["24", "78", "138"], fireball.AmountTexts);
    }

    [Fact]
    public void Of_FourHits_ShowsPerHitAndTotal()
    {
        var flurry = RowOf("Flurry", ClassSkillBalanceRows.Of(BalanceFixtures.World, _startingLevels, modifierPermille: 0));

        Assert.Equal(["24 x 4 = 96", "78 x 4 = 312", "138 x 4 = 552"], flurry.AmountTexts);
    }

    // The cleric's Power is 10, 28 and 48: 1 % each on 15 gives 16.5, 19.2 and 22.2, rounded down.
    [Fact]
    public void Of_FlatSkill_NamesItsPowerRaisedByDamagePercent()
    {
        var smite = RowOf("Smite", ClassSkillBalanceRows.Of(BalanceFixtures.World, _startingLevels, modifierPermille: 0));

        Assert.Equal("flat 15", smite.TierLine);
        Assert.Equal(["16", "19", "22"], smite.AmountTexts);
    }

    [Fact]
    public void Of_StatusOnlySkill_NotListed()
    {
        var rows = ClassSkillBalanceRows.Of(BalanceFixtures.World, _startingLevels, modifierPermille: 0);

        Assert.Equal(
            ["Fireball", "Flurry", "Nova"],
            rows.Where(row => row.ClassName == "Mage").Select(row => row.SkillName));
    }

    // The world lists the mage first.
    [Fact]
    public void Of_TwoClasses_InIdOrder()
    {
        var rows = ClassSkillBalanceRows.Of(BalanceFixtures.World, _startingLevels, modifierPermille: 0);

        Assert.Equal(["Cleric", "Mage"], rows.Select(row => row.ClassName).Distinct());
    }

    // Nova is learned at 5: Power 24 at level 5 and 27 at 6.
    [Fact]
    public void Of_SkillLearnedAtFive_DashBelowFiveOnly()
    {
        var nova = RowOf("Nova", ClassSkillBalanceRows.Of(BalanceFixtures.World, [4, 5, 6], modifierPermille: 0));

        Assert.Equal(["-", "48", "54"], nova.AmountTexts);
    }

    [Fact]
    public void Of_HealAndDamage_OnlyTheHealMarked()
    {
        var rows = ClassSkillBalanceRows.Of(BalanceFixtures.World, _startingLevels, modifierPermille: 0);

        Assert.Equal((true, false), (RowOf("Mend", rows).IsHeal, RowOf("Fireball", rows).IsHeal));
    }

    // Fireball's 24 x 0.75 = 18 at level 1; Mend keeps its 30 = 10 x 3.
    [Fact]
    public void Of_MinusTwentyFivePercent_LowersDamageNotHeals()
    {
        var rows = ClassSkillBalanceRows.Of(BalanceFixtures.World, _startingLevels, modifierPermille: -250);

        Assert.Equal(("18", "30"), (RowOf("Fireball", rows).AmountTexts[0], RowOf("Mend", rows).AmountTexts[0]));
    }

    private static SkillBalanceRow RowOf(string skillName, IReadOnlyList<SkillBalanceRow> rows) =>
        rows.Single(row => row.SkillName == skillName);
}
