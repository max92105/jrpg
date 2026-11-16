using Jrpg.Core.Battle;
using Jrpg.Editor.Battle;

namespace Jrpg.Editor.Tests.Battle;

// What tells a list's owner to check again (decision 849): a rule's box typed or picked, the threshold typed, a rule
// added, removed or moved, each once; never a rule's own sentence, number or problem notice.
public sealed class TacticListRulesChangedTests
{
    private static readonly EnemyPhase _twoRulePhase = new()
    {
        BelowHpPercent = 40,
        Tactics = [new EnemyAction { Weight = 4 }, new EnemyAction { SkillId = "cleave", Weight = 2 }],
    };

    public static TheoryData<string> TypedBoxes => ["When", "NumberText", "SkillId", "On"];

    [Fact]
    public void RulesChanged_WeightTyped_IsRaised()
    {
        var (list, raised) = Watched();

        list.Rules[1].WeightText = "5";

        Assert.Equal(1, raised());
    }

    [Theory]
    [MemberData(nameof(TypedBoxes))]
    public void RulesChanged_BoxTyped_IsRaised(string box)
    {
        var (list, raised) = Watched();

        Type(list.Rules[1], box);

        Assert.Equal(1, raised());
    }

    [Fact]
    public void RulesChanged_ThresholdTyped_IsRaised()
    {
        var (list, raised) = Watched();

        list.ThresholdText = "30";

        Assert.Equal(1, raised());
    }

    [Fact]
    public void RulesChanged_RuleAdded_IsRaised()
    {
        var (list, raised) = Watched();

        list.AddRuleCommand.Execute(null);

        Assert.Equal(1, raised());
    }

    [Fact]
    public void RulesChanged_RuleRemoved_IsRaised()
    {
        var (list, raised) = Watched();

        list.RemoveRuleCommand.Execute(null);

        Assert.Equal(1, raised());
    }

    [Fact]
    public void RulesChanged_RuleMoved_IsRaised()
    {
        var (list, raised) = Watched();

        list.MoveRuleDownCommand.Execute(null);

        Assert.Equal(1, raised());
    }

    // A moved rule is listed as both taken out and put in: it stays watched once, so typing in it tells the owner once.
    [Fact]
    public void RulesChanged_MovedRuleTyped_IsRaisedOnce()
    {
        var list = TacticListViewModel.ForPhase(_twoRulePhase);
        list.MoveRuleDownCommand.Execute(null);
        var raised = 0;
        list.RulesChanged += (_, _) => raised++;

        list.Rules[1].WeightText = "6";

        Assert.Equal(1, raised);
    }

    // A removed rule is no longer watched, so typing in a rule the list no longer holds tells no one.
    [Fact]
    public void RulesChanged_RemovedRuleTyped_IsNotRaised()
    {
        var list = TacticListViewModel.ForPhase(_twoRulePhase);
        var removed = list.Rules[0];
        list.RemoveRuleCommand.Execute(null);
        var raised = 0;
        list.RulesChanged += (_, _) => raised++;

        removed.WeightText = "6";

        Assert.Equal(0, raised);
    }

    // Setting the number again raises the rule's own Index and Sentence notices, which the owner must not re-check on.
    [Fact]
    public void RulesChanged_SentenceNotice_IsNotRaised()
    {
        var (list, raised) = Watched();
        var notices = new List<string?>();
        list.Rules[1].PropertyChanged += (_, e) => notices.Add(e.PropertyName);

        list.Rules[1].Index = 7;

        Assert.Equal(["Index", "Sentence"], notices);
        Assert.Equal(0, raised());
    }

    // The owner names a phase's place; that is no edit of the rules.
    [Fact]
    public void RulesChanged_FieldSet_IsNotRaised()
    {
        var (list, raised) = Watched();

        list.Field = "phases[1].tactics";

        Assert.Equal(0, raised());
    }

    [Fact]
    public void ThresholdText_Typed_RaisesLabelAndProblem()
    {
        var list = TacticListViewModel.ForPhase(_twoRulePhase);
        var notices = new List<string?>();
        list.PropertyChanged += (_, e) => notices.Add(e.PropertyName);

        list.ThresholdText = "x";

        Assert.Equal(["ThresholdText", "Label", "Problem"], notices);
    }

    [Fact]
    public void Field_Set_RaisesProblem()
    {
        var list = TacticListViewModel.ForPhase(_twoRulePhase);
        var notices = new List<string?>();
        list.PropertyChanged += (_, e) => notices.Add(e.PropertyName);

        list.Field = "phases[1].tactics";

        Assert.Equal(["Field", "Problem"], notices);
    }

    private static (TacticListViewModel List, Func<int> Raised) Watched()
    {
        var list = TacticListViewModel.ForPhase(_twoRulePhase);
        var raised = 0;
        list.RulesChanged += (_, _) => raised++;
        return (list, () => raised);
    }

    private static void Type(TacticRuleRow row, string box)
    {
        switch (box)
        {
            case "When":
                row.When = TacticConditionChoice.Of(TacticConditionKind.OwnHpBelow);
                break;
            case "NumberText":
                row.NumberText = "30";
                break;
            case "SkillId":
                row.SkillId = "field_dressing";
                break;
            case "On":
                row.On = TacticTargetChoice.Of(TacticTarget.LowestHpFoe);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(box), box, "No such box.");
        }
    }
}
