using Jrpg.Core.Items;
using Jrpg.Core.Randomness;

namespace Jrpg.Core.Loot;

/// <summary>
/// Rolls the defeated enemies' drop tables after a victory (ADR 0030): each entry on its own, drawing only when the
/// outcome is uncertain (ADR 0026), so a certain drop or a single quantity leaves the random sequence as it was. Each
/// dropped copy of gear rolls its rarity and affixes; starting items and map gifts never come through here (decision
/// 344).
/// </summary>
/// <param name="items">Every item by id, from the catalog.</param>
/// <param name="gearRoll">Rolls each dropped copy of gear into a piece.</param>
/// <param name="random">The session's random source, the one <paramref name="gearRoll"/> draws from too.</param>
internal sealed class DropRoll(IReadOnlyDictionary<string, ItemDefinition> items, GearRoll gearRoll, IRandom random)
{
    private const int PermillePerPercent = 10;

    /// <param name="tables">One per defeated enemy that has one, in the enemies' order.</param>
    /// <returns>Stacks merged by item in first-drop order; gear as one rolled piece per copy, in drop order.</returns>
    public Drops Roll(IReadOnlyList<DropTable> tables)
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
                gear.AddRange(Enumerable.Range(0, quantity).Select(_ => gearRoll.Roll(item)));
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
