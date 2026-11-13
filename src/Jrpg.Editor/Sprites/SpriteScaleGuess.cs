using Jrpg.Core.Maps;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Sprites;

/// <summary>
/// The draw scale a new sprite sheet starts with (decision 562): the one the set-up tilesheets are drawn at, so a
/// character from the same pack stands at the right size next to its tiles (3 for Mana Seed's 16 px sheets).
/// </summary>
internal static class SpriteScaleGuess
{
    /// <returns>
    /// 48 over the most common tile width among the sheets when it divides evenly; 1 when none does or none is set up.
    /// </returns>
    public static int For(IEnumerable<TilesheetDefinition> sheets)
    {
        var commonest = sheets
            .GroupBy(sheet => sheet.TileWidth)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key)
            .Select(group => group.Key)
            .FirstOrDefault();
        var cell = (int)MapDrawing.CellPixels;
        return commonest > 0 && cell % commonest == 0 ? cell / commonest : 1;
    }
}
