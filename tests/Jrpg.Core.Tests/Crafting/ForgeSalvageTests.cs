using Jrpg.Core.Crafting;
using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Crafting;

// Salvage breaks an owned piece into its rarity's materials, and only when the whole yield fits (decisions 354, 358).
public sealed class ForgeSalvageTests
{
    private static readonly ItemDefinition _scrap = new ItemDefinitionBuilder().WithId("scrap").WithMaxStack(99).AsMaterial().Build();
    private static readonly ItemDefinition _essence = new ItemDefinitionBuilder().WithId("essence").WithMaxStack(99).AsMaterial().Build();
    private static readonly ItemDefinition _sword = new ItemDefinitionBuilder().WithId("sword").AsWeapon(5).Build();

    // Each tier's yield differs from the others', so a test can tell which row was read; Unique has none.
    private static readonly CraftingRules _rules = new()
    {
        Salvage = new Dictionary<Rarity, IReadOnlyList<InventoryEntry>>
        {
            [Rarity.Common] = [new("scrap", 1)],
            [Rarity.Magic] = [new("scrap", 2)],
            [Rarity.Rare] = [new("scrap", 3), new("essence", 1)],
        },
    };

    private readonly Inventory _inventory = new();

    [Fact]
    public void TrySalvage_OwnedPiece_RemovesItAndAddsTheYield()
    {
        var piece = GearPiece.Rolled(_sword, Rarity.Magic, []);
        _inventory.AddGear(piece);

        var salvaged = NewForge().TrySalvage(piece);

        Assert.True(salvaged);
        Assert.Empty(_inventory.Gear);
        Assert.Equal(2, _inventory.QuantityOf("scrap"));
    }

    // A worn piece isn't in the inventory, so it is as unowned as this one (decision 369).
    [Fact]
    public void TrySalvage_NotOwned_ReturnsFalseAndChangesNothing()
    {
        _inventory.AddGear(GearPiece.Plain(_sword));

        var salvaged = NewForge().TrySalvage(GearPiece.Rolled(_sword, Rarity.Magic, []));

        Assert.False(salvaged);
        Assert.Equal([GearPiece.Plain(_sword)], _inventory.Gear);
        Assert.Equal(0, _inventory.QuantityOf("scrap"));
    }

    // 98 Scrap and a yield of 2 would pass 99: nothing is destroyed for less than it yields (decision 358).
    [Fact]
    public void TrySalvage_WholeYieldWouldNotFit_ReturnsFalseAndKeepsAll()
    {
        var piece = GearPiece.Rolled(_sword, Rarity.Magic, []);
        _inventory.AddGear(piece);
        _inventory.Add(_scrap, 98);

        var salvaged = NewForge().TrySalvage(piece);

        Assert.False(salvaged);
        Assert.Equal([piece], _inventory.Gear);
        Assert.Equal(98, _inventory.QuantityOf("scrap"));
    }

    [Fact]
    public void TrySalvage_RarityWithoutYield_ReturnsFalseAndKeepsThePiece()
    {
        var piece = GearPiece.Rolled(_sword, Rarity.Unique, []);
        _inventory.AddGear(piece);

        var salvaged = NewForge().TrySalvage(piece);

        Assert.False(salvaged);
        Assert.Equal([piece], _inventory.Gear);
    }

    [Fact]
    public void QuoteSalvage_UniquePiece_RefusedNoYield()
    {
        var quote = NewForge().QuoteSalvage(GearPiece.Rolled(_sword, Rarity.Unique, []));

        Assert.Equal(ForgeRefusal.NoYield, quote.Refusal);
        Assert.Empty(quote.Materials);
    }

    [Fact]
    public void QuoteSalvage_RarePiece_IsRaresYield()
    {
        var quote = NewForge().QuoteSalvage(GearPiece.Rolled(_sword, Rarity.Rare, []));

        Assert.Equal([new InventoryEntry("scrap", 3), new InventoryEntry("essence", 1)], quote.Materials);
        Assert.False(quote.IsRefused);
    }

    // The Scrap fits but the Essence doesn't: every material of the yield must fit, not only the first.
    [Fact]
    public void QuoteSalvage_SecondMaterialWouldNotFit_RefusedStackFull()
    {
        _inventory.Add(_essence, 99);

        var quote = NewForge().QuoteSalvage(GearPiece.Rolled(_sword, Rarity.Rare, []));

        Assert.Equal(ForgeRefusal.StackFull, quote.Refusal);
    }

    private Forge NewForge() => new(_inventory, new Dictionary<string, ItemDefinition>
    {
        [_scrap.Id] = _scrap,
        [_essence.Id] = _essence,
        [_sword.Id] = _sword,
    }, _rules, new GearRoll([], new LootRules(new Dictionary<Rarity, RarityRule>()), new FixedSequenceRandom()));
}
