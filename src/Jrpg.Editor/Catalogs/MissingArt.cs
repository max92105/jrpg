using Jrpg.Core.Content;

namespace Jrpg.Editor.Catalogs;

/// <summary>
/// Which sheets' images and which NPC sprites the catalog names but the art folder lacks (decision 442), worked out
/// from file-exists lookups so this reads no file itself; the drawing marks their tiles and NPCs, the status names
/// them.
/// </summary>
/// <param name="SheetIds">The sheets whose image is missing.</param>
/// <param name="SpriteFiles">The field-sprite files NPC objects name that are missing.</param>
internal sealed record MissingArt(IReadOnlySet<string> SheetIds, IReadOnlySet<string> SpriteFiles)
{
    public static MissingArt None { get; } = new(new HashSet<string>(StringComparer.Ordinal), new HashSet<string>(StringComparer.Ordinal));

    /// <param name="catalog">The sheets and objects checked.</param>
    /// <param name="hasTilesheetImage">Whether a file name exists in the tilesheet art folder.</param>
    /// <param name="hasFieldSprite">Whether a file name exists in the field-sprite art folder.</param>
    public static MissingArt Of(ContentCatalog catalog, Func<string, bool> hasTilesheetImage, Func<string, bool> hasFieldSprite) => new(
        catalog.Tilesheets.Values.Where(sheet => !hasTilesheetImage(sheet.File)).Select(sheet => sheet.Id).ToHashSet(StringComparer.Ordinal),
        catalog.Objects.Values.Select(definition => definition.FieldSpriteFile).OfType<string>().Where(file => !hasFieldSprite(file)).ToHashSet(StringComparer.Ordinal));

    public bool IsSheetMissing(string sheetId) => SheetIds.Contains(sheetId);

    public bool IsSpriteMissing(string file) => SpriteFiles.Contains(file);
}
