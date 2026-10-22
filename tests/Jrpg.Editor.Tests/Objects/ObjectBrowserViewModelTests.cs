using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Tests.Objects;

public sealed class ObjectBrowserViewModelTests
{
    [Fact]
    public void Objects_TwoCategories_SortedByCategoryThenId()
    {
        ObjectDefinition[] givenOutOfOrder = [Definition("well", "Village"), Definition("bush", "Nature"), Definition("barrel", "Village"), Definition("rock", "Nature")];
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content")) with
        {
            Objects = givenOutOfOrder.ToDictionary(definition => definition.Id),
        };

        var objects = new ObjectBrowserViewModel(new CatalogDraft(catalog)).Objects;

        Assert.Equal(["bush", "rock", "barrel", "well"], objects.Select(definition => definition.Id));
    }

    [Fact]
    public void Objects_AfterSetObject_ListsTheNewRecordAndKeepsTheSelection()
    {
        var draft = new CatalogDraft(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content")));
        var browser = new ObjectBrowserViewModel(draft);
        browser.SelectedObject = browser.Objects.First(definition => definition.Id == "tree");
        var edited = browser.SelectedObject with { CollisionPolygon = [] };

        draft.SetObject(edited);

        Assert.Same(edited, browser.Objects.Single(definition => definition.Id == "tree"));
        Assert.Same(edited, browser.SelectedObject);
    }

    [Fact]
    public void Objects_ChosenObjectUndoneAway_ChoosesNothing()
    {
        var draft = new CatalogDraft(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content")));
        var browser = new ObjectBrowserViewModel(draft);
        draft.SetObject(draft.Current.Objects["tree"] with { Id = "tree_new" });
        browser.SelectedObject = browser.Objects.Single(definition => definition.Id == "tree_new");

        draft.Undo();

        Assert.Null(browser.SelectedObject);
        Assert.DoesNotContain(browser.Objects, definition => definition.Id == "tree_new");
    }

    [Fact]
    public void Objects_TileChange_KeepsTheList()
    {
        var draft = new CatalogDraft(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content")));
        var browser = new ObjectBrowserViewModel(draft);
        var listed = browser.Objects;

        draft.SetWalkable(draft.Current.Tiles.Keys.First(), isWalkable: false);

        Assert.Same(listed, browser.Objects);
    }

    private static ObjectDefinition Definition(string id, string category) => new()
    {
        Id = id,
        Width = 1,
        Height = 1,
        CollisionPolygon = [],
        Category = category,
        Tiles = [],
        AbovePlayerZones = [],
    };
}
