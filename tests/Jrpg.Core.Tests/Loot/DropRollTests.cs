using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Loot;

// A scripted random with no values throws on any draw, so a test without values proves nothing was drawn. A chance
// draws one value below 1000: a draw below the chance in tenths of a percent drops.
public sealed class DropRollTests
{
    private static readonly ItemDefinition _potion = new ItemDefinitionBuilder().WithId("potion").Build();
    private static readonly ItemDefinition _sword = new ItemDefinitionBuilder().WithId("bronze_sword").AsWeapon(24).Build();
    private static readonly Dictionary<string, ItemDefinition> _items = new() { ["potion"] = _potion, ["bronze_sword"] = _sword };

    private static readonly LootRules _commonOnly = new(new Dictionary<Rarity, RarityRule> { [Rarity.Common] = new(Weight: 1, MinAffixes: 0, MaxAffixes: 0) });

    [Fact]
    public void Roll_CertainEntry_DropsWithoutDrawing()
    {
        var drops = Roll([Table(new DropEntry("potion", 100))], new FixedSequenceRandom());

        Assert.Equal([new ItemDrop(_potion, 1)], drops.Stacks);
    }

    [Fact]
    public void Roll_DrawBelowChance_Drops()
    {
        var drops = Roll([Table(new DropEntry("potion", 30))], new FixedSequenceRandom(299));

        Assert.Equal([new ItemDrop(_potion, 1)], drops.Stacks);
    }

    [Fact]
    public void Roll_DrawAtChance_DropsNothing()
    {
        var drops = Roll([Table(new DropEntry("potion", 30))], new FixedSequenceRandom(300));

        Assert.True(drops.IsEmpty);
    }

    // 2..4 copies, both ends included: the draw picks one of three quantities, and the highest draw gives 2 + 2.
    [Fact]
    public void Roll_QuantityRange_DrawsWithinIt()
    {
        var entry = new DropEntry("potion", 100) { MinQuantity = 2, MaxQuantity = 4 };

        var drops = Roll([Table(entry)], new FixedSequenceRandom(2));

        Assert.Equal([new ItemDrop(_potion, 4)], drops.Stacks);
    }

    [Fact]
    public void Roll_SameItemFromTwoEnemies_MergesStack()
    {
        var table = Table(new DropEntry("potion", 100));

        var drops = Roll([table, table], new FixedSequenceRandom());

        Assert.Equal([new ItemDrop(_potion, 2)], drops.Stacks);
    }

    [Fact]
    public void Roll_Equipment_DropsPlainPiece()
    {
        var drops = Roll([Table(new DropEntry("bronze_sword", 100) { MinQuantity = 2, MaxQuantity = 2 })], new FixedSequenceRandom());

        Assert.Equal([GearPiece.Plain(_sword), GearPiece.Plain(_sword)], drops.Gear);
        Assert.Empty(drops.Stacks);
    }

    // A Rare-only table and three one-value weapon affixes: the count draws 0 (two affixes), each pick draws 0.
    [Fact]
    public void Roll_Equipment_RollsThroughGearRoll()
    {
        var rareOnly = new LootRules(new Dictionary<Rarity, RarityRule> { [Rarity.Rare] = new(Weight: 1, MinAffixes: 2, MaxAffixes: 3) });
        AffixDefinition[] pool = [Affix("power", StatKind.Power), Affix("crit_chance", StatKind.CritChance), Affix("accuracy", StatKind.Accuracy)];
        var random = new FixedSequenceRandom(0, 0, 0);
        var roll = new DropRoll(_items, new GearRoll(pool, rareOnly, random), random);

        var drops = roll.Roll([Table(new DropEntry("bronze_sword", 100))], luckPermille: 0);

        var piece = Assert.Single(drops.Gear);
        Assert.Equal(Rarity.Rare, piece.Rarity);
        Assert.Equal(["power", "crit_chance"], piece.Affixes.Select(affix => affix.AffixId));
    }

    // A 30 % entry is 300 permille: 300 misses it, but +50 % luck makes it 450 (decision 349).
    [Fact]
    public void Roll_DrawJustAboveChance_DropsWithLuck()
    {
        var random = new FixedSequenceRandom(300);
        var roll = new DropRoll(_items, new GearRoll([], _commonOnly, random), random);

        var drops = roll.Roll([Table(new DropEntry("potion", 30))], luckPermille: 500);

        Assert.Equal([new ItemDrop(_potion, 1)], drops.Stacks);
    }

    // Every dropped piece rolls Common without a draw, so these tests' draws are the drop chances and quantities only.
    private static Drops Roll(IReadOnlyList<DropTable> tables, FixedSequenceRandom random) =>
        new DropRoll(_items, new GearRoll([], _commonOnly, random), random).Roll(tables, luckPermille: 0);

    private static AffixDefinition Affix(string id, StatKind stat) =>
        new() { Id = id, Stat = stat, Min = 3, Max = 3, Kinds = [EquipmentKind.Weapon], Weight = 10 };

    private static DropTable Table(params DropEntry[] entries) => new("table", entries);
}
