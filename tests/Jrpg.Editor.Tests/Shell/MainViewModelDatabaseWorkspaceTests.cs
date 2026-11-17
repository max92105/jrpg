using Jrpg.Core.Loot;
using Jrpg.Editor.Workspaces;
using static Jrpg.Editor.Tests.Shell.ShopsPageSteps;

namespace Jrpg.Editor.Tests.Shell;

/// <summary>
/// The mode bar going through the Database workspace on the test world (decisions 917, 919, 922, 926): Database opens
/// the page list, a Database menu entry shows its page, a page's Edit... another page on a record (decision 928), and
/// World or Back to previous map leave the Database mode only once the shown page applies.
/// </summary>
public sealed class MainViewModelDatabaseWorkspaceTests : IDisposable
{
    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void DatabaseWorkspace_ContentLoaded_ListsEveryPageInGroupOrder()
    {
        var viewModel = _content.Open(_content.TestWorld);

        Assert.Equal(
            ["Quests", "Cinematics", "Enemies", "Encounters", "Skills", "Effects", "Statuses", "Drop tables",
                "Effect strips", "Battle backgrounds", "Items", "Affixes", "Shops", "Character sprites"],
            viewModel.DatabaseWorkspace!.Pages.Select(page => page.Title));
    }

    [Fact]
    public void ShowDatabase_FirstVisit_ShowsTheFirstPage()
    {
        var viewModel = _content.Open(_content.TestWorld);

        viewModel.ShowDatabaseCommand.Execute(null);

        Assert.Equal("Quests", viewModel.DatabaseWorkspace!.ShownPage!.Title);
    }

    [Fact]
    public void ShowDatabase_DatabaseShown_KeepsWhatWasTyped()
    {
        var viewModel = OpenOnArmory(_content, _content.TestWorld);
        ShownShops(viewModel).SelectedShop!.Name = "Old Armory";

        viewModel.ShowDatabaseCommand.Execute(null);

        Assert.Equal("Old Armory", ShownShops(viewModel).SelectedShop!.Name);
    }

    [Fact]
    public void ShowDatabasePage_Shops_ShowsDatabaseOnTheShopsPage()
    {
        var viewModel = _content.Open(_content.TestWorld);

        viewModel.ShowDatabasePageCommand.Execute("Shops");

        Assert.Equal((EditorWorkspace.Database, "Shops"), (viewModel.Workspace, viewModel.DatabaseWorkspace!.ShownPage!.Title));
    }

    [Fact]
    public void ShowDatabasePage_ShownPageWithProblem_KeepsIt()
    {
        var viewModel = OpenOnArmory(_content, _content.TestWorld);
        ShownShops(viewModel).AddShopCommand.Execute(null);

        viewModel.ShowDatabasePageCommand.Execute("Quests");

        Assert.Equal("Shops", viewModel.DatabaseWorkspace!.ShownPage!.Title);
        Assert.StartsWith("The Shops page has a problem: ", viewModel.DatabaseWorkspace.Refusal);
    }

    [Fact]
    public void ShowDatabasePageCommand_KnownTitle_IsEnabled()
    {
        var viewModel = _content.Open(_content.TestWorld);

        Assert.True(viewModel.ShowDatabasePageCommand.CanExecute("Character sprites"));
    }

    [Fact]
    public void ShowDatabasePageCommand_UnknownTitle_IsGreyed()
    {
        var viewModel = _content.Open(_content.TestWorld);

        Assert.False(viewModel.ShowDatabasePageCommand.CanExecute("Nowhere"));
    }

    [Fact]
    public void ShowDatabasePageOnRecord_DropTables_ShowsThePageOnTheTable()
    {
        var viewModel = _content.Open(_content.TestWorld);
        // Vault comes after crypt in the page's id order, so it is never the table chosen first.
        viewModel.Content!.SetDropTables([.. viewModel.Catalog!.DropTables.Values, Table("crypt"), Table("vault")]);
        viewModel.ShowDatabasePageCommand.Execute("Enemies");

        viewModel.ShowDatabasePageOnRecord("Drop tables", "vault");

        var workspace = viewModel.DatabaseWorkspace!;
        Assert.Equal(("Drop tables", "vault"), (workspace.ShownPage!.Title, workspace.Session!.Edited.ChosenKey));
    }

    [Fact]
    public void ShowWorld_TypedName_AppliesIt()
    {
        var viewModel = OpenOnArmory(_content, _content.TestWorld);
        ShownShops(viewModel).SelectedShop!.Name = "Old Armory";

        viewModel.ShowWorldCommand.Execute(null);

        Assert.Equal("Old Armory", viewModel.Catalog!.Shops["armory"].Name);
    }

    [Fact]
    public void ShowWorld_FromDatabase_EndsThePageSession()
    {
        var viewModel = OpenOnArmory(_content, _content.TestWorld);

        viewModel.ShowWorldCommand.Execute(null);

        Assert.Null(viewModel.DatabaseWorkspace!.Session);
    }

    [Fact]
    public void ShowWorld_PageWithProblem_StaysInDatabase()
    {
        var viewModel = OpenOnArmory(_content, _content.TestWorld);
        ShownShops(viewModel).AddShopCommand.Execute(null);

        viewModel.ShowWorldCommand.Execute(null);

        Assert.Equal(EditorWorkspace.Database, viewModel.Workspace);
        Assert.StartsWith("The Shops page has a problem: ", viewModel.DatabaseWorkspace!.Refusal);
    }

    [Fact]
    public void GoBackToMap_PageWithProblem_StaysOnTheMap()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var firstMapId = viewModel.CurrentMap!.Map.Id;
        var secondMapId = viewModel.MapIds.First(id => id != firstMapId);
        viewModel.TryOpenMap(secondMapId);
        ShowArmory(viewModel);
        ShownShops(viewModel).AddShopCommand.Execute(null);

        viewModel.GoBackToMapCommand.Execute(null);

        Assert.Equal((EditorWorkspace.Database, secondMapId), (viewModel.Workspace, viewModel.CurrentMap!.Map.Id));
    }

    private static DropTable Table(string id) => new(id, [new DropEntry("potion", 10)]);
}
