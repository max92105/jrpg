using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Art;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Maps.Editing;
using Jrpg.Editor.Tests.Shell;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Tests.Tiles;

/// <summary>
/// Remove sheet on the city sheet of a test-world copy whose <c>City.png</c> is deleted (decision 442): the purge,
/// asked first, written at once; refused while anything is unsaved or a staying event names an object it would remove.
/// </summary>
public sealed class SheetRemovalMissingImageTests : IDisposable
{
    private readonly EditorTestContent _content = new();
    private readonly string _root;

    public SheetRemovalMissingImageTests()
    {
        _root = _content.CopyOfTestWorld();
        File.Delete(Path.Combine(ArtFolders.RootIn(_root), ArtFolders.Tilesheets, "City.png"));
    }

    public void Dispose() => _content.Dispose();

    [Fact]
    public void RemoveSheet_MissingImageUsedOnMaps_AsksWithTheCounts()
    {
        var (removal, draft) = CityRemoval(ContentCatalogLoader.Load(_root));
        var expected = SheetPurge.Purge(draft.Current, "city");

        removal.RemoveSheetCommand.Execute(null);

        var question = removal.PurgeQuestion!;
        Assert.StartsWith("Sheet city's image is missing. Removing it takes out its ", question.Text, StringComparison.Ordinal);
        Assert.Contains($"erases {expected.Cells} cells on ", question.Text, StringComparison.Ordinal);
        Assert.EndsWith("This is written at once and can't be undone. Remove?", question.Text, StringComparison.Ordinal);
        Assert.Equal(expected.ChangedMapIds, question.Purge.ChangedMapIds);
        Assert.Contains($"with their {expected.Placements} placements ({expected.PlacementsWithEvents} with events)", question.Text, StringComparison.Ordinal);
    }

    [Fact]
    public void ApplyPurge_Asked_WritesAndRemovesEverything()
    {
        var (removal, draft) = CityRemoval(ContentCatalogLoader.Load(_root));
        removal.RemoveSheetCommand.Execute(null);

        removal.ApplyPurge();

        var written = ContentCatalogLoader.Load(_root);
        Assert.False(written.Tilesheets.ContainsKey("city"));
        Assert.DoesNotContain(written.Tiles.Values, tile => tile.SheetId == "city");
        Assert.Equal(written.Tilesheets.Keys.Order(StringComparer.Ordinal), draft.Current.Tilesheets.Keys.Order(StringComparer.Ordinal));
        Assert.False(draft.IsDirty);
        Assert.Equal("Removed sheet city and everything drawn from it; saved.", removal.Status);
    }

    [Fact]
    public void DropPurge_Asked_ChangesNothing()
    {
        var (removal, draft) = CityRemoval(ContentCatalogLoader.Load(_root));
        var before = draft.Current;
        var tilesBefore = File.ReadAllBytes(Path.Combine(_root, "tiles.json"));
        removal.RemoveSheetCommand.Execute(null);

        removal.DropPurge();

        Assert.Null(removal.PurgeQuestion);
        Assert.Same(before, draft.Current);
        Assert.Equal(tilesBefore, File.ReadAllBytes(Path.Combine(_root, "tiles.json")));
    }

    [Fact]
    public void RemoveSheet_UnsavedChanges_RefusesAskingToSave()
    {
        var (removal, draft) = CityRemoval(ContentCatalogLoader.Load(_root));
        draft.SetWalkable(draft.Current.Tiles.Values.First(tile => tile.IsWalkable).Id, isWalkable: false);

        removal.RemoveSheetCommand.Execute(null);

        Assert.Null(removal.PurgeQuestion);
        Assert.Equal("Save first: removing sheet city writes the content at once.", removal.Status);
    }

