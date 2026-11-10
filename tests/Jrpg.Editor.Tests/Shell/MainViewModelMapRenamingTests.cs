using Jrpg.Core.Events;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Shell;

namespace Jrpg.Editor.Tests.Shell;

/// <summary>
/// Map > Rename map through the editor (decision 530), on a copy of the test world: the forest, which start warps into,
/// is renamed to the woods and every path follows it.
/// </summary>
public sealed class MainViewModelMapRenamingTests : IDisposable
{
    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void TryRenameMap_Saved_WritesAndShowsTheMapUnderItsNewId()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var renaming = viewModel.StartRenamingMap()!;
        renaming.NewId = "woods";

        Assert.True(viewModel.TryRenameMap(renaming));

        Assert.Equal("woods", viewModel.CurrentMap!.Map.Id);
        Assert.Contains(viewModel.MapEntries, entry => entry is { Id: "woods", Text: "Forest (woods)" });
        Assert.True(File.Exists(Path.Combine(_content.ContentCopy, "maps", "woods.json")));
        Assert.False(File.Exists(Path.Combine(_content.ContentCopy, "map-layouts", "forest.json")));
        Assert.Null(viewModel.SaveError);
    }

    [Fact]
    public void TryRenameMap_AnotherMapUnsaved_RefusesAndWritesNothing()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        viewModel.TryOpenMap("start");
        EditorTestContent.PaintTopLeft(viewModel);
        viewModel.TryOpenMap("forest");
        var renaming = new MapRenamingViewModel(viewModel.CurrentMap!.Map, []) { NewId = "woods" };

        Assert.False(viewModel.TryRenameMap(renaming));

        Assert.Equal("Save all first (Ctrl+Shift+S): a rename writes every map that leads to this one.", viewModel.SaveError);
        Assert.True(File.Exists(Path.Combine(_content.ContentCopy, "map-layouts", "forest.json")));
        Assert.Equal("forest", viewModel.CurrentMap.Map.Id);
    }

    [Fact]
    public void TryRenameMap_IdWithAProblem_WritesNothing()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var renaming = viewModel.StartRenamingMap()!;
        renaming.NewId = "Woods";

        Assert.False(viewModel.TryRenameMap(renaming));

        Assert.True(File.Exists(Path.Combine(_content.ContentCopy, "map-layouts", "forest.json")));
        Assert.Equal("forest", viewModel.CurrentMap!.Map.Id);
    }

    [Fact]
    public void CanRenameMap_AnythingUnsaved_IsFalse()
    {
        var viewModel = _content.Open(_content.TestWorld);
        Assert.True(viewModel.CanRenameMap);

        EditorTestContent.BlockFirstWalkableTile(viewModel);

        Assert.False(viewModel.CanRenameMap);
        Assert.Null(viewModel.StartRenamingMap());
        Assert.StartsWith("Save all first", viewModel.RenameMapTip, StringComparison.Ordinal);
    }

    [Fact]
    public void CanRenameMap_PropertyChangedWhenAMapIsEdited_IsRaised()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var raised = new List<string?>();
        viewModel.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        EditorTestContent.PaintTopLeft(viewModel);

        Assert.Contains(nameof(MainViewModel.CanRenameMap), raised);
        Assert.Contains(nameof(MainViewModel.RenameMapTip), raised);
    }

    // A save keeps its steps undoable; the rename starts every history afresh (decision 442's pattern).
    [Fact]
    public void TryRenameMap_Renamed_NothingToUndo()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        EditorTestContent.PaintTopLeft(viewModel);
        viewModel.SaveCommand.Execute(null);
        Assert.True(viewModel.History!.UndoCommand.CanExecute(null));

        Rename(viewModel, "woods");

        Assert.False(viewModel.History.UndoCommand.CanExecute(null));
    }

    [Fact]
    public void TryRenameMap_Renamed_WarpStepsOfOpenMapsFollow()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        viewModel.TryOpenMap("start");
        viewModel.TryOpenMap("forest");

        Rename(viewModel, "woods");

        viewModel.TryOpenMap("start");
        var warps = viewModel.CurrentMap!.Map.Layout.CellEvents.SelectMany(cellEvent => cellEvent.Event.Steps).OfType<WarpStep>();
        Assert.Contains(warps, warp => warp.MapId == "woods");
        Assert.DoesNotContain(warps, warp => warp.MapId == "forest");
    }

    [Fact]
    public void TryRenameMap_ThenGoBack_ReturnsToTheMapShownBefore()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        viewModel.TryOpenMap("start");
        viewModel.TryOpenMap("forest");
        Rename(viewModel, "woods");

        viewModel.Navigation.GoBackCommand.Execute(null);

        Assert.Equal("start", viewModel.CurrentMap!.Map.Id);
    }

    [Fact]
    public void TryRenameMap_ThenGoBackTwice_ReturnsToTheRenamedMap()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        viewModel.TryOpenMap("start");
        viewModel.TryOpenMap("forest");
        Rename(viewModel, "woods");

        viewModel.Navigation.GoBackCommand.Execute(null);
        viewModel.Navigation.GoBackCommand.Execute(null);

        Assert.Equal("woods", viewModel.CurrentMap!.Map.Id);
    }

    private static void Rename(MainViewModel viewModel, string newId)
    {
        var renaming = viewModel.StartRenamingMap()!;
        renaming.NewId = newId;
        Assert.True(viewModel.TryRenameMap(renaming));
    }
}
