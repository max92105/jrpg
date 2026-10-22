using Jrpg.Core.Maps;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Exploration;

/// <summary>Where the interaction prompt goes on the virtual screen: centered over the object it names.</summary>
internal static class PromptPlacement
{
    /// <summary>The gap between the prompt's bottom and the object's top edge (placeholder).</summary>
    public const int GapAboveObject = 6;

    /// <param name="placed">The object the prompt names.</param>
    /// <param name="definition">What the object is now, for its width.</param>
    /// <param name="cameraOrigin">The map pixel at the screen's top-left corner.</param>
    /// <param name="boxSize">The prompt's size in pixels.</param>
    public static Rectangle BoxFor(PlacedObject placed, ObjectDefinition definition, Vector2 cameraOrigin, Point boxSize)
    {
        const int cell = MapView.CellPixels;
        var objectLeft = (placed.Position.X * cell) - cameraOrigin.X;
        var objectTop = (placed.Position.Y * cell) - cameraOrigin.Y;
        var left = (int)Math.Round(objectLeft + (((definition.Width * cell) - boxSize.X) / 2.0));
        var top = (int)Math.Round(objectTop) - GapAboveObject - boxSize.Y;
        return new Rectangle(left, top, boxSize.X, boxSize.Y);
    }
}
