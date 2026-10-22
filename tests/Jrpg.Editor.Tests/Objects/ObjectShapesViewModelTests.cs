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

    private static readonly ObjectDefinition _sign = _house with { Id = "sign", Width = 1, Height = 1, CollisionPolygon = [], AbovePlayerZones = [] };

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
    public void Constructor_TreeWithZones_ListsThemWithPointCounts()
    {
        var tree = _house with { AbovePlayerZones = [_house.AbovePlayerZones[0], [new Point2D(0, 0), new Point2D(1, 0), new Point2D(1, 1), new Point2D(0, 1)]] };

        var shapes = new ObjectShapesViewModel(tree);

        Assert.Equal(["Zone 1 (3 points)", "Zone 2 (4 points)"], shapes.ZoneEntries);
        Assert.True(shapes.IsEditingCollision);
        Assert.Equal(-1, shapes.SelectedZoneIndex);
    }

    [Fact]
    public void AddPoint_EditingZonesWithNoZone_StartsZoneOne()
    {
        var shapes = new ObjectShapesViewModel(_sign) { IsEditingZones = true };

        shapes.AddPoint(new Point2D(0.5, 0.5));

        Assert.Equal([new Point2D(0.5, 0.5)], Assert.Single(shapes.Zones));
        Assert.Equal(0, shapes.SelectedZoneIndex);
    }

    [Fact]
    public void AddPoint_EditingZones_LeavesCollisionOutlineUnchanged()
    {
        var shapes = new ObjectShapesViewModel(_house) { IsEditingZones = true, SelectedZoneIndex = 0 };

        shapes.AddPoint(new Point2D(0, 0.25));

        Assert.Equal(_house.CollisionPolygon, shapes.CollisionOutline);
        Assert.Equal([.. _house.AbovePlayerZones[0], new Point2D(0, 0.25)], shapes.Zones[0]);
    }

    [Fact]
    public void RemoveLastPoint_EditingZone_ShrinksZoneAndKeepsOutline()
    {
        var shapes = new ObjectShapesViewModel(_house) { IsEditingZones = true, SelectedZoneIndex = 0 };

        shapes.RemoveLastPoint();

        Assert.Equal(_house.AbovePlayerZones[0].Take(2), shapes.Zones[0]);
        Assert.Equal(_house.CollisionPolygon, shapes.CollisionOutline);
    }

    [Fact]
    public void ClearShapeCommand_EditingZone_EmptiesOnlyThatZone()
    {
        var shapes = new ObjectShapesViewModel(_house) { IsEditingZones = true, SelectedZoneIndex = 0 };

        shapes.ClearShapeCommand.Execute(null);

        Assert.Empty(shapes.Zones[0]);
        Assert.Equal(_house.CollisionPolygon, shapes.CollisionOutline);
    }

    [Fact]
    public void NewZoneCommand_ObjectWithOneZone_AddsEmptyZoneAndSelectsIt()
    {
        var shapes = new ObjectShapesViewModel(_house) { IsEditingZones = true };

        shapes.NewZoneCommand.Execute(null);

        Assert.Equal(2, shapes.Zones.Count);
        Assert.Empty(shapes.Zones[1]);
        Assert.Equal(1, shapes.SelectedZoneIndex);
        Assert.Empty(shapes.ActivePoints);
    }

    [Fact]
    public void DeleteZoneCommand_SelectedZone_RemovesItAndSelectsThePrevious()
    {
        var shapes = new ObjectShapesViewModel(_house) { IsEditingZones = true };
        shapes.NewZoneCommand.Execute(null);

        shapes.DeleteZoneCommand.Execute(null);

        Assert.Equal(_house.AbovePlayerZones, shapes.Zones);
        Assert.Equal(0, shapes.SelectedZoneIndex);
    }

    [Fact]
    public void DeleteZoneCommand_NoZoneSelected_CannotExecute()
    {
        var shapes = new ObjectShapesViewModel(_house) { IsEditingZones = true };

        Assert.False(shapes.DeleteZoneCommand.CanExecute(null));
    }

    [Fact]
    public void Problem_ZoneWithTwoPoints_NamesTheZone()
    {
        var shapes = new ObjectShapesViewModel(_house) { IsEditingZones = true };
        shapes.NewZoneCommand.Execute(null);
        shapes.AddPoint(new Point2D(0, 0));

        shapes.AddPoint(new Point2D(1, 0));

        Assert.Equal("Zone 2 needs at least 3 points (has 2).", shapes.Problem);
    }

    [Fact]
    public void ToDefinition_EmptyZone_IsDropped()
    {
        var shapes = new ObjectShapesViewModel(_house) { IsEditingZones = true };
        shapes.NewZoneCommand.Execute(null);

        var edited = shapes.ToDefinition();

        Assert.Equal(_house.AbovePlayerZones, edited.AbovePlayerZones);
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
