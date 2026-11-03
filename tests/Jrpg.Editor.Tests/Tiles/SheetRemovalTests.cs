using Jrpg.Core.Content;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Tests.Tiles;

/// <summary>
/// Remove sheet in the Tiles panel (decision 443): a scratch sheet of the tests' own with two tiles, used nowhere in
/// the test world unless a test paints one on a map or builds an object from it.
/// </summary>
public sealed class SheetRemovalTests
{
    private static readonly ContentCatalog _testWorld = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));
    private static readonly TilesheetDefinition _scratch = new() { Id = "scratch", File = "Scratch.png", TileWidth = 16, TileHeight = 16, Margin = 0, Spacing = 0 };
    private static readonly int _firstId = _testWorld.Tiles.Keys.Max() + 1;

    private static readonly ContentCatalog _catalog = _testWorld with
    {
        Tilesheets = new Dictionary<string, TilesheetDefinition>(_testWorld.Tilesheets) { ["scratch"] = _scratch },
        Tiles = new Dictionary<int, TileDefinition>(_testWorld.Tiles)
        {
            [_firstId] = new() { Id = _firstId, IsWalkable = true, SheetId = "scratch", Column = 0, Row = 0 },
            [_firstId + 1] = new() { Id = _firstId + 1, IsWalkable = true, SheetId = "scratch", Column = 1, Row = 0 },
        },
    };

    [Fact]
    public void RemoveSheet_NoTileUsed_RemovesItAndSaysSo()
    {
        var draft = new CatalogDraft(_catalog);
        var (removal, _) = RemovalOn(draft, openMap: null);

        removal.RemoveSheetCommand.Execute(null);

        Assert.False(draft.Current.Tilesheets.ContainsKey("scratch"));
        Assert.False(draft.Current.Tiles.ContainsKey(_firstId));
        Assert.Equal("Removed sheet scratch and its 2 tiles.", removal.Status);
    }

    [Fact]
    public void RemoveSheet_TileUsedOnAMap_RefusesNamingTheUsers()
    {
        var draft = new CatalogDraft(WithTilePaintedOnSavedForest(_catalog));
        var (removal, _) = RemovalOn(draft, openMap: null);

        removal.RemoveSheetCommand.Execute(null);

        Assert.Equal("Can't remove sheet scratch: its tiles are used on map forest.", removal.Status);
        Assert.True(draft.Current.Tilesheets.ContainsKey("scratch"));
    }

    [Fact]
    public void RemoveSheet_TileUsedByAnObjectAndOnMaps_NamesThemAll()
    {
        var crate = _catalog.Objects["chest"] with { Id = "crate", Tiles = [new ObjectTile(0, 0, _firstId)] };
        var catalog = WithTilePaintedOnSavedForest(_catalog) with { Objects = new Dictionary<string, ObjectDefinition>(_catalog.Objects) { ["crate"] = crate } };
        var draft = new CatalogDraft(catalog);
        var (removal, _) = RemovalOn(draft, new MapViewModel(new MapDefinition(WithTileAtTopLeft(catalog.Maps["start"].Layout, _firstId + 1), catalog.Maps["start"].Settings)));

        removal.RemoveSheetCommand.Execute(null);

        Assert.Equal("Can't remove sheet scratch: its tiles are used on maps forest, start and object crate.", removal.Status);
    }

    // The open forest no longer paints the tile; its saved file still does, but the screen is what counts.
    [Fact]
    public void RemoveSheet_TileUsedOnlyInTheOpenMapsSavedFile_Removes()
    {
        var draft = new CatalogDraft(WithTilePaintedOnSavedForest(_catalog));
        var (removal, _) = RemovalOn(draft, new MapViewModel(_catalog.Maps["forest"]));

        removal.RemoveSheetCommand.Execute(null);

        Assert.False(draft.Current.Tilesheets.ContainsKey("scratch"));
    }

    [Fact]
    public void RemoveSheet_Refused_LeavesTheCatalogUnchanged()
    {
        var draft = new CatalogDraft(WithTilePaintedOnSavedForest(_catalog));
        var before = draft.Current;
        var (removal, _) = RemovalOn(draft, openMap: null);

        removal.RemoveSheetCommand.Execute(null);

        Assert.Same(before, draft.Current);
        Assert.False(draft.CanUndo);
    }

    [Fact]
    public void RemoveSheet_Removed_ShowsTheImageAsNotSetUp()
    {
        var (removal, browser) = RemovalOn(new CatalogDraft(_catalog), openMap: null);

        removal.RemoveSheetCommand.Execute(null);

        Assert.Equal(new SheetChoice("Scratch.png", null), browser.SelectedChoice);
        Assert.False(removal.RemoveSheetCommand.CanExecute(null));
    }

    [Fact]
    public void Status_ShownSheetChanges_IsCleared()
    {
        var (removal, browser) = RemovalOn(new CatalogDraft(WithTilePaintedOnSavedForest(_catalog)), openMap: null);
        removal.RemoveSheetCommand.Execute(null);
        Assert.NotNull(removal.Status);

        browser.SelectedChoice = browser.SheetChoices.First(choice => choice.Sheet?.Id != "scratch");

        Assert.Null(removal.Status);
    }

    private static (SheetRemoval Removal, TileBrowserViewModel Browser) RemovalOn(CatalogDraft draft, MapViewModel? openMap)
    {
        var files = draft.Current.Tilesheets.Values.Select(sheet => sheet.File).Distinct().Order(StringComparer.Ordinal).ToList();
        var images = new SheetImages(() => files, _ => (1000, 1000));
        var browser = new TileBrowserViewModel(draft, images, _ => true, () => openMap?.Map.Layout);
        browser.SelectedChoice = browser.SheetChoices.First(choice => choice.Sheet?.Id == "scratch");
        return (new SheetRemoval(draft, browser, () => openMap), browser);
    }

    private static ContentCatalog WithTilePaintedOnSavedForest(ContentCatalog catalog)
    {
        var forest = catalog.Maps["forest"];
        var painted = new MapDefinition(WithTileAtTopLeft(forest.Layout, _firstId), forest.Settings);
        return catalog with { Maps = new Dictionary<string, MapDefinition>(catalog.Maps) { ["forest"] = painted } };
    }

    private static MapLayout WithTileAtTopLeft(MapLayout layout, int tileId)
    {
        var ground = layout.Layers[0];
        IReadOnlyList<int> row = [tileId, .. ground.Rows[0].Skip(1)];
        return layout with { Layers = [ground with { Rows = [row, .. ground.Rows.Skip(1)] }, .. layout.Layers.Skip(1)] };
    }
}
