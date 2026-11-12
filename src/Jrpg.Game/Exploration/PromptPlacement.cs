using Microsoft.Xna.Framework;

namespace Jrpg.Game.Exploration;

/// <summary>Where the interaction prompt goes on the virtual screen: centered over the object it names.</summary>
internal static class PromptPlacement
{
    /// <summary>The gap between the prompt's bottom and the object's top edge (placeholder).</summary>
    public const int GapAboveObject = 6;

    /// <param name="objectArea">
    /// Where the object is drawn (<see cref="ObjectScreenArea"/>), a tall NPC's head included.
    /// </param>
    /// <param name="boxSize">The prompt's size in pixels.</param>
    public static Rectangle BoxFor(Rectangle objectArea, Point boxSize)
    {
        var left = (int)Math.Round(objectArea.X + ((objectArea.Width - boxSize.X) / 2.0));
        var top = objectArea.Y - GapAboveObject - boxSize.Y;
        return new Rectangle(left, top, boxSize.X, boxSize.Y);
    }
}
