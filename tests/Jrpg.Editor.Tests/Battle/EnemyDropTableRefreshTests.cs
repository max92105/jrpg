using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Loot;
using Jrpg.Data.Content;
using Jrpg.Editor.Battle;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Battle;

// The Enemies window after a Drop tables page opened from it applied (decision 602): new tables are offered in place,
// no enemy's choice changes, and the problem line checks against the new tables.
public sealed class EnemyDropTableRefreshTests
{
    private static readonly ContentCatalog _testWorld =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    private static readonly DropTable _goblin = new("goblin", [new DropEntry("potion", 30)]);

    [Fact]
    public void TakeDropTables_TablesAddedOutOfOrder_InsertsThemInIdOrder()
    {
        var catalog = new EnemyCatalogViewModel(WithGoblinRolling(_goblin), _ => []);

        catalog.TakeDropTables(WithGoblinRolling(_goblin, Table("zeta"), Table("aardvark")));

        Assert.Equal(["", "aardvark", "goblin", "zeta"], catalog.DropTableChoices);
    }

    [Fact]
    public void TakeDropTables_TableAdded_KeepsTheSameList()
    {
        var catalog = new EnemyCatalogViewModel(WithGoblinRolling(_goblin), _ => []);
        var choices = catalog.DropTableChoices;

        catalog.TakeDropTables(WithGoblinRolling(_goblin, Table("slime")));

        Assert.Same(choices, catalog.DropTableChoices);
    }

    [Fact]
    public void TakeDropTables_TableAdded_KeepsEachEnemysTable()
    {
        var catalog = new EnemyCatalogViewModel(WithGoblinRolling(_goblin), _ => []);

        catalog.TakeDropTables(WithGoblinRolling(_goblin, Table("aardvark")));

        Assert.Equal("goblin", catalog.Enemies.Single(enemy => enemy.Id == "goblin").DropTableId);
    }

    [Fact]
    public void TakeDropTables_EnemyNamingTheNewTable_ClearsTheProblem()
    {
        var catalog = new EnemyCatalogViewModel(WithGoblinRolling(_goblin), _ => []);
        catalog.Enemies.Single(enemy => enemy.Id == "goblin").DropTableId = "slime";
        Assert.NotNull(catalog.Problem);

        catalog.TakeDropTables(WithGoblinRolling(_goblin, Table("slime")));

        Assert.Null(catalog.Problem);
    }

    [Fact]
    public void TakeDropTables_Always_RaisesProblemAndCanApply()
    {
        var catalog = new EnemyCatalogViewModel(WithGoblinRolling(_goblin), _ => []);
        var notified = new List<string?>();
        catalog.PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        catalog.TakeDropTables(WithGoblinRolling(_goblin, Table("slime")));

        Assert.Contains(nameof(EnemyCatalogViewModel.Problem), notified);
        Assert.Contains(nameof(EnemyCatalogViewModel.CanApply), notified);
    }

    [Fact]
    public void RefreshDropTables_AfterSetDropTables_OffersTheNewTable()
    {
        var draft = new CatalogDraft(WithGoblinRolling(_goblin));
        var editing = new EnemyEditingViewModel(draft, _ => []);
        var edited = editing.Start();
        draft.SetDropTables([_goblin, Table("slime")]);

        editing.RefreshDropTables(edited);

        Assert.Contains("slime", edited.DropTableChoices);
    }

    // Only the test world's goblin, rolling the goblin table: the choices see only the tables a test gives.
    private static ContentCatalog WithGoblinRolling(params DropTable[] tables) =>
        _testWorld with
        {
            DropTables = tables.ToDictionary(table => table.Id),
            Enemies = new Dictionary<string, EnemyDefinition>
            {
                ["goblin"] = _testWorld.Enemies["goblin"] with { DropTableId = "goblin" },
            },
        };

    private static DropTable Table(string id) => new(id, [new DropEntry("potion", 10)]);
}
