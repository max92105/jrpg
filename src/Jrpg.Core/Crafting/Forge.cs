using Jrpg.Core.Items;

namespace Jrpg.Core.Crafting;

/// <summary>
/// Works the party's owned pieces with materials (decisions 351-354). Worn pieces aren't owned, so they aren't worked
/// (decision 369). Each action is quoted first; a refused one changes nothing.
/// </summary>
public sealed class Forge
{
    private readonly Inventory _inventory;
    private readonly IReadOnlyDictionary<string, ItemDefinition> _items;
    private readonly CraftingRules _rules;

    /// <param name="inventory">The party's: where pieces are taken from and materials go.</param>
    /// <param name="items">Every item by id, to find the materials' stack sizes.</param>
    /// <param name="rules">What salvage gives.</param>
    internal Forge(Inventory inventory, IReadOnlyDictionary<string, ItemDefinition> items, CraftingRules rules)
    {
        _inventory = inventory;
        _items = items;
        _rules = rules;
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
}
