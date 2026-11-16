using Jrpg.Core.Statuses;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Skills;

namespace Jrpg.Editor.Tests.Skills;

// The Skills page starts from the draft and its whole session is one undo step (decision 974).
public sealed class SkillEditingViewModelTests
{
    private readonly CatalogDraft _draft = new(SkillFixtures.CatalogWithoutUsers([SkillFixtures.Bolt], [SkillFixtures.Fire1]));
    private readonly SkillEditingViewModel _skillEditing;

    public SkillEditingViewModelTests() =>
        _skillEditing = new SkillEditingViewModel(_draft, () => SkillFixtures.PoisonOnlyChoices);

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

    // The grants are rebuilt from their rows over the page's records, here copies of the draft's: equal by value, never
    // the same instances, so only a comparison by content keeps the step out.
    [Fact]
    public void Apply_UntouchedSkillWithTwoGrants_RecordsNoStep()
    {
        var stun = SkillFixtures.Poison1 with { Id = "stun", Name = "Stun" };
        var bolt = SkillFixtures.Bolt with
        {
            Effect = SkillFixtures.Bolt.Effect with
            {
                Statuses = [new StatusGrant(stun, 1) { Chance = 60 }, new StatusGrant(SkillFixtures.Poison1, 2)],
            },
        };
        var draft = new CatalogDraft(
            SkillFixtures.CatalogWithoutUsers([bolt], [SkillFixtures.Fire1]) with
            {
                Statuses = new Dictionary<string, StatusDefinition> { ["poison_1"] = SkillFixtures.Poison1, ["stun"] = stun },
            });
        var copies = new Dictionary<string, StatusDefinition>
        {
            ["poison_1"] = SkillFixtures.Poison1 with { },
            ["stun"] = stun with { },
        };
        var editing = new SkillEditingViewModel(draft, () => SkillFixtures.PoisonOnlyChoices with { StatusRecords = copies });
        var steps = 0;
        draft.StepRecorded += (_, _) => steps++;

        editing.Apply(editing.Start());

        Assert.Equal(0, steps);
    }
}
