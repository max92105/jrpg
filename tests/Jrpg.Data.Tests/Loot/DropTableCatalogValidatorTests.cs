using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Data.Content;
using Jrpg.Data.Loot;

namespace Jrpg.Data.Tests.Loot;

public sealed class DropTableCatalogValidatorTests
{
    private static readonly Dictionary<string, ItemDefinition> _items = new()
    {
        ["potion"] = Item("potion", maxStack: 99),
        ["bronze_sword"] = Item("bronze_sword", maxStack: 1) with { Equipment = new EquipmentStats(EquipmentKind.Weapon) { Damage = 24 } },
    };

    public static TheoryData<DropEntry, string> InvalidEntries => new()
    {
        { new DropEntry("megalixir", 30), "entries[0]: itemId has unknown item id 'megalixir'." },
        { new DropEntry("potion", 0), "entries[0]: chance must be within 1..100 (was 0)." },
        { new DropEntry("potion", 101), "entries[0]: chance must be within 1..100 (was 101)." },
        { new DropEntry("potion", 30) { MinQuantity = 3, MaxQuantity = 2 }, "entries[0]: maxQuantity must be within 3..99 (was 2)." },
        { new DropEntry("potion", 30) { MaxQuantity = 100 }, "entries[0]: maxQuantity must be within 1..99 (was 100)." },
        { new DropEntry("potion", 30) { MinQuantity = 0 }, "entries[0]: minQuantity must be within 1..99 (was 0)." },
        { new DropEntry("potion", 30) { MinQuantity = 100, MaxQuantity = 100 }, "entries[0]: minQuantity must be within 1..99 (was 100)." },
        { new DropEntry("bronze_sword", 6) { MinQuantity = 3, MaxQuantity = 2 }, "entries[0]: maxQuantity must be at least 3 (was 2)." },
        { new DropEntry("bronze_sword", 6) { MinQuantity = 0 }, "entries[0]: minQuantity must be at least 1 (was 0)." },
    };

    [Fact]
    public void Validate_ValidTable_DoesNotThrow()
    {
        var table = new DropTable("goblin", [new DropEntry("potion", 30) { MaxQuantity = 2 }, new DropEntry("bronze_sword", 6)]);

        var exception = Record.Exception(() => DropTableCatalogValidator.Validate("drop-tables.json", [table], _items));

        Assert.Null(exception);
    }

    // Gear has no stack to fill (decision 345): a sword whose stack holds one may still drop two.
    [Fact]
    public void Validate_GearAboveItsMaxStack_DoesNotThrow()
    {
        var table = new DropTable("goblin", [new DropEntry("bronze_sword", 6) { MaxQuantity = 2 }]);

        var exception = Record.Exception(() => DropTableCatalogValidator.Validate("drop-tables.json", [table], _items));

        Assert.Null(exception);
    }

    [Theory]
    [MemberData(nameof(InvalidEntries))]
    public void Validate_InvalidEntry_ThrowsNamingTheTableAndField(DropEntry entry, string problem)
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => DropTableCatalogValidator.Validate("drop-tables.json", [new DropTable("goblin", [entry])], _items));

        Assert.Equal($"drop-tables.json: drop table 'goblin': {problem}", exception.Message);
    }

    [Fact]
    public void Validate_NoEntries_Throws()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => DropTableCatalogValidator.Validate("drop-tables.json", [new DropTable("goblin", [])], _items));

        Assert.Equal("drop-tables.json: drop table 'goblin': entries must list at least one item.", exception.Message);
    }

    [Fact]
    public void Validate_NullEntry_Throws()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => DropTableCatalogValidator.Validate("drop-tables.json", [new DropTable("goblin", [new DropEntry("potion", 30), null!])], _items));

        Assert.Equal("drop-tables.json: drop table 'goblin': entries[1] is null.", exception.Message);
    }

    private static ItemDefinition Item(string id, int maxStack) => new()
    {
        Id = id,
        Name = id,
        Description = "A test item.",
        IconFile = "icon.png",
        MaxStack = maxStack,
        IsUsableFromMenu = false,
    };
}
