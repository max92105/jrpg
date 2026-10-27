using Jrpg.Core.Stats;

namespace Jrpg.Core.Tests.Stats;

public sealed class StatSheetTests
{
    // Every field a different number, so a kind read from the wrong field fails.
    private static readonly StatSheet _sheet = new(
        Primaries: new PrimaryStats(Power: 1, Agility: 2, Intelligence: 3, Constitution: 4, Will: 5),
        MaxHp: 6,
        Speed: 7,
        AccuracyPermille: 8,
        CritChancePermille: 9,
        CritDamagePermille: 10,
        DodgePermille: 11,
        CrowdControlResistancePermille: 12,
        DamagePermille: 13,
        Resistances: new Resistances(Blunt: 14, Piercing: 15, Slashing: 16, Fire: 17, Ice: 18, Lightning: 19, Light: 20, Dark: 21),
        StatusPermille: 22,
        ExtraStatusTurns: 23);

    [Theory]
    [InlineData(StatKind.Power, 1)]
    [InlineData(StatKind.Agility, 2)]
    [InlineData(StatKind.Intelligence, 3)]
    [InlineData(StatKind.Constitution, 4)]
    [InlineData(StatKind.Will, 5)]
    [InlineData(StatKind.MaxHp, 6)]
    [InlineData(StatKind.Speed, 7)]
    [InlineData(StatKind.Accuracy, 8)]
    [InlineData(StatKind.CritChance, 9)]
    [InlineData(StatKind.CritDamage, 10)]
    [InlineData(StatKind.Dodge, 11)]
    [InlineData(StatKind.CrowdControlResistance, 12)]
    [InlineData(StatKind.BluntResistance, 14)]
    [InlineData(StatKind.PiercingResistance, 15)]
    [InlineData(StatKind.SlashingResistance, 16)]
    [InlineData(StatKind.FireResistance, 17)]
    [InlineData(StatKind.IceResistance, 18)]
    [InlineData(StatKind.LightningResistance, 19)]
    [InlineData(StatKind.LightResistance, 20)]
    [InlineData(StatKind.DarkResistance, 21)]
    public void ValueOf_EveryKind_ReadsItsField(StatKind kind, int expected)
    {
        Assert.Equal(expected, _sheet.ValueOf(kind));
    }
}
