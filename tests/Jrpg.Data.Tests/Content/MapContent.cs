using Jrpg.Core.Content;
using Jrpg.Core.Maps;

namespace Jrpg.Data.Tests.Content;

/// <summary>
/// The content a writer test hands to <c>WriteMaps</c>: the loaded catalog with one map edited or added.
/// </summary>
internal static class MapContent
{
    /// <summary>The content with the map added, or replacing the one with its id.</summary>
    public static ContentCatalog With(ContentCatalog content, MapDefinition map) =>
        content with { Maps = new Dictionary<string, MapDefinition>(content.Maps) { [map.Id] = map } };
}
