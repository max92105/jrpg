using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Battle;

/// <summary>
/// Where the command panel goes for the acting member: left of their spot, its top a sprite's height above them, kept
/// below the HUD and above the screen's bottom.
/// </summary>
internal static class CommandPanelPlacement
{
    public const int Width = 360;

    // Placeholder gap, highest top and tallest panel on the 1920 x 1080 virtual screen.
    private const int ActorGap = 40;
    private const int MinTop = 260;
    private const int MaxHeight = 420;

    /// <param name="actorSpot">The acting member's spot in the formation.</param>
    /// <returns>The panel's top left corner.</returns>
    public static Point Place(Rectangle actorSpot)
    {
        var top = Math.Min(actorSpot.Y - BattleLayout.SpotSize, VirtualScreen.Height - MaxHeight);
        return new Point(actorSpot.X - ActorGap - Width, Math.Max(MinTop, top));
    }
}
