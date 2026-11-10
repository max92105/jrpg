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
