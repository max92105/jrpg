using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Sprites;

namespace Jrpg.Core.Tests.Sprites;

// A sequence compares by its cells in order (decision 769): the Character sprites page's changed-record check and the
// editor's frame cache compare whole sheets, each built with its own lists.
public sealed class FacingSequenceTests
{
    [Fact]
    public void Equals_SameCellsInAnotherList_IsEqual()
    {
        Assert.Equal(Sequence((0, 1), (2, 1)), Sequence((0, 1), (2, 1)));
    }

    [Fact]
    public void GetHashCode_SameCellsInAnotherList_IsEqual()
    {
        Assert.Equal(Sequence((0, 1), (2, 1)).GetHashCode(), Sequence((0, 1), (2, 1)).GetHashCode());
    }

    [Fact]
    public void Equals_SameCellsInAnotherOrder_Differs()
    {
        Assert.NotEqual(Sequence((0, 1), (2, 1)), Sequence((2, 1), (0, 1)));
    }

    [Fact]
    public void Equals_OtherMirror_Differs()
    {
        Assert.NotEqual(new FacingSequence { MirrorOf = Facing.Left }, new FacingSequence { MirrorOf = Facing.Right });
    }

    [Fact]
    public void FieldSpriteSheet_Equals_SameSequencesBuiltApart_IsEqual()
    {
        Assert.Equal(Sheet(), Sheet());
    }

    [Fact]
    public void FieldSpriteSheet_GetHashCode_SameSequencesBuiltApart_IsEqual()
    {
        Assert.Equal(Sheet().GetHashCode(), Sheet().GetHashCode());
    }

    private static FieldSpriteSheet Sheet() => new()
    {
        File = "npc.png",
        FrameWidth = 32,
        FrameHeight = 32,
        Scale = 3,
        Walk = new FacingCycle
        {
            Rows = new FacingRows { Down = 0, Up = 2, Left = 1, Right = 2 },
            Frames = new TileAnimation { FrameCount = 4, FrameMilliseconds = 150 },
            Sequences = new FacingSequences { Up = Sequence((0, 2), (1, 2), (0, 2)) },
        },
    };

    private static FacingSequence Sequence(params (int Column, int Row)[] cells) =>
        new() { Cells = [.. cells.Select(cell => new SpriteCell(cell.Column, cell.Row))] };
}
