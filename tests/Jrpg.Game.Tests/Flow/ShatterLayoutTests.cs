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

    [Fact]
    public void Break_Shards_CoverTheScreenExactly()
    {
        var layout = ShatterLayout.Break(_center, seed: 7);

        var area = layout.Shards.Sum(shard => (double)shard.Area);

        Assert.Equal((double)VirtualScreen.Width * VirtualScreen.Height, area, precision: 0);
    }

    // The shards at the impact touch it; every other shard has a corner on the ring, which lies within 57 % of the way
    // from the impact to the screen's farthest corner.
    [Fact]
    public void Break_Shards_AllHaveACornerAtOrAroundTheImpactRing()
    {
        var layout = ShatterLayout.Break(_center, seed: 7);
        var ringReach = 0.57f * Vector2.Distance(_center.ToVector2(), Vector2.Zero) + 1;

        var nearest = layout.Shards.Select(shard => new[] { shard.A, shard.B, shard.C }.Min(corner => Vector2.Distance(corner, _center.ToVector2())));

        Assert.All(nearest, distance => Assert.True(distance <= ringReach, $"A shard's nearest corner is {distance} px out."));
    }

    [Fact]
    public void Break_ShardCount_IsEightSectorsOfThree()
    {
        var layout = ShatterLayout.Break(_center, seed: 7);

        Assert.Equal(24, layout.Shards.Count);
    }

    [Fact]
    public void Break_EachShard_FliesAwayFromTheImpact()
    {
        var layout = ShatterLayout.Break(_center, seed: 7);
        var impact = _center.ToVector2();

        var gains = layout.Shards.Select(shard => Vector2.Distance(shard.Center + shard.OffsetAt(0.1f), impact) - Vector2.Distance(shard.Center, impact));

        Assert.All(gains, gain => Assert.True(gain > 0, $"A shard came {-gain} px closer to the impact."));
    }

    [Fact]
    public void Break_SameSeed_GivesTheSameFlights()
    {
        var first = ShatterLayout.Break(_center, seed: 7);

        var second = ShatterLayout.Break(_center, seed: 7);

        Assert.Equal(first.Shards.Select(shard => (shard.Speed, shard.Spin)), second.Shards.Select(shard => (shard.Speed, shard.Spin)));
    }

    [Fact]
    public void Break_EachShard_FliesAtTheSpeedAndSpinRange()
    {
        var layout = ShatterLayout.Break(_center, seed: 7);

        var flights = layout.Shards.Select(shard => (shard.Speed, shard.Spin));

        Assert.All(flights, flight =>
        {
            Assert.InRange(flight.Speed, 300, 900);
            Assert.InRange(flight.Spin, -1.5f, 1.5f);
        });
    }
}
