using Jrpg.Core.Items;

namespace Jrpg.Core.Crafting;

/// <summary>
/// What crafting at the Forge gives and costs, in materials (decisions 351-354); every number is content's.
/// </summary>
public sealed record CraftingRules
{
    /// <summary>
    /// What a piece breaks into, by its rarity; a rarity with no row is never salvaged (Unique, decision 339).
    /// </summary>
    public IReadOnlyDictionary<Rarity, IReadOnlyList<InventoryEntry>> Salvage { get; init; } =
        new Dictionary<Rarity, IReadOnlyList<InventoryEntry>>();
}
