using Jrpg.Core.Items;
using Jrpg.Core.Randomness;

namespace Jrpg.Core.Loot;

/// <summary>
/// Rolls a dropped base item into a piece (ADR 0030): a rarity by weight, an affix count in that rarity's range, then
/// that many affixes by weight among the pool's affixes for the item's kind, never one twice (decision 359), each value
/// in its range. Every draw is skipped when only one outcome is possible, so a Common-only table leaves the random
/// untouched.
/// </summary>
internal sealed class GearRoll(IReadOnlyList<AffixDefinition> pool, LootRules rules, IRandom random)
{
    /// <param name="item">A base item that is equipment.</param>
    /// <returns>A piece with fewer affixes than its rarity asks when the pool has too few for the kind.</returns>
    /// <exception cref="ArgumentException"><paramref name="item"/> isn't equipment.</exception>
    public GearPiece Roll(ItemDefinition item)
    {
        var kind = item.Equipment?.Kind ?? throw new ArgumentException($"Item '{item.Id}' is not equipment.", nameof(item));
        var rarity = RollRarity();
        var rule = rules.Rarities[rarity];
        var count = RangeRoll.Within(rule.MinAffixes, rule.MaxAffixes, random);
        var candidates = pool.Where(affix => affix.Kinds.Contains(kind)).ToList();
        var affixes = new List<RolledAffix>();
        while (affixes.Count < count && candidates.Count > 0)
        {
            var index = WeightedPick.Pick([.. candidates.Select(affix => affix.Weight)], random);
            var affix = candidates[index];
            candidates.RemoveAt(index);
            affixes.Add(new RolledAffix(affix.Id, affix.Stat, RangeRoll.Within(affix.Min, affix.Max, random)));
        }

        return GearPiece.Rolled(item, rarity, affixes);
    }

    // The rows in rarity order, lowest first, so a draw lands on the same tier whatever order content lists them in.
    private Rarity RollRarity()
    {
        var rows = rules.Rarities.OrderBy(row => row.Key).ToList();
        return rows[WeightedPick.Pick([.. rows.Select(row => row.Value.Weight)], random)].Key;
    }
}
