using static Jrpg.Editor.Tests.Shell.ShopsPageSteps;

namespace Jrpg.Editor.Tests.Shell;

/// <summary>
/// Undo and Redo with the Shops page shown on the shop armory, on the test world (decision 920): what was typed is
/// applied first, as one step, or the undo or redo is refused naming the page.
/// </summary>
public sealed class MainViewModelPageUndoTests : IDisposable
{
    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void Undo_TypedName_TakesItBackAsOneStep()
    {
        var viewModel = OpenOnArmory(_content, _content.TestWorld);
        ShownShops(viewModel).SelectedShop!.Name = "Old Armory";

        viewModel.History!.UndoCommand.Execute(null);

        Assert.Equal("Armory", viewModel.Catalog!.Shops["armory"].Name);
    }

    [Fact]
    public void Redo_AfterUndoOfTypedName_BringsItBack()
    {
        var viewModel = OpenOnArmory(_content, _content.TestWorld);
        ShownShops(viewModel).SelectedShop!.Name = "Old Armory";
        viewModel.History!.UndoCommand.Execute(null);

        viewModel.History.RedoCommand.Execute(null);

        Assert.Equal("Old Armory", viewModel.Catalog!.Shops["armory"].Name);
    }

    [Fact]
    public void Redo_TypedChangesPending_AppliesAndDropsTheRedo()
    {
        var viewModel = OpenOnArmory(_content, _content.TestWorld);
        ShownShops(viewModel).SelectedShop!.Name = "Old Armory";
        viewModel.History!.UndoCommand.Execute(null);
        ShownShops(viewModel).SelectedShop!.Name = "Big Armory";

        viewModel.History.RedoCommand.Execute(null);

        Assert.Equal(
            ("Big Armory", false),
            (viewModel.Catalog!.Shops["armory"].Name, viewModel.History.RedoCommand.CanExecute(null)));
    }

    [Fact]
    public void Undo_PageWithProblem_RefusedNamingThePage()
    {
        var viewModel = OpenOnArmory(_content, _content.TestWorld);
        ShownShops(viewModel).AddShopCommand.Execute(null);

        viewModel.History!.UndoCommand.Execute(null);

        Assert.StartsWith("The Shops page has a problem: ", viewModel.History.Refusal, StringComparison.Ordinal);
    }

    [Fact]
    public void Undo_PageWithProblem_KeepsWhatWasTyped()
    {
        var viewModel = OpenOnArmory(_content, _content.TestWorld);
        var shops = ShownShops(viewModel);
        shops.SelectedShop!.Name = "Old Armory";
        shops.AddShopCommand.Execute(null);

        viewModel.History!.UndoCommand.Execute(null);

        Assert.Equal(
            ("Armory", "Old Armory"),
            (viewModel.Catalog!.Shops["armory"].Name, ShownShops(viewModel).Shops.Single(shop => shop.Id == "armory").Name));
    }

    [Fact]
    public void Undo_PageWithProblemFixed_ClearsTheRefusal()
    {
        var viewModel = OpenOnArmory(_content, _content.TestWorld);
        var shops = ShownShops(viewModel);
        shops.AddShopCommand.Execute(null);
        viewModel.History!.UndoCommand.Execute(null);

        shops.RemoveShopCommand.Execute(null);

        Assert.Null(viewModel.History.Refusal);
    }

    [Fact]
    public void Undo_PageRevertedAfterARefusal_ClearsTheRefusal()
    {
        var viewModel = OpenOnArmory(_content, _content.TestWorld);
        ShownShops(viewModel).AddShopCommand.Execute(null);
        viewModel.History!.UndoCommand.Execute(null);

        viewModel.DatabaseWorkspace!.RevertCommand.Execute(null);

        Assert.Null(viewModel.History.Refusal);
    }

    [Fact]
    public void Redo_PageWithProblem_RefusedNamingThePage()
    {
        var viewModel = OpenOnArmory(_content, _content.TestWorld);
        ShownShops(viewModel).SelectedShop!.Name = "Old Armory";
        viewModel.History!.UndoCommand.Execute(null);
        ShownShops(viewModel).AddShopCommand.Execute(null);

        viewModel.History.RedoCommand.Execute(null);

        Assert.StartsWith("The Shops page has a problem: ", viewModel.History.Refusal, StringComparison.Ordinal);
    }
}
