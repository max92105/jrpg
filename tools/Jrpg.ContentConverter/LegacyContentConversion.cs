using Jrpg.ContentConverter.Tiles;
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

        // Everything is converted and validated before the folder is touched, so a failed run leaves the last good content.
        var generatedRoot = ReplaceGeneratedFolder(contentRoot);
        ContentFileWriter.Write(Path.Combine(contentRoot, ContentCatalogLoader.TilesFile), tiles);
        File.WriteAllText(Path.Combine(generatedRoot, ReportFile), string.Concat(report.Lines.Select(line => line + "\r\n")));
        return report;
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
