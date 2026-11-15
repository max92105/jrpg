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
/// <param name="HitFrame">The frame that shows at the impact (decision 656).</param>
internal readonly record struct EffectStrip(int FrameCount, TimeSpan FrameTime, int HitFrame)
{
    /// <param name="imageSize">The strip image's width and height in pixels.</param>
    /// <param name="strip">Its record, listed or unlisted, whose pace it plays at.</param>
    public static EffectStrip Of(Point imageSize, EffectStripDefinition strip) =>
        new(imageSize.X / imageSize.Y, strip.FrameTime, strip.HitFrame);

    /// <returns>The frame showing <paramref name="elapsed"/> after the effect started, from 0; null once it has played.</returns>
    public int? FrameAt(TimeSpan elapsed)
    {
        var frame = (int)(elapsed / FrameTime);
        return frame < FrameCount ? frame : null;
    }

    /// <summary>
    /// The frame showing <paramref name="sinceImpact"/> after the impact: the strip starts its lead (hit frame x frame
    /// time) before it, so its hit frame shows at the impact (decision 656). A lead longer than the wind-up starts the
    /// strip partway, its first frames never shown.
    /// </summary>
    /// <param name="sinceImpact">Negative before the impact, while the action winds up.</param>
    /// <returns>The frame from 0; null before the strip starts and once it has played.</returns>
    public int? FrameAtImpactTime(TimeSpan sinceImpact)
    {
        var sinceStart = (HitFrame * FrameTime) + sinceImpact;
        return sinceStart < TimeSpan.Zero ? null : FrameAt(sinceStart);
    }
}
