namespace Jrpg.Editor.Tiles;

/// <summary>
/// How an image splits into a grid of equal cells along one side, given the empty pixels around the grid and between
/// cells: the tile Cut panel's rule, shared with the sprite setup (which has no margin or spacing).
/// </summary>
/// <param name="Margin">Empty pixels around the whole grid.</param>
/// <param name="Spacing">Empty pixels between neighbouring cells.</param>
internal readonly record struct GridCut(int Margin, int Spacing)
{
    /// <summary>A grid of touching cells from the image's edge: a sprite sheet's.</summary>
    public static GridCut Plain { get; } = new(0, 0);

    /// <summary>
    /// A cell's length along a side, rounded down when the side doesn't split evenly; 0 for no cells.
    /// </summary>
    public int LengthAlong(int imageLength, int cells) => cells < 1 ? 0 : SpanAlong(imageLength, cells) / cells;

    /// <summary>Why a side doesn't split into this many cells, or null when it does, into whole pixels.</summary>
    /// <param name="imageLength">The side's length in image pixels.</param>
    /// <param name="cells">The cells asked for along it.</param>
    /// <param name="cellsName">"columns" or "rows", for the message.</param>
    public string? ProblemAlong(int imageLength, int cells, string cellsName)
    {
        if (LengthAlong(imageLength, cells) < 1)
        {
            return $"{imageLength} px is too small for {cells} {cellsName}.";
        }

        return SpanAlong(imageLength, cells) % cells != 0 ? $"{imageLength} px doesn't split into {cells} {cellsName}." : null;
    }

    // The image less the margin on both sides and the spacing between cells, shared by the cells.
    private int SpanAlong(int imageLength, int cells) => imageLength - (2 * Margin) - ((cells - 1) * Spacing);
}
