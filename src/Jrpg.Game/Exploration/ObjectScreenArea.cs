using Jrpg.Core.Maps;
using Jrpg.Core.Sprites;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Exploration;

/// <summary>
/// Where a placed object is drawn on the virtual screen: its cells, or for an NPC its sheet's frame standing on its
/// cell (<see cref="FieldSpriteSheet.AreaOver"/>, ADR 0043). The map view draws into it and the prompt sits over it, so
/// a tall NPC's head is never under the prompt.
/// </summary>
internal static class ObjectScreenArea
{
    /// <param name="placed">The object, for its position.</param>
    /// <param name="definition">What the object is now, after any swap.</param>
    /// <param name="sprites">Every field sprite's record, for an NPC's frame.</param>
    /// <param name="cameraOrigin">The map pixel at the screen's top-left corner.</param>
    public static Rectangle Of(
        PlacedObject placed,
        ObjectDefinition definition,
        IReadOnlyDictionary<string, FieldSpriteSheet> sprites,
        Vector2 cameraOrigin)
    {
        const int cell = MapView.CellPixels;

        // Objects can sit between cells (positions such as 2.5), so the top-left is rounded to a whole pixel once.
        var left = (int)Math.Round((placed.Position.X * cell) - cameraOrigin.X);
        var top = (int)Math.Round((placed.Position.Y * cell) - cameraOrigin.Y);
        if (definition.FieldSpriteFile is { } spriteFile)
        {
            var over = sprites[spriteFile].AreaOver(cell);
            return new Rectangle(left + over.Left, top + over.Top, over.Width, over.Height);
        }

        return new Rectangle(left, top, definition.Width * cell, definition.Height * cell);
    }
}
