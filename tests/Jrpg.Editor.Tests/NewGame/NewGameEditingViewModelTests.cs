using Jrpg.Editor.Catalogs;
using Jrpg.Editor.NewGame;
using static Jrpg.Editor.Tests.NewGame.NewGameFixtures;

namespace Jrpg.Editor.Tests.NewGame;

// The New game page starts from the draft as it is now, and its whole session is one undo step (decision 968).
public sealed class NewGameEditingViewModelTests
{
    private readonly CatalogDraft _draft = new(World);

    private readonly NewGameEditingViewModel _editing;

    public NewGameEditingViewModelTests() => _editing = new NewGameEditingViewModel(_draft);

    [Fact]
    public void Apply_EditedGold_ChangesTheDraft()
    {
        var edited = _editing.Start();
        edited.GoldText = "900";

        _editing.Apply(edited);

        Assert.Equal(900, _draft.Current.NewGame.StartingGold);
    }

    [Fact]
    public void Apply_Untouched_RecordsNothing()
    {
        _editing.Apply(_editing.Start());

        Assert.False(_draft.CanUndo);
    }
}
