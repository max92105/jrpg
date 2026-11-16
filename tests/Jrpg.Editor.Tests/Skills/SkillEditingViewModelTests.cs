using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Skills;

namespace Jrpg.Editor.Tests.Skills;

// The Skills page starts from the draft and its whole session is one undo step (decision 974).
public sealed class SkillEditingViewModelTests
{
    private readonly CatalogDraft _draft = new(SkillFixtures.CatalogWithoutUsers([SkillFixtures.Bolt], [SkillFixtures.Fire1]));
    private readonly SkillEditingViewModel _skillEditing;

    public SkillEditingViewModelTests() => _skillEditing = new SkillEditingViewModel(_draft, () => SkillFixtures.NoChoices);

    [Fact]
    public void Apply_EditedName_IsOneStep()
    {
        var steps = 0;
        _draft.StepRecorded += (_, _) => steps++;
        var edited = _skillEditing.Start();
        edited.Skills.Single(skill => skill.Id == "bolt").Name = "Thunder";

        _skillEditing.Apply(edited);

        Assert.Equal(1, steps);
        Assert.Equal("Thunder", _draft.Current.Skills["bolt"].Name);
    }

    // The cost lists are rebuilt from their rows, so only a comparison by content keeps the step out.
    [Fact]
    public void Apply_Untouched_RecordsNoStep()
    {
        var steps = 0;
        _draft.StepRecorded += (_, _) => steps++;
        var edited = _skillEditing.Start();

        _skillEditing.Apply(edited);

        Assert.Equal(0, steps);
    }
}
