using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Data.Content;
using Jrpg.Editor.Loot;

namespace Jrpg.Editor.Tests.Loot;

// The Drop tables page's list (ADR 0045): tables in id order, only a new table removable (decision 607), the item box
// by name, "Rolled by" from the draft's enemies (decision 603), and the problem line ending with Data's message.
public sealed class DropTableCatalogViewModelTests
{
    private static readonly ContentCatalog _testWorld =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    private static readonly DropTable _wolfDrops = new("wolf_drops", [new DropEntry("potion", 30), new DropEntry("scrap", 20)]);
    private static readonly DropTable _spare = new("spare", [new DropEntry("potion", 5)]);

    [Fact]
    public void Constructor_Draft_ListsTablesInIdOrder()
    {
        var catalog = new DropTableCatalogViewModel(WithTables(_wolfDrops, _spare));

        Assert.Equal(["spare", "wolf_drops"], catalog.Tables.Select(table => table.Id));
        Assert.Same(catalog.Tables[0], catalog.SelectedTable);
        Assert.Null(catalog.Problem);
    }

    [Fact]
    public void OfferedItems_Catalog_SortsByNameThenId()
    {
        var items = new Dictionary<string, ItemDefinition>(_testWorld.Items)
        {
            ["zz_charm"] = Item("zz_charm", "Aaa Charm"),
            ["aa_charm"] = Item("aa_charm", "Aaa Charm"),
        };

        var charms = new DropTableCatalogViewModel(_testWorld with { Items = items }).OfferedItems
            .Where(item => item.Id.EndsWith("_charm", StringComparison.Ordinal))
            .ToList();

        Assert.Equal(["aa_charm", "zz_charm"], charms.Select(item => item.Id));
        Assert.Equal("Aaa Charm (aa_charm)", charms[0].Label);
    }

    [Fact]
    public void AddTable_Always_AppendsANewTableAndChoosesIt()
    {
        var catalog = new DropTableCatalogViewModel(WithTables(_wolfDrops));

        catalog.AddTableCommand.Execute(null);

        Assert.Same(catalog.Tables[^1], catalog.SelectedTable);
        Assert.True(catalog.SelectedTable!.IsNew);
        Assert.Equal("Drop table #2: The id must not be empty.", catalog.Problem);
    }

    [Fact]
    public void RemoveTable_SavedTable_IsGreyed()
    {
        Assert.False(new DropTableCatalogViewModel(WithTables(_wolfDrops)).RemoveTableCommand.CanExecute(null));
    }

    [Fact]
    public void RemoveTable_AddedTable_RemovesIt()
    {
        var catalog = new DropTableCatalogViewModel(WithTables(_wolfDrops));
        catalog.AddTableCommand.Execute(null);

        catalog.RemoveTableCommand.Execute(null);

        Assert.Equal(["wolf_drops"], catalog.Tables.Select(table => table.Id));
        Assert.Null(catalog.Problem);
    }

    // Given wolf then bat, the line names them by id: bat first.
    [Fact]
    public void RolledBy_TableTwoEnemiesRoll_NamesThemById()
    {
        var catalog = new DropTableCatalogViewModel(WithEnemiesRolling("wolf_drops", "wolf", "bat") with
        {
            DropTables = new Dictionary<string, DropTable> { ["wolf_drops"] = _wolfDrops },
        });

        Assert.Equal("Rolled by: bat, wolf", catalog.RolledBy);
    }

    [Fact]
    public void RolledBy_TableNoEnemyRolls_SaysSo()
    {
        var catalog = new DropTableCatalogViewModel(WithTables(_spare));

        Assert.Equal("Rolled by no enemy", catalog.RolledBy);
    }

    [Fact]
    public void RolledBy_AddedTable_IsEmpty()
    {
        var catalog = new DropTableCatalogViewModel(WithTables(_spare));

        catalog.AddTableCommand.Execute(null);

        Assert.Equal("", catalog.RolledBy);
    }

    [Fact]
    public void RolledBy_OtherTableChosen_IsRaised()
    {
        var catalog = new DropTableCatalogViewModel(WithTables(_wolfDrops, _spare));
        var notified = new List<string?>();
        catalog.PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        catalog.SelectedTable = catalog.Tables[1];

        Assert.Contains(nameof(DropTableCatalogViewModel.RolledBy), notified);
    }

    [Fact]
    public void Problem_TwoTablesSameId_NamesTheDouble()
    {
        var catalog = new DropTableCatalogViewModel(WithTables(_wolfDrops));
        catalog.AddTableCommand.Execute(null);

        catalog.SelectedTable!.Id = "wolf_drops";

        Assert.Equal("Drop table wolf_drops is listed twice.", catalog.Problem);
    }

