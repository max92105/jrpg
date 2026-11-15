using Microsoft.Xna.Framework;

namespace Jrpg.Game.Ui;

/// <summary>
/// One segment of a progress ring (decision 626): the ring's slice of <see cref="Count"/> around its top, white, baked
/// into a small texture and drawn turned about the ring's center once per segment. The ring's inner and outer edges are
/// smoothed over a pixel; the slice's sides are cut hard, so neighbors meet without a seam. Equal shapes are equal
/// records, so a shape is its own texture cache key.
/// </summary>
/// <param name="OuterRadius">The ring's outer edge from its center, in pixels.</param>
/// <param name="Thickness">The ring's width, in pixels.</param>
/// <param name="Count">How many segments make the ring.</param>
internal sealed record RingSegmentShape(int OuterRadius, int Thickness, int Count)
{
    // A pixel of room above the outer edge and at each side, so the smoothing isn't cut off.
    private const int Margin = 1;

    private float HalfAngle => MathF.PI / Count;

    private int InnerRadius => OuterRadius - Thickness;

    public int Width => (2 * (int)MathF.Ceiling(OuterRadius * MathF.Sin(HalfAngle))) + (2 * Margin);

    public int Height => (int)MathF.Ceiling(OuterRadius - (InnerRadius * MathF.Cos(HalfAngle))) + (2 * Margin);

    /// <summary>
    /// Where the ring's center is in the texture's space, below it and out of its bounds: the point the segment turns
    /// about when drawn.
    /// </summary>
    public Vector2 Center => new(Width / 2f, OuterRadius + Margin);

    /// <returns><see cref="Width"/> x <see cref="Height"/> premultiplied white pixels, row by row.</returns>
    public Color[] Pixels()
    {
        var pixels = new Color[Width * Height];
        var center = Center;
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var offset = new Vector2(x + 0.5f - center.X, y + 0.5f - center.Y);
                pixels[(y * Width) + x] = Color.White * Coverage(offset);
            }
        }

        return pixels;
    }

    // Inside the slice's angle and between the ring's edges, ramping over a pixel at each edge.
    private float Coverage(Vector2 offset)
    {
        if (MathF.Abs(MathF.Atan2(offset.X, -offset.Y)) > HalfAngle)
        {
            return 0;
        }

        var distance = offset.Length();
        var insideOuter = Math.Clamp(OuterRadius - distance + 0.5f, 0, 1);
        var outsideInner = Math.Clamp(distance - InnerRadius + 0.5f, 0, 1);
        return insideOuter * outsideInner;
    }
}
