namespace Jrpg.ContentConverter;

/// <summary>What converting one legacy map needs besides the map itself.</summary>
/// <param name="SourceFile">The map's path under the assets folder, such as <c>Maps/start.json</c>; messages name it.</param>
/// <param name="ObjectIdsByLegacyId">The new object ids, by the legacy int ids that placed objects and events use.</param>
/// <param name="Report">Where skipped legacy content is listed.</param>
internal sealed record MapConversionContext(
    string SourceFile,
    IReadOnlyDictionary<int, string> ObjectIdsByLegacyId,
    ConversionReport Report);
