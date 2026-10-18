using Jrpg.Core.Maps;

namespace Jrpg.Data.Maps;

/// <summary>Everything a map layout refers to: the other loaded content, and every map's layout, since warps lead to them.</summary>
internal sealed record MapReferences(MapContentReferences Content, IReadOnlyDictionary<string, MapLayout> Layouts);
