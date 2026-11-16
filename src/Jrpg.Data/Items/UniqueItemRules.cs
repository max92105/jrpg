using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Data.Content;

namespace Jrpg.Data.Items;

/// <summary>
/// The rules a unique item's record keeps (ADR 0058): it is equipment, only it carries fixed affixes, and each fixed
/// affix names a pool affix once, with a nonzero value within an item bonus's bound on its affix's side of zero, free
/// of the affix's roll range (decision 866).
/// </summary>
internal static class UniqueItemRules
{
    /// <param name="item">One item of <c>items.json</c>, its own fields already checked.</param>
    /// <param name="pool">The affix pool by id, as <c>affixes.json</c> holds it.</param>
    /// <returns>The first rule the item breaks, or null.</returns>
    public static string? FindProblem(ItemDefinition item, IReadOnlyDictionary<string, AffixDefinition> pool) =>
        item.IsUnique && item.Equipment is null ? "isUnique needs equipment: a unique is worn (ADR 0058)."
        : !item.IsUnique && item.FixedAffixes.Count > 0 ? "fixedAffixes are only for a unique (isUnique true)."
        : FindFixedAffixProblem(item.FixedAffixes, pool);

    private static string? FindFixedAffixProblem(
        IReadOnlyList<RolledAffix> fixedAffixes, IReadOnlyDictionary<string, AffixDefinition> pool)
    {
        var named = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < fixedAffixes.Count; index++)
        {
            // The strict reader lets a null entry through, and a null line would crash the piece built from it.
            if (fixedAffixes[index] is not { } affix)
            {
                return $"fixedAffixes[{index}] is null.";
            }

            if (!named.Add(affix.AffixId))
            {
                return $"fixedAffixes names '{affix.AffixId}' twice.";
            }

            if (FindValueProblem(affix, pool) is { } problem)
            {
                return $"fixedAffixes[{index}] ({affix.AffixId}): {problem}";
            }
        }

        return null;
    }

    // Bound as an item's bonus, not by its affix's roll range, so a unique can beat a good Rare (decision 866). The
    // reader refuses an affix the pool lacks; an edited catalog may still name one its pool no longer holds.
    private static string? FindValueProblem(RolledAffix affix, IReadOnlyDictionary<string, AffixDefinition> pool)
    {
        const int bound = ItemCatalogValidator.MostBonus;
        if (!pool.TryGetValue(affix.AffixId, out var definition))
        {
            return $"{ContentCatalogLoader.AffixesFile} has no such affix.";
        }

        return affix.Value is 0 or < -bound or > bound
            ? $"value must be nonzero and within -{bound}..{bound} (was {affix.Value})."
            : FindSignProblem(affix, definition);
    }

    // A fixed line gives what its affix gives, never the opposite. The pool's own check keeps a range on one side of 0
    // and an affix-only stat's above it (AffixCatalogValidator), so the range's side settles the stat's too.
    private static string? FindSignProblem(RolledAffix affix, AffixDefinition definition) =>
        definition.Max > 0 == affix.Value > 0 ? null
        : definition.Max > 0 ? $"value must be above 0, as affix '{definition.Id}' rolls (was {affix.Value})."
        : $"value must be below 0, as affix '{definition.Id}' rolls (was {affix.Value}).";
}
