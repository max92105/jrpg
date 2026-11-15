using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Sprites;

namespace Jrpg.Core.Tests.Sprites;

// A facing that mirrors another (decision 766): the mirrored facing's frame at the same time, flipped. Over a sheet
// whose facings read rows 0 to 3, 4 frames of 100 ms, Right mirroring Left (row 1).
public sealed class FieldSpriteSheetMirrorTests
{
    private static readonly FieldSpriteSheet _rows = new()
    {
        File = "sheet.png",
        FrameWidth = 32,
        FrameHeight = 32,
        Scale = 3,
        Walk = new FacingCycle
        {
            Rows = new FacingRows { Down = 0, Left = 1, Right = 2, Up = 3 },
            Frames = new TileAnimation { FrameCount = 4, FrameMilliseconds = 100 },
        },
    };

    private static readonly FieldSpriteSheet _rightMirrorsLeft =
        WithSequences(new FacingSequences { Right = new FacingSequence { MirrorOf = Facing.Left } });

    [Fact]
    public void StandingFrame_MirrorFacing_IsTheMirroredFacingsFirstFlipped()
    {
        Assert.Equal(new SpriteFrame(new SpriteCell(0, 1), IsMirrored: true), _rightMirrorsLeft.StandingFrame(Facing.Right));
    }

    [Fact]
    public void WalkFrameAt_MirrorFacing_FollowsTheMirroredFacingsFrame()
    {
        var walked = TimeSpan.FromMilliseconds(200);

        var frame = _rightMirrorsLeft.WalkFrameAt(Facing.Right, walked);

        Assert.Equal(new SpriteFrame(new SpriteCell(2, 1), IsMirrored: true), frame);
    }

    // Left's sequence of three cells: the mirror loops over its count, not the rows' four frames.
    [Fact]
    public void WalkFrameAt_MirrorOfASequence_PlaysItsCellsFlipped()
    {
        var left = new FacingSequence { Cells = [new SpriteCell(4, 6), new SpriteCell(5, 6), new SpriteCell(6, 6)] };
        var sheet = WithSequences(new FacingSequences { Left = left, Right = new FacingSequence { MirrorOf = Facing.Left } });

        var frame = sheet.WalkFrameAt(Facing.Right, TimeSpan.FromMilliseconds(300));

        Assert.Equal(new SpriteFrame(new SpriteCell(4, 6), IsMirrored: true), frame);
    }

    [Fact]
    public void WalkFrameAt_RowFacing_IsNotMirrored()
    {
        Assert.False(_rightMirrorsLeft.WalkFrameAt(Facing.Left, TimeSpan.FromMilliseconds(200)).IsMirrored);
    }

    // Right's own row 9 is unread under its mirror: the rows read are 0, 1 and 3.
    [Fact]
    public void RowsNeeded_MirrorFacingsOwnRow_IsNotCounted()
    {
        var rows = _rightMirrorsLeft.Walk.Rows with { Right = 9 };

        Assert.Equal(4, (_rightMirrorsLeft with { Walk = _rightMirrorsLeft.Walk with { Rows = rows } }).RowsNeeded);
    }

    private static FieldSpriteSheet WithSequences(FacingSequences sequences) =>
        _rows with { Walk = _rows.Walk with { Sequences = sequences } };
}
