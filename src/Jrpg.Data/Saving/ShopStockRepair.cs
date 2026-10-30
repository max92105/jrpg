using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Core.Trade;

namespace Jrpg.Data.Saving;

/// <summary>
/// The shops' half of <see cref="SaveGameRepair"/> (decision 385): the stock of a shop that no longer exists is
/// dropped; a piece whose base is gone or no longer gear, or that carries an affix the pool no longer has, is dropped;
/// an affix value out of range is clamped (<see cref="GearRepair"/>'s rule, decision 360). Each change is reported.
/// </summary>
internal sealed class ShopStockRepair
{
    private readonly ContentCatalog _catalog;
    private readonly GearRepair _gear;
    private readonly List<string> _changes;

    /// <param name="catalog">The content the save must fit.</param>
    /// <param name="gear">Clamps the kept pieces' affixes, as it does the owned ones'.</param>
    /// <param name="changes">The repair's report, which this adds to.</param>
    public ShopStockRepair(ContentCatalog catalog, GearRepair gear, List<string> changes)
    {
        _catalog = catalog;
        _gear = gear;
        _changes = changes;
    }

    public List<SavedShopStock> Repair(IReadOnlyList<SavedShopStock> stocks)
    {
        var repaired = new List<SavedShopStock>();
        foreach (var stock in stocks)
        {
            if (!_catalog.Shops.ContainsKey(stock.ShopId))
            {
                _changes.Add($"shop '{stock.ShopId}': dropped its stock, since the shop no longer exists.");
                continue;
            }

            repaired.Add(stock with { Pieces = RepairPieces(stock) });
        }

        return repaired;
    }

    private List<SavedGear> RepairPieces(SavedShopStock stock)
    {
        var owner = $"shop '{stock.ShopId}'";
        var kept = new List<SavedGear>();
        foreach (var piece in stock.Pieces)
        {
            if (IsKept(piece, owner))
            {
                kept.Add(_gear.RepairAffixes(piece, owner));
            }
        }

        return kept;
    }

    // A stock piece is one of a kind and never worn, so one that no longer fits is dropped rather than mended.
    private bool IsKept(SavedGear piece, string owner)
    {
        if (!_catalog.Items.TryGetValue(piece.ItemId, out var item))
        {
            _changes.Add($"{owner}: dropped '{piece.ItemId}', which no longer exists.");
            return false;
        }

        if (item.Equipment is null)
        {
            _changes.Add($"{owner}: dropped '{piece.ItemId}', which is no longer gear.");
            return false;
        }

        if (piece.Affixes.FirstOrDefault(affix => !_catalog.Affixes.ContainsKey(affix.AffixId)) is { } unknown)
        {
            _changes.Add($"{owner}: dropped '{piece.ItemId}' with affix '{unknown.AffixId}', which no longer exists.");
            return false;
        }

        return true;
    }
}
