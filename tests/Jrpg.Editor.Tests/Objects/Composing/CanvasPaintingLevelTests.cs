using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Objects.Composing;
using Jrpg.Editor.Tests.Shell;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Tests.Objects.Composing;

/// <summary>
/// The composer's chosen level and its choices (decision 478), over the Tiles panel's picker on the test world.
/// </summary>
public sealed class CanvasPaintingLevelTests : IDisposable
{
    private static readonly GridCell _cell = new(1, 1);

    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void PaintAt_OnLevelOne_StampsOverTheBaseCell()
    {
        var painting = Painting(ObjectCanvas.Empty(3, 3));
        var chosen = ChooseTopLeftCell(painting.Picker);
        Stroke(painting, _cell);

        painting.Level = 1;
        Stroke(painting, _cell);

        Assert.Equal(2, painting.Canvas.Cells.Count);
        Assert.True(painting.Canvas.Cells.ContainsKey(new CellOnLevel(_cell, 0)));
        Assert.Equal(chosen.Cells[0], painting.Canvas.Cells[new CellOnLevel(_cell, 1)]);
    }

    [Fact]
    public void LevelChoices_BlankCanvas_IsTheBaseOnly() => Assert.Equal(["0 (base)"], Painting(ObjectCanvas.Empty(3, 3)).LevelChoices);

    [Fact]
    public void LevelChoices_PaintedOnLevelOne_EndsWithAnEmptyLevelTwo()
    {
        var painting = Painting(ObjectCanvas.Empty(3, 3).Stamp(0, _cell, Cell(0)).Stamp(1, _cell, Cell(1)));

        Assert.Equal(["0 (base)", "1", "2"], painting.LevelChoices);
    }

    [Fact]
    public void LevelChoices_PaintedOnTheHighestLevel_StopsAtIt()
    {
        var painting = Painting(ObjectCanvas.Empty(3, 3).Stamp(ObjectTile.MaxLevel, _cell, Cell(0)));

        Assert.Equal(ObjectTile.MaxLevel + 1, painting.LevelChoices.Count);
        Assert.Equal("7", painting.LevelChoices[^1]);
    }

    [Fact]
    public void LevelChoices_StrokeOnTheTopLevel_RaisesPropertyChanged()
    {
        var painting = Painting(ObjectCanvas.Empty(3, 3));
        ChooseTopLeftCell(painting.Picker);
        var raised = new List<string?>();
        painting.PropertyChanged += (_, args) => raised.Add(args.PropertyName);

        Stroke(painting, _cell);

        Assert.Equal(["0 (base)", "1"], painting.LevelChoices);
        Assert.Equal(raised.IndexOf(nameof(CanvasPainting.LevelChoices)) + 1, raised.IndexOf(nameof(CanvasPainting.Level)));
    }

    [Fact]
    public void Undo_StrokeThatOpenedTheChosenLevel_ChoosesTheHighestLevelLeft()
    {
        var painting = Painting(ObjectCanvas.Empty(3, 3).Stamp(0, _cell, Cell(0)));
        ChooseTopLeftCell(painting.Picker);
        painting.Level = 1;
        Stroke(painting, _cell);
        painting.Level = 2;

        painting.UndoCommand.Execute(null);

        Assert.Equal(["0 (base)", "1"], painting.LevelChoices);
        Assert.Equal(1, painting.Level);
    }

    [Fact]
    public void Level_SetToMinusOne_KeepsTheChosenLevel()
    {
        var painting = Painting(ObjectCanvas.Empty(3, 3).Stamp(0, _cell, Cell(0)));
        painting.Level = 1;

        painting.Level = -1;

        Assert.Equal(1, painting.Level);
    }

    [Fact]
    public void Level_SetPastTheChoices_KeepsTheChosenLevel()
    {
        var painting = Painting(ObjectCanvas.Empty(3, 3).Stamp(0, _cell, Cell(0)));
        painting.Level = 1;

        painting.Level = 2;

        Assert.Equal(1, painting.Level);
    }

    private static SheetCellRectangle Cell(int column) => new("forest", column, 0, 1, 1);

    private CanvasPainting Painting(ObjectCanvas start) =>
        new(start, _content.Open(_content.TestWorld).Painting!.Browser);

    // A click on the shown sheet's top-left cell.
    private static SheetCellRectangle ChooseTopLeftCell(TileBrowserViewModel picker)
    {
        var sheet = picker.SelectedSheet!;
        picker.SelectCellsBetween(new Point2D(sheet.CellLeft(0), sheet.CellTop(0)), new Point2D(sheet.CellLeft(0), sheet.CellTop(0)));
        return picker.MarkedCells!.Value;
    }

    private static void Stroke(CanvasPainting painting, GridCell cell)
    {
        painting.BeginStroke();
        painting.PaintAt(cell);
        painting.EndStroke();
    }
}
