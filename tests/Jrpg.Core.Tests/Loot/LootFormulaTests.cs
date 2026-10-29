using Jrpg.Core.Items;
using Jrpg.Core.Loot;

namespace Jrpg.Core.Tests.Loot;

public sealed class LootFormulaTests
{
    [Fact]
    public void ChancePermille_ThirtyPercentWithFiftyLuck_Is450()
    {
        Assert.Equal(450, LootFormula.ChancePermille(30, 500));
    }

    [Fact]
    public void ChancePermille_NoLuck_IsTheChanceInPermille()
    {
        Assert.Equal(300, LootFormula.ChancePermille(30, 0));
    }

    // 80 % raised by half is 120 %: held at a certain drop.
    [Fact]
    public void ChancePermille_HighChanceWithLuck_CappedAtWhole()
    {
        Assert.Equal(1000, LootFormula.ChancePermille(80, 500));
    }

    [Fact]
    public void RarityWeight_Common_Unchanged()
    {
        Assert.Equal(650, LootFormula.RarityWeight(650, Rarity.Common, 500));
    }

    [Fact]
    public void RarityWeight_RareWithFiftyPercentLuck_IsHalfAgainAsMuch()
    {
        Assert.Equal(105, LootFormula.RarityWeight(70, Rarity.Rare, 500));
    }

    // 280 × 1.25 is 350; 3 × 1.25 is 3.75, rounded down.
    [Theory]
    [InlineData(280, 350)]
    [InlineData(3, 3)]
    public void RarityWeight_Magic_RaisedAndRoundedDown(int weight, int expected)
    {
        Assert.Equal(expected, LootFormula.RarityWeight(weight, Rarity.Magic, 250));
    }
}
