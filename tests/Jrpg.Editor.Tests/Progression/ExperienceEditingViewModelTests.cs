using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Progression;
using static Jrpg.Editor.Tests.Progression.ExperienceFixtures;

namespace Jrpg.Editor.Tests.Progression;

// The Experience page starts from the draft as it is now, and its whole session is one undo step (decision 956).
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
}
