namespace Jrpg.Core.Progression;

/// <summary>
/// Experience needed per level. Entry n is what a member needs to go from level n + 1 to level n + 2,
/// so the maximum level is one past the last entry. Data rather than a formula, so per-class curves
/// can later be more tables.
/// </summary>
public sealed record ExperienceTable(IReadOnlyList<int> ExperienceToNextLevel)
{
    public int MaxLevel => ExperienceToNextLevel.Count + 1;

    public bool IsMaxLevel(int level) => level >= MaxLevel;

    public int ExperienceToReachNext(int level)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(level, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(level, MaxLevel);

        return ExperienceToNextLevel[level - 1];
    }
}
