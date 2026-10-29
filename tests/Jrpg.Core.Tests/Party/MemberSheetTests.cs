using Jrpg.Core.Party;
using Jrpg.Core.Stats;

namespace Jrpg.Core.Tests.Party;

public sealed class MemberSheetTests
{
    // Every rate and base distinct, so a number put in the wrong place can't match by chance.
    private static readonly StatRules _rules = new(
        HpPercentPer100Constitution: 100,
        BaseAccuracy: 90,
        BaseCritChance: 5,
        BaseDodge: 0,
        BaseCrowdControlResistance: 15,
        DamagePercentPer100Power: 100,
        CritDamagePercentPer100Power: 50,
        CritChancePer100Agility: 25,
        DodgePer100Agility: 20,
        PhysicalResistancePer100Constitution: 25,
        MagicalResistancePer100Will: 25,
        StatusPercentPer100Intelligence: 100,
        IntelligencePerExtraStatusTurn: 25,
        UnarmedDamage: 18,
        UnarmedDamageType: DamageType.Blunt);

    [Fact]
    public void Of_ClassAtLevel_FillsEveryNumberFromItsOwnSource()
    {
        var primaries = new PrimaryStats(Power: 20, Agility: 20, Intelligence: 30, Constitution: 10, Will: 20);
        var atLevel = new ClassStats(MaxHp: 200, Speed: 12) { Accuracy = 5, Primaries = primaries };

        var sheet = MemberSheet.Of(atLevel, StatBonuses.None, weaponDamage: null, _rules);

        var expected = new StatSheet(
            Primaries: primaries,
            MaxHp: 220,
            Speed: 12,
            AttackDamage: 18,
            AccuracyPermille: 950,
            CritChancePermille: 100,
            CritDamagePermille: 1600,
            DodgePermille: 40,
            CrowdControlResistancePermille: 150,
            DamagePermille: 1200,
            Resistances: Resistances.ByGroup(physicalPermille: 25, magicalPermille: 50),
            StatusPermille: 1300,
            ExtraStatusTurns: 1);
        Assert.Equal(expected, sheet);
    }

    [Fact]
    public void Of_WeaponWorn_AttackDamageIsTheWeapons()
    {
        var sheet = MemberSheet.Of(ClassWith(PrimaryStats.None), StatBonuses.None, weaponDamage: 24, _rules);

        Assert.Equal(24, sheet.AttackDamage);
    }

    [Fact]
    public void Of_PowerBonus_RaisesPowerAndDamagePercent()
    {
        var atLevel = ClassWith(new PrimaryStats(Power: 10, Agility: 0, Intelligence: 0, Constitution: 0, Will: 0));

        var sheet = MemberSheet.Of(atLevel, Bonus(StatKind.Power, 5), weaponDamage: null, _rules);

        Assert.Equal((15, 1150), (sheet.Primaries.Power, sheet.DamagePermille));
    }

    // Drop luck isn't on CombatStats (battles don't read it), so the sheet is where its copy is checked.
    [Fact]
    public void Of_DropLuckWorn_InPermille()
    {
        var sheet = MemberSheet.Of(ClassWith(PrimaryStats.None), Bonus(StatKind.DropLuck, 12), weaponDamage: null, _rules);

        Assert.Equal(120, sheet.DropLuckPermille);
    }

    [Fact]
    public void Of_FlatFireResistance_AddsAfterWill()
    {
        var atLevel = ClassWith(new PrimaryStats(Power: 0, Agility: 0, Intelligence: 0, Constitution: 0, Will: 20));

        var sheet = MemberSheet.Of(atLevel, Bonus(StatKind.FireResistance, 10), weaponDamage: null, _rules);

        // 5 % from 20 Will, then the gear's 10 %; Ice has only Will's.
        Assert.Equal((150, 50), (sheet.Resistances.Fire, sheet.Resistances.Ice));
    }

    [Fact]
    public void Of_DodgeBonusPastCap_StopsAtCap()
    {
        var atLevel = ClassWith(new PrimaryStats(Power: 0, Agility: 200, Intelligence: 0, Constitution: 0, Will: 0));

        var sheet = MemberSheet.Of(atLevel, Bonus(StatKind.Dodge, 20), weaponDamage: null, _rules);

        // 40 % from 200 Agility and the gear's 20 % would be 60 %; the cap is 50 %.
        Assert.Equal(500, sheet.DodgePermille);
    }

    [Fact]
    public void Of_DodgePenaltyOverCap_CapsOnceAfterGear()
    {
        var atLevel = ClassWith(new PrimaryStats(Power: 0, Agility: 300, Intelligence: 0, Constitution: 0, Will: 0));

        var sheet = MemberSheet.Of(atLevel, Bonus(StatKind.Dodge, -15), weaponDamage: null, _rules);

        // 60 % from 300 Agility, less the gear's 15 %: 45 %, under the cap. Capping first would leave 35 %.
        Assert.Equal(450, sheet.DodgePermille);
    }

    [Fact]
    public void Of_MaxHpPenaltyPastTheClassHp_LeavesOne()
    {
        var atLevel = ClassWith(PrimaryStats.None);

        var sheet = MemberSheet.Of(atLevel, Bonus(StatKind.MaxHp, -100).Plus(Bonus(StatKind.Constitution, -100)), weaponDamage: null, _rules);

        Assert.Equal(1, sheet.MaxHp);
    }

    [Fact]
    public void Of_ConstitutionBonus_RaisesMaxHpThroughTheConversion()
    {
        var atLevel = ClassWith(PrimaryStats.None);

        var sheet = MemberSheet.Of(atLevel, Bonus(StatKind.Constitution, 10), weaponDamage: null, _rules);

        Assert.Equal(220, sheet.MaxHp);
    }

    [Fact]
    public void Of_EveryDerivedBonus_AddsToItsOwnNumber()
    {
        var atLevel = ClassWith(PrimaryStats.None);
        var worn = new StatBonuses(new Dictionary<StatKind, int>
        {
            [StatKind.MaxHp] = 11,
            [StatKind.Speed] = 2,
            [StatKind.Accuracy] = 3,
            [StatKind.CritChance] = 4,
            [StatKind.CritDamage] = 6,
            [StatKind.CrowdControlResistance] = 7,
            [StatKind.SlashingResistance] = 8,
        });

        var sheet = MemberSheet.Of(atLevel, worn, weaponDamage: null, _rules);

        // The class gives 200 HP, speed 12 and the rules' bases; each bonus lands on its own number (percents ×10).
        Assert.Equal(
            (211, 14, 930, 90, 1560, 220, 80, 0),
            (sheet.MaxHp, sheet.Speed, sheet.AccuracyPermille, sheet.CritChancePermille, sheet.CritDamagePermille,
                sheet.CrowdControlResistancePermille, sheet.Resistances.Slashing, sheet.Resistances.Blunt));
    }

    private static ClassStats ClassWith(PrimaryStats primaries) =>
        new ClassStats(MaxHp: 200, Speed: 12) { Primaries = primaries };

    private static StatBonuses Bonus(StatKind kind, int bonus) => new(new Dictionary<StatKind, int> { [kind] = bonus });
}
