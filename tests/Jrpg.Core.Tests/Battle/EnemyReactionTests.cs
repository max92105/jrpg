using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// A reaction holds from a foe's hit until the end of the enemy's next turn (decisions 844, 845), read through battles
// run by Execute. The combatants act in the order given, every hit lands, and a crit chance of 0 or 100 draws nothing,
// so each sequence holds exactly the draws expected: an eligible cleave draws nothing, the attack instead its target.
public sealed class EnemyReactionTests
{
    private static readonly SkillDefinition _cleave = Skill("cleave", new Effect(EffectKind.Damage, 10, Targeting.AllEnemies));
    private static readonly SkillDefinition _strike = Skill("strike", new Effect(EffectKind.Damage, 7, Targeting.SingleEnemy));
    private static readonly SkillDefinition _mend = Skill("mend", new Effect(EffectKind.RestoreHp, 25, Targeting.SingleAlly));
    private static readonly Dictionary<string, SkillDefinition> _skills =
        new[] { _cleave, _strike, _mend }.ToDictionary(skill => skill.Id);

    [Fact]
    public void ChooseCommand_WasHitSinceItsLastTurn_IsEligible()
    {
        var actor = Reacting(TacticConditionKind.WasHit);
        var battle = InOrder(Member(), actor);
        battle.Execute(new AttackCommand(actor));

        var command = Tactics().ChooseCommand(battle);

        Assert.True(IsCleave(command));
    }

    // Its own attack ended the turn that answered the hit; the member then defends, hitting nobody.
    [Fact]
    public void ChooseCommand_WasHitBeforeItsLastTurn_IsForgotten()
    {
        var member = Member();
        var actor = Reacting(TacticConditionKind.WasHit);
        var battle = InOrder(member, actor);
        battle.Execute(new AttackCommand(actor));
        battle.Execute(new AttackCommand(member));
        battle.Execute(new DefendCommand());

        var command = Tactics(0).ChooseCommand(battle);

        Assert.Equal(new AttackCommand(member), command);
    }

    // A dodge of 50 % leaves the member's hit a roll, which 999 misses.
    [Fact]
    public void ChooseCommand_HitMissed_IsNotAHit()
    {
        var member = Member();
        var actor = Combatant.FromEnemy(Enemy("Actor").WithDodge(50).WithTactics(CleaveWhen(TacticConditionKind.WasHit)).Build());
        var battle = new BattleState([member, actor], new Inventory(), new FixedSequenceRandom(1, 999));
        battle.Execute(new AttackCommand(actor));

        var command = Tactics(0).ChooseCommand(battle);

        Assert.Equal(new AttackCommand(member), command);
    }

    [Fact]
    public void ChooseCommand_HitOnAnotherEnemy_IsNotEligible()
    {
        var member = Member();
        var other = Reacting(TacticConditionKind.WasHit, "Other");
        var battle = InOrder(member, Reacting(TacticConditionKind.WasHit), other);
        battle.Execute(new AttackCommand(other));

        var command = Tactics(0).ChooseCommand(battle);

        Assert.Equal(new AttackCommand(member), command);
    }

    // The actor's own cleave forgets its reaction, not the other enemy's, who acts next.
    [Fact]
    public void ChooseCommand_AllEnemiesSkill_NotesEveryTarget()
    {
        var battle = InOrder(Member(), Reacting(TacticConditionKind.WasHit), Reacting(TacticConditionKind.WasHit, "Other"));
        battle.Execute(new SkillCommand(_cleave, battle.TargetChoiceFor(_cleave).Candidates));
        var actorsCommand = Tactics().ChooseCommand(battle);
        battle.Execute(actorsCommand);

        var othersCommand = Tactics().ChooseCommand(battle);

        Assert.True(IsCleave(actorsCommand) && IsCleave(othersCommand));
    }

    [Fact]
    public void ChooseCommand_WasCriticallyHitByACrit_IsEligible()
    {
        var actor = Reacting(TacticConditionKind.WasCriticallyHit);
        var battle = InOrder(Member(critChance: 100), actor);
        battle.Execute(new AttackCommand(actor));

        var command = Tactics().ChooseCommand(battle);

        Assert.True(IsCleave(command));
    }

