using Jrpg.Core.Maps;

namespace Jrpg.Data.Maps;

/// <summary>The other content a map layout refers to, including every map's layout, since warps lead to them.</summary>
internal sealed record MapReferences(
    IReadOnlySet<int> TileIds,
    IReadOnlyDictionary<string, ObjectDefinition> Objects,
    IReadOnlySet<string> ItemIds,
    IReadOnlyDictionary<string, MapLayout> Layouts);
