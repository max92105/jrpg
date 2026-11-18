using System.Collections.ObjectModel;
using Jrpg.Editor.Events;

namespace Jrpg.Editor.Tests.Events;

// Adding after the chosen row, as every Add beside a list of rows does, greying the moves at the list's ends, numbering
// the rows by their place, and keeping the record's own list while the typed one equals it.
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

    [Theory]
    [InlineData("first", false)]
    [InlineData("second", true)]
    [InlineData("third", true)]
    public void CanMoveUp_ChosenRow_IsFalseOnlyForTheFirst(string chosen, bool expected)
    {
        ObservableCollection<string> rows = ["first", "second", "third"];

        Assert.Equal(expected, ListOrder.CanMoveUp(rows, chosen));
    }

    [Theory]
    [InlineData("first", true)]
    [InlineData("second", true)]
    [InlineData("third", false)]
    public void CanMoveDown_ChosenRow_IsFalseOnlyForTheLast(string chosen, bool expected)
    {
        ObservableCollection<string> rows = ["first", "second", "third"];

        Assert.Equal(expected, ListOrder.CanMoveDown(rows, chosen));
    }

    [Fact]
    public void CanMoveUpOrDown_NoneChosen_IsFalse()
    {
        ObservableCollection<string> rows = ["first", "second"];

        Assert.Equal((false, false), (ListOrder.CanMoveUp(rows, null), ListOrder.CanMoveDown(rows, null)));
    }

    [Fact]
    public void Renumber_ThreeRows_GivesEachItsPlaceFromZero()
    {
        string[] rows = ["first", "second", "third"];
        var places = new List<(string, int)>();

        ListOrder.Renumber(rows, (row, place) => places.Add((row, place)));

        Assert.Equal([("first", 0), ("second", 1), ("third", 2)], places);
    }

    [Fact]
    public void KeptIfEqual_SameRowsInOrder_KeepsTheSourceList()
    {
        string[] source = ["first", "second"];

        var kept = ListOrder.KeptIfEqual(["first", "second"], source);

        Assert.Same(source, kept);
    }

    [Fact]
    public void KeptIfEqual_RowsReordered_GivesTheTypedList()
    {
        string[] typed = ["second", "first"];

        var kept = ListOrder.KeptIfEqual(typed, ["first", "second"]);

        Assert.Same(typed, kept);
    }
}
