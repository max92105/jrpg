using Jrpg.Core.Maps;

namespace Jrpg.ContentConverter.Objects;

/// <summary>The object catalog, ordered by id, and the new id of each legacy int id, which legacy maps place objects by.</summary>
internal sealed record ConvertedObjects(
    IReadOnlyList<ObjectDefinition> Definitions,
    IReadOnlyDictionary<int, string> IdsByLegacyId);
