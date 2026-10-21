using Microsoft.Xna.Framework;

namespace Jrpg.Game.Battle;

/// <summary>
/// An effect played from a strip of square frames, left to right, once, at 0.08 s a frame (decision 57).
/// </summary>
/// <param name="FrameCount">How many frames the strip holds: its width over its height.</param>
internal readonly record struct EffectStrip(int FrameCount)
{
    private static readonly TimeSpan _frameTime = TimeSpan.FromSeconds(0.08);

    /// <param name="imageSize">The strip image's width and height in pixels.</param>
    public static EffectStrip Of(Point imageSize) => new(imageSize.X / imageSize.Y);

    /// <returns>The frame showing <paramref name="elapsed"/> after the effect started, from 0; null once it has played.</returns>
    public int? FrameAt(TimeSpan elapsed)
    {
        var frame = (int)(elapsed / _frameTime);
        return frame < FrameCount ? frame : null;
    }
}
