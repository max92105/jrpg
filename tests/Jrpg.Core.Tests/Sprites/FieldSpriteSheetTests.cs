using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Sprites;

namespace Jrpg.Core.Tests.Sprites;

public sealed class FieldSpriteSheetTests
{
    private static readonly FieldSpriteSheet _player = Sheet(new FacingRows { Down = 0, Left = 1, Right = 2, Up = 3 }, frames: 8);

    // A bought 32 px sheet drawn at 3x: a 96 px frame over a 48 px cell.
    private static readonly FieldSpriteSheet _bought = _player with { FrameWidth = 32, FrameHeight = 32, Scale = 3 };

    // The player sheet today (decision 561): 48 px frames at scale 1 fill the 48 px cell exactly.
    [Fact]
    public void AreaOver_PlayerSheet_FillsTheCell() => Assert.Equal(new SpriteArea(0, 0, 48, 48), _player.AreaOver(48));

    // A bought 32 px frame at scale 3 is 96 px: it spills 24 px past each side and rises 48 px above the cell.
    [Fact]
    public void AreaOver_ScaledTallFrame_StandsOnTheCellsBottom() =>
        Assert.Equal(new SpriteArea(-24, -48, 96, 96), _bought.AreaOver(48));

    [Fact]
    public void AreaOver_FeetPixels_LowersTheSprite() =>
        Assert.Equal(new SpriteArea(-24, -42, 96, 96), (_bought with { FeetPixels = 2 }).AreaOver(48));

    [Theory]
    [InlineData(0, 0)]
    [InlineData(99, 0)]
    [InlineData(100, 1)]
    [InlineData(799, 7)]
    [InlineData(800, 0)]
    public void WalkCellAt_PlayerSheet_MatchesTodaysFrames(int walkedMilliseconds, int column) =>
        Assert.Equal(new SpriteCell(column, 2), _player.WalkCellAt(Facing.Right, TimeSpan.FromMilliseconds(walkedMilliseconds)));

    [Theory]
    [InlineData(Facing.Down, 0)]
    [InlineData(Facing.Left, 1)]
    [InlineData(Facing.Right, 2)]
    [InlineData(Facing.Up, 3)]
    public void StandingCell_EachFacing_IsColumnZeroOfItsRow(Facing facing, int row) =>
        Assert.Equal(new SpriteCell(0, row), _player.StandingCell(facing));

    [Fact]
    public void ColumnsNeeded_EightFrameWalk_IsEight() => Assert.Equal(8, _player.ColumnsNeeded);

    [Fact]
    public void RowsNeeded_RowsZeroToThree_IsFour() => Assert.Equal(4, _player.RowsNeeded);

    // A sheet drawn with two facings reads one row twice; the highest row named counts, wherever it is.
    [Fact]
    public void RowsNeeded_RepeatedRow_CountsTheHighest() =>
        Assert.Equal(6, Sheet(new FacingRows { Down = 5, Left = 1, Right = 1, Up = 0 }, frames: 4).RowsNeeded);

    // A row typed at the int limit must not wrap to a count the smallest image holds.
    [Fact]
    public void RowsNeeded_RowAtIntLimit_DoesNotWrap()
    {
        var sheet = Sheet(new FacingRows { Down = 0, Left = 0, Right = 0, Up = int.MaxValue }, frames: 2);

        Assert.Equal(2147483648L, sheet.RowsNeeded);
    }

    private static FieldSpriteSheet Sheet(FacingRows rows, int frames) => new()
    {
        File = "sheet.png",
        FrameWidth = 48,
        FrameHeight = 48,
        Scale = 1,
        Walk = new FacingCycle { Rows = rows, Frames = new TileAnimation { FrameCount = frames, FrameMilliseconds = 100 } },
    };
}
