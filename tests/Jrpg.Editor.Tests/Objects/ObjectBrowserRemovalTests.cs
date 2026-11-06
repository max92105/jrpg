using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Tests.Objects;

/// <summary>
/// The Objects panel's Remove (decision 443): an object nothing uses leaves as one undo step; one a map places or an
/// event swaps to stays, and the panel says which map. The open map counts as it is on screen, in place of its saved
/// file. A crate of the tests' own, placed nowhere in the test world, so the test world's own objects don't matter.
/// </summary>
public sealed class ObjectBrowserRemovalTests
{
    private static readonly ContentCatalog _testWorld = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    private static readonly ObjectDefinition _crate = new()
    {
        Id = "crate",
        Width = 1,
        Height = 1,
        CollisionPolygon = [],
        Category = "General",
        Tiles = [],
        AbovePlayerZones = [],
    };

    private static readonly ContentCatalog _catalog = _testWorld with { Objects = new Dictionary<string, ObjectDefinition>(_testWorld.Objects) { ["crate"] = _crate } };

    [Fact]
    public void RemoveSelected_UnusedObject_RemovesItAndSaysSo()
    {
        var draft = new CatalogDraft(_catalog);
        var browser = BrowserOn(draft, openLayout: null);

        browser.RemoveSelectedCommand.Execute(null);

        Assert.False(draft.Current.Objects.ContainsKey("crate"));
        Assert.Equal("Removed object crate.", browser.RemovalStatus);
        Assert.Null(browser.SelectedObject);
    }

    [Fact]
    public void RemoveSelected_PlacedOnASavedMap_RefusesNamingTheMap()
    {
        var draft = new CatalogDraft(WithCratePlacedOnSavedForest(_catalog));
        var browser = BrowserOn(draft, openLayout: null);

        browser.RemoveSelectedCommand.Execute(null);

        Assert.Equal("Can't remove object crate: it is used on map forest.", browser.RemovalStatus);
        Assert.True(draft.Current.Objects.ContainsKey("crate"));
    }

    [Fact]
    public void RemoveSelected_PlacedOnlyInTheOpenMapsUnsavedChanges_Refuses()
    {
        var draft = new CatalogDraft(_catalog);
        var browser = BrowserOn(draft, WithCratePlaced(_catalog.Maps["start"].Layout));

        browser.RemoveSelectedCommand.Execute(null);

        Assert.Equal("Can't remove object crate: it is used on map start.", browser.RemovalStatus);
        Assert.True(draft.Current.Objects.ContainsKey("crate"));
    }

    // The open forest no longer places the crate; its saved file still does, but the screen is what counts.
    [Fact]
    public void RemoveSelected_PlacementRemovedFromTheOpenMapButStillSaved_Removes()
    {
        var saved = WithCratePlacedOnSavedForest(_catalog);
        var draft = new CatalogDraft(saved);
        var browser = BrowserOn(draft, _catalog.Maps["forest"].Layout);

        browser.RemoveSelectedCommand.Execute(null);

        Assert.False(draft.Current.Objects.ContainsKey("crate"));
    }

    [Fact]
    public void RemoveSelected_SwappedToByAnEvent_RefusesNamingTheMap()
    {
        var draft = new CatalogDraft(_catalog);
        var start = _catalog.Maps["start"].Layout;
        var first = start.Objects[0];
        var swapping = new MapEvent { Id = $"{first.Uid}_swap", Activation = EventActivation.Interact, Prompt = "Swap", Steps = [new SwapObjectStep("crate")] };
        var browser = BrowserOn(draft, start with { Objects = [first with { Events = [.. first.Events, swapping] }, .. start.Objects.Skip(1)] });

        browser.RemoveSelectedCommand.Execute(null);

        Assert.Equal("Can't remove object crate: it is used on map start.", browser.RemovalStatus);
    }

    [Fact]
    public void RemoveSelected_Refused_LeavesTheCatalogUnchanged()
    {
        var draft = new CatalogDraft(WithCratePlacedOnSavedForest(_catalog));
        var before = draft.Current;
        var browser = BrowserOn(draft, openLayout: null);

        browser.RemoveSelectedCommand.Execute(null);

        Assert.Same(before, draft.Current);
        Assert.False(draft.CanUndo);
    }

    [Fact]
    public void RemoveSelectedCommand_NothingChosen_CannotExecute()
    {
        var browser = new ObjectBrowserViewModel(new CatalogDraft(_catalog), () => []);

        Assert.False(browser.RemoveSelectedCommand.CanExecute(null));
    }

    [Fact]
    public void RemovalStatus_ChoiceChanges_IsCleared()
    {
        var draft = new CatalogDraft(WithCratePlacedOnSavedForest(_catalog));
        var browser = BrowserOn(draft, openLayout: null);
        browser.RemoveSelectedCommand.Execute(null);
        Assert.NotNull(browser.RemovalStatus);

        browser.SelectedObject = browser.Objects.First(definition => definition.Id == "chest");

        Assert.Null(browser.RemovalStatus);
    }

    [Fact]
    public void RemovalStatus_UndoAfterRemove_IsCleared()
    {
        var draft = new CatalogDraft(_catalog);
        var browser = BrowserOn(draft, openLayout: null);
        browser.RemoveSelectedCommand.Execute(null);
        Assert.NotNull(browser.RemovalStatus);

        draft.Undo();

        Assert.Null(browser.RemovalStatus);
        Assert.Contains(browser.Objects, definition => definition.Id == "crate");
    }

    private static ObjectBrowserViewModel BrowserOn(CatalogDraft draft, MapLayout? openLayout)
    {
        var browser = new ObjectBrowserViewModel(draft, () => openLayout is null ? [] : [openLayout]);
        browser.SelectedObject = browser.Objects.First(definition => definition.Id == "crate");
        return browser;
    }

    private static MapLayout WithCratePlaced(MapLayout layout) =>
        layout with { Objects = [.. layout.Objects, new PlacedObject { Uid = "crate_1", ObjectId = "crate", Position = new Point2D(1, 1), Z = 1 }] };

    private static ContentCatalog WithCratePlacedOnSavedForest(ContentCatalog catalog)
    {
        var forest = catalog.Maps["forest"];
        var placing = new MapDefinition(WithCratePlaced(forest.Layout), forest.Settings);
        return catalog with { Maps = new Dictionary<string, MapDefinition>(catalog.Maps) { ["forest"] = placing } };
    }
}
