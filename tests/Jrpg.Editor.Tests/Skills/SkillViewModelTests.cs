using Jrpg.Core.Resources;
using Jrpg.Core.Statuses;
using Jrpg.Editor.Skills;

namespace Jrpg.Editor.Tests.Skills;

// One skill as the Skills page edits it: its texts, files, costs, effect and the statuses it grants as typed, the id
// fixed once saved.
public sealed class SkillViewModelTests
{
    [Fact]
    public void From_Skill_FillsEveryBox()
    {
        var skill = SkillViewModel.From(SkillFixtures.Bolt with { IsQuick = true }, SkillFixtures.PoisonOnlyChoices);

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
        var skill = SkillViewModel.From(bolt, SkillFixtures.PoisonOnlyChoices);

        skill.Name = " Spark ";
        skill.Description = "A small spark.";
        skill.IconFile = "Spark.png";
        skill.SoundFile = "Spark.wav";

        // The cost lists and the effect are rebuilt from their boxes, so they are compared by content.
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
        Assert.Equal(bolt.Effect, edited.Effect);
        Assert.Equal(bolt.ResourceChanges, edited.ResourceChanges);
        Assert.Equal(bolt.Requirements, edited.Requirements);
    }

    [Fact]
    public void ToSkill_AmountRetyped_WritesTheTypedCost()
    {
        var skill = SkillViewModel.From(SkillFixtures.Bolt, SkillFixtures.PoisonOnlyChoices);

        skill.Costs.Changes[0].AmountText = "-3";

        Assert.Equal([new ResourceChange("mana", -3)], skill.ToSkill().ResourceChanges);
    }

    [Fact]
    public void Problem_CostRowNotANumber_IsTheRowProblem()
    {
        var skill = SkillViewModel.From(SkillFixtures.Bolt, SkillFixtures.PoisonOnlyChoices);

        skill.Costs.Needs[0].AtLeastText = "two";

        Assert.Equal("Needs row 1: At least must be a whole number (was 'two').", skill.Problem);
    }

    [Fact]
    public void Problem_EffectBoxNotANumber_IsTheBoxProblem()
    {
        var skill = SkillViewModel.From(SkillFixtures.Bolt, SkillFixtures.PoisonOnlyChoices);

        skill.Effect.HitsText = "two";

        Assert.Equal("Hits must be a whole number (was 'two').", skill.Problem);
    }

    // Copied from the boxes: reading the half-typed amount would throw.
    [Fact]
    public void DuplicateOf_AmountHalfTyped_CopiesTheTypedRows()
    {
        var chosen = SkillViewModel.From(SkillFixtures.Bolt, SkillFixtures.PoisonOnlyChoices);
        chosen.Costs.Changes[0].AmountText = "-";

        var copy = SkillViewModel.DuplicateOf(chosen, ["bolt"]);

        Assert.Equal(("mana", "-"), (copy.Costs.Changes[0].ResourceId, copy.Costs.Changes[0].AmountText));
        Assert.NotSame(chosen.Costs.Changes[0], copy.Costs.Changes[0]);
    }

    [Fact]
    public void ToSkill_GrantsEdited_WritesTheRows()
    {
        var skill = SkillViewModel.From(SkillFixtures.Bolt, SkillFixtures.PoisonOnlyChoices);

        skill.Grants.Rows[0].TurnsText = "4";
        skill.Grants.Rows[0].ChanceText = "35";

        Assert.Equal([new StatusGrant(SkillFixtures.Poison1, 4) { Chance = 35 }], skill.ToSkill().Effect.Statuses);
    }

    [Fact]
    public void Problem_GrantRowNotANumber_IsTheRowProblem()
    {
        var skill = SkillViewModel.From(SkillFixtures.Bolt, SkillFixtures.PoisonOnlyChoices);

        skill.Grants.Rows[0].TurnsText = "long";

        Assert.Equal("Statuses row 1: Turns must be a whole number (was 'long').", skill.Problem);
    }

    // Copied from the boxes: reading the half-typed chance would throw.
    [Fact]
    public void DuplicateOf_ChanceHalfTyped_CopiesTheTypedGrants()
    {
        var chosen = SkillViewModel.From(SkillFixtures.Bolt, SkillFixtures.PoisonOnlyChoices);
        chosen.Grants.Rows[0].ChanceText = "5x";

        var copy = SkillViewModel.DuplicateOf(chosen, ["bolt"]);

        Assert.Equal(("poison_1", "5x"), (copy.Grants.Rows[0].StatusId, copy.Grants.Rows[0].ChanceText));
        Assert.NotSame(chosen.Grants, copy.Grants);
    }

