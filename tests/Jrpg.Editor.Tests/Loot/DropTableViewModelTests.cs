using Jrpg.Core.Loot;
using Jrpg.Editor.Loot;

namespace Jrpg.Editor.Tests.Loot;

// One drop table as typed (ADR 0045): its id fixed once saved, its rows in the roll's order and numbered from 0 as Data
// numbers entries (decision 598), the problem naming the row's position and item.
public sealed class DropTableViewModelTests
{
    [Fact]
    public void From_Table_ListsEntriesInOrderWithTheirNames()
    {
        var table = DropTableViewModel.From(DropTableFixtures.Goblin, DropTableFixtures.Items);

        Assert.Equal(["potion", "bronze_sword", "scrap", "essence"], table.Entries.Select(row => row.ItemId));
        Assert.Equal("Bronze Sword (bronze_sword)", table.Entries[1].ItemLabel);
        Assert.Same(table.Entries[0], table.SelectedEntry);
        Assert.Equal("goblin", table.Label);
        Assert.False(table.IsNew);
    }

    [Fact]
    public void From_Entries_NumbersRowsFromZero()
    {
        var table = DropTableViewModel.From(DropTableFixtures.Goblin, DropTableFixtures.Items);

        Assert.Equal([0, 1, 2, 3], table.Entries.Select(row => row.Index));
    }

    // An entry naming an item the content lacks still shows, by its id; Data's message names it.
    [Fact]
    public void From_UnknownItem_ShowsItsId()
    {
        var table = DropTableViewModel.From(new DropTable("odd", [new DropEntry("relic", 5)]), DropTableFixtures.Items);

        Assert.Equal("relic (relic)", table.Entries[0].ItemLabel);
    }

    [Fact]
    public void Problem_NewTableWithBadId_NamesTheId()
    {
        var table = DropTableViewModel.New();

        table.Id = "Slime Drops";

        Assert.Equal("The id may only use lowercase letters, digits and _ (like tree_4).", table.Problem);
        Assert.Equal("Slime Drops", table.Label);
    }

    [Fact]
    public void Problem_NewTableWithoutId_ShowsAPlaceholderLabel()
    {
        var table = DropTableViewModel.New();

        Assert.Equal(("(new table)", "The id must not be empty."), (table.Label, table.Problem));
    }

    // A saved id is fixed, whatever it looks like: the hand-written content's ids were never checked by this rule.
    [Fact]
    public void Problem_SavedTable_IgnoresTheIdRule()
    {
        var table = DropTableViewModel.From(DropTableFixtures.Goblin with { Id = "Goblin-Drops" }, DropTableFixtures.Items);

        Assert.Null(table.Problem);
    }

    [Fact]
    public void Problem_ThirdRowChanceEmpty_NamesEntries2AndItsItem()
    {
        var table = DropTableViewModel.From(DropTableFixtures.Goblin, DropTableFixtures.Items);

        table.Entries[2].ChanceText = "";

        Assert.Equal("entries[2] (scrap): Chance must be a whole number (was '').", table.Problem);
        Assert.Throws<InvalidOperationException>(table.ToTable);
    }

    [Fact]
    public void ToTable_EditedChance_KeepsEntryOrder()
    {
        var table = DropTableViewModel.From(DropTableFixtures.Goblin, DropTableFixtures.Items);

        table.Entries[1].ChanceText = "8";

        var built = table.ToTable();
        Assert.Equal(["potion", "bronze_sword", "scrap", "essence"], built.Entries.Select(entry => entry.ItemId));
        Assert.Equal(8, built.Entries[1].Chance);
        Assert.Equal(DropTableFixtures.Goblin.Entries[2], built.Entries[2]);
    }

    [Fact]
    public void ToTable_Unchanged_EqualsTheTable()
    {
        var table = DropTableViewModel.From(DropTableFixtures.Goblin, DropTableFixtures.Items);

        Assert.Equal(DropTableFixtures.Goblin, table.ToTable());
    }
}
