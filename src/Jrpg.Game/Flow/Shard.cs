using Microsoft.Xna.Framework;

namespace Jrpg.Game.Flow;

/// <summary>
/// One shard of the broken frame (decision 258): a triangle whose corners are in virtual-screen pixels, which are also
/// where its picture comes from in the frozen frame.
/// </summary>
internal readonly record struct Shard(Vector2 A, Vector2 B, Vector2 C)
{
    public Vector2 Center => (A + B + C) / 3;

    /// <summary>The triangle's area in square pixels, whichever way its corners turn.</summary>
    public float Area => MathF.Abs(((B.X - A.X) * (C.Y - A.Y)) - ((C.X - A.X) * (B.Y - A.Y))) / 2;
}
