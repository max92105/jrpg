using System.Text;
using System.Text.Json.Nodes;
using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Core.Stats;
using Jrpg.Core.Statuses;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// items.json and affixes.json are saved with the other catalogs (decision 862), the items checked with everything the
// loader checks against them: the Forge's materials, the shops, the drop tables and the new game (decision 863).
public sealed class ContentWriterItemTests : IDisposable
{
    private static readonly string[] _defaultValuedFields =
        ["\"effect\"", "\"price\"", "\"effectFile\"", "\"isMaterial\"", "\"damage\"", "\"damageType\"", "\"bonuses\""];

    private readonly CopiedContentRoot _content = new("writer-items", "test-world");
    private readonly ContentCatalog _catalog;
    private readonly ContentWriter _writer;

    public ContentWriterItemTests()
    {
        _catalog = ContentCatalogLoader.Load(_content.Path);
        _writer = new ContentWriter(_content.Path);
    }

    public void Dispose() => _content.Dispose();

    [Fact]
    public void WriteMapCatalogs_ItemPriceChanged_WritesItemsJsonWithTheNewPrice()
    {
        var potion = _catalog.Items["potion"];

        _writer.WriteMapCatalogs(WithItems(potion with { Price = potion.Price + 7 }));

        Assert.Equal(potion.Price + 7, ContentCatalogLoader.Load(_content.Path).Items["potion"].Price);
    }

    [Fact]
    public void WriteMapCatalogs_ItemGrantingAStatus_WritesTheStatusId()
    {
        var bomb = _catalog.Items["bomb"];
        var effect = bomb.Effect! with { Statuses = [new StatusGrant(_catalog.Statuses["poison_1"], 2)] };

        _writer.WriteMapCatalogs(WithItems(bomb with { Effect = effect }));

        var written = JsonNode.Parse(File.ReadAllText(_content.PathOf("items.json")))!
            .AsArray()
            .Single(item => (string?)item!["id"] == "bomb")!;
        Assert.Equal("poison_1", (string?)written["effect"]!["statuses"]![0]!["status"]);
    }

    [Fact]
    public void WriteMapCatalogs_ShopItemLosesItsPrice_RefusesNamingShopsJson()
    {
        var before = ContentFileBytes();
        var sold = _catalog.Items[_catalog.Shops.Values.SelectMany(shop => shop.ItemIds).First()];

        var exception = Assert.Throws<ContentValidationException>(
            () => _writer.WriteMapCatalogs(WithItems(sold with { Price = null })));

        Assert.StartsWith("shops.json", exception.Message, StringComparison.Ordinal);
        Assert.Equal(before, ContentFileBytes());
    }

    [Fact]
    public void WriteMapCatalogs_MaterialStackBelowTheForgesCost_RefusesNamingRulesJson()
    {
        var before = ContentFileBytes();
        var cost = _catalog.Rules.Crafting.UpgradeCosts.Values
            .SelectMany(materials => materials)
            .MaxBy(entry => entry.Quantity)!;
        var material = _catalog.Items[cost.ItemId] with { MaxStack = cost.Quantity - 1 };

        var exception = Assert.Throws<ContentValidationException>(() => _writer.WriteMapCatalogs(WithItems(material)));

        Assert.StartsWith("rules.json", exception.Message, StringComparison.Ordinal);
        Assert.Equal(before, ContentFileBytes());
    }

    [Fact]
    public void WriteMapCatalogs_AffixRangeChanged_WritesAffixesJson()
    {
        var power = _catalog.Affixes["power"];

        _writer.WriteMapCatalogs(WithAffixes(power with { Max = power.Max + 1 }));

        Assert.Equal(power.Max + 1, ContentCatalogLoader.Load(_content.Path).Affixes["power"].Max);
    }

    [Fact]
    public void WriteMapCatalogs_AffixWithNoKind_RefusesNamingAffixesJson()
    {
        var before = ContentFileBytes();

        var exception = Assert.Throws<ContentValidationException>(
            () => _writer.WriteMapCatalogs(WithAffixes(_catalog.Affixes["power"] with { Kinds = [] })));

        Assert.StartsWith("affixes.json", exception.Message, StringComparison.Ordinal);
        Assert.Equal(before, ContentFileBytes());
    }

    [Fact]
    public void Write_PlainArmor_LeavesOutEveryDefaultField()
    {
        var armor = new ItemDefinition
        {
            Id = "plain_vest",
            Name = "Plain Vest",
            Description = "",
            IconFile = "Armor.png",
            MaxStack = 99,
            IsUsableFromMenu = false,
            Equipment = new EquipmentStats(EquipmentKind.Chest) { Bonuses = StatBonuses.None },
        };

        var text = Encoding.UTF8.GetString(ContentFileWriter.Serialize<ItemDefinition[]>([armor]));

        Assert.Contains("\"kind\": \"chest\"", text, StringComparison.Ordinal);
        Assert.All(_defaultValuedFields, field => Assert.DoesNotContain(field, text, StringComparison.Ordinal));
    }

    private ContentCatalog WithItems(ItemDefinition item) =>
        _catalog with { Items = new Dictionary<string, ItemDefinition>(_catalog.Items) { [item.Id] = item } };

    private ContentCatalog WithAffixes(AffixDefinition affix) =>
        _catalog with { Affixes = new Dictionary<string, AffixDefinition>(_catalog.Affixes) { [affix.Id] = affix } };

    // Every catalog file one after the other: a refused save leaves them all as they were.
    private byte[] ContentFileBytes() =>
        [.. Directory.EnumerateFiles(_content.Path, "*.json")
            .Order(StringComparer.Ordinal)
            .SelectMany(File.ReadAllBytes)];
}
