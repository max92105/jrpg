using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Objects.Composing;
using Jrpg.Editor.Tests.Shell;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Tests.Objects.Composing;

/// <summary>
/// The composer's chosen level, any from -7 to 7 (decision 490), over the Tiles panel's picker on the test world.
/// </summary>
public sealed class CanvasPaintingLevelTests : IDisposable
{
    private static readonly GridCell _cell = new(1, 1);

    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void Level_New_IsTheBase() => Assert.Equal(0, Painting(ObjectCanvas.Empty(3, 3)).Level);

    [Fact]
    public void LevelChoices_New_ListsEveryLevelFromTheTopDown()
    {
        var choices = Painting(ObjectCanvas.Empty(3, 3)).LevelChoices;

        Assert.Equal(ObjectTile.MaxLevel - ObjectTile.MinLevel + 1, choices.Count);
        Assert.Equal((ObjectTile.MaxLevel, ObjectTile.MinLevel), (choices[0].Level, choices[^1].Level));
        Assert.Equal("0 (base)", choices.Single(choice => choice.Level == 0).Name);
        Assert.Equal("-1", choices.Single(choice => choice.Level == -1).Name);
    }

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
    public void PaintAt_OnLevelMinusOne_StampsUnderTheBaseCell()
    {
        var painting = Painting(ObjectCanvas.Empty(3, 3).Stamp(0, _cell, Cell(5)));
        var chosen = ChooseTopLeftCell(painting.Picker);

        painting.Level = -1;
        Stroke(painting, _cell);

        Assert.Equal(new SheetCell("forest", 5, 0), painting.Canvas.Cells[new CellOnLevel(_cell, 0)]);
        Assert.Equal(chosen.Cells[0], painting.Canvas.Cells[new CellOnLevel(_cell, -1)]);
    }

    [Fact]
    public void PaintAt_OnAnEmptyLevelAboveTheBase_StampsThere()
    {
        var painting = Painting(ObjectCanvas.Empty(3, 3));
        var chosen = ChooseTopLeftCell(painting.Picker);

        painting.Level = 5;
        Stroke(painting, _cell);

        Assert.Equal(chosen.Cells[0], Assert.Single(painting.Canvas.Cells, pair => pair.Key == new CellOnLevel(_cell, 5)).Value);
    }

    [Fact]
    public void Undo_OnlyStrokeOnTheChosenLevel_KeepsTheLevelChosen()
    {
        var painting = Painting(ObjectCanvas.Empty(3, 3));
        ChooseTopLeftCell(painting.Picker);
        painting.Level = 3;
        Stroke(painting, _cell);

        painting.UndoCommand.Execute(null);

        Assert.True(painting.Canvas.IsBlank);
        Assert.Equal(3, painting.Level);
    }

    [Fact]
    public void LevelChoices_PaintedUnderAndOverTheBase_MarksThoseLevelsOnly()
    {
        var painting = Painting(ObjectCanvas.Empty(3, 3).Stamp(-1, _cell, Cell(0)).Stamp(2, _cell, Cell(1)));

        Assert.Equal(["2 •", "0 (base)", "-1 •"], new[] { 2, 0, -1 }.Select(level => ChoiceOf(painting, level).Text));
        Assert.Equal(2, painting.LevelChoices.Count(choice => choice.IsPainted));
    }

    [Fact]
    public void PaintAt_OnAnEmptyLevel_RaisesItsChoiceText()
    {
        var painting = Painting(ObjectCanvas.Empty(3, 3));
        ChooseTopLeftCell(painting.Picker);
        var raised = new List<string?>();
        ChoiceOf(painting, 4).PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        painting.Level = 4;
        Stroke(painting, _cell);

        Assert.Contains(nameof(LevelChoice.Text), raised);
        Assert.Equal("4 •", ChoiceOf(painting, 4).Text);
    }

    [Fact]
    public void Undo_OnlyStrokeOnALevel_ClearsItsMark()
    {
        var painting = Painting(ObjectCanvas.Empty(3, 3));
        ChooseTopLeftCell(painting.Picker);
        painting.Level = -2;
        Stroke(painting, _cell);

        painting.UndoCommand.Execute(null);

        Assert.False(ChoiceOf(painting, -2).IsPainted);
    }

    private static LevelChoice ChoiceOf(CanvasPainting painting, int level) => painting.LevelChoices.Single(choice => choice.Level == level);

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
