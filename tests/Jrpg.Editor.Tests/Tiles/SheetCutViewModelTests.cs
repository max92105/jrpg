using Jrpg.Core.Maps;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Tests.Tiles;

public sealed class SheetCutViewModelTests
{
    // A 96 x 64 image cut into 32 px tiles: 3 columns, 2 rows.
    private static readonly TilesheetDefinition _sheet = new() { Id = "grass_3", File = "Grass3.png", TileWidth = 32, TileHeight = 32, Margin = 0, Spacing = 0 };

    private readonly SheetCutViewModel _cut = new(_sheet, (96, 64));

    [Fact]
    public void Constructor_SheetCut_ShowsItsColumnsAndRows()
    {
        Assert.Equal((3, 2), (_cut.Columns, _cut.Rows));
        Assert.Equal("32 x 32 px; 3 x 2 = 6 cells", _cut.Summary);
    }

    [Fact]
    public void Columns_ImageSplitsEvenly_SetsTileWidth()
    {
        _cut.Columns = 2;

        Assert.Equal(48, _cut.TileWidth);
        Assert.Null(_cut.Problem);
    }

    [Fact]
    public void Columns_ImageDoesNotSplit_ReportsProblem()
    {
        _cut.Columns = 5;

        Assert.Equal("96 px doesn't split into 5 columns.", _cut.Problem);
        Assert.Equal("", _cut.Summary);
    }

    [Fact]
    public void Columns_TilesUnderOnePixel_ReportsProblemAndHasNoPreview()
    {
        _cut.Columns = 97;

        Assert.Equal("96 px is too small for 97 columns.", _cut.Problem);
        Assert.Null(_cut.PreviewSheet);
    }

    // 96 - 2 * 2 = 92 px across 2 columns, 64 - 4 = 60 px down 2 rows.
    [Fact]
    public void Margin_Set_RecomputesTileSize()
    {
        _cut.Columns = 2;

        _cut.Margin = 2;

        Assert.Equal((46, 30), (_cut.TileWidth, _cut.TileHeight));
        Assert.Equal(_sheet with { TileWidth = 46, TileHeight = 30, Margin = 2 }, _cut.PreviewSheet);
    }

    [Fact]
    public void ToSheet_ColumnsChanged_KeepsIdAndFile()
    {
        _cut.Columns = 2;

        Assert.Equal(_sheet with { TileWidth = 48 }, _cut.ToSheet());
    }

    // 96 px in 8 columns past a 12 px margin: 9 px tiles, but 12 + 9 * 9 = 93 fits a ninth whole column.
    [Fact]
    public void Margin_WideAsACell_ReportsProblem()
    {
        _cut.Columns = 8;

        _cut.Margin = 12;

        Assert.Equal("A 12 px margin leaves room for another cell; make it smaller than a cell.", _cut.Problem);
    }

    [Theory]
    [InlineData(3, 3, 0, 0, "64 px doesn't split into 3 rows.")]
    [InlineData(0, 2, 0, 0, "Columns and rows must be at least 1.")]
    [InlineData(3, 2, -1, 0, "Margin and spacing must not be negative.")]
    public void Problem_FieldsOutOfRange_SaysWhy(int columns, int rows, int margin, int spacing, string expected)
    {
        _cut.Columns = columns;
        _cut.Rows = rows;
        _cut.Margin = margin;
        _cut.Spacing = spacing;

        Assert.Equal(expected, _cut.Problem);
    }

    [Fact]
    public void ToSheet_CutWithProblem_ThrowsWithIt()
    {
        _cut.Columns = 5;

        var exception = Assert.Throws<InvalidOperationException>(_cut.ToSheet);

        Assert.Equal("96 px doesn't split into 5 columns.", exception.Message);
    }
}
