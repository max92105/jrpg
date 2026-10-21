using Jrpg.Core.Maps;

namespace Jrpg.Core.Tests.Maps;

public sealed class TilesheetDefinitionTests
{
    private static readonly TilesheetDefinition _sheet = new()
    {
        Id = "forest",
        File = "Forest.png",
        TileWidth = 16,
        TileHeight = 24,
        Margin = 2,
        Spacing = 1,
    };

    [Fact]
    public void CellLeft_WithMarginAndSpacing_SkipsBoth()
    {
        Assert.Equal(2 + (3 * 17), _sheet.CellLeft(3));
    }

    [Fact]
    public void CellTop_WithMarginAndSpacing_SkipsBoth()
    {
        Assert.Equal(2 + (2 * 25), _sheet.CellTop(2));
    }

    // Column 3 ends at 2 + 3 * 17 + 16 = 69: an image 69 wide holds 4 whole columns, one a pixel narrower only 3.
    [Theory]
    [InlineData(69, 4)]
    [InlineData(68, 3)]
    [InlineData(1, 0)]
    public void ColumnsIn_MarginAndSpacing_CountsWholeCells(int imageWidth, int expected)
    {
        Assert.Equal(expected, _sheet.ColumnsIn(imageWidth));
    }

    [Fact]
    public void ColumnsIn_ImageNarrowerThanItsMargin_IsZero()
    {
        Assert.Equal(0, (_sheet with { Margin = 40 }).ColumnsIn(10));
    }

    // Row 2 ends at 2 + 2 * 25 + 24 = 76.
    [Theory]
    [InlineData(76, 3)]
    [InlineData(75, 2)]
    public void RowsIn_PartialLastRow_IsNotCounted(int imageHeight, int expected)
    {
        Assert.Equal(expected, _sheet.RowsIn(imageHeight));
    }

    [Fact]
    public void CellLeft_FirstColumn_IsTheMargin()
    {
        Assert.Equal(2, _sheet.CellLeft(0));
    }
}
