using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Sprites;

namespace Jrpg.Core.Tests.Sprites;

// A facing's sequence of cells overriding its row (ADR 0054): its cells in order at the walk's frame time, looping over
// its own count, and what the image must hold. Over a sheet whose facings read rows 0 to 3, 4 frames of 100 ms.
public sealed class FieldSpriteSheetSequenceTests
{
    private static readonly FieldSpriteSheet _rows = new()
    {
        File = "sheet.png",
        FrameWidth = 32,
        FrameHeight = 32,
        Scale = 3,
        Walk = new FacingCycle
        {
            Rows = new FacingRows { Down = 0, Up = 1, Left = 2, Right = 3 },
            Frames = new TileAnimation { FrameCount = 4, FrameMilliseconds = 100 },
        },
    };

    [Fact]
    public void StandingCell_FacingWithSequence_IsItsFirstCell()
    {
        var sheet = WithSequences(new FacingSequences { Up = Sequence((2, 5), (3, 5)) });

        Assert.Equal(new SpriteCell(2, 5), sheet.StandingCell(Facing.Up));
    }

    [Theory]
    [InlineData(0, 2)]
    [InlineData(100, 0)]
    [InlineData(200, 3)]
    public void WalkCellAt_Sequence_PlaysItsCellsInOrder(int walkedMilliseconds, int column)
    {
        var sheet = WithSequences(new FacingSequences { Up = Sequence((2, 5), (0, 5), (3, 5)) });

        Assert.Equal(new SpriteCell(column, 5), sheet.WalkCellAt(Facing.Up, TimeSpan.FromMilliseconds(walkedMilliseconds)));
    }

    [Fact]
    public void WalkCellAt_SequenceRepeatingACell_ShowsItAgainInTurn()
    {
        var sheet = WithSequences(new FacingSequences { Down = Sequence((0, 4), (1, 4), (2, 4), (1, 4)) });

        Assert.Equal(new SpriteCell(1, 4), sheet.WalkCellAt(Facing.Down, TimeSpan.FromMilliseconds(300)));
    }

    // Three cells against four-frame rows: the sequence loops over its own count.
    [Fact]
    public void WalkCellAt_AfterTheSequencesLastCell_LoopsToItsFirst()
    {
        var sheet = WithSequences(new FacingSequences { Up = Sequence((5, 6), (6, 6), (7, 6)) });
        var walked = TimeSpan.FromMilliseconds(300);

        Assert.Equal(new SpriteCell(5, 6), sheet.WalkCellAt(Facing.Up, walked));
        Assert.Equal(new SpriteCell(3, 2), sheet.WalkCellAt(Facing.Left, walked));
    }

    [Fact]
    public void WalkCellAt_FacingWithoutSequence_StillReadsItsRow()
    {
        var sheet = WithSequences(new FacingSequences { Up = Sequence((5, 6)) });

        Assert.Equal(new SpriteCell(1, 2), sheet.WalkCellAt(Facing.Left, TimeSpan.FromMilliseconds(100)));
    }

    [Fact]
    public void ColumnsNeeded_SequenceCellPastTheRowRun_CountsIt()
    {
        var sheet = WithSequences(new FacingSequences { Up = Sequence((6, 1)) });

        Assert.Equal(7, sheet.ColumnsNeeded);
    }

    // Nothing reads the frame count once every facing has a sequence (decision 767).
    [Fact]
    public void ColumnsNeeded_EveryFacingOnASequence_IgnoresFrameCount()
    {
        var everyFacing = Sequence((0, 0), (2, 0));
        var sheet = WithSequences(
            new FacingSequences { Down = everyFacing, Up = everyFacing, Left = everyFacing, Right = everyFacing });
        var eightFrames = sheet.Walk.Frames with { FrameCount = 8 };

        Assert.Equal(3, (sheet with { Walk = sheet.Walk with { Frames = eightFrames } }).ColumnsNeeded);
    }

    [Fact]
    public void RowsNeeded_SequenceCellBelowTheRows_CountsIt()
    {
        var sheet = WithSequences(new FacingSequences { Up = Sequence((0, 7)) });

        Assert.Equal(8, sheet.RowsNeeded);
    }

    [Fact]
    public void RowsNeeded_RowUnderASequence_IsNotCounted()
    {
        var rows = new FacingRows { Down = 0, Up = 9, Left = 1, Right = 2 };
        var sheet = WithSequences(new FacingSequences { Up = Sequence((0, 1), (1, 1)) });

        Assert.Equal(3, (sheet with { Walk = sheet.Walk with { Rows = rows } }).RowsNeeded);
    }

    [Fact]
    public void ColumnsNeeded_CellAtIntMaxColumn_DoesNotWrap()
    {
        var sheet = WithSequences(new FacingSequences { Up = Sequence((int.MaxValue, 0)) });

        Assert.Equal(2147483648L, sheet.ColumnsNeeded);
    }

    [Fact]
    public void RowsNeeded_CellAtIntMaxRow_DoesNotWrap()
    {
        var sheet = WithSequences(new FacingSequences { Up = Sequence((0, int.MaxValue)) });

        Assert.Equal(2147483648L, sheet.RowsNeeded);
    }

    private static FieldSpriteSheet WithSequences(FacingSequences sequences) =>
        _rows with { Walk = _rows.Walk with { Sequences = sequences } };

    private static FacingSequence Sequence(params (int Column, int Row)[] cells) =>
        new() { Cells = [.. cells.Select(cell => new SpriteCell(cell.Column, cell.Row))] };
}
