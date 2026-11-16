using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// Weakened (-25 %, decision 806) on a member with 20 Power and 100 % more damage per 100 Power, against one goblin
// without resistances; every hit lands and none crits (the neutral rules), and the shuffle (1) keeps the member first.
public sealed class BattleWeakenedTests
{
    private static readonly StatRules _powerRules = TestStatRules.Neutral with { DamagePercentPer100Power = 100 };

    private static readonly SkillDefinition _bolt = Skill(
        "bolt", new Effect(EffectKind.Damage, 0, Targeting.SingleEnemy) { Tier = new("blunt_1", EffectKind.Damage, 2) });

    // 20 x 2 = 40, then 1 - 0.25: 30.
    [Fact]
    public void Execute_WeakenedHoldersTieredSkill_DealsLess()
    {
        var (goblin, battle) = Battle(WeakenedMember([_bolt]), new Inventory());

        var events = battle.Execute(new SkillCommand(_bolt, [goblin]));

        Assert.Equal(30, events.OfType<EffectApplied>().Single().Amount);
    }

    // Unarmed 10 at Power's 120 % is 12; Weakened's sum makes it 10 x 1.2 x 0.75 = 9.
    [Fact]
    public void Execute_WeakenedHoldersAttack_DealsLess()
    {
        var (goblin, battle) = Battle(WeakenedMember([]), new Inventory());

        var events = battle.Execute(new AttackCommand(goblin));

        Assert.Equal(9, events.OfType<EffectApplied>().Single().Amount);
    }

    // The damage modifiers never touch healing (decision 786): 20 x 2 = 40, Weakened or not.
    [Fact]
    public void Execute_WeakenedHoldersHeal_HealsTheSame()
    {
        var mend = Skill(
            "mend", new Effect(EffectKind.RestoreHp, 0, Targeting.Self) { Tier = new("heal_1", EffectKind.RestoreHp, 2) });
        var member = WeakenedMember([mend]);
        member.Vitals.TakeDamage(50);
        var (_, battle) = Battle(member, new Inventory());

        var events = battle.Execute(new SkillCommand(mend, [member]));

        Assert.Equal(new EffectApplied(member, EffectKind.RestoreHp, 40), events.OfType<EffectApplied>().Single());
    }

    // An item isn't raised by its user (decision 321), nor lowered: the bomb deals its 30.
    [Fact]
    public void Execute_WeakenedHoldersItem_DealsTheSame()
    {
        var bomb = new ItemDefinitionBuilder().WithId("bomb")
            .WithEffect(new Effect(EffectKind.Damage, 30, Targeting.SingleEnemy)).Build();
        var inventory = new Inventory();
        inventory.Add(bomb, 1);
        var (goblin, battle) = Battle(WeakenedMember([]), inventory);

        var events = battle.Execute(new ItemCommand(bomb, [goblin]));

        Assert.Equal(30, events.OfType<EffectApplied>().Single().Amount);
    }

    // Decision 817: 40 x 0.75 = 30, then Decay's +50 %: 45, where one sum (1 - 0.25 + 0.5) would give 50.
    [Fact]
    public void Execute_WeakenedHolderWithDominantForce_MultipliesNotSums()
    {
        var thorns = Skill(
            "thorns",
            new Effect(EffectKind.Damage, 0, Targeting.SingleEnemy) { Tier = new("blunt_1", EffectKind.Damage, 2) },
            new ResourceChange("decay", 1));
        var druid = WeakenedMember(
            [thorns],
            new ResourceDefinition("growth", "Growth", Max: 10) { Dominance = new DominanceRule("decay", 50) },
            new ResourceDefinition("decay", "Decay", Max: 10) { Dominance = new DominanceRule("growth", 50) });
        druid.ChangeResource("decay", 2, []);
        var (goblin, battle) = Battle(druid, new Inventory());

        var events = battle.Execute(new SkillCommand(thorns, [goblin]));

        Assert.Equal(45, events.OfType<EffectApplied>().Single().Amount);
    }

    private static (Combatant Goblin, BattleState Battle) Battle(Combatant member, Inventory inventory)
    {
        var goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp: 200, damage: 20).Build());
        return (goblin, new BattleState([member, goblin], inventory, new FixedSequenceRandom(1)));
    }

    private static Combatant WeakenedMember(SkillDefinition[] skills, params ResourceDefinition[] resources)
    {
        var member = Combatant.FromPartyMember(PartyMember.Recruit(
            new PartyMemberBuilder().BuildDefinition(),
            new ClassBuilder()
                .WithPrimaries(PrimaryStats.None with { Power = 20 }, PrimaryStats.None)
                .WithResources(resources)
                .WithSkills([.. skills.Select(skill => skill.Id)])
                .Build(),
            skills.ToDictionary(skill => skill.Id),
            _powerRules));
        member.Statuses.Apply(TestStatuses.Weakened(-25, 3));
        return member;
    }

    private static SkillDefinition Skill(string id, Effect effect, params ResourceChange[] changes) =>
        new() { Id = id, Name = id, Description = "", IconFile = "", ResourceChanges = changes, Effect = effect };
}
