using Jrpg.Core.Trade;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Trade;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Tests.Workspaces;

/// <summary>
/// Leaving a Database page applies it first, or is refused naming the page and why (ADR 0060, decisions 919, 939,
/// 1008); a new catalog in the draft starts the shown page again on its record (decision 921). The draft is the test
/// world's with two shops set by the test; the pages are Shops and a Notes page built here, its problem and its throw
/// set by the test.
/// </summary>
public sealed class DatabaseWorkspaceLeavingTests
{
    private static readonly ShopDefinition _armory = new("armory", "Armory", ["bronze_sword"]);
    private static readonly ShopDefinition _smithy = new("smithy", "Smithy", ["potion"]);

    private readonly CatalogDraft _draft = new(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world")));
    private readonly NotesEditing _notesEditing = new();
    private readonly CatalogPage _shopsPage;
    private readonly CatalogPage _notesPage;
    private readonly DatabaseWorkspaceViewModel _workspace;

    public DatabaseWorkspaceLeavingTests()
    {
        _draft.SetShops([_armory, _smithy]);
        _shopsPage = CatalogPage.Of("Shops", new ShopEditingViewModel(_draft));
        _notesPage = CatalogPage.Of("Notes", _notesEditing);
        _workspace = new DatabaseWorkspaceViewModel(_draft, [_shopsPage, _notesPage]);
    }

    private ShopCatalogViewModel ShownShops => (ShopCatalogViewModel)_workspace.Session!.Edited;

    private NotesCatalog ShownNotes => (NotesCatalog)_workspace.Session!.Edited;

    [Fact]
    public void TryApplyShown_TypedName_AppliesIt()
    {
        _workspace.Open();
        ShownShops.SelectedShop!.Name = "Old Armory";

        var isApplied = _workspace.TryApplyShown();

        Assert.True(isApplied);
        Assert.Equal("Old Armory", _draft.Current.Shops["armory"].Name);
    }

    [Fact]
    public void TryApplyShown_Problem_RefusesNamingThePageAndItsProblem()
    {
        ShowNotesWithProblem();

        var isApplied = _workspace.TryApplyShown();

        Assert.False(isApplied);
        Assert.Equal("The Notes page has a problem: No title. Fix it or press Revert.", _workspace.Refusal);
    }

    [Fact]
    public void TryApplyShown_Problem_AppliesNothing()
    {
        ShowNotesWithProblem();

        _workspace.TryApplyShown();

        Assert.Equal(0, _notesEditing.ApplyCount);
    }

    [Fact]
    public void TryApplyShown_ApplyThrows_RefusesNamingItsMessage()
    {
        _workspace.ShowPageCommand.Execute(_notesPage);
        _notesEditing.Thrown = new InvalidOperationException("broken");

        var isApplied = _workspace.TryApplyShown();

        Assert.False(isApplied);
        Assert.Equal("The Notes page can't apply: broken", _workspace.Refusal);
    }

    [Fact]
    public void TryApplyShown_ContentRefused_RefusesNamingItsMessage()
    {
        _workspace.ShowPageCommand.Execute(_notesPage);
        _notesEditing.Thrown = new ContentValidationException("notes.json: refused.");

        var isApplied = _workspace.TryApplyShown();

        Assert.False(isApplied);
        Assert.Equal("The Notes page can't apply: notes.json: refused.", _workspace.Refusal);
    }

    [Fact]
    public void TryApplyShown_NothingShown_Passes()
    {
        var isApplied = _workspace.TryApplyShown();

        Assert.True(isApplied);
        Assert.Null(_workspace.Refusal);
    }

    [Fact]
    public void ShowPage_TypedName_AppliesItFirst()
    {
        _workspace.Open();
        ShownShops.SelectedShop!.Name = "Old Armory";

        _workspace.ShowPageCommand.Execute(_notesPage);

        Assert.Equal("Old Armory", _draft.Current.Shops["armory"].Name);
        Assert.Same(_notesPage, _workspace.ShownPage);
    }

    [Fact]
    public void ShowPage_Problem_KeepsTheShownPage()
    {
        ShowNotesWithProblem();
        var session = _workspace.Session;

        _workspace.ShowPageCommand.Execute(_shopsPage);

        Assert.Same(_notesPage, _workspace.ShownPage);
        Assert.Same(session, _workspace.Session);
    }

    [Fact]
    public void ShowPage_AfterARefusal_ClearsTheRefusal()
    {
        _workspace.ShowPageCommand.Execute(_notesPage);
        _notesEditing.Thrown = new InvalidOperationException("broken");
        _workspace.ShowPageCommand.Execute(_shopsPage);
        _notesEditing.Thrown = null;

        _workspace.ShowPageCommand.Execute(_shopsPage);

        Assert.Null(_workspace.Refusal);
        Assert.Same(_shopsPage, _workspace.ShownPage);
    }

    [Fact]
    public void ShowPage_PageLeftBefore_ReopensOnItsChosenShop()
    {
        _workspace.Open();
        ShownShops.Choose("smithy");
        _workspace.ShowPageCommand.Execute(_notesPage);

        _workspace.ShowPageCommand.Execute(_shopsPage);

        Assert.Equal("smithy", ShownShops.ChosenKey);
    }

    [Fact]
    public void TryClose_Problem_KeepsTheSession()
    {
        ShowNotesWithProblem();
        var session = _workspace.Session;

        var isClosed = _workspace.TryClose();

        Assert.False(isClosed);
        Assert.Same(session, _workspace.Session);
    }

    [Fact]
    public void TryClose_NothingShown_Passes()
    {
        var isClosed = _workspace.TryClose();

        Assert.True(isClosed);
        Assert.Null(_workspace.Session);
    }

    [Fact]
    public void DraftChanged_UndoWhileShown_StartsAgainOnTheSameShop()
    {
        _workspace.Open();
        ShownShops.Choose("smithy");
        ShownShops.SelectedShop!.Name = "Forge";
        _workspace.TryApplyShown();

        _draft.Undo();

        Assert.Equal("smithy", ShownShops.ChosenKey);
        Assert.Equal("Smithy", ShownShops.SelectedShop!.Name);
    }

    [Fact]
    public void DraftChanged_ChosenShopUndoneAway_ChoosesTheFirst()
    {
        _workspace.Open();
        _draft.SetShops([_armory, _smithy, new ShopDefinition("forge", "Forge", ["potion"])]);
        ShownShops.Choose("forge");

        _draft.Undo();

        Assert.Equal("armory", ShownShops.ChosenKey);
    }

    [Fact]
    public void ProblemChanged_AfterARefusal_ClearsTheRefusal()
    {
        ShowNotesWithProblem();
        _workspace.TryApplyShown();

        ShownNotes.Problem = null;

        Assert.Null(_workspace.Refusal);
    }

    [Fact]
    public void Revert_AfterARefusal_ClearsTheRefusal()
    {
        ShowNotesWithProblem();
        _workspace.TryApplyShown();

        _workspace.RevertCommand.Execute(null);

        Assert.Null(_workspace.Refusal);
    }

    [Fact]
    public void UnappliedPageLine_Problem_NamesThePageAndItsProblem()
    {
        ShowNotesWithProblem();

        _workspace.TryApplyShown();

        Assert.Equal("the Notes page, not applied: No title", _workspace.UnappliedPageLine);
    }

    [Fact]
    public void UnappliedPageLine_ApplyThrows_NamesItsMessage()
    {
        _workspace.ShowPageCommand.Execute(_notesPage);
        _notesEditing.Thrown = new InvalidOperationException("broken");

        _workspace.TryApplyShown();

        Assert.Equal("the Notes page, not applied: broken", _workspace.UnappliedPageLine);
    }

    [Fact]
    public void UnappliedPageLine_ProblemFixedAfterARefusal_IsNull()
    {
        ShowNotesWithProblem();
        _workspace.TryApplyShown();

        ShownNotes.Problem = null;

        Assert.Null(_workspace.UnappliedPageLine);
    }

    private void ShowNotesWithProblem()
    {
        _workspace.ShowPageCommand.Execute(_notesPage);
        ShownNotes.Problem = "No title.";
    }
}
