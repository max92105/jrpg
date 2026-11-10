using Jrpg.Editor.Maps;
using Jrpg.Editor.Shell;

namespace Jrpg.Editor.Tests.Shell;

/// <summary>
/// The map list's tree in the navigation (decisions 532, 534): the session's folders and the open ones kept.
/// </summary>
public sealed class MapNavigationTreeTests
{
    private static readonly MapEntry _forest = new("forest", "Forest (forest)");
    private static readonly MapEntry _mill = new("mill", "Old Mill (mill)", "Act 1");

    private readonly List<string> _opened = [];
    private readonly MapNavigation _navigation;

    public MapNavigationTreeTests()
    {
        _navigation = new MapNavigation(mapId =>
        {
            _opened.Add(mapId);
            return true;
        });
        _navigation.ShowEntries([_forest, _mill]);
    }

    // The folder's map comes first in the tree though the forest is first in the list.
    [Fact]
    public void TryOpenFirstShown_MapInAFolderFirstInTheTree_OpensIt()
    {
        Assert.True(_navigation.TryOpenFirstShown());

        Assert.Equal(["mill"], _opened);
    }

    [Fact]
    public void ShowEntries_FolderWasExpanded_StaysExpanded()
    {
        _navigation.NoteExpanded("Act 1", isExpanded: true);

        _navigation.ShowEntries([_forest, _mill with { Text = "Old Mill (mill) *" }]);

        Assert.True(_navigation.ShownTree[0].IsExpanded);
    }

    [Fact]
    public void ShowEntries_FolderWasClosed_StaysClosed()
    {
        _navigation.NoteExpanded("Act 1", isExpanded: true);
        _navigation.NoteExpanded("Act 1", isExpanded: false);

        _navigation.ShowEntries([_forest, _mill]);

        Assert.False(_navigation.ShownTree[0].IsExpanded);
    }

    // A filter opens every folder it shows and the tree reports each as opened.
    [Fact]
    public void NoteExpanded_WhileFiltering_LeavesTheFolderClosedOnceTheFilterClears()
    {
        _navigation.Filter = "mill";

        _navigation.NoteExpanded("Act 1", isExpanded: true);
        _navigation.Filter = "";

        Assert.False(_navigation.ShownTree[0].IsExpanded);
    }

    [Fact]
    public void AddSessionFolder_ShowsItInTheTree()
    {
        _navigation.AddSessionFolder("Act 2");

        Assert.Equal(["Act 1", "Act 2", "Forest (forest)"], _navigation.ShownTree.Select(row => row.Text));
        Assert.True(_navigation.ShownTree[1].IsExpanded);
    }

    [Fact]
    public void Filter_Typed_NarrowsTheTree()
    {
        _navigation.Filter = "forest";

        Assert.Equal(["Forest (forest)"], _navigation.ShownTree.Select(row => row.Text));
    }
}
