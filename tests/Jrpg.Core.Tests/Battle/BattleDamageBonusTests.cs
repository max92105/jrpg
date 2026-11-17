using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// A member with 20 Power and 100 % more damage per 100 Power wields a 25-damage sword rolled with +20 % damage
// (decision 886) against one goblin without resistances; every hit lands and none crits (the neutral rules), and the
// shuffle (1) keeps the member first.
public sealed class BattleDamageBonusTests
{
    private const int DamageBonusPercent = 20;

    private static readonly StatRules _powerRules = TestStatRules.Neutral with { DamagePercentPer100Power = 100 };

    private static readonly SkillDefinition _bolt = Skill(
        "bolt", new Effect(EffectKind.Damage, 0, Targeting.SingleEnemy) { Tier = new("blunt_1", EffectKind.Damage, 2) });

    private static readonly SkillDefinition _mend = Skill(
        "mend", new Effect(EffectKind.RestoreHp, 0, Targeting.Self) { Tier = new("heal_1", EffectKind.RestoreHp, 2) });

    // 20 x 2 = 40, then 1 + 0.2: 48.
    [Fact]
    public void Execute_WearerOfTwentyPercentUsesATieredSkill_DealsOneAndAFifthMore()
    {
        var (_, goblin, battle) = Battle(new Inventory());

        var events = battle.Execute(new SkillCommand(_bolt, [goblin]));

        Assert.Equal(48, events.OfType<EffectApplied>().Single().Amount);
    }

    // The sword's 25 at Power's 120 % is 30; the bonus makes it 25 x 1.2 x 1.2 = 36.
    [Fact]
    public void Execute_WearerOfTwentyPercentAttacks_DealsOneAndAFifthMore()
    {
        var (_, goblin, battle) = Battle(new Inventory());

        var events = battle.Execute(new AttackCommand(goblin));

        Assert.Equal(36, events.OfType<EffectApplied>().Single().Amount);
    }

    // The damage modifiers never touch healing (decision 786): 20 x 2 = 40, bonus or not.
    [Fact]
    public void Execute_WearerOfTwentyPercentHeals_HealsTheSame()
    {
        var (member, _, battle) = Battle(new Inventory());
        member.Vitals.TakeDamage(50);

        var events = battle.Execute(new SkillCommand(_mend, [member]));

        Assert.Equal(new EffectApplied(member, EffectKind.RestoreHp, 40), events.OfType<EffectApplied>().Single());
    }

    // An item keeps its flat power whoever throws it (decisions 321, 873): the bomb deals its 30.
    [Fact]
    public void Execute_WearerOfTwentyPercentUsesABomb_DealsTheSame()
    {
        var bomb = new ItemDefinitionBuilder().WithId("bomb")
            .WithEffect(new Effect(EffectKind.Damage, 30, Targeting.SingleEnemy)).Build();
        var inventory = new Inventory();
        inventory.Add(bomb, 1);
        var (_, goblin, battle) = Battle(inventory);

        var events = battle.Execute(new ItemCommand(bomb, [goblin]));

        Assert.Equal(30, events.OfType<EffectApplied>().Single().Amount);
    }

    // One sum (decision 704): 40 x (1 + 0.2 - 0.25) = 38, where the two as factors (1.2 x 0.75) would give 36.
    [Fact]
    public void Execute_WeakenedWearerOfTwentyPercent_AddsBothToTheSum()
    {
        var (member, goblin, battle) = Battle(new Inventory());
        member.Statuses.Apply(TestStatuses.Weakened(-25, 3));

        var events = battle.Execute(new SkillCommand(_bolt, [goblin]));

        Assert.Equal(38, events.OfType<EffectApplied>().Single().Amount);
    }

    private static (Combatant Member, Combatant Goblin, BattleState Battle) Battle(Inventory inventory)
    {
        var member = PartyMember.Recruit(
            new PartyMemberBuilder().BuildDefinition(),
            new ClassBuilder()
                .WithPrimaries(PrimaryStats.None with { Power = 20 }, PrimaryStats.None)
                .WithSkills([_bolt.Id, _mend.Id])
                .Build(),
            new Dictionary<string, SkillDefinition> { [_bolt.Id] = _bolt, [_mend.Id] = _mend },
            _powerRules);
        var sword = new ItemDefinitionBuilder().WithId("sword").AsWeapon(25).Build();
        var piece = GearPiece.Rolled(sword, Rarity.Magic, [new RolledAffix("fury", StatKind.DamageBonus, DamageBonusPercent)]);
        inventory.AddGear(piece);
        Assert.True(new PartyEquipment(inventory).TryEquip(member, piece));
        var wearer = Combatant.FromPartyMember(member);
        var goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp: 200, damage: 20).Build());
        return (wearer, goblin, new BattleState([wearer, goblin], inventory, new FixedSequenceRandom(1)));
    }

    private static SkillDefinition Skill(string id, Effect effect) =>
        new() { Id = id, Name = id, Description = "", IconFile = "", Effect = effect };
}
