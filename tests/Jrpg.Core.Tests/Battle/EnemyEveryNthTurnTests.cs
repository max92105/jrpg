using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Skills;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// Every Nth turn holds on the enemy's Nth, 2Nth... turn acted, a turn a stun skipped not counted (decision 847), read
// through battles run by Execute. The combatants act in the order given and defend, which draws nothing, so each
// sequence holds exactly the draws expected: an eligible rule draws nothing, the attack instead its target.
public sealed class EnemyEveryNthTurnTests
{
    private static readonly SkillDefinition _cleave = Skill("cleave", new Effect(EffectKind.Damage, 10, Targeting.AllEnemies));
    private static readonly SkillDefinition _mend = Skill("mend", new Effect(EffectKind.RestoreHp, 25, Targeting.SingleAlly));
    private static readonly Dictionary<string, SkillDefinition> _skills =
        new[] { _cleave, _mend }.ToDictionary(skill => skill.Id);

    private static readonly TacticCondition _everyThirdTurn = new(TacticConditionKind.EveryNthTurn, Turns: 3);
    private static readonly TacticCondition _everySecondTurn = new(TacticConditionKind.EveryNthTurn, Turns: 2);

    [Fact]
    public void ChooseCommand_EveryThirdTurnOnItsFirstTurn_IsNotEligible()
    {
        var member = Member();
        var battle = InOrder(CleavingOnItsThirdTurns(), member);

        var command = Tactics(0).ChooseCommand(battle);

        Assert.Equal(new AttackCommand(member), command);
    }

    [Fact]
    public void ChooseCommand_EveryThirdTurnOnItsThirdTurn_IsEligible()
    {
        var battle = InOrder(CleavingOnItsThirdTurns(), Member());
        DefendFor(battle, rounds: 2);

        var command = Tactics().ChooseCommand(battle);

        Assert.True(IsCleave(command));
    }

    // Its third turn was eligible; "from the third turn on" would hold here too.
    [Fact]
    public void ChooseCommand_EveryThirdTurnOnItsFourthTurn_IsNotEligible()
    {
        var member = Member();
        var battle = InOrder(CleavingOnItsThirdTurns(), member);
        DefendFor(battle, rounds: 3);

        var command = Tactics(0).ChooseCommand(battle);

        Assert.Equal(new AttackCommand(member), command);
    }

    [Fact]
    public void ChooseCommand_EveryThirdTurnOnItsSixthTurn_IsEligible()
    {
        var battle = InOrder(CleavingOnItsThirdTurns(), Member());
        DefendFor(battle, rounds: 5);

        var command = Tactics().ChooseCommand(battle);

        Assert.True(IsCleave(command));
    }

    // Granted after its first turn, the stun takes its second, so the turn chosen for now is its third but its second
    // acted; counting the skipped turn would make it eligible.
    [Fact]
    public void ChooseCommand_SkippedTurn_DoesNotCount()
    {
        var member = Member();
        var actor = CleavingOnItsThirdTurns();
        var battle = InOrder(actor, member);
        battle.Execute(new DefendCommand());
        actor.Statuses.Apply(TestStatuses.Stun(100, 1));
        var skipping = battle.Execute(new DefendCommand());
        battle.Execute(new DefendCommand());

        var command = Tactics(0).ChooseCommand(battle);

        Assert.Contains(skipping, happened => happened is TurnSkipped skipped && skipped.Combatant == actor);
        Assert.Equal(new AttackCommand(member), command);
    }

    // The wounded ally is lower, which the automatic heal would take.
    [Fact]
    public void ChooseCommand_SubjectOfEveryNthTurn_AimsAtItself()
    {
        var rule = new EnemyAction { SkillId = _mend.Id, Weight = 1, When = _everySecondTurn, On = TacticTarget.Subject };
        var actor = Hurt(Enemy("Actor").WithTactics(rule), hp: 80);
        var battle = InOrder(actor, Hurt(Enemy("Wounded"), hp: 10), Member());
        DefendFor(battle, rounds: 1);

        var command = Tactics().ChooseCommand(battle);

        Assert.Equal(actor, TargetOf(command));
    }

    // The trigger names only the actor; before decision 835 the heal kept it alone. The lowest share comes last,
    // against a pick of the first.
    [Fact]
    public void ChooseCommand_AutomaticSingleAllyHealOnEveryNthTurn_HealsTheLowestShare()
    {
        var rule = new EnemyAction { SkillId = _mend.Id, Weight = 1, When = _everySecondTurn };
        var actor = Combatant.FromEnemy(Enemy("Actor").WithTactics(rule).Build());
        var battle = InOrder(actor, Hurt(Enemy("Wounded"), hp: 40), Hurt(Enemy("Lowest"), hp: 30), Member());
        DefendFor(battle, rounds: 1);

        var command = Tactics().ChooseCommand(battle);

        Assert.Equal("Lowest", TargetOf(command).Name);
    }

    // Everyone defends, changing nothing but the turns, so the actor's next turn is its (rounds + 1)th.
    private static void DefendFor(BattleState battle, int rounds)
    {
        for (var turn = 0; turn < rounds * battle.Combatants.Count; turn++)
        {
            battle.Execute(new DefendCommand());
        }
    }

    private static EnemyTactics Tactics(params int[] draws) => new(new FixedSequenceRandom(draws), _skills);

    // Each shuffle value swaps a combatant with itself, so they act in the order given.
    private static BattleState InOrder(params Combatant[] combatants) =>
        new(combatants, new Inventory(), new FixedSequenceRandom([.. Enumerable.Range(1, combatants.Length - 1).Reverse()]));

    private static bool IsCleave(BattleCommand command) => command is SkillCommand { Skill.Id: "cleave" };

    private static Combatant TargetOf(BattleCommand command) => Assert.Single(Assert.IsType<SkillCommand>(command).Targets);

    private static Combatant CleavingOnItsThirdTurns() =>
        Combatant.FromEnemy(
            Enemy("Actor").WithTactics(new EnemyAction { SkillId = _cleave.Id, Weight = 1, When = _everyThirdTurn }).Build());

    private static Combatant Hurt(EnemyDefinitionBuilder enemy, int hp)
    {
        var hurt = Combatant.FromEnemy(enemy.Build());
        hurt.Vitals.TakeDamage(100 - hp);
        return hurt;
    }

    private static EnemyDefinitionBuilder Enemy(string name) =>
        new EnemyDefinitionBuilder().WithName(name).WithStats(maxHp: 100, damage: 5);

    private static Combatant Member() => Combatant.FromPartyMember(new PartyMemberBuilder().Build());

    private static SkillDefinition Skill(string id, Effect effect) =>
        new() { Id = id, Name = id, Description = "", IconFile = "", Effect = effect };
}
