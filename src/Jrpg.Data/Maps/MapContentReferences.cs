using Jrpg.Core.Maps;

namespace Jrpg.Data.Maps;

/// <summary>The already loaded content maps refer to, apart from other maps.</summary>
internal sealed record MapContentReferences(
    IReadOnlySet<int> TileIds,
    IReadOnlyDictionary<string, ObjectDefinition> Objects,
    IReadOnlySet<string> ItemIds,
    IReadOnlySet<string> EncounterIds);
