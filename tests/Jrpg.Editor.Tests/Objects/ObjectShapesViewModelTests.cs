using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Tests.Objects;

public sealed class ObjectShapesViewModelTests
{
    // A 2 x 3 house whose outline covers its lower half; one zone over its roof.
    private static readonly ObjectDefinition _house = new()
    {
        Id = "house",
        Width = 2,
        Height = 3,
        Category = "Buildings",
        Tiles = [new ObjectTile(0, 0, 7)],
        CollisionPolygon = [new Point2D(0, 0.5), new Point2D(1, 0.5), new Point2D(1, 1), new Point2D(0, 1)],
        AbovePlayerZones = [[new Point2D(0, 0), new Point2D(1, 0), new Point2D(1, 0.5)]],
    };

    private static readonly ObjectDefinition _sign = _house with { Id = "sign", Width = 1, Height = 1, CollisionPolygon = [] };

    [Fact]
    public void Constructor_HouseWithOutline_StartsWithItsPoints()
    {
        var shapes = new ObjectShapesViewModel(_house);

        Assert.Equal(_house.CollisionPolygon, shapes.CollisionOutline);
        Assert.Equal(("house", 2, 3), (shapes.ObjectId, shapes.Width, shapes.Height));
    }

    [Fact]
    public void AddPoint_InsideObject_AppendsToCollisionOutline()
    {
        var shapes = new ObjectShapesViewModel(_house);

        shapes.AddPoint(new Point2D(0.5, 0.25));

        Assert.Equal([.. _house.CollisionPolygon, new Point2D(0.5, 0.25)], shapes.CollisionOutline);
    }

    [Fact]
    public void AddPoint_OutsideTheObject_ClampsToTheEdge()
    {
        var shapes = new ObjectShapesViewModel(_sign);

        shapes.AddPoint(new Point2D(1.3, -0.2));

        Assert.Equal([new Point2D(1, 0)], shapes.CollisionOutline);
    }

    // A 1-cell object is 48 map pixels across: 0.5004 is 24.02 pixels, rounded to 24.
    [Fact]
    public void AddPoint_BetweenPixels_RoundsToTheObjectsPixelGrid()
    {
        var shapes = new ObjectShapesViewModel(_sign);

        shapes.AddPoint(new Point2D(0.5004, 0.5004));

        Assert.Equal([new Point2D(24.0 / 48, 24.0 / 48)], shapes.CollisionOutline);
    }

    [Fact]
    public void RemoveLastPoint_ThreePoints_RemovesTheNewest()
    {
        var shapes = new ObjectShapesViewModel(_sign);
        shapes.AddPoint(new Point2D(0, 0));
        shapes.AddPoint(new Point2D(1, 0));
        shapes.AddPoint(new Point2D(1, 1));

        shapes.RemoveLastPoint();

        Assert.Equal([new Point2D(0, 0), new Point2D(1, 0)], shapes.CollisionOutline);
    }

    [Fact]
    public void RemoveLastPoint_NoPoints_ChangesNothing()
    {
        var shapes = new ObjectShapesViewModel(_sign);

        shapes.RemoveLastPoint();

        Assert.Empty(shapes.CollisionOutline);
    }

    [Fact]
    public void ClearShapeCommand_OutlineWithPoints_EmptiesIt()
    {
        var shapes = new ObjectShapesViewModel(_house);

        shapes.ClearShapeCommand.Execute(null);

        Assert.Empty(shapes.CollisionOutline);
        Assert.Null(shapes.Problem);
    }

    [Fact]
    public void Problem_TwoPoints_SaysAtLeastThree()
    {
        var shapes = new ObjectShapesViewModel(_sign);
        shapes.AddPoint(new Point2D(0, 0));

        shapes.AddPoint(new Point2D(1, 0));

        Assert.Equal("The collision outline needs at least 3 points (has 2), or none.", shapes.Problem);
        Assert.Throws<InvalidOperationException>(shapes.ToDefinition);
    }

    [Fact]
    public void Problem_NoPoints_IsNull()
    {
        Assert.Null(new ObjectShapesViewModel(_sign).Problem);
    }

    [Fact]
    public void ToDefinition_OutlineEdited_KeepsIdTilesAndZones()
    {
        var shapes = new ObjectShapesViewModel(_house);
        shapes.RemoveLastPoint();

        var edited = shapes.ToDefinition();

        Assert.Equal(_house.CollisionPolygon.Take(3), edited.CollisionPolygon);
        Assert.Equal(_house with { CollisionPolygon = edited.CollisionPolygon }, edited);
    }
}
