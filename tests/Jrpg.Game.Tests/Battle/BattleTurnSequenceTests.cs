using Jrpg.Core.Battle;
using Jrpg.Core.Items;
using Jrpg.Core.Randomness;
using Jrpg.Game.Battle;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Battle;

// A turn on screen: 0.2 s forward, then the command or the enemy's action, a 1.0 s hold (2.0 s with a fall), 0.2 s back.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class BattleTurnSequenceTests
{
    private static readonly TimeSpan _step = TimeSpan.FromSeconds(0.2);
    private static readonly TimeSpan _longEnoughForEveryEnemyTurn = TimeSpan.FromSeconds(10);

    [Fact]
    public void Update_EnemyActsFirst_RunsUntilPartyTurn()
    {
        var (_, sequence) = SarxianAgainstGoblinTrio(BattleScenarios.GoblinsFirstSeed);
        Assert.False(sequence.IsAwaitingCommand);

        sequence.Update(_longEnoughForEveryEnemyTurn);

        Assert.True(sequence.IsAwaitingCommand);
        Assert.Equal(BattleSide.Party, sequence.Actor.Side);
    }

    [Fact]
    public void Submit_Flee_IsOver()
    {
        var (_, sequence) = SarxianAgainstGoblinTrio(BattleScenarios.SarxianFirstSeed);
        sequence.Update(_step);

        sequence.Submit(new FleeCommand());
        sequence.Update(_step);

        Assert.True(sequence.IsOver);
    }

    [Fact]
    public void Submit_NotAwaiting_Throws()
    {
        var (_, sequence) = SarxianAgainstGoblinTrio(BattleScenarios.GoblinsFirstSeed);

        Assert.Throws<InvalidOperationException>(() => sequence.Submit(new FleeCommand()));
    }

    [Fact]
    public void Update_EnemyTurn_StepsForwardBeforeExecuting()
    {
        var (battle, sequence) = SarxianAgainstGoblinTrio(BattleScenarios.GoblinsFirstSeed);
        var sarxian = battle.LivingOn(BattleSide.Party)[0];
        var hpBefore = sarxian.Vitals.Hp;

        sequence.Update(TimeSpan.FromSeconds(0.1));

        Assert.Equal(TurnStage.StepForward, sequence.Stage);
        Assert.Equal(0.5f, sequence.StepShare(sequence.Actor), 3);
        Assert.Equal(hpBefore, sarxian.Vitals.Hp);
    }

    [Fact]
    public void Update_PartyTurn_AwaitsCommandOnceForward()
    {
        var (_, sequence) = SarxianAgainstGoblinTrio(BattleScenarios.SarxianFirstSeed);

        sequence.Update(_step);

        Assert.Equal(TurnStage.AwaitingCommand, sequence.Stage);
    }

    [Fact]
    public void Submit_HoldsThenStepsBack()
    {
        var (battle, sequence) = SarxianAgainstGoblinTrio(BattleScenarios.SarxianFirstSeed);
        sequence.Update(_step);

        sequence.Submit(new AttackCommand(battle.Combatants[1]));
        var stageAfterSubmit = sequence.Stage;
        sequence.Update(TimeSpan.FromSeconds(1.0));

        Assert.Equal(TurnStage.Holding, stageAfterSubmit);
        Assert.Equal(TurnStage.StepBack, sequence.Stage);
    }

    [Fact]
    public void Submit_Defend_StepsBackWithoutHolding()
    {
        var (_, sequence) = SarxianAgainstGoblinTrio(BattleScenarios.SarxianFirstSeed);
        sequence.Update(_step);

        sequence.Submit(new DefendCommand());
        sequence.Update(TimeSpan.FromSeconds(0.1));

        Assert.Equal(TurnStage.StepBack, sequence.Stage);
        Assert.Equal(0.5f, sequence.StepShare(sequence.Actor), 3);
    }

    [Fact]
    public void Update_TargetFell_HoldsLonger()
    {
        var (battle, sequence) = SarxianAgainstGoblinTrio(BattleScenarios.SarxianFirstSeed);
        var goblin = battle.Combatants[1];
        goblin.Vitals.TakeDamage(goblin.Vitals.Hp - 1);
        sequence.Update(_step);
        sequence.Submit(new AttackCommand(goblin));

        sequence.Update(TimeSpan.FromSeconds(1.5));

        Assert.False(goblin.IsAlive);
        Assert.Equal(TurnStage.Holding, sequence.Stage);
    }

    [Fact]
    public void Update_Holding_ActorIsStillTheOneWhoActed()
    {
        var (battle, sequence) = SarxianAgainstGoblinTrio(BattleScenarios.SarxianFirstSeed);
        var sarxian = battle.Combatants[0];
        sequence.Update(_step);

        sequence.Submit(new AttackCommand(battle.Combatants[1]));
        sequence.Update(TimeSpan.FromSeconds(0.5));

        Assert.NotSame(sarxian, battle.CurrentActor);
        Assert.Same(sarxian, sequence.Actor);
    }

    [Fact]
    public void Update_LongDelta_CarriesIntoNextStage()
    {
        var (battle, sequence) = SarxianAgainstGoblinTrio(BattleScenarios.GoblinsFirstSeed);
        var secondGoblin = battle.Combatants[2];

        // Goblin A: 0.2 s forward, 1.0 s hold, 0.2 s back; then 0.05 s into goblin B's step.
        sequence.Update(TimeSpan.FromSeconds(1.45));

        Assert.Equal(TurnStage.StepForward, sequence.Stage);
        Assert.Same(secondGoblin, sequence.Actor);
        Assert.Equal(0.25f, sequence.StepShare(secondGoblin), 3);
    }

    // Omer is a mage, so ending the turn gains mana: a ResourceChanged the screen must take without a beat.
    [Fact]
    public void Update_OmerEndsTurn_NoThrowAndNextActorSteps()
    {
        var random = new SeededRandom(BattleScenarios.SarxianFirstSeed);
        var battle = BattleScenarios.MemberAgainstGoblinTrio("omer", new Inventory(), random);
        var sequence = new BattleTurnSequence(battle, new EnemyTactics(random), RepositoryScreenText.Text.Messages);
        sequence.Update(_step);

        sequence.Submit(new DefendCommand());
        sequence.Update(_step);
        sequence.Update(TimeSpan.FromSeconds(0.1));

        Assert.Equal(TurnStage.StepForward, sequence.Stage);
        Assert.Equal(BattleSide.Enemies, sequence.Actor.Side);
        Assert.Equal(5, battle.Combatants[0].Resources.AmountOf("mana"));
    }

    // Ragnir acts first; Shield Bash stuns goblin A, whose turn is skipped, so goblin B steps forward next.
    [Fact]
    public void Update_EnemyStunnedByBash_NextStageIsTheFollowingActor()
    {
        var random = new SeededRandom(BattleScenarios.SarxianFirstSeed);
        var battle = BattleScenarios.MemberAgainstGoblinTrio("ragnir", new Inventory(), random);
        var sequence = new BattleTurnSequence(battle, new EnemyTactics(random), RepositoryScreenText.Text.Messages);
        sequence.Update(_step);

        sequence.Submit(new SkillCommand(BattleScenarios.Catalog.Skills["shield_bash"], [battle.Combatants[1]]));
        sequence.Update(BattlePacing.ImpactHold + BattlePacing.SkipHold + BattlePacing.Step);

        sequence.Update(TimeSpan.FromSeconds(0.1));

        Assert.Equal(TurnStage.StepForward, sequence.Stage);
        Assert.Same(battle.Combatants[2], sequence.Actor);
    }

    // Ready is quick, so after its banner Lazarus steps forward again for the rest of his turn.
    [Fact]
    public void Update_AfterReady_SameMemberStepsForwardAgain()
    {
        var random = new SeededRandom(BattleScenarios.SarxianFirstSeed);
        var battle = BattleScenarios.MemberAgainstGoblinTrio("lazarus", new Inventory(), random);
        var sequence = new BattleTurnSequence(battle, new EnemyTactics(random), RepositoryScreenText.Text.Messages);
        var lazarus = battle.CurrentActor;
        sequence.Update(_step);

        sequence.Submit(new SkillCommand(BattleScenarios.Catalog.Skills["ready"], [battle.Combatants[1]]));
        sequence.Update(_longEnoughForEveryEnemyTurn);

        Assert.Equal(TurnStage.AwaitingCommand, sequence.Stage);
        Assert.Same(lazarus, sequence.Actor);
        Assert.Equal([battle.Combatants[1], lazarus], battle.UpcomingActors(2));
    }

    private static (BattleState Battle, BattleTurnSequence Sequence) SarxianAgainstGoblinTrio(int seed)
    {
        var random = new SeededRandom(seed);
        var battle = BattleScenarios.SarxianAgainstGoblinTrio(random);
        return (battle, new BattleTurnSequence(battle, new EnemyTactics(random), RepositoryScreenText.Text.Messages));
    }
}
