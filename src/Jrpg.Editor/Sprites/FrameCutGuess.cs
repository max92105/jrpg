namespace Jrpg.Editor.Sprites;

/// <summary>The columns and rows a new sprite sheet's cut starts with, before the developer corrects them.</summary>
internal static class FrameCutGuess
{
    /// <summary>
    /// The bought sheets' walk is four frames wide (the plan's sheet, decision 438), so the cut starts there.
    /// </summary>
    public const int Columns = 4;

    /// <summary>
    /// The rows that make square frames of a width split in <see cref="Columns"/>: the height over the frame width when
    /// both split evenly; 1 otherwise, for the developer to set.
    /// </summary>
    public static int RowsFor(int imageWidth, int imageHeight)
    {
        if (imageWidth % Columns != 0)
        {
            return 1;
        }

        var frame = imageWidth / Columns;
        return frame > 0 && imageHeight % frame == 0 ? imageHeight / frame : 1;
    }
}
