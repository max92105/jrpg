using Jrpg.Core.Battle;
using Jrpg.Core.Randomness;
using Jrpg.Game.Battle;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Battle;

// Who blinks while an action winds up (decision 648): its actor only, and only while it winds up. On Sarxian against
// the goblin trio, stepped at the game's update.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class ActionWindUpFlashTests
{
    private static readonly BattleState _battle =
        BattleScenarios.SarxianAgainstGoblinTrio(new SeededRandom(BattleScenarios.SarxianFirstSeed));

    private static readonly Combatant _sarxian = _battle.Combatants[0];
    private static readonly Combatant _goblin = _battle.Combatants[1];

    [Fact]
    public void BlinkOf_ActorAtTheFirstBlink_IsFull()
    {
        var windUp = SteppedTo(new AttackCommand(_goblin), TimeSpan.FromSeconds(0.10));

        Assert.Equal(1, windUp.BlinkOf(_sarxian));
    }

    [Fact]
    public void BlinkOf_Target_IsZero()
    {
        var windUp = SteppedTo(new AttackCommand(_goblin), TimeSpan.FromSeconds(0.10));

        Assert.Equal(0, windUp.BlinkOf(_goblin));
    }

    [Fact]
    public void BlinkOf_ActorAfterTheWindUp_IsZero()
    {
        var windUp = SteppedTo(new AttackCommand(_goblin), BattlePacing.WindUp);

        Assert.Equal(0, windUp.BlinkOf(_sarxian));
    }

    // Defend lands at once, but the sequence keeps its wind-up and the time running until the next action.
    [Fact]
    public void BlinkOf_DefenderAtTheFirstBlinksTime_IsZero()
    {
        var windUp = SteppedTo(new DefendCommand(), TimeSpan.FromSeconds(0.10));

        Assert.Equal(0, windUp.BlinkOf(_sarxian));
    }

    private static ActionWindUp SteppedTo(BattleCommand command, TimeSpan time)
    {
        var windUp = ActionWindUp.Of(_sarxian, command, RepositoryScreenText.Text.Messages, BattleScenarios.NoEffectStrips);
        while (windUp.Elapsed < time)
        {
            windUp.Update(StartMapScenarios.OneUpdate);
        }

        return windUp;
    }
}
