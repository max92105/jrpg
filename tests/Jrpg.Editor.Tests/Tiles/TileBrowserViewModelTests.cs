using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Tests.Tiles;

public sealed class TileBrowserViewModelTests
{
    // Two sheets: "dungeon" has 16-pixel cells inside a 2-pixel margin, 1 pixel apart, and one tile at column 1, row 0.
    // Its image is 54 x 19: three whole columns (the third ends at 2 + 2 * 17 + 16 = 52) and one row.
    private static readonly TilesheetDefinition _dungeon = new() { Id = "dungeon", File = "Dungeon.png", TileWidth = 16, TileHeight = 16, Margin = 2, Spacing = 1 };
    private static readonly TilesheetDefinition _forest = new() { Id = "forest", File = "Forest.png", TileWidth = 48, TileHeight = 48, Margin = 0, Spacing = 0 };

    private static readonly Dictionary<string, (int Width, int Height)> _imageSizes = new()
    {
        ["Dungeon.png"] = (54, 19),
        ["Forest.png"] = (96, 48),
        ["Grass3.png"] = (96, 64),
    };

    private readonly CatalogDraft _draft = new(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content")) with
    {
        Tilesheets = new Dictionary<string, TilesheetDefinition> { ["forest"] = _forest, ["dungeon"] = _dungeon },
        Tiles = new Dictionary<int, TileDefinition>
        {
            [7] = new() { Id = 7, IsWalkable = true, SheetId = "dungeon", Column = 1, Row = 0 },
            [8] = new() { Id = 8, IsWalkable = true, SheetId = "forest", Column = 0, Row = 0 },
        },
    });

    private readonly TileBrowserViewModel _browser;
    private readonly List<string> _questions = [];

    // Grass3.png has no sheet yet; a test adds to the folder as an import would.
    private readonly List<string> _imageFiles = ["Dungeon.png", "Forest.png", "Grass3.png"];
    private bool _answer = true;

    public TileBrowserViewModelTests()
    {
        var images = new SheetImages(() => [.. _imageFiles], file => _imageSizes[file]);
        _browser = new TileBrowserViewModel(_draft, images, Ask, () => null);
    }

    [Fact]
    public void Constructor_ImagesSortedByFile_ShowsTheFirst()
    {
        Assert.Equal(["Dungeon.png", "Forest.png", "Grass3.png"], _browser.SheetChoices.Select(choice => choice.File));
        Assert.Equal(_dungeon, _browser.SelectedSheet);
        Assert.Equal([7], _browser.SheetTiles.Select(tile => tile.Id));
        Assert.Null(_browser.SelectedTileId);
    }

    [Fact]
    public void SheetChoices_ImageWithoutSheet_IsListedNotSetUp()
    {
        Assert.Equal(["dungeon", "forest", "Grass3.png (not set up)"], _browser.SheetChoices.Select(choice => choice.Label));
        Assert.Null(_browser.SheetChoices[2].Sheet);
    }

    [Fact]
    public void SelectCellAt_ImageNotSetUp_ChoosesNothing()
    {
        _browser.SelectCellAt(19, 2);
        _browser.SelectedChoice = _browser.SheetChoices[2];

        _browser.SelectCellAt(5, 5);

        Assert.Equal(new SheetCell("dungeon", 1, 0), _browser.SelectedCell);
        Assert.Null(_browser.SelectedSheet);
        Assert.Equal("Not set up yet. Brush: cell (1, 0) of dungeon", _browser.ChoiceText);
    }

    [Fact]
    public void SelectCellAt_CellThatIsATile_SelectsItsTile()
    {
        // Column 1 starts at margin 2 + one cell of 16 + spacing 1 = 19.
        _browser.SelectCellAt(19, 2);

        Assert.Equal(new SheetCell("dungeon", 1, 0), _browser.SelectedCell);
        Assert.Equal(7, _browser.SelectedTileId);
    }

