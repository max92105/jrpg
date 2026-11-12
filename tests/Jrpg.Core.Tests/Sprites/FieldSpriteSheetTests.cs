using Jrpg.Core.Maps;
using Jrpg.Core.Sprites;

namespace Jrpg.Core.Tests.Sprites;

public sealed class FieldSpriteSheetTests
{
    private static readonly FieldSpriteSheet _player = Sheet(new FacingRows { Down = 0, Left = 1, Right = 2, Up = 3 }, frames: 8);

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
