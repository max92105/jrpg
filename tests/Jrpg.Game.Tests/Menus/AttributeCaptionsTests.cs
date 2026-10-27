using Jrpg.Core.Stats;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Menus;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class AttributeCaptionsTests
{
    // design.md's placeholder rates.
    private static readonly StatRules _rules = new(
        HpPercentPer100Constitution: 100,
        BaseAccuracy: 90,
        BaseCritChance: 5,
        BaseDodge: 0,
        BaseCrowdControlResistance: 0,
        DamagePercentPer100Power: 100,
        CritDamagePercentPer100Power: 50,
        CritChancePer100Agility: 25,
        DodgePer100Agility: 20,
        PhysicalResistancePer100Constitution: 25,
        MagicalResistancePer100Will: 25,
        StatusPercentPer100Intelligence: 100,
        IntelligencePerExtraStatusTurn: 25);

    [Fact]
    public void Of_Primaries_SaysWhatEachGivesOverItsBase()
    {
        var primaries = new PrimaryStats(Power: 15, Agility: 20, Intelligence: 6, Constitution: 14, Will: 8);

        var captions = AttributeCaptions.Of(primaries, _rules, RepositoryScreenText.Text.Messages);

        string[] expected = ["+15 % damage", "+5 % crit, +4 % dodge", "+6 % status", "+14 % HP, +3 % phys. res.", "+2 % magic res."];
        Assert.Equal(expected, captions);
    }

    [Fact]
    public void Of_WillBelowZero_ReadsAsALoss()
    {
        // -4 Will at 25 % per 100: -1 % magic resistance, as gear that takes Will away leaves it.
        var primaries = new PrimaryStats(Power: 0, Agility: 0, Intelligence: 0, Constitution: 0, Will: -4);

        var captions = AttributeCaptions.Of(primaries, _rules, RepositoryScreenText.Text.Messages);

        Assert.Equal("-1 % magic res.", captions[4]);
        Assert.Equal("+0 % damage", captions[0]);
    }
}
