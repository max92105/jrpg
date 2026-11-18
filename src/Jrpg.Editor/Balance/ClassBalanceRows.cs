using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Core.Stats;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Balance;

/// <summary>
/// The balance table's class rows (decision 992): each class's max HP and Power at the table's levels, a member of the
/// class at each level wearing nothing (Core's <see cref="ClassSheet"/>, decision 948).
/// </summary>
internal static class ClassBalanceRows
{
    /// <returns>The classes in id order (the catalog keeps no file order).</returns>
    /// <param name="content">The catalog whose classes and stat rules the rows read.</param>
    /// <param name="levels">The table's levels in the boxes' order, each from 1.</param>
    public static IReadOnlyList<ClassBalanceRow> Of(ContentCatalog content, IReadOnlyList<int> levels) =>
    [
        .. content.Classes.Values
            .OrderBy(characterClass => characterClass.Id, StringComparer.Ordinal)
            .Select(characterClass => new ClassBalanceRow(
                characterClass.Name,
                [.. levels.Select(level => HpAndPowerText(ClassSheet.At(characterClass, level, content.Rules.Stats)))])),
    ];

    private static string HpAndPowerText(StatSheet sheet) =>
        $"{TypedNumber.Text(sheet.MaxHp)} / {TypedNumber.Text(sheet.Primaries.Power)}";
}
