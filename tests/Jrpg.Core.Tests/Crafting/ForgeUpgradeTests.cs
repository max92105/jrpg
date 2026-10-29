using Jrpg.Core.Crafting;
using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Crafting;

// An upgrade raises an owned piece one tier with one more affix, for its next tier's cost (decision 351). Two weapon
// affixes of weight 10: a pick between both draws below 20, a pick of the last one left draws nothing.
public sealed class ForgeUpgradeTests
{
    private static readonly ItemDefinition _scrap = new ItemDefinitionBuilder().WithId("scrap").AsMaterial().Build();
    private static readonly ItemDefinition _essence = new ItemDefinitionBuilder().WithId("essence").AsMaterial().Build();
    private static readonly ItemDefinition _sword = new ItemDefinitionBuilder().WithId("sword").AsWeapon(5).Build();
    private static readonly AffixDefinition _power = Affix("power", StatKind.Power, 2, 5);
    private static readonly AffixDefinition _crit = Affix("crit_chance", StatKind.CritChance, 1, 3);

    private static readonly CraftingRules _rules = new()
    {
        UpgradeCosts = new Dictionary<Rarity, IReadOnlyList<InventoryEntry>>
        {
            [Rarity.Magic] = [new("scrap", 3)],
            [Rarity.Rare] = [new("scrap", 5), new("essence", 1)],
        },
    };

    private readonly Inventory _inventory = new();

    // 10 picks the crit chance, 2 its highest value: 3.
    [Fact]
    public void TryUpgrade_CommonWithMaterials_MagicWithOneAffixAndCostTaken()
    {
        var piece = GearPiece.Plain(_sword);
        _inventory.AddGear(piece);
        _inventory.Add(_scrap, 3);

        var upgraded = NewForge([_power, _crit], 10, 2).TryUpgrade(piece);

        Assert.True(upgraded);
        Assert.Equal([GearPiece.Rolled(_sword, Rarity.Magic, [new RolledAffix("crit_chance", StatKind.CritChance, 3)])], _inventory.Gear);
        Assert.Equal(0, _inventory.QuantityOf("scrap"));
    }

    // A crafted Rare has two affixes, the old one first (decision 351); the crit chance is the only one left.
    [Fact]
    public void TryUpgrade_Magic_RareWithASecondAffix()
    {
        var piece = GearPiece.Rolled(_sword, Rarity.Magic, [new RolledAffix("power", StatKind.Power, 4)]);
        _inventory.AddGear(piece);
        _inventory.Add(_scrap, 5);
        _inventory.Add(_essence, 1);

        Assert.True(NewForge([_power, _crit], 0).TryUpgrade(piece));

        var expected = GearPiece.Rolled(_sword, Rarity.Rare, [new RolledAffix("power", StatKind.Power, 4), new RolledAffix("crit_chance", StatKind.CritChance, 1)]);
        Assert.Equal([expected], _inventory.Gear);
        Assert.Equal((0, 0), (_inventory.QuantityOf("scrap"), _inventory.QuantityOf("essence")));
    }

    // Enough Scrap but no Essence for Rare: nothing is paid, so the Scrap stays, and nothing is drawn.
    [Fact]
    public void TryUpgrade_ShortOfEssence_ReturnsFalseAndKeepsAll()
    {
        var piece = GearPiece.Rolled(_sword, Rarity.Magic, [new RolledAffix("power", StatKind.Power, 4)]);
        _inventory.AddGear(piece);
        _inventory.Add(_scrap, 5);
        var forge = NewForge([_power, _crit]);

        var upgraded = forge.TryUpgrade(piece);

        Assert.False(upgraded);
        Assert.Equal(ForgeRefusal.ShortOfMaterials, forge.QuoteUpgrade(piece).Refusal);
        Assert.Equal([piece], _inventory.Gear);
        Assert.Equal(5, _inventory.QuantityOf("scrap"));
    }

