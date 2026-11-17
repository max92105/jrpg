using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Core.Stats;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Party;

/// <summary>
/// A class's numbers at every level of the draft's experience table, as the Levels tab shows them (decision 948): each
/// level read through Core's <see cref="ClassSheet"/>, so the tab shows what the game computes, or why it can't.
/// </summary>
internal static class ClassLevelPreview
{
    /// <summary>
    /// The first stat, in the page's order, whose number at <paramref name="maxLevel"/> would pass the largest or the
    /// smallest whole number: "Growth power is too large: level 21 would pass 2147483647."; null when every one fits.
    /// Worked in a long, so the class's whole-number sums never wrap; growth being linear and the base a whole number,
    /// the max level is the one level that can pass.
    /// </summary>
    public static string? ProblemOf(ClassStats baseStats, ClassStats growth, int maxLevel) =>
        StatsOf(baseStats, growth).Select(stat => ProblemAt(stat, maxLevel)).FirstOrDefault(problem => problem is not null);

    /// <summary>One row per level from 1 to the table's max level.</summary>
    /// <exception cref="InvalidOperationException">
    /// <see cref="ProblemOf"/> names a stat at the table's max level; the tab shows that problem instead.
    /// </exception>
    public static IReadOnlyList<LevelPreviewRow> RowsOf(ClassStats baseStats, ClassStats growth, GameRules rules)
    {
        if (ProblemOf(baseStats, growth, rules.Experience.MaxLevel) is { } problem)
        {
            throw new InvalidOperationException(problem);
        }

        var characterClass = new ClassDefinition("", "", "", baseStats, growth);
        return [.. Enumerable.Range(1, rules.Experience.MaxLevel).Select(level => RowAt(characterClass, level, rules))];
    }

    private static LevelPreviewRow RowAt(ClassDefinition characterClass, int level, GameRules rules)
    {
        var sheet = ClassSheet.At(characterClass, level, rules.Stats);
        var table = rules.Experience;
        return new LevelPreviewRow(
            level,
            table.IsMaxLevel(level) ? null : table.ExperienceToReachNext(level),
            sheet.MaxHp,
            sheet.Speed,
            StatFormula.WholePercentOf(sheet.AccuracyPermille),
            sheet.Primaries);
    }

    private static string? ProblemAt((string Name, int Base, int Growth) stat, int maxLevel)
    {
        var atMaxLevel = stat.Base + ((long)stat.Growth * (maxLevel - 1));
        if (atMaxLevel is >= int.MinValue and <= int.MaxValue)
        {
            return null;
        }

        var (size, bound) = atMaxLevel > int.MaxValue ? ("large", int.MaxValue) : ("small", int.MinValue);
        return $"Growth {stat.Name} is too {size}: level {maxLevel} would pass {TypedNumber.Text(bound)}.";
    }

    // The page's order: max HP, speed and accuracy, then the primaries in StatKinds' order.
    private static (string Name, int Base, int Growth)[] StatsOf(ClassStats baseStats, ClassStats growth) =>
    [
        ("max HP", baseStats.MaxHp, growth.MaxHp),
        ("speed", baseStats.Speed, growth.Speed),
        ("accuracy", baseStats.Accuracy, growth.Accuracy),
        ("power", baseStats.Primaries.Power, growth.Primaries.Power),
        ("agility", baseStats.Primaries.Agility, growth.Primaries.Agility),
        ("intelligence", baseStats.Primaries.Intelligence, growth.Primaries.Intelligence),
        ("constitution", baseStats.Primaries.Constitution, growth.Primaries.Constitution),
        ("will", baseStats.Primaries.Will, growth.Primaries.Will),
    ];
}
