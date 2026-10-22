using System.IO;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tiles;

/// <summary>
/// Copies images into the tilesheet art through Data, which refuses what the game couldn't use (E7a). A file copy, not a
/// catalog step: it isn't undone.
/// </summary>
/// <param name="writer">Writes into the content folder.</param>
/// <param name="catalog">The sheets and tiles a replacement is checked against, unsaved tiles included.</param>
internal sealed class SheetImport(ContentWriter writer, CatalogDraft catalog)
{
    /// <param name="paths">The images chosen, in order.</param>
    /// <param name="confirmReplace">Asked, with the file name, before an image already in the folder is replaced (decision 125).</param>
    public SheetImportResult Import(IReadOnlyList<string> paths, Func<string, bool> confirmReplace)
    {
        var imported = new List<string>();
        var refusals = new List<string>();
        foreach (var path in paths)
        {
            var fileName = Path.GetFileName(path);
            if (writer.HasTilesheetImage(fileName) && !confirmReplace(fileName))
            {
                continue;
            }

            try
            {
                imported.Add(writer.ImportTilesheetImage(path, catalog.Current));
            }
            catch (Exception exception) when (exception is ContentValidationException or IOException or UnauthorizedAccessException)
            {
                refusals.Add(exception.Message);
            }
        }

        return new SheetImportResult(imported, refusals);
    }
}
