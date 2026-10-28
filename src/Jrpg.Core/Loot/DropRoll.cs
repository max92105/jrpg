using Jrpg.Core.Items;
using Jrpg.Core.Randomness;

namespace Jrpg.Core.Loot;

/// <summary>
/// Rolls the defeated enemies' drop tables after a victory (ADR 0030): each entry on its own, drawing only when the
/// outcome is uncertain (ADR 0026), so a certain drop or a single quantity leaves the random sequence as it was.
/// </summary>
internal static class DropRoll
{
    private const int PermillePerPercent = 10;

    /// <param name="tables">One per defeated enemy that has one, in the enemies' order.</param>
    /// <param name="items">Every item by id, from the catalog.</param>
    /// <param name="random">The session's random source.</param>
    /// <returns>
    /// Stacks merged by item in first-drop order; equipment as plain pieces (decision 344, until gear rolls).
    /// </returns>
    public static Drops Roll(IReadOnlyList<DropTable> tables, IReadOnlyDictionary<string, ItemDefinition> items, IRandom random)
    {
        var rolls = new ChanceRolls(random);
        var stacks = new List<ItemDrop>();
        var gear = new List<GearPiece>();
        foreach (var entry in tables.SelectMany(table => table.Entries))
        {
            if (!rolls.Succeeds(entry.Chance * PermillePerPercent))
            {
                continue;
            }

            var item = items[entry.ItemId];
            var quantity = RangeRoll.Within(entry.MinQuantity, entry.MaxQuantity, random);
            if (item.Equipment is not null)
            {
                gear.AddRange(Enumerable.Repeat(GearPiece.Plain(item), quantity));
            }
            else
            {
                AddToStack(stacks, item, quantity);
            }
        }

        return new Drops(stacks, gear);
    }

    private static void AddToStack(List<ItemDrop> stacks, ItemDefinition item, int quantity)
    {
        var index = stacks.FindIndex(stack => stack.Item.Id == item.Id);
        if (index < 0)
        {
            stacks.Add(new ItemDrop(item, quantity));
        }
        else
        {
            stacks[index] = stacks[index] with { Quantity = stacks[index].Quantity + quantity };
        }
    }
}
