namespace Jrpg.Core.Loot;

/// <summary>
/// One item a drop table may give; it rolls on its own, apart from the table's other entries (decision 343).
/// </summary>
/// <param name="ItemId">The item dropped.</param>
/// <param name="Chance">In whole percents, as an enemy's crit chance.</param>
public sealed record DropEntry(string ItemId, int Chance)
{
    /// <summary>The fewest copies a drop gives; content leaves it out for a single copy.</summary>
    public int MinQuantity { get; init; } = 1;

    /// <summary>The most copies a drop gives; content leaves it out for a single copy.</summary>
    public int MaxQuantity { get; init; } = 1;
}
