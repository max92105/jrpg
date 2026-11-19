using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Data.Content;
using Jrpg.Editor.Battle;

namespace Jrpg.Editor.Tests.Battle;

// All of one enemy's rules as the Tactics tab edits them (decision 849): the list from the start then each phase's,
// phases added after the last at a threshold of their own (decision 859) and removed, each phase's list named by its
// place, the skills the rules name, and the tactics and phases built back.
public sealed class EnemyTacticsViewModelTests
{
    private static readonly ContentCatalog _catalog =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    // The test world's goblin with rules of the test's own: two phases, a skill named in two lists.
    private static readonly EnemyDefinition _boss = _catalog.Enemies["gnoll"] with
    {
        Tactics = [new EnemyAction { Weight = 4 }, new EnemyAction { SkillId = "cleave", Weight = 2 }],
        Phases =
        [
            new EnemyPhase { BelowHpPercent = 40, Tactics = [new EnemyAction { SkillId = "field_dressing", Weight = 3 }] },
            new EnemyPhase { BelowHpPercent = 15, Tactics = [new EnemyAction { SkillId = "cleave", Weight = 1 }] },
        ],
    };

    // A phase's rules are a list, which the record compares by reference: compare them entry by entry.
    private static readonly IEqualityComparer<EnemyPhase> _phaseComparer = EqualityComparer<EnemyPhase>.Create(
        (left, right) => left!.BelowHpPercent == right!.BelowHpPercent && left.Tactics.SequenceEqual(right.Tactics),
        phase => phase.BelowHpPercent);

    [Fact]
    public void From_EnemyWithTwoPhases_ListsStartThenPhasesInOrder()
    {
        var tactics = EnemyTacticsViewModel.From(_boss);

        Assert.Equal(["From the start", "Below 40 % HP", "Below 15 % HP"], tactics.Lists.Select(list => list.Label));
        Assert.Equal(["tactics", "phases[0].tactics", "phases[1].tactics"], tactics.Lists.Select(list => list.Field));
        Assert.Same(tactics.Lists[0], tactics.SelectedList);
        Assert.Null(tactics.Problem);
    }

    [Fact]
    public void AddPhase_AfterAPhaseAt40_StartsAt20AndIsChosen()
    {
        var tactics = EnemyTacticsViewModel.From(_boss with { Phases = [_boss.Phases[0]] });

        tactics.AddPhaseCommand.Execute(null);

        Assert.Equal(("20", "phases[1].tactics"), (tactics.Lists[2].ThresholdText, tactics.Lists[2].Field));
        Assert.Same(tactics.Lists[2], tactics.SelectedList);
    }

    [Fact]
    public void AddPhase_NoPhaseYet_StartsAt50()
    {
        var tactics = EnemyTacticsViewModel.From(_boss with { Phases = [] });

        tactics.AddPhaseCommand.Execute(null);

        Assert.Equal(("50", "phases[0].tactics"), (tactics.Lists[1].ThresholdText, tactics.Lists[1].Field));
    }

    // Half rounded down, never under 1; a last threshold that isn't a number gives the first phase's 50.
    [Theory]
    [InlineData("3", "1")]
    [InlineData("2", "1")]
    [InlineData("1", "1")]
    [InlineData("x", "50")]
    public void AddPhase_LastThresholdTyped_StartsAtHalfOfItAtLeast1(string lastThreshold, string expected)
    {
        var tactics = EnemyTacticsViewModel.From(_boss);
        tactics.Lists[2].ThresholdText = lastThreshold;

        tactics.AddPhaseCommand.Execute(null);

        Assert.Equal(expected, tactics.Lists[3].ThresholdText);
    }

    [Fact]
    public void RemovePhase_FirstOfTwo_RenamesTheOthersField()
    {
        var tactics = EnemyTacticsViewModel.From(_boss);
        var second = tactics.Lists[2];
        tactics.SelectedList = tactics.Lists[1];

        tactics.RemovePhaseCommand.Execute(null);

        Assert.Equal([tactics.Lists[0], second], tactics.Lists);
        Assert.Equal("phases[0].tactics", second.Field);
        Assert.Same(tactics.Lists[0], tactics.SelectedList);
    }

    [Fact]
    public void RemovePhase_LastOfTwo_ChoosesThePhaseBefore()
    {
        var tactics = EnemyTacticsViewModel.From(_boss);
        var first = tactics.Lists[1];
        tactics.SelectedList = tactics.Lists[2];

        tactics.RemovePhaseCommand.Execute(null);

        Assert.Same(first, tactics.SelectedList);
        Assert.Equal(2, tactics.Lists.Count);
    }

