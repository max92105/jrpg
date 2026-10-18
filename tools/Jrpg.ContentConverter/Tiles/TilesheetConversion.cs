using Jrpg.Core.Maps;

namespace Jrpg.ContentConverter.Tiles;

/// <summary>Lists every tilesheet a tile uses, with its grid, from the legacy sheet settings.</summary>
internal static class TilesheetConversion
{
    public const string SourceFile = "Tiles/Tilesheets.json";

    // The legacy TilesheetDatabase cut a sheet without settings into MapConstant.TileSize squares.
    private const int LegacyDefaultCellPixels = 48;

    /// <summary>The sheet id for a legacy sheet file name: <c>RA_Jungle.png</c> -> <c>ra_jungle</c>.</summary>
    public static string IdOf(string file) => LegacyIds.FromName(SourceFile, Path.GetFileNameWithoutExtension(file));

    /// <param name="legacySheets">The configured sheets.</param>
    /// <param name="usedFiles">The sheet file of every tile; legacy compared file names only, ignoring case.</param>
    /// <param name="report">Gets a line per sheet given the default grid and per configured sheet no tile uses.</param>
    /// <returns>The used sheets, ordered by id.</returns>
    public static IReadOnlyList<TilesheetDefinition> Convert(
        IReadOnlyList<LegacyTilesheetJson> legacySheets, IEnumerable<string> usedFiles, ConversionReport report)
    {
        var settingsByFile = SettingsByFile(legacySheets);
        var filesById = new Dictionary<string, string>();
        var sheets = new List<TilesheetDefinition>();
        var fileNames = usedFiles.Select(Path.GetFileName).OfType<string>();
        foreach (var file in fileNames.Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.Ordinal))
        {
            var id = IdOf(file);
            if (!filesById.TryAdd(id, file))
            {
                throw new ConversionException($"{SourceFile}: sheets {filesById[id]} and {file} both become tilesheet id '{id}'.");
            }

            sheets.Add(settingsByFile.Remove(file, out var settings) ? FromSettings(id, settings) : FromLegacyDefault(id, file, report));
        }

        foreach (var unusedFile in settingsByFile.Keys.Order(StringComparer.Ordinal))
        {
            report.AddSkipped(SourceFile, $"tilesheet {unusedFile}, which no tile uses");
        }

        return [.. sheets.OrderBy(sheet => sheet.Id, StringComparer.Ordinal)];
    }

    private static Dictionary<string, LegacyTilesheetJson> SettingsByFile(IReadOnlyList<LegacyTilesheetJson> legacySheets)
    {
        var settingsByFile = new Dictionary<string, LegacyTilesheetJson>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < legacySheets.Count; index++)
        {
            // The serializer doesn't apply nullable annotations to list elements, so a stray null gets here.
            var settings = legacySheets[index] ?? throw new ConversionException($"{SourceFile}: entry {index} is null.");
            if (string.IsNullOrWhiteSpace(settings.File) || !settingsByFile.TryAdd(Path.GetFileName(settings.File), settings))
            {
                throw new ConversionException($"{SourceFile}: entry {index} has an empty or repeated file '{settings.File}'.");
            }
        }

        return settingsByFile;
    }

    private static TilesheetDefinition FromSettings(string id, LegacyTilesheetJson settings) => new()
    {
        Id = id,
        File = Path.GetFileName(settings.File),
        TileWidth = settings.TileWidth,
        TileHeight = settings.TileHeight,
        Margin = settings.Margin,
        Spacing = settings.Spacing,
    };

    private static TilesheetDefinition FromLegacyDefault(string id, string file, ConversionReport report)
    {
        report.AddChanged(SourceFile, $"tilesheet {file} to the legacy {LegacyDefaultCellPixels} px grid, since it has no settings");
        return new TilesheetDefinition
        {
            Id = id,
            File = file,
            TileWidth = LegacyDefaultCellPixels,
            TileHeight = LegacyDefaultCellPixels,
            Margin = 0,
            Spacing = 0,
        };
    }
}
