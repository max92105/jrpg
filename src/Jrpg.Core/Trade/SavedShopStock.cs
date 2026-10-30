using Jrpg.Core.Items;

namespace Jrpg.Core.Trade;

/// <summary>
/// One shop's rolled stock as a save keeps it (decision 385): the pieces its gear entries rolled into, and the leader's
/// level they were rolled at, so the next opening after a level gained rolls them again.
/// </summary>
/// <param name="ShopId">The shop in <c>shops.json</c>.</param>
/// <param name="RolledAtLevel">The leader's level when the stock rolled.</param>
/// <param name="Pieces">The rolled pieces still for sale, in the shop list's order.</param>
public sealed record SavedShopStock(string ShopId, int RolledAtLevel, IReadOnlyList<SavedGear> Pieces)
{
    // A record compares a list by reference; two saved stocks with the same pieces in the same order are one stock.
    public bool Equals(SavedShopStock? other) =>
        other is not null && (ShopId, RolledAtLevel) == (other.ShopId, other.RolledAtLevel) && Pieces.SequenceEqual(other.Pieces);

    public override int GetHashCode() => HashCode.Combine(ShopId, RolledAtLevel, Pieces.Count);
}
