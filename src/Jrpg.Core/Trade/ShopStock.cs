using Jrpg.Core.Items;

namespace Jrpg.Core.Trade;

/// <summary>
/// One shop's goods as the party finds them: the stacks it sells, in the shop's order, and the pieces its gear entries
/// rolled into (decision 385). A bought piece leaves the stock; the stacks never run out.
/// </summary>
public sealed class ShopStock
{
    private readonly List<GearPiece> _pieces;

    /// <param name="definition">The shop as content describes it.</param>
    /// <param name="stacks">The items sold by the copy, in the shop's order.</param>
    /// <param name="pieces">The rolled pieces for sale, in the shop's order; copied.</param>
    /// <param name="rolledAtLevel">The leader's level when the pieces rolled.</param>
    public ShopStock(ShopDefinition definition, IReadOnlyList<ItemDefinition> stacks, IEnumerable<GearPiece> pieces, int rolledAtLevel)
    {
        Definition = definition;
        Stacks = stacks;
        _pieces = [.. pieces];
        RolledAtLevel = rolledAtLevel;
    }

    public ShopDefinition Definition { get; }

    public IReadOnlyList<ItemDefinition> Stacks { get; }

    /// <summary>The rolled pieces still for sale.</summary>
    public IReadOnlyList<GearPiece> Pieces => _pieces;

    /// <summary>
    /// The leader's level when the pieces rolled; a higher one on opening rolls them again (decision 385).
    /// </summary>
    public int RolledAtLevel { get; }

    /// <summary>Takes the first piece equal to <paramref name="piece"/> out of the stock.</summary>
    /// <returns>False when the stock holds no such piece.</returns>
    internal bool TryTake(GearPiece piece) => _pieces.Remove(piece);
}
