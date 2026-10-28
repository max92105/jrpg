using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Data.Content;
using static Jrpg.Data.Content.RangeProblems;

namespace Jrpg.Data.Loot;

/// <summary>
/// Checks <c>drop-tables.json</c> (decision 343): each table lists at least one entry, each entry a known item, a
/// chance of 1..100 % and a quantity range of at least one copy that a stackable item's stack can hold.
/// </summary>
internal static class DropTableCatalogValidator
{
    private const int WholeChance = 100;

    public static void Validate(string fileName, IReadOnlyList<DropTable> tables, IReadOnlyDictionary<string, ItemDefinition> items) =>
        new ContentListValidator<DropTable>(fileName, "drop table", table => table.Id)
            .Validate(tables, table => FindProblem(table, items));

    private static string? FindProblem(DropTable table, IReadOnlyDictionary<string, ItemDefinition> items)
    {
        if (table.Entries.Count == 0)
        {
            return "entries must list at least one item.";
        }

        for (var index = 0; index < table.Entries.Count; index++)
        {
            // The serializer doesn't apply nullable annotations to collection elements, so a stray null gets here.
            if (table.Entries[index] is not { } entry)
            {
                return $"entries[{index}] is null.";
            }

            if (FindEntryProblem(entry, items) is { } problem)
            {
                return $"entries[{index}]: {problem}";
            }
        }

        return null;
    }

    // Gear has no cap (decision 345), so only a stackable's range is held within its stack.
    private static string? FindEntryProblem(DropEntry entry, IReadOnlyDictionary<string, ItemDefinition> items)
    {
        if (!items.TryGetValue(entry.ItemId, out var item))
        {
            return $"itemId has unknown item id '{entry.ItemId}'.";
        }

        return Within("chance", entry.Chance, 1, WholeChance)
            ?? (item.Equipment is null ? FindStackRangeProblem(entry, item.MaxStack) : FindGearRangeProblem(entry));
    }

    private static string? FindStackRangeProblem(DropEntry entry, int maxStack) =>
        Within("minQuantity", entry.MinQuantity, 1, maxStack) ?? Within("maxQuantity", entry.MaxQuantity, entry.MinQuantity, maxStack);

    private static string? FindGearRangeProblem(DropEntry entry) =>
        AtLeast("minQuantity", entry.MinQuantity, 1) ?? AtLeast("maxQuantity", entry.MaxQuantity, entry.MinQuantity);
}
