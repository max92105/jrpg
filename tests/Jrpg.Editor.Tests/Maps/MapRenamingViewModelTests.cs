using Jrpg.Core.Maps;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Tests.Maps;

/// <summary>The Rename map dialog's id and its problems (decision 530).</summary>
public sealed class MapRenamingViewModelTests
{
    private const string RuleProblem = "An id is lower-case letters, digits and underscores, such as old_mill.";

    private static readonly MapDefinition _mill = Map("map_3", "Old Mill");

    [Fact]
    public void NewId_StartsFromTheDisplayName() => Assert.Equal("old_mill", new MapRenamingViewModel(_mill, ["map_3", "forest"]).NewId);

    [Fact]
    public void NewId_DisplayNameMatchesAnotherMap_ProposesTheNextFree() =>
        Assert.Equal("old_mill_2", new MapRenamingViewModel(_mill, ["map_3", "old_mill"]).NewId);

    [Fact]
    public void Problem_SameAsOld_SaysSo()
    {
        var renaming = new MapRenamingViewModel(_mill, ["map_3"]) { NewId = "map_3" };

        Assert.Equal("The id is already this map's.", renaming.Problem);
        Assert.False(renaming.CanApply);
    }

    // A hand-made id with a capital: the prefill gives its lower-case form, which is the same file on Windows.
    [Fact]
    public void Problem_OwnIdWithOtherCase_SaysSo()
    {
        var renaming = new MapRenamingViewModel(Map("Old_Mill", "Old Mill"), ["Old_Mill"]);

        Assert.Equal("old_mill", renaming.NewId);
        Assert.Equal("The id is already this map's.", renaming.Problem);
    }

    [Fact]
    public void Problem_Taken_NamesTheMap()
    {
        var renaming = new MapRenamingViewModel(_mill, ["map_3", "forest"]) { NewId = "forest" };

        Assert.Equal("Map 'forest' already has this id.", renaming.Problem);
    }

    [Fact]
    public void Problem_TakenWithOtherCase_NamesTheMap()
    {
        var renaming = new MapRenamingViewModel(_mill, ["map_3", "Forest"]) { NewId = "forest" };

        Assert.Equal("Map 'Forest' already has this id.", renaming.Problem);
    }

    [Fact]
    public void Problem_Uppercase_NamesTheRule()
    {
        var renaming = new MapRenamingViewModel(_mill, ["map_3"]) { NewId = "OldMill" };

        Assert.Equal(RuleProblem, renaming.Problem);
    }

    [Fact]
    public void Problem_Empty_NamesTheRule()
    {
        var renaming = new MapRenamingViewModel(_mill, ["map_3"]) { NewId = " " };

        Assert.Equal(RuleProblem, renaming.Problem);
    }

    [Fact]
    public void Problem_WellFormedAndFree_IsNull()
    {
        var renaming = new MapRenamingViewModel(_mill, ["map_3", "forest"]) { NewId = " mill_2 " };

        Assert.Null(renaming.Problem);
        Assert.True(renaming.CanApply);
    }

    [Fact]
    public void Problem_PropertyChangedOnIdTyped_IsRaised()
    {
        var renaming = new MapRenamingViewModel(_mill, ["map_3"]);
        var raised = new List<string?>();
        renaming.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        renaming.NewId = "Bad Id";

        Assert.Contains(nameof(MapRenamingViewModel.Problem), raised);
        Assert.Contains(nameof(MapRenamingViewModel.CanApply), raised);
    }

    private static MapDefinition Map(string id, string displayName) => new(
        new MapLayout { Id = id, Width = 1, Height = 1, Layers = [], Objects = [] },
        new MapSettings { Id = id, DisplayName = displayName });
}
