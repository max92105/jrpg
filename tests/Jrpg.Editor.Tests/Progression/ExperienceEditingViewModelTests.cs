using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Progression;
using static Jrpg.Editor.Tests.Progression.ExperienceFixtures;

namespace Jrpg.Editor.Tests.Progression;

// The Experience page starts from the draft as it is now, its max level note measured against the saved table, and its
// whole session is one undo step (decision 956).
public sealed class ExperienceEditingViewModelTests
{
    private readonly CatalogDraft _draft = new(World);

    private readonly ExperienceEditingViewModel _editing;

    public ExperienceEditingViewModelTests() => _editing = new ExperienceEditingViewModel(_draft);

    [Fact]
    public void Apply_EditedRow_ChangesTheDraftsTable()
    {
        var edited = _editing.Start();
        edited.Rows[2].AmountText = "45";

        _editing.Apply(edited);

        Assert.Equal([10, 20, 45], _draft.Current.Rules.Experience.ExperienceToNextLevel);
    }

    [Fact]
    public void Apply_Untouched_RecordsNothing()
    {
        _editing.Apply(_editing.Start());

        Assert.False(_draft.CanUndo);
    }

    // Applying restarts the page, but rules.json and the saves still hold the old max level until Save.
    [Fact]
    public void Start_LastLevelRemovedAndApplied_NoteNamesTheSavedMax()
    {
        var editing = new ExperienceEditingViewModel(DraftWithLastLevelRemovedAndApplied());

        var again = editing.Start();

        Assert.Equal("Max level 4, was 5: saved members above it drop to 4 when a save loads.", again.MaxLevelNote);
    }

    [Fact]
    public void Start_LastLevelRemovedAppliedAndSaved_HasNoNote()
    {
        var draft = DraftWithLastLevelRemovedAndApplied();
        draft.MarkSaved();

        var again = new ExperienceEditingViewModel(draft).Start();

        Assert.Null(again.MaxLevelNote);
    }

    // Four steps, so the skilled archer's expose at level 4 stays within the table once the last is removed.
    private static CatalogDraft DraftWithLastLevelRemovedAndApplied()
    {
        var draft = new CatalogDraft(WithTable(new([10, 20, 40, 80])));
        var editing = new ExperienceEditingViewModel(draft);
        var edited = editing.Start();
        edited.RemoveLastLevelCommand.Execute(null);
        editing.Apply(edited);
        return draft;
    }
}
