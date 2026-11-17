using Jrpg.Core.Trade;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Quests;
using Jrpg.Editor.Trade;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Tests.Workspaces;

/// <summary>
/// A Database page's session (ADR 0060, decisions 918, 921, 922): started from the draft, applied as one undo step and
/// started again on the same record, reverted, and reopened where it was left. The draft is the test world's with two
/// shops set by the test, the Shops page first of two.
/// </summary>
public sealed class DatabaseWorkspaceViewModelTests
{
    private static readonly ShopDefinition _armory = new("armory", "Armory", ["bronze_sword"]);
    private static readonly ShopDefinition _smithy = new("smithy", "Smithy", ["potion"]);

    private readonly CatalogDraft _draft = new(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world")));
    private readonly CatalogPage _shopsPage;
    private readonly DatabaseWorkspaceViewModel _workspace;
    private int _stepsRecorded;

    public DatabaseWorkspaceViewModelTests()
    {
        _draft.SetShops([_armory, _smithy]);
        _draft.StepRecorded += (_, _) => _stepsRecorded++;
        _shopsPage = CatalogPage.Of("Shops", new ShopEditingViewModel(_draft));
        var questsPage = CatalogPage.Of("Quests", new QuestEditingViewModel(_draft));
        _workspace = new DatabaseWorkspaceViewModel(_draft, [_shopsPage, questsPage]);
    }

    private ShopCatalogViewModel ShownShops => (ShopCatalogViewModel)_workspace.Session!.Edited;

    [Fact]
    public void Open_FirstVisit_ShowsTheFirstPage()
    {
        _workspace.Open();

        Assert.Same(_shopsPage, _workspace.ShownPage);
    }

    [Fact]
    public void Open_FirstVisit_StartsItsSessionFromTheDraft()
    {
        _workspace.Open();

        Assert.Equal(["armory", "smithy"], ShownShops.Shops.Select(shop => shop.Id));
        Assert.Equal("armory", ShownShops.ChosenKey);
    }

    [Fact]
    public void Apply_TypedName_IsOneUndoStep()
    {
        _workspace.Open();
        ShownShops.SelectedShop!.Name = "Old Armory";

        _workspace.ApplyCommand.Execute(null);

        Assert.Equal(1, _stepsRecorded);
        Assert.Equal("Old Armory", _draft.Current.Shops["armory"].Name);
    }

    [Fact]
    public void Apply_TypedNameThenUndo_PutsTheOldNameBack()
    {
        _workspace.Open();
        ShownShops.SelectedShop!.Name = "Old Armory";
        _workspace.ApplyCommand.Execute(null);

        _draft.Undo();

        Assert.Equal("Armory", _draft.Current.Shops["armory"].Name);
    }

    [Fact]
    public void Apply_AddedShop_StartsAgainOnItWithItsIdFixed()
    {
        _workspace.Open();
        TypeNewForge();
        AddPotionToChosenShop();

        _workspace.ApplyCommand.Execute(null);

        Assert.Equal("forge", ShownShops.ChosenKey);
        Assert.False(ShownShops.SelectedShop!.IsNew);
    }

    [Fact]
    public void Apply_Unchanged_RecordsNothing()
    {
        _workspace.Open();

        _workspace.ApplyCommand.Execute(null);

        Assert.Equal(0, _stepsRecorded);
    }

    [Fact]
    public void ApplyCommand_ProblemShown_IsGreyed()
    {
        _workspace.Open();

        ShownShops.AddShopCommand.Execute(null);

        Assert.NotNull(ShownShops.Problem);
        Assert.False(_workspace.ApplyCommand.CanExecute(null));
    }

    [Fact]
    public void ApplyCommand_ProblemFixed_RaisesCanExecuteChanged()
    {
        _workspace.Open();
        TypeNewForge();
        var raised = 0;
        _workspace.ApplyCommand.CanExecuteChanged += (_, _) => raised++;

        AddPotionToChosenShop();

        Assert.True(raised > 0);
        Assert.True(_workspace.ApplyCommand.CanExecute(null));
    }

    [Fact]
    public void Revert_ProblemShown_RaisesApplyCanExecuteChanged()
    {
        _workspace.Open();
        ShownShops.AddShopCommand.Execute(null);
        var raised = 0;
        _workspace.ApplyCommand.CanExecuteChanged += (_, _) => raised++;

        _workspace.RevertCommand.Execute(null);

        Assert.True(raised > 0);
        Assert.True(_workspace.ApplyCommand.CanExecute(null));
    }

    [Fact]
    public void Revert_TypedName_StartsFromTheDraftOnTheSameShop()
    {
        _workspace.Open();
        ShownShops.Choose("smithy");
        ShownShops.SelectedShop!.Name = "Forge";

        _workspace.RevertCommand.Execute(null);

        Assert.Equal("smithy", ShownShops.ChosenKey);
        Assert.Equal("Smithy", ShownShops.SelectedShop!.Name);
        Assert.Equal(0, _stepsRecorded);
    }

    [Fact]
    public void Close_Shown_DropsTheSession()
    {
        _workspace.Open();

        _workspace.Close();

        Assert.Null(_workspace.Session);
    }

    [Fact]
    public void Open_AfterClose_ReopensOnTheChosenShop()
    {
        _workspace.Open();
        ShownShops.Choose("smithy");
        _workspace.Close();

        _workspace.Open();

        Assert.Equal("smithy", ShownShops.ChosenKey);
    }

    [Fact]
    public void DraftChanged_Closed_StartsNothing()
    {
        _workspace.Open();
        _workspace.Close();

        _draft.SetShops([_armory]);

        Assert.Null(_workspace.Session);
    }

    [Fact]
    public void MarkSaved_TypedName_KeepsTheSession()
    {
        _workspace.Open();
        var session = _workspace.Session;
        ShownShops.SelectedShop!.Name = "Old Armory";

        _draft.MarkSaved();

        Assert.Same(session, _workspace.Session);
        Assert.Equal("Old Armory", ShownShops.SelectedShop!.Name);
    }

    // Leaves the shop without items: a shop must sell one, so the page shows a problem until AddPotionToChosenShop.
    private void TypeNewForge()
    {
        ShownShops.AddShopCommand.Execute(null);
        ShownShops.SelectedShop!.Id = "forge";
        ShownShops.SelectedShop.Name = "Forge";
    }

    private void AddPotionToChosenShop()
    {
        var shop = ShownShops.SelectedShop!;
        shop.ItemToAdd = ShownShops.OfferedItems.Single(item => item.Id == "potion");
        shop.AddItemCommand.Execute(null);
    }
}
