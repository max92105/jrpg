using Jrpg.Core.Items;
using Jrpg.Core.Loot;

namespace Jrpg.Core.Crafting;

/// <summary>
/// Works the party's owned pieces with materials (decisions 351-354). Worn pieces aren't owned, so they aren't worked
/// (decision 369). Each action is quoted first; a refused one changes nothing and draws nothing.
/// </summary>
public sealed class Forge
{
    private readonly Inventory _inventory;
    private readonly IReadOnlyDictionary<string, ItemDefinition> _items;
    private readonly CraftingRules _rules;
    private readonly GearRoll _gearRoll;

    /// <param name="inventory">The party's: where pieces are taken from and materials go.</param>
    /// <param name="items">Every item by id, to find the materials' stack sizes.</param>
    /// <param name="rules">What salvage gives and upgrades and rerolls cost.</param>
    /// <param name="gearRoll">The drops' roll, so an upgrade or a reroll draws affixes as a drop does.</param>
    internal Forge(Inventory inventory, IReadOnlyDictionary<string, ItemDefinition> items, CraftingRules rules, GearRoll gearRoll)
    {
        _inventory = inventory;
        _items = items;
        _rules = rules;
        _gearRoll = gearRoll;
    }

    /// <summary>What salvaging the piece would give: its rarity's yield.</summary>
    public ForgeQuote QuoteSalvage(GearPiece piece)
    {
        if (!_rules.Salvage.TryGetValue(piece.Rarity, out var yield))
        {
            return new ForgeQuote([], ForgeRefusal.NoYield);
        }

        // Nothing is destroyed for less than it yields (decision 358): every material must fit whole.
        var fits = yield.All(material => _inventory.HasRoomFor(_items[material.ItemId], material.Quantity));
        return new ForgeQuote(yield, fits ? null : ForgeRefusal.StackFull);
    }

    /// <summary>Breaks an owned piece into its yield.</summary>
    /// <returns>False when the piece isn't owned or its quote is refused; nothing changes then.</returns>
    public bool TrySalvage(GearPiece piece)
    {
        var quote = QuoteSalvage(piece);
        if (quote.IsRefused || !_inventory.TryRemoveGear(piece))
        {
            return false;
        }

        foreach (var material in quote.Materials)
        {
            _inventory.Add(_items[material.ItemId], material.Quantity);
        }

        return true;
    }

    /// <summary>What raising the piece one tier would cost: the cost of the tier it reaches.</summary>
    public ForgeQuote QuoteUpgrade(GearPiece piece)
    {
        if (NextTierOf(piece.Rarity) is not { } next || !_rules.UpgradeCosts.TryGetValue(next, out var cost))
        {
            return new ForgeQuote([], ForgeRefusal.HighestRarity);
        }

        var refusal = !_gearRoll.HasAffixLeftFor(piece) ? ForgeRefusal.NoAffixLeft
            : !CanPay(cost) ? ForgeRefusal.ShortOfMaterials
            : (ForgeRefusal?)null;
        return new ForgeQuote(cost, refusal);
    }

    /// <summary>
    /// Pays the cost and raises an owned piece one tier with one more affix, in its place in the list.
    /// </summary>
    /// <returns>
    /// False when the piece isn't owned or its quote is refused; nothing changes and nothing is drawn then.
    /// </returns>
    public bool TryUpgrade(GearPiece piece)
    {
        var quote = QuoteUpgrade(piece);
        if (quote.IsRefused || !_inventory.Gear.Contains(piece) || NextTierOf(piece.Rarity) is not { } next
            || _gearRoll.AddAffix(piece, next) is not { } upgraded)
        {
            return false;
        }

        Pay(quote.Materials);
        return _inventory.TryReplace(piece, upgraded);
    }

    /// <summary>What drawing every affix of the piece again would cost.</summary>
    public ForgeQuote QuoteReroll(GearPiece piece)
    {
        var cost = _rules.RerollCost;
        var refusal = piece.Affixes.Count == 0 ? ForgeRefusal.NoAffixes
            : !CanPay(cost) ? ForgeRefusal.ShortOfMaterials
            : (ForgeRefusal?)null;
        return new ForgeQuote(cost, refusal);
    }

    /// <summary>Pays the cost and draws every affix of an owned piece again, in its place in the list.</summary>
    /// <returns>
    /// False when the piece isn't owned or its quote is refused; nothing changes and nothing is drawn then.
    /// </returns>
    public bool TryReroll(GearPiece piece)
    {
        var quote = QuoteReroll(piece);
        if (quote.IsRefused || !_inventory.Gear.Contains(piece))
        {
            return false;
        }

        var rerolled = _gearRoll.Reroll(piece);
        Pay(quote.Materials);
        return _inventory.TryReplace(piece, rerolled);
    }

    // Common to Magic to Rare, one affix more each step (decision 351); Unique is never crafted (decision 339).
    private static Rarity? NextTierOf(Rarity rarity) => rarity switch
    {
        Rarity.Common => Rarity.Magic,
        Rarity.Magic => Rarity.Rare,
        _ => null,
    };

    private bool CanPay(IReadOnlyList<InventoryEntry> cost) =>
        cost.All(material => _inventory.QuantityOf(material.ItemId) >= material.Quantity);

    private void Pay(IReadOnlyList<InventoryEntry> cost)
    {
        foreach (var material in cost)
        {
            _inventory.TryRemove(material.ItemId, material.Quantity);
        }
    }
}
