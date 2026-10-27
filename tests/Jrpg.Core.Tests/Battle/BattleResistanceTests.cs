using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// Every hit here has power 40 against defense 0, so a resistance alone changes what lands; the shuffle (1) keeps the
// first member first.
public sealed class BattleResistanceTests
{
    private static readonly SkillDefinition _fireBlast = Skill(Targeting.SingleEnemy);

    // Fire -25 % adds a quarter: 40 becomes 50.
    [Fact]
    public void Execute_FireSkillOnFireWeakEnemy_DealsMore()
    {
        var mage = Member("elena", PrimaryStats.None, TestStatRules.Neutral, _fireBlast);
        var goblin = Goblin(Resistances.None with { Fire = -250 });
        var battle = new BattleState([mage, goblin], new Inventory(), new FixedSequenceRandom(1));

        var events = battle.Execute(new SkillCommand(_fireBlast, [goblin]));

        Assert.Contains(new EffectApplied(goblin, EffectKind.Damage, 50), events);
    }

    // Slashing 50 % takes half off the sword's 40.
    [Fact]
    public void Execute_AttackOnSlashingResistantEnemy_DealsLess()
    {
        var fighter = new PartyMemberBuilder().WithStats(attack: 40, defense: 0).Build();
        var sword = new ItemDefinitionBuilder().WithId("bronze_sword").AsWeapon(0).WithDamageType(DamageType.Slashing).Build();
        var inventory = new Inventory();
        inventory.Add(sword, 1);
        new PartyEquipment(inventory).TryEquip(fighter, sword);
        var goblin = Goblin(Resistances.None with { Slashing = 500 });
        var battle = new BattleState([Combatant.FromPartyMember(fighter), goblin], inventory, new FixedSequenceRandom(1));

        var events = battle.Execute(new AttackCommand(goblin));

        Assert.Contains(new EffectApplied(goblin, EffectKind.Damage, 20), events);
    }

    // 20 Will at 100 % per 100 points is 20 % magical resistance: the 40 fire hit lands as 32.
    [Fact]
    public void Execute_MemberWithWillResistsFireHit()
    {
        var friendlyFire = Skill(Targeting.SingleAlly);
        var mage = Member("elena", PrimaryStats.None, TestStatRules.Neutral, friendlyFire);
        var willRules = TestStatRules.Neutral with { MagicalResistancePer100Will = 100 };
        var cleric = Member("mira", PrimaryStats.None with { Will = 20 }, willRules);
        var battle = new BattleState([mage, cleric, Goblin(Resistances.None)], new Inventory(), new FixedSequenceRandom(2, 1));

        var events = battle.Execute(new SkillCommand(friendlyFire, [cleric]));

        Assert.Contains(new EffectApplied(cleric, EffectKind.Damage, 32), events);
    }

    private static Combatant Member(string id, PrimaryStats primaries, StatRules rules, params SkillDefinition[] skills) =>
        Combatant.FromPartyMember(PartyMember.Recruit(
            new PartyMemberBuilder().WithId(id).BuildDefinition(),
            new ClassBuilder().WithId(id).WithBase(new ClassStats(MaxHp: 100, Attack: 10, Defense: 0, Speed: 10)).WithPrimaries(primaries, PrimaryStats.None).WithSkills([.. skills.Select(skill => skill.Id)]).Build(),
            skills.ToDictionary(skill => skill.Id),
            rules));

    private static Combatant Goblin(Resistances resistances) =>
        Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp: 100, attack: 20, defense: 0).WithResistances(resistances).Build());

    private static SkillDefinition Skill(Targeting targeting) => new()
    {
        Id = "fire_blast",
        Name = "Fire Blast",
        Description = "",
        IconFile = "",
        Effect = new Effect(EffectKind.Damage, 40, targeting, DamageType: DamageType.Fire),
    };
}