    [Fact]
    public void DuplicateOf_EffectRetyped_CopiesTheTypedBoxes()
    {
        var chosen = SkillViewModel.From(SkillFixtures.Bolt, SkillFixtures.PoisonOnlyChoices);
        chosen.Effect.HitsText = "x";

        var copy = SkillViewModel.DuplicateOf(chosen, ["bolt"]);

        Assert.Equal("x", copy.Effect.HitsText);
        Assert.NotSame(chosen.Effect, copy.Effect);
    }

    [Fact]
    public void ToSkill_EmptyStrip_WritesNone()
    {
        var skill = SkillViewModel.From(SkillFixtures.Bolt, SkillFixtures.PoisonOnlyChoices);

        skill.EffectFile = " ";

        Assert.Null(skill.ToSkill().EffectFile);
    }

    [Fact]
    public void ToSkill_StripWithSpaces_TrimsIt()
    {
        var skill = SkillViewModel.From(SkillFixtures.Bolt, SkillFixtures.PoisonOnlyChoices);

        skill.EffectFile = " Spark.png ";

        Assert.Equal("Spark.png", skill.ToSkill().EffectFile);
    }

    [Fact]
    public void ToSkill_NewSkillWithAFreeId_TakesItTrimmed()
    {
        var skill = SkillViewModel.ForNew(SkillFixtures.Bolt, ["bolt"], SkillFixtures.PoisonOnlyChoices);

        skill.IdText = " spark ";

        Assert.Equal("spark", skill.ToSkill().Id);
    }

    [Fact]
    public void ToSkill_Problem_Throws()
    {
        var skill = SkillViewModel.ForNew(SkillFixtures.Bolt, ["bolt"], SkillFixtures.PoisonOnlyChoices);

        Assert.Throws<InvalidOperationException>(skill.ToSkill);
    }

    [Fact]
    public void Problem_NewSkillWithATakenId_NamesIt()
    {
        var skill = SkillViewModel.ForNew(SkillFixtures.Bolt, ["bolt"], SkillFixtures.PoisonOnlyChoices);

        skill.IdText = "bolt";

        Assert.Equal("The id bolt is taken.", skill.Problem);
    }

    [Fact]
    public void Changed_TypedName_Raised()
    {
        var skill = SkillViewModel.From(SkillFixtures.Bolt, SkillFixtures.PoisonOnlyChoices);
        var raised = 0;
        skill.Changed += (_, _) => raised++;

        skill.Name = "Thunder";

        Assert.Equal(1, raised);
    }

    // Typing the id also raises its Id and Problem notices; only the typed field tells the list.
    [Fact]
    public void Changed_ProblemNotice_NotRaised()
    {
        var skill = SkillViewModel.ForNew(SkillFixtures.Bolt, ["bolt"], SkillFixtures.PoisonOnlyChoices);
        var raised = 0;
        skill.Changed += (_, _) => raised++;

        skill.IdText = "spark";

        Assert.Equal(1, raised);
    }

    [Fact]
    public void Changed_CostTyped_Raised()
    {
        var skill = SkillViewModel.From(SkillFixtures.Bolt, SkillFixtures.PoisonOnlyChoices);
        var raised = 0;
        skill.Changed += (_, _) => raised++;

        skill.Costs.Changes[0].AmountText = "-3";

        Assert.Equal(1, raised);
    }

    [Fact]
    public void Changed_GrantTyped_Raised()
    {
        var skill = SkillViewModel.From(SkillFixtures.Bolt, SkillFixtures.PoisonOnlyChoices);
        var raised = 0;
        skill.Changed += (_, _) => raised++;

        skill.Grants.Rows[0].ChanceText = "80";

        Assert.Equal(1, raised);
    }

    [Fact]
    public void Changed_EffectTyped_Raised()
    {
        var skill = SkillViewModel.From(SkillFixtures.Bolt, SkillFixtures.PoisonOnlyChoices);
        var raised = 0;
        skill.Changed += (_, _) => raised++;

        skill.Effect.HitsText = "2";

        Assert.Equal(1, raised);
    }
}
