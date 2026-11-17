using Jrpg.Core.Loot;
using Jrpg.Data.Content;
using Jrpg.Editor.Battle;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Loot;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Tests.Workspaces;

/// <summary>
/// An Edit... reaching another catalog shows its page on a record, the shown page applied first or the move refused
/// (decision 928), and a page shown again starts from the draft, so the Enemies page offers a table added since. The
/// draft is the test world's with two tables the test adds, crypt before vault; the pages are Enemies and Drop tables.
/// </summary>
public sealed class DatabaseWorkspaceOnRecordTests
{
    private readonly CatalogDraft _draft = new(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world")));
    private readonly CatalogPage _enemiesPage;
    private readonly CatalogPage _dropTablesPage;
    private readonly DatabaseWorkspaceViewModel _workspace;

    public DatabaseWorkspaceOnRecordTests()
    {
        _draft.SetDropTables([.. _draft.Current.DropTables.Values, Table("crypt"), Table("vault")]);
        _enemiesPage = CatalogPage.Of("Enemies", CatalogGroup.Battle, new EnemyEditingViewModel(_draft, _ => []));
        _dropTablesPage = CatalogPage.Of("Drop tables", CatalogGroup.Battle, new DropTableEditingViewModel(_draft));
        _workspace = new DatabaseWorkspaceViewModel(_draft, [_enemiesPage, _dropTablesPage]);
    }

    private EnemyCatalogViewModel ShownEnemies => (EnemyCatalogViewModel)_workspace.Session!.Edited;

    private DropTableCatalogViewModel ShownTables => (DropTableCatalogViewModel)_workspace.Session!.Edited;

    [Fact]
    public void ShowPageOnRecord_Key_ShowsThePageOnIt()
    {
        _workspace.Open();

        _workspace.ShowPageOnRecord(_dropTablesPage, "vault");

        Assert.Same(_dropTablesPage, _workspace.ShownPage);
        Assert.Equal("vault", ShownTables.ChosenKey);
    }

    [Fact]
    public void ShowPageOnRecord_NoTableNamed_ShowsTheFirstTable()
    {
        _workspace.Open();

        _workspace.ShowPageOnRecord(_dropTablesPage, "");

        Assert.Equal(ShownTables.Tables[0].Id, ShownTables.ChosenKey);
    }

    [Fact]
    public void ShowPageOnRecord_TypedName_AppliesTheShownPageFirst()
    {
        _workspace.Open();
        var enemy = ShownEnemies.SelectedEnemy!;
        enemy.Name = "Crypt Lurker";

        _workspace.ShowPageOnRecord(_dropTablesPage, "vault");

        Assert.Equal("Crypt Lurker", _draft.Current.Enemies[enemy.Id].Name);
    }

    [Fact]
    public void ShowPageOnRecord_ShownPageWithProblem_KeepsTheShownPage()
    {
        _workspace.Open();
        ShownEnemies.AddEnemyCommand.Execute(null);
        var session = _workspace.Session;

        _workspace.ShowPageOnRecord(_dropTablesPage, "vault");

        Assert.Same(_enemiesPage, _workspace.ShownPage);
        Assert.Same(session, _workspace.Session);
        Assert.StartsWith("The Enemies page has a problem: ", _workspace.Refusal);
    }

    [Fact]
    public void DraftChanged_DropTableAdded_TheEnemiesPageOffersIt()
    {
        _workspace.Open();

        _draft.SetDropTables([.. _draft.Current.DropTables.Values, Table("lair")]);

        Assert.Contains("lair", ShownEnemies.DropTableChoices);
    }

    private static DropTable Table(string id) => new(id, [new DropEntry("potion", 10)]);
}
