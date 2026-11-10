using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Tests.Maps;

/// <summary>
/// The map list arranged into folders (decisions 532, 534, 535); entries given in the opposite of the expected order.
/// </summary>
public sealed class MapTreeTests
{
    private static readonly MapEntry _town = new("town", "Town (town)");
    private static readonly MapEntry _forest = new("forest", "Forest (forest)", "Act 1");
    private static readonly MapEntry _mill = new("mill", "Old Mill (mill)", "Act 1/Village");
    private static readonly MapEntry _inn = new("inn", "Inn (inn)", "Act 1/Village");
    private static readonly HashSet<string> _noFolders = [];

    [Fact]
    public void Of_MapsWithoutFolders_AtTheTop()
    {
        var rows = MapTree.Of([_town, new MapEntry("abbey", "Abbey (abbey)")], [], "", _noFolders);

        Assert.Equal(["Town (town)", "Abbey (abbey)"], rows.Select(row => row.Text));
        Assert.All(rows, row => Assert.False(row.IsFolder));
    }

    [Fact]
    public void Of_NestedFolder_HoldsItsMaps()
    {
        var rows = MapTree.Of([_inn, _mill, _forest], [], "", _noFolders);

        var act = Assert.Single(rows);
        Assert.Equal(("Act 1", "Act 1"), (act.Text, act.FolderPath));
        Assert.Equal(["Village", "Forest (forest)"], act.Rows.Select(row => row.Text));
        Assert.Equal(["Inn (inn)", "Old Mill (mill)"], act.Rows[0].Rows.Select(row => row.Text));
        Assert.Equal("Act 1/Village", act.Rows[0].FolderPath);
    }

    [Fact]
    public void Of_FoldersBeforeMaps_EachSortedByName()
    {
        var rows = MapTree.Of([_town, new MapEntry("cave", "Cave (cave)", "Zeta"), _forest], [], "", _noFolders);

        Assert.Equal(["Act 1", "Zeta", "Town (town)"], rows.Select(row => row.Text));
    }

    [Fact]
    public void Of_FolderNamesDifferingByCase_SortedIgnoringCase()
    {
        var rows = MapTree.Of([new MapEntry("a", "A (a)", "beta"), new MapEntry("b", "B (b)", "Alpha")], [], "", _noFolders);

        Assert.Equal(["Alpha", "beta"], rows.Select(row => row.Text));
    }

    // "Act 10/Keep" starts with the letters of "Act 1" but isn't under it.
    [Fact]
    public void Of_SiblingFolderSharingAPrefix_NestsOnlyUnderItsOwn()
    {
        var rows = MapTree.Of([new MapEntry("keep", "Keep (keep)", "Act 10/Keep"), _forest], [], "", _noFolders);

        Assert.Equal(["Act 1", "Act 10"], rows.Select(row => row.Text));
        Assert.Equal(["Forest (forest)"], rows[0].Rows.Select(row => row.Text));
        Assert.Equal(["Keep"], rows[1].Rows.Select(row => row.Text));
    }

    [Fact]
    public void Of_SessionFolderUnderAMapsFolder_ShowsTheFolderOnce()
    {
        var rows = MapTree.Of([_forest], ["Act 1/Keep"], "", _noFolders);

        var act = Assert.Single(rows);
        Assert.Equal(["Keep", "Forest (forest)"], act.Rows.Select(row => row.Text));
    }

    [Fact]
    public void Of_SessionFolderEmpty_Shown()
    {
        var rows = MapTree.Of([_town], ["Act 2/Keep"], "", _noFolders);

        Assert.Equal(["Act 2", "Town (town)"], rows.Select(row => row.Text));
        Assert.Equal(["Keep"], rows[0].Rows.Select(row => row.Text));
    }

    [Fact]
    public void Of_SessionFolderEmpty_HiddenWhileFiltering()
    {
        var rows = MapTree.Of([_town], ["Act 2"], "town", _noFolders);

        Assert.Equal(["Town (town)"], rows.Select(row => row.Text));
    }

    [Fact]
    public void Of_Filter_KeepsOnlyMatchingMapsAndTheirFolders()
    {
        var rows = MapTree.Of([_town, _inn, _mill, _forest], [], "mill", _noFolders);

        var act = Assert.Single(rows);
        var village = Assert.Single(act.Rows);
        Assert.Equal(["Old Mill (mill)"], village.Rows.Select(row => row.Text));
    }

    [Fact]
    public void Of_Filter_OpensEveryFolderShown()
    {
        var rows = MapTree.Of([_mill], [], "mill", _noFolders);

        Assert.True(rows[0].IsExpanded);
        Assert.True(rows[0].Rows[0].IsExpanded);
    }

    [Fact]
    public void Of_NoFilter_OpensOnlyTheExpandedFolders()
    {
        var rows = MapTree.Of([_mill], [], "", new HashSet<string> { "Act 1" });

        Assert.True(rows[0].IsExpanded);
        Assert.False(rows[0].Rows[0].IsExpanded);
    }

    [Fact]
    public void PathTo_NestedMap_GivesEveryFolderAboveThenTheMap()
    {
        var rows = MapTree.Of([_town, _mill, _forest], [], "", _noFolders);

        var path = MapTree.PathTo(rows, "mill");

        Assert.Equal(["Act 1", "Act 1/Village", null], path.Select(node => node.FolderPath));
        Assert.Equal("mill", path[^1].Map?.Id);
    }

    [Fact]
    public void PathTo_TopLevelMap_IsTheMapAlone()
    {
        var rows = MapTree.Of([_town, _mill], [], "", _noFolders);

        Assert.Equal(["Town (town)"], MapTree.PathTo(rows, "town").Select(node => node.Text));
    }

    [Fact]
    public void PathTo_UnknownMap_IsEmpty() => Assert.Empty(MapTree.PathTo(MapTree.Of([_town], [], "", _noFolders), "cave"));

    [Fact]
    public void MapsInOrder_Nested_DepthFirst()
    {
        var rows = MapTree.Of([_town, _forest, _mill], [], "", _noFolders);

        Assert.Equal(["mill", "forest", "town"], MapTree.MapsInOrder(rows).Select(map => map.Id));
    }
}
