using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// Every hit lands for certain, so the only draw after the shuffle is the crit roll: against 50 % crit chance, 0 crits
// and 999 doesn't. The shuffle (1) keeps the hero first against one goblin, (0) puts the goblin first.
public sealed class BattleCritTests
{
    private static readonly SkillDefinition _mend = new()
    {
        Id = "mend",
        Name = "Mend",
        Description = "",
        IconFile = "",
        Effect = new Effect(EffectKind.RestoreHp, 20, Targeting.Self),
    };

    // The neutral rules' unarmed 10, half again on a crit (decision 300).
    [Fact]
    public void Execute_CritRollSucceeds_ReportsCritical()
    {
        var hero = Member(TestStatRules.Neutral with { BaseCritChance = 50 });
        var goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().Build());
        var battle = new BattleState([hero, goblin], new Inventory(), new FixedSequenceRandom(1, 0));

        var events = battle.Execute(new AttackCommand(goblin));

        Assert.Equal(new EffectApplied(goblin, EffectKind.Damage, 15, IsCritical: true), events.OfType<EffectApplied>().Single());
    }

    [Fact]
    public void Execute_CritRollFails_HitsNormally()
    {
        var hero = Member(TestStatRules.Neutral with { BaseCritChance = 50 });
        var goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().Build());
        var battle = new BattleState([hero, goblin], new Inventory(), new FixedSequenceRandom(1, 999));

        var events = battle.Execute(new AttackCommand(goblin));

        Assert.Equal(new EffectApplied(goblin, EffectKind.Damage, 10), events.OfType<EffectApplied>().Single());
    }

    // 20 Power at 100 % per 100 points adds 20 % to the base 150 %: the unarmed 10 crits for 17.
    [Fact]
    public void Execute_MemberCrit_UsesPowerRaisedCritDamage()
    {
        var hero = Combatant.FromPartyMember(PartyMember.Recruit(
            new PartyMemberBuilder().BuildDefinition(),
            new ClassBuilder().WithPrimaries(PrimaryStats.None with { Power = 20 }, PrimaryStats.None).Build(),
            new Dictionary<string, SkillDefinition>(),
            TestStatRules.Neutral with { BaseCritChance = 50, CritDamagePercentPer100Power = 100 }));
        var goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().Build());
        var battle = new BattleState([hero, goblin], new Inventory(), new FixedSequenceRandom(1, 0));

        var events = battle.Execute(new AttackCommand(goblin));

        Assert.Equal(17, events.OfType<EffectApplied>().Single().Amount);
    }

    // A sure crit would draw nothing, and a heal draws nothing either: the scripted random has only the shuffle.
    [Fact]
    public void Execute_Heal_NeverCrits()
    {
        var hero = Member(TestStatRules.Neutral with { BaseCritChance = 100 }, _mend);
        hero.Vitals.TakeDamage(50);
        var goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().Build());
        var battle = new BattleState([hero, goblin], new Inventory(), new FixedSequenceRandom(1));

        var events = battle.Execute(new SkillCommand(_mend, [hero]));

        Assert.Equal(new EffectApplied(hero, EffectKind.RestoreHp, 20), events.OfType<EffectApplied>().Single());
    }

    // The builder's goblin hits for 20; an enemy's crit is the base 150 %, with no Power to raise it.
    [Fact]
    public void Execute_EnemyCrit_UsesBaseCritDamage()
    {
        var hero = Member(TestStatRules.Neutral);
        var goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithCritChance(50).Build());
        var battle = new BattleState([hero, goblin], new Inventory(), new FixedSequenceRandom(0, 0));

        var events = battle.Execute(new AttackCommand(hero));

        Assert.Equal(new EffectApplied(hero, EffectKind.Damage, 30, IsCritical: true), events.OfType<EffectApplied>().Single());
    }

    private static Combatant Member(StatRules rules, params SkillDefinition[] skills) =>
        Combatant.FromPartyMember(PartyMember.Recruit(
            new PartyMemberBuilder().BuildDefinition(),
            new ClassBuilder().WithSkills([.. skills.Select(skill => skill.Id)]).Build(),
            skills.ToDictionary(skill => skill.Id),
            rules));
}
