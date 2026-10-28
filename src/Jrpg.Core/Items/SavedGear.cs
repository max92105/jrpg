namespace Jrpg.Core.Items;

/// <summary>
/// A gear piece as a save keeps it (ADR 0029): its base item's id, its rarity and its affixes in roll order.
/// </summary>
public sealed record SavedGear(string ItemId, Rarity Rarity, IReadOnlyList<SavedAffix> Affixes)
{
    // A record compares a list by reference; two saved pieces with the same affixes in the same order are one piece.
    public bool Equals(SavedGear? other) =>
        other is not null && (ItemId, Rarity) == (other.ItemId, other.Rarity) && Affixes.SequenceEqual(other.Affixes);

    public override int GetHashCode() => HashCode.Combine(ItemId, Rarity, Affixes.Count);
}
