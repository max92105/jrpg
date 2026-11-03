using Jrpg.Core.Geometry;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps.Editing;
using Jrpg.Editor.Shell;

namespace Jrpg.Editor.Tests.Shell;

/// <summary>Saving maps and catalogs from the editor, on a copy of the test world.</summary>
public sealed class MainViewModelSaveTests : IDisposable
{
    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void Save_ValidEdit_WritesTheMapAndClearsUnsavedChanges()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var painted = EditorTestContent.PaintTopLeft(viewModel);

        viewModel.SaveCommand.Execute(null);

        Assert.False(viewModel.CurrentMap!.IsDirty);
        Assert.Null(viewModel.SaveError);
        Assert.Equal(painted, ContentCatalogLoader.Load(_content.ContentCopy).Maps["forest"].Layout.Layers[0].TileIdAt(new GridCell(0, 0)));
    }

    [Fact]
    public void Save_MapTheGameWouldReject_ShowsTheLoaderMessageAndStaysUnsaved()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var forest = viewModel.CurrentMap!;
        forest.Do(new SetTilesEdit(forest.Map.Layout.Layers[0].Z, [new TileChange(new GridCell(0, 0), forest.Map.Layout.Layers[0].TileIdAt(new GridCell(0, 0)), 99_999)]));

        viewModel.SaveCommand.Execute(null);

        Assert.StartsWith("map-layouts/forest.json: ", viewModel.SaveError, StringComparison.Ordinal);
        Assert.True(forest.IsDirty);
    }

    [Fact]
    public void Save_ObjectOutlineChanged_LoadsBackWithTheNewOutline()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var placing = viewModel.Placing!;
        placing.Browser.SelectedObject = viewModel.Catalog!.Objects["chest"];
        var shapes = placing.StartEditingShapes()!;
        shapes.ClearShapeCommand.Execute(null);
        placing.ApplyShapes(shapes);

        viewModel.SaveCommand.Execute(null);

        Assert.Null(viewModel.SaveError);
        Assert.Empty(ContentCatalogLoader.Load(_content.ContentCopy).Objects["chest"].CollisionPolygon);
    }

    [Fact]
    public void Save_CatalogChangedMapUnchanged_WritesTilesAndClearsUnsavedChanges()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var blockedId = EditorTestContent.BlockFirstWalkableTile(viewModel);

        viewModel.SaveCommand.Execute(null);

        Assert.Null(viewModel.SaveError);
        Assert.False(viewModel.HasUnsavedChanges);
        Assert.False(ContentCatalogLoader.Load(_content.ContentCopy).Tiles[blockedId].IsWalkable);
    }

    [Fact]
    public void Save_MapPaintedWithNewTile_WritesTileAndMapThatLoadBack()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var cell = _content.ChooseCellNotATile(viewModel);
        viewModel.Painting!.StartPainting(new GridCell(0, 0));
        viewModel.Painting.EndStroke();

        viewModel.SaveCommand.Execute(null);

        Assert.Null(viewModel.SaveError);
        var loaded = ContentCatalogLoader.Load(_content.ContentCopy);
        var tile = loaded.Tiles.Values.Single(tile => new SheetCell(tile.SheetId, tile.Column, tile.Row) == cell);
        var layer = loaded.Maps["forest"].Layout.Layers.Single(layer => layer.Z == viewModel.Layer.Z);
        Assert.Equal(tile.Id, layer.TileIdAt(new GridCell(0, 0)));
    }

    // Art is the game's startup check (decision 442): the save goes through and the status band warns.
    [Fact]
    public void Save_CatalogTheArtCheckRefuses_SavesAndWarns()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        viewModel.Content!.AddTile(new SheetCell(viewModel.Catalog!.Tilesheets.Keys.First(), 9999, 0));
        EditorTestContent.PaintTopLeft(viewModel);

        viewModel.SaveCommand.Execute(null);

        Assert.Null(viewModel.SaveError);
        Assert.False(viewModel.CurrentMap!.IsDirty);
        Assert.Contains("lies outside", viewModel.Art!.Warning, StringComparison.Ordinal);
    }

    [Fact]
    public void Save_NewMap_WritesItAndAddsItToTheList()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var newMap = viewModel.StartNewMap()!;
        newMap.Name = "Old Mill";
        viewModel.TryCreateMap(newMap);

        viewModel.SaveCommand.Execute(null);

        Assert.Null(viewModel.SaveError);
        Assert.Contains("old_mill", viewModel.MapIds);
        Assert.Equal(viewModel.MapIds.Order(StringComparer.Ordinal), viewModel.MapIds);
        Assert.Equal(viewModel.MapIds, viewModel.EventEditing!.MapIds);
        Assert.False(viewModel.CurrentMap!.IsDirty);
        Assert.Equal("Old Mill", ContentCatalogLoader.Load(_content.ContentCopy).Maps["old_mill"].Settings.DisplayName);
    }

    [Fact]
    public void Save_NewMap_AddsItsEntry()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var newMap = viewModel.StartNewMap()!;
        newMap.Name = "Old Mill";
        viewModel.TryCreateMap(newMap);

        viewModel.SaveCommand.Execute(null);

        Assert.Contains(new MapEntry("old_mill", "Old Mill (old_mill)"), viewModel.MapEntries);
    }
}
