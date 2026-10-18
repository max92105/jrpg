using Jrpg.Core.Maps;
using Jrpg.Data.Content;

namespace Jrpg.Data.Maps;

/// <summary>
/// Reads every map: the generated layouts and the hand-written settings (ADR 0005), paired by file name.
/// Every layout needs settings and every settings file a layout, so a renamed or deleted map fails loudly.
/// </summary>
internal static class MapFolderReader
{
    internal const string LayoutsFolder = ContentCatalogLoader.GeneratedFolder + "/maps";
    internal const string SettingsFolder = "maps";

    /// <summary>Reads all layouts before validating any, since a warp's target map must exist and contain its cell.</summary>
    public static IReadOnlyList<MapDefinition> Read(
        string contentRoot,
        IReadOnlySet<int> tileIds,
        IReadOnlyDictionary<string, ObjectDefinition> objects,
        IReadOnlySet<string> itemIds)
    {
        var mapIds = PairedMapIds(contentRoot);
        var layouts = mapIds.ToDictionary(id => id, id => ContentFileReader.Read<MapLayout>(Path.Combine(contentRoot, LayoutFile(id))));
        var references = new MapReferences(tileIds, objects, itemIds, layouts);
        return [.. mapIds.Select(id => ReadValidatedMap(contentRoot, id, references))];
    }

    internal static string LayoutFile(string mapId) => $"{LayoutsFolder}/{mapId}.json";

    private static List<string> PairedMapIds(string contentRoot)
    {
        var layoutIds = MapIdsIn(contentRoot, LayoutsFolder);
        var settingsIds = MapIdsIn(contentRoot, SettingsFolder);
        var layoutWithoutSettings = layoutIds.Except(settingsIds).FirstOrDefault();
        if (layoutWithoutSettings is not null)
        {
            throw new ContentValidationException(
                $"{LayoutFile(layoutWithoutSettings)}: map '{layoutWithoutSettings}' has no settings file in {SettingsFolder}/.");
        }

        var settingsWithoutLayout = settingsIds.Except(layoutIds).FirstOrDefault();
        if (settingsWithoutLayout is not null)
        {
            throw new ContentValidationException(
                $"{SettingsFolder}/{settingsWithoutLayout}.json: map '{settingsWithoutLayout}' has no layout in {LayoutsFolder}/; run the content converter.");
        }

        return layoutIds;
    }

    // The id comes from the file names; the layout's own id must match it, which the validator checks.
    private static MapDefinition ReadValidatedMap(string contentRoot, string id, MapReferences references)
    {
        var layout = references.Layouts[id];
        MapLayoutValidator.Validate(LayoutFile(id), layout, references);

        var settingsFile = $"{SettingsFolder}/{id}.json";
        var settings = ContentFileReader.Read<MapSettings>(Path.Combine(contentRoot, settingsFile));
        MapSettingsValidator.Validate(settingsFile, settings);
        return new MapDefinition(layout, settings);
    }

    private static List<string> MapIdsIn(string contentRoot, string folder)
    {
        var path = Path.Combine(contentRoot, folder);
        return Directory.Exists(path)
            ? [.. Directory.EnumerateFiles(path, "*.json").Select(Path.GetFileNameWithoutExtension).OfType<string>().Order(StringComparer.Ordinal)]
            : [];
    }
}
