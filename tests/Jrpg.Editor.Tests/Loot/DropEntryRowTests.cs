using Jrpg.Core.Loot;
using Jrpg.Editor.Loot;

namespace Jrpg.Editor.Tests.Loot;

// One drop entry as typed (decision 597): its boxes kept as text, the problem only a box that isn't a whole number, in
// the row's order; the ranges are Data's.
public sealed class DropEntryRowTests
{
    private static readonly DropEntry _scrap = new("scrap", 20) { MaxQuantity = 2 };

    [Fact]
    public void From_ScrapEntry_ShowsItsNumbersAsTyped()
    {
        var row = DropEntryRow.From(_scrap, "Scrap");

        Assert.Equal(("scrap", "Scrap (scrap)"), (row.ItemId, row.ItemLabel));
        Assert.Equal(("20", "1", "2"), (row.ChanceText, row.MinQuantityText, row.MaxQuantityText));
    }

    [Fact]
    public void Problem_ChanceNotANumber_NamesChance()
    {
        var row = DropEntryRow.From(_scrap, "Scrap");

        row.ChanceText = "lots";
        row.MaxQuantityText = "";

        Assert.Equal("Chance must be a whole number (was 'lots').", row.Problem);
    }

    [Fact]
    public void Problem_MaxNotANumber_NamesMax()
    {
        var row = DropEntryRow.From(_scrap, "Scrap");

        row.MaxQuantityText = "two";

        Assert.Equal("Max must be a whole number (was 'two').", row.Problem);
    }

    // Data refuses a chance of 0; the row only checks the box's shape.
    [Fact]
    public void Problem_WholeNumbers_IsNull()
    {
        var row = DropEntryRow.From(_scrap, "Scrap");

        row.ChanceText = "0";

        Assert.Null(row.Problem);
    }

    [Fact]
    public void ToEntry_TypedQuantities_BuildsThem()
    {
        var row = DropEntryRow.From(_scrap, "Scrap");

        row.MinQuantityText = " 2 ";
        row.MaxQuantityText = "3";

        Assert.Equal(new DropEntry("scrap", 20) { MinQuantity = 2, MaxQuantity = 3 }, row.ToEntry());
    }

    [Fact]
    public void ToEntry_ProblemShown_Throws()
    {
        var row = DropEntryRow.From(_scrap, "Scrap");

        row.MinQuantityText = "x";

        Assert.Throws<InvalidOperationException>(row.ToEntry);
    }
}
