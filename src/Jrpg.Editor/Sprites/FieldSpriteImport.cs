using System.IO;
using Jrpg.Data.Art;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Sprites;

/// <summary>
/// Copies images into the field-sprite art through Data, which refuses what the game couldn't use (an image too small
/// for its record); as <see cref="SheetImport"/>, a file copy and not a catalog step, so it isn't undone.
/// </summary>
/// <param name="writer">Writes into the content folder.</param>
/// <param name="catalog">The records a replacement is checked against, unsaved ones included.</param>
internal sealed class FieldSpriteImport(ContentWriter writer, CatalogDraft catalog)
{
    /// <param name="paths">The images chosen, in order.</param>
    /// <param name="confirmReplace">
    /// Asked, with the file name, before an image already in the folder is replaced (decision 125).
    /// </param>
    public SheetImportResult Import(IReadOnlyList<string> paths, Func<string, bool> confirmReplace)
    {
        var imported = new List<string>();
        var refusals = new List<string>();
        foreach (var path in paths)
        {
            var fileName = Path.GetFileName(path);
            if (writer.HasArtImage(ArtFolders.FieldSprites, fileName) && !confirmReplace(fileName))
            {
                continue;
            }

            try
            {
                imported.Add(writer.ImportArtImage(ArtFolders.FieldSprites, path, catalog.Current));
            }
            catch (Exception exception)
                when (exception is ContentValidationException or IOException or UnauthorizedAccessException)
            {
                refusals.Add(exception.Message);
            }
        }

        return new SheetImportResult(imported, refusals);
    }
}
