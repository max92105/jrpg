using Jrpg.Core.Battle;
using Jrpg.Editor.Battle;

namespace Jrpg.Editor.Tests.Battle;

// One list of an enemy's rules as the Tactics tab edits it (decision 849): read from the start's actions or a phase,
// numbered from 0 as Data numbers them, rules added after the chosen one, removed and moved, its problem named by
// Data's field, and the phase built back.
public sealed class TacticListViewModelTests
{
    private static readonly EnemyPhase _enragedPhase = new()
    {
        BelowHpPercent = 40,
        Tactics =
        [
            new EnemyAction { Weight = 4 },
            new EnemyAction { SkillId = "cleave", Weight = 2 },
            new EnemyAction
            {
                SkillId = "field_dressing",
                Weight = 3,
                When = new TacticCondition(TacticConditionKind.AllyHpBelow, 50),
                On = TacticTarget.Subject,
            },
        ],
    };

    // A phase's rules are a list, which the record compares by reference: compare them entry by entry.
    private static readonly IEqualityComparer<EnemyPhase> _phaseComparer = EqualityComparer<EnemyPhase>.Create(
        (left, right) => left!.BelowHpPercent == right!.BelowHpPercent && left.Tactics.SequenceEqual(right.Tactics),
        phase => phase.BelowHpPercent);

    [Fact]
    public void ForPhase_Phase_ReadsTheThresholdAndRules()
    {
        var list = TacticListViewModel.ForPhase(_enragedPhase);

        Assert.True(list.IsPhase);
        Assert.Equal(("40", "Below 40 % HP"), (list.ThresholdText, list.Label));
        Assert.Equal(["", "cleave", "field_dressing"], list.Rules.Select(row => row.SkillId));
        Assert.Equal([0, 1, 2], list.Rules.Select(row => row.Index));
        Assert.Same(list.Rules[0], list.SelectedRule);
        Assert.Null(list.Problem);
    }

    [Fact]
    public void Label_ThresholdEmptied_ShowsAQuestionMark()
    {
        var list = TacticListViewModel.ForPhase(_enragedPhase);

        list.ThresholdText = " ";

        Assert.Equal("Below ? % HP", list.Label);
    }

    [Fact]
    public void FromStart_Actions_IsNoPhase()
    {
        var list = TacticListViewModel.FromStart(_enragedPhase.Tactics);

        Assert.False(list.IsPhase);
        Assert.Equal(("", "From the start", "tactics"), (list.ThresholdText, list.Label, list.Field));
        Assert.Equal(_enragedPhase.Tactics, list.ToActions());
    }

    // Data refuses a phase without a rule, so a new one starts with the rule Add would give.
    [Fact]
    public void NewPhase_Threshold_HoldsOneNewRule()
    {
        var list = TacticListViewModel.NewPhase(25);

        var expected = new EnemyPhase { BelowHpPercent = 25, Tactics = [new EnemyAction { Weight = 1 }] };
        Assert.Equal(expected, list.ToPhase(), _phaseComparer);
    }

    [Fact]
    public void AddRule_WithAChosenRule_AddsAfterItAndChoosesIt()
    {
        var list = TacticListViewModel.ForPhase(_enragedPhase);
        list.SelectedRule = list.Rules[1];

        list.AddRuleCommand.Execute(null);

        Assert.Equal(["", "cleave", "", "field_dressing"], list.Rules.Select(row => row.SkillId));
        Assert.Same(list.Rules[2], list.SelectedRule);
        Assert.Equal([0, 1, 2, 3], list.Rules.Select(row => row.Index));
    }

    [Fact]
    public void AddRule_NoChosenRule_AddsLast()
    {
        var list = TacticListViewModel.ForPhase(_enragedPhase);
        list.SelectedRule = null;

        list.AddRuleCommand.Execute(null);

        Assert.Equal(3, list.Rules.IndexOf(list.SelectedRule!));
    }

    [Fact]
    public void MoveRuleDown_FirstOfThree_RenumbersBoth()
    {
        var list = TacticListViewModel.ForPhase(_enragedPhase);
        var (first, second) = (list.Rules[0], list.Rules[1]);

        list.MoveRuleDownCommand.Execute(null);

        Assert.Equal((1, 0), (first.Index, second.Index));
        Assert.Equal(["cleave", "", "field_dressing"], list.Rules.Select(row => row.SkillId));
    }

