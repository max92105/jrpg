namespace Jrpg.Editor.Tiles;

/// <summary>The square tile size a new sheet starts with, before the developer corrects the cut.</summary>
internal static class SheetCutGuess
{
    // The map's 48 px cell first, then the usual sizes of bought tilesheets. 64 never wins, since 32 divides whatever
    // 64 does; the Cut panel corrects a 64 px sheet.
    private static readonly int[] _commonSizes = [48, 32, 16, 64, 24];

    /// <returns>The first common size that divides both sides of the image, else 48.</returns>
    public static int TileSizeFor(int imageWidth, int imageHeight) =>
        _commonSizes.FirstOrDefault(size => imageWidth % size == 0 && imageHeight % size == 0, _commonSizes[0]);
}
