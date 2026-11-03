using Jrpg.Core.Maps;

namespace Jrpg.Editor.Tiles;

/// <summary>
/// The sheet box's list: each image in the folder with its sheets, or alone when not set up, then each sheet whose
/// image is missing.
/// </summary>
internal static class SheetChoices
{
    /// <param name="imageFiles">The readable images in the tilesheet folder, in the order the box lists them.</param>
    /// <param name="sheets">Every sheet in the catalog.</param>
    public static IReadOnlyList<SheetChoice> Of(IReadOnlyList<string> imageFiles, IEnumerable<TilesheetDefinition> sheets)
    {
        // Matched ignoring case, as Windows finds the file a sheet names whatever its case.
        var ordered = sheets.OrderBy(sheet => sheet.Id, StringComparer.Ordinal).ToList();
        var sheetsByFile = ordered.ToLookup(sheet => sheet.File, StringComparer.OrdinalIgnoreCase);
        var listedFiles = imageFiles.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var withImages = imageFiles.SelectMany(file => sheetsByFile.Contains(file)
            ? sheetsByFile[file].Select(sheet => new SheetChoice(sheet.File, sheet))
            : [new SheetChoice(file, null)]);
        var withoutImages = ordered.Where(sheet => !listedFiles.Contains(sheet.File)).Select(sheet => new SheetChoice(sheet.File, sheet, IsImageMissing: true));
        return [.. withImages, .. withoutImages];
    }
}
