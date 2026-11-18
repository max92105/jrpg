namespace Jrpg.Editor.Progression;

/// <summary>
/// The Experience page's fill (decision 957): a first amount and a growth percent make the table's rows, the first,
/// then each the one before times (100 + growth) / 100, rounded down, the curve today's table was typed from (first
/// 100, 20 %, max level 21). An authoring aid only: the game reads the rows. Each step is worked out in a <c>long</c>,
/// the product taken before the division, so a step near the largest whole number is refused, never wrapped.
/// </summary>
internal static class ExperienceCurve
{
    /// <summary>The fewest levels a fill makes: one step, as Data needs one entry.</summary>
    public const int FewestLevels = 2;

    /// <summary>
    /// The most levels a fill makes, the genre's usual cap (decision 958); a table typed by hand has none.
    /// </summary>
    public const int MostLevels = 99;

    private const int WholePercent = 100;

    /// <returns>One amount per level step, <paramref name="maxLevel"/> - 1 of them.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The inputs have a <see cref="ProblemOf"/>.</exception>
    public static IReadOnlyList<int> Rows(int first, int growthPercent, int maxLevel) =>
        ProblemOf(first, growthPercent, maxLevel) is { } problem
            ? throw new ArgumentOutOfRangeException(null, problem)
            : [.. Steps(first, growthPercent, maxLevel).Select(amount => (int)amount)];

    /// <summary>
    /// Why the inputs make no table: the max level, the growth or the first out of range, then the first step past the
    /// largest whole number or below 1 ("Level 9 to 10 would need less than 1."), so the developer sees where; null
    /// when they make one.
    /// </summary>
    public static string? ProblemOf(int first, int growthPercent, int maxLevel)
    {
        if (maxLevel is < FewestLevels or > MostLevels)
        {
            return $"Max level must be {FewestLevels} to {MostLevels} (was {maxLevel}).";
        }

        if (growthPercent <= -WholePercent)
        {
            return $"Growth must be above -100 % (was {growthPercent}).";
        }

        return first < 1 ? $"First must be at least 1 (was {first})." : StepProblemOf(first, growthPercent, maxLevel);
    }

    private static string? StepProblemOf(int first, int growthPercent, int maxLevel)
    {
        var level = 1;
        foreach (var amount in Steps(first, growthPercent, maxLevel))
        {
            if (amount > int.MaxValue)
            {
                return $"{ExperienceRowViewModel.LabelOf(level)} would need more than {int.MaxValue}.";
            }

            if (amount < 1)
            {
                return $"{ExperienceRowViewModel.LabelOf(level)} would need less than 1.";
            }

            level++;
        }

        return null;
    }

    // Lazily, so the problem check stops at the first step that breaks: each step taken from one within int, times a
    // factor of at most int.MaxValue + 100, fits a long.
    private static IEnumerable<long> Steps(int first, int growthPercent, int maxLevel)
    {
        long amount = first;
        for (var level = 1; level < maxLevel; level++)
        {
            yield return amount;
            amount = amount * (WholePercent + (long)growthPercent) / WholePercent;
        }
    }
}
