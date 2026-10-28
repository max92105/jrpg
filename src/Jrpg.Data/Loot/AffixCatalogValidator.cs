using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Data.Content;
using Jrpg.Data.Items;
using static Jrpg.Data.Content.RangeProblems;

namespace Jrpg.Data.Loot;

/// <summary>
/// Checks <c>affixes.json</c>, the pool dropped gear rolls on (ADR 0030): each affix rolls a range of nonzero values
/// within an item bonus's bound, on at least one kind of gear, with a weight of at least 1. An unknown stat or kind
/// name, or a null kind, is refused when the file is read.
/// </summary>
internal static class AffixCatalogValidator
{
    public static void Validate(string fileName, IReadOnlyList<AffixDefinition> affixes) =>
        new ContentListValidator<AffixDefinition>(fileName, "affix", affix => affix.Id)
            .Validate(affixes, FindProblem);

    private static string? FindProblem(AffixDefinition affix) =>
        FindValueProblem("min", affix.Min)
            ?? FindValueProblem("max", affix.Max)
            ?? (affix.Min > affix.Max ? $"min must be at most max (was {affix.Min} and {affix.Max})." : null)
            ?? (affix.Min < 0 && affix.Max > 0 ? $"min and max must be on the same side of 0, or a roll could add nothing (was {affix.Min} and {affix.Max})." : null)
            ?? FindKindsProblem(affix.Kinds)
            ?? AtLeast("weight", affix.Weight, 1);

    // As an item's bonus: 0 would roll a line that adds nothing; past the bound one roll outweighs any item.
    private static string? FindValueProblem(string field, int value) =>
        value == 0 || Math.Abs(value) > ItemCatalogValidator.MostBonus
            ? $"{field} must be nonzero and within -{ItemCatalogValidator.MostBonus}..{ItemCatalogValidator.MostBonus} (was {value})."
            : null;

    private static string? FindKindsProblem(IReadOnlyList<EquipmentKind> kinds) =>
        kinds.Count == 0 ? "kinds must list at least one kind of gear." : null;
}
