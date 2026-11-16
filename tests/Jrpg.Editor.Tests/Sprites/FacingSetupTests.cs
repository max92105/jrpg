using Jrpg.Core.Geometry;
using Jrpg.Core.Sprites;
using Jrpg.Editor.Sprites;

namespace Jrpg.Editor.Tests.Sprites;

// One facing as the Character sprites window types it (ADR 0054): what it gives the record, and the window's own
// problems against a 4 x 4 cut (decision 778), counting only what the facing reads.
public sealed class FacingSetupTests
{
    [Fact]
    public void ToSequence_CellsSource_GivesItsCellsInOrder()
    {
        var up = Up(FacingSource.Cells, new SpriteCell(2, 1), new SpriteCell(0, 1));

        Assert.Equal(new FacingSequence { Cells = [new SpriteCell(2, 1), new SpriteCell(0, 1)] }, up.ToSequence());
    }

    [Fact]
    public void ToSequence_MirrorSource_NamesTheFacing()
    {
        var up = Up(FacingSource.Mirror);
        up.MirrorOf = Facing.Left;

        Assert.Equal(new FacingSequence { MirrorOf = Facing.Left }, up.ToSequence());
    }

    // The cells are kept while the row is read, and given to no record.
    [Fact]
    public void ToSequence_RowSource_IsNull()
    {
        Assert.Null(Up(FacingSource.Row, new SpriteCell(2, 1)).ToSequence());
    }

    [Fact]
    public void ProblemWithin_CellsWithoutCell_NamesTheFacing()
    {
        Assert.Equal("Up has no cells: pick them on the image.", Up(FacingSource.Cells).ProblemWithin(4, 4));
    }

    [Fact]
    public void ProblemWithin_CellPastTheCut_NamesItsPlaceFromOne()
    {
        var up = Up(FacingSource.Cells, new SpriteCell(0, 1), new SpriteCell(1, 1), new SpriteCell(4, 1));

        Assert.Equal("Up's cell 3 (5, 2) is outside the 4 x 4 frames.", up.ProblemWithin(4, 4));
    }

    [Fact]
    public void ProblemWithin_CellOnTheLastColumnAndRow_IsNull()
    {
        Assert.Null(Up(FacingSource.Cells, new SpriteCell(3, 3)).ProblemWithin(4, 4));
    }

    [Fact]
    public void ProblemWithin_RowPastTheRowsUnderASequence_IsNull()
    {
        var up = Up(FacingSource.Cells, new SpriteCell(0, 0));
        up.Row = 9;

        Assert.Null(up.ProblemWithin(4, 4));
    }

    [Fact]
    public void ProblemWithin_RowPastTheRowsOnRow_NamesIt()
    {
        var up = Up(FacingSource.Row);
        up.Row = 4;

        Assert.Equal("The up row must be within 1..4 (was 5).", up.ProblemWithin(4, 4));
    }

    [Fact]
    public void MirrorChoices_Left_ListsTheOtherThree()
    {
        Assert.Equal([Facing.Down, Facing.Up, Facing.Right], new FacingSetup(Facing.Left).MirrorChoices);
    }

    [Fact]
    public void CellsText_ThreeCells_NumbersFromOne()
    {
        var up = Up(FacingSource.Cells, new SpriteCell(0, 2), new SpriteCell(1, 2), new SpriteCell(0, 2));

        Assert.Equal("3 cells: (1, 3) (2, 3) (1, 3)", up.CellsText);
    }

    [Fact]
    public void Source_ChangedToMirror_RaisesIsMirror()
    {
        var up = Up(FacingSource.Row);
        var raised = new List<string?>();
        up.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        up.Source = FacingSource.Mirror;

        Assert.Contains(nameof(FacingSetup.IsMirror), raised);
    }

    [Fact]
    public void CellsText_CellAdded_IsRaised()
    {
        var up = Up(FacingSource.Cells);
        var raised = new List<string?>();
        up.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        up.AddCell(new SpriteCell(1, 1));

        Assert.Contains(nameof(FacingSetup.CellsText), raised);
    }

    [Fact]
    public void RemoveLastCell_ThreeCells_KeepsTheFirstTwo()
    {
        var up = Up(FacingSource.Cells, new SpriteCell(0, 2), new SpriteCell(1, 2), new SpriteCell(2, 2));

        up.RemoveLastCellCommand.Execute(null);

        Assert.Equal([new SpriteCell(0, 2), new SpriteCell(1, 2)], up.Cells);
    }

    [Fact]
    public void RemoveLastCellCanExecute_CellAdded_IsRaised()
    {
        var up = Up(FacingSource.Cells);
        var lit = false;
        up.RemoveLastCellCommand.CanExecuteChanged += (_, _) => lit = up.RemoveLastCellCommand.CanExecute(null);

        up.AddCell(new SpriteCell(1, 1));

        Assert.True(lit);
    }

    [Fact]
    public void ClearCells_ThenAddCell_StartsANewSequence()
    {
        var up = Up(FacingSource.Cells, new SpriteCell(0, 2), new SpriteCell(1, 2));

        up.ClearCellsCommand.Execute(null);
        up.AddCell(new SpriteCell(3, 0));

        Assert.Equal([new SpriteCell(3, 0)], up.Cells);
    }

    private static FacingSetup Up(FacingSource source, params SpriteCell[] cells) =>
        new(Facing.Up) { Source = source, Cells = cells };
}