    [Fact]
    public void ChooseCommand_WasCriticallyHitByAPlainHit_IsNotEligible()
    {
        var member = Member();
        var actor = Reacting(TacticConditionKind.WasCriticallyHit);
        var battle = InOrder(member, actor);
        battle.Execute(new AttackCommand(actor));

        var command = Tactics(0).ChooseCommand(battle);

        Assert.Equal(new AttackCommand(member), command);
    }

    // The member's unarmed hit is blunt, so a weakness to piercing leaves it neutral.
    [Theory]
    [InlineData(DamageType.Blunt, true)]
    [InlineData(DamageType.Piercing, false)]
    public void ChooseCommand_WasHitOnWeakness_EligibleOnlyForAWeakHit(DamageType weakTo, bool isEligible)
    {
        var rule = CleaveWhen(TacticConditionKind.WasHitOnWeakness);
        var actor = Combatant.FromEnemy(Enemy("Actor").WithResistances(WeakTo(weakTo)).WithTactics(rule).Build());
        var battle = InOrder(Member(), actor);
        battle.Execute(new AttackCommand(actor));

        var command = Tactics(isEligible ? [] : [0]).ChooseCommand(battle);

        Assert.Equal(isEligible, IsCleave(command));
    }

    // Granted before the battle, the stun takes the actor's first turn; the member then defends.
    [Fact]
    public void ChooseCommand_StunnedThroughItsTurn_KeepsTheReaction()
    {
        var actor = Reacting(TacticConditionKind.WasHit);
        actor.Statuses.Apply(TestStatuses.Stun(100, 1));
        var battle = InOrder(Member(), actor);
        battle.Execute(new AttackCommand(actor));
        battle.Execute(new DefendCommand());

        var command = Tactics().ChooseCommand(battle);

        Assert.True(IsCleave(command));
    }

    // The first member's share is the lowest, which a pick among all foes would take; only the second hit the actor.
    [Fact]
    public void ChooseCommand_SubjectOfWasHit_AimsAtTheHitter()
    {
        var hitter = Member();
        var actor = Combatant.FromEnemy(Enemy("Actor").WithTactics(StrikeOnTheHitter()).Build());
        var battle = InOrder(Member(hp: 30), hitter, actor);
        battle.Execute(new DefendCommand());
        battle.Execute(new AttackCommand(actor));

        var command = Tactics().ChooseCommand(battle);

        Assert.Equal(hitter, Assert.Single(Assert.IsType<SkillCommand>(command).Targets));
    }

    // The other enemy's attack fells the hitter, so the strike has no legal one and the attack takes the last member.
    [Fact]
    public void ChooseCommand_SubjectOfWasHitWhoFell_RuleNotEligible()
    {
        var hitter = Member(hp: 1);
        var actor = Combatant.FromEnemy(Enemy("Actor").WithTactics(StrikeOnTheHitter()).Build());
        var keeper = Member();
        var battle = InOrder(hitter, Combatant.FromEnemy(Enemy("Feller").Build()), actor, keeper);
        battle.Execute(new AttackCommand(actor));
        battle.Execute(new AttackCommand(hitter));

        var command = Tactics(0).ChooseCommand(battle);

        Assert.Equal(new AttackCommand(keeper), command);
    }

    // The roll of 500 fails the actor's 50 % shrug-off (decision 838).
    [Fact]
    public void ChooseCommand_SubjectOfWasHitWhileAnotherMemberTaunts_AimsAtTheTaunter()
    {
        var taunter = Member();
        taunter.Statuses.Apply(TestStatuses.Taunt(2));
        var actor = Combatant.FromEnemy(
            Enemy("Actor").WithCrowdControlResistance(50).WithTactics(StrikeOnTheHitter()).Build());
        var battle = InOrder(Member(), actor, taunter);
        battle.Execute(new AttackCommand(actor));

        var command = Tactics(500).ChooseCommand(battle);

        Assert.Equal(taunter, Assert.Single(Assert.IsType<SkillCommand>(command).Targets));
    }

