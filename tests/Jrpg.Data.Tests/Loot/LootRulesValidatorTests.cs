using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Data.Content;
using Jrpg.Data.Loot;

namespace Jrpg.Data.Tests.Loot;

public sealed class LootRulesValidatorTests
{
    private static readonly Dictionary<Rarity, RarityRule> _table = new()
    {
        [Rarity.Common] = new(Weight: 650, MinAffixes: 0, MaxAffixes: 0),
        [Rarity.Magic] = new(Weight: 280, MinAffixes: 1, MaxAffixes: 1),
        [Rarity.Rare] = new(Weight: 70, MinAffixes: 2, MaxAffixes: 3),
    };

    public static TheoryData<Rarity, RarityRule, string> InvalidRows => new()
    {
        { Rarity.Magic, new(Weight: -1, MinAffixes: 1, MaxAffixes: 1), "magic.weight must be at least 0 (was -1)." },
        { Rarity.Rare, new(Weight: 70, MinAffixes: 3, MaxAffixes: 2), "rare.maxAffixes must be within 3..5 (was 2)." },
        { Rarity.Rare, new(Weight: 70, MinAffixes: 2, MaxAffixes: 6), "rare.maxAffixes must be within 2..5 (was 6)." },
        { Rarity.Common, new(Weight: 650, MinAffixes: 0, MaxAffixes: 1), "common.maxAffixes must be within 0..0 (was 1)." },
        { Rarity.Unique, new(Weight: 1, MinAffixes: 3, MaxAffixes: 3), "unique must not be listed: unique items never roll." },
    };

    [Fact]
    public void Validate_DesignTable_DoesNotThrow()
    {
        var exception = Record.Exception(() => LootRulesValidator.Validate("rules.json", new LootRules(_table)));

        Assert.Null(exception);
    }

    [Theory]
    [MemberData(nameof(InvalidRows))]
    public void Validate_InvalidRow_ThrowsNamingTheRarityAndField(Rarity rarity, RarityRule rule, string problem)
    {
        var table = new Dictionary<Rarity, RarityRule>(_table) { [rarity] = rule };

        var exception = Assert.Throws<ContentValidationException>(() => LootRulesValidator.Validate("rules.json", new LootRules(table)));

        Assert.Equal($"rules.json: loot.rarities.{problem}", exception.Message);
    }

    [Fact]
    public void Validate_MissingRarity_Throws()
    {
        var table = new Dictionary<Rarity, RarityRule>(_table);
        table.Remove(Rarity.Magic);

        var exception = Assert.Throws<ContentValidationException>(() => LootRulesValidator.Validate("rules.json", new LootRules(table)));

        Assert.Equal("rules.json: loot.rarities.magic is missing.", exception.Message);
    }

    [Fact]
    public void Validate_NullRow_Throws()
    {
        var table = new Dictionary<Rarity, RarityRule>(_table) { [Rarity.Rare] = null! };

        var exception = Assert.Throws<ContentValidationException>(() => LootRulesValidator.Validate("rules.json", new LootRules(table)));

        Assert.Equal("rules.json: loot.rarities.rare is null.", exception.Message);
    }

    [Fact]
    public void Validate_EveryWeightZero_Throws()
    {
        var table = _table.ToDictionary(row => row.Key, row => row.Value with { Weight = 0 });

        var exception = Assert.Throws<ContentValidationException>(() => LootRulesValidator.Validate("rules.json", new LootRules(table)));

        Assert.Equal("rules.json: loot.rarities.weight must be above 0 for at least one rarity.", exception.Message);
    }
}
