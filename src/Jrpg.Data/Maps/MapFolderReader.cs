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

    public static IReadOnlyList<MapDefinition> Read(string contentRoot, MapReferences references)
    {
        var layoutIds = MapIdsIn(contentRoot, LayoutsFolder);
        var settingsIds = MapIdsIn(contentRoot, SettingsFolder);
        var layoutWithoutSettings = layoutIds.Except(settingsIds).FirstOrDefault();
        if (layoutWithoutSettings is not null)
        {
            throw new ContentValidationException(
                $"{LayoutsFolder}/{layoutWithoutSettings}.json: map '{layoutWithoutSettings}' has no settings file in {SettingsFolder}/.");
        }

        var settingsWithoutLayout = settingsIds.Except(layoutIds).FirstOrDefault();
        if (settingsWithoutLayout is not null)
        {
            throw new ContentValidationException(
                $"{SettingsFolder}/{settingsWithoutLayout}.json: map '{settingsWithoutLayout}' has no layout in {LayoutsFolder}/; run the content converter.");
        }

        return [.. layoutIds.Select(id => ReadMap(contentRoot, id, references))];
    }

    private static MapDefinition ReadMap(string contentRoot, string id, MapReferences references)
    {
        var layoutFile = $"{LayoutsFolder}/{id}.json";
        var layout = ContentFileReader.Read<MapLayout>(Path.Combine(contentRoot, layoutFile));
        MapLayoutValidator.Validate(layoutFile, layout, references);

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
