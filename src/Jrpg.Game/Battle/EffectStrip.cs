using Jrpg.Core.Battle;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Battle;

/// <summary>
/// An effect played from a strip of square frames, left to right, once, each frame shown for its record's frame time
/// (decision 654); a strip <c>effect-strips.json</c> doesn't list shows each frame for
/// <see cref="EffectStripDefinition.UnlistedFrameTime"/>.
/// </summary>
/// <param name="FrameCount">How many frames the strip holds: its width over its height.</param>
/// <param name="FrameTime">How long each frame shows.</param>
internal readonly record struct EffectStrip(int FrameCount, TimeSpan FrameTime)
{
    /// <param name="imageSize">The strip image's width and height in pixels.</param>
    /// <param name="strip">Its record, listed or unlisted, whose pace it plays at.</param>
    public static EffectStrip Of(Point imageSize, EffectStripDefinition strip) => new(imageSize.X / imageSize.Y, strip.FrameTime);

    /// <returns>The frame showing <paramref name="elapsed"/> after the effect started, from 0; null once it has played.</returns>
    public int? FrameAt(TimeSpan elapsed)
    {
        var frame = (int)(elapsed / FrameTime);
        return frame < FrameCount ? frame : null;
    }
}
