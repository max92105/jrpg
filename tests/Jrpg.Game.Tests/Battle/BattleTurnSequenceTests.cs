using Jrpg.Core.Battle;
using Jrpg.Core.Randomness;
using Jrpg.Game.Battle;

namespace Jrpg.Game.Tests.Battle;

public sealed class BattleTurnSequenceTests
{
    [Fact]
    public void Update_EnemyActsFirst_RunsUntilPartyTurn()
    {
        var sequence = SarxianAgainstGoblinTrio(BattleScenarios.GoblinsFirstSeed);
        Assert.False(sequence.IsAwaitingCommand);

        sequence.Update();

        Assert.True(sequence.IsAwaitingCommand);
        Assert.Equal(BattleSide.Party, sequence.Actor.Side);
    }

    [Fact]
    public void Submit_Flee_IsOver()
    {
        var sequence = SarxianAgainstGoblinTrio(BattleScenarios.SarxianFirstSeed);

        sequence.Submit(new FleeCommand());

        Assert.True(sequence.IsOver);
    }

    [Fact]
    public void Submit_NotAwaiting_Throws()
    {
        var sequence = SarxianAgainstGoblinTrio(BattleScenarios.GoblinsFirstSeed);

        Assert.Throws<InvalidOperationException>(() => sequence.Submit(new FleeCommand()));
    }

    private static BattleTurnSequence SarxianAgainstGoblinTrio(int seed)
    {
        var random = new SeededRandom(seed);
        return new BattleTurnSequence(BattleScenarios.SarxianAgainstGoblinTrio(random), new EnemyTactics(random));
    }
}
