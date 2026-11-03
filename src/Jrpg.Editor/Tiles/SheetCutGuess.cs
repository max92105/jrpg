namespace Jrpg.Editor.Tiles;

/// <summary>The square tile size a new sheet starts with, before the developer corrects the cut.</summary>
internal static class SheetCutGuess
{
    // Mana Seed is the art family (decision 438): every sheet's sides are multiples of 16, so 16 wins whenever it
    // divides; 24 catches the odd bought sheet. The map's 48 px cell is the fallback (16 divides whatever 48 does).
    private static readonly int[] _commonSizes = [16, 24];

    private const int Fallback = 48;

    /// <returns>The first common size that divides both sides of the image, else 48.</returns>
    public static int TileSizeFor(int imageWidth, int imageHeight) =>
        _commonSizes.FirstOrDefault(size => imageWidth % size == 0 && imageHeight % size == 0, Fallback);
}
