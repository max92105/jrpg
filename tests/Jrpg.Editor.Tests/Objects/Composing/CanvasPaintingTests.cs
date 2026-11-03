using Jrpg.Core.Geometry;
using Jrpg.Data.Art;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Objects.Composing;
using Jrpg.Editor.Tests.Shell;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Tests.Objects.Composing;

/// <summary>The composer's strokes and size over the Tiles panel's picker, on the test world.</summary>
public sealed class CanvasPaintingTests : IDisposable
{
    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void PaintAt_RectangleChosenInThePicker_StampsItAtTheCell()
    {
        var painting = Painting(ObjectCanvas.Empty(4, 4));
        var cells = ChooseTopLeftSquare(painting.Picker);

        Stroke(painting, new GridCell(1, 1));

        Assert.Equal(ObjectCanvas.Empty(4, 4).Stamp(new GridCell(1, 1), cells).Cells, painting.Canvas.Cells);
    }

    [Fact]
    public void PaintAt_NothingChosen_ChangesNothing()
    {
        var painting = Painting(ObjectCanvas.Empty(3, 3));

        Stroke(painting, new GridCell(1, 1));

        Assert.True(painting.Canvas.IsBlank);
        Assert.False(painting.CanUndo);
    }

    // A city cell chosen with City.png gone: Replace cells refused it the same way.
    [Fact]
    public void PaintAt_CellsOfASheetWithoutItsImage_ChangesNothing()
    {
        var root = _content.CopyOfTestWorld();
        File.Delete(Path.Combine(ArtFolders.RootIn(root), ArtFolders.Tilesheets, "City.png"));
        var viewModel = _content.Open(root);
        var picker = viewModel.Painting!.Browser;
        picker.Select(viewModel.Catalog!.Tiles.Values.First(tile => tile.SheetId == "city").Id);
        var painting = new CanvasPainting(ObjectCanvas.Empty(3, 3), picker);

        Stroke(painting, new GridCell(1, 1));

        Assert.True(painting.Canvas.IsBlank);
    }

    [Fact]
    public void PaintAt_EraseToggleOn_EmptiesTheCell()
    {
        var painting = Painting(ObjectCanvas.Of(new SheetCellRectangle("forest", 0, 0, 2, 2)));
        ChooseTopLeftSquare(painting.Picker);
        painting.IsErasing = true;

        Stroke(painting, new GridCell(1, 0));

        Assert.False(painting.Canvas.Cells.ContainsKey(new GridCell(1, 0)));
        Assert.Equal(3, painting.Canvas.Cells.Count);
    }

    [Fact]
    public void EraseAt_PaintedCell_EmptiesIt()
    {
        var painting = Painting(ObjectCanvas.Of(new SheetCellRectangle("forest", 0, 0, 2, 2)));

        painting.BeginStroke();
        painting.EraseAt(new GridCell(0, 1));
        painting.EndStroke();

        Assert.False(painting.Canvas.Cells.ContainsKey(new GridCell(0, 1)));
        Assert.True(painting.CanUndo);
    }

    [Fact]
    public void Undo_AfterAStrokeOverThreeCells_RestoresTheCanvasBeforeTheStroke()
    {
        var painting = Painting(ObjectCanvas.Empty(6, 6));
        ChooseTopLeftSquare(painting.Picker);
        var before = painting.Canvas;

        Stroke(painting, new GridCell(0, 0), new GridCell(2, 0), new GridCell(4, 0));
        painting.UndoCommand.Execute(null);

        Assert.Same(before, painting.Canvas);
        Assert.False(painting.CanUndo);
    }

    [Fact]
    public void Redo_AfterUndo_PaintsTheStrokeAgain()
    {
        var painting = Painting(ObjectCanvas.Empty(6, 6));
        ChooseTopLeftSquare(painting.Picker);
        Stroke(painting, new GridCell(0, 0), new GridCell(2, 0));
        var painted = painting.Canvas;
        painting.UndoCommand.Execute(null);

        painting.RedoCommand.Execute(null);

        Assert.Same(painted, painting.Canvas);
        Assert.False(painting.CanRedo);
    }

    [Fact]
    public void Undo_AfterAResize_BringsTheDroppedCellsBack()
    {
        var start = ObjectCanvas.Of(new SheetCellRectangle("forest", 0, 0, 3, 2));
        var painting = Painting(start);
        painting.ColumnsText = "1";

        painting.UndoCommand.Execute(null);

        Assert.Same(start, painting.Canvas);
        Assert.Equal(("3", "2"), (painting.ColumnsText, painting.RowsText));
    }

    [Theory]
    [InlineData("1", 1)]
    [InlineData("32", 32)]
    public void ColumnsText_ValidSizes_Resize(string typed, int columns)
    {
        var painting = Painting(ObjectCanvas.Empty(3, 3));

        painting.ColumnsText = typed;

        Assert.Equal((columns, 3), (painting.Canvas.Width, painting.Canvas.Height));
        Assert.Null(painting.SizeProblem);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("33")]
    [InlineData("x")]
    public void ColumnsText_InvalidSizes_SetSizeProblemAndKeepTheSize(string typed)
    {
        var painting = Painting(ObjectCanvas.Empty(3, 3));

        painting.ColumnsText = typed;

        Assert.Equal((3, 3), (painting.Canvas.Width, painting.Canvas.Height));
        Assert.Equal("Columns must be a whole number from 1 to 32.", painting.SizeProblem);
    }

    [Fact]
    public void EndStroke_ErasingAnEmptyCell_RecordsNoStep()
    {
        var painting = Painting(ObjectCanvas.Empty(3, 3));

        painting.BeginStroke();
        painting.EraseAt(new GridCell(1, 1));
        painting.EndStroke();

        Assert.False(painting.CanUndo);
    }

    [Fact]
    public void RowsText_ValidSize_Resizes()
    {
        var painting = Painting(ObjectCanvas.Empty(3, 3));

        painting.RowsText = "5";

        Assert.Equal((3, 5), (painting.Canvas.Width, painting.Canvas.Height));
        Assert.Null(painting.SizeProblem);
    }

    [Fact]
    public void RowsText_InvalidSize_SetsSizeProblemAndKeepsTheSize()
    {
        var painting = Painting(ObjectCanvas.Empty(3, 3));

        painting.RowsText = "0";

        Assert.Equal((3, 3), (painting.Canvas.Width, painting.Canvas.Height));
        Assert.Equal("Rows must be a whole number from 1 to 32.", painting.SizeProblem);
    }

    private CanvasPainting Painting(ObjectCanvas start) =>
        new(start, _content.Open(_content.TestWorld).Painting!.Browser);

    // Shift + drag over the shown sheet's top-left 2 x 2 cells.
    private static SheetCellRectangle ChooseTopLeftSquare(TileBrowserViewModel picker)
    {
        var sheet = picker.SelectedSheet!;
        picker.SelectCellsBetween(new Point2D(sheet.CellLeft(0), sheet.CellTop(0)), new Point2D(sheet.CellLeft(1), sheet.CellTop(1)));
        return picker.SelectedCells!.Value;
    }

    private static void Stroke(CanvasPainting painting, params GridCell[] cells)
    {
        painting.BeginStroke();
        foreach (var cell in cells)
        {
            painting.PaintAt(cell);
        }

        painting.EndStroke();
    }
}
