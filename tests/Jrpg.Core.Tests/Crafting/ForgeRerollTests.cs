using Jrpg.Core.Crafting;
using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Crafting;

// A reroll draws every affix of an owned piece again for one cost (decision 352). Two weapon affixes of weight 10: a
// draw of 10 picks the crit chance.
public sealed class ForgeRerollTests
{
    private static readonly ItemDefinition _scrap = new ItemDefinitionBuilder().WithId("scrap").AsMaterial().Build();
    private static readonly ItemDefinition _essence = new ItemDefinitionBuilder().WithId("essence").AsMaterial().Build();
    private static readonly ItemDefinition _sword = new ItemDefinitionBuilder().WithId("sword").AsWeapon(5).Build();
    private static readonly GearPiece _magic = GearPiece.Rolled(_sword, Rarity.Magic, [new RolledAffix("power", StatKind.Power, 4)]);

    private static readonly AffixDefinition[] _pool =
    [
        new() { Id = "power", Stat = StatKind.Power, Min = 2, Max = 5, Kinds = [EquipmentKind.Weapon], Weight = 10 },
        new() { Id = "crit_chance", Stat = StatKind.CritChance, Min = 1, Max = 3, Kinds = [EquipmentKind.Weapon], Weight = 10 },
    ];

    private static readonly CraftingRules _rules = new() { RerollCost = [new("scrap", 2), new("essence", 1)] };

    private readonly Inventory _inventory = new();

    [Fact]
    public void TryReroll_WithMaterials_NewAffixesAndCostTaken()
    {
        _inventory.AddGear(_magic);
        _inventory.Add(_scrap, 2);
        _inventory.Add(_essence, 1);

        var rerolled = NewForge(10, 0).TryReroll(_magic);

        Assert.True(rerolled);
        Assert.Equal([GearPiece.Rolled(_sword, Rarity.Magic, [new RolledAffix("crit_chance", StatKind.CritChance, 1)])], _inventory.Gear);
        Assert.Equal((0, 0), (_inventory.QuantityOf("scrap"), _inventory.QuantityOf("essence")));
    }

    // The quote names the cost even when refused, so the screen can show what it would take.
    [Fact]
    public void QuoteReroll_PlainPiece_RefusedNoAffixes()
    {
        var quote = NewForge().QuoteReroll(GearPiece.Plain(_sword));

        Assert.Equal(ForgeRefusal.NoAffixes, quote.Refusal);
        Assert.Equal(_rules.RerollCost, quote.Materials);
    }

    // Enough Scrap but no Essence: nothing is paid and, the scripted random being empty, nothing is drawn.
    [Fact]
    public void TryReroll_ShortOfMaterials_ReturnsFalseAndKeepsAll()
    {
        _inventory.AddGear(_magic);
        _inventory.Add(_scrap, 2);
        var forge = NewForge();

        var rerolled = forge.TryReroll(_magic);

        Assert.False(rerolled);
        Assert.Equal(ForgeRefusal.ShortOfMaterials, forge.QuoteReroll(_magic).Refusal);
        Assert.Equal([_magic], _inventory.Gear);
        Assert.Equal(2, _inventory.QuantityOf("scrap"));
    }

    // A worn piece isn't owned (decision 369).
    [Fact]
    public void TryReroll_NotOwned_ReturnsFalseAndDrawsNothing()
    {
        _inventory.Add(_scrap, 2);
        _inventory.Add(_essence, 1);

        var rerolled = NewForge().TryReroll(_magic);

        Assert.False(rerolled);
        Assert.Equal((2, 1), (_inventory.QuantityOf("scrap"), _inventory.QuantityOf("essence")));
    }

    private Forge NewForge(params int[] draws)
    {
        var gearRoll = new GearRoll(_pool, new LootRules(new Dictionary<Rarity, RarityRule>()), new FixedSequenceRandom(draws));
        var items = new Dictionary<string, ItemDefinition> { [_scrap.Id] = _scrap, [_essence.Id] = _essence, [_sword.Id] = _sword };
        return new Forge(_inventory, items, _rules, gearRoll);
    }
}