    // The catalogs are saved, but the open map has an unsaved edit: the purge would write and reopen it, losing the
    // edit.
    [Fact]
    public void RemoveSheet_OpenMapUnsaved_RefusesAskingToSave()
    {
        var loaded = ContentCatalogLoader.Load(_root);
        var openMap = new MapViewModel(loaded.Maps["forest"]);
        var ground = openMap.Map.Layout.Layers[0];
        var before = ground.TileIdAt(new GridCell(0, 0));
        var after = ground.Rows.SelectMany(row => row).First(id => id != before && id != 0);
        openMap.Do(new SetTilesEdit(ground.Z, [new TileChange(new GridCell(0, 0), before, after)]));
        var (removal, _) = CityRemoval(loaded, openMap);

        removal.RemoveSheetCommand.Execute(null);

        Assert.Null(removal.PurgeQuestion);
        Assert.Equal("Save first: removing sheet city writes the content at once.", removal.Status);
    }

    // A crate made of a city tile, and a swap to it on a placed object that isn't made of city tiles, so it stays.
    [Fact]
    public void RemoveSheet_AnEventSwapsToAnObjectItWouldRemove_RefusesNamingIt()
    {
        var loaded = ContentCatalogLoader.Load(_root);
        var cityTile = loaded.Tiles.Values.First(tile => tile.SheetId == "city").Id;
        var crate = loaded.Objects["chest"] with { Id = "city_crate", Tiles = [new ObjectTile(0, 0, cityTile)] };
        var start = loaded.Maps["start"];
        var staying = start.Layout.Objects.First(placed => loaded.Objects[placed.ObjectId].Tiles.All(tile => loaded.Tiles[tile.TileId].SheetId != "city"));
        var swap = new MapEvent { Id = "swap_to_crate", Activation = EventActivation.Interact, Prompt = "Swap", Steps = [new SwapObjectStep("city_crate")] };
        var layout = start.Layout with { Objects = [.. start.Layout.Objects.Select(placed => placed == staying ? placed with { Events = [.. placed.Events, swap] } : placed)] };
        var catalog = loaded with
        {
            Objects = new Dictionary<string, ObjectDefinition>(loaded.Objects) { ["city_crate"] = crate },
            Maps = new Dictionary<string, MapDefinition>(loaded.Maps) { ["start"] = new MapDefinition(layout, start.Settings) },
        };
        var (removal, _) = CityRemoval(catalog);

        removal.RemoveSheetCommand.Execute(null);

        Assert.Null(removal.PurgeQuestion);
        Assert.Equal("Can't remove sheet city: an event on map start names object city_crate, which is made of its tiles.", removal.Status);
    }

    // City.png back in the folder: the sheet is used, so E3a's rule refuses with no question.
    [Fact]
    public void RemoveSheet_ImagePresentAndUsed_StillRefuses()
    {
        File.Copy(Path.Combine(ArtFolders.RootIn(_content.TestWorld), ArtFolders.Tilesheets, "City.png"), Path.Combine(ArtFolders.RootIn(_root), ArtFolders.Tilesheets, "City.png"));
        var (removal, _) = CityRemoval(ContentCatalogLoader.Load(_root));

        removal.RemoveSheetCommand.Execute(null);

        Assert.Null(removal.PurgeQuestion);
        Assert.StartsWith("Can't remove sheet city: its tiles are used on ", removal.Status, StringComparison.Ordinal);
    }

    private (SheetRemoval Removal, CatalogDraft Draft) CityRemoval(ContentCatalog catalog, MapViewModel? openMap = null)
    {
        var draft = new CatalogDraft(catalog);
        var artRoot = ArtFolders.RootIn(_root);
        var images = new SheetImages(() => [.. ArtFolders.TilesheetImagesIn(artRoot)], file => PngSize.Read(Path.Combine(artRoot, ArtFolders.Tilesheets, file)));
        var browser = new TileBrowserViewModel(draft, images, _ => true, () => openMap?.Map.Layout);
        browser.SelectedChoice = browser.SheetChoices.Single(choice => choice.Sheet?.Id == "city");
        return (new SheetRemoval(draft, browser, () => openMap, new ContentWriter(_root)), draft);
    }
}
