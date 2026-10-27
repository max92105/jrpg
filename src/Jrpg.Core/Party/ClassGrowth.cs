using Jrpg.Core.Stats;

namespace Jrpg.Core.Party;

/// <summary>The stats a class gives at a level: the one formula for a member's numbers (ADR 0017).</summary>
public static class ClassGrowth
{
    /// <summary>
    /// <c>base + growth × (level − 1)</c>: linear growth, the simplest rule that makes every level matter, as a D&amp;D
    /// class adds the same hit points each level (decision 164). A curve can replace it here without touching saves.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="level"/> is below 1.</exception>
    public static ClassStats StatsAt(ClassDefinition characterClass, int level)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(level, 1);

        var levelsGained = level - 1;
        var (baseStats, growth) = (characterClass.Base, characterClass.Growth);
        return new ClassStats(
            baseStats.MaxHp + (growth.MaxHp * levelsGained),
            baseStats.Defense + (growth.Defense * levelsGained),
            baseStats.Speed + (growth.Speed * levelsGained))
        {
            Accuracy = baseStats.Accuracy + (growth.Accuracy * levelsGained),
            Primaries = PrimariesAt(baseStats.Primaries, growth.Primaries, levelsGained),
        };
    }

    // The same formula, primary by primary.
    private static PrimaryStats PrimariesAt(PrimaryStats baseStats, PrimaryStats growth, int levelsGained) => new(
        baseStats.Power + (growth.Power * levelsGained),
        baseStats.Agility + (growth.Agility * levelsGained),
        baseStats.Intelligence + (growth.Intelligence * levelsGained),
        baseStats.Constitution + (growth.Constitution * levelsGained),
        baseStats.Will + (growth.Will * levelsGained));
}
