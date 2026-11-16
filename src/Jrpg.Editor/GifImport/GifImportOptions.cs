namespace Jrpg.Editor.GifImport;

/// <summary>What one GIF import asks (decisions 637, 638, 987).</summary>
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

    /// <summary>128 px high, the background kept, each GIF at its most common delay.</summary>
    public static GifImportOptions Default { get; } = new(DefaultBattleHeight, false);

    /// <summary>
    /// The pace typed in the dialog, in whole milliseconds, at least 1, for a battle sprite's loop or an effect strip's
    /// row on the Effect strips page (decision 988); null for each GIF's most common delay (decision 632), or an
    /// effect's unlisted pace.
    /// </summary>
    public int? FrameMilliseconds { get; init; }
}