    [Fact]
    public void SelectCellAt_CellThatIsNoTile_SelectsTheCell()
    {
        _browser.SelectCellAt(19, 2);

        _browser.SelectCellAt(5, 5);

        Assert.Equal(new SheetCell("dungeon", 0, 0), _browser.SelectedCell);
        Assert.Null(_browser.SelectedTileId);
    }

    // Past the last whole column (53), on the spacing after column 0 (18), on the margin (1).
    [Theory]
    [InlineData(53, 5)]
    [InlineData(18, 5)]
    [InlineData(1, 5)]
    public void SelectCellAt_OffEveryWholeCell_KeepsTheChoice(double x, double y)
    {
        _browser.SelectCellAt(19, 2);

        _browser.SelectCellAt(x, y);

        Assert.Equal(7, _browser.SelectedTileId);
    }

    // 96 x 64: 32 is the first common size that divides both sides, so 3 columns and 2 rows.
    [Fact]
    public void SetUpSheetCommand_ImageNotSetUp_OpensCutWithGuessedColumnsAndRows()
    {
        _browser.SelectedChoice = _browser.SheetChoices[2];

        _browser.SetUpSheetCommand.Execute(null);

        Assert.Equal(("grass_3", 3, 2), (_browser.Cut!.SheetId, _browser.Cut.Columns, _browser.Cut.Rows));
        Assert.False(_draft.Current.Tilesheets.ContainsKey("grass_3"));
    }

    [Fact]
    public void ApplyCutCommand_NewSheet_AddsItAndShowsTheGrid()
    {
        OpenGrass3Cut();
        _browser.Cut!.Columns = 2;

        _browser.ApplyCutCommand.Execute(null);

        var expected = new TilesheetDefinition { Id = "grass_3", File = "Grass3.png", TileWidth = 48, TileHeight = 32, Margin = 0, Spacing = 0 };
        Assert.Equal(expected, _draft.Current.Tilesheets["grass_3"]);
        Assert.Equal(expected, _browser.SelectedSheet);
        Assert.Null(_browser.Cut);
        Assert.Equal(["dungeon", "forest", "grass_3"], _browser.SheetChoices.Select(choice => choice.Label));
    }

    [Fact]
    public void ApplyCutCommand_CutWithProblem_CannotExecute()
    {
        OpenGrass3Cut();

        _browser.Cut!.Columns = 5;

        Assert.False(_browser.ApplyCutCommand.CanExecute(null));
    }

    // The dungeon's 54 x 19 image holds 3 whole columns and 1 row past its margin of 2 and spacing of 1.
    [Fact]
    public void CutSheetCommand_SetUpSheet_OpensCutWithItsColumnsAndRows()
    {
        _browser.CutSheetCommand.Execute(null);

        Assert.Equal(("dungeon", 3, 1, 2, 1), (_browser.Cut!.SheetId, _browser.Cut.Columns, _browser.Cut.Rows, _browser.Cut.Margin, _browser.Cut.Spacing));
        Assert.False(_browser.SetUpSheetCommand.CanExecute(null));
    }

    [Fact]
    public void CutSheetCommand_ImageNotSetUpOrPanelOpen_CannotExecute()
    {
        OpenGrass3Cut();
        Assert.False(_browser.CutSheetCommand.CanExecute(null));

        _browser.CancelCutCommand.Execute(null);

        Assert.False(_browser.CutSheetCommand.CanExecute(null));
    }

    // Dungeon cell (2, 0) becomes a tile while the panel is open; 2 columns would leave it outside.
    [Fact]
    public void ApplyCutCommand_TileMadeWhileOpenOutsideNewGrid_CannotExecute()
    {
        _browser.CutSheetCommand.Execute(null);
        _browser.Cut!.Columns = 2;
        _browser.Cut.Margin = 0;
        _browser.Cut.Spacing = 0;
        Assert.True(_browser.ApplyCutCommand.CanExecute(null));

        _draft.AddTile(new SheetCell("dungeon", 2, 0));

        Assert.False(_browser.ApplyCutCommand.CanExecute(null));
    }

