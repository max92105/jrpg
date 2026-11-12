using Jrpg.Core.Sprites;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Exploration;

/// <summary>
/// Where one frame sits in a field sprite sheet (the player's and NPCs'), as its record cuts it (ADR 0043).
/// </summary>
internal static class FieldSpriteFrame
{
    public static Rectangle SourceOf(FieldSpriteSheet sheet, SpriteCell cell) =>
        new(sheet.FrameLeft(cell), sheet.FrameTop(cell), sheet.FrameWidth, sheet.FrameHeight);
}
