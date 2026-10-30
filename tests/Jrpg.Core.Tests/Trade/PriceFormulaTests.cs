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

    // Summed in a long: two values near the cap don't wrap around.
    [Fact]
    public void PieceValue_PastMaxGold_IsMaxGold()
    {
        Assert.Equal(Purse.MaxGold, PriceFormula.PieceValue(Purse.MaxGold, [Purse.MaxGold, Purse.MaxGold]));
    }
}
