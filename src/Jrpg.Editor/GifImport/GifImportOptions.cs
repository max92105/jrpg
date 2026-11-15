namespace Jrpg.Editor.GifImport;

/// <summary>What one GIF import asks (decisions 637, 638).</summary>
/// <param name="BattleHeight">
/// Each frame's height in the strip, in screen pixels, at least 1; the width follows.
/// </param>
/// <param name="RemovesBackground">
/// Whether the top-left pixel's color becomes transparent, for a GIF on a solid color.
/// </param>
internal sealed record GifImportOptions(int BattleHeight, bool RemovesBackground)
{
    // The goblin's drawn height under decision 100, so an imported monster stands as tall as the battle's first one.
    private const int DefaultBattleHeight = 128;

    /// <summary>128 px high, the background kept.</summary>
    public static GifImportOptions Default { get; } = new(DefaultBattleHeight, false);
}
