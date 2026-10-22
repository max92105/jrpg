using Microsoft.Xna.Framework;

namespace Jrpg.Game.Battle;

/// <summary>
/// How a battler image reaches the screen (developer decision 100, placeholders until battle sprite sheets): a painting
/// is shrunk smoothly once to a fixed square, pixel art is point-scaled by a whole factor so its pixels stay crisp.
/// </summary>
/// <param name="DrawnSize">The size it is drawn at, in screen pixels.</param>
/// <param name="IsShrunkSmoothly">Whether it is drawn from a smoothly shrunk copy rather than the source itself.</param>
internal readonly record struct BattlerScaling(Point DrawnSize, bool IsShrunkSmoothly)
{
    /// <summary>The square a painting is shrunk to, the battle's sprite size (placeholder).</summary>
    public const int PaintingSize = BattleLayout.SpotSize;

    // Paintings are 1024 px and the pixel art about 256 px; anything past this is treated as painted.
    private const int LargestPixelArt = 512;

    // The goblin's 256 px art at half size, 128 px (decision 100).
    private const int PixelArtDivisor = 2;

    /// <param name="sourceSize">The image's width and height in pixels.</param>
    public static BattlerScaling Of(Point sourceSize) =>
        Math.Max(sourceSize.X, sourceSize.Y) > LargestPixelArt
            ? new(new Point(PaintingSize, PaintingSize), true)
            : new(new Point(sourceSize.X / PixelArtDivisor, sourceSize.Y / PixelArtDivisor), false);
}
