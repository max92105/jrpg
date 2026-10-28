using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Progression;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Party;

// What a member wears adds to their sheet (ADR 0026): 200 HP and speed 10 at level 1, +20 HP a level, no Constitution
// of their own, at +1 % HP per Constitution point.
public sealed class PartyMemberGearTests
{
    private static readonly StatRules _rules = TestStatRules.Neutral with { HpPercentPer100Constitution = 100 };

    private static readonly ClassDefinition _class = new ClassBuilder()
        .WithBase(new ClassStats(MaxHp: 200, Speed: 10))
        .WithGrowth(new ClassStats(MaxHp: 20, Speed: 0))
        .Build();

    private readonly Inventory _inventory = new();

    [Fact]
    public void Wear_ConstitutionArmor_RaisesMaxHpKeepsHp()
    {
        var member = NewMember();

        Equip(member, Armor(StatKind.Constitution, 10));

        Assert.Equal((200, 220), (member.Vitals.Hp, member.Vitals.MaxHp));
    }

    [Fact]
    public void TakeOff_ConstitutionArmor_ClampsHpToNewMax()
    {
        var member = NewMember();
        Equip(member, Armor(StatKind.Constitution, 10));
        member.Rest();

        Assert.True(new PartyEquipment(_inventory).TryUnequip(member, EquipmentSlot.Chest));

        Assert.Equal((200, 200), (member.Vitals.Hp, member.Vitals.MaxHp));
    }

    [Fact]
    public void Wear_SpeedBonus_RaisesSpeed()
    {
        var member = NewMember();

        Equip(member, Armor(StatKind.Speed, 5));

        Assert.Equal(15, member.Sheet.Speed);
    }

    [Fact]
    public void GainExperience_WearingConstitution_RaisesHpByTheLevelsGain()
    {
        var member = NewMember();
        Equip(member, Armor(StatKind.Constitution, 10));
        member.Rest();

        member.GainExperience(100, new ExperienceTable([100, 100]));

        // Level 2 in the armor: the class's 220 HP raised 10 % by its Constitution.
        Assert.Equal((242, 242), (member.Vitals.Hp, member.Vitals.MaxHp));
    }

    [Fact]
    public void SheetWithout_WornArmor_LosesItsBonuses()
    {
        var member = NewMember();
        Equip(member, Armor(StatKind.Constitution, 10));

        var sheet = member.SheetWithout(EquipmentSlot.Chest);

        Assert.Equal((0, 200), (sheet.Primaries.Constitution, sheet.MaxHp));
        Assert.Equal(10, member.Sheet.Primaries.Constitution);
    }

    [Fact]
    public void SheetWith_Armor_AddsItsBonusesWithoutWearingIt()
    {
        var member = NewMember();

        var sheet = member.SheetWith(GearPiece.Plain(Armor(StatKind.Constitution, 10)));

        Assert.Equal(220, sheet.MaxHp);
        Assert.Null(member.EquippedIn(EquipmentSlot.Chest));
        Assert.Equal(200, member.Sheet.MaxHp);
    }

    // A second ring previewed in the second ring slot keeps the first ring on: the page's second ring row compares
    // against that slot, not the first.
    [Fact]
    public void SheetWith_RingInTheSecondSlot_KeepsTheFirstRing()
    {
        var member = NewMember();
        var ring = new ItemDefinitionBuilder().WithId("power_ring").AsWorn(EquipmentKind.Ring).WithBonuses(new StatBonuses(new Dictionary<StatKind, int> { [StatKind.Power] = 5 })).Build();
        _inventory.Add(ring, 1);
        Assert.True(new PartyEquipment(_inventory).TryEquip(member, ring, EquipmentSlot.FirstRing));

        var sheet = member.SheetWith(GearPiece.Plain(ring), EquipmentSlot.SecondRing);

        Assert.Equal(member.Sheet.Primaries.Power + 5, sheet.Primaries.Power);
    }

    [Fact]
    public void SheetWith_RingInTheWeaponSlot_Throws()
    {
        var member = NewMember();
        var ring = GearPiece.Plain(new ItemDefinitionBuilder().WithId("power_ring").AsWorn(EquipmentKind.Ring).Build());

        Assert.Throws<ArgumentException>(() => member.SheetWith(ring, EquipmentSlot.Weapon));
    }

    // A rolled piece's affixes reach the sheet as its base bonuses do (ADR 0029).
    [Fact]
    public void Wear_RolledRing_AffixRaisesSheet()
    {
        var member = NewMember();
        var ring = new ItemDefinitionBuilder().WithId("power_ring").AsWorn(EquipmentKind.Ring).WithBonuses(new StatBonuses(new Dictionary<StatKind, int> { [StatKind.Power] = 2 })).Build();

        member.Wear(EquipmentSlot.FirstRing, GearPiece.Rolled(ring, Rarity.Magic, [new RolledAffix("power", StatKind.Power, 4)]));

        Assert.Equal(member.SheetWithout(EquipmentSlot.FirstRing).Primaries.Power + 6, member.Sheet.Primaries.Power);
    }

    // Decision 322: an off-hand item is stats only; the basic attack stays the unarmed one.
    [Fact]
    public void Wear_ShieldInOffHand_AddsItsResistancesNotAnAttack()
    {
        var member = NewMember();
        var shield = new ItemDefinitionBuilder().WithId("wooden_shield").AsWorn(EquipmentKind.OffHand).WithBonuses(new StatBonuses(new Dictionary<StatKind, int> { [StatKind.BluntResistance] = 8 })).Build();

        Equip(member, shield);

        Assert.Same(shield, member.EquippedIn(EquipmentSlot.OffHand)?.Base);
        Assert.Equal(80, member.Sheet.Resistances.Blunt);
        Assert.Equal(_rules.UnarmedDamage, member.Sheet.AttackDamage);
    }

    private static PartyMember NewMember() =>
        PartyMember.Recruit(new PartyMemberBuilder().BuildDefinition(), _class, new Dictionary<string, SkillDefinition>(), _rules);

    private static ItemDefinition Armor(StatKind kind, int bonus) =>
        new ItemDefinitionBuilder().WithId("test_armor").AsChest().WithBonuses(new StatBonuses(new Dictionary<StatKind, int> { [kind] = bonus })).Build();

    private void Equip(PartyMember member, ItemDefinition item)
    {
        _inventory.Add(item, 1);
        Assert.True(new PartyEquipment(_inventory).TryEquip(member, item));
    }
}
