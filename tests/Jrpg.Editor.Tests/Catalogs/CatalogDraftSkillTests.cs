using Jrpg.Core.Resources;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Tests.Skills;

namespace Jrpg.Editor.Tests.Catalogs;

// The skills are replaced whole as one undo step, compared by content so a page reopened and closed unchanged records
// nothing, costs and the effect's grants included.
public sealed class CatalogDraftSkillTests
{
    private readonly CatalogDraft _draft =
        new(SkillFixtures.CatalogWith([SkillFixtures.Bolt], [SkillFixtures.Fire1], [SkillFixtures.Poison1]));

    [Fact]
    public void SetSkills_ChangedName_IsOneUndoStep()
    {
        var steps = 0;
        _draft.StepRecorded += (_, _) => steps++;

        _draft.SetSkills([SkillFixtures.Bolt with { Name = "Thunder" }]);

        Assert.Equal(1, steps);
        Assert.Equal("Thunder", _draft.Current.Skills["bolt"].Name);
    }

    [Fact]
    public void SetSkills_EqualByContentWithFreshLists_RecordsNothing()
    {
        var bolt = SkillFixtures.Bolt;
        var copy = bolt with
        {
            ResourceChanges = [.. bolt.ResourceChanges],
            Requirements = [.. bolt.Requirements],
            Effect = bolt.Effect with { Statuses = [.. bolt.Effect.Statuses] },
        };

        _draft.SetSkills([copy]);

        Assert.False(_draft.CanUndo);
    }

    [Fact]
    public void SetSkills_CostChanged_RecordsAStep()
    {
        _draft.SetSkills([SkillFixtures.Bolt with { ResourceChanges = [new ResourceChange("mana", -3)] }]);

        Assert.True(_draft.CanUndo);
    }

    [Fact]
    public void SetSkills_RequirementChanged_RecordsAStep()
    {
        _draft.SetSkills([SkillFixtures.Bolt with { Requirements = [new ResourceRequirement("mana", 3)] }]);

        Assert.True(_draft.CanUndo);
    }

    [Fact]
    public void SetSkills_AddedSkill_UndoRemovesIt()
    {
        _draft.SetSkills([SkillFixtures.Bolt, SkillFixtures.Bolt with { Id = "spark" }]);

        _draft.Undo();

        Assert.False(_draft.Current.Skills.ContainsKey("spark"));
        Assert.Same(SkillFixtures.Bolt, _draft.Current.Skills["bolt"]);
    }
}
