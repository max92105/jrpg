using Jrpg.Core.Events;
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
    public void CanMoveTo_OpenFloor_ReturnsTrue()
    {
        var collision = new MapCollision(new ActiveMapBuilder().WithRows("111", "111").Build());

        Assert.True(collision.CanMoveTo(new Point2D(0.5, 0.5), new Point2D(1.5, 0.5), Radius));
    }

    [Fact]
    public void CanMoveTo_BodyOverlapsBlockedNeighbourCell_ReturnsFalse()
    {
        var collision = new MapCollision(new ActiveMapBuilder().WithRows("12").Build());
        var start = new Point2D(0.5, 0.5);

        // The center is in the floor cell, but the body's right edge reaches into the wall at x = 1.
        Assert.False(collision.CanMoveTo(start, new Point2D(0.95, 0.5), Radius));
        Assert.True(collision.CanMoveTo(start, new Point2D(0.85, 0.5), Radius));
    }

    [Fact]
    public void CanMoveTo_CenterEntersObjectPolygon_ReturnsFalse()
    {
        var map = new ActiveMapBuilder().WithRows("1111", "1111", "1111").WithObject(_rock, new Point2D(1, 0)).Build();

        var canMove = new MapCollision(map).CanMoveTo(new Point2D(0.5, 1.5), new Point2D(1.5, 1.5), Radius);

        Assert.False(canMove);
    }

    [Fact]
    public void CanMoveTo_InsideBoundsButOutsidePolygon_ReturnsTrue()
    {
        var map = new ActiveMapBuilder().WithRows("1111", "1111", "1111").WithObject(_rock, new Point2D(1, 0)).Build();

        var canMove = new MapCollision(map).CanMoveTo(new Point2D(3.5, 0.5), new Point2D(2.5, 0.5), Radius);

        Assert.True(canMove);
    }

    [Fact]
    public void CanMoveTo_ObjectWithoutPolygon_NeverBlocks()
    {
        var rug = _rock with { Id = "rug", CollisionPolygon = [], Category = "test", Tiles = [], AbovePlayerZones = [] };
        var map = new ActiveMapBuilder().WithRows("1111", "1111").WithObject(rug, new Point2D(1, 0)).Build();

        var canMove = new MapCollision(map).CanMoveTo(new Point2D(0.5, 1.5), new Point2D(1.5, 1.5), Radius);

        Assert.True(canMove);
    }

    // A walked NPC blocks where its walk ended, not where it was placed (decision 574).
    [Fact]
    public void CanMoveTo_NpcWalkedAway_FreesItsPlacedCell()
    {
        var collision = new MapCollision(WithGuardWalkedRight());

        Assert.True(collision.CanMoveTo(new Point2D(0.5, 0.5), new Point2D(1.5, 0.5), Radius));
    }

    [Fact]
    public void CanMoveTo_NpcWalkedHere_Blocks()
    {
        var collision = new MapCollision(WithGuardWalkedRight());

        Assert.False(collision.CanMoveTo(new Point2D(3.5, 0.5), new Point2D(2.5, 0.5), Radius));
    }

    // The NPC walked onto the player: the polygon already holds them, so it never refuses their step (decision 593).
    [Fact]
    public void CanMoveTo_FromInsideAnObject_MayStepOut()
    {
        var collision = new MapCollision(WithGuardWalkedRight());

        Assert.True(collision.CanMoveTo(new Point2D(2.5, 0.5), new Point2D(2.6, 0.5), Radius));
    }

    [Fact]
    public void KeepOnMap_PastTheEdges_ClampsByRadius()
    {
        var collision = new MapCollision(new ActiveMapBuilder().WithRows("111", "111").Build());

        var kept = collision.KeepOnMap(new Point2D(-1, 5), Radius);

        Assert.Equal(new Point2D(Radius, 2 - Radius), kept);
    }

    // A one-cell NPC with a square outline, placed at (1, 0), walked one cell right to (2, 0).
    private static ActiveMap WithGuardWalkedRight()
    {
        var guard = _rock with
        {
            Id = "guard",
            Width = 1,
            Height = 1,
            CollisionPolygon = [new(0, 0), new(1, 0), new(1, 1), new(0, 1)],
            FieldSpriteFile = "guard.png",
        };
        var map = new ActiveMapBuilder().WithRows("1111", "1111").WithObject(guard, new Point2D(1, 0)).Build();
        map.StartWalk(map.Definition.Layout.Objects[0], new MoveNpcStep("guard_0", [Facing.Right]));
        map.FinishWalks();
        return map;
    }
}
