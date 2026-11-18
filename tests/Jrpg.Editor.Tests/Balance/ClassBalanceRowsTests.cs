using Jrpg.Editor.Balance;

namespace Jrpg.Editor.Tests.Balance;

// The balance table's class rows (decision 992): each class's max HP and Power at the table's levels, by Core's own
// rule for a member of the class at each level wearing nothing.
public sealed class ClassBalanceRowsTests
{
    private static readonly int[] _startingLevels = [1, 10, 20];

    // The mage's class HP is 100, 190 and 290, raised 1 % a Constitution point (5, 14, 24): 105, 216.6 and 359.6,
    // rounded down; its Power is 12, 39 and 69.
    [Fact]
    public void Of_StartingLevels_HpAndPowerOfTheClassAtEach()
    {
        var rows = ClassBalanceRows.Of(BalanceFixtures.World, _startingLevels);

        Assert.Equal(["105 / 12", "216 / 39", "359 / 69"], rows.Single(row => row.ClassName == "Mage").HpAndPowerTexts);
    }

    // The world lists the mage first.
    [Fact]
    public void Of_TwoClasses_InIdOrder()
    {
        var rows = ClassBalanceRows.Of(BalanceFixtures.World, _startingLevels);

        Assert.Equal(["Cleric", "Mage"], rows.Select(row => row.ClassName));
    }
}