    [Fact]
    public void ChooseCommand_AutomaticSingleAllyHealOnWasHit_HealsTheLowestShare()
    {
        Assert.Equal("Lowest", HealTargetAfterAWeakCrit(TacticConditionKind.WasHit).Name);
    }

    [Fact]
    public void ChooseCommand_AutomaticSingleAllyHealOnWasCriticallyHit_HealsTheLowestShare()
    {
        Assert.Equal("Lowest", HealTargetAfterAWeakCrit(TacticConditionKind.WasCriticallyHit).Name);
    }

    [Fact]
    public void ChooseCommand_AutomaticSingleAllyHealOnWasHitOnWeakness_HealsTheLowestShare()
    {
        Assert.Equal("Lowest", HealTargetAfterAWeakCrit(TacticConditionKind.WasHitOnWeakness).Name);
    }

    // A crit on the actor's weakness notes all three reactions, which name only the member; before decision 835 the
    // heal kept those its trigger named, nobody, and threw. The lowest share comes last, against a pick of the first.
    private static Combatant HealTargetAfterAWeakCrit(TacticConditionKind kind)
    {
        var rule = new EnemyAction { SkillId = _mend.Id, Weight = 1, When = new TacticCondition(kind) };
        var actor = Combatant.FromEnemy(Enemy("Actor").WithResistances(WeakTo(DamageType.Blunt)).WithTactics(rule).Build());
        var battle = InOrder(Member(critChance: 100), actor, Hurt("Wounded", hp: 40), Hurt("Lowest", hp: 30));
        battle.Execute(new AttackCommand(actor));

        var command = Tactics().ChooseCommand(battle);

        return Assert.Single(Assert.IsType<SkillCommand>(command).Targets);
    }

    private static EnemyTactics Tactics(params int[] draws) => new(new FixedSequenceRandom(draws), _skills);

    // Each shuffle value swaps a combatant with itself, so they act in the order given.
    private static BattleState InOrder(params Combatant[] combatants) =>
        new(combatants, new Inventory(), new FixedSequenceRandom([.. Enumerable.Range(1, combatants.Length - 1).Reverse()]));

    private static bool IsCleave(BattleCommand command) => command is SkillCommand { Skill.Id: "cleave" };

    private static EnemyAction CleaveWhen(TacticConditionKind kind) =>
        new() { SkillId = _cleave.Id, Weight = 1, When = new TacticCondition(kind) };

    private static EnemyAction StrikeOnTheHitter() =>
        new() { SkillId = _strike.Id, Weight = 1, When = new(TacticConditionKind.WasHit), On = TacticTarget.Subject };

    private static Combatant Reacting(TacticConditionKind kind, string name = "Actor") =>
        Combatant.FromEnemy(Enemy(name).WithTactics(CleaveWhen(kind)).Build());

    private static Combatant Hurt(string name, int hp)
    {
        var enemy = Combatant.FromEnemy(Enemy(name).Build());
        enemy.Vitals.TakeDamage(100 - hp);
        return enemy;
    }

    private static EnemyDefinitionBuilder Enemy(string name) =>
        new EnemyDefinitionBuilder().WithName(name).WithStats(maxHp: 100, damage: 5);

    // A weakness of -25 %, in permille.
    private static Resistances WeakTo(DamageType type) =>
        type == DamageType.Blunt ? Resistances.None with { Blunt = -250 } : Resistances.None with { Piercing = -250 };

    // The neutral rules make every hit land; the member knows cleave, for the hit on every enemy.
    private static Combatant Member(int critChance = 0, int hp = 100)
    {
        var member = Combatant.FromPartyMember(PartyMember.Recruit(
            new PartyMemberBuilder().BuildDefinition(),
            new ClassBuilder().WithSkills(_cleave.Id).Build(),
            new Dictionary<string, SkillDefinition> { [_cleave.Id] = _cleave },
            TestStatRules.Neutral with { BaseCritChance = critChance }));
        member.Vitals.TakeDamage(100 - hp);
        return member;
    }

    private static SkillDefinition Skill(string id, Effect effect) =>
        new() { Id = id, Name = id, Description = "", IconFile = "", Effect = effect };
}
