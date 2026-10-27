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

// A member with 20 Power at 100 % per 100 points deals 20 % more, against one goblin with defense 0; the shuffle (1)
// keeps the member first.
public sealed class BattlePowerTests
{
    private static readonly StatRules _powerRules = TestStatRules.Neutral with { DamagePercentPer100Power = 100 };

    private static readonly SkillDefinition _blast = Skill("blast", new Effect(EffectKind.Damage, 40, Targeting.SingleEnemy));

    [Fact]
    public void Execute_SkillByStrongMember_DealsScaledDamage()
    {
        var (goblin, battle) = Battle(Member([_blast]), new Inventory());

        var events = battle.Execute(new SkillCommand(_blast, [goblin]));

        Assert.Equal(48, events.OfType<EffectApplied>().Single().Amount);
    }

    // The neutral rules' unarmed attack is 10; the sword's 30 replaces it.
    [Fact]
    public void Execute_AttackWithSword_UsesSwordDamageNotUnarmed()
    {
        var fighter = new PartyMemberBuilder().Build();
        var sword = new ItemDefinitionBuilder().WithId("bronze_sword").AsWeapon(30).Build();
        var inventory = new Inventory();
        inventory.Add(sword, 1);
        new PartyEquipment(inventory).TryEquip(fighter, sword);
        var (goblin, battle) = Battle(Combatant.FromPartyMember(fighter), inventory);

        var events = battle.Execute(new AttackCommand(goblin));

        Assert.Equal(30, events.OfType<EffectApplied>().Single().Amount);
    }

    // The sword's 30, raised 20 % by Power: 36.
    [Fact]
    public void Execute_AttackByStrongMember_DealsScaledDamage()
    {
        var fighter = StrongMember([]);
        var sword = new ItemDefinitionBuilder().WithId("bronze_sword").AsWeapon(30).Build();
        var inventory = new Inventory();
        inventory.Add(sword, 1);
        new PartyEquipment(inventory).TryEquip(fighter, sword);
        var (goblin, battle) = Battle(Combatant.FromPartyMember(fighter), inventory);

        var events = battle.Execute(new AttackCommand(goblin));

        Assert.Equal(36, events.OfType<EffectApplied>().Single().Amount);
    }

    // A consumable lands its own number, whoever throws it (decision 321).
    [Fact]
    public void Execute_ItemByStrongMember_LandsItsOwnPower()
    {
        var bomb = new ItemDefinitionBuilder().WithId("bomb").WithEffect(new Effect(EffectKind.Damage, 40, Targeting.SingleEnemy)).Build();
        var inventory = new Inventory();
        inventory.Add(bomb, 1);
        var (goblin, battle) = Battle(Member([]), inventory);

        var events = battle.Execute(new ItemCommand(bomb, [goblin]));

        Assert.Equal(40, events.OfType<EffectApplied>().Single().Amount);
    }

    // Power first: 45 × 1.2 = 54, then Decay's +50 %: 81. Empowering the raw 45 first would give 67, then 80.
    [Fact]
    public void Execute_DominantForceOnScaledSkill_RaisesTheScaledValue()
    {
        var thorns = Skill("thorns", new Effect(EffectKind.Damage, 45, Targeting.SingleEnemy), new ResourceChange("decay", 1));
        var druid = Member(
            [thorns],
            new ResourceDefinition("growth", "Growth", Max: 10) { Dominance = new DominanceRule("decay", 50) },
            new ResourceDefinition("decay", "Decay", Max: 10) { Dominance = new DominanceRule("growth", 50) });
        druid.ChangeResource("decay", 2, []);
        var (goblin, battle) = Battle(druid, new Inventory());

        var events = battle.Execute(new SkillCommand(thorns, [goblin]));

        Assert.Equal(81, events.OfType<EffectApplied>().Single().Amount);
    }

    private static (Combatant Goblin, BattleState Battle) Battle(Combatant member, Inventory inventory)
    {
        var goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp: 200, attack: 20, defense: 0).Build());
        return (goblin, new BattleState([member, goblin], inventory, new FixedSequenceRandom(1)));
    }

    private static Combatant Member(SkillDefinition[] skills, params ResourceDefinition[] resources) =>
        Combatant.FromPartyMember(StrongMember(skills, resources));

    private static PartyMember StrongMember(SkillDefinition[] skills, params ResourceDefinition[] resources) =>
        PartyMember.Recruit(
            new PartyMemberBuilder().BuildDefinition(),
            new ClassBuilder()
                .WithPrimaries(PrimaryStats.None with { Power = 20 }, PrimaryStats.None)
                .WithResources(resources)
                .WithSkills([.. skills.Select(skill => skill.Id)])
                .Build(),
            skills.ToDictionary(skill => skill.Id),
            _powerRules);

    private static SkillDefinition Skill(string id, Effect effect, params ResourceChange[] changes) =>
        new() { Id = id, Name = id, Description = "", IconFile = "", ResourceChanges = changes, Effect = effect };
}
