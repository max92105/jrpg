namespace Jrpg.Editor.Catalogs;

/// <summary>
/// The list of a page that edits an art folder's records by file (the Effect strips and Character sprites pages): the
/// folder's images first, then each record whose image is missing (decision 442), listed so it stays editable and can
/// be imported again.
/// </summary>
internal static class ArtImageRows
{
    /// <param name="images">The readable images in the folder, in the order the list shows them.</param>
    /// <param name="recordFiles">The files the records describe, in any order.</param>
    /// <returns>The images in the given order, then the records' files without an image, by file.</returns>
    public static IReadOnlyList<ArtImageRow> Of(IReadOnlyList<string> images, IEnumerable<string> recordFiles)
    {
        var inFolder = images.ToHashSet(StringComparer.Ordinal);
        var withoutImages = recordFiles.Where(file => !inFolder.Contains(file)).Order(StringComparer.Ordinal);
        return
        [
            .. images.Select(file => new ArtImageRow(file, IsImageMissing: false)),
            .. withoutImages.Select(file => new ArtImageRow(file, IsImageMissing: true)),
        ];
    }
}
