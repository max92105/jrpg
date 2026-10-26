using Microsoft.Xna.Framework;

namespace Jrpg.Game.Flow;

/// <summary>
/// One triangle of the frozen frame broken by an encounter (decision 258), and how it flies away. Its corners are in
/// screen pixels, which are also where it takes its picture from in the frame.
/// </summary>
/// <param name="A">A corner.</param>
/// <param name="B">A corner.</param>
/// <param name="C">A corner.</param>
/// <param name="Velocity">How it flies, in pixels per second: away from the impact.</param>
/// <param name="Spin">How fast it turns about its center, in radians per second; negative turns the other way.</param>
internal readonly record struct Shard(Vector2 A, Vector2 B, Vector2 C, Vector2 Velocity, float Spin)
{
    public Vector2 Center => (A + B + C) / 3f;

    /// <summary>How far it has moved after <paramref name="seconds"/> of flight.</summary>
    public Vector2 OffsetAt(float seconds) => Velocity * seconds;

    /// <summary>How far it has turned after <paramref name="seconds"/> of flight, in radians.</summary>
    public float RotationAt(float seconds) => Spin * seconds;
}
