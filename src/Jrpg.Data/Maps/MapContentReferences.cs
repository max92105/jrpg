using Jrpg.Core.Maps;
using Jrpg.Core.Quests;

namespace Jrpg.Data.Maps;

/// <summary>The already loaded content maps refer to, apart from other maps.</summary>
internal sealed record MapContentReferences(
    IReadOnlySet<int> TileIds,
    IReadOnlyDictionary<string, ObjectDefinition> Objects,
    IReadOnlySet<string> ItemIds,
    IReadOnlySet<string> EncounterIds,
    IReadOnlySet<string> MemberIds)
{
    /// <summary>The shops an <c>openShop</c> step may name (ADR 0031); empty where no shop is loaded.</summary>
    public IReadOnlySet<string> ShopIds { get; init; } = new HashSet<string>();

    /// <summary>
    /// The quests the quest steps may name, with their stages (ADR 0032); empty where no quest is loaded.
    /// </summary>
    public IReadOnlyDictionary<string, QuestDefinition> Quests { get; init; } = new Dictionary<string, QuestDefinition>();
}
