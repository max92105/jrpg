using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Data.Content;
using Jrpg.Data.Items;

namespace Jrpg.Data.Tests.Items;

// A price, when an item has one, is 1 to the purse's cap (ADR 0031); an item nobody trades has none at all.
public sealed class ItemCatalogValidatorPriceTests
{
    private static readonly ItemDefinition _potion = new()
    {
        Id = "potion",
        Name = "Potion",
        Description = "Heals.",
        IconFile = "Item.png",
        MaxStack = 99,
        IsUsableFromMenu = false,
    };

    [Theory]
    [InlineData(0)]
    [InlineData(Purse.MaxGold + 1)]
    public void Validate_PriceOutOfRange_ThrowsNamingTheField(int price)
    {
        var exception = Assert.Throws<ContentValidationException>(() => ItemCatalogValidator.Validate("items.json", [_potion with { Price = price }]));

        Assert.EndsWith($"price must be within 1..{Purse.MaxGold} (was {price}).", exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(Purse.MaxGold)]
    public void Validate_PriceAtABound_DoesNotThrow(int price)
    {
        Assert.Null(Record.Exception(() => ItemCatalogValidator.Validate("items.json", [_potion with { Price = price }])));
    }

    [Fact]
    public void Validate_NoPrice_DoesNotThrow()
    {
        Assert.Null(Record.Exception(() => ItemCatalogValidator.Validate("items.json", [_potion])));
    }
}
