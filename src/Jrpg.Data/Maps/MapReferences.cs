using Jrpg.Core.Maps;

namespace Jrpg.Data.Maps;

/// <summary>The already loaded content a map layout refers to.</summary>
internal sealed record MapReferences(
    IReadOnlySet<int> TileIds,
    IReadOnlyDictionary<string, ObjectDefinition> Objects);
