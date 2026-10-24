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
/// The exploration tests' map: a 5x5 floor with a chest at (2, 0) that gives a potion once and opens, a lever at (0, 0)
/// that only swaps, a notice board at (4, 0) that talks every time, an open door at (4, 2) that greets on touch with a
/// doormat cell before it at (3, 2), a sign cell at (1, 3), and a cell at (0, 2) that talks only once.
/// </summary>
internal static class TestExplorationMap
{
    /// <summary>One walk step long enough to cross into the next cell.</summary>
    public static readonly TimeSpan ThirdOfASecond = TimeSpan.FromSeconds(1.0 / 3);

    public static readonly ObjectDefinition Chest = new() { Id = "chest", Width = 1, Height = 1, CollisionPolygon = [], Category = "test", Tiles = [], AbovePlayerZones = [] };
    public static readonly ObjectDefinition OpenedChest = Chest with { Id = "chest_opened" };
    public static readonly ObjectDefinition Lever = Chest with { Id = "lever" };
    public static readonly ObjectDefinition LeverPulled = Chest with { Id = "lever_pulled" };
    public static readonly ObjectDefinition OpenDoor = Chest with { Id = "door_open" };
    public static readonly ObjectDefinition NoticeBoard = Chest with { Id = "notice_board" };
    public static readonly ItemDefinition Potion = new ItemDefinitionBuilder().WithId("potion").Build();

    public static readonly DialogueStep Greeting = new(new Speech(["Welcome!"]));
    public static readonly MapEvent OpenChest = new MapEventBuilder().WithId("chest_0").AsOnce()
        .WithSteps(new GiveItemStep("potion", 1), new SwapObjectStep("chest_opened")).Build();

    public static readonly MapEvent PullLever = new MapEventBuilder().WithId("lever_0").WithSteps(new SwapObjectStep("lever_pulled")).Build();
    public static readonly MapEvent GreetAtDoor = new MapEventBuilder().WithId("door_0").AsTouch().WithSteps(Greeting).Build();
    public static readonly MapEvent ReadSign = new MapEventBuilder().WithId("cell_1_3").AsTouch().WithSteps(Greeting).Build();
    public static readonly MapEvent RememberOnce = new MapEventBuilder().WithId("cell_0_2").AsTouch().AsOnce().WithSteps(Greeting).Build();
    public static readonly MapEvent WipeFeet = new MapEventBuilder().WithId("cell_3_2").AsTouch().WithSteps(Greeting).Build();
    public static readonly MapEvent ReadNotice = new MapEventBuilder().WithId("notice_0").WithSteps(Greeting).Build();

    /// <param name="inventory">The party's inventory, which the chest fills.</param>
    /// <param name="position">Where the player starts.</param>
    /// <param name="facing">Which way the player starts facing.</param>
    public static ExplorationState Start(Inventory inventory, Point2D position, Facing facing)
    {
        var layout = new MapLayoutBuilder().WithLayer("11111", "11111", "11111", "11111", "11111").Build() with
        {
            Objects =
            [
                new PlacedObject { Uid = "lever@0,0", ObjectId = "lever", Position = new Point2D(0, 0), Z = MapLayers.PlayerZ, Events = [PullLever] },
                new PlacedObject { Uid = "chest@2,0", ObjectId = "chest", Position = new Point2D(2, 0), Z = MapLayers.PlayerZ, Events = [OpenChest] },
                new PlacedObject { Uid = "door@4,2", ObjectId = "door_open", Position = new Point2D(4, 2), Z = MapLayers.PlayerZ, Events = [GreetAtDoor] },
                new PlacedObject { Uid = "notice@4,0", ObjectId = "notice_board", Position = new Point2D(4, 0), Z = MapLayers.PlayerZ, Events = [ReadNotice] },
            ],
            CellEvents =
            [
                new CellEvent(new GridCell(1, 3), ReadSign),
                new CellEvent(new GridCell(0, 2), RememberOnce),
                new CellEvent(new GridCell(3, 2), WipeFeet),
            ],
        };
        var catalog = new ContentCatalogBuilder()
            .WithTiles(TestTiles.Walkable(1))
            .WithObjects(Chest, OpenedChest, Lever, LeverPulled, OpenDoor, NoticeBoard)
            .WithItems(Potion)
            .WithMaps(layout)
            .Build();
        var context = new ExplorationContext(catalog, inventory, new WorldState(), new FixedSequenceRandom());
        return new ExplorationState(context, TestParties.LedBy("sarxian"), new MapLocation(layout.Id, position, facing));
    }
}
