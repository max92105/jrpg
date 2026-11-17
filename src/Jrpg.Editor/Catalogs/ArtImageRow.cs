namespace Jrpg.Editor.Catalogs;

/// <summary>One row of an art page's list: an image in its folder, or a record whose image the folder lacks.</summary>
/// <param name="File">The image's file name in its art folder, the record's key.</param>
/// <param name="IsImageMissing">Whether the folder lacks a readable image under <paramref name="File"/>.</param>
internal sealed record ArtImageRow(string File, bool IsImageMissing);
