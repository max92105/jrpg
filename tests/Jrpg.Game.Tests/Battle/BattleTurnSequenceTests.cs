using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Randomness;
using Jrpg.Data.Content;
using Jrpg.Game.Battle;

namespace Jrpg.Game.Tests.Battle;

public sealed class BattleTurnSequenceTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    // Turn orders the seeds shuffle for Sarxian and three goblins.
    private const int GoblinsFirstSeed = 1;
    private const int SarxianFirstSeed = 4;

    [Fact]
    public void Update_EnemyActsFirst_RunsUntilPartyTurn()
    {
        var sequence = SarxianAgainstGoblinTrio(GoblinsFirstSeed);
        Assert.False(sequence.IsAwaitingCommand);

        sequence.Update();

        Assert.True(sequence.IsAwaitingCommand);
        Assert.Equal(BattleSide.Party, sequence.Actor.Side);
    }

    [Fact]
    public void Submit_Flee_IsOver()
    {
        var sequence = SarxianAgainstGoblinTrio(SarxianFirstSeed);

        sequence.Submit(new FleeCommand());

        Assert.True(sequence.IsOver);
    }

    [Fact]
    public void Submit_NotAwaiting_Throws()
    {
        var sequence = SarxianAgainstGoblinTrio(GoblinsFirstSeed);

        Assert.Throws<InvalidOperationException>(() => sequence.Submit(new FleeCommand()));
    }

    private static BattleTurnSequence SarxianAgainstGoblinTrio(int seed)
    {
        var sarxian = PartyMember.Recruit(_catalog.PartyMembers["sarxian"], _catalog.Skills);
        var party = new PartyState([sarxian], [sarxian], _catalog.Rules.Party);
        var goblin = _catalog.Enemies["goblin"];
        var random = new SeededRandom(seed);
        var battle = BattleState.Start(party, [goblin, goblin, goblin], new Inventory(), random);
        return new BattleTurnSequence(battle, new EnemyTactics(random));
    }
}
