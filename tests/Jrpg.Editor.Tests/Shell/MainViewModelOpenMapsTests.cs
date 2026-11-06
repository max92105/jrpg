using Jrpg.Core.Geometry;

namespace Jrpg.Editor.Tests.Shell;

/// <summary>
/// Switching maps: every map opened stays open with its changes, and nothing asks (decisions 497, 498).
/// </summary>
public sealed class MainViewModelOpenMapsTests : IDisposable
{
    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void TryOpenMap_NoUnsavedChanges_OpensItWithoutAsking()
    {
        var viewModel = _content.Open(_content.TestWorld);

        Assert.True(viewModel.TryOpenMap("start"));

        Assert.Equal("start", viewModel.CurrentMap?.Map.Id);
        Assert.Empty(_content.Questions);
    }

    [Fact]
    public void TryOpenMap_UnsavedChanges_OpensTheOtherWithoutAsking()
    {
        var viewModel = _content.Open(_content.TestWorld);
        EditorTestContent.PaintTopLeft(viewModel);

        Assert.True(viewModel.TryOpenMap("start"));

        Assert.Equal("start", viewModel.CurrentMap?.Map.Id);
        Assert.Empty(_content.Questions);
    }

    [Fact]
    public void TryOpenMap_BackToAMapLeftUnsaved_ShowsItsChanges()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var painted = EditorTestContent.PaintTopLeft(viewModel);
        viewModel.TryOpenMap("start");

        viewModel.TryOpenMap("forest");

        Assert.Equal(painted, viewModel.CurrentMap!.Map.Layout.Layers[0].TileIdAt(new GridCell(0, 0)));
        Assert.True(viewModel.CurrentMap.IsDirty);
    }

    [Fact]
    public void TryOpenMap_BackToAMap_ShowsTheSameViewModel()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var forest = viewModel.CurrentMap;
        viewModel.TryOpenMap("start");

        viewModel.TryOpenMap("forest");

        Assert.Same(forest, viewModel.CurrentMap);
    }

    [Fact]
    public void TryOpenMap_MidStroke_EndsTheStrokeOnTheMapLeft()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var forest = viewModel.CurrentMap!;
        viewModel.Painting!.StartErasing(new GridCell(0, 0));

        viewModel.TryOpenMap("start");

        Assert.True(forest.IsDirty);
    }

    [Fact]
    public void TryOpenMap_UnknownId_ReturnsFalseAndStays()
    {
        var viewModel = _content.Open(_content.TestWorld);

        Assert.False(viewModel.TryOpenMap("nowhere"));

        Assert.Equal("forest", viewModel.CurrentMap?.Map.Id);
    }

    [Fact]
    public void TryOpenMap_UnsavedCatalogChangeOnly_OpensWithoutAsking()
    {
        var viewModel = _content.Open(_content.TestWorld);
        EditorTestContent.BlockFirstWalkableTile(viewModel);

        Assert.True(viewModel.TryOpenMap("start"));

        Assert.Empty(_content.Questions);
        Assert.True(viewModel.HasUnsavedChanges);
    }

    [Fact]
    public void HasUnsavedChanges_HiddenMapChanged_IsTrue()
    {
        var viewModel = _content.Open(_content.TestWorld);
        EditorTestContent.PaintTopLeft(viewModel);

        viewModel.TryOpenMap("start");

        Assert.True(viewModel.HasUnsavedChanges);
    }

    [Fact]
    public void TryCreateMap_ValidFields_OpensItUnsavedOutsideTheSavedIds()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var newMap = viewModel.StartNewMap()!;
        newMap.Name = "Old Mill";

        Assert.True(viewModel.TryCreateMap(newMap));

        Assert.Equal("old_mill", viewModel.CurrentMap!.Map.Id);
        Assert.True(viewModel.CurrentMap.IsDirty);
        Assert.DoesNotContain("old_mill", viewModel.MapIds);
    }

    [Fact]
    public void TryCreateMap_WithAnUnsavedMapOpen_CreatesWithoutAsking()
    {
        var viewModel = _content.Open(_content.TestWorld);
        EditorTestContent.PaintTopLeft(viewModel);
        var newMap = viewModel.StartNewMap()!;
        newMap.Name = "Old Mill";

        Assert.True(viewModel.TryCreateMap(newMap));

        Assert.Equal("old_mill", viewModel.CurrentMap!.Map.Id);
        Assert.Empty(_content.Questions);
    }

    [Fact]
    public void TryCreateMap_NewMap_IsListedAndMarked()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var newMap = viewModel.StartNewMap()!;
        newMap.Name = "Old Mill";

        viewModel.TryCreateMap(newMap);

        Assert.Contains(viewModel.MapEntries, entry => entry is { Id: "old_mill", Text: "Old Mill (old_mill) *" });
    }

    [Fact]
    public void TryOpenMap_AwayFromANewMapAndBack_ShowsTheSameDraft()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var newMap = viewModel.StartNewMap()!;
        newMap.Name = "Old Mill";
        viewModel.TryCreateMap(newMap);
        var created = viewModel.CurrentMap;
        viewModel.TryOpenMap("start");

        Assert.True(viewModel.TryOpenMap("old_mill"));

        Assert.Same(created, viewModel.CurrentMap);
    }

    [Fact]
    public void StartNewMap_NameOfAnOpenNewMap_GetsAnotherId()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var first = viewModel.StartNewMap()!;
        first.Name = "Old Mill";
        viewModel.TryCreateMap(first);

        var second = viewModel.StartNewMap()!;
        second.Name = "Old Mill";

        Assert.NotEqual("old_mill", second.MapId);
        Assert.StartsWith("old_mill", second.MapId, StringComparison.Ordinal);
    }

    [Fact]
    public void MapEntries_MapPainted_MarksItsRow()
    {
        var viewModel = _content.Open(_content.TestWorld);

        EditorTestContent.PaintTopLeft(viewModel);

        Assert.EndsWith(" *", viewModel.MapEntries.Single(entry => entry.Id == "forest").Text, StringComparison.Ordinal);
        Assert.DoesNotContain(viewModel.MapEntries, entry => entry.Id != "forest" && entry.Text.EndsWith(" *", StringComparison.Ordinal));
    }

    [Fact]
    public void MapEntries_MapPaintedThenUndone_LosesItsMark()
    {
        var viewModel = _content.Open(_content.TestWorld);
        EditorTestContent.PaintTopLeft(viewModel);

        viewModel.History!.UndoCommand.Execute(null);

        Assert.DoesNotContain(viewModel.MapEntries, entry => entry.Text.EndsWith(" *", StringComparison.Ordinal));
    }

    [Fact]
    public void ConfirmClose_TwoMapsChanged_AsksOnceNamingBoth()
    {
        var viewModel = _content.Open(_content.TestWorld);
        EditorTestContent.PaintTopLeft(viewModel);
        viewModel.TryOpenMap("start");
        EditorTestContent.PaintTopLeft(viewModel);

        Assert.False(viewModel.ConfirmClose());

        Assert.Equal(["Maps 'forest' and 'start' have unsaved changes. Discard them?"], _content.Questions);
    }

    [Fact]
    public void ConfirmClose_UnsavedMapAndCatalogChanges_AsksOnceNamingBoth()
    {
        var viewModel = _content.Open(_content.TestWorld);
        EditorTestContent.BlockFirstWalkableTile(viewModel);
        EditorTestContent.PaintTopLeft(viewModel);

        Assert.False(viewModel.ConfirmClose());

        Assert.Equal(["Map 'forest' and the tiles have unsaved changes. Discard them?"], _content.Questions);
    }
}
