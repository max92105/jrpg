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

// A tiered skill (ADR 0056) deals its user's Power times its record's per-Power, between a member and one goblin
// without resistances; the member has 20 Power from the class and 100 % more damage per 100 Power, which a tiered skill
// ignores (decision 784). The member's tests shuffle with 1, keeping the member first; the enemy's test with 0.
public sealed class BattleTieredSkillTests
{
    private static readonly StatRules _powerRules = TestStatRules.Neutral with { DamagePercentPer100Power = 100 };

    private static readonly TieredEffect _blunt1 = new("blunt_1", EffectKind.Damage, 2);

    private static readonly SkillDefinition _bolt =
        Skill("bolt", new Effect(EffectKind.Damage, 0, Targeting.SingleEnemy) { Tier = _blunt1 });

    [Fact]
    public void Execute_TieredSkill_DealsTheMembersPowerTimesPerPower()
    {
        var (goblin, battle) = Battle(Combatant.FromPartyMember(Member([_bolt])), new Inventory());

        var events = battle.Execute(new SkillCommand(_bolt, [goblin]));

        Assert.Equal(40, events.OfType<EffectApplied>().Single().Amount);
    }

    // A ring adds 5 Power: (20 + 5) x 2 = 50.
    [Fact]
    public void Execute_TieredSkillWithPowerFromGear_CountsTheGear()
    {
        var member = Member([_bolt]);
        var ring = new ItemDefinitionBuilder().WithId("ring").AsWorn(EquipmentKind.Ring)
            .WithBonuses(new StatBonuses(new Dictionary<StatKind, int> { [StatKind.Power] = 5 })).Build();
        var inventory = new Inventory();
        inventory.Add(ring, 1);
        new PartyEquipment(inventory).TryEquip(member, GearPiece.Plain(ring));
        var (goblin, battle) = Battle(Combatant.FromPartyMember(member), inventory);

        var events = battle.Execute(new SkillCommand(_bolt, [goblin]));

        Assert.Equal(50, events.OfType<EffectApplied>().Single().Amount);
    }

    // The tiered amount first (40), then Decay's +50 %: 60 (decision 783's order).
    [Fact]
    public void Execute_TieredSkillWithDominantForce_RaisesTheTieredAmount()
    {
        var thorns = Skill(
            "thorns",
            new Effect(EffectKind.Damage, 0, Targeting.SingleEnemy) { Tier = _blunt1 },
            new ResourceChange("decay", 1));
        var druid = Combatant.FromPartyMember(Member(
            [thorns],
            new ResourceDefinition("growth", "Growth", Max: 10) { Dominance = new DominanceRule("decay", 50) },
            new ResourceDefinition("decay", "Decay", Max: 10) { Dominance = new DominanceRule("growth", 50) }));
        druid.ChangeResource("decay", 2, []);
        var (goblin, battle) = Battle(druid, new Inventory());

        var events = battle.Execute(new SkillCommand(thorns, [goblin]));

        Assert.Equal(60, events.OfType<EffectApplied>().Single().Amount);
    }

    // An enemy's content Power (decision 787): 5 x 2 = 10 against a member without resistances. The shuffle (0) puts
    // the enemy first.
    [Fact]
    public void Execute_EnemyTieredSkill_DealsItsPowerTimesTheTier()
    {
        var chief = Combatant.FromEnemy(new EnemyDefinitionBuilder()
            .WithStats(maxHp: 200, damage: 20)
            .WithPower(5)
            .WithTactics(new EnemyAction { SkillId = _bolt.Id, Weight = 1 })
            .Build());
        var member = Combatant.FromPartyMember(new PartyMemberBuilder().Build());
        var battle = new BattleState([member, chief], new Inventory(), new FixedSequenceRandom(0));

        var events = battle.Execute(new SkillCommand(_bolt, [member]));

        Assert.Same(member, events.OfType<EffectApplied>().Single().Target);
        Assert.Equal(10, events.OfType<EffectApplied>().Single().Amount);
    }

    private static (Combatant Goblin, BattleState Battle) Battle(Combatant member, Inventory inventory)
    {
        var goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp: 200, damage: 20).Build());
        return (goblin, new BattleState([member, goblin], inventory, new FixedSequenceRandom(1)));
    }

    private static PartyMember Member(SkillDefinition[] skills, params ResourceDefinition[] resources) =>
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
