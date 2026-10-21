using Jrpg.Core.Maps;

namespace Jrpg.Editor.Tiles;

/// <summary>One entry of the tile browser's sheet box: an image in the tilesheet folder, and its sheet once it is set up.</summary>
/// <param name="File">The image's file name in the tilesheet folder.</param>
/// <param name="Sheet">How the image is cut into cells; null until it is set up.</param>
internal sealed record SheetChoice(string File, TilesheetDefinition? Sheet)
{
    /// <summary>The sheet's id, or the file name for an image not set up yet.</summary>
    public string Label => Sheet?.Id ?? $"{File} (not set up)";
}
