using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Tests.Objects;

public sealed class ObjectBrowserViewModelTests
{
    [Fact]
    public void Objects_RepositoryContent_SortedByCategoryThenId()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        var objects = new ObjectBrowserViewModel(new CatalogDraft(catalog)).Objects;

        Assert.Equal(
            ["chest", "chest_opened", "door_close", "door_open", "tree", "tree_2", "tree_3", "house_1", "house_2", "house_3", "house_4", "house_5", "villager"],
            objects.Select(definition => definition.Id));
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
}
