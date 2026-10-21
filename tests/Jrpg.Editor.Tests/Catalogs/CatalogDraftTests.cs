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

    [Fact]
    public void AddTile_EmptyCell_AddsWalkableTileAboveHighestId()
    {
        var highest = _draft.Current.Tiles.Keys.Max();

        var id = _draft.AddTile(EmptyCell);

        Assert.Equal(highest + 1, id);
        Assert.Equal(new TileDefinition { Id = id, IsWalkable = true, SheetId = EmptyCell.SheetId, Column = EmptyCell.Column, Row = EmptyCell.Row }, _draft.Current.Tiles[id]);
        Assert.Equal(id, _draft.TileAt(EmptyCell)?.Id);
    }

    [Fact]
    public void AddTile_EmptyCell_MarksDirty()
    {
        _draft.AddTile(EmptyCell);

        Assert.True(_draft.IsDirty);
    }

    [Fact]
    public void TileAt_CellWithoutTile_ReturnsNull()
    {
        Assert.Null(_draft.TileAt(EmptyCell));
    }

    [Fact]
    public void MarkSaved_AfterAddTile_IsClean()
    {
        _draft.AddTile(EmptyCell);

        _draft.MarkSaved();

        Assert.False(_draft.IsDirty);
    }

    [Fact]
    public void SetWalkable_ExistingTile_ReplacesItAndMarksDirty()
    {
        var tile = _draft.Current.Tiles.Values.First(tile => tile.IsWalkable);

        _draft.SetWalkable(tile.Id, isWalkable: false);

        Assert.Equal(tile with { IsWalkable = false }, _draft.Current.Tiles[tile.Id]);
        Assert.True(_draft.IsDirty);
    }

    [Fact]
    public void SetWalkable_SameValue_StaysClean()
    {
        var before = _draft.Current;
        var tile = before.Tiles.Values.First(tile => tile.IsWalkable);

        _draft.SetWalkable(tile.Id, isWalkable: true);

        Assert.Same(before, _draft.Current);
        Assert.False(_draft.IsDirty);
    }

    [Fact]
    public void Undo_AfterSetWalkable_RestoresTheTile()
    {
        var tile = _draft.Current.Tiles.Values.First(tile => tile.IsWalkable);
        _draft.SetWalkable(tile.Id, isWalkable: false);

        _draft.Undo();

        Assert.Equal(tile, _draft.Current.Tiles[tile.Id]);
    }

    [Fact]
    public void Undo_BackToSavedVersion_IsClean()
    {
        var tile = _draft.Current.Tiles.Values.First(tile => tile.IsWalkable);
        _draft.SetWalkable(tile.Id, isWalkable: false);
        _draft.MarkSaved();
        _draft.SetWalkable(tile.Id, isWalkable: true);

        _draft.Undo();

        Assert.False(_draft.IsDirty);
    }

    [Fact]
    public void Undo_NothingDone_ChangesNothing()
    {
        var before = _draft.Current;

        _draft.Undo();

        Assert.Same(before, _draft.Current);
    }

    [Fact]
    public void Undo_AddBlockedTile_RemovesTheTile()
    {
        _draft.AddBlockedTile(EmptyCell);
        Assert.False(_draft.TileAt(EmptyCell)!.IsWalkable);

        _draft.Undo();

        Assert.Null(_draft.TileAt(EmptyCell));
        Assert.False(_draft.IsDirty);
    }

    [Fact]
    public void Undo_AfterAddTile_StaysDirty()
    {
        var tile = _draft.Current.Tiles.Values.First(tile => tile.IsWalkable);
        _draft.SetWalkable(tile.Id, isWalkable: false);
        _draft.AddTile(EmptyCell);

        _draft.Undo();

        Assert.NotNull(_draft.TileAt(EmptyCell));
        Assert.True(_draft.IsDirty);
    }

    // Far past any sheet's last cell, so no tile of the repository's content is cut from it.
    private SheetCell EmptyCell => new(_draft.Current.Tilesheets.Keys.First(), 999, 999);
}
