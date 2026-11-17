using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Randomness;
using Jrpg.Core.Skills;
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
    private static readonly StatusDefinition _burning =
        TestStatuses.Record("burning_1", "Burning", new StatusPart(StatusPartKind.DamageEachTurn, 5)) with { IsHarmful = true };

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

    // The rim and the letter come from the record (decision 810), whatever its name.
    [Fact]
    public void Refresh_HarmfulRecord_MarkIsHarmful()
    {
        var (marks, hex) = MarksOfGoblinCHexedWith(_burning);
        hex();

        marks.Refresh();

        Assert.True(Assert.Single(marks.Marks).IsHarmful);
    }

    [Fact]
    public void Refresh_HelpfulRecord_MarkIsHelpful()
    {
        var ward = TestStatuses.Record("ward_1", "Ward", new StatusPart(StatusPartKind.ResistanceChange, 10));
        var (marks, hex) = MarksOfGoblinCHexedWith(ward);
        hex();

        marks.Refresh();

        Assert.False(Assert.Single(marks.Marks).IsHarmful);
    }

    [Fact]
    public void Refresh_NewStatus_LetterIsTheNamesFirst()
    {
        var (marks, hex) = MarksOfGoblinCHexedWith(_burning);
        hex();

        marks.Refresh();

        Assert.Equal("B", Assert.Single(marks.Marks).Letter.Text);
    }

    // Decision 810: a record's mark letter wins over its name's first, so Slowed doesn't show Stun's "S".
    [Fact]
    public void Refresh_StatusWithMarkLetter_ShowsIt()
    {
        var record = TestStatuses.Record("slowed_1", "Slowed", new StatusPart(StatusPartKind.DamageTakenChange, 10));
        var slowed = record with { MarkLetter = "L" };
        var (marks, hex) = MarksOfGoblinCHexedWith(slowed);
        hex();

        marks.Refresh();

        Assert.Equal("L", Assert.Single(marks.Marks).Letter.Text);
    }

    // Decision 800: a holder keeps one status per family, so the screen shows one mark for it.
    [Fact]
    public void Refresh_TwoRecordsOfOneFamily_ShowsOneMark()
    {
        var weak = TestStatuses.Poison with { Family = "poison" };
        var strong = weak with { Id = "poison_2", Parts = [new StatusPart(StatusPartKind.DamageEachTurn, 12)] };
        var (marks, hex) = MarksOfGoblinCHexedWith(weak, strong);
        hex();

        marks.Refresh();

        Assert.Equal("poison", Assert.Single(marks.Marks).FamilyKey);
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

    // Ragnir knows a hex granting the statuses for 2 turns; the returned action casts it on goblin C.
    private static (StatusMarksView Marks, Action Hex) MarksOfGoblinCHexedWith(params StatusDefinition[] statuses)
    {
        var hex = new SkillDefinition
        {
            Id = "hex",
            Name = "Hex",
            Description = "",
            IconFile = "",
            Effect = new Effect(EffectKind.GrantStatus, 0, Targeting.SingleEnemy)
            {
                Statuses = [.. statuses.Select(status => new StatusGrant(status, Turns: 2))],
            },
        };
        var catalog = BattleScenarios.Catalog;
        var definition = catalog.PartyMembers["ragnir"];
        var member = PartyMember.Recruit(
            definition,
            catalog.ClassOf(definition) with { Skills = [new(hex.Id)] },
            new Dictionary<string, SkillDefinition> { [hex.Id] = hex },
            BattleScenarios.CertainRules);
        var battle = BattleState.Start(
            new PartyState([member], [member], catalog.Rules.Party),
            [BattleScenarios.Goblin, BattleScenarios.Goblin, BattleScenarios.Goblin],
            new Inventory(),
            new SeededRandom(BattleScenarios.SarxianFirstSeed));
        var goblin = battle.Combatants[3];
        return (
            new StatusMarksView(goblin.Statuses, RepositoryScreenText.Text, new UiTextures()),
            () => battle.Execute(new SkillCommand(hex, [goblin])));
    }

    private Combatant GoblinC => _battle.Combatants[3];

    private StatusMarksView MarksOfGoblinC() => new(GoblinC.Statuses, RepositoryScreenText.Text, new UiTextures());

    private void BashGoblinC() =>
        _battle.Execute(new SkillCommand(BattleScenarios.Skills["shield_bash"], [GoblinC]));
}
