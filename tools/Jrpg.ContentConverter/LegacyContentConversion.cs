using Jrpg.ContentConverter.Maps;
using Jrpg.ContentConverter.Objects;
using Jrpg.ContentConverter.Tiles;
using Jrpg.Core.Items;
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
        var (tiles, tilesheets) = ConvertTiles(assetsRoot, report);

        var legacyObjects = LegacyFileReader.Read<IReadOnlyList<LegacyObjectJson>>(assetsRoot, ObjectConversion.SourceFile);
        var objects = ObjectConversion.Convert(legacyObjects, report);
        var definitions = objects.Definitions;
        ObjectCatalogValidator.Validate(ContentCatalogLoader.ObjectsFile, definitions);
        report.AddConverted(ObjectConversion.SourceFile, $"{definitions.Count} objects");

        var maps = ConvertMaps(assetsRoot, objects.IdsByLegacyId, report);
        // Only layouts are checked here; the encounter ids matter for the hand-written settings, which the converter never reads.
        var content = new MapContentReferences(
            tiles.Select(tile => tile.Id).ToHashSet(),
            definitions.ToDictionary(definition => definition.Id),
            ReadItemIds(contentRoot),
            EncounterIds: new HashSet<string>());
        var references = new MapReferences(content, maps.ToDictionary(map => map.Id));
        foreach (var map in maps)
        {
            MapLayoutValidator.Validate(MapFolderReader.LayoutFile(map.Id), map, references);
        }

        // Everything is converted and validated before the folder is touched, so a failed run leaves the last good content.
        var generatedRoot = ReplaceGeneratedFolder(contentRoot);
        ContentFileWriter.Write(Path.Combine(contentRoot, ContentCatalogLoader.TilesheetsFile), tilesheets);
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

    private static (IReadOnlyList<TileDefinition> Tiles, IReadOnlyList<TilesheetDefinition> Tilesheets) ConvertTiles(
        string assetsRoot, ConversionReport report)
    {
        var legacyTiles = LegacyFileReader.Read<Dictionary<string, LegacyTileJson>>(assetsRoot, TileConversion.SourceFile);
        var tiles = TileConversion.Convert(legacyTiles);
        report.AddConverted(TileConversion.SourceFile, $"{tiles.Count} tiles");

        var tilesheets = TilesheetConversion.Convert(
            LegacyFileReader.Read<IReadOnlyList<LegacyTilesheetJson>>(assetsRoot, TilesheetConversion.SourceFile),
            // TileConversion has already refused a tile without a sheet.
            legacyTiles.Values.Select(tile => tile.Tilesheet!),
            report);
        report.AddConverted(TilesheetConversion.SourceFile, $"{tilesheets.Count} tilesheets");

        TilesheetCatalogValidator.Validate(ContentCatalogLoader.TilesheetsFile, tilesheets);
        TileCatalogValidator.Validate(ContentCatalogLoader.TilesFile, tiles, tilesheets.Select(sheet => sheet.Id).ToHashSet());
        return (tiles, tilesheets);
    }

    /// <summary>Converts the live maps. The older ones in <c>Maps/Back/</c> use their own tile set and aren't converted (developer, slice 3c).</summary>
    private static List<MapLayout> ConvertMaps(
        string assetsRoot, IReadOnlyDictionary<int, string> objectIdsByLegacyId, ConversionReport report)
    {
        var sourceFiles = Directory.EnumerateFiles(Path.Combine(assetsRoot, MapConversion.SourceFolder), "*.json")
            .Select(path => $"{MapConversion.SourceFolder}/{Path.GetFileName(path)}")
            .Order(StringComparer.Ordinal);
        var sourceFilesById = new Dictionary<string, string>();
        var maps = new List<MapLayout>();
        foreach (var sourceFile in sourceFiles)
        {
            var context = new MapConversionContext(sourceFile, objectIdsByLegacyId, report);
            var map = MapConversion.Convert(LegacyFileReader.Read<LegacyMapJson>(assetsRoot, sourceFile), context);
            if (!sourceFilesById.TryAdd(map.Id, sourceFile))
            {
                throw new ConversionException($"{sourceFilesById[map.Id]} and {sourceFile} both become map id '{map.Id}'.");
            }

            var objectEventCount = map.Objects.Sum(placed => placed.Events.Count);
            report.AddConverted(
                sourceFile,
                $"map '{map.Id}' ({map.Width}x{map.Height}, {map.Layers.Count} layers, {map.Objects.Count} objects, {objectEventCount} object events, {map.CellEvents.Count} cell events)");
            maps.Add(map);
        }

        return maps;
    }

    // Items are hand-written content; events that give one are checked against them before anything is written.
    private static HashSet<string> ReadItemIds(string contentRoot)
    {
        var itemsPath = Path.Combine(contentRoot, ContentCatalogLoader.ItemsFile);
        if (!File.Exists(itemsPath))
        {
            throw new ConversionException($"{ContentCatalogLoader.ItemsFile}: not found in the content folder; event items are checked against it.");
        }

        return [.. ContentFileReader.Read<IReadOnlyList<ItemDefinition>>(itemsPath).Select(item => item.Id)];
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
