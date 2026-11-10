using Jrpg.Data.Content;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Tests.Shell;

/// <summary>
/// New folder and Move to folder through the editor (decisions 533, 534), on the test world, which opens on the forest
/// with the start map hidden.
/// </summary>
public sealed class MainViewModelMapFoldersTests : IDisposable
{
    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void TryMoveMap_HiddenMap_ShowsItAndMovesIt()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var choice = viewModel.StartMovingMap("start");
        choice.PathText = "Act 1";

        Assert.True(viewModel.TryMoveMap("start", choice));

        Assert.Equal("start", viewModel.CurrentMap!.Map.Id);
        Assert.Equal("Act 1", viewModel.CurrentMap.Map.Settings.Folder);
        Assert.Contains(viewModel.MapEntries, entry => entry is { Id: "start", Folder: "Act 1" } && entry.Text.EndsWith(" *", StringComparison.Ordinal));
    }

    // A second edit doesn't flip the unsaved mark; the row still moves, as the list is rebuilt on every step.
    [Fact]
    public void TryMoveMapTo_MapAlreadyUnsaved_MovesItsRowAtOnce()
    {
        var viewModel = _content.Open(_content.TestWorld);
        EditorTestContent.PaintTopLeft(viewModel);

        viewModel.TryMoveMapTo("forest", "Act 1");

        Assert.Contains(viewModel.MapEntries, entry => entry is { Id: "forest", Folder: "Act 1" });
    }

    [Fact]
    public void TryMoveMapTo_MapAlreadyUnsaved_Undo_PutsItsRowBack()
    {
        var viewModel = _content.Open(_content.TestWorld);
        EditorTestContent.PaintTopLeft(viewModel);
        viewModel.TryMoveMapTo("forest", "Act 1");

        viewModel.CurrentMap!.UndoCommand.Execute(null);

        Assert.Contains(viewModel.MapEntries, entry => entry is { Id: "forest", Folder: null });
    }

    [Fact]
    public void TryMoveMapTo_SameFolderHiddenMap_StaysOnTheShownMap()
    {
        var viewModel = _content.Open(_content.TestWorld);

        Assert.False(viewModel.TryMoveMapTo("start", null));

        Assert.Equal("forest", viewModel.CurrentMap!.Map.Id);
        Assert.False(viewModel.Navigation.GoBackCommand.CanExecute(null));
    }

    [Fact]
    public void TryMoveMap_Moved_Undo_PutsItBack()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var choice = viewModel.StartMovingMap("start");
        choice.PathText = "Act 1";
        viewModel.TryMoveMap("start", choice);

        viewModel.CurrentMap!.UndoCommand.Execute(null);

        Assert.Null(viewModel.CurrentMap.Map.Settings.Folder);
        Assert.False(viewModel.CurrentMap.IsDirty);
        Assert.Contains(viewModel.MapEntries, entry => entry is { Id: "start", Folder: null });
    }

    [Fact]
    public void TryMoveMap_SameFolder_RecordsNothing()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var choice = viewModel.StartMovingMap("forest");

        Assert.False(viewModel.TryMoveMap("forest", choice));

        Assert.False(viewModel.CurrentMap!.IsDirty);
    }

    [Fact]
    public void TryMoveMap_PathWithAProblem_RecordsNothing()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var choice = viewModel.StartMovingMap("forest");
        choice.PathText = "Act 1/";

        Assert.False(viewModel.TryMoveMap("forest", choice));

        Assert.False(viewModel.CurrentMap!.IsDirty);
    }

    [Fact]
    public void TryMoveMap_Saved_WritesTheFolder()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var choice = viewModel.StartMovingMap("forest");
        choice.PathText = "Act 1/Woods";
        viewModel.TryMoveMap("forest", choice);

        viewModel.SaveCommand.Execute(null);

        Assert.Null(viewModel.SaveError);
        Assert.Equal("Act 1/Woods", ContentCatalogLoader.Load(_content.ContentCopy).Maps["forest"].Settings.Folder);
    }

    [Fact]
    public void TryMoveMapTo_Top_ClearsTheFolder()
    {
        var viewModel = _content.Open(_content.TestWorld);
        viewModel.TryMoveMapTo("forest", "Act 1");

        Assert.True(viewModel.TryMoveMapTo("forest", null));

        Assert.Null(viewModel.CurrentMap!.Map.Settings.Folder);
    }

    [Fact]
    public void TryMoveMapTo_UnknownMap_RecordsNothing()
    {
        var viewModel = _content.Open(_content.TestWorld);

        Assert.False(viewModel.TryMoveMapTo("cave", "Act 1"));

        Assert.Equal("forest", viewModel.CurrentMap!.Map.Id);
        Assert.False(viewModel.CurrentMap.IsDirty);
    }

    [Fact]
    public void StartMovingMap_MapInAFolder_StartsOnIt()
    {
        var viewModel = _content.Open(_content.TestWorld);
        viewModel.TryMoveMapTo("forest", "Act 1");

        Assert.Equal("Act 1", viewModel.StartMovingMap("forest").PathText);
    }

    [Fact]
    public void TryAddFolder_Empty_ShownInTheTree()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var choice = viewModel.StartNewFolder(null);
        choice.PathText = "Act 2";

        Assert.True(viewModel.TryAddFolder(choice));

        Assert.Contains(viewModel.Navigation.ShownTree, row => row is { IsFolder: true, FolderPath: "Act 2" });
    }

    [Fact]
    public void TryAddFolder_BadPath_AddsNothing()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var choice = viewModel.StartNewFolder(null);
        choice.PathText = "Act 2/";

        Assert.False(viewModel.TryAddFolder(choice));

        Assert.DoesNotContain(viewModel.Navigation.ShownTree, row => row.IsFolder);
    }

    [Fact]
    public void TryAddFolder_Top_AddsNothing()
    {
        var viewModel = _content.Open(_content.TestWorld);

        Assert.False(viewModel.TryAddFolder(viewModel.StartNewFolder(null)));

        Assert.DoesNotContain(viewModel.Navigation.ShownTree, row => row.IsFolder);
    }

    [Fact]
    public void StartNewFolder_UnderAFolder_OffersItsPath()
    {
        var viewModel = _content.Open(_content.TestWorld);

        Assert.Equal("Act 1/", viewModel.StartNewFolder("Act 1").PathText);
    }

    // The session's folder and the moved map's are offered, each once.
    [Fact]
    public void StartMovingMap_SessionAndMapFolders_Offered()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var newFolder = viewModel.StartNewFolder(null);
        newFolder.PathText = "Act 2";
        viewModel.TryAddFolder(newFolder);
        viewModel.TryMoveMapTo("start", "Act 1");

        Assert.Equal([MapFolderChoiceViewModel.Top, "Act 1", "Act 2"], viewModel.StartMovingMap("forest").Folders);
    }
}
