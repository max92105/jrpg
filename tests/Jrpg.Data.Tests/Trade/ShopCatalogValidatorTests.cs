using Jrpg.Core.Items;
using Jrpg.Core.Trade;
using Jrpg.Data.Content;
using Jrpg.Data.Trade;

namespace Jrpg.Data.Tests.Trade;

public sealed class ShopCatalogValidatorTests
{
    private const string FileName = "shops.json";

    private static readonly Dictionary<string, ItemDefinition> _items = new()
    {
        ["potion"] = Item("potion", price: 20),
        ["bomb"] = Item("bomb", price: 40),
        ["heirloom"] = Item("heirloom", price: null),
    };

    private static readonly ShopDefinition _store = new("general_store", "General Store", ["potion", "bomb"]);

    [Fact]
    public void Validate_ValidShop_DoesNotThrow()
    {
        Assert.Null(Record.Exception(() => ShopCatalogValidator.Validate(FileName, [_store], _items)));
    }

    public static TheoryData<ShopDefinition, string> InvalidShops => new()
    {
        { _store with { ItemIds = ["potion", "elixir"] }, "itemIds[1] has unknown item id 'elixir'." },
        { _store with { ItemIds = ["heirloom"] }, "itemIds[0] 'heirloom' has no price, so no shop can sell it." },
        { _store with { ItemIds = ["potion", "bomb", "potion"] }, "itemIds lists 'potion' more than once." },
        { _store with { ItemIds = [] }, "itemIds must list at least one item." },
        { _store with { ItemIds = ["potion", null!] }, "itemIds[1] is null." },
        { _store with { Name = " " }, "name must not be empty." },
    };

    [Theory]
    [MemberData(nameof(InvalidShops))]
    public void Validate_InvalidShop_ThrowsNamingTheShopAndTheProblem(ShopDefinition shop, string problem)
    {
        var exception = Assert.Throws<ContentValidationException>(() => ShopCatalogValidator.Validate(FileName, [shop], _items));

        Assert.StartsWith(FileName, exception.Message, StringComparison.Ordinal);
        Assert.Contains("general_store", exception.Message, StringComparison.Ordinal);
        Assert.EndsWith(problem, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_RepeatedShopId_Throws()
    {
        var exception = Assert.Throws<ContentValidationException>(() => ShopCatalogValidator.Validate(FileName, [_store, _store with { Name = "Other" }], _items));

        Assert.Contains("general_store", exception.Message, StringComparison.Ordinal);
    }

    private static ItemDefinition Item(string id, int? price) => new()
    {
        Id = id,
        Name = id,
        Description = string.Empty,
        IconFile = "Item.png",
        MaxStack = 99,
        IsUsableFromMenu = false,
        Price = price,
    };
}
