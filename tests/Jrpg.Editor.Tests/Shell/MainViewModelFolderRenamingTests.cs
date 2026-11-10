using Jrpg.Data.Content;
using Jrpg.Editor.Shell;

namespace Jrpg.Editor.Tests.Shell;

/// <summary>
/// Rename folder through the editor (decision 533), on a copy of the test world: the forest in "Act 1" and the start
/// map in "Act 1/Village", saved together, then "Act 1" renamed.
/// </summary>
public sealed class MainViewModelFolderRenamingTests : IDisposable
{
    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void TryRenameFolder_TwoMapsInIt_WritesBothAndShowsTheRenamedFolder()
    {
        var viewModel = OpenWithAct1Saved();

        Assert.True(RenameFolder(viewModel, "Act 1", "Chapter 1"));

        var written = ContentCatalogLoader.Load(_content.ContentCopy);
        Assert.Equal("Chapter 1", written.Maps["forest"].Settings.Folder);
        Assert.Equal("Chapter 1/Village", written.Maps["start"].Settings.Folder);
        Assert.Equal("Chapter 1", viewModel.CurrentMap!.Map.Settings.Folder);
        Assert.Contains(viewModel.MapEntries, entry => entry is { Id: "start", Folder: "Chapter 1/Village" });
        Assert.False(viewModel.HasUnsavedChanges);
        Assert.Null(viewModel.SaveError);
    }

    [Fact]
    public void TryRenameFolder_SimilarName_LeftAlone()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        viewModel.TryMoveMapTo("forest", "Act 10");
        viewModel.TryMoveMapTo("start", "Act 1");
        viewModel.SaveAllCommand.Execute(null);

        Assert.True(RenameFolder(viewModel, "Act 1", "Chapter 1"));

        Assert.Equal("Act 10", ContentCatalogLoader.Load(_content.ContentCopy).Maps["forest"].Settings.Folder);
    }

    [Fact]
    public void TryRenameFolder_AnythingUnsaved_RefusesAndWritesNothing()
    {
        var viewModel = OpenWithAct1Saved();
        EditorTestContent.PaintTopLeft(viewModel);

        Assert.False(RenameFolder(viewModel, "Act 1", "Chapter 1"));

        Assert.Equal("Save all first (Ctrl+Shift+S): renaming a folder writes every map in it.", viewModel.SaveError);
        Assert.Equal("Act 1", ContentCatalogLoader.Load(_content.ContentCopy).Maps["forest"].Settings.Folder);
    }

    [Fact]
    public void TryRenameFolder_PathUnchanged_ChangesNothing()
    {
        var viewModel = OpenWithAct1Saved();

        Assert.False(RenameFolder(viewModel, "Act 1", "Act 1"));

        Assert.Null(viewModel.SaveError);
    }

    [Fact]
    public void TryRenameFolder_EmptySessionFolder_RenamesItInTheListAlone()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var newFolder = viewModel.StartNewFolder(null);
        newFolder.PathText = "Act 2";
        viewModel.TryAddFolder(newFolder);

        Assert.True(RenameFolder(viewModel, "Act 2", "Act 3"));

        Assert.Equal(["Act 3"], viewModel.Navigation.ShownTree.Where(row => row.IsFolder).Select(row => row.Text));
        Assert.False(viewModel.HasUnsavedChanges);
    }

    // A save keeps its steps undoable; the rename starts every history afresh (decision 442's pattern).
    [Fact]
    public void TryRenameFolder_Renamed_NothingToUndo()
    {
        var viewModel = OpenWithAct1Saved();
        Assert.True(viewModel.History!.UndoCommand.CanExecute(null));

        RenameFolder(viewModel, "Act 1", "Chapter 1");

        Assert.False(viewModel.History.UndoCommand.CanExecute(null));
    }

    [Fact]
    public void CanRenameFolder_AnythingUnsaved_IsFalseAndRaised()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var raised = new List<string?>();
        viewModel.PropertyChanged += (_, e) => raised.Add(e.PropertyName);
        Assert.True(viewModel.CanRenameFolder);

        EditorTestContent.PaintTopLeft(viewModel);

        Assert.False(viewModel.CanRenameFolder);
        Assert.StartsWith("Save all first", viewModel.RenameFolderTip, StringComparison.Ordinal);
        Assert.Contains(nameof(MainViewModel.CanRenameFolder), raised);
        Assert.Contains(nameof(MainViewModel.RenameFolderTip), raised);
    }

    private MainViewModel OpenWithAct1Saved()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        viewModel.TryMoveMapTo("start", "Act 1/Village");
        viewModel.TryMoveMapTo("forest", "Act 1");
        viewModel.SaveAllCommand.Execute(null);
        Assert.False(viewModel.HasUnsavedChanges);
        return viewModel;
    }

    private static bool RenameFolder(MainViewModel viewModel, string folderPath, string newPath)
    {
        var choice = viewModel.StartRenamingFolder(folderPath);
        choice.PathText = newPath;
        return viewModel.TryRenameFolder(folderPath, choice);
    }
}
