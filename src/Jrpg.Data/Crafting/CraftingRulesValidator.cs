using System.Text.Json;
using Jrpg.Core.Crafting;
using Jrpg.Core.Items;
using Jrpg.Data.Content;
using static Jrpg.Data.Content.RangeProblems;

namespace Jrpg.Data.Crafting;

/// <summary>
/// Checks <c>rules.json</c> <c>crafting</c> against the items, once they are loaded (decisions 351-354): salvage has a
/// yield for Common, Magic and Rare (Unique none, decision 339), upgradeCosts a cost for Magic and Rare, the tiers a
/// craft reaches, and rerollCost at least one material. Every list names known materials, each within 1 and its stack
/// size, and none twice: a doubled material would pass the room check entry by entry and overflow (decision 358).
/// </summary>
internal static class CraftingRulesValidator
{
    private static readonly Rarity[] _salvaged = [Rarity.Common, Rarity.Magic, Rarity.Rare];
    private static readonly Rarity[] _reached = [Rarity.Magic, Rarity.Rare];

    public static void Validate(string fileName, CraftingRules crafting, IReadOnlyDictionary<string, ItemDefinition> items)
    {
        var problem = FindTableProblem("salvage", crafting.Salvage, _salvaged, items)
            ?? FindTableProblem("upgradeCosts", crafting.UpgradeCosts, _reached, items)
            ?? FindListProblem("rerollCost", crafting.RerollCost, items);
        if (problem is not null)
        {
            throw new ContentValidationException($"{fileName}: crafting.{problem}");
        }
    }

    private static string? FindTableProblem(
        string field,
        IReadOnlyDictionary<Rarity, IReadOnlyList<InventoryEntry>> table,
        Rarity[] rarities,
        IReadOnlyDictionary<string, ItemDefinition> items)
    {
        foreach (var rarity in rarities)
        {
            if (!table.TryGetValue(rarity, out var materials))
            {
                return $"{field}.{NameOf(rarity)} is missing.";
            }

            if (FindListProblem($"{field}.{NameOf(rarity)}", materials, items) is { } problem)
            {
                return problem;
            }
        }

        return table.Keys.Except(rarities).Select(NameOf).FirstOrDefault() is { } extra
            ? $"{field}.{extra} must not be listed: a craft never works that tier."
            : null;
    }

    private static string? FindListProblem(string field, IReadOnlyList<InventoryEntry>? materials, IReadOnlyDictionary<string, ItemDefinition> items)
    {
        // The serializer doesn't apply nullable annotations to dictionary values, so a stray null gets here.
        if (materials is null || materials.Count == 0)
        {
            return $"{field} must list at least one material.";
        }

        for (var index = 0; index < materials.Count; index++)
        {
            if (materials[index] is not { } entry)
            {
                return $"{field}[{index}] is null.";
            }

            if (FindEntryProblem(entry, items) is { } problem)
            {
                return $"{field}[{index}]: {problem}";
            }
        }

        return materials.GroupBy(entry => entry.ItemId).FirstOrDefault(same => same.Count() > 1) is { } doubled
            ? $"{field} lists '{doubled.Key}' more than once."
            : null;
    }

    private static string? FindEntryProblem(InventoryEntry entry, IReadOnlyDictionary<string, ItemDefinition> items) =>
        !items.TryGetValue(entry.ItemId, out var item) ? $"itemId has unknown item id '{entry.ItemId}'."
        : !item.IsMaterial ? $"itemId '{entry.ItemId}' must be a material (isMaterial true)."
        : Within("quantity", entry.Quantity, 1, item.MaxStack);

    private static string NameOf(Rarity rarity) => JsonNamingPolicy.CamelCase.ConvertName(rarity.ToString());
}
