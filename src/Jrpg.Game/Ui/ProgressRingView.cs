using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Ui;

/// <summary>
/// A thin ring that fills clockwise from its top as a share grows, such as the hold to skip a cinematic (decision 626):
/// <see cref="SegmentCount"/> baked segments drawn turned about the center, lit up to the share, the segment at the
/// edge lit by its part of the share so the fill moves smoothly, the rest a dim track.
/// </summary>
/// <param name="ui">Bakes the segment once.</param>
internal sealed class ProgressRingView(UiTextures ui)
{
    /// <summary>How many segments make the ring: enough that the fill reads as smooth at the hint's size.</summary>
    public const int SegmentCount = 48;

    private const float SegmentAngle = MathHelper.TwoPi / SegmentCount;

    private readonly RingSegmentShape _shape = new(UiTheme.SkipRingRadius, UiTheme.SkipRingThickness, SegmentCount);

    /// <summary>How many segments are fully lit at a share.</summary>
    /// <param name="share">0 to 1; outside it is clamped.</param>
    internal static int LitSegments(float share) => (int)(Math.Clamp(share, 0, 1) * SegmentCount);

    /// <summary>
    /// How lit the segment at the fill's edge is, 0 to 1: the part of the share past the lit segments.
    /// </summary>
    internal static float EdgeAlpha(float share)
    {
        var segments = Math.Clamp(share, 0, 1) * SegmentCount;
        return segments - MathF.Floor(segments);
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the segment is made.</param>
    /// <param name="center">The ring's center, in virtual pixels.</param>
    /// <param name="share">0 to 1: how much of the ring is lit.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Vector2 center, float share)
    {
        var segment = ui.RingSegment(textures, _shape);
        var lit = LitSegments(share);
        var edge = EdgeAlpha(share);
        for (var index = 0; index < SegmentCount; index++)
        {
            var color = index < lit ? UiTheme.SkipRingLit
                : index == lit ? Color.Lerp(UiTheme.SkipRingTrack, UiTheme.SkipRingLit, edge)
                : UiTheme.SkipRingTrack;

            // Segment 0 spans the first slice clockwise from the top, so the fill starts at twelve o'clock.
            var rotation = (index + 0.5f) * SegmentAngle;
            batch.Draw(segment, center, null, color, rotation, _shape.Center, 1f, SpriteEffects.None, 0);
        }
    }
}
