using Jrpg.Core.Party;
using Jrpg.Core.Stats;

namespace Jrpg.Core.Tests.Party;

public sealed class MemberSheetTests
{
    [Fact]
    public void Of_ClassAtLevel_FillsEveryNumberFromItsOwnSource()
    {
        // Every rate and base distinct, so a number put in the wrong place can't match by chance.
        var rules = new StatRules(
            HpPercentPer100Constitution: 100,
            BaseAccuracy: 90,
            BaseCritChance: 5,
            BaseDodge: 0,
            BaseCrowdControlResistance: 15,
            DamagePercentPer100Power: 100,
            CritDamagePercentPer100Power: 50,
            CritChancePer100Agility: 25,
            DodgePer100Agility: 20);
        var primaries = new PrimaryStats(Power: 20, Agility: 20, Intelligence: 0, Constitution: 10, Will: 0);
        var atLevel = new ClassStats(MaxHp: 200, Attack: 10, Defense: 5, Speed: 12) { Accuracy = 5, Primaries = primaries };

        var sheet = MemberSheet.Of(atLevel, rules);

        var expected = new StatSheet(
            Primaries: primaries,
            MaxHp: 220,
            Speed: 12,
            AccuracyPermille: 950,
            CritChancePermille: 100,
            CritDamagePermille: 1600,
            DodgePermille: 40,
            CrowdControlResistancePermille: 150,
            DamagePermille: 1200);
        Assert.Equal(expected, sheet);
    }
}