    [Fact]
    public void RemovePhaseCommand_StartListChosen_CannotExecute()
    {
        var tactics = EnemyTacticsViewModel.From(_boss);
        tactics.SelectedList = tactics.Lists[1];

        tactics.SelectedList = tactics.Lists[0];

        Assert.False(tactics.RemovePhaseCommand.CanExecute(null));
        Assert.True(tactics.AddPhaseCommand.CanExecute(null));
    }

    [Fact]
    public void RemovePhaseCommand_NoListChosen_CannotExecute()
    {
        var tactics = EnemyTacticsViewModel.From(_boss);

        tactics.SelectedList = null;

        Assert.False(tactics.RemovePhaseCommand.CanExecute(null));
    }

    [Fact]
    public void RemovePhaseCommand_PhaseChosen_CanExecuteAndIsRaised()
    {
        var tactics = EnemyTacticsViewModel.From(_boss);
        var raised = 0;
        tactics.RemovePhaseCommand.CanExecuteChanged += (_, _) => raised++;

        tactics.SelectedList = tactics.Lists[1];

        Assert.True(tactics.RemovePhaseCommand.CanExecute(null));
        Assert.Equal(1, raised);
    }

    [Fact]
    public void SkillsUsedLine_SkillInTwoLists_NamedOnce()
    {
        var tactics = EnemyTacticsViewModel.From(_boss);

        Assert.Equal("Uses: cleave, field_dressing", tactics.SkillsUsedLine);
    }

    [Fact]
    public void SkillsUsedLine_OnlyAttacks_SaysSo()
    {
        var tactics = EnemyTacticsViewModel.From(_boss with { Tactics = [new EnemyAction { Weight = 1 }], Phases = [] });

        Assert.Equal("Uses: only the attack", tactics.SkillsUsedLine);
    }

    [Fact]
    public void Problem_ProblemsInTwoPhases_NamesTheFirst()
    {
        var tactics = EnemyTacticsViewModel.From(_boss);

        tactics.Lists[2].Rules[0].WeightText = "y";
        tactics.Lists[1].Rules[0].WeightText = "x";

        Assert.Equal("phases[0].tactics[0]: Weight must be a whole number (was 'x').", tactics.Problem);
    }

    [Fact]
    public void Problem_OnlyTheLastPhase_NamesItsField()
    {
        var tactics = EnemyTacticsViewModel.From(_boss);

        tactics.Lists[2].Rules[0].WeightText = "y";

        Assert.Equal("phases[1].tactics[0]: Weight must be a whole number (was 'y').", tactics.Problem);
    }

    [Fact]
    public void ToTactics_WeightTyped_BuildsTheStartList()
    {
        var tactics = EnemyTacticsViewModel.From(_boss);

        tactics.Lists[0].Rules[1].WeightText = "6";

        Assert.Equal([_boss.Tactics[0], _boss.Tactics[1] with { Weight = 6 }], tactics.ToTactics());
    }

    [Fact]
    public void ToPhases_Typed_BuildsEveryPhase()
    {
        var tactics = EnemyTacticsViewModel.From(_boss);

        tactics.Lists[1].ThresholdText = "45";
        tactics.Lists[2].Rules[0].WeightText = "7";

        EnemyPhase[] expected =
        [
            _boss.Phases[0] with { BelowHpPercent = 45 },
            _boss.Phases[1] with { Tactics = [_boss.Phases[1].Tactics[0] with { Weight = 7 }] },
        ];
        Assert.Equal(expected, tactics.ToPhases(), _phaseComparer);
    }

    [Fact]
    public void CopyOf_ProblemTyped_CopiesEveryListAsTyped()
    {
        var tactics = EnemyTacticsViewModel.From(_boss);
        tactics.Lists[2].ThresholdText = "x";

        var copy = EnemyTacticsViewModel.CopyOf(tactics);

        Assert.NotSame(tactics.Lists[2], copy.Lists[2]);
        Assert.Equal(tactics.Lists.Select(list => list.Label), copy.Lists.Select(list => list.Label));
        Assert.Equal(tactics.Lists.Select(list => list.Field), copy.Lists.Select(list => list.Field));
        Assert.Equal(tactics.Problem, copy.Problem);
        Assert.Same(copy.Lists[0], copy.SelectedList);
    }
}
