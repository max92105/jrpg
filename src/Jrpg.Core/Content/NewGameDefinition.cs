using Jrpg.Core.Items;
using Jrpg.Core.Maps;

namespace Jrpg.Core.Content;

/// <summary>The state a new playthrough starts from.</summary>
public sealed record NewGameDefinition
{
    public required IReadOnlyList<string> RecruitedMemberIds { get; init; }

    /// <summary>The active party in order; the first is the leader.</summary>
    public required IReadOnlyList<string> ActiveMemberIds { get; init; }

    public required IReadOnlyList<InventoryEntry> StartingItems { get; init; }

    public required MapLocation StartLocation { get; init; }
}
