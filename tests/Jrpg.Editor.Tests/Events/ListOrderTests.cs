using System.Collections.ObjectModel;
using Jrpg.Editor.Events;

namespace Jrpg.Editor.Tests.Events;

// Adding after the chosen row, as every Add beside a list of rows does.
public sealed class ListOrderTests
{
    [Fact]
    public void InsertAfter_NoneChosen_AppendsIt()
    {
        ObservableCollection<string> rows = ["first", "second"];

        ListOrder.InsertAfter(rows, "added", null);

        Assert.Equal(["first", "second", "added"], rows);
    }

    [Fact]
    public void InsertAfter_MiddleChosen_PutsItRightAfter()
    {
        ObservableCollection<string> rows = ["first", "second", "third"];

        ListOrder.InsertAfter(rows, "added", "second");

        Assert.Equal(["first", "second", "added", "third"], rows);
    }
}
