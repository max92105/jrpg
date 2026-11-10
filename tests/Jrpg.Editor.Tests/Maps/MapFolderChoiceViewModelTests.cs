using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Tests.Maps;

/// <summary>The folder dialog's choice (decisions 532-534).</summary>
public sealed class MapFolderChoiceViewModelTests
{
    [Fact]
    public void Folders_SavedOpenAndSession_ListedOnceSortedWithTheTopFirst()
    {
        var choice = new MapFolderChoiceViewModel(["Zeta", "Act 1/Village", "act 2", "Zeta"], null);

        Assert.Equal(["(top of the list)", "Act 1", "Act 1/Village", "act 2", "Zeta"], choice.Folders);
    }

    [Fact]
    public void PathText_StartPathNull_IsTheTop()
    {
        var choice = new MapFolderChoiceViewModel([], null);

        Assert.Equal("(top of the list)", choice.PathText);
        Assert.Null(choice.ToPath());
    }

    [Fact]
    public void ForNewFolder_NoParent_StartsEmptyWithTheNameAsked()
    {
        var choice = MapFolderChoiceViewModel.ForNewFolder(["Act 1"], null);

        Assert.Equal("", choice.PathText);
        Assert.Equal("Type the new folder's name.", choice.Problem);
        Assert.Equal(["Act 1"], choice.Folders);
    }

    [Fact]
    public void ForNewFolder_UnderAFolder_OffersItsPathUntilANameIsTyped()
    {
        var choice = MapFolderChoiceViewModel.ForNewFolder([], "Act 1");
        Assert.Equal("Act 1/", choice.PathText);
        Assert.False(choice.CanApply);

        choice.PathText = "Act 1/Village";

        Assert.Equal("Act 1/Village", choice.ToPath());
    }

    [Fact]
    public void ForRenamingFolder_PathUnchanged_NamesTheProblem()
    {
        var choice = MapFolderChoiceViewModel.ForRenamingFolder(["Act 1"], "Act 1");

        Assert.Equal("The folder already has this path.", choice.Problem);
        Assert.False(choice.CanApply);
    }

    [Fact]
    public void ForRenamingFolder_NewPathTyped_GivesIt()
    {
        var choice = MapFolderChoiceViewModel.ForRenamingFolder([], "Act 1");

        choice.PathText = "Chapter 1";

        Assert.Equal("Chapter 1", choice.ToPath());
    }

    [Fact]
    public void ForRenamingFolder_Emptied_AsksForAPath()
    {
        var choice = MapFolderChoiceViewModel.ForRenamingFolder([], "Act 1");

        choice.PathText = "";

        Assert.Equal("Type the folder's new path.", choice.Problem);
    }

    [Fact]
    public void Problem_BadPath_NamesTheRule()
    {
        var choice = new MapFolderChoiceViewModel([], "Act 1/");

        Assert.Equal("A folder path is names joined by '/', each with text, no space at either end and no backslash.", choice.Problem);
        Assert.False(choice.CanApply);
    }

    [Fact]
    public void Problem_Empty_IsNull()
    {
        var choice = new MapFolderChoiceViewModel([], "Act 1") { PathText = "" };

        Assert.Null(choice.Problem);
        Assert.Null(choice.ToPath());
    }

    [Fact]
    public void ToPath_Typed_GivesItTrimmed()
    {
        var choice = new MapFolderChoiceViewModel([], null) { PathText = " Act 1/Village " };

        Assert.Equal("Act 1/Village", choice.ToPath());
    }

    [Fact]
    public void ToPath_Problem_Throws()
    {
        var choice = new MapFolderChoiceViewModel([], "Act 1/");

        Assert.Throws<InvalidOperationException>(() => choice.ToPath());
    }

    [Fact]
    public void PathText_Changed_RaisesProblemAndCanApply()
    {
        var choice = new MapFolderChoiceViewModel([], null);
        var changed = new List<string?>();
        choice.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        choice.PathText = "Act 1/";

        Assert.Contains(nameof(MapFolderChoiceViewModel.Problem), changed);
        Assert.Contains(nameof(MapFolderChoiceViewModel.CanApply), changed);
    }
}
