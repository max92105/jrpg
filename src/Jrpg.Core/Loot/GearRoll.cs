using Jrpg.Core.Items;
using Jrpg.Core.Randomness;

namespace Jrpg.Core.Loot;

/// <summary>
/// Rolls a dropped base item into a piece (ADR 0030): a rarity by weight, an affix count in that rarity's range, then
/// that many affixes by weight among the pool's affixes for the item's kind, never one twice (decision 359), each value
/// in its range; and, at the Forge, adds one more affix to a piece or draws all of its affixes again the same way
/// (decisions 351, 352). Every draw is skipped when only one outcome is possible, so a Common-only table leaves the
/// random untouched.
/// </summary>
internal sealed class GearRoll(IReadOnlyList<AffixDefinition> pool, LootRules rules, IRandom random)
{
    /// <param name="item">A base item that is equipment.</param>
    /// <param name="luckPermille">The party's drop luck, raising the tiers above Common (decision 349).</param>
    /// <returns>A piece with fewer affixes than its rarity asks when the pool has too few for the kind.</returns>
    /// <exception cref="ArgumentException"><paramref name="item"/> isn't equipment.</exception>
    public GearPiece Roll(ItemDefinition item, int luckPermille)
    {
        var kind = item.Equipment?.Kind ?? throw new ArgumentException($"Item '{item.Id}' is not equipment.", nameof(item));
        var rarity = RollRarity(luckPermille);
        var rule = rules.Rarities[rarity];
        var count = RangeRoll.Within(rule.MinAffixes, rule.MaxAffixes, random);
        return GearPiece.Rolled(item, rarity, DrawAffixes(kind, count, kept: []));
    }

    /// <summary>
    /// Whether the pool has an affix for the piece's kind that the piece doesn't have yet; draws nothing.
    /// </summary>
    public bool HasAffixLeftFor(GearPiece piece) => CandidatesFor(piece.Kind, piece.Affixes).Count > 0;

    /// <summary>
    /// The piece raised to <paramref name="raisedTo"/> with one more affix after its own, drawn as a drop's are
    /// (decision 351). No luck: luck is a drop's (decision 349).
    /// </summary>
    /// <returns>Null, drawing nothing, when the pool has no affix left for the piece's kind (decision 357).</returns>
    public GearPiece? AddAffix(GearPiece piece, Rarity raisedTo) =>
        HasAffixLeftFor(piece) ? GearPiece.Rolled(piece.Base, raisedTo, [.. piece.Affixes, .. DrawAffixes(piece.Kind, 1, piece.Affixes)]) : null;

    /// <summary>
    /// The piece with as many affixes as it had, each drawn again among every affix for its kind, its old ones included
    /// (decision 352); the same rarity. No luck: luck is a drop's (decision 349).
    /// </summary>
    /// <returns>Fewer affixes when the pool now has fewer for the kind than the piece held.</returns>
    public GearPiece Reroll(GearPiece piece) =>
        GearPiece.Rolled(piece.Base, piece.Rarity, DrawAffixes(piece.Kind, piece.Affixes.Count, kept: []));

    // Up to count affixes by weight among the kind's that aren't kept, never one twice, each value in its range.
    private List<RolledAffix> DrawAffixes(EquipmentKind kind, int count, IReadOnlyList<RolledAffix> kept)
    {
        var candidates = CandidatesFor(kind, kept);
        var affixes = new List<RolledAffix>();
        while (affixes.Count < count && candidates.Count > 0)
        {
            var index = WeightedPick.Pick([.. candidates.Select(affix => affix.Weight)], random);
            var affix = candidates[index];
            candidates.RemoveAt(index);
            affixes.Add(new RolledAffix(affix.Id, affix.Stat, RangeRoll.Within(affix.Min, affix.Max, random)));
        }

        return affixes;
    }

    private List<AffixDefinition> CandidatesFor(EquipmentKind kind, IReadOnlyList<RolledAffix> kept) =>
        [.. pool.Where(affix => affix.Kinds.Contains(kind) && !kept.Any(rolled => rolled.AffixId == affix.Id))];

    // The rows in rarity order, lowest first, so a draw lands on the same tier whatever order content lists them in.
    private Rarity RollRarity(int luckPermille)
    {
        var rows = rules.Rarities.OrderBy(row => row.Key).ToList();
        return rows[WeightedPick.Pick([.. rows.Select(row => LootFormula.RarityWeight(row.Value.Weight, row.Key, luckPermille))], random)].Key;
    }
}
