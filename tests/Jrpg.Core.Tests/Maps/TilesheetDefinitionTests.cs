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

    [Fact]
    public void CellLeft_FirstColumn_IsTheMargin()
    {
        Assert.Equal(2, _sheet.CellLeft(0));
    }
}
