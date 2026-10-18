using Jrpg.ContentConverter.Maps;
using Jrpg.ContentConverter.Objects;
using Jrpg.ContentConverter.Tiles;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Data.Maps;

namespace Jrpg.ContentConverter;

/// <summary>
/// Regenerates <c>{content}/generated/</c> from the legacy <c>Assets/</c> folder (ADR 0005). The converter owns that
/// folder and nothing else: every run replaces all of it, and no other file under the content folder is touched.
/// </summary>
internal static class LegacyContentConversion
{
    private const string ReportFile = "conversion-report.txt";

    /// <exception cref="ConversionException">A legacy file can't be read or converted; nothing was written.</exception>
    /// <exception cref="ContentValidationException">The converted content breaks a content rule; nothing was written.</exception>
    public static ConversionReport Convert(string assetsRoot, string contentRoot)
    {
        var report = new ConversionReport();
        var tiles = TileConversion.Convert(
            LegacyFileReader.Read<Dictionary<string, LegacyTileJson>>(assetsRoot, TileConversion.SourceFile));
        TileCatalogValidator.Validate(ContentCatalogLoader.TilesFile, tiles);
        report.AddConverted(TileConversion.SourceFile, $"{tiles.Count} tiles");

        var legacyObjects = LegacyFileReader.Read<IReadOnlyList<LegacyObjectJson>>(assetsRoot, ObjectConversion.SourceFile);
        var objects = ObjectConversion.Convert(legacyObjects, report);
        var definitions = objects.Definitions;
        ObjectCatalogValidator.Validate(ContentCatalogLoader.ObjectsFile, definitions);
        report.AddConverted(ObjectConversion.SourceFile, $"{definitions.Count} objects");

        var references = new MapReferences(
            tiles.Select(tile => tile.Id).ToHashSet(), definitions.ToDictionary(definition => definition.Id));
        var maps = ConvertMaps(assetsRoot, objects.IdsByLegacyId, references, report);

        // Everything is converted and validated before the folder is touched, so a failed run leaves the last good content.
        var generatedRoot = ReplaceGeneratedFolder(contentRoot);
        ContentFileWriter.Write(Path.Combine(contentRoot, ContentCatalogLoader.TilesFile), tiles);
        ContentFileWriter.Write(Path.Combine(contentRoot, ContentCatalogLoader.ObjectsFile), definitions);
        Directory.CreateDirectory(Path.Combine(contentRoot, MapFolderReader.LayoutsFolder));
        foreach (var map in maps)
        {
            ContentFileWriter.Write(Path.Combine(contentRoot, MapFolderReader.LayoutsFolder, $"{map.Id}.json"), map);
        }

        File.WriteAllText(Path.Combine(generatedRoot, ReportFile), string.Concat(report.Lines.Select(line => line + "\r\n")));
        return report;
    }

    /// <summary>Converts the live maps; the older ones in <c>Maps/Back/</c> come in slice 3c.</summary>
    private static List<MapLayout> ConvertMaps(
        string assetsRoot, IReadOnlyDictionary<int, string> objectIdsByLegacyId, MapReferences references, ConversionReport report)
    {
        var sourceFiles = Directory.EnumerateFiles(Path.Combine(assetsRoot, MapConversion.SourceFolder), "*.json")
            .Select(path => $"{MapConversion.SourceFolder}/{Path.GetFileName(path)}")
            .Order(StringComparer.Ordinal);
        var sourceFilesById = new Dictionary<string, string>();
        var maps = new List<MapLayout>();
        foreach (var sourceFile in sourceFiles)
        {
            var map = MapConversion.Convert(sourceFile, LegacyFileReader.Read<LegacyMapJson>(assetsRoot, sourceFile), objectIdsByLegacyId);
            if (!sourceFilesById.TryAdd(map.Id, sourceFile))
            {
                throw new ConversionException($"{sourceFilesById[map.Id]} and {sourceFile} both become map id '{map.Id}'.");
            }

            MapLayoutValidator.Validate($"{MapFolderReader.LayoutsFolder}/{map.Id}.json", map, references);
            report.AddConverted(sourceFile, $"map '{map.Id}' ({map.Width}x{map.Height}, {map.Layers.Count} layers, {map.Objects.Count} objects)");
            maps.Add(map);
        }

        return maps;
    }

    private static string ReplaceGeneratedFolder(string contentRoot)
    {
        var generatedRoot = Path.Combine(contentRoot, ContentCatalogLoader.GeneratedFolder);
        if (Directory.Exists(generatedRoot))
        {
            Directory.Delete(generatedRoot, recursive: true);
        }

        Directory.CreateDirectory(generatedRoot);
        return generatedRoot;
    }
}
