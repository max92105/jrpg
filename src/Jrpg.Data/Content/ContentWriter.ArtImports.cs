using Jrpg.Core.Content;
using Jrpg.Data.Art;

namespace Jrpg.Data.Content;

/// <summary>The image imports: a PNG copied into an art folder after that folder's own check.</summary>
public sealed partial class ContentWriter
{
    /// <param name="fileName">A file name in the tilesheet art folder.</param>
    public bool HasTilesheetImage(string fileName) => File.Exists(TilesheetImagePath(fileName));

    /// <summary>
    /// Copies a PNG into the tilesheet art folder under its own file name, replacing one already there (the caller asks
    /// first, decision 125). A replacement must still hold every tile cut from it by the catalog's sheets.
    /// </summary>
    /// <param name="sourcePath">The image to copy, anywhere on disk.</param>
    /// <param name="catalog">
    /// The sheets and tiles the image is checked against; the editor's draft, unsaved tiles included.
    /// </param>
    /// <returns>The file name it was copied under.</returns>
    /// <exception cref="ContentValidationException">
    /// Not a PNG, or too small for a tile cut from it; nothing was copied.
    /// </exception>
    public string ImportTilesheetImage(string sourcePath, ContentCatalog catalog)
    {
        var fileName = Path.GetFileName(sourcePath);
        var size = PngSize.Read(sourcePath);
        if (ArtReferenceCheck.FirstTileOutside(catalog, fileName, size) is { } outside)
        {
            throw new ContentValidationException(
                $"{fileName}: tile {outside.Id} at ({outside.Column}, {outside.Row}) "
                + $"would be outside the new {size.Width} x {size.Height} image.");
        }

        var destination = TilesheetImagePath(fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        File.Copy(sourcePath, destination + TemporarySuffix, overwrite: true);
        File.Move(destination + TemporarySuffix, destination, overwrite: true);
        return fileName;
    }
}
