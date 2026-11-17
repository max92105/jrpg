using Jrpg.Core.Battle;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Battle;

/// <summary>
/// The part of a battle's background image shown at the battle's time (decision 899): a still's cover-fit crop, or an
/// animated one's current frame, cover-fit within its cell, from frame 0 when the battle screen opens and looping.
/// </summary>
internal sealed class BattleBackdrop
{
    private static readonly Point _screen = new(VirtualScreen.Width, VirtualScreen.Height);

    private readonly AnimatedBackgroundDefinition? _animation;
    private TimeSpan _elapsed;

    /// <param name="file">The background's file in the backgrounds folder.</param>
    /// <param name="animation">Its record when it is an animated sheet (ADR 0059); null for a still picture.</param>
    public BattleBackdrop(string file, AnimatedBackgroundDefinition? animation)
    {
        File = file;
        _animation = animation;
    }

    public string File { get; }

    /// <summary>
    /// Moves the battle's time on by the update's delta, never a clock; a still shows the same either way.
    /// </summary>
    public void Advance(TimeSpan delta) => _elapsed += delta;

    /// <summary>The part of the image drawn over the whole screen now.</summary>
    /// <param name="imageSize">The loaded image's width and height in pixels.</param>
    public Rectangle SourceIn(Point imageSize)
    {
        if (_animation is null)
        {
            return CoverFit.SourceIn(imageSize, _screen);
        }

        var (x, y) = _animation.FrameOrigin(_animation.FrameAt(_elapsed), imageSize.X);
        var crop = CoverFit.SourceIn(new Point(_animation.FrameWidth, _animation.FrameHeight), _screen);
        return new Rectangle(x + crop.X, y + crop.Y, crop.Width, crop.Height);
    }
}
