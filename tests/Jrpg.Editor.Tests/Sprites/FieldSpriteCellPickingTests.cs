using Jrpg.Core.Content;
using Jrpg.Core.Sprites;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Sprites;

namespace Jrpg.Editor.Tests.Sprites;

// Picking cells on the Character sprites page's image (decision 772): a point in image pixels adds the cell under it to
// the picking facing's sequence. On a 100 x 130 px image cut in 4 x 4 frames of 25 x 32 px, so the bottom 2 px belong
// to no frame.
public sealed class FieldSpriteCellPickingTests
{
    private static readonly ContentCatalog _catalog =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    private readonly FieldSpriteSetupViewModel _setup;

    public FieldSpriteCellPickingTests()
    {
        _setup = FieldSpriteSetupViewModel.ForNew("picked.png", (100, 130), new CatalogDraft(_catalog));
        _setup.Columns = 4;
        _setup.Rows = 4;
        _setup.Walk.Up.Source = FacingSource.Cells;
        _setup.Walk.Up.ClearCellsCommand.Execute(null);
    }

    [Fact]
    public void PickCellAt_InsideAFrame_AddsItsCellToThePickingFacing()
    {
        _setup.PickCellAt(60, 70);

        Assert.Equal([new SpriteCell(2, 2)], _setup.Walk.Up.Cells);
    }

    [Fact]
    public void PickCellAt_SameFrameTwice_AddsItTwice()
    {
        _setup.PickCellAt(60, 70);
        _setup.PickCellAt(61, 71);

        Assert.Equal([new SpriteCell(2, 2), new SpriteCell(2, 2)], _setup.Walk.Up.Cells);
    }

    [Fact]
    public void PickCellAt_LastWholeFrame_AddsIt()
    {
        _setup.PickCellAt(99, 127);

        Assert.Equal([new SpriteCell(3, 3)], _setup.Walk.Up.Cells);
    }

    [Fact]
    public void PickCellAt_BelowTheWholeRows_AddsNothing()
    {
        _setup.PickCellAt(10, 129);

        Assert.Empty(_setup.Walk.Up.Cells);
    }

    [Fact]
    public void PickCellAt_NegativePoint_AddsNothing()
    {
        _setup.PickCellAt(-1, 10);

        Assert.Empty(_setup.Walk.Up.Cells);
    }

    [Fact]
    public void PickCellAt_NoFacingPicking_AddsNothing()
    {
        _setup.Walk.TogglePickingCommand.Execute(_setup.Walk.Up);

        _setup.PickCellAt(60, 70);

        Assert.Empty(_setup.Walk.Up.Cells);
    }

    [Fact]
    public void Source_RowToCells_MakesItThePickingFacing()
    {
        _setup.Walk.Left.Source = FacingSource.Cells;

        Assert.Same(_setup.Walk.Left, _setup.Walk.PickingFacing);
    }

    [Fact]
    public void Source_LeavingCells_ClearsThePickingFacing()
    {
        _setup.Walk.Up.Source = FacingSource.Row;

        Assert.Null(_setup.Walk.PickingFacing);
    }

    [Fact]
    public void TogglePicking_ThePickingFacing_ClearsIt()
    {
        _setup.Walk.TogglePickingCommand.Execute(_setup.Walk.Up);

        Assert.Null(_setup.Walk.PickingFacing);
    }

    [Fact]
    public void PickCellAt_AboveTheImage_AddsNothing()
    {
        _setup.PickCellAt(10, -1);

        Assert.Empty(_setup.Walk.Up.Cells);
    }

    // 102 px across 4 columns: frames of 25 px, the last 2 px no frame.
    [Fact]
    public void PickCellAt_PastTheWholeColumns_AddsNothing()
    {
        var wide = FieldSpriteSetupViewModel.ForNew("wide.png", (102, 128), new CatalogDraft(_catalog));
        wide.Columns = 4;
        wide.Rows = 4;
        wide.Walk.Up.Source = FacingSource.Cells;
        wide.Walk.Up.ClearCellsCommand.Execute(null);

        wide.PickCellAt(101, 10);

        Assert.Empty(wide.Walk.Up.Cells);
    }

    // A Pick click changes nothing in the record: the problem line, and a refused copy's line with it, stay.
    [Fact]
    public void TogglePicking_OnASetup_RaisesNoProblem()
    {
        var raised = new List<string?>();
        _setup.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        _setup.Walk.TogglePickingCommand.Execute(_setup.Walk.Up);

        Assert.DoesNotContain(nameof(FieldSpriteSetupViewModel.Problem), raised);
    }

    // Greyed unless the facing reads cells.
    [Fact]
    public void TogglePickingCommandCanExecute_FacingSwitchedToCells_IsRaised()
    {
        var lit = false;
        _setup.Walk.TogglePickingCommand.CanExecuteChanged +=
            (_, _) => lit = _setup.Walk.TogglePickingCommand.CanExecute(_setup.Walk.Down);

        _setup.Walk.Down.Source = FacingSource.Cells;

        Assert.True(lit);
    }

    [Fact]
    public void TogglePickingCommand_FacingOnRow_CannotExecute()
    {
        Assert.False(_setup.Walk.TogglePickingCommand.CanExecute(_setup.Walk.Down));
    }
}
