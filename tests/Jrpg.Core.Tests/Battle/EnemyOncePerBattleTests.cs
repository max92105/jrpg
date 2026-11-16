using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Skills;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// A once rule isn't eligible after the enemy used its skill in this battle, through any rule (decision 842), read
// through battles run by Execute. The combatants act in the order given and every hit lands, so each sequence holds
// exactly the draws expected: an eligible cleave draws nothing, the attack instead its target.
public sealed class EnemyOncePerBattleTests
{
    private static readonly SkillDefinition _cleave = new()
    {
        Id = "cleave",
        Name = "cleave",
        Description = "",
        IconFile = "",
        Effect = new Effect(EffectKind.Damage, 10, Targeting.AllEnemies),
    };

    private static readonly Dictionary<string, SkillDefinition> _skills = new() { [_cleave.Id] = _cleave };

    private static readonly EnemyAction _onceCleave = new() { SkillId = _cleave.Id, Weight = 1, IsOncePerBattle = true };

    // Its first turn was an attack, which uses no skill.
    [Fact]
    public void ChooseCommand_OnceRuleNotUsedYet_IsEligible()
    {
        var member = Member();
        var actor = Combatant.FromEnemy(Enemy("Actor").WithTactics(_onceCleave).Build());
        var battle = InOrder(actor, member);
        battle.Execute(new AttackCommand(member));
        battle.Execute(new DefendCommand());

        var command = Tactics().ChooseCommand(battle);

        Assert.True(IsCleave(command));
    }

    [Fact]
    public void ChooseCommand_OnceRuleAfterItsSkillWasUsed_IsNotEligible()
    {
        var member = Member();
        var actor = Combatant.FromEnemy(Enemy("Actor").WithTactics(_onceCleave).Build());
        var battle = InOrder(actor, member);
        battle.Execute(Tactics().ChooseCommand(battle));
        battle.Execute(new DefendCommand());

        var command = Tactics(0).ChooseCommand(battle);

        Assert.Equal(new AttackCommand(member), command);
    }

    // The member's hit makes the priority rule the one chosen on the first turn; the second, unhit, holds it no more.
    [Fact]
    public void ChooseCommand_OnceSkillUsedThroughAnotherRule_IsNotEligible()
    {
        var member = Member();
        var whenHit = new EnemyAction
        {
            SkillId = _cleave.Id,
            Weight = 1,
            When = new TacticCondition(TacticConditionKind.WasHit),
            IsPriority = true,
        };
        var actor = Combatant.FromEnemy(Enemy("Actor").WithTactics(_onceCleave, whenHit).Build());
        var battle = InOrder(member, actor);
        battle.Execute(new AttackCommand(actor));
        battle.Execute(Tactics().ChooseCommand(battle));
        battle.Execute(new DefendCommand());

        var command = Tactics(0).ChooseCommand(battle);

        Assert.Equal(new AttackCommand(member), command);
    }

    [Fact]
    public void ChooseCommand_OnceSkillUsedByAnotherEnemy_StillEligible()
    {
        var other = Combatant.FromEnemy(Enemy("Other").WithTactics(_onceCleave).Build());
        var battle = InOrder(other, Combatant.FromEnemy(Enemy("Actor").WithTactics(_onceCleave).Build()), Member());
        battle.Execute(Tactics().ChooseCommand(battle));

        var command = Tactics().ChooseCommand(battle);

        Assert.True(IsCleave(command));
    }

    // The member's hit takes the actor below half, into the phase, whose list holds a once cleave of its own.
    [Fact]
    public void ChooseCommand_OnceRuleInAPhaseAfterUseBeforeThePhase_IsNotEligible()
    {
        var member = Member(attack: 60);
        var phase = new EnemyPhase { BelowHpPercent = 50, Tactics = [_onceCleave] };
        var actor = Combatant.FromEnemy(Enemy("Actor").WithTactics(_onceCleave).WithPhases(phase).Build());
        var battle = InOrder(actor, member);
        battle.Execute(Tactics().ChooseCommand(battle));
        var hit = battle.Execute(new AttackCommand(actor));

        var command = Tactics(0).ChooseCommand(battle);

        Assert.Contains(new EnemyPhaseEntered(actor, phase), hit);
        Assert.Equal(new AttackCommand(member), command);
    }

    private static EnemyTactics Tactics(params int[] draws) => new(new FixedSequenceRandom(draws), _skills);

    // Each shuffle value swaps a combatant with itself, so they act in the order given.
    private static BattleState InOrder(params Combatant[] combatants) =>
        new(combatants, new Inventory(), new FixedSequenceRandom([.. Enumerable.Range(1, combatants.Length - 1).Reverse()]));

    private static bool IsCleave(BattleCommand command) => command is SkillCommand { Skill.Id: "cleave" };

    private static EnemyDefinitionBuilder Enemy(string name) =>
        new EnemyDefinitionBuilder().WithName(name).WithStats(maxHp: 100, damage: 5);

    private static Combatant Member(int attack = 10) =>
        Combatant.FromPartyMember(new PartyMemberBuilder().WithAttack(attack).Build());
}
