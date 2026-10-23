using Jrpg.Core.Geometry;
using Jrpg.Data.Art;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Exploration;

/// <summary>
/// Where one frame sits in a field sprite sheet (the player's and NPCs', ADR 0015), laid out as
/// <see cref="FieldSpriteLayout"/> says: a row per facing and a column per walk frame.
/// </summary>
internal static class FieldSpriteFrame
{
    /// <param name="facing">Picks the row (<see cref="WalkAnimation.RowOf"/>).</param>
    /// <param name="frame">The column, 0 for standing.</param>
    public static Rectangle SourceOf(Facing facing, int frame)
    {
        const int pixels = FieldSpriteLayout.FramePixels;
        return new(frame * pixels, WalkAnimation.RowOf(facing) * pixels, pixels, pixels);
    }
}
