using Jrpg.Core.Trade;
using Jrpg.Editor.Shell;
using Jrpg.Editor.Trade;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Tests.Shell;

/// <summary>
/// The main view model tests' steps in the Database mode: the Shops page shown on the shop armory, built here and set
/// on the draft first, and a page shown by its title or built by the test.
/// </summary>
internal static class ShopsPageSteps
{
    private static readonly ShopDefinition _armory = new("armory", "Armory", ["bronze_sword"]);

    /// <summary>The shown page's edited shops; the Shops page must be shown.</summary>
    public static ShopCatalogViewModel ShownShops(MainViewModel viewModel) =>
        (ShopCatalogViewModel)viewModel.DatabaseWorkspace!.Session!.Edited;

    /// <param name="content">The tests' content, which records the questions asked.</param>
    /// <param name="contentRoot">The test world, or a copy of it for a test that saves.</param>
    /// <param name="closeAnswer">What the user answers to the close question.</param>
    public static MainViewModel OpenOnArmory(
        EditorTestContent content,
        string contentRoot,
        UnsavedChangesAnswer closeAnswer = UnsavedChangesAnswer.Cancel)
    {
        var viewModel = content.Open(contentRoot, closeAnswer);
        ShowArmory(viewModel);
        return viewModel;
    }

    public static void ShowArmory(MainViewModel viewModel)
    {
        viewModel.Content!.SetShops([.. viewModel.Catalog!.Shops.Values, _armory]);
        viewModel.ShowDatabaseCommand.Execute(null);
        var workspace = viewModel.DatabaseWorkspace!;
        ShowPage(viewModel, workspace.Pages.Single(page => page.Title == "Shops"));
        ShownShops(viewModel).Choose("armory");
    }

    /// <summary>Shows the page in the Database mode, which must be shown.</summary>
    public static void ShowPage(MainViewModel viewModel, CatalogPage page) =>
        viewModel.DatabaseWorkspace!.ShowPageCommand.Execute(page);
}
