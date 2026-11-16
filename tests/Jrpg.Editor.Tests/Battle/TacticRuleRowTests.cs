using Jrpg.Core.Battle;
using Jrpg.Editor.Battle;

namespace Jrpg.Editor.Tests.Battle;

// One rule as the Tactics tab types it (decision 849): its boxes read from an action and built back, and the problem of
// a box that isn't a number; its sentence is TacticRuleRowSentenceTests'.
public sealed class TacticRuleRowTests
{
    private static readonly EnemyAction _healOnThatOne = new()
    {
        SkillId = "field_dressing",
        Weight = 3,
        When = new TacticCondition(TacticConditionKind.AllyHpBelow, 50),
        On = TacticTarget.Subject,
    };

    [Fact]
    public void From_ActionWithACondition_ReadsEveryBox()
    {
        var row = TacticRuleRow.From(_healOnThatOne);

        Assert.Same(TacticConditionChoice.All[2], row.When);
        Assert.Same(TacticTargetChoice.All[1], row.On);
        Assert.Equal(("50", "field_dressing", "3"), (row.NumberText, row.SkillId, row.WeightText));
    }

    [Fact]
    public void From_KindWithoutNumber_LeavesTheNumberEmpty()
    {
        var row = TacticRuleRow.From(new EnemyAction { Weight = 1, When = new TacticCondition(TacticConditionKind.AllyFallen) });

        Assert.Equal("", row.NumberText);
    }

    [Fact]
    public void ToAction_TypedBoxes_BuildsTheAction()
    {
        var row = TacticRuleRow.New();

        row.When = TacticConditionChoice.Of(TacticConditionKind.OwnHpBelow);
        row.NumberText = "30";
        row.SkillId = "cleave";
        row.On = TacticTargetChoice.Of(TacticTarget.LowestHpFoe);
        row.WeightText = "5";

        var expected = new EnemyAction
        {
            SkillId = "cleave",
            Weight = 5,
            When = new TacticCondition(TacticConditionKind.OwnHpBelow, 30),
            On = TacticTarget.LowestHpFoe,
        };
        Assert.Equal(expected, row.ToAction());
    }

    [Fact]
    public void ToAction_AttackRow_HasNoSkillId()
    {
        var row = TacticRuleRow.From(new EnemyAction { SkillId = "cleave", Weight = 2 });

        row.SkillId = "";

        Assert.Equal(new EnemyAction { Weight = 2 }, row.ToAction());
    }

    // The number box still holds 40 under the hidden box: only the kind decides what is written.
    [Fact]
    public void ToAction_KindWithoutNumber_LeavesThePercentAtZero()
    {
        var row = TacticRuleRow.New();

        row.When = TacticConditionChoice.Of(TacticConditionKind.AllyFallen);
        row.NumberText = "40";

        Assert.Equal(new TacticCondition(TacticConditionKind.AllyFallen), row.ToAction().When);
    }

    [Fact]
    public void ToAction_PriorityChecked_IsPriority()
    {
        var row = TacticRuleRow.New();

        row.IsPriority = true;

        Assert.True(row.ToAction().IsPriority);
    }

    [Fact]
    public void ToAction_OnceChecked_IsOncePerBattle()
    {
        var row = TacticRuleRow.New();

        row.IsOncePerBattle = true;

        Assert.True(row.ToAction().IsOncePerBattle);
    }

    [Fact]
    public void CopyOf_ProblemTyped_CopiesTheBoxesAsTyped()
    {
        var row = TacticRuleRow.From(_healOnThatOne);
        row.WeightText = "x";
        row.IsPriority = true;
        row.IsOncePerBattle = true;

        var copy = TacticRuleRow.CopyOf(row);

        Assert.NotSame(row, copy);
        Assert.Equal(
            (row.When, row.NumberText, row.SkillId, row.On, "x", true, true),
            (copy.When, copy.NumberText, copy.SkillId, copy.On, copy.WeightText, copy.IsPriority, copy.IsOncePerBattle));
    }

    [Fact]
    public void Problem_WeightNotANumber_NamesWeight()
    {
        var row = TacticRuleRow.New();

        row.WeightText = "x";

        Assert.Equal("Weight must be a whole number (was 'x').", row.Problem);
    }

    [Fact]
    public void Problem_PercentNotANumberWhileShown_NamesPercent()
    {
        var row = TacticRuleRow.New();

        row.When = TacticConditionChoice.Of(TacticConditionKind.OwnHpBelow);
        row.NumberText = "x";

        Assert.Equal("Percent must be a whole number (was 'x').", row.Problem);
    }

    // A kind without a number hides the box, so what it holds can't stop the rule.
    [Fact]
    public void Problem_PercentNotANumberWhileHidden_IsNull()
    {
        var row = TacticRuleRow.New();
        row.NumberText = "x";

        row.When = TacticConditionChoice.Of(TacticConditionKind.AllyFallen);

        Assert.Null(row.Problem);
    }

    [Fact]
    public void NumberUnit_KindWithoutNumber_IsEmpty()
    {
        var row = TacticRuleRow.From(_healOnThatOne);

        row.When = TacticConditionChoice.Of(TacticConditionKind.AllyFallen);

        Assert.Equal(("", false), (row.NumberUnit, row.IsNumberShown));
    }
}
