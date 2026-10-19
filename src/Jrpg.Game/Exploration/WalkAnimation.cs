using Jrpg.Core.Geometry;

namespace Jrpg.Game.Exploration;

/// <summary>
/// Which frame of the field sprite sheet the player shows: 8 frames per row at 10 frames a second while walking, frame 0
/// when standing (legacy <c>PlayerManager</c>, developer decision 86).
/// </summary>
internal sealed class WalkAnimation
{
    private const int FrameCount = 8;
    private static readonly TimeSpan _frameDuration = TimeSpan.FromSeconds(0.1);

    private TimeSpan _sinceLastFrame;

    /// <summary>The sheet column, 0..7.</summary>
    public int Frame { get; private set; }

    /// <summary>The sheet row of a facing; the legacy sheet's rows, top to bottom, are down, left, right, up.</summary>
    public static int RowOf(Facing facing) => facing switch
    {
        Facing.Down => 0,
        Facing.Left => 1,
        Facing.Right => 2,
        Facing.Up => 3,
        _ => throw new ArgumentOutOfRangeException(nameof(facing), facing, null),
    };

    /// <summary>
    /// Moves on by the frames <paramref name="delta"/> covers. The leftover time carries to the next call, so 60 Hz
    /// updates still average exactly 10 frames a second (legacy dropped it and ran slow).
    /// </summary>
    public void Advance(TimeSpan delta)
    {
        _sinceLastFrame += delta;
        var frames = (int)(_sinceLastFrame.Ticks / _frameDuration.Ticks);
        _sinceLastFrame -= _frameDuration * frames;
        Frame = (Frame + frames) % FrameCount;
    }

    public void Stand()
    {
        Frame = 0;
        _sinceLastFrame = TimeSpan.Zero;
    }
}
