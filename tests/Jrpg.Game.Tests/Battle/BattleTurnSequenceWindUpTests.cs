using Jrpg.Core.Battle;
using Jrpg.Core.Items;
using Jrpg.Core.Randomness;
using Jrpg.Game.Battle;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Battle;

// The wind-up between the command and Core's Execute (ADR 0013 amended, decision 644): Core executes at its end, the
// impact. Against the goblin trio, whose goblins never dodge, so Sarxian's attack lands.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class BattleTurnSequenceWindUpTests
{
    [Fact]
    public void Submit_Attack_WindsUpWithoutExecuting()
    {
        var (battle, sequence) = Start(BattleScenarios.SarxianFirstSeed);
        var goblin = battle.Combatants[1];
        var hpBefore = goblin.Vitals.Hp;

        sequence.Submit(new AttackCommand(goblin));

        Assert.Equal(TurnStage.WindingUp, sequence.Stage);
        Assert.Null(sequence.Playback);
        Assert.Equal(hpBefore, goblin.Vitals.Hp);
    }

    // At the game's fixed step: one tick short of the wind-up nothing has landed; the next tick lands it.
    [Fact]
    public void Update_WindUpEnds_ExecutesAtTheImpact()
    {
        var (battle, sequence) = Start(BattleScenarios.SarxianFirstSeed);
        var goblin = battle.Combatants[1];
        var hpBefore = goblin.Vitals.Hp;
        sequence.Submit(new AttackCommand(goblin));
        var tick = StartMapScenarios.OneUpdate;
        for (var elapsed = TimeSpan.Zero; elapsed + tick < BattlePacing.WindUp; elapsed += tick)
        {
            sequence.Update(StartMapScenarios.OneUpdate);
        }

        Assert.Equal((TurnStage.WindingUp, hpBefore), (sequence.Stage, goblin.Vitals.Hp));
        sequence.Update(StartMapScenarios.OneUpdate);
        Assert.Equal(TurnStage.Holding, sequence.Stage);
        Assert.True(goblin.Vitals.Hp < hpBefore);
    }

    [Fact]
    public void Submit_Defend_ExecutesAtOnce()
    {
        var (_, sequence) = Start(BattleScenarios.SarxianFirstSeed);

        sequence.Submit(new DefendCommand());

        Assert.NotNull(sequence.Playback);
        Assert.Equal(TurnStage.Holding, sequence.Stage);
    }

    [Fact]
    public void Update_EnemyPauseEnds_WindsUpItsChoice()
    {
        var random = new SeededRandom(BattleScenarios.GoblinsFirstSeed);
        var battle = BattleScenarios.SarxianAgainstGoblinTrio(random);
        var sequence = BattleScenarios.SequenceOver(battle, random);

        sequence.Update(BattlePacing.Step);
        var stageAfterThePause = sequence.Stage;
        sequence.Update(BattlePacing.WindUp);

        Assert.Equal((TurnStage.WindingUp, TurnStage.Holding), (stageAfterThePause, sequence.Stage));
    }

    [Fact]
    public void StepShare_PartyActorWindingUp_IsOne()
    {
        var (battle, sequence) = Start(BattleScenarios.SarxianFirstSeed);

        sequence.Submit(new AttackCommand(battle.Combatants[1]));
        sequence.Update(TimeSpan.FromSeconds(0.3));

        Assert.Equal(1f, sequence.StepShare(sequence.Actor));
    }

    // Lazarus readies: he stays forward, and his next command winds up again.
    [Fact]
    public void Update_QuickSkill_WindsUpTheNextCommandToo()
    {
        var random = new SeededRandom(BattleScenarios.SarxianFirstSeed);
        var battle = BattleScenarios.MemberAgainstGoblinTrio("lazarus", new Inventory(), random);
        var sequence = BattleScenarios.SequenceOver(battle, random);
        sequence.Update(BattlePacing.Step);
        sequence.Submit(new SkillCommand(BattleScenarios.Catalog.Skills["ready"], [battle.Combatants[1]]));
        sequence.Update(BattlePacing.WindUp);
        sequence.Update(sequence.Playback!.Duration);

        sequence.Submit(new AttackCommand(battle.Combatants[1]));

        Assert.Equal(TurnStage.WindingUp, sequence.Stage);
    }

    // Sarxian's turn, stepped forward and awaiting a command.
    private static (BattleState Battle, BattleTurnSequence Sequence) Start(int seed)
    {
        var random = new SeededRandom(seed);
        var battle = BattleScenarios.SarxianAgainstGoblinTrio(random);
        var sequence = BattleScenarios.SequenceOver(battle, random);
        sequence.Update(BattlePacing.Step);
        return (battle, sequence);
    }
}
