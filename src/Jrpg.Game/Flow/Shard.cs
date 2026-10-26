using Microsoft.Xna.Framework;

namespace Jrpg.Game.Flow;

/// <summary>
/// One triangle of the frozen frame broken by an encounter (decision 258). Its corners are in screen pixels, which are
/// also where it takes its picture from in the frame.
/// </summary>
internal readonly record struct Shard(Vector2 A, Vector2 B, Vector2 C)
{
    public Vector2 Center => (A + B + C) / 3f;
}
