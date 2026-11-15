using Jrpg.Core.Maps;
using Jrpg.Data.Content;

namespace Jrpg.Data.Maps;

/// <summary>
/// One animation's rule, shared by a tile's animation, a field sprite's walk and an enemy's idle loop (ADRs 0039, 0043,
/// 0047): at least <see cref="TileAnimation.MinFrames"/> frames, each shown at least a millisecond.
/// </summary>
internal static class AnimationProblems
{
    /// <param name="field">
    /// Where the animation lies in its record, which prefixes the message: <c>walk.frames</c>.
    /// </param>
    /// <param name="animation">The animation checked.</param>
    /// <returns>The first problem; null when the animation is fine.</returns>
    public static string? Find(string field, TileAnimation animation) =>
        RangeProblems.AtLeast($"{field}.frameCount", animation.FrameCount, TileAnimation.MinFrames)
        ?? RangeProblems.AtLeast($"{field}.frameMilliseconds", animation.FrameMilliseconds, 1);
}