    // The forest's 96 x 48 image in 4 columns: 24 px wide tiles; tile 8 keeps its cell (0, 0).
    [Fact]
    public void ApplyCutCommand_ReCut_ReplacesTheCut()
    {
        _browser.SelectedChoice = _browser.SheetChoices[1];
        _browser.CutSheetCommand.Execute(null);
        _browser.Cut!.Columns = 4;

        _browser.ApplyCutCommand.Execute(null);

        Assert.Equal(_forest with { TileWidth = 24 }, _draft.Current.Tilesheets["forest"]);
        Assert.Equal(_forest with { TileWidth = 24 }, _browser.SelectedSheet);
        Assert.Equal(8, _draft.TileAt(new SheetCell("forest", 0, 0))?.Id);
    }

    // Tile 8 is cut from the forest sheet, so a re-cut asks first.
    [Fact]
    public void ApplyCutCommand_UserDeclinesWarning_KeepsTheCut()
    {
        _answer = false;
        _browser.SelectedChoice = _browser.SheetChoices[1];
        _browser.CutSheetCommand.Execute(null);
        _browser.Cut!.Columns = 4;

        _browser.ApplyCutCommand.Execute(null);

        Assert.StartsWith("1 tile is cut from this sheet", Assert.Single(_questions));
        Assert.Equal(_forest, _draft.Current.Tilesheets["forest"]);
        Assert.NotNull(_browser.Cut);
    }

    [Fact]
    public void ApplyCutCommand_UserAccepts_ReplacesTheCut()
    {
        _browser.SelectedChoice = _browser.SheetChoices[1];
        _browser.CutSheetCommand.Execute(null);
        _browser.Cut!.Columns = 4;

        _browser.ApplyCutCommand.Execute(null);

        Assert.Single(_questions);
        Assert.Equal(_forest with { TileWidth = 24 }, _draft.Current.Tilesheets["forest"]);
    }

    [Fact]
    public void ApplyCutCommand_NewSheet_AsksNothing()
    {
        OpenGrass3Cut();

        _browser.ApplyCutCommand.Execute(null);

        Assert.Empty(_questions);
    }

    [Fact]
    public void ApplyCutCommand_FieldMadeWrong_AnnouncesItCannotExecute()
    {
        OpenGrass3Cut();
        var announced = false;
        _browser.ApplyCutCommand.CanExecuteChanged += (_, _) => announced = true;

        _browser.Cut!.Columns = 5;

        Assert.True(announced);
    }

    [Fact]
    public void SelectedChoice_OtherImageWhileCutOpen_ClosesCut()
    {
        OpenGrass3Cut();

        _browser.SelectedChoice = _browser.SheetChoices[0];

        Assert.Null(_browser.Cut);
    }

    [Fact]
    public void SetUpSheetCommand_CutOpen_CannotExecute()
    {
        OpenGrass3Cut();

        Assert.False(_browser.SetUpSheetCommand.CanExecute(null));
    }

    [Fact]
    public void CancelCutCommand_NewSheet_AddsNothing()
    {
        OpenGrass3Cut();

        _browser.CancelCutCommand.Execute(null);

        Assert.Null(_browser.Cut);
        Assert.False(_draft.Current.Tilesheets.ContainsKey("grass_3"));
        Assert.Equal(new SheetChoice("Grass3.png", null), _browser.SelectedChoice);
    }

    [Fact]
    public void SetUpSheetCommand_SheetAlreadySetUp_CannotExecute()
    {
        Assert.False(_browser.SetUpSheetCommand.CanExecute(null));
    }

    [Fact]
    public void Undo_SetUpOfTheChosenCellsSheet_ClearsTheChoice()
    {
        OpenGrass3Cut();
        _browser.ApplyCutCommand.Execute(null);
        _browser.SelectCellAt(5, 5);

        _draft.Undo();

        Assert.Null(_browser.SelectedCell);
        Assert.Equal(new SheetChoice("Grass3.png", null), _browser.SelectedChoice);
    }

