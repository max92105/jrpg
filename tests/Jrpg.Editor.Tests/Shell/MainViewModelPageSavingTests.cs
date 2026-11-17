using Jrpg.Data.Content;
using Jrpg.Editor.Shell;
using Jrpg.Editor.Tests.Workspaces;
using Jrpg.Editor.Workspaces;
using static Jrpg.Editor.Tests.Shell.ShopsPageSteps;

namespace Jrpg.Editor.Tests.Shell;

/// <summary>
/// Save, Save all and the close question with a Database page shown (decisions 923, 924): the Shops page on the shop
/// armory, or a test page whose apply throws; what was typed is written, or nothing is and the page is named.
/// </summary>
public sealed class MainViewModelPageSavingTests : IDisposable
{
    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void Save_TypedName_WritesIt()
    {
        var viewModel = OpenOnArmory(_content, _content.CopyOfTestWorld());
        ShownShops(viewModel).SelectedShop!.Name = "Old Armory";

        viewModel.SaveCommand.Execute(null);

        Assert.Equal("Old Armory", ContentCatalogLoader.Load(_content.ContentCopy).Shops["armory"].Name);
    }

    [Fact]
    public void Save_PageWithProblem_WritesNothingNamingThePage()
    {
        var viewModel = OpenOnArmory(_content, _content.CopyOfTestWorld());
        ShownShops(viewModel).AddShopCommand.Execute(null);

        viewModel.SaveCommand.Execute(null);

        Assert.False(ContentCatalogLoader.Load(_content.ContentCopy).Shops.ContainsKey("armory"));
        Assert.StartsWith("The Shops page has a problem: ", viewModel.SaveError, StringComparison.Ordinal);
        Assert.EndsWith(" Nothing was written.", viewModel.SaveError, StringComparison.Ordinal);
    }

    [Fact]
    public void Save_ApplyThrows_WritesNothingNamingThePage()
    {
        var viewModel = OpenOnArmory(_content, _content.CopyOfTestWorld());
        ShowThrowingPage(viewModel);

        viewModel.SaveCommand.Execute(null);

        Assert.False(ContentCatalogLoader.Load(_content.ContentCopy).Shops.ContainsKey("armory"));
        Assert.Equal("The Notes page can't apply: broken Nothing was written.", viewModel.SaveError);
    }

    [Fact]
    public void SaveAll_PageWithProblem_WritesNothingNamingThePage()
    {
        var viewModel = OpenOnArmory(_content, _content.CopyOfTestWorld());
        ShownShops(viewModel).AddShopCommand.Execute(null);

        viewModel.SaveAllCommand.Execute(null);

        Assert.False(ContentCatalogLoader.Load(_content.ContentCopy).Shops.ContainsKey("armory"));
        Assert.StartsWith("The Shops page has a problem: ", viewModel.SaveError, StringComparison.Ordinal);
        Assert.EndsWith(" Nothing was written.", viewModel.SaveError, StringComparison.Ordinal);
    }

    [Fact]
    public void Save_PageWithProblemFixed_ClearsTheSaveError()
    {
        var viewModel = OpenOnArmory(_content, _content.TestWorld);
        var shops = ShownShops(viewModel);
        shops.AddShopCommand.Execute(null);
        viewModel.SaveCommand.Execute(null);

        shops.RemoveShopCommand.Execute(null);

        Assert.Null(viewModel.SaveError);
    }

    // Only the line quoting the page's refusal clears with it.
    [Fact]
    public void SaveError_MapRefusedThenPageRefusalCleared_Stays()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        EditorTestContent.PaintUnknownTile(viewModel.CurrentMap!);
        viewModel.SaveCommand.Execute(null);
        ShowArmory(viewModel);
        var shops = ShownShops(viewModel);
        shops.AddShopCommand.Execute(null);
        viewModel.ShowWorldCommand.Execute(null);

        shops.RemoveShopCommand.Execute(null);

        Assert.StartsWith("map-layouts/forest.json: ", viewModel.SaveError, StringComparison.Ordinal);
    }

    // The problem's own period is left to the line's sentence.
    [Fact]
    public void UnsavedChanges_PageWithProblem_NamesThePage()
    {
        var viewModel = _content.Open(_content.TestWorld);
        viewModel.ShowDatabaseCommand.Execute(null);
        ShowPage(viewModel, CatalogPage.Of("Notes", CatalogGroup.Story, new NotesEditing()));
        ((NotesCatalog)viewModel.DatabaseWorkspace!.Session!.Edited).Problem = "A note has no title.";

        viewModel.ShowWorldCommand.Execute(null);

        Assert.Equal("Unsaved changes in the Notes page, not applied: A note has no title.", viewModel.UnsavedChanges);
    }

    [Fact]
    public void ConfirmClose_TypedName_AsksAndSaveAllWritesIt()
    {
        var viewModel = OpenOnArmory(_content, _content.CopyOfTestWorld(), UnsavedChangesAnswer.SaveAll);
        viewModel.SaveAllCommand.Execute(null);
        ShownShops(viewModel).SelectedShop!.Name = "Old Armory";

        Assert.True(viewModel.ConfirmClose());
        Assert.Equal(["Unsaved changes in the tiles and objects."], _content.Questions);
        Assert.Equal("Old Armory", ContentCatalogLoader.Load(_content.ContentCopy).Shops["armory"].Name);
    }

    [Fact]
    public void ConfirmClose_PageWithProblemAndSaveAll_StaysOpen()
    {
        var viewModel = OpenOnArmory(_content, _content.CopyOfTestWorld(), UnsavedChangesAnswer.SaveAll);
        ShownShops(viewModel).AddShopCommand.Execute(null);

        Assert.False(viewModel.ConfirmClose());
        Assert.False(ContentCatalogLoader.Load(_content.ContentCopy).Shops.ContainsKey("armory"));
    }

    [Fact]
    public void ConfirmClose_PageWithProblemAndDiscard_Closes()
    {
        var viewModel = OpenOnArmory(_content, _content.TestWorld, UnsavedChangesAnswer.Discard);
        ShownShops(viewModel).AddShopCommand.Execute(null);

        Assert.True(viewModel.ConfirmClose());
        Assert.Contains(" and the Shops page, not applied: ", Assert.Single(_content.Questions), StringComparison.Ordinal);
    }

    [Fact]
    public void ConfirmClose_ApplyThrows_DiscardCloses()
    {
        var viewModel = _content.Open(_content.TestWorld, UnsavedChangesAnswer.Discard);
        viewModel.ShowDatabaseCommand.Execute(null);
        ShowThrowingPage(viewModel);

        Assert.True(viewModel.ConfirmClose());
        Assert.Equal(["Unsaved changes in the Notes page, not applied: broken."], _content.Questions);
    }

    // A page built here whose Apply throws, as a bug its problem line missed would (decision 939).
    private static void ShowThrowingPage(MainViewModel viewModel) =>
        ShowPage(
            viewModel,
            CatalogPage.Of("Notes", CatalogGroup.Story, new NotesEditing { Thrown = new InvalidOperationException("broken") }));
}
