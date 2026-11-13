namespace Jrpg.Editor.Sprites;

/// <summary>
/// One row of the Character sprites window's list: an image in the field-sprite folder, set up or not; or a record
/// whose image the folder lacks (decision 442), listed so it can be re-imported.
/// </summary>
/// <param name="File">The image's file name in the field-sprite folder, the record's key.</param>
/// <param name="IsSetUp">Whether a record describes the image.</param>
/// <param name="IsImageMissing">Whether the folder lacks a readable image under <paramref name="File"/>.</param>
internal sealed record FieldSpriteChoice(string File, bool IsSetUp, bool IsImageMissing = false)
{
    public string Label => IsImageMissing ? $"{File} (image missing)" : IsSetUp ? File : $"{File} (not set up)";
}
