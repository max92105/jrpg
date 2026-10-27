using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Party;

// A member's stats: their class's at their level, and what worn gear adds or would add.
public sealed class PartyMemberStatTests
{
    private static readonly ClassDefinition _growingClass = new ClassBuilder()
        .WithBase(new ClassStats(MaxHp: 100, Attack: 12, Defense: 7, Speed: 0))
        .WithGrowth(new ClassStats(MaxHp: 10, Attack: 2, Defense: 1, Speed: 0))
        .Build();

    [Fact]
    public void BaseStats_RestoredAtLevelThree_AreTheClassAtThree()
    {
        var progress = new PartyMemberProgress("test_member", 3, 0, Hp: 50, WeaponId: null, ArmorId: null, LastingAmounts: new Dictionary<string, int>());

        var member = PartyMember.Restore(
            new PartyMemberBuilder().BuildDefinition(), _growingClass, new Dictionary<string, SkillDefinition>(), progress);

        Assert.Equal(new CombatStats(Attack: 16, Defense: 9), member.BaseStats);
        Assert.Equal((50, 120), (member.Vitals.Hp, member.Vitals.MaxHp));
    }

    [Fact]
    public void TotalStats_WithWeaponAndArmor_AddsBonuses()
    {
        var member = new PartyMemberBuilder().WithClass("fighter").WithStats(attack: 10, defense: 5).Build();
        Equip(member, new ItemDefinitionBuilder().WithId("bronze_sword").AsWeapon(5).Build());
        Equip(member, new ItemDefinitionBuilder().WithId("chain_mail").AsArmor(6).Build());

        var stats = member.TotalStats;

        Assert.Equal(new CombatStats(Attack: 15, Defense: 11), stats);
    }

    [Fact]
    public void StatsWithout_Weapon_DropsOnlyTheWeaponBonus()
    {
        var member = new PartyMemberBuilder().WithClass("fighter").WithStats(attack: 10, defense: 5).Build();
        Equip(member, new ItemDefinitionBuilder().WithId("bronze_sword").AsWeapon(5).Build());
        Equip(member, new ItemDefinitionBuilder().WithId("chain_mail").AsArmor(6).Build());

        var preview = member.StatsWithout(EquipmentSlot.Weapon);

        Assert.Equal(new CombatStats(Attack: 10, Defense: 11), preview);
        Assert.Equal(new CombatStats(Attack: 15, Defense: 11), member.TotalStats);
    }

    [Fact]
    public void StatsWith_OtherWeapon_ReplacesCurrentWeaponBonus()
    {
        var member = new PartyMemberBuilder().WithClass("fighter").WithStats(attack: 10, defense: 5).Build();
        Equip(member, new ItemDefinitionBuilder().WithId("bronze_sword").AsWeapon(5).Build());
        Equip(member, new ItemDefinitionBuilder().WithId("chain_mail").AsArmor(6).Build());
        var ironSword = new ItemDefinitionBuilder().WithId("iron_sword").AsWeapon(8).Build();

        var preview = member.StatsWith(ironSword);

        Assert.Equal(new CombatStats(Attack: 18, Defense: 11), preview);
        Assert.Equal(new CombatStats(Attack: 15, Defense: 11), member.TotalStats);
    }

    [Fact]
    public void TotalStats_WeaponWithDefenseBonus_AddsItToDefense()
    {
        var member = new PartyMemberBuilder().WithClass("knight").WithStats(attack: 10, defense: 5).Build();
        Equip(member, new ItemDefinitionBuilder().WithId("shield_sword").AsWeapon(0).WithBonuses(attack: 4, defense: 2).Build());

        var stats = member.TotalStats;

        Assert.Equal(new CombatStats(Attack: 14, Defense: 7), stats);
    }

    [Fact]
    public void StatsWith_OtherArmor_ReplacesCurrentArmorBonus()
    {
        var member = new PartyMemberBuilder().WithClass("fighter").WithStats(attack: 10, defense: 5).Build();
        Equip(member, new ItemDefinitionBuilder().WithId("leather_armor").AsArmor(3).Build());
        var chainMail = new ItemDefinitionBuilder().WithId("chain_mail").AsArmor(6).Build();

        var preview = member.StatsWith(chainMail);

        Assert.Equal(new CombatStats(Attack: 10, Defense: 11), preview);
    }

    [Fact]
    public void StatsWith_NotEquipment_Throws()
    {
        var member = new PartyMemberBuilder().Build();

        Assert.Throws<ArgumentException>(() => member.StatsWith(new ItemDefinitionBuilder().Build()));
    }

    private static void Equip(PartyMember member, ItemDefinition item)
    {
        var inventory = new Inventory();
        inventory.Add(item, 1);
        Assert.True(new PartyEquipment(inventory).TryEquip(member, item));
    }
}
