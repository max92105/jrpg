using Jrpg.Editor.Loot;

namespace Jrpg.Editor.Tests.Loot;

// A table's entries added from the item box, removed and moved, renumbered after each (decision 598), and every typed
// or list change told once through EntriesChanged, never a row's own notices.
public sealed class DropTableEntryEditingTests
{
    private static readonly DropItemChoice _bomb = new("bomb", "Bomb");

    [Fact]
    public void AddEntry_SecondRowChosen_InsertsAfterItWithDefaults()
    {
        var table = Goblin();
        table.SelectedEntry = table.Entries[1];
        table.ItemToAdd = _bomb;

        table.AddEntryCommand.Execute(null);

        var added = table.Entries[2];
        Assert.Equal(("bomb", "Bomb (bomb)", 2), (added.ItemId, added.ItemLabel, added.Index));
        Assert.Equal(("10", "1", "1"), (added.ChanceText, added.MinQuantityText, added.MaxQuantityText));
        Assert.Same(added, table.SelectedEntry);
        Assert.Equal(4, table.Entries[4].Index);
    }

    [Fact]
    public void AddEntry_NothingChosen_AddsLast()
    {
        var table = Goblin();
        table.SelectedEntry = null;
        table.ItemToAdd = _bomb;

        table.AddEntryCommand.Execute(null);

        Assert.Equal("bomb", table.Entries[^1].ItemId);
    }

    // Data lets a table list an item twice, so the editor adds no rule of its own.
    [Fact]
    public void AddEntry_ItemAlreadyListed_AddsASecondRow()
    {
        var table = Goblin();
        table.ItemToAdd = new DropItemChoice("potion", "Potion");

        table.AddEntryCommand.Execute(null);

        Assert.Equal(2, table.Entries.Count(row => row.ItemId == "potion"));
    }

    [Fact]
    public void AddEntry_NoItemPicked_IsGreyed()
    {
        Assert.False(Goblin().AddEntryCommand.CanExecute(null));
    }

    [Fact]
    public void RemoveEntry_Chosen_RemovesItAndRenumbers()
    {
        var table = Goblin();
        table.SelectedEntry = table.Entries[1];

        table.RemoveEntryCommand.Execute(null);

        Assert.Equal(["potion", "scrap", "essence"], table.Entries.Select(row => row.ItemId));
        Assert.Equal([0, 1, 2], table.Entries.Select(row => row.Index));
    }

    [Fact]
    public void RemoveEntry_NothingChosen_IsGreyed()
    {
        var table = Goblin();

        table.SelectedEntry = null;

        Assert.False(table.RemoveEntryCommand.CanExecute(null));
        Assert.False(table.MoveEntryUpCommand.CanExecute(null));
        Assert.False(table.MoveEntryDownCommand.CanExecute(null));
    }

    [Fact]
    public void MoveEntryUp_Chosen_SwapsWithTheOneAboveAndRenumbers()
    {
        var table = Goblin();
        var essence = table.Entries[3];
        var scrap = table.Entries[2];
        table.SelectedEntry = essence;

        table.MoveEntryUpCommand.Execute(null);

        Assert.Equal((2, 3), (essence.Index, scrap.Index));
        Assert.Same(essence, table.Entries[2]);
    }

    [Fact]
    public void MoveEntryDown_Chosen_SwapsWithTheOneBelowAndRenumbers()
    {
        var table = Goblin();
        var potion = table.Entries[0];
        var sword = table.Entries[1];

        table.MoveEntryDownCommand.Execute(null);

        Assert.Equal((1, 0), (potion.Index, sword.Index));
        Assert.Same(potion, table.Entries[1]);
    }

    // The moved row stays chosen, so a second Down moves it again.
    [Fact]
    public void MoveEntryDown_Chosen_KeepsItChosen()
    {
        var table = Goblin();
        var potion = table.Entries[0];

        table.MoveEntryDownCommand.Execute(null);
        table.MoveEntryDownCommand.Execute(null);

        Assert.Same(potion, table.SelectedEntry);
        Assert.Equal(2, potion.Index);
    }

    // Typing raises the row's ChanceText and Problem notices: a relay reacting to every notice would count two.
    [Fact]
    public void EntriesChanged_RowChanceTyped_IsRaisedOnce()
    {
        var table = Goblin();
        var raised = 0;
        table.EntriesChanged += (_, _) => raised++;

        table.Entries[0].ChanceText = "35";

        Assert.Equal(1, raised);
    }

    [Fact]
    public void EntriesChanged_RowMaxTyped_IsRaisedOnce()
    {
        var table = Goblin();
        var raised = 0;
        table.EntriesChanged += (_, _) => raised++;

        table.Entries[2].MaxQuantityText = "3";

        Assert.Equal(1, raised);
    }

    [Fact]
    public void EntriesChanged_EntryAdded_IsRaised()
    {
        var table = Goblin();
        table.ItemToAdd = _bomb;
        var raised = 0;
        table.EntriesChanged += (_, _) => raised++;

        table.AddEntryCommand.Execute(null);

        Assert.Equal(1, raised);
    }

    // The list's problem line names entries by position, so a move must tell the list too.
    [Fact]
    public void EntriesChanged_EntryMoved_IsRaised()
    {
        var table = Goblin();
        var raised = 0;
        table.EntriesChanged += (_, _) => raised++;

        table.MoveEntryDownCommand.Execute(null);

        Assert.Equal(1, raised);
    }

    [Fact]
    public void EntriesChanged_AddedRowTyped_IsRaised()
    {
        var table = Goblin();
        table.ItemToAdd = _bomb;
        table.AddEntryCommand.Execute(null);
        var raised = 0;
        table.EntriesChanged += (_, _) => raised++;

        table.SelectedEntry!.ChanceText = "40";

        Assert.Equal(1, raised);
    }

    [Fact]
    public void EntriesChanged_RemovedRowTyped_IsNotRaised()
    {
        var table = Goblin();
        var removed = table.Entries[0];
        table.RemoveEntryCommand.Execute(null);
        var raised = 0;
        table.EntriesChanged += (_, _) => raised++;

        removed.ChanceText = "50";

        Assert.Equal(0, raised);
    }

    private static DropTableViewModel Goblin() => DropTableViewModel.From(DropTableFixtures.Goblin, DropTableFixtures.Items);
}
