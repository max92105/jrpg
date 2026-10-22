using Jrpg.Core.Geometry;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Exploration;

/// <summary>
/// Where one frame sits in a field sprite sheet (the player's and NPCs', ADR 0015): square frames, a row per facing and
/// a column per walk frame. The editor mirrors the frame size, since it can't reference the game.
/// </summary>
internal static class FieldSpriteFrame
{
    /// <summary>
    /// A frame of the field sprite sheet in image pixels, the size of a screen cell only by coincidence (question 58).
    /// </summary>
    public const int Pixels = 48;

    /// <param name="facing">Picks the row (<see cref="WalkAnimation.RowOf"/>).</param>
    /// <param name="frame">The column, 0 for standing.</param>
    public static Rectangle SourceOf(Facing facing, int frame) => new(frame * Pixels, WalkAnimation.RowOf(facing) * Pixels, Pixels, Pixels);
}
