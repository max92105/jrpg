using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Randomness;
using Jrpg.Data.Content;
using Jrpg.Game.Battle;

namespace Jrpg.Game.Tests.Battle;

public sealed class TurnOrderStripTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    // Seed 4 shuffles Sarxian and the three goblins into that same order.
    private const int SarxianFirstSeed = 4;

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

    private static BattleState SarxianAgainstGoblinTrio()
    {
        var sarxian = PartyMember.Recruit(_catalog.PartyMembers["sarxian"], _catalog.Skills);
        var party = new PartyState([sarxian], [sarxian], _catalog.Rules.Party);
        var goblin = _catalog.Enemies["goblin"];
        return BattleState.Start(party, [goblin, goblin, goblin], new Inventory(), new SeededRandom(SarxianFirstSeed));
    }
}
