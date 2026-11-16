using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Sprites;
using Jrpg.Editor.Sprites;

namespace Jrpg.Editor.Tests.Sprites;

// A walk as the Character sprites page types it (ADR 0054): read from a record and given back whole, its frames checked
// against the columns only while a facing reads its row (decision 767).
public sealed class FacingCycleSetupTests
{
    // Down on row 0, Up a sequence over row 5, Left on row 1, Right mirroring Left over row 2.
    private static readonly FacingCycle _mixed = new()
    {
        Rows = new FacingRows { Down = 0, Up = 5, Left = 1, Right = 2 },
        Frames = new TileAnimation { FrameCount = 4, FrameMilliseconds = 150 },
        Sequences = new FacingSequences
        {
            Up = new FacingSequence { Cells = [new SpriteCell(0, 3), new SpriteCell(1, 3), new SpriteCell(0, 3)] },
            Right = new FacingSequence { MirrorOf = Facing.Left },
        },
    };

    [Fact]
    public void CopyFrom_RecordWithSequenceAndMirror_ToCycleGivesItBack()
    {
        var walk = new FacingCycleSetup();

        walk.CopyFrom(_mixed);

        Assert.Equal(_mixed, walk.ToCycle());
    }

    [Fact]
    public void ToCycle_RowRecord_HasNoSequences()
    {
        var walk = new FacingCycleSetup();

        walk.CopyFrom(_mixed with { Sequences = null });

        Assert.Null(walk.ToCycle().Sequences);
    }

    [Fact]
    public void ProblemWithin_NoFacingOnRow_IgnoresFrames()
    {
        var walk = EveryFacingOnCells();
        walk.Frames = 8;

        Assert.Null(walk.ProblemWithin(4, 4));
    }

    [Fact]
    public void ProblemWithin_OneFacingOnRow_ChecksFrames()
    {
        var walk = EveryFacingOnCells();
        walk.Frames = 8;
        walk.Down.Source = FacingSource.Row;

        Assert.Equal("The walk's 8 frames don't fit in 4 columns.", walk.ProblemWithin(4, 4));
    }

    [Fact]
    public void Changed_FacingCells_RaisesItsFacing()
    {
        var walk = new FacingCycleSetup();
        var raised = new List<string?>();
        walk.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        walk.Up.Cells = [new SpriteCell(1, 1)];

        Assert.Contains(nameof(FacingCycleSetup.Up), raised);
    }

    // Decision 771: Left reads 4 frames on row 1, so its sequence starts as that run.
    [Fact]
    public void Source_RowToCells_StartsWithTheRowsRun()
    {
        var walk = Mixed();

        walk.Left.Source = FacingSource.Cells;

        Assert.Equal([new SpriteCell(0, 1), new SpriteCell(1, 1), new SpriteCell(2, 1), new SpriteCell(3, 1)], walk.Left.Cells);
    }

    [Fact]
    public void Source_RowToCellsAgain_KeepsThePickedCells()
    {
        var walk = Mixed();
        walk.Left.Source = FacingSource.Cells;
        walk.Left.Cells = [new SpriteCell(2, 0)];
        walk.Left.Source = FacingSource.Row;

        walk.Left.Source = FacingSource.Cells;

        Assert.Equal([new SpriteCell(2, 0)], walk.Left.Cells);
    }

    [Fact]
    public void ToCycle_CellsBackToRow_GivesTheRowKept()
    {
        var walk = Mixed();

        walk.Up.Source = FacingSource.Row;

        Assert.Equal((5, null), (walk.ToCycle().Rows.Up, walk.ToCycle().Sequences!.Up));
    }

    [Fact]
    public void IsAnyFacingOnRow_EveryFacingOffRow_IsFalse()
    {
        Assert.False(EveryFacingOnCells().IsAnyFacingOnRow);
    }

    [Fact]
    public void IsAnyFacingOnRow_OneFacingOnRow_IsTrue()
    {
        var walk = EveryFacingOnCells();

        walk.Left.Source = FacingSource.Row;

        Assert.True(walk.IsAnyFacingOnRow);
    }

    private static FacingCycleSetup Mixed()
    {
        var walk = new FacingCycleSetup();
        walk.CopyFrom(_mixed);
        return walk;
    }

    private static FacingCycleSetup EveryFacingOnCells()
    {
        var walk = new FacingCycleSetup();
        walk.CopyFrom(_mixed);
        foreach (var facing in new[] { walk.Down, walk.Up, walk.Left, walk.Right })
        {
            facing.Source = FacingSource.Cells;
            facing.Cells = [new SpriteCell(0, 0)];
        }

        return walk;
    }
}
