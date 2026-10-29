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

    /// <summary>
    /// What an upgrade costs, by the tier it reaches (Magic, Rare); a tier with no row is never reached.
    /// </summary>
    public IReadOnlyDictionary<Rarity, IReadOnlyList<InventoryEntry>> UpgradeCosts { get; init; } =
        new Dictionary<Rarity, IReadOnlyList<InventoryEntry>>();

    /// <summary>What a reroll costs, whatever the piece's rarity.</summary>
    public IReadOnlyList<InventoryEntry> RerollCost { get; init; } = [];
}
