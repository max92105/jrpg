using Jrpg.Core.Battle;
using Jrpg.Editor.Battle;

namespace Jrpg.Editor.Tests.Battle;

// The numbered sentence the list shows for one rule (decision 849), its skill by id (decision 853), and the notices
// that redraw it and the problem line as the boxes change.
public sealed class TacticRuleRowSentenceTests
{
    private static readonly EnemyAction _healOnThatOne = new()
    {
        SkillId = "field_dressing",
        Weight = 3,
        When = new TacticCondition(TacticConditionKind.AllyHpBelow, 50),
        On = TacticTarget.Subject,
    };

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
    public void Sentence_FoeHpBelow_ReadsTheShare()
    {
        var row = TacticRuleRow.New();

        row.When = TacticConditionChoice.Of(TacticConditionKind.FoeHpBelow);
        row.NumberText = "50";

        Assert.Equal("0. When a foe's HP is below 50 %: attack on automatic, weight 1", row.Sentence);
    }

    // No ordinal: every 3 turns are its turns 3, 6, 9 (decision 847).
    [Fact]
    public void Sentence_EveryNthTurn_ReadsEveryNTurns()
    {
        var row = TacticRuleRow.New();

        row.When = TacticConditionChoice.Of(TacticConditionKind.EveryNthTurn);
        row.NumberText = "3";
        row.SkillId = "cleave";

        Assert.Equal("0. Every 3 turns of its own: cleave on automatic, weight 1", row.Sentence);
    }

    [Fact]
    public void Sentence_NumberBoxEmpty_ReadsAQuestionMark()
    {
        var row = TacticRuleRow.New();

        row.When = TacticConditionChoice.Of(TacticConditionKind.OwnHpBelow);

        Assert.Equal("0. When its HP is below ? %: attack on automatic, weight 1", row.Sentence);
    }

    // Read from the action, so the sentence also shows From copied it.
    [Fact]
    public void Sentence_Priority_EndsWithPriority()
    {
        var row = TacticRuleRow.From(new EnemyAction { Weight = 2, IsPriority = true });

        Assert.Equal("0. Always: attack on automatic, weight 2, priority", row.Sentence);
    }

    // Read from the action, so the sentence also shows From copied it; after priority, as the boxes are ordered.
    [Fact]
    public void Sentence_Once_EndsWithOncePerBattle()
    {
        var once = new EnemyAction { SkillId = "cleave", Weight = 2, IsPriority = true, IsOncePerBattle = true };

        var row = TacticRuleRow.From(once);

        Assert.Equal("0. Always: cleave on automatic, weight 2, priority, once per battle", row.Sentence);
    }

    [Fact]
    public void Sentence_PriorityChecked_IsRaised()
    {
        var row = TacticRuleRow.New();
        var notified = NoticesOf(row);

        row.IsPriority = true;

        Assert.Contains(nameof(TacticRuleRow.Sentence), notified);
    }

    [Fact]
    public void Sentence_OnceChecked_IsRaised()
    {
        var row = TacticRuleRow.New();
        var notified = NoticesOf(row);

        row.IsOncePerBattle = true;

        Assert.Contains(nameof(TacticRuleRow.Sentence), notified);
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
