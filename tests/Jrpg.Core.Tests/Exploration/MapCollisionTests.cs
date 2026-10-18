using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Exploration;

public sealed class MapCollisionTests
{
    private const double Radius = 0.1;

    // A 2x2 object whose outline is the lower-left triangle of its bounds.
    private static readonly ObjectDefinition _rock = new()
    {
        Id = "rock",
        Width = 2,
        Height = 2,
        CollisionPolygon = [new(0, 0), new(1, 1), new(0, 1)],
        Category = "test",
        Tiles = [],
        AbovePlayerZones = [],
    };

    [Fact]
    public void CanStandAt_OpenFloor_ReturnsTrue()
    {
        var collision = new MapCollision(new ActiveMapBuilder().WithRows("111", "111").Build());

        Assert.True(collision.CanStandAt(new Point2D(1.5, 0.5), Radius));
    }

    [Fact]
    public void CanStandAt_BodyOverlapsBlockedNeighbourCell_ReturnsFalse()
    {
        var collision = new MapCollision(new ActiveMapBuilder().WithRows("12").Build());

        // The center is in the floor cell, but the body's right edge reaches into the wall at x = 1.
        Assert.False(collision.CanStandAt(new Point2D(0.95, 0.5), Radius));
        Assert.True(collision.CanStandAt(new Point2D(0.85, 0.5), Radius));
    }

    [Fact]
    public void CanStandAt_CenterInsideObjectPolygon_ReturnsFalse()
    {
        var map = new ActiveMapBuilder().WithRows("1111", "1111", "1111").WithObject(_rock, new Point2D(1, 0)).Build();

        var canStand = new MapCollision(map).CanStandAt(new Point2D(1.5, 1.5), Radius);

        Assert.False(canStand);
    }

    [Fact]
    public void CanStandAt_InsideBoundsButOutsidePolygon_ReturnsTrue()
    {
        var map = new ActiveMapBuilder().WithRows("1111", "1111", "1111").WithObject(_rock, new Point2D(1, 0)).Build();

        var canStand = new MapCollision(map).CanStandAt(new Point2D(2.5, 0.5), Radius);

        Assert.True(canStand);
    }

    [Fact]
    public void CanStandAt_ObjectWithoutPolygon_NeverBlocks()
    {
        var rug = _rock with { Id = "rug", CollisionPolygon = [], Category = "test", Tiles = [], AbovePlayerZones = [] };
        var map = new ActiveMapBuilder().WithRows("1111", "1111").WithObject(rug, new Point2D(1, 0)).Build();

        var canStand = new MapCollision(map).CanStandAt(new Point2D(1.5, 1.5), Radius);

        Assert.True(canStand);
    }

    [Fact]
    public void KeepOnMap_PastTheEdges_ClampsByRadius()
    {
        var collision = new MapCollision(new ActiveMapBuilder().WithRows("111", "111").Build());

        var kept = collision.KeepOnMap(new Point2D(-1, 5), Radius);

        Assert.Equal(new Point2D(Radius, 2 - Radius), kept);
    }
}
