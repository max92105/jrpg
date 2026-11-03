using Jrpg.Core.Maps;

namespace Jrpg.Core.Tests.Maps;

public sealed class TileDefinitionTests
{
    private static readonly TileDefinition _still = new() { Id = 1, IsWalkable = true, SheetId = "water", Column = 5, Row = 2 };

    [Fact]
    public void ColumnShownAt_AnimatedTileAtTheThirdFrame_IsTwoColumnsRight()
    {
        var animated = _still with { Animation = new TileAnimation { FrameCount = 4, FrameMilliseconds = 100 } };

        Assert.Equal(7, animated.ColumnShownAt(TimeSpan.FromMilliseconds(250)));
    }

    [Fact]
    public void ColumnShownAt_StillTile_IsItsColumn() => Assert.Equal(5, _still.ColumnShownAt(TimeSpan.FromMilliseconds(250)));
}
