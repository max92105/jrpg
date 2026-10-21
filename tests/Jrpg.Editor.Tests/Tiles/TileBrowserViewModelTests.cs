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

    public TileBrowserViewModelTests()
    {
        // Grass3.png has no sheet yet.
        _browser = new TileBrowserViewModel(_draft, ["Dungeon.png", "Forest.png", "Grass3.png"], file => _imageSizes[file]);
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

    private void OpenGrass3Cut()
    {
        _browser.SelectedChoice = _browser.SheetChoices[2];
        _browser.SetUpSheetCommand.Execute(null);
    }
}
