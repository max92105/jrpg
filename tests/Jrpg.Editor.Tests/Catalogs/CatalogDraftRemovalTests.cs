using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Catalogs;

/// <summary>Taking records out of the catalog, each as one undo step (decision 443).</summary>
public sealed class CatalogDraftRemovalTests
{
    private readonly CatalogDraft _draft = new(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world")));

    [Fact]
    public void RemoveObject_KnownObject_LeavesTheCatalogAsOneUndoStep()
    {
        _draft.RemoveObject("chest");

        Assert.False(_draft.Current.Objects.ContainsKey("chest"));
        Assert.True(_draft.IsDirty);
        Assert.True(_draft.CanUndo);
    }

    [Fact]
    public void Undo_AfterRemoveObject_BringsItBack()
    {
        var chest = _draft.Current.Objects["chest"];
        _draft.RemoveObject("chest");

        _draft.Undo();

        Assert.Same(chest, _draft.Current.Objects["chest"]);
        Assert.False(_draft.IsDirty);
    }

    [Fact]
    public void RemoveObject_UnknownId_Throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => _draft.RemoveObject("no_such_object"));

        Assert.Equal("No object 'no_such_object' to remove.", exception.Message);
        Assert.False(_draft.IsDirty);
    }

    // Ids never change once used, so a removed object's id can come back as a different record: the undo keeps the old
    // one.
    [Fact]
    public void Undo_AfterRemoveThenSetObjectWithTheSameId_RestoresTheRemovedRecord()
    {
        var chest = _draft.Current.Objects["chest"];
        _draft.RemoveObject("chest");
        _draft.SetObject(chest with { Category = "Other" });

        _draft.Undo();
        _draft.Undo();

        Assert.Same(chest, _draft.Current.Objects["chest"]);
    }
}
