using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;

namespace Jrpg.Core.Tests.Exploration;

/// <summary>What an NPC faces (ADR 0044, decision 577): the player while its own event runs, else down.</summary>
public sealed class NpcFacingTests
{
    private static readonly PlacedObject _villager = Placed("villager_1", "villager");
    private static readonly PlacedObject _guard = Placed("guard_1", "guard");

    [Fact]
    public void Of_NoEventRunning_FacesDown() => Assert.Equal(Facing.Down, NpcFacing.Of(_villager, speaking: null, Facing.Left));

    [Theory]
    [InlineData(Facing.Left, Facing.Right)]
    [InlineData(Facing.Right, Facing.Left)]
    [InlineData(Facing.Up, Facing.Down)]
    [InlineData(Facing.Down, Facing.Up)]
    public void Of_OwnEventRunning_FacesThePlayer(Facing playerFacing, Facing expected) =>
        Assert.Equal(expected, NpcFacing.Of(_villager, _villager, playerFacing));

    [Fact]
    public void Of_NoEventRunning_FacesItsPlacedFacing() =>
        Assert.Equal(Facing.Left, NpcFacing.Of(_villager with { Facing = Facing.Left }, speaking: null, Facing.Up));

    // The player wins over the placed facing while the event runs; the placed one is what it turns back to after.
    [Fact]
    public void Of_OwnEventRunning_IgnoresItsPlacedFacing()
    {
        var turned = _villager with { Facing = Facing.Left };

        Assert.Equal(Facing.Down, NpcFacing.Of(turned, turned, Facing.Up));
    }

    [Fact]
    public void Of_OtherObjectsEventRunning_FacesDown() =>
        Assert.Equal(Facing.Down, NpcFacing.Of(_villager, _guard, Facing.Left));

    // Uids repeat across maps: another map's villager_1 is not the one whose event runs.
    [Fact]
    public void Of_AnotherObjectWithTheSameUid_FacesDown() =>
        Assert.Equal(Facing.Down, NpcFacing.Of(_villager with { Position = new Point2D(5, 5) }, _villager, Facing.Left));

    private static PlacedObject Placed(string uid, string objectId) =>
        new() { Uid = uid, ObjectId = objectId, Position = new Point2D(1, 1), Z = MapLayers.PlayerZ, Events = [] };
}
