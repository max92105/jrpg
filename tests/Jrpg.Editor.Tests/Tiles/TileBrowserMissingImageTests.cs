using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Tests.Tiles;

/// <summary>
/// The sheet box with a sheet whose image is missing (decision 442): shown, but never sized or hit-tested.
/// </summary>
public sealed class TileBrowserMissingImageTests
{
    private static readonly TilesheetDefinition _forest = new() { Id = "forest", File = "Forest.png", TileWidth = 48, TileHeight = 48, Margin = 0, Spacing = 0 };
    private static readonly TilesheetDefinition _city = new() { Id = "city", File = "City.png", TileWidth = 48, TileHeight = 48, Margin = 0, Spacing = 0 };

    private readonly CatalogDraft _draft = new(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world")) with
    {
        Tilesheets = new Dictionary<string, TilesheetDefinition> { ["forest"] = _forest, ["city"] = _city },
        Tiles = new Dictionary<int, TileDefinition>
        {
            [8] = new() { Id = 8, IsWalkable = true, SheetId = "forest", Column = 0, Row = 0 },
            [9] = new() { Id = 9, IsWalkable = true, SheetId = "city", Column = 1, Row = 1 },
        },
    });

    private readonly TileBrowserViewModel _browser;

    // Only the forest's image is in the folder; asking the city's size would be a bug, so it throws.
    public TileBrowserMissingImageTests()
    {
        var images = new SheetImages(() => ["Forest.png"], file => file == "Forest.png" ? (96, 96) : throw new InvalidOperationException($"{file} has no image to size."));
        _browser = new TileBrowserViewModel(_draft, images, _ => true, () => null);
        _browser.SelectedChoice = _browser.SheetChoices.Single(choice => choice.IsImageMissing);
    }

    [Fact]
    public void SheetChoices_SheetWithoutImage_IsListedAsMissing()
    {
        Assert.Equal(["forest", "city (image missing)"], _browser.SheetChoices.Select(choice => choice.Label));
    }

    [Fact]
    public void SetUpSheetCommand_MissingImage_CannotExecute()
    {
        Assert.False(_browser.SetUpSheetCommand.CanExecute(null));
    }

    [Fact]
    public void CutSheetCommand_MissingImage_CannotExecute()
    {
        Assert.False(_browser.CutSheetCommand.CanExecute(null));
    }

    [Fact]
    public void SelectCellAt_MissingImage_SelectsNothing()
    {
        _browser.SelectCellAt(10, 10);

        Assert.Null(_browser.SelectedCell);
    }

    [Fact]
    public void SelectCellsBetween_MissingImage_SelectsNothing()
    {
        _browser.SelectCellsBetween(new Point2D(5, 5), new Point2D(60, 60));

        Assert.Null(_browser.SelectedCells);
    }

    // The eyedropper on a marked tile shows the missing sheet, so the notice says what to do.
    [Fact]
    public void Select_TileOfMissingSheet_ShowsTheMissingChoice()
    {
        _browser.SelectedChoice = _browser.SheetChoices[0];

        _browser.Select(9);

        Assert.True(_browser.SelectedChoice!.IsImageMissing);
        Assert.Equal(new SheetCell("city", 1, 1), _browser.SelectedCell);
    }
}
