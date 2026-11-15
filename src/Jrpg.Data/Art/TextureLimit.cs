namespace Jrpg.Data.Art;

/// <summary>
/// The widest image side the game loads, so an import can refuse what the battle couldn't draw. The import checks a
/// battle image's width only: a strip grows sideways with its frames, its height is the battle height chosen.
/// </summary>
public static class TextureLimit
{
    /// <summary>
    /// HiDef's texture limit: the game sets no graphics profile, and MonoGame takes the highest the device supports.
    /// </summary>
    public const int MaxSide = 4096;
}
