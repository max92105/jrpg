using Jrpg.Core.Items;

namespace Jrpg.Core.Loot;

/// <summary>
/// Copies of one dropped item that stacks, with the item itself so the victory screen needs no catalog.
/// </summary>
public sealed record ItemDrop(ItemDefinition Item, int Quantity);
