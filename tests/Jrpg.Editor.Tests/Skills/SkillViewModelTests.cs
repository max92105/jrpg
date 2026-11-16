using Jrpg.Core.Resources;
using Jrpg.Editor.Skills;

namespace Jrpg.Editor.Tests.Skills;

// One skill as the Skills page edits it: its texts, files and costs as typed, the id fixed once saved, and what no box
// edits yet (the effect) carried through from the record.
public sealed class SkillViewModelTests
{
    [Fact]
    public void From_Skill_FillsEveryBox()
    {
        var skill = SkillViewModel.From(SkillFixtures.Bolt with { IsQuick = true }, "mana");

        Assert.Equal(("bolt", "Bolt", "A bolt of fire that poisons."), (skill.IdText, skill.Name, skill.Description));
        Assert.Equal(("Bolt.png", "Bolt.wav", "BoltStrip.png"), (skill.IconFile, skill.SoundFile, skill.EffectFile));
        Assert.True(skill.IsQuick);
        Assert.False(skill.IsNew);
        Assert.Null(skill.Problem);
    }

    [Fact]
    public void ToSkill_EditedTexts_KeepsTheEffectAndCostsByContent()
    {
        var bolt = SkillFixtures.Bolt;
        var skill = SkillViewModel.From(bolt, "mana");

        skill.Name = " Spark ";
        skill.Description = "A small spark.";
        skill.IconFile = "Spark.png";
        skill.SoundFile = "Spark.wav";

        // The cost lists are rebuilt from their rows, so they are compared by content below.
        var edited = skill.ToSkill();
        Assert.Equal(
            bolt with
            {
                Name = "Spark",
                Description = "A small spark.",
                IconFile = "Spark.png",
                SoundFile = "Spark.wav",
                ResourceChanges = edited.ResourceChanges,
                Requirements = edited.Requirements,
            },
            edited);
        Assert.Same(bolt.Effect, edited.Effect);
        Assert.Equal(bolt.ResourceChanges, edited.ResourceChanges);
        Assert.Equal(bolt.Requirements, edited.Requirements);
    }

    [Fact]
    public void ToSkill_AmountRetyped_WritesTheTypedCost()
    {
        var skill = SkillViewModel.From(SkillFixtures.Bolt, "mana");

        skill.Costs.Changes[0].AmountText = "-3";

        Assert.Equal([new ResourceChange("mana", -3)], skill.ToSkill().ResourceChanges);
    }

    [Fact]
    public void Problem_CostRowNotANumber_IsTheRowProblem()
    {
        var skill = SkillViewModel.From(SkillFixtures.Bolt, "mana");

        skill.Costs.Needs[0].AtLeastText = "two";

        Assert.Equal("Needs row 1: At least must be a whole number (was 'two').", skill.Problem);
    }

    // Copied from the boxes: reading the half-typed amount would throw.
    [Fact]
    public void DuplicateOf_AmountHalfTyped_CopiesTheTypedRows()
    {
        var chosen = SkillViewModel.From(SkillFixtures.Bolt, "mana");
        chosen.Costs.Changes[0].AmountText = "-";

        var copy = SkillViewModel.DuplicateOf(chosen, ["bolt"]);

        Assert.Equal(("mana", "-"), (copy.Costs.Changes[0].ResourceId, copy.Costs.Changes[0].AmountText));
        Assert.NotSame(chosen.Costs.Changes[0], copy.Costs.Changes[0]);
    }

    [Fact]
    public void ToSkill_EmptyStrip_WritesNone()
    {
        var skill = SkillViewModel.From(SkillFixtures.Bolt, "mana");

        skill.EffectFile = " ";

        Assert.Null(skill.ToSkill().EffectFile);
    }

    [Fact]
    public void ToSkill_StripWithSpaces_TrimsIt()
    {
        var skill = SkillViewModel.From(SkillFixtures.Bolt, "mana");

        skill.EffectFile = " Spark.png ";

        Assert.Equal("Spark.png", skill.ToSkill().EffectFile);
    }

    [Fact]
    public void ToSkill_NewSkillWithAFreeId_TakesItTrimmed()
    {
        var skill = SkillViewModel.ForNew(SkillFixtures.Bolt, ["bolt"], "mana");

        skill.IdText = " spark ";

        Assert.Equal("spark", skill.ToSkill().Id);
    }

    [Fact]
    public void ToSkill_Problem_Throws()
    {
        var skill = SkillViewModel.ForNew(SkillFixtures.Bolt, ["bolt"], "mana");

        Assert.Throws<InvalidOperationException>(skill.ToSkill);
    }

    [Fact]
    public void Problem_NewSkillWithATakenId_NamesIt()
    {
        var skill = SkillViewModel.ForNew(SkillFixtures.Bolt, ["bolt"], "mana");

        skill.IdText = "bolt";

        Assert.Equal("The id bolt is taken.", skill.Problem);
    }

    [Fact]
    public void Changed_TypedName_Raised()
    {
        var skill = SkillViewModel.From(SkillFixtures.Bolt, "mana");
        var raised = 0;
        skill.Changed += (_, _) => raised++;

        skill.Name = "Thunder";

        Assert.Equal(1, raised);
    }

    // Typing the id also raises its Id and Problem notices; only the typed field tells the list.
    [Fact]
    public void Changed_ProblemNotice_NotRaised()
    {
        var skill = SkillViewModel.ForNew(SkillFixtures.Bolt, ["bolt"], "mana");
        var raised = 0;
        skill.Changed += (_, _) => raised++;

        skill.IdText = "spark";

        Assert.Equal(1, raised);
    }

    [Fact]
    public void Changed_CostTyped_Raised()
    {
        var skill = SkillViewModel.From(SkillFixtures.Bolt, "mana");
        var raised = 0;
        skill.Changed += (_, _) => raised++;

        skill.Costs.Changes[0].AmountText = "-3";

        Assert.Equal(1, raised);
    }
}
