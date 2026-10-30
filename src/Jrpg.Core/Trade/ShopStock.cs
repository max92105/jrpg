using Jrpg.Core.Items;

namespace Jrpg.Core.Trade;

/// <summary>One shop's goods as the party finds them: the stacks it sells, in the shop's order.</summary>
public sealed class ShopStock
{
    /// <param name="definition">The shop as content describes it.</param>
    /// <param name="stacks">The items sold by the copy, in the shop's order.</param>
    public ShopStock(ShopDefinition definition, IReadOnlyList<ItemDefinition> stacks)
    {
        Definition = definition;
        Stacks = stacks;
    }

    public ShopDefinition Definition { get; }

    public IReadOnlyList<ItemDefinition> Stacks { get; }
}
