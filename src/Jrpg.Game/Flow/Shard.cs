using Microsoft.Xna.Framework;

namespace Jrpg.Game.Flow;

/// <summary>
/// One shard of the broken frame (decision 258): a triangle whose corners are in virtual-screen pixels, which are also
/// where its picture comes from in the frozen frame, and how it flies once the glass breaks apart.
/// </summary>
/// <param name="A">A corner, in virtual-screen pixels.</param>
/// <param name="B">The next corner.</param>
/// <param name="C">The last corner.</param>
/// <param name="Direction">Away from the impact, through the shard's center; one pixel long.</param>
/// <param name="Speed">In pixels a second.</param>
/// <param name="Spin">In turns a second; negative turns the other way.</param>
internal readonly record struct Shard(Vector2 A, Vector2 B, Vector2 C, Vector2 Direction, float Speed, float Spin)
{
    public Vector2 Center => (A + B + C) / 3;

    /// <summary>The triangle's area in square pixels, whichever way its corners turn.</summary>
    public float Area => MathF.Abs(((B.X - A.X) * (C.Y - A.Y)) - ((C.X - A.X) * (B.Y - A.Y))) / 2;

    /// <summary>How far the shard has moved after flying for <paramref name="seconds"/>.</summary>
    public Vector2 OffsetAt(float seconds) => Direction * Speed * seconds;

    /// <summary>How far the shard has turned after flying for <paramref name="seconds"/>, in radians.</summary>
    public float RotationAt(float seconds) => Spin * MathF.Tau * seconds;
}
