using Jrpg.Core.Battle;
using Jrpg.Core.Randomness;
using Jrpg.Game.Battle;

namespace Jrpg.Game.Tests.Battle;

public sealed class TurnOrderStripTests
{
    private readonly TurnOrderStrip _strip = new();

    [Fact]
    public void Refresh_SarxianAndThreeGoblins_ListsCurrentThenEightNext()
    {
        var battle = SarxianAgainstGoblinTrio();
        var (sarxian, a, b, c) = (battle.Combatants[0], battle.Combatants[1], battle.Combatants[2], battle.Combatants[3]);

        _strip.Refresh(battle);

        Assert.Equal([sarxian, a, b, c, sarxian, a, b, c, sarxian], _strip.Shown);
    }

    [Fact]
    public void Highlight_OneTarget_DimsEveryoneElse()
    {
        var battle = SarxianAgainstGoblinTrio();
        var target = battle.Combatants[2];

        _strip.Highlight([target]);

        Assert.Equal([false, false, true, false], battle.Combatants.Select(_strip.IsLit));
    }

    [Fact]
    public void Highlight_EmptyTargets_LightsEveryone()
    {
        var battle = SarxianAgainstGoblinTrio();
        _strip.Highlight([battle.Combatants[2]]);

        _strip.Highlight([]);

        Assert.All(battle.Combatants, combatant => Assert.True(_strip.IsLit(combatant)));
    }

    private static BattleState SarxianAgainstGoblinTrio() =>
        BattleScenarios.SarxianAgainstGoblinTrio(new SeededRandom(BattleScenarios.SarxianFirstSeed));
}
