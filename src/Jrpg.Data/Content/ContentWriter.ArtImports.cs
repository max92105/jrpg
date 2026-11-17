using Jrpg.Core.Content;
using Jrpg.Data.Art;

namespace Jrpg.Data.Content;

/// <summary>
/// The image imports: a PNG copied into an art folder after that folder's own check, the one the game's startup makes
/// (<see cref="ArtReferenceCheck"/>), so an imported image never fails the start.
/// </summary>
public sealed partial class ContentWriter
{
    // The folders ImportArtImage serves; tilesheets have their own import, with the tiles cut from them to check.
    private static readonly string[] _artImportFolders =
        [ArtFolders.FieldSprites, ArtFolders.Battlers, ArtFolders.Portraits, ArtFolders.Effects, ArtFolders.Backgrounds];

    /// <param name="fileName">A file name in the tilesheet art folder.</param>
    public bool HasTilesheetImage(string fileName) => File.Exists(ArtImagePath(ArtFolders.Tilesheets, fileName));

    /// <param name="artFolder">One of <see cref="ArtFolders"/>' names.</param>
    /// <param name="fileName">A file name in that folder.</param>
    public bool HasArtImage(string artFolder, string fileName) => File.Exists(ArtImagePath(artFolder, fileName));

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

        CopyInto(ArtFolders.Tilesheets, sourcePath, fileName);
        return fileName;
    }

    /// <summary>
    /// Copies a PNG into an art folder under its own file name, replacing one already there (the caller asks first),
    /// after the folder's own check: a field sprite must hold the frames of the record of the same name, if it has one
    /// yet (ADR 0043); a battler or an effect strip at most <see cref="TextureLimit.MaxSide"/> wide, the widest the
    /// game loads (decision 637); a background at most that on both sides (decision 895); an effect strip must be whole
    /// square frames (decision 57); a portrait only needs to be a PNG.
    /// </summary>
    /// <param name="artFolder">The folder, one of field sprites, battlers, portraits, effects or backgrounds.</param>
    /// <param name="sourcePath">The image to copy, anywhere on disk.</param>
    /// <param name="catalog">
    /// The records the image is checked against; the editor's draft, unsaved records included.
    /// </param>
    /// <returns>The file name it was copied under.</returns>
    /// <exception cref="ArgumentException">A folder this import doesn't serve.</exception>
    /// <exception cref="ContentValidationException">
    /// Not a PNG, or refused by the folder's check; nothing was copied.
    /// </exception>
    public string ImportArtImage(string artFolder, string sourcePath, ContentCatalog catalog)
    {
        if (!_artImportFolders.Contains(artFolder, StringComparer.Ordinal))
        {
            throw new ArgumentException($"No image import into the '{artFolder}' art folder.", nameof(artFolder));
        }

        var fileName = Path.GetFileName(sourcePath);
        var size = PngSize.Read(sourcePath);
        RefuseTooLarge(artFolder, fileName, size);
        if (artFolder == ArtFolders.FieldSprites)
        {
            RefuseSpriteTooSmall(fileName, size, catalog);
        }
        else if (artFolder == ArtFolders.Effects && !ArtReferenceCheck.IsRowOfSquareFrames(size))
        {
            throw new ContentValidationException(
                $"{fileName}: a {size.Width}x{size.Height} strip isn't a row of whole square frames.");
        }

        CopyInto(artFolder, sourcePath, fileName);
        return fileName;
    }

    // Battle art and a battle background are each one texture; a strip grows only sideways, so only its width is
    // checked, while a background is checked on both sides, as the startup check does. A portrait is drawn by other
    // code, which this import doesn't limit.
    private static void RefuseTooLarge(string artFolder, string fileName, (int Width, int Height) size)
    {
        if (artFolder is ArtFolders.Battlers or ArtFolders.Effects && size.Width > TextureLimit.MaxSide)
        {
            throw new ContentValidationException(
                $"{fileName}: a {size.Width} px wide image is wider than the game loads ({TextureLimit.MaxSide} px); "
                + "import it at a smaller height.");
        }

        if (artFolder == ArtFolders.Backgrounds && TextureLimit.ProblemOf(size) is { } problem)
        {
            throw new ContentValidationException($"{fileName}: {problem}.");
        }
    }

    // A sprite without a record yet is taken: the window sets the record up after the import, the start checks it.
    private static void RefuseSpriteTooSmall(string fileName, (int Width, int Height) size, ContentCatalog catalog)
    {
        if (catalog.FieldSprites.TryGetValue(fileName, out var sheet) && !ArtReferenceCheck.HoldsFrames(sheet, size))
        {
            throw new ContentValidationException(
                $"{fileName}: a {size.Width}x{size.Height} sprite doesn't hold the {sheet.ColumnsNeeded} x {sheet.RowsNeeded} "
                + $"frames of {sheet.FrameWidth} x {sheet.FrameHeight} px its record needs.");
        }
    }

    // Copied aside first, then moved over, so a failed copy leaves the folder as it was.
    private void CopyInto(string artFolder, string sourcePath, string fileName)
    {
        var destination = ArtImagePath(artFolder, fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        File.Copy(sourcePath, destination + TemporarySuffix, overwrite: true);
        File.Move(destination + TemporarySuffix, destination, overwrite: true);
    }

    private string ArtImagePath(string artFolder, string fileName) =>
        Path.Combine(ArtFolders.RootIn(contentRoot), artFolder, fileName);
}
