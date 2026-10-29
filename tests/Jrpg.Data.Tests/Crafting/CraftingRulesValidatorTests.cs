using Jrpg.Core.Crafting;
using Jrpg.Core.Items;
using Jrpg.Data.Content;
using Jrpg.Data.Crafting;

namespace Jrpg.Data.Tests.Crafting;

// The fixture is the content's numbers; the valid case is ContentCatalogLoaderTests.Load_RepositoryContent_Succeeds.
public sealed class CraftingRulesValidatorTests
{
    private const string FileName = "rules.json";

    private static readonly Dictionary<string, ItemDefinition> _items = new()
    {
        ["scrap"] = Material("scrap"),
        ["essence"] = Material("essence"),
        ["potion"] = Material("potion") with { IsMaterial = false },
    };

    private static readonly CraftingRules _rules = new()
    {
        Salvage = new Dictionary<Rarity, IReadOnlyList<InventoryEntry>>
        {
            [Rarity.Common] = [new("scrap", 1)],
            [Rarity.Magic] = [new("scrap", 2)],
            [Rarity.Rare] = [new("scrap", 2), new("essence", 1)],
        },
        UpgradeCosts = new Dictionary<Rarity, IReadOnlyList<InventoryEntry>>
        {
            [Rarity.Magic] = [new("scrap", 3)],
            [Rarity.Rare] = [new("scrap", 5), new("essence", 1)],
        },
        RerollCost = [new("scrap", 2), new("essence", 1)],
    };

    [Fact]
    public void Validate_ContentNumbers_DoesNotThrow()
    {
        Assert.Null(Record.Exception(() => CraftingRulesValidator.Validate(FileName, _rules, _items)));
    }

    [Fact]
    public void Validate_SalvageRarityMissing_ThrowsNamingIt()
    {
        var rules = _rules with { Salvage = Without(_rules.Salvage, Rarity.Magic) };

        Assert.Equal("rules.json: crafting.salvage.magic is missing.", MessageOf(rules));
    }

    [Fact]
    public void Validate_UpgradeCostMissing_ThrowsNamingIt()
    {
        var rules = _rules with { UpgradeCosts = Without(_rules.UpgradeCosts, Rarity.Rare) };

        Assert.Equal("rules.json: crafting.upgradeCosts.rare is missing.", MessageOf(rules));
    }

    // Unique items are never salvaged until a plan makes them (decision 339).
    [Fact]
    public void Validate_SalvageForUnique_Throws()
    {
        var rules = _rules with { Salvage = new Dictionary<Rarity, IReadOnlyList<InventoryEntry>>(_rules.Salvage) { [Rarity.Unique] = [new("scrap", 1)] } };

        Assert.Equal("rules.json: crafting.salvage.unique must not be listed: a craft never works that tier.", MessageOf(rules));
    }

    [Fact]
    public void Validate_RerollCostEmpty_Throws()
    {
        Assert.Equal("rules.json: crafting.rerollCost must list at least one material.", MessageOf(_rules with { RerollCost = [] }));
    }

    // A list in content can hold a null the strict serializer lets through.
    [Fact]
    public void Validate_NullEntry_ThrowsNamingTheIndex()
    {
        Assert.Equal("rules.json: crafting.rerollCost[1] is null.", MessageOf(_rules with { RerollCost = [new("scrap", 2), null!] }));
    }

    [Fact]
    public void Validate_NullList_Throws()
    {
        var rules = _rules with { UpgradeCosts = new Dictionary<Rarity, IReadOnlyList<InventoryEntry>>(_rules.UpgradeCosts) { [Rarity.Magic] = null! } };

        Assert.Equal("rules.json: crafting.upgradeCosts.magic must list at least one material.", MessageOf(rules));
    }

    [Fact]
    public void Validate_UnknownItem_ThrowsNamingTheId()
    {
        Assert.Equal("rules.json: crafting.rerollCost[0]: itemId has unknown item id 'ore'.", MessageOf(_rules with { RerollCost = [new("ore", 1)] }));
    }

    [Fact]
    public void Validate_YieldNotAMaterial_Throws()
    {
        var rules = _rules with { Salvage = new Dictionary<Rarity, IReadOnlyList<InventoryEntry>>(_rules.Salvage) { [Rarity.Common] = [new("potion", 1)] } };

        Assert.Equal("rules.json: crafting.salvage.common[0]: itemId 'potion' must be a material (isMaterial true).", MessageOf(rules));
    }

    // Both ends: none at all, and more than a stack holds, which no salvage could ever fit (decision 358).
    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void Validate_QuantityOutsideTheStack_ThrowsNamingTheField(int quantity)
    {
        var expected = $"rules.json: crafting.rerollCost[0]: quantity must be within 1..99 (was {quantity}).";

        Assert.Equal(expected, MessageOf(_rules with { RerollCost = [new("scrap", quantity)] }));
    }

    [Fact]
    public void Validate_SameMaterialTwice_Throws()
    {
        var rules = _rules with { RerollCost = [new("scrap", 1), new("essence", 1), new("scrap", 1)] };

        Assert.Equal("rules.json: crafting.rerollCost lists 'scrap' more than once.", MessageOf(rules));
    }

    private static string MessageOf(CraftingRules rules) =>
        Assert.Throws<ContentValidationException>(() => CraftingRulesValidator.Validate(FileName, rules, _items)).Message;

    private static Dictionary<Rarity, IReadOnlyList<InventoryEntry>> Without(IReadOnlyDictionary<Rarity, IReadOnlyList<InventoryEntry>> table, Rarity rarity) =>
        table.Where(row => row.Key != rarity).ToDictionary();

    private static ItemDefinition Material(string id) => new()
    {
        Id = id,
        Name = id,
        Description = "A material.",
        IconFile = "Item.png",
        MaxStack = 99,
        IsUsableFromMenu = false,
        IsMaterial = true,
    };
}
