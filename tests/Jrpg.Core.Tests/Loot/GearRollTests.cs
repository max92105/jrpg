using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Loot;

// The draws, in order: the rarity (650 Common, 280 Magic, 70 Rare out of 1000), Rare's count (2 or 3), then for each
// affix the pick among the kind's affixes left (10 each) and its value. A draw with one possible outcome is skipped.
public sealed class GearRollTests
{
    private const int MagicDraw = 650;
    private const int RareDraw = 930;

    private static readonly ItemDefinition _sword = new ItemDefinitionBuilder().WithId("bronze_sword").AsWeapon(24).Build();
    private static readonly ItemDefinition _boots = new ItemDefinitionBuilder().WithId("leather_boots").AsWorn(EquipmentKind.Feet).Build();

    private static readonly AffixDefinition[] _pool =
    [
        Affix("power", StatKind.Power, 2, 5, EquipmentKind.Weapon),
        Affix("crit_chance", StatKind.CritChance, 1, 3, EquipmentKind.Weapon),
        Affix("accuracy", StatKind.Accuracy, 2, 5, EquipmentKind.Weapon, EquipmentKind.Hands),
        Affix("dodge", StatKind.Dodge, 1, 3, EquipmentKind.Feet),
    ];

    private static readonly LootRules _rules = Rules((Rarity.Common, 650, 0, 0), (Rarity.Magic, 280, 1, 1), (Rarity.Rare, 70, 2, 3));

    // The same weights with no affixes, so a lucky roll draws its rarity only.
    private static readonly LootRules _rarityOnly = Rules((Rarity.Common, 650, 0, 0), (Rarity.Magic, 280, 0, 0), (Rarity.Rare, 70, 0, 0));

    [Fact]
    public void Roll_MagicPicked_HasOneAffix()
    {
        var piece = Roll(_sword, _rules, MagicDraw, 0, 0);

        Assert.Equal(Rarity.Magic, piece.Rarity);
        Assert.Equal([new RolledAffix("power", StatKind.Power, 2)], piece.Affixes);
    }

    [Fact]
    public void Roll_RareLowDraw_HasTwoAffixes()
    {
        var piece = Roll(_sword, _rules, RareDraw, 0, 0, 0, 0, 0);

        Assert.Equal(Rarity.Rare, piece.Rarity);
        Assert.Equal(["power", "crit_chance"], piece.Affixes.Select(affix => affix.AffixId));
    }

    // The third affix is the last weapon one left, so its pick draws nothing.
    [Fact]
    public void Roll_RareHighDraw_HasThreeAffixes()
    {
        var piece = Roll(_sword, _rules, RareDraw, 1, 0, 0, 0, 0, 0);

        Assert.Equal(["power", "crit_chance", "accuracy"], piece.Affixes.Select(affix => affix.AffixId));
    }

    [Fact]
    public void Roll_CommonOnly_DrawsNothingAndIsPlain()
    {
        var piece = Roll(_sword, Rules((Rarity.Common, 1, 0, 0)));

        Assert.Equal(GearPiece.Plain(_sword), piece);
    }

    // Boots roll among the feet's affixes only: dodge, the one there, so its pick draws nothing.
    [Fact]
    public void Roll_Affixes_OnlyThoseForTheKind()
    {
        var piece = Roll(_boots, Rules((Rarity.Magic, 1, 1, 1)), 0);

        Assert.Equal("dodge", Assert.Single(piece.Affixes).AffixId);
    }

    // The second pick is among the two weapon affixes left: power, taken first, isn't among them.
    [Fact]
    public void Roll_SameAffix_NeverTwice()
    {
        var piece = Roll(_sword, Rules((Rarity.Rare, 1, 2, 2)), 0, 0, 0, 0);

        Assert.Equal(["power", "crit_chance"], piece.Affixes.Select(affix => affix.AffixId));
    }

    [Fact]
    public void Roll_PoolSmallerThanCount_RollsWhatItHas()
    {
        var piece = Roll(_boots, Rules((Rarity.Rare, 1, 3, 3)), 0);

        Assert.Single(piece.Affixes);
    }

    [Fact]
    public void Roll_LowestDraw_TakesMin()
    {
        var piece = Roll(_boots, Rules((Rarity.Magic, 1, 1, 1)), 0);

        Assert.Equal(1, Assert.Single(piece.Affixes).Value);
    }

    [Fact]
    public void Roll_HighestDraw_TakesMax()
    {
        var piece = Roll(_boots, Rules((Rarity.Magic, 1, 1, 1)), 2);

        Assert.Equal(3, Assert.Single(piece.Affixes).Value);
    }

    // Listed Rare, Common, Magic: unsorted, 650 would land on Common; in rarity order it is Magic's first value.
    [Fact]
    public void Roll_RowsListedOutOfOrder_RollsInRarityOrder()
    {
        var rules = Rules((Rarity.Rare, 70, 2, 3), (Rarity.Common, 650, 0, 0), (Rarity.Magic, 280, 1, 1));

        var piece = Roll(_sword, rules, MagicDraw, 0, 0);

        Assert.Equal(Rarity.Magic, piece.Rarity);
    }

    // +50 % luck makes 650/420/105, total 1175: 1100 lands on Rare. Without luck the total is 1000, below the draw.
    [Fact]
    public void Roll_DrawPastUnluckyTotal_IsRareWithLuck()
    {
        var piece = new GearRoll(_pool, _rarityOnly, new FixedSequenceRandom(1100)).Roll(_sword, luckPermille: 500);

        Assert.Equal(Rarity.Rare, piece.Rarity);
    }

    // Common keeps its 650 under luck (decision 349), so 650 is still the first Magic draw; were Common raised, it
    // would be Common.
    [Fact]
    public void Roll_DrawJustPastCommon_IsMagicWithLuck()
    {
        var piece = new GearRoll(_pool, _rarityOnly, new FixedSequenceRandom(650)).Roll(_sword, luckPermille: 500);

        Assert.Equal(Rarity.Magic, piece.Rarity);
    }

    // The scripted random holds nothing, so the refusal comes before any draw.
    [Fact]
    public void Roll_NotEquipment_Throws()
    {
        var potion = new ItemDefinitionBuilder().WithId("potion").Build();

        Assert.Throws<ArgumentException>(() => Roll(potion, _rules));
    }

    private static GearPiece Roll(ItemDefinition item, LootRules rules, params int[] draws) =>
        new GearRoll(_pool, rules, new FixedSequenceRandom(draws)).Roll(item, luckPermille: 0);

    private static LootRules Rules(params (Rarity Rarity, int Weight, int Min, int Max)[] rows) =>
        new(rows.ToDictionary(row => row.Rarity, row => new RarityRule(row.Weight, row.Min, row.Max)));

    private static AffixDefinition Affix(string id, StatKind stat, int min, int max, params EquipmentKind[] kinds) =>
        new() { Id = id, Stat = stat, Min = min, Max = max, Kinds = kinds, Weight = 10 };
}