    [Fact]
    public void MoveRuleUp_LastOfThree_SwapsWithTheOneBefore()
    {
        var list = TacticListViewModel.ForPhase(_enragedPhase);
        list.SelectedRule = list.Rules[2];

        list.MoveRuleUpCommand.Execute(null);

        Assert.Equal(["", "field_dressing", "cleave"], list.Rules.Select(row => row.SkillId));
    }

    [Fact]
    public void RemoveRule_Chosen_ChoosesTheNext()
    {
        var list = TacticListViewModel.ForPhase(_enragedPhase);
        list.SelectedRule = list.Rules[1];

        list.RemoveRuleCommand.Execute(null);

        Assert.Equal(["", "field_dressing"], list.Rules.Select(row => row.SkillId));
        Assert.Same(list.Rules[1], list.SelectedRule);
        Assert.Equal(1, list.SelectedRule!.Index);
    }

    // Data refuses a phase without a rule; the page shows its message once the enemy is built (EB6).
    [Fact]
    public void RemoveRule_OnlyRuleOfAPhase_LeavesThePhaseEmpty()
    {
        var list = TacticListViewModel.NewPhase(25);

        list.RemoveRuleCommand.Execute(null);

        Assert.Empty(list.Rules);
        Assert.Null(list.SelectedRule);
        Assert.Empty(list.ToPhase().Tactics);
    }

    [Fact]
    public void RemoveRule_NoChosenRule_Greyed()
    {
        var list = TacticListViewModel.ForPhase(_enragedPhase);

        list.SelectedRule = null;

        Assert.False(list.RemoveRuleCommand.CanExecute(null));
        Assert.False(list.MoveRuleUpCommand.CanExecute(null));
        Assert.False(list.MoveRuleDownCommand.CanExecute(null));
        Assert.True(list.AddRuleCommand.CanExecute(null));
    }

    [Fact]
    public void SelectedRule_Chosen_RaisesRemoveAndMoveStates()
    {
        var list = TacticListViewModel.ForPhase(_enragedPhase);
        var raised = new List<string>();
        list.RemoveRuleCommand.CanExecuteChanged += (_, _) => raised.Add("remove");
        list.MoveRuleUpCommand.CanExecuteChanged += (_, _) => raised.Add("up");
        list.MoveRuleDownCommand.CanExecuteChanged += (_, _) => raised.Add("down");

        list.SelectedRule = list.Rules[2];

        Assert.Equal(["remove", "up", "down"], raised);
    }

    [Fact]
    public void Problem_RowInAPhase_NamesThePhaseField()
    {
        var list = TacticListViewModel.ForPhase(_enragedPhase);
        list.Field = "phases[1].tactics";

        list.Rules[2].WeightText = "x";

        Assert.Equal("phases[1].tactics[2]: Weight must be a whole number (was 'x').", list.Problem);
    }

    [Fact]
    public void Problem_ThresholdNotANumber_NamesThreshold()
    {
        var list = TacticListViewModel.ForPhase(_enragedPhase);

        list.ThresholdText = "x";

        Assert.Equal("Threshold must be a whole number (was 'x').", list.Problem);
    }

    [Fact]
    public void ToPhase_Typed_BuildsThePhase()
    {
        var list = TacticListViewModel.ForPhase(_enragedPhase);

        list.ThresholdText = "30";
        list.Rules[1].WeightText = "5";

        var expected = _enragedPhase with
        {
            BelowHpPercent = 30,
            Tactics = [_enragedPhase.Tactics[0], _enragedPhase.Tactics[1] with { Weight = 5 }, _enragedPhase.Tactics[2]],
        };
        Assert.Equal(expected, list.ToPhase(), _phaseComparer);
    }

    [Fact]
    public void ToPhase_StartList_Throws()
    {
        var list = TacticListViewModel.FromStart(_enragedPhase.Tactics);

        Assert.Throws<InvalidOperationException>(list.ToPhase);
    }

    [Fact]
    public void CopyOf_ProblemTyped_CopiesTheListAsTyped()
    {
        var list = TacticListViewModel.ForPhase(_enragedPhase);
        list.Field = "phases[2].tactics";
        list.ThresholdText = "x";

        var copy = TacticListViewModel.CopyOf(list);

        Assert.NotSame(list.Rules[0], copy.Rules[0]);
        Assert.Equal(("x", "phases[2].tactics", true), (copy.ThresholdText, copy.Field, copy.IsPhase));
        Assert.Equal(list.Rules.Select(row => row.Sentence), copy.Rules.Select(row => row.Sentence));
    }
}
