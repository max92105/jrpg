using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Sprites;
using Jrpg.Game.Exploration;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Exploration;

public sealed class FieldSpriteFrameTests
{
    // The player's record (decision 561): the rectangles must be the ones the fixed layout gave before ADR 0043.
    private static readonly FieldSpriteSheet _player = Sheet(48, 48, new FacingRows { Down = 0, Left = 1, Right = 2, Up = 3 });

    [Theory]
    [InlineData(Facing.Down, 0, 0, 0)]
    [InlineData(Facing.Left, 0, 0, 48)]
    [InlineData(Facing.Right, 7, 336, 96)]
    [InlineData(Facing.Up, 2, 96, 144)]
    public void SourceOf_PlayerSheetCell_MatchesTodaysRectangle(Facing facing, int column, int left, int top)
    {
        var cell = new SpriteCell(column, _player.Walk.Rows.RowOf(facing));

        Assert.Equal(new Rectangle(left, top, 48, 48), FieldSpriteFrame.SourceOf(_player, cell));
    }

    [Fact]
    public void SourceOf_NonSquareFrames_UsesWidthAndHeight()
    {
        var tall = Sheet(16, 24, new FacingRows { Down = 2, Left = 0, Right = 1, Up = 3 });

        Assert.Equal(new Rectangle(32, 48, 16, 24), FieldSpriteFrame.SourceOf(tall, new SpriteCell(2, 2)));
    }

    private static FieldSpriteSheet Sheet(int width, int height, FacingRows rows) => new()
    {
        File = "sheet.png",
        FrameWidth = width,
        FrameHeight = height,
        Scale = 1,
        Walk = new FacingCycle { Rows = rows, Frames = new TileAnimation { FrameCount = 8, FrameMilliseconds = 100 } },
    };
}
