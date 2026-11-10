using Jrpg.Editor.Maps;
using Jrpg.Editor.Shell;

namespace Jrpg.Editor.Tests.Shell;

public sealed class MapNavigationTests
{
    private static readonly MapEntry _forest = new("forest", "Forest (forest)");
    private static readonly MapEntry _inn = new("interior_1", "Inn (interior_1)");
    private static readonly MapEntry _mill = new("old_mill", "Old Mill (old_mill)");

    private readonly List<string> _opened = [];
    private readonly MapNavigation _navigation;

    // Opening a map shows it, as the editor does: the navigation hears of it through NoteShown.
    public MapNavigationTests()
    {
        _navigation = new MapNavigation(mapId =>
        {
            _opened.Add(mapId);
            _navigation!.NoteShown(mapId);
            return true;
        });
        _navigation.ShowEntries([_forest, _inn, _mill]);
    }

    [Fact]
    public void ShownTree_EmptyFilter_ShowsEveryRowInOrder()
    {
        Assert.Equal([_forest, _inn, _mill], MapTree.MapsInOrder(_navigation.ShownTree));
    }

    [Fact]
    public void ShownTree_FilterInTheName_ShowsOnlyMatchingRows()
    {
        _navigation.Filter = "mill";

        Assert.Equal([_mill], MapTree.MapsInOrder(_navigation.ShownTree));
    }

    [Fact]
    public void ShownTree_FilterInTheIdOnly_ShowsTheRow()
    {
        _navigation.Filter = "interior";

        Assert.Equal([_inn], MapTree.MapsInOrder(_navigation.ShownTree));
    }

    [Fact]
    public void ShownTree_FilterInAnotherCase_ShowsTheRow()
    {
        _navigation.Filter = "FOREST";

        Assert.Equal([_forest], MapTree.MapsInOrder(_navigation.ShownTree));
    }

    [Fact]
    public void ShownTree_RowsChangedWhileFiltered_KeepsTheFilter()
    {
        _navigation.Filter = "mill";

        _navigation.ShowEntries([_forest, _mill with { Text = "Old Mill (old_mill) *" }]);

        Assert.Equal(["old_mill"], MapTree.MapsInOrder(_navigation.ShownTree).Select(entry => entry.Id));
    }

    [Fact]
    public void Filter_Changed_RaisesShownTree()
    {
        var changed = new List<string?>();
        _navigation.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        _navigation.Filter = "inn";

        Assert.Contains(nameof(MapNavigation.ShownTree), changed);
    }

    [Fact]
    public void TryOpenFirstShown_TwoMatches_OpensTheFirst()
    {
        _navigation.Filter = "e";

        Assert.True(_navigation.TryOpenFirstShown());

        Assert.Equal(["forest"], _opened);
    }

    [Fact]
    public void TryOpenFirstShown_NoMatch_OpensNothingAndReturnsFalse()
    {
        _navigation.Filter = "dungeon";

        Assert.False(_navigation.TryOpenFirstShown());

        Assert.Empty(_opened);
    }

    [Fact]
    public void GoBackCommand_NoMapShownBefore_CannotExecute()
    {
        _navigation.NoteShown("forest");

        Assert.False(_navigation.GoBackCommand.CanExecute(null));
    }

    [Fact]
    public void GoBack_AfterShowingAnother_ShowsThePrevious()
    {
        _navigation.NoteShown("forest");
        _navigation.NoteShown("interior_1");

        _navigation.GoBackCommand.Execute(null);

        Assert.Equal(["forest"], _opened);
    }

    [Fact]
    public void GoBack_Twice_ReturnsToTheSecondMap()
    {
        _navigation.NoteShown("forest");
        _navigation.NoteShown("interior_1");

        _navigation.GoBackCommand.Execute(null);
        _navigation.GoBackCommand.Execute(null);

        Assert.Equal(["forest", "interior_1"], _opened);
    }

    [Fact]
    public void NoteShown_SameMapAgain_KeepsThePrevious()
    {
        _navigation.NoteShown("forest");
        _navigation.NoteShown("interior_1");

        _navigation.NoteShown("interior_1");
        _navigation.GoBackCommand.Execute(null);

        Assert.Equal(["forest"], _opened);
    }

    [Fact]
    public void RenameMap_PreviousMapRenamed_GoBackOpensTheNewId()
    {
        _navigation.NoteShown("forest");
        _navigation.NoteShown("interior_1");

        _navigation.RenameMap("forest", "woods");
        _navigation.ShowEntries([new MapEntry("woods", "Forest (woods)"), _inn, _mill]);
        _navigation.GoBackCommand.Execute(null);

        Assert.Equal(["woods"], _opened);
    }

    [Fact]
    public void RenameMap_ShownMapRenamed_KeepsThePreviousForGoBack()
    {
        _navigation.NoteShown("forest");
        _navigation.NoteShown("interior_1");

        _navigation.RenameMap("interior_1", "shop");
        _navigation.NoteShown("shop");
        _navigation.GoBackCommand.Execute(null);

        Assert.Equal(["forest"], _opened);
    }

    [Fact]
    public void GoBackCommand_PreviousNoLongerListed_CannotExecute()
    {
        _navigation.NoteShown("old_mill");
        _navigation.NoteShown("forest");
        var raised = 0;
        _navigation.GoBackCommand.CanExecuteChanged += (_, _) => raised++;

        _navigation.ShowEntries([_forest, _inn]);

        Assert.False(_navigation.GoBackCommand.CanExecute(null));
        Assert.True(raised > 0);
    }
}
