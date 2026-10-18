using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.World;

namespace Jrpg.Core.Tests.Exploration;

public sealed class ActiveMapTests
{
    private static readonly ObjectDefinition _closedDoor = new()
    {
        Id = "door_close",
        Width = 1,
        Height = 1,
        CollisionPolygon = [new(0, 0), new(1, 0), new(1, 1), new(0, 1)],
        Category = "test",
        Tiles = [],
        AbovePlayerZones = [],
    };

    private static readonly ObjectDefinition _openDoor = _closedDoor with { Id = "door_open", CollisionPolygon = [] };

    [Fact]
    public void CurrentDefinitionOf_SwappedObject_ReturnsSwappedDefinition()
    {
        var map = DoorMap(new WorldState());
        var door = map.Definition.Layout.Objects[0];

        map.SwapObject(door, "door_open");

        Assert.Equal(_openDoor, map.CurrentDefinitionOf(door));
    }

    [Fact]
    public void CurrentDefinitionOf_SwapMadeOnEarlierVisit_StillApplies()
    {
        var world = new WorldState();
        var firstVisit = DoorMap(world);
        firstVisit.SwapObject(firstVisit.Definition.Layout.Objects[0], "door_open");

        var secondVisit = DoorMap(world);

        Assert.Equal("door_open", secondVisit.CurrentDefinitionOf(secondVisit.Definition.Layout.Objects[0]).Id);
    }

    [Fact]
    public void MapCollision_CanStandAt_AfterSwapToObjectWithoutPolygon_ReturnsTrue()
    {
        var map = DoorMap(new WorldState());
        var collision = new MapCollision(map);
        var doorCenter = new Point2D(1.5, 0.5);
        Assert.False(collision.CanStandAt(doorCenter, 0.1));

        map.SwapObject(map.Definition.Layout.Objects[0], "door_open");

        Assert.True(collision.CanStandAt(doorCenter, 0.1));
    }

    private static ActiveMap DoorMap(WorldState world) =>
        new ActiveMapBuilder().WithRows("111")
            .WithObject(_closedDoor, new Point2D(1, 0))
            .WithKnownObject(_openDoor)
            .WithWorld(world)
            .Build();
}
