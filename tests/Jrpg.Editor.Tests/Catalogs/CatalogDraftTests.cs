using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Catalogs;

public sealed class CatalogDraftTests
{
    private readonly CatalogDraft _draft = new(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content")));

    [Fact]
    public void AddSavedMap_NewMap_JoinsCurrent()
    {
        var forest = _draft.Current.Maps["forest"];
        var copy = new MapDefinition(forest.Layout with { Id = "forest_copy" }, forest.Settings with { Id = "forest_copy" });

        _draft.AddSavedMap(copy);

        Assert.Same(copy, _draft.Current.Maps["forest_copy"]);
        Assert.Same(forest, _draft.Current.Maps["forest"]);
    }

    [Fact]
    public void AddSavedMap_ExistingMap_ReplacesIt()
    {
        var forest = _draft.Current.Maps["forest"];
        var renamed = new MapDefinition(forest.Layout, forest.Settings with { DisplayName = "Deep forest" });

        _draft.AddSavedMap(renamed);

        Assert.Same(renamed, _draft.Current.Maps["forest"]);
    }
}
