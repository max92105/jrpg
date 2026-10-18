using Jrpg.Core.Dialogue;
using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Core.Items;
using Jrpg.Core.Maps;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;
using Jrpg.Core.World;

namespace Jrpg.Core.Tests.Exploration;

/// <summary>
/// Two 4x4 floor maps. "town" has a chest at (1, 0) that gives a potion once, swaps open and warps to the cave, and a
/// cell at (3, 3) that warps to the cave. "cave" has a cell at (2, 2) that warps back onto the town's warp cell (3, 3) and a sign
/// cell at (1, 1) that talks.
/// </summary>
public sealed class ExplorationWarpTests
{
    private static readonly ObjectDefinition _chest = new() { Id = "chest", Width = 1, Height = 1, CollisionPolygon = [], Category = "test", Tiles = [], AbovePlayerZones = [] };
    private static readonly ObjectDefinition _openedChest = _chest with { Id = "chest_opened" };
    private static readonly ItemDefinition _potion = new ItemDefinitionBuilder().WithId("potion").Build();
    private static readonly DialogueStep _greeting = new(new Speech(["Welcome to the cave."]));

    private static readonly MapEvent _openChest = new MapEventBuilder().WithId("chest_0").AsOnce()
        .WithSteps(new GiveItemStep("potion", 1), new SwapObjectStep("chest_opened"), new WarpStep("cave", new GridCell(1, 1)))
        .Build();

    private readonly Inventory _inventory = new();
    private readonly WorldState _world = new();

    [Fact]
    public void ContinueEvent_WarpStep_EntersTargetMapAtCellCenter()
    {
        var exploration = StartAt(new MapLocation("town", new Point2D(2.5, 3.5), Facing.Right));
        exploration.Walk(MoveIntent.FromAxes(1, 0), TimeSpan.FromSeconds(1.0 / 3));
        Assert.Equal(new WarpStep("cave", new GridCell(0, 3)), exploration.CurrentEventStep);

        exploration.ContinueEvent();

        Assert.Equal(new MapLocation("cave", new Point2D(0.5, 3.5), Facing.Right), exploration.Location);
        Assert.False(exploration.IsEventRunning);
    }

    [Fact]
    public void ContinueEvent_WarpOntoTouchCell_DoesNotFireArrivalCell()
    {
        var exploration = StartAt(new MapLocation("cave", new Point2D(2.5, 1.5), Facing.Down));
        exploration.Walk(MoveIntent.FromAxes(0, 1), TimeSpan.FromSeconds(1.0 / 3));
        exploration.ContinueEvent();
        Assert.Equal("town", exploration.Location.MapId);

        Assert.Equal(new GridCell(3, 3), GridCell.Containing(exploration.Location.Position));

        exploration.Walk(MoveIntent.FromAxes(-1, 0), TimeSpan.FromSeconds(0.05));

        Assert.Equal(new GridCell(3, 3), GridCell.Containing(exploration.Location.Position));
        Assert.Equal("town", exploration.Location.MapId);
        Assert.False(exploration.IsEventRunning);
    }

    [Fact]
    public void ContinueEvent_WarpBackToMapWithOpenedChest_ChestStaysOpen()
    {
        var exploration = StartAt(new MapLocation("town", new Point2D(1.5, 1.5), Facing.Up));
        exploration.TryInteract();
        exploration.ContinueEvent();
        exploration.ContinueEvent();
        Assert.Equal("cave", exploration.Location.MapId);
        exploration.Walk(MoveIntent.FromAxes(1, 1), TimeSpan.FromSeconds(0.25));
        exploration.ContinueEvent();
        Assert.Equal("town", exploration.Location.MapId);
        // From the town's warp cell (3.5, 3.5) back up to (1.5, 1.5), below the chest: 2√2 tiles at 3 tiles a second.
        exploration.Walk(MoveIntent.FromAxes(-1, -1), TimeSpan.FromSeconds(2 * Math.Sqrt(2) / 3));

        var reopened = exploration.TryInteract();

        Assert.Equal(new GridCell(1, 1), GridCell.Containing(exploration.Location.Position));
        Assert.False(reopened);
        Assert.Equal(1, _inventory.QuantityOf("potion"));
    }

    [Fact]
    public void ContinueEvent_OnceEventWithWarp_IsDoneOnSourceMap()
    {
        var exploration = StartAt(new MapLocation("town", new Point2D(1.5, 1.5), Facing.Up));
        exploration.TryInteract();
        exploration.ContinueEvent();

        exploration.ContinueEvent();

        Assert.True(_world.IsEventDone(new MapEventKey("town", "chest_0")));
        Assert.False(_world.IsEventDone(new MapEventKey("cave", "chest_0")));
        Assert.Equal("chest_opened", _world.SwappedObjectId("town", "chest@1,0"));
    }

    private ExplorationState StartAt(MapLocation start)
    {
        var town = new MapLayoutBuilder().WithLayer("1111", "1111", "1111", "1111").Build() with
        {
            Id = "town",
            Objects = [new PlacedObject { Uid = "chest@1,0", ObjectId = "chest", Position = new Point2D(1, 0), Events = [_openChest] }],
            CellEvents = [new CellEvent(new GridCell(3, 3), WarpEvent("cell_3_3", "cave", new GridCell(0, 3)))],
        };
        var cave = new MapLayoutBuilder().WithLayer("1111", "1111", "1111", "1111").Build() with
        {
            Id = "cave",
            CellEvents =
            [
                new CellEvent(new GridCell(2, 2), WarpEvent("cell_2_2", "town", new GridCell(3, 3))),
                new CellEvent(new GridCell(1, 1), new MapEventBuilder().WithId("cell_1_1").AsTouch().WithSteps(_greeting).Build()),
            ],
        };
        var catalog = new ContentCatalogBuilder()
            .WithObjects(_chest, _openedChest)
            .WithItems(_potion)
            .WithMaps(town, cave)
            .Build();
        return new ExplorationState(new ExplorationContext(catalog, _inventory, _world, new FixedSequenceRandom()), start);
    }

    private static MapEvent WarpEvent(string id, string mapId, GridCell cell) =>
        new MapEventBuilder().WithId(id).AsTouch().WithSteps(new WarpStep(mapId, cell)).Build();
}
