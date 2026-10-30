namespace Jrpg.Core.Trade;

/// <summary>
/// One shop as content describes it (ADR 0031): its name and the items it sells, in the order it lists them.
/// </summary>
/// <param name="Id">The shop's id, which an <c>openShop</c> step names.</param>
/// <param name="Name">The shop's name, as the screen shows it.</param>
/// <param name="ItemIds">What it sells; every one has a price.</param>
public sealed record ShopDefinition(string Id, string Name, IReadOnlyList<string> ItemIds)
{
    // A record compares a list by reference; two shops read from the same content are one shop.
    public bool Equals(ShopDefinition? other) =>
        other is not null && (Id, Name) == (other.Id, other.Name) && ItemIds.SequenceEqual(other.ItemIds);

    public override int GetHashCode() => HashCode.Combine(Id, Name, ItemIds.Count);
}
