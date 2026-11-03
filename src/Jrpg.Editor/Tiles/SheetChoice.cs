using Jrpg.Core.Maps;

namespace Jrpg.Editor.Tiles;

/// <summary>
/// One entry of the tile browser's sheet box: an image in the tilesheet folder, and its sheet once it is set up; or a
/// sheet whose image the folder lacks (decision 442), listed so it can be re-imported or removed.
/// </summary>
/// <param name="File">The image's file name in the tilesheet folder.</param>
/// <param name="Sheet">How the image is cut into cells; null until it is set up.</param>
/// <param name="IsImageMissing">Whether the folder lacks a readable image under <paramref name="File"/>.</param>
internal sealed record SheetChoice(string File, TilesheetDefinition? Sheet, bool IsImageMissing = false)
{
    /// <summary>
    /// The sheet's id, or the file name for an image not set up yet; "(image missing)" after a sheet without one.
    /// </summary>
    public string Label => Sheet is null ? $"{File} (not set up)" : IsImageMissing ? $"{Sheet.Id} (image missing)" : Sheet.Id;
}
