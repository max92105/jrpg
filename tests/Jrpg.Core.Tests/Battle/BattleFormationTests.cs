using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Skills;
using Jrpg.Core.Statuses;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// Ragnir, Teelle and Zanyr against a goblin; the shuffle (3, 2, 1) keeps the listed order, so Ragnir acts first.
public sealed class BattleFormationTests
{
    private static readonly int[] _firstActsFirst = [4, 3, 2, 1];
    private static readonly int[] _secondActsFirst = [4, 3, 2, 0];

    private static readonly SkillDefinition _aura = new()
    {
        Id = "aura",
        Name = "Aura",
        Description = "",
        IconFile = "",
        Effect = new Effect(EffectKind.GrantStatus, 0, Targeting.AlliesInRange, new StatusGrant(StatusKind.Guarded, Power: 5, Turns: 3), Range: 1),
    };

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

    [Fact]
    public void TargetChoiceFor_AuraRangeOneInMiddle_ReachesThree()
    {
        var (line, battle) = LineOfFour(_secondActsFirst);

        var choice = battle.TargetChoiceFor(_aura);

        Assert.Equal([line[0], line[1], line[2]], choice.Candidates);
        Assert.True(choice.TakesAll);
    }

    [Fact]
    public void TargetChoiceFor_AuraRangeOneAtEnd_ReachesTwo()
    {
        var (line, battle) = LineOfFour(_firstActsFirst);

        var choice = battle.TargetChoiceFor(_aura);

        Assert.Equal([line[0], line[1]], choice.Candidates);
    }

    [Fact]
    public void TargetChoiceFor_AuraSkipsFallenInRange()
    {
        var (line, battle) = LineOfFour(_secondActsFirst);
        line[0].Vitals.TakeDamage(999);

        var choice = battle.TargetChoiceFor(_aura);

        Assert.Equal([line[1], line[2]], choice.Candidates);
    }

    // The first member moves behind the second, so the second, acting next, now stands at the front of the line.
    [Fact]
    public void TargetChoiceFor_AfterMove_RangeFollowsNewPlace()
    {
        var (line, battle) = LineOfFour(_firstActsFirst);
        battle.Execute(new MoveCommand(line[1]));

        var choice = battle.TargetChoiceFor(_aura);

        Assert.Same(line[1], battle.CurrentActor);
        Assert.Equal([line[1], line[0]], choice.Candidates);
    }

    [Fact]
    public void Execute_Aura_GuardsOnlyAlliesInRange()
    {
        var (line, battle) = LineOfFour(_secondActsFirst);

        battle.Execute(new SkillCommand(_aura, battle.TargetChoiceFor(_aura).Candidates));

        Assert.Equal([true, true, true, false], line.Select(member => member.Statuses.Has(StatusKind.Guarded)));
    }

    // Four members, each knowing the aura (range 1, Guarded +5), then a goblin. The shuffle (4, 3, 2, 1) keeps the
    // listed turn order; (4, 3, 2, 0) lets the second member act first. The formation is the listed order either way.
    private static (Combatant[] Line, BattleState Battle) LineOfFour(int[] shuffle)
    {
        Combatant[] line = [Knowing("ragnir"), Knowing("teelle"), Knowing("zanyr"), Knowing("omer")];
        var goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp: 40, attack: 20, defense: 2).Build());
        return (line, new BattleState([.. line, goblin], new Inventory(), new FixedSequenceRandom(shuffle)));
    }

    private static Combatant Knowing(string id) => Combatant.FromPartyMember(new PartyMemberBuilder().WithId(id).WithSkills(_aura).Build());

    private static Combatant Member(string id) => Combatant.FromPartyMember(new PartyMemberBuilder().WithId(id).Build());
}
