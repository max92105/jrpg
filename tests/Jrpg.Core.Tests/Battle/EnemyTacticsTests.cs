using Jrpg.Core.Battle;
using Jrpg.Core.Items;
using Jrpg.Core.Statuses;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

public sealed class EnemyTacticsTests
{
    private readonly Combatant _sarxian = PartyCombatant("sarxian");
    private readonly Combatant _jazog = PartyCombatant("jazog");
    private readonly Combatant _teelle = PartyCombatant("teelle");
    private readonly Combatant _goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().Build());

    [Fact]
    public void ChooseCommand_ScriptedRandom_AttacksChosenLivingPartyMember()
    {
        _jazog.Vitals.TakeDamage(999);
        // Shuffle values (0, 2, 1) swap the goblin to the front, so it is the enemy's turn.
        var battle = new BattleState([_sarxian, _jazog, _teelle, _goblin], new Inventory(), new FixedSequenceRandom(0, 2, 1));
        var tactics = new EnemyTactics(new FixedSequenceRandom(1));

        var command = tactics.ChooseCommand(battle);

        // Living party members, in battle-list order, are [Sarxian, Teelle]; index 1 is Teelle.
        Assert.Equal(new AttackCommand(_teelle), command);
    }

    [Fact]
    public void ChooseCommand_OnPartyTurn_Throws()
    {
        var battle = new BattleState([_sarxian, _goblin], new Inventory(), new FixedSequenceRandom(1));
        var tactics = new EnemyTactics(new FixedSequenceRandom(0));

        Assert.Throws<InvalidOperationException>(() => tactics.ChooseCommand(battle));
    }

    // The builder's goblin resists nothing, so a taunt costs it no draw: the one value is the target's.
    [Fact]
    public void ChooseCommand_OneMemberTaunting_AttacksThem()
    {
        Taunt(_jazog);
        var tactics = new EnemyTactics(new FixedSequenceRandom(0));

        var command = tactics.ChooseCommand(GoblinsTurn());

        Assert.Equal(new AttackCommand(_jazog), command);
    }

    [Fact]
    public void ChooseCommand_TwoTaunting_PicksAmongThemOnly()
    {
        Taunt(_jazog);
        Taunt(_teelle);
        var tactics = new EnemyTactics(new FixedSequenceRandom(1));

        var command = tactics.ChooseCommand(GoblinsTurn());

        // The taunting members, in battle-list order, are [Jazog, Teelle]; index 1 is Teelle.
        Assert.Equal(new AttackCommand(_teelle), command);
    }

    // Without a taunt, the targets are every living member and one random call picks among them, as before Taunt.
    [Fact]
    public void ChooseCommand_NoTaunt_SameTargetAsBeforeForSequence()
    {
        var tactics = new EnemyTactics(new FixedSequenceRandom(2));

        var command = tactics.ChooseCommand(GoblinsTurn());

        Assert.Equal(new AttackCommand(_teelle), command);
    }

    [Fact]
    public void ChooseCommand_TaunterFallen_AttacksAnyLivingMember()
    {
        Taunt(_jazog);
        _jazog.Vitals.TakeDamage(999);
        var tactics = new EnemyTactics(new FixedSequenceRandom(1));

        var command = tactics.ChooseCommand(GoblinsTurn());

        // The living members are [Sarxian, Teelle]; index 1 is Teelle.
        Assert.Equal(new AttackCommand(_teelle), command);
    }

    // The goblin resists half the taunts: a draw of 499 shrugs one off, 500 doesn't; then the target draw.
    [Fact]
    public void ChooseCommand_TauntResisted_TargetsAnyLivingMember()
    {
        Taunt(_jazog);
        var tactics = new EnemyTactics(new FixedSequenceRandom(499, 2));

        var command = tactics.ChooseCommand(GoblinsTurn(StubbornGoblin()));

        // The living members are [Sarxian, Jazog, Teelle]; index 2 is Teelle.
        Assert.Equal(new AttackCommand(_teelle), command);
    }

    [Fact]
    public void ChooseCommand_TauntNotResisted_TargetsTheTaunter()
    {
        Taunt(_jazog);
        var tactics = new EnemyTactics(new FixedSequenceRandom(500, 0));

        var command = tactics.ChooseCommand(GoblinsTurn(StubbornGoblin()));

        Assert.Equal(new AttackCommand(_jazog), command);
    }

    // Without a taunt there is nothing to resist, so even a stubborn goblin draws only for its target.
    [Fact]
    public void ChooseCommand_NoTauntAgainstResistantEnemy_DrawsOnlyForTheTarget()
    {
        var tactics = new EnemyTactics(new FixedSequenceRandom(2));

        var command = tactics.ChooseCommand(GoblinsTurn(StubbornGoblin()));

        Assert.Equal(new AttackCommand(_teelle), command);
    }

    // Shuffle values (0, 2, 1) swap the goblin to the front, so it is the enemy's turn.
    private BattleState GoblinsTurn() => GoblinsTurn(_goblin);

    private BattleState GoblinsTurn(Combatant goblin) =>
        new([_sarxian, _jazog, _teelle, goblin], new Inventory(), new FixedSequenceRandom(0, 2, 1));

    private static Combatant StubbornGoblin() =>
        Combatant.FromEnemy(new EnemyDefinitionBuilder().WithCrowdControlResistance(50).Build());

    private static void Taunt(Combatant member) => member.Statuses.Apply(new StatusGrant(StatusKind.Taunt, Power: 0, Turns: 2));

    private static Combatant PartyCombatant(string id) => Combatant.FromPartyMember(new PartyMemberBuilder().WithId(id).Build());
}
