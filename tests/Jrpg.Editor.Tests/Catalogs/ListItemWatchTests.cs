using System.Collections.ObjectModel;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Catalogs;

// A list view model watches exactly the rows its list holds; each change of the list reaches it once the rows the
// change took in or out are watched or dropped.
public sealed class ListItemWatchTests
{
    private readonly List<string> _watched = [];

    private int _listChanges;

    [Fact]
    public void WatchRowsOf_RowsHeld_WatchesEach()
    {
        ObservableCollection<string> rows = ["first", "second"];

        Watch(rows);

        Assert.Equal(["first", "second"], _watched);
    }

    [Fact]
    public void WatchRowsOf_RowAdded_WatchesItAndRunsTheListChange()
    {
        ObservableCollection<string> rows = ["first"];
        Watch(rows);

        rows.Add("added");

        Assert.Equal(["first", "added"], _watched);
        Assert.Equal(1, _listChanges);
    }

    [Fact]
    public void WatchRowsOf_RowRemoved_StopsWatchingIt()
    {
        ObservableCollection<string> rows = ["first", "second"];
        Watch(rows);

        rows.Remove("first");

        Assert.Equal(["second"], _watched);
    }

    // A move lists its row as both removed and added: it stays watched once.
    [Fact]
    public void WatchRowsOf_RowMoved_KeepsItWatchedOnceAndRunsTheListChange()
    {
        ObservableCollection<string> rows = ["first", "second"];
        Watch(rows);

        rows.Move(0, 1);

        Assert.Equal(["second", "first"], _watched);
        Assert.Equal(1, _listChanges);
    }

    private void Watch(ObservableCollection<string> rows) =>
        ListItemWatch.WatchRowsOf(rows, _watched.Add, row => _watched.Remove(row), () => _listChanges++);
}