    // Rare is the top a craft reaches (decision 351); Unique is never crafted (decision 339).
    [Theory]
    [InlineData(Rarity.Rare)]
    [InlineData(Rarity.Unique)]
    public void QuoteUpgrade_RareOrUnique_RefusedHighestRarity(Rarity rarity)
    {
        var quote = NewForge([_power, _crit]).QuoteUpgrade(GearPiece.Rolled(_sword, rarity, []));

        Assert.Equal(ForgeRefusal.HighestRarity, quote.Refusal);
        Assert.Empty(quote.Materials);
    }

    // Content gives no Rare cost, so Magic is as high as a craft goes.
    [Fact]
    public void QuoteUpgrade_NoCostRowForTheNextTier_RefusedHighestRarity()
    {
        var magicOnly = new CraftingRules { UpgradeCosts = new Dictionary<Rarity, IReadOnlyList<InventoryEntry>> { [Rarity.Magic] = [new("scrap", 3)] } };
        var forge = new Forge(_inventory, new Dictionary<string, ItemDefinition>(), magicOnly, NewRoll([_power, _crit]));

        var quote = forge.QuoteUpgrade(GearPiece.Rolled(_sword, Rarity.Magic, []));

        Assert.Equal(ForgeRefusal.HighestRarity, quote.Refusal);
    }

    // The quote names the cost even when refused, so the screen can show what it would take.
    [Fact]
    public void QuoteUpgrade_Common_IsTheMagicCost()
    {
        var quote = NewForge([_power, _crit]).QuoteUpgrade(GearPiece.Plain(_sword));

        Assert.Equal([new InventoryEntry("scrap", 3)], quote.Materials);
        Assert.Equal(ForgeRefusal.ShortOfMaterials, quote.Refusal);
    }

    // Decision 357: the pool's only weapon affix is the one the piece has, so the upgrade costs nothing.
    [Fact]
    public void TryUpgrade_NoAffixLeftForKind_ReturnsFalseAndCostsNothing()
    {
        var piece = GearPiece.Rolled(_sword, Rarity.Magic, [new RolledAffix("power", StatKind.Power, 4)]);
        _inventory.AddGear(piece);
        _inventory.Add(_scrap, 5);
        _inventory.Add(_essence, 1);
        var forge = NewForge([_power]);

        var upgraded = forge.TryUpgrade(piece);

        Assert.False(upgraded);
        Assert.Equal(ForgeRefusal.NoAffixLeft, forge.QuoteUpgrade(piece).Refusal);
        Assert.Equal([piece], _inventory.Gear);
        Assert.Equal((5, 1), (_inventory.QuantityOf("scrap"), _inventory.QuantityOf("essence")));
    }

    // A worn piece isn't owned (decision 369): the scripted random holds nothing, so a draw would throw.
    [Fact]
    public void TryUpgrade_NotOwned_ReturnsFalseAndDrawsNothing()
    {
        _inventory.Add(_scrap, 3);

        var upgraded = NewForge([_power, _crit]).TryUpgrade(GearPiece.Plain(_sword));

        Assert.False(upgraded);
        Assert.Equal(3, _inventory.QuantityOf("scrap"));
    }

    private static AffixDefinition Affix(string id, StatKind stat, int min, int max) =>
        new() { Id = id, Stat = stat, Min = min, Max = max, Kinds = [EquipmentKind.Weapon], Weight = 10 };

    private static GearRoll NewRoll(AffixDefinition[] pool, params int[] draws) =>
        new(pool, new LootRules(new Dictionary<Rarity, RarityRule>()), new FixedSequenceRandom(draws));

    private Forge NewForge(AffixDefinition[] pool, params int[] draws)
    {
        var items = new Dictionary<string, ItemDefinition> { [_scrap.Id] = _scrap, [_essence.Id] = _essence, [_sword.Id] = _sword };
        return new Forge(_inventory, items, _rules, NewRoll(pool, draws));
    }
}
