using Jrpg.Game.Flow;
using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Flow;

public sealed class ShatterLayoutTests
{
    private static readonly Point _center = new(VirtualScreen.Width / 2, VirtualScreen.Height / 2);

    [Fact]
    public void Break_SameSeed_GivesTheSameShards()
    {
        var first = ShatterLayout.Break(_center, seed: 7);

        var second = ShatterLayout.Break(_center, seed: 7);

        Assert.Equal(first.Shards, second.Shards);
    }

    [Fact]
    public void Break_DifferentSeeds_GiveDifferentShards()
    {
        var first = ShatterLayout.Break(_center, seed: 7);

        var second = ShatterLayout.Break(_center, seed: 8);

        Assert.NotEqual(first.Shards, second.Shards);
    }

    // Non-overlapping triangles that tile the screen add up to its area; an overlap or a hole would change the sum.
    [Fact]
    public void Break_Shards_CoverTheScreenExactly()
    {
        var layout = ShatterLayout.Break(_center, seed: 7);

        var area = layout.Shards.Sum(shard => AreaOf(shard));

        Assert.Equal((double)VirtualScreen.Width * VirtualScreen.Height, area, 0);
    }

    // The impact and the ring points lie inside the screen and every other corner on its border, so a shard with a
    // corner off the border has one at the impact or on the ring.
    [Fact]
    public void Break_Shards_AllHaveACornerAtOrAroundTheImpactRing()
    {
        var layout = ShatterLayout.Break(_center, seed: 7);

        Assert.All(layout.Shards, shard => Assert.Contains(new[] { shard.A, shard.B, shard.C }, corner => !IsOnTheBorder(corner)));
    }

    [Fact]
    public void Break_ShardCount_IsAboutTheTarget()
    {
        var layout = ShatterLayout.Break(_center, seed: 7);

        Assert.InRange(layout.Shards.Count, 20, 28);
    }

    private static bool IsOnTheBorder(Vector2 corner) =>
        corner.X == 0 || corner.Y == 0 || corner.X == VirtualScreen.Width || corner.Y == VirtualScreen.Height;

    private static double AreaOf(Shard shard)
    {
        var (ab, ac) = (shard.B - shard.A, shard.C - shard.A);
        return Math.Abs(((double)ab.X * ac.Y) - ((double)ab.Y * ac.X)) / 2;
    }
}
