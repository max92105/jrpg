using Jrpg.Core.Battle;
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
        var battle = new BattleState([_sarxian, _jazog, _teelle, _goblin], new FixedSequenceRandom(0, 2, 1));
        var tactics = new EnemyTactics(new FixedSequenceRandom(1));

        var command = tactics.ChooseCommand(battle);

        // Living party members, in battle-list order, are [Sarxian, Teelle]; index 1 is Teelle.
        Assert.Equal(new AttackCommand(_teelle), command);
    }

    [Fact]
    public void ChooseCommand_OnPartyTurn_Throws()
    {
        var battle = new BattleState([_sarxian, _goblin], new FixedSequenceRandom(1));
        var tactics = new EnemyTactics(new FixedSequenceRandom(0));

        Assert.Throws<InvalidOperationException>(() => tactics.ChooseCommand(battle));
    }

    private static Combatant PartyCombatant(string id) => Combatant.FromPartyMember(new PartyMemberBuilder().WithId(id).Build());
}
