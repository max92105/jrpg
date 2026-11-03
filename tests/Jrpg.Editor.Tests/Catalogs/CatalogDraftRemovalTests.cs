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

    [Fact]
    public void RemoveTilesheet_WithTiles_RemovesBothAsOneUndoStep()
    {
        var sheetId = _draft.Current.Tiles.Values.First().SheetId;
        var cut = CatalogUsage.TilesCutFrom(sheetId, _draft.Current);

        _draft.RemoveTilesheet(sheetId);

        Assert.False(_draft.Current.Tilesheets.ContainsKey(sheetId));
        Assert.All(cut, tileId => Assert.False(_draft.Current.Tiles.ContainsKey(tileId)));
        Assert.True(_draft.CanUndo);
    }

    [Fact]
    public void Undo_AfterRemoveTilesheet_BringsTheSheetAndItsTilesBack()
    {
        var before = _draft.Current;
        var sheetId = _draft.Current.Tiles.Values.First().SheetId;
        _draft.RemoveTilesheet(sheetId);

        _draft.Undo();

        Assert.Equal(before.Tilesheets[sheetId], _draft.Current.Tilesheets[sheetId]);
        Assert.Equal(before.Tiles.Keys.Order(), _draft.Current.Tiles.Keys.Order());
        Assert.False(_draft.IsDirty);
    }

    [Fact]
    public void RemoveTilesheet_UnknownId_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => _draft.RemoveTilesheet("no_such_sheet"));
    }

    // The sheet holding the highest tile id goes; a tile painted next still takes a higher id than any that existed.
    [Fact]
    public void AddTile_AfterRemovingTheHighestTiles_NeverReusesTheirIds()
    {
        var highest = _draft.Current.Tiles.Keys.Max();
        var highestSheet = _draft.Current.Tiles[highest].SheetId;
        var otherSheet = _draft.Current.Tilesheets.Keys.First(id => id != highestSheet);
        _draft.RemoveTilesheet(highestSheet);

        var id = _draft.AddTile(new SheetCell(otherSheet, 999, 999));

        Assert.Equal(highest + 1, id);
    }

    [Fact]
    public void Undo_RemoveTilesheetAfterANewTileWasPainted_KeepsTheNewTile()
    {
        var highest = _draft.Current.Tiles.Keys.Max();
        var highestSheet = _draft.Current.Tiles[highest].SheetId;
        var otherSheet = _draft.Current.Tilesheets.Keys.First(id => id != highestSheet);
        _draft.RemoveTilesheet(highestSheet);
        var painted = _draft.AddTile(new SheetCell(otherSheet, 999, 999));

        _draft.Undo();

        Assert.Equal(otherSheet, _draft.Current.Tiles[painted].SheetId);
        Assert.Equal(highestSheet, _draft.Current.Tiles[highest].SheetId);
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
