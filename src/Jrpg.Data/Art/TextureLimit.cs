namespace Jrpg.Data.Art;

/// <summary>
/// The widest image side the game loads, so an import can refuse what the game couldn't draw. The import checks a
/// battler's or an effect strip's width only: a strip grows sideways with its frames, its height is the battle height
/// chosen. A battle background is checked on both sides, by the import and the startup art check alike (decision 895).
/// </summary>
public static class TextureLimit
{
    /// <summary>
    /// HiDef's texture limit: the game sets no graphics profile, and MonoGame takes the highest the device supports.
    /// </summary>
    public const int MaxSide = 4096;

    /// <returns>
    /// Why the game can't load an image this size, without the file's name; null when both sides fit.
    /// </returns>
    internal static string? ProblemOf((int Width, int Height) size) =>
        size.Width > MaxSide || size.Height > MaxSide
            ? $"a {size.Width} x {size.Height} px image is larger than the game loads ({MaxSide} px a side)"
            : null;
}
