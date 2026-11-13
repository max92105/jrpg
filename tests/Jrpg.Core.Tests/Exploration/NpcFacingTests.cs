using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;

namespace Jrpg.Core.Tests.Exploration;

/// <summary>
/// What an NPC faces (ADR 0044, decision 577): its step's way while walking, else the player while its own event runs,
/// else its pose's facing (its last walk's, or its placed one, down by default).
/// </summary>
public sealed class NpcFacingTests
{
    private static readonly PlacedObject _villager = Placed("villager_1", "villager");
    private static readonly PlacedObject _guard = Placed("guard_1", "guard");

    [Fact]
    public void Of_NoEventRunning_FacesDown() =>
        Assert.Equal(Facing.Down, NpcFacing.Of(_villager, speaking: null, Facing.Left, Standing(_villager)));

    [Theory]
    [InlineData(Facing.Left, Facing.Right)]
    [InlineData(Facing.Right, Facing.Left)]
    [InlineData(Facing.Up, Facing.Down)]
    [InlineData(Facing.Down, Facing.Up)]
    public void Of_OwnEventRunning_FacesThePlayer(Facing playerFacing, Facing expected) =>
        Assert.Equal(expected, NpcFacing.Of(_villager, _villager, playerFacing, Standing(_villager)));

    [Fact]
    public void Of_NoEventRunning_FacesItsPlacedFacing()
    {
        var turned = _villager with { Facing = Facing.Left };

        Assert.Equal(Facing.Left, NpcFacing.Of(turned, speaking: null, Facing.Up, Standing(turned)));
    }

    // The player wins over the placed facing while the event runs; the placed one is what it turns back to after.
    [Fact]
    public void Of_OwnEventRunning_IgnoresItsPlacedFacing()
    {
        var turned = _villager with { Facing = Facing.Left };

        Assert.Equal(Facing.Down, NpcFacing.Of(turned, turned, Facing.Up, Standing(turned)));
    }

    [Fact]
    public void Of_OtherObjectsEventRunning_FacesDown() =>
        Assert.Equal(Facing.Down, NpcFacing.Of(_villager, _guard, Facing.Left, Standing(_villager)));

    // Uids repeat across maps: another map's villager_1 is not the one whose event runs.
    [Fact]
    public void Of_AnotherObjectWithTheSameUid_FacesDown()
    {
        var elsewhere = _villager with { Position = new Point2D(5, 5) };

        Assert.Equal(Facing.Down, NpcFacing.Of(elsewhere, _villager, Facing.Left, Standing(elsewhere)));
    }

    // A walk's step wins over everything, the player's own event included (decision 577).
    [Fact]
    public void Of_Walking_FacesItsStep()
    {
        var walking = new NpcPose(new Point2D(1.5, 1), Facing.Right, TimeSpan.FromMilliseconds(200), IsWalking: true);

        Assert.Equal(Facing.Right, NpcFacing.Of(_villager, _villager, Facing.Right, walking));
    }

    [Fact]
    public void Of_WalkedThenEventEnded_KeepsLastStepFacing()
    {
        var arrived = new NpcPose(new Point2D(3, 1), Facing.Up, TimeSpan.Zero, IsWalking: false);

        Assert.Equal(Facing.Up, NpcFacing.Of(_villager, speaking: null, Facing.Left, arrived));
    }

    private static NpcPose Standing(PlacedObject placed) =>
        new(placed.Position, placed.Facing ?? Facing.Down, TimeSpan.Zero, IsWalking: false);

    private static PlacedObject Placed(string uid, string objectId) =>
        new() { Uid = uid, ObjectId = objectId, Position = new Point2D(1, 1), Z = MapLayers.PlayerZ, Events = [] };
}
