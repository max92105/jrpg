using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Catalogs;

public sealed class CatalogHistoryTests
{
    private static readonly CatalogEdit _first = new(catalog => catalog, catalog => catalog);
    private static readonly CatalogEdit _second = new(catalog => catalog, catalog => catalog);

    private readonly CatalogHistory _history = new();

    [Fact]
    public void Undo_AfterOneEdit_ReturnsIt()
    {
        _history.Push(_first);

        Assert.Same(_first, _history.Undo());
        Assert.False(_history.CanUndo);
    }

    [Fact]
    public void Redo_AfterUndo_ReturnsTheSameEdit()
    {
        _history.Push(_first);
        _history.Undo();

        Assert.Same(_first, _history.Redo());
        Assert.False(_history.CanRedo);
    }

    [Fact]
    public void Push_AfterUndo_ClearsRedo()
    {
        _history.Push(_first);
        _history.Undo();

        _history.Push(_second);

        Assert.False(_history.CanRedo);
    }
}
