using Jrpg.Core.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Exploration;

/// <summary>
/// Where one frame sits in a field sprite sheet (the player's and NPCs'), as its record cuts it (ADR 0043), and whether
/// it draws flipped (ADR 0054).
/// </summary>
internal static class FieldSpriteFrame
{
    public static Rectangle SourceOf(FieldSpriteSheet sheet, SpriteCell cell) =>
        new(sheet.FrameLeft(cell), sheet.FrameTop(cell), sheet.FrameWidth, sheet.FrameHeight);

    /// <summary>A facing that mirrors another draws flipped left to right in the same area (decision 766).</summary>
    public static SpriteEffects EffectsOf(SpriteFrame frame) =>
        frame.IsMirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
}
