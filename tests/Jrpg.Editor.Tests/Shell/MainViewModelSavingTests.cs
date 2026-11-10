using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Data.Content;
using Jrpg.Editor.Shell;

namespace Jrpg.Editor.Tests.Shell;

/// <summary>Save all and the close question, on a copy of the test world (decisions 500, 501, 518).</summary>
public sealed class MainViewModelSavingTests : IDisposable
{
    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void SaveAll_TwoMapsChanged_WritesBothAndClearsTheMarks()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var paintedForest = EditorTestContent.PaintTopLeft(viewModel);
        viewModel.TryOpenMap("start");
        var paintedStart = EditorTestContent.PaintTopLeft(viewModel);

        viewModel.SaveAllCommand.Execute(null);

        var saved = ContentCatalogLoader.Load(_content.ContentCopy);
        Assert.Equal(paintedForest, TopLeftOf(saved, "forest"));
        Assert.Equal(paintedStart, TopLeftOf(saved, "start"));
        Assert.DoesNotContain(viewModel.MapEntries, entry => entry.Text.EndsWith(" *", StringComparison.Ordinal));
        Assert.False(viewModel.HasUnsavedChanges);
    }

    [Fact]
    public void SaveAll_OnlyTilesChanged_WritesTheTiles()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var tileId = EditorTestContent.BlockFirstWalkableTile(viewModel);

        viewModel.SaveAllCommand.Execute(null);

        Assert.False(ContentCatalogLoader.Load(_content.ContentCopy).Tiles[tileId].IsWalkable);
        Assert.False(viewModel.Content!.IsDirty);
    }

    // All or none (decision 518): the valid forest isn't written either, whichever map is refused.
    [Fact]
    public void SaveAll_SecondMapRefused_WritesNeitherAndShowsTheReason()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var forest = viewModel.CurrentMap!;
        var before = TopLeftOf(ContentCatalogLoader.Load(_content.ContentCopy), "forest");
        EditorTestContent.PaintTopLeft(viewModel);
        viewModel.TryOpenMap("start");
        var start = viewModel.CurrentMap!;
        EditorTestContent.PaintUnknownTile(start);

        viewModel.SaveAllCommand.Execute(null);

        Assert.Equal(before, TopLeftOf(ContentCatalogLoader.Load(_content.ContentCopy), "forest"));
        Assert.True(forest.IsDirty);
        Assert.True(start.IsDirty);
        Assert.StartsWith("map-layouts/start.json: ", viewModel.SaveError, StringComparison.Ordinal);
        Assert.EndsWith(" Nothing was written.", viewModel.SaveError, StringComparison.Ordinal);
    }

    [Fact]
    public void SaveAll_FirstMapRefused_WritesNeither()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var forest = viewModel.CurrentMap!;
        EditorTestContent.PaintUnknownTile(forest);
        viewModel.TryOpenMap("start");
        var start = viewModel.CurrentMap!;
        var before = TopLeftOf(ContentCatalogLoader.Load(_content.ContentCopy), "start");
        EditorTestContent.PaintTopLeft(viewModel);

        viewModel.SaveAllCommand.Execute(null);

        Assert.True(start.IsDirty);
        Assert.True(forest.IsDirty);
        Assert.Equal(before, TopLeftOf(ContentCatalogLoader.Load(_content.ContentCopy), "start"));
        Assert.StartsWith("map-layouts/forest.json: ", viewModel.SaveError, StringComparison.Ordinal);
    }

    [Fact]
    public void SaveAll_NewMapOpen_WritesItAndListsItWithoutAMark()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var newMap = viewModel.StartNewMap()!;
        newMap.Name = "Old Mill";
        viewModel.TryCreateMap(newMap);

        viewModel.SaveAllCommand.Execute(null);

        Assert.True(ContentCatalogLoader.Load(_content.ContentCopy).Maps.ContainsKey("old_mill"));
        Assert.Contains(viewModel.MapEntries, entry => entry is { Id: "old_mill", Text: "Old Mill (old_mill)" });
    }

    [Fact]
    public void ConfirmClose_NothingUnsaved_ClosesWithoutAsking()
    {
        var viewModel = _content.Open(_content.TestWorld);

        Assert.True(viewModel.ConfirmClose());
        Assert.Empty(_content.Questions);
    }

    [Fact]
    public void ConfirmClose_TwoMapsAndTilesChanged_AsksOnceNamingEach()
    {
        var viewModel = _content.Open(_content.TestWorld);
        EditorTestContent.BlockFirstWalkableTile(viewModel);
        EditorTestContent.PaintTopLeft(viewModel);
        viewModel.TryOpenMap("start");
        EditorTestContent.PaintTopLeft(viewModel);

        viewModel.ConfirmClose();

        Assert.Equal(["Unsaved changes in map 'forest', map 'start' and the tiles and objects."], _content.Questions);
    }

    [Fact]
    public void ConfirmClose_OnlyTilesChanged_NamesThem()
    {
        var viewModel = _content.Open(_content.TestWorld);
        EditorTestContent.BlockFirstWalkableTile(viewModel);

        viewModel.ConfirmClose();

        Assert.Equal(["Unsaved changes in the tiles and objects."], _content.Questions);
    }

    [Fact]
    public void ConfirmClose_MidStroke_Asks()
    {
        var viewModel = _content.Open(_content.TestWorld);
        viewModel.Painting!.StartErasing(new GridCell(0, 0));

        Assert.False(viewModel.ConfirmClose());
        Assert.Equal(["Unsaved changes in map 'forest'."], _content.Questions);
    }

    [Fact]
    public void ConfirmClose_UserSavesAll_WritesAndCloses()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld(), UnsavedChangesAnswer.SaveAll);
        var painted = EditorTestContent.PaintTopLeft(viewModel);

        Assert.True(viewModel.ConfirmClose());
        Assert.Equal(painted, TopLeftOf(ContentCatalogLoader.Load(_content.ContentCopy), "forest"));
    }

    [Fact]
    public void ConfirmClose_UserSavesAllAndAMapIsRefused_StaysOpen()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld(), UnsavedChangesAnswer.SaveAll);
        EditorTestContent.PaintUnknownTile(viewModel.CurrentMap!);

        Assert.False(viewModel.ConfirmClose());
        Assert.NotNull(viewModel.SaveError);
    }

    [Fact]
    public void ConfirmClose_UserDiscards_ClosesWithoutWriting()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld(), UnsavedChangesAnswer.Discard);
        var before = _content.TilesAndForestBytes();
        EditorTestContent.BlockFirstWalkableTile(viewModel);
        EditorTestContent.PaintTopLeft(viewModel);

        Assert.True(viewModel.ConfirmClose());
        Assert.Equal(before, _content.TilesAndForestBytes());
    }

    [Fact]
    public void ConfirmClose_UserCancels_StaysOpen()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld(), UnsavedChangesAnswer.Cancel);
        EditorTestContent.PaintTopLeft(viewModel);

        Assert.False(viewModel.ConfirmClose());
        Assert.True(viewModel.HasUnsavedChanges);
    }

    private static int TopLeftOf(ContentCatalog catalog, string mapId) =>
        catalog.Maps[mapId].Layout.Layers[0].TileIdAt(new GridCell(0, 0));
}
