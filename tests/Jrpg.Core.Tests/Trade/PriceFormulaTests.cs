using Jrpg.Core.Party;
using Jrpg.Core.Trade;

namespace Jrpg.Core.Tests.Trade;

public sealed class PriceFormulaTests
{
    [Fact]
    public void PieceValue_BaseAndAffixes_AddsThem()
    {
        Assert.Equal(145, PriceFormula.PieceValue(100, [20, 25]));
    }

    [Fact]
    public void PieceValue_NoAffixes_IsTheBase()
    {
        Assert.Equal(100, PriceFormula.PieceValue(100, []));
    }

    [Fact]
    public void SalePrice_HalfOfOddPrice_RoundsDown()
    {
        Assert.Equal(25, PriceFormula.SalePrice(51, 50));
    }

    [Fact]
    public void SalePrice_ZeroPercent_IsZero()
    {
        Assert.Equal(0, PriceFormula.SalePrice(51, 0));
    }

    [Fact]
    public void Total_PastMaxGold_IsMaxGold()
    {
        Assert.Equal(Purse.MaxGold, PriceFormula.Total(Purse.MaxGold, 99));
    }

    // The product is a long: the top price at the top share doesn't wrap around.
    [Fact]
    public void SalePrice_MaxGoldAtFullShare_IsMaxGold()
    {
        Assert.Equal(Purse.MaxGold, PriceFormula.SalePrice(Purse.MaxGold, 100));
    }

    // Summed in a long: two values near the cap don't wrap around.
    [Fact]
    public void PieceValue_PastMaxGold_IsMaxGold()
    {
        Assert.Equal(Purse.MaxGold, PriceFormula.PieceValue(Purse.MaxGold, [Purse.MaxGold, Purse.MaxGold]));
    }
}