    [Fact]
    public void Problem_ChanceZero_ShowsDatasMessage()
    {
        var catalog = new DropTableCatalogViewModel(WithTables(_wolfDrops));

        catalog.Tables[0].Entries[1].ChanceText = "0";

        var expected = "drop-tables.json: drop table 'wolf_drops': entries[1]: chance must be within 1..100 (was 0).";
        Assert.Equal(expected, catalog.Problem);
    }

    [Fact]
    public void Problem_EntryChanceTyped_IsRaised()
    {
        var catalog = new DropTableCatalogViewModel(WithTables(_wolfDrops));
        var notified = new List<string?>();
        catalog.PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        catalog.Tables[0].Entries[0].ChanceText = "35";

        Assert.Contains(nameof(DropTableCatalogViewModel.Problem), notified);
    }

    // Typing an id raises the table's Id, Label and Problem notices: only the Id is relayed.
    [Fact]
    public void Problem_NewTableIdTyped_IsRaisedOnce()
    {
        var catalog = new DropTableCatalogViewModel(WithTables(_wolfDrops));
        catalog.AddTableCommand.Execute(null);
        var raised = 0;
        catalog.PropertyChanged += (_, e) => raised += e.PropertyName == nameof(DropTableCatalogViewModel.Problem) ? 1 : 0;

        catalog.SelectedTable!.Id = "slime_drops";

        Assert.Equal(1, raised);
    }

    [Fact]
    public void Problem_RemovedTableTyped_IsNotRaised()
    {
        var catalog = new DropTableCatalogViewModel(WithTables(_wolfDrops));
        catalog.AddTableCommand.Execute(null);
        var removed = catalog.SelectedTable!;
        catalog.RemoveTableCommand.Execute(null);
        var notified = new List<string?>();
        catalog.PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        removed.Id = "gone";

        Assert.Empty(notified);
    }

    [Fact]
    public void ToTables_ProblemShown_Throws()
    {
        var catalog = new DropTableCatalogViewModel(WithTables(_wolfDrops));

        catalog.Tables[0].Entries[0].ChanceText = "";

        Assert.Throws<InvalidOperationException>(catalog.ToTables);
    }

    [Fact]
    public void ToTables_NewTableWithOneEntry_ListsIt()
    {
        var catalog = new DropTableCatalogViewModel(WithTables(_wolfDrops));
        catalog.AddTableCommand.Execute(null);
        var table = catalog.SelectedTable!;
        table.Id = "slime_drops";
        table.ItemToAdd = catalog.OfferedItems.Single(item => item.Id == "potion");

        table.AddEntryCommand.Execute(null);

        Assert.Null(catalog.Problem);
        Assert.Equal(new DropTable("slime_drops", [new DropEntry("potion", 10)]), catalog.ToTables()[^1]);
    }

    [Fact]
    public void Choose_KnownId_ChoosesIt()
    {
        var catalog = new DropTableCatalogViewModel(WithTables(_spare, _wolfDrops));

        catalog.Choose("wolf_drops");

        Assert.Equal("wolf_drops", catalog.SelectedTable!.Id);
    }

    // An enemy with no table passes "": the page opens on its first table, as from the menu.
    [Fact]
    public void Choose_EmptyId_KeepsTheFirst()
    {
        var catalog = new DropTableCatalogViewModel(WithTables(_spare, _wolfDrops));

        catalog.Choose("");

        Assert.Equal("spare", catalog.SelectedTable!.Id);
    }

    // No enemy: "Rolled by" sees only what a test gives it, never the repository's enemies.
    private static ContentCatalog WithTables(params DropTable[] tables) =>
        _testWorld with
        {
            DropTables = tables.ToDictionary(table => table.Id),
            Enemies = new Dictionary<string, EnemyDefinition>(),
        };

    // The test world's goblin as each named enemy, every one rolling the table.
    private static ContentCatalog WithEnemiesRolling(string tableId, params string[] enemyIds) =>
        _testWorld with
        {
            Enemies = enemyIds.ToDictionary(id => id, id => _testWorld.Enemies["gnoll"] with { Id = id, DropTableId = tableId }),
        };

    private static ItemDefinition Item(string id, string name) => new()
    {
        Id = id,
        Name = name,
        Description = name,
        IconFile = "Placeholder.png",
        MaxStack = 9,
        IsUsableFromMenu = false,
    };
}
