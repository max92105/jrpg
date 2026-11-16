using Jrpg.Core.Battle;
using Jrpg.Editor.Battle;

namespace Jrpg.Editor.Tests.Battle;

// One rule as the Tactics tab types it (decision 849): its boxes read from an action and built back, the problem of a
// box that isn't a number, and the numbered sentence the list shows, its skill by id (decision 853).
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
    public void CopyOf_ProblemTyped_CopiesTheBoxesAsTyped()
    {
        var row = TacticRuleRow.From(_healOnThatOne);
        row.WeightText = "x";

        var copy = TacticRuleRow.CopyOf(row);

        Assert.NotSame(row, copy);
        Assert.Equal(
            (row.When, row.NumberText, row.SkillId, row.On, "x"),
            (copy.When, copy.NumberText, copy.SkillId, copy.On, copy.WeightText));
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

    [Fact]
    public void Sentence_HealOnThatOne_ReadsWhenUseOnAndWeight()
    {
        var row = TacticRuleRow.From(_healOnThatOne);

        row.Index = 2;

        Assert.Equal("2. When an ally's HP is below 50 %: field_dressing on that one, weight 3", row.Sentence);
    }

    [Fact]
    public void Sentence_AlwaysAttack_ReadsAlwaysAndAttack()
    {
        var row = TacticRuleRow.New();

        row.WeightText = "4";

        Assert.Equal("0. Always: attack on automatic, weight 4", row.Sentence);
    }

    [Fact]
    public void Sentence_KindWithoutNumber_HasNoNumber()
    {
        var row = TacticRuleRow.From(_healOnThatOne);

        row.When = TacticConditionChoice.Of(TacticConditionKind.AllyFallen);

        Assert.Equal("0. When an ally has fallen: field_dressing on that one, weight 3", row.Sentence);
    }

    [Fact]
    public void Sentence_NumberBoxEmpty_ReadsAQuestionMark()
    {
        var row = TacticRuleRow.New();

        row.When = TacticConditionChoice.Of(TacticConditionKind.OwnHpBelow);

        Assert.Equal("0. When its HP is below ? %: attack on automatic, weight 1", row.Sentence);
    }

    [Fact]
    public void Sentence_SkillChanged_IsRaised()
    {
        var row = TacticRuleRow.New();
        var notified = NoticesOf(row);

        row.SkillId = "cleave";

        Assert.Contains(nameof(TacticRuleRow.Sentence), notified);
    }

    [Fact]
    public void Sentence_TargetChanged_IsRaised()
    {
        var row = TacticRuleRow.New();
        var notified = NoticesOf(row);

        row.On = TacticTargetChoice.Of(TacticTarget.Self);

        Assert.Contains(nameof(TacticRuleRow.Sentence), notified);
    }

    [Fact]
    public void Sentence_IndexChanged_IsRaised()
    {
        var row = TacticRuleRow.New();
        var notified = NoticesOf(row);

        row.Index = 3;

        Assert.Contains(nameof(TacticRuleRow.Sentence), notified);
    }

    [Theory]
    [InlineData(nameof(TacticRuleRow.NumberText))]
    [InlineData(nameof(TacticRuleRow.WeightText))]
    public void Problem_NumberBoxTyped_IsRaisedWithTheSentence(string box)
    {
        var row = TacticRuleRow.New();
        var notified = NoticesOf(row);

        typeof(TacticRuleRow).GetProperty(box)!.SetValue(row, "7");

        Assert.Superset(
            new HashSet<string?> { nameof(TacticRuleRow.Problem), nameof(TacticRuleRow.Sentence) },
            notified.ToHashSet());
    }

    [Fact]
    public void When_Changed_RaisesTheNumberBoxAndTheLines()
    {
        var row = TacticRuleRow.New();
        var notified = NoticesOf(row);

        row.When = TacticConditionChoice.Of(TacticConditionKind.OwnHpBelow);

        Assert.Superset(
            new HashSet<string?>
            {
                nameof(TacticRuleRow.IsNumberShown),
                nameof(TacticRuleRow.NumberUnit),
                nameof(TacticRuleRow.Sentence),
                nameof(TacticRuleRow.Problem),
            },
            notified.ToHashSet());
    }

    private static List<string?> NoticesOf(TacticRuleRow row)
    {
        var notified = new List<string?>();
        row.PropertyChanged += (_, e) => notified.Add(e.PropertyName);
        return notified;
    }
}
