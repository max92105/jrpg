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
            ["chest", "chest_opened", "door_close", "door_open", "tree", "tree_2", "tree_3", "house_1", "house_2", "house_3", "house_4", "house_5"],
            objects.Select(definition => definition.Id));
    }
}
