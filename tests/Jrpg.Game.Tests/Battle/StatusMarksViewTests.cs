using Jrpg.Core.Battle;
using Jrpg.Core.Items;
using Jrpg.Core.Randomness;
using Jrpg.Core.Statuses;
using Jrpg.Game.Battle;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Battle;

// Ragnir, a fighter, acts first against goblins A, B and C. Shield Bash on C stuns it for 1 turn, and it stays stunned
// while A and B act before it.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class StatusMarksViewTests
{
    private readonly BattleState _battle =
        BattleScenarios.MemberAgainstGoblinTrio("ragnir", new Inventory(), new SeededRandom(BattleScenarios.SarxianFirstSeed));

    [Fact]
    public void PopScale_JustApplied_StartsAt1Point3()
    {
        Assert.Equal(1.3f, StatusMarksView.PopScale(TimeSpan.Zero), 3);
    }

    [Fact]
    public void PopScale_AfterPopTime_Is1()
    {
        Assert.Equal(1f, StatusMarksView.PopScale(TimeSpan.FromSeconds(0.2)), 3);
    }

    [Fact]
    public void FadeOpacity_Halfway_IsHalf()
    {
        Assert.Equal(0.5f, StatusMarksView.FadeOpacity(TimeSpan.FromSeconds(0.1)), 3);
    }

    [Theory]
    [InlineData(StatusKind.Stun, true)]
    [InlineData(StatusKind.Poison, true)]
    [InlineData(StatusKind.Exposed, true)]
    [InlineData(StatusKind.Guarded, false)]
    [InlineData(StatusKind.Regen, false)]
    [InlineData(StatusKind.Taunt, false)]
    public void IsHarmful_Kind_MatchesHarmfulRule(StatusKind kind, bool expected)
    {
        Assert.Equal(expected, StatusMarksView.IsHarmful(kind));
    }

    [Fact]
    public void Refresh_StunWithOneTurn_ShowsLetterAndTurns()
    {
        var marks = MarksOfGoblinC();
        BashGoblinC();

        marks.Refresh();

        var mark = Assert.Single(marks.Marks);
        Assert.Equal(("S", "1"), (mark.Letter.Text, mark.Turns.Text));
    }

    [Fact]
    public void Refresh_StatusEnded_FadesThenGoes()
    {
        var marks = MarksOfGoblinC();
        BashGoblinC();
        marks.Refresh();
        _battle.Execute(new DefendCommand());
        _battle.Execute(new DefendCommand());
        marks.Refresh();
        var fading = marks.Marks[0].SinceEnded;

        marks.Update(TimeSpan.FromSeconds(0.2));

        Assert.Equal(TimeSpan.Zero, fading);
        Assert.Empty(marks.Marks);
    }

    [Fact]
    public void Refresh_Unchanged_KeepsSameTurnsString()
    {
        var marks = MarksOfGoblinC();
        BashGoblinC();
        marks.Refresh();
        var turns = marks.Marks[0].Turns.Text;

        marks.Refresh();

        Assert.Same(turns, marks.Marks[0].Turns.Text);
    }

    private Combatant GoblinC => _battle.Combatants[3];

    private StatusMarksView MarksOfGoblinC() => new(GoblinC.Statuses, RepositoryScreenText.Text, new UiTextures());

    private void BashGoblinC() =>
        _battle.Execute(new SkillCommand(BattleScenarios.Catalog.Skills["shield_bash"], [GoblinC]));
}
