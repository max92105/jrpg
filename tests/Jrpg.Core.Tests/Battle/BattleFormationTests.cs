using Jrpg.Core.Battle;
using Jrpg.Core.Items;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// Ragnir, Teelle and Zanyr against a goblin; the shuffle (3, 2, 1) keeps the listed order, so Ragnir acts first.
public sealed class BattleFormationTests
{
    private readonly Combatant _ragnir = Member("ragnir");
    private readonly Combatant _teelle = Member("teelle");
    private readonly Combatant _zanyr = Member("zanyr");
    private readonly BattleState _battle;

    public BattleFormationTests()
    {
        var goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp: 40, attack: 20, defense: 2).Build());
        _battle = new BattleState([_ragnir, _teelle, _zanyr, goblin], new Inventory(), new FixedSequenceRandom(3, 2, 1));
    }

    [Fact]
    public void Execute_MoveToNeighbor_SwapsAndReports()
    {
        var events = _battle.Execute(new MoveCommand(_teelle));

        Assert.Equal([_teelle, _ragnir, _zanyr], _battle.Formation.Line);
        Assert.Equal(new PlacesSwapped(_ragnir, _teelle), events[1]);
    }

    [Fact]
    public void Execute_MoveToNonNeighbor_Throws()
    {
        Assert.Throws<ArgumentException>(() => _battle.Execute(new MoveCommand(_zanyr)));

        Assert.Equal([_ragnir, _teelle, _zanyr], _battle.Formation.Line);
        Assert.Same(_ragnir, _battle.CurrentActor);
    }

    [Fact]
    public void Execute_MoveToFallenNeighbor_Swaps()
    {
        _teelle.Vitals.TakeDamage(999);

        _battle.Execute(new MoveCommand(_teelle));

        Assert.Equal([_teelle, _ragnir, _zanyr], _battle.Formation.Line);
    }

    [Fact]
    public void Execute_Move_EndsTurn()
    {
        _battle.Execute(new MoveCommand(_teelle));

        Assert.Same(_teelle, _battle.CurrentActor);
    }

    [Fact]
    public void TargetChoiceForMove_FirstInLine_OffersTheOneNeighbor()
    {
        var choice = _battle.TargetChoiceForMove();

        Assert.Equal([_teelle], choice.Candidates);
    }

    [Fact]
    public void Execute_MoveOnEnemyTurn_Throws()
    {
        var battle = GoblinFirst();

        Assert.Throws<InvalidOperationException>(() => battle.Execute(new MoveCommand(_teelle)));
    }

    [Fact]
    public void TargetChoiceForMove_EnemyTurn_OffersNone()
    {
        var battle = GoblinFirst();

        var choice = battle.TargetChoiceForMove();

        Assert.Empty(choice.Candidates);
    }

    // The shuffle (0, 2, 1) swaps the goblin to the front of the turn order.
    private BattleState GoblinFirst()
    {
        var goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp: 40, attack: 20, defense: 2).Build());
        return new BattleState([_ragnir, _teelle, _zanyr, goblin], new Inventory(), new FixedSequenceRandom(0, 2, 1));
    }

    private static Combatant Member(string id) => Combatant.FromPartyMember(new PartyMemberBuilder().WithId(id).Build());
}
