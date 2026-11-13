using System.IO;
using Jrpg.Data.Content;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Catalogs;

/// <summary>
/// Copies images into an art folder through Data, which refuses what the game couldn't use (a field sprite too small
/// for its record, an effect strip that isn't whole square frames, decision 590); as <see cref="SheetImport"/>, a file
/// copy and not a catalog step, so it isn't undone. Serves the Character sprites and Enemies windows' Import....
/// </summary>
/// <param name="writer">Writes into the content folder.</param>
/// <param name="catalog">The records an image is checked against, unsaved ones included.</param>
internal sealed class ArtImageImport(ContentWriter writer, CatalogDraft catalog)
{
    /// <param name="artFolder">
    /// The field sprites, battlers, portraits or effects folder (<see cref="Data.Art.ArtFolders"/>).
    /// </param>
    /// <param name="paths">The images chosen, in order.</param>
    /// <param name="confirmReplace">
    /// Asked, with the file name, before an image already in the folder is replaced (decision 125).
    /// </param>
    public SheetImportResult Import(string artFolder, IReadOnlyList<string> paths, Func<string, bool> confirmReplace)
    {
        var imported = new List<string>();
        var refusals = new List<string>();
        foreach (var path in paths)
        {
            var fileName = Path.GetFileName(path);
            if (writer.HasArtImage(artFolder, fileName) && !confirmReplace(fileName))
            {
                continue;
            }

            try
            {
                imported.Add(writer.ImportArtImage(artFolder, path, catalog.Current));
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
