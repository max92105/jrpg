using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Data.Content;
using Jrpg.Editor.Battle;

namespace Jrpg.Editor.Tests.Battle;

// What tells an enemy's owner to check its rules again (decision 849): a list's rules or threshold typed, a phase added
// or removed, each once, with the "Uses:" line; never a choice of list or rule, nor a list the enemy no longer holds.
public sealed class EnemyTacticsChangedTests
{
    private static readonly ContentCatalog _catalog =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    // The test world's goblin with rules of the test's own: the attack from the start, then two phases.
    private static readonly EnemyDefinition _boss = _catalog.Enemies["goblin"] with
    {
        Tactics = [new EnemyAction { Weight = 4 }],
        Phases =
        [
            new EnemyPhase { BelowHpPercent = 40, Tactics = [new EnemyAction { SkillId = "cleave", Weight = 3 }] },
            new EnemyPhase { BelowHpPercent = 15, Tactics = [new EnemyAction { Weight = 1 }] },
        ],
    };

    [Fact]
    public void Changed_RuleAddedInAPhase_IsRaised()
    {
        var (tactics, raised) = Watched();

        tactics.Lists[2].AddRuleCommand.Execute(null);

        Assert.Equal(1, raised());
    }

    [Fact]
    public void Changed_ThresholdTyped_IsRaised()
    {
        var (tactics, raised) = Watched();

        tactics.Lists[1].ThresholdText = "30";

        Assert.Equal(1, raised());
    }

    [Fact]
    public void Changed_PhaseAdded_IsRaised()
    {
        var (tactics, raised) = Watched();

        tactics.AddPhaseCommand.Execute(null);

        Assert.Equal(1, raised());
    }

    [Fact]
    public void Changed_PhaseRemoved_IsRaised()
    {
        var (tactics, raised) = Watched();
        tactics.SelectedList = tactics.Lists[1];

        tactics.RemovePhaseCommand.Execute(null);

        Assert.Equal(1, raised());
    }

    // A removed phase is no longer followed, so typing in a list the enemy no longer holds tells no one.
    [Fact]
    public void Changed_RemovedPhaseTyped_IsNotRaised()
    {
        var tactics = EnemyTacticsViewModel.From(_boss);
        var removed = tactics.Lists[1];
        tactics.SelectedList = removed;
        tactics.RemovePhaseCommand.Execute(null);
        var raised = 0;
        tactics.Changed += (_, _) => raised++;

        removed.Rules[0].WeightText = "6";

        Assert.Equal(0, raised);
    }

    [Fact]
    public void Changed_ListAndRuleChosen_IsNotRaised()
    {
        var (tactics, raised) = Watched();

        tactics.SelectedList = tactics.Lists[1];
        tactics.Lists[1].SelectedRule = null;

        Assert.Equal(0, raised());
    }

    // A copy follows its own lists: typing in it tells the copy's owner, not the original's.
    [Fact]
    public void Changed_RuleTypedInACopy_IsRaisedOnTheCopyOnly()
    {
        var (tactics, raised) = Watched();
        var copy = EnemyTacticsViewModel.CopyOf(tactics);
        var copyRaised = 0;
        copy.Changed += (_, _) => copyRaised++;

        copy.Lists[2].Rules[0].WeightText = "6";

        Assert.Equal((0, 1), (raised(), copyRaised));
    }

    [Fact]
    public void SkillsUsedLine_SkillPickedInAPhase_IsRaisedWithIt()
    {
        var tactics = EnemyTacticsViewModel.From(_boss);
        var notices = new List<string?>();
        tactics.PropertyChanged += (_, e) => notices.Add(e.PropertyName);

        tactics.Lists[2].Rules[0].SkillId = "field_dressing";

        Assert.Equal(["SkillsUsedLine"], notices);
        Assert.Equal("Uses: cleave, field_dressing", tactics.SkillsUsedLine);
    }

    private static (EnemyTacticsViewModel Tactics, Func<int> Raised) Watched()
    {
        var tactics = EnemyTacticsViewModel.From(_boss);
        var raised = 0;
        tactics.Changed += (_, _) => raised++;
        return (tactics, () => raised);
    }
}