    [Fact]
    public void Show_NewImageFile_ListsItNotSetUpAndShowsIt()
    {
        _imageFiles.Insert(0, "Cave.png");

        _browser.Show("Cave.png");

        Assert.Equal(["Cave.png (not set up)", "dungeon", "forest", "Grass3.png (not set up)"], _browser.SheetChoices.Select(choice => choice.Label));
        Assert.Equal(new SheetChoice("Cave.png", null), _browser.SelectedChoice);
    }

    // The forest's 96 x 48 image holds 2 x 1 cells of 48 px.
    [Fact]
    public void SelectCellsBetween_TwoCells_ChoosesTheRectangleAndItsTopLeftCell()
    {
        _browser.SelectedChoice = _browser.SheetChoices[1];

        _browser.SelectCellsBetween(new Point2D(60, 10), new Point2D(5, 40));

        Assert.Equal(new SheetCellRectangle("forest", 0, 0, 2, 1), _browser.SelectedCells);
        Assert.Equal(new SheetCell("forest", 0, 0), _browser.SelectedCell);
        Assert.Equal("2 x 1 cells selected: the brush paints the top-left one; New object from selection makes an object of them.", _browser.ChoiceText);
    }

    // The dungeon's last whole column is 2 (it ends at 52 of 54 pixels); 53 is past it, -3 is on the margin.
    [Fact]
    public void SelectCellsBetween_CornerPastLastWholeColumn_ClampsToIt()
    {
        _browser.SelectCellsBetween(new Point2D(-3, 5), new Point2D(53, 5));

        Assert.Equal(new SheetCellRectangle("dungeon", 0, 0, 3, 1), _browser.SelectedCells);
    }

    [Fact]
    public void SelectCellsBetween_SameCell_ChoosesOneCell()
    {
        _browser.SelectCellsBetween(new Point2D(19, 2), new Point2D(25, 8));

        Assert.Null(_browser.SelectedCells);
        Assert.Equal("Cell (1, 0): tile 7", _browser.ChoiceText);
    }

    [Fact]
    public void Select_AfterRectangle_ClearsIt()
    {
        _browser.SelectCellsBetween(new Point2D(5, 5), new Point2D(40, 5));

        _browser.Select(8);

        Assert.Null(_browser.SelectedCells);
    }

    [Fact]
    public void SelectCellAt_AfterRectangle_ClearsIt()
    {
        _browser.SelectCellsBetween(new Point2D(5, 5), new Point2D(40, 5));

        _browser.SelectCellAt(19, 2);

        Assert.Null(_browser.SelectedCells);
        Assert.Equal(new SheetCellRectangle("dungeon", 1, 0, 1, 1), _browser.MarkedCells);
    }

    [Fact]
    public void Select_TileOnAnotherSheet_ShowsItsSheet()
    {
        _browser.Select(8);

        Assert.Equal(_forest, _browser.SelectedSheet);
        Assert.Equal(8, _browser.SelectedTileId);
    }

    [Fact]
    public void ChoiceText_CellThatIsNoTile_SaysNotATileYet()
    {
        _browser.SelectCellAt(40, 5);

        Assert.Equal("Cell (2, 0): not a tile yet", _browser.ChoiceText);
    }

    [Fact]
    public void AddTile_ChosenCell_NamesTheNewTileAndMarksIt()
    {
        _browser.SelectCellAt(40, 5);
        var changed = new List<string?>();
        _browser.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        _draft.AddTile(_browser.SelectedCell!.Value);

        Assert.Equal("Cell (2, 0): tile 9", _browser.ChoiceText);
        Assert.Contains(nameof(TileBrowserViewModel.ChoiceText), changed);
        Assert.Contains(nameof(TileBrowserViewModel.SheetTiles), changed);
    }

    [Fact]
    public void ChoiceText_CellThatIsATile_NamesTheTile()
    {
        _browser.SelectCellAt(19, 2);

        Assert.Equal("Cell (1, 0): tile 7", _browser.ChoiceText);
    }

    private bool Ask(string question)
    {
        _questions.Add(question);
        return _answer;
    }

    private void OpenGrass3Cut()
    {
        _browser.SelectedChoice = _browser.SheetChoices[2];
        _browser.SetUpSheetCommand.Execute(null);
    }
}
