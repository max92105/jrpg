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

public sealed class ExplorationArrivalTests
{
    private static readonly DialogueStep _omersIntro = new(new Speech(["Omer's intro."]));
    private static readonly DialogueStep _sarxiansIntro = new(new Speech(["Sarxian's intro."]));
    private static readonly DialogueStep _afterTheWarp = new(new Speech(["Through the gate."]));

    private static readonly MapEvent _omersArrival =
        new MapEventBuilder().WithId("cell_0_0").AsArrive().AsOnce().OnlyWhenLeaderIs("omer").WithSteps(_omersIntro).Build();

    private static readonly MapEvent _sarxiansArrival =
        new MapEventBuilder().WithId("cell_1_0").AsArrive().AsOnce().OnlyWhenLeaderIs("sarxian").WithSteps(_sarxiansIntro).Build();

    private static readonly DialogueStep _boardSpeaks = new(new Speech(["The board speaks."]));
    private static readonly DialogueStep _floorSpeaks = new(new Speech(["The floor speaks."]));

    [Fact]
    public void Start_MapWithArrivalEventForTheLeader_RunsIt()
    {
        var exploration = Start("hall", "sarxian", new WorldState());

        Assert.Equal(_sarxiansIntro, exploration.CurrentEventStep);
    }

    [Fact]
    public void Start_ArrivalEventForAnotherLeader_DoesNotRun()
    {
        var exploration = Start("hall", "zanyr", new WorldState());

        Assert.False(exploration.IsEventRunning);
    }

    [Fact]
    public void Start_OnceArrivalEventDone_DoesNotRunAgain()
    {
        var world = new WorldState([new MapEventKey("hall", _sarxiansArrival.Id)], []);

        var exploration = Start("hall", "sarxian", world);

        Assert.False(exploration.IsEventRunning);
    }

    [Fact]
    public void Start_ArrivalEventsOnObjectAndCell_RunsTheObjectsFirst()
    {
        var exploration = Start("shrine", "sarxian", new WorldState());

        Assert.Equal(_boardSpeaks, exploration.CurrentEventStep);
    }

    [Fact]
    public void Start_ObjectArrivalEventForSwappedObject_RunsTheCellsInstead()
    {
        var world = new WorldState([], [new ObjectSwap("shrine", "board@0,0", "chest")]);

        var exploration = Start("shrine", "sarxian", world);

        Assert.Equal(_floorSpeaks, exploration.CurrentEventStep);
    }

    [Fact]
    public void Start_NotOnceArrivalEventAlreadyRun_RunsAgain()
    {
        var world = new WorldState();
        var first = Start("shrine", "sarxian", world);
        first.ContinueEvent();

        var second = Start("shrine", "sarxian", world);

        Assert.Equal(_boardSpeaks, second.CurrentEventStep);
    }

    [Fact]
    public void ContinueEvent_WarpOntoMapWithArrivalEvent_RunsItAfterLanding()
    {
        var exploration = Start("yard", "omer", new WorldState());
        exploration.TryInteract();

        exploration.ContinueEvent();

        Assert.Equal("hall", exploration.CurrentMap.Id);
        Assert.Equal(_omersIntro, exploration.CurrentEventStep);
    }

    [Fact]
    public void ContinueEvent_StepsAfterTheWarp_RunBeforeTheArrivalEvent()
    {
        var exploration = Start("gate", "omer", new WorldState());
        exploration.TryInteract();
        exploration.ContinueEvent();
        Assert.Equal(_afterTheWarp, exploration.CurrentEventStep);

        exploration.ContinueEvent();

        Assert.Equal(_omersIntro, exploration.CurrentEventStep);
    }

    [Fact]
    public void Walk_IntoCellWithArrivalEvent_DoesNotRunIt()
    {
        var exploration = Start("hall", "zanyr", new WorldState(), new Point2D(0.5, 1.5));

        exploration.Walk(MoveIntent.FromAxes(0, -1), TestExplorationMap.ThirdOfASecond);

        Assert.False(exploration.IsEventRunning);
    }

    // "hall" holds Omer's and Sarxian's arrival events on its top row. "yard" and "gate" each have a sign the player
    // faces that warps to the hall; the gate's sign has a line to read after the warp. "shrine" has an arrival event, run
    // every time, on a notice board (only while it is one) and another on a cell.
    private static ExplorationState Start(string mapId, string leaderId, WorldState world, Point2D? position = null)
    {
        var hall = new MapLayoutBuilder().WithLayer("111", "111").Build() with
        {
            Id = "hall",
            CellEvents = [new CellEvent(new GridCell(0, 0), _omersArrival), new CellEvent(new GridCell(1, 0), _sarxiansArrival)],
        };
        var warpToHall = new WarpStep("hall", new GridCell(2, 1));
        var yard = WithSign("yard", new MapEventBuilder().WithId("sign_0").WithSteps(warpToHall).Build());
        var gate = WithSign("gate", new MapEventBuilder().WithId("sign_0").WithSteps(warpToHall, _afterTheWarp).Build());
        var boardArrival = new MapEventBuilder().WithId("board_0").AsArrive().OnlyWhenObjectIs("notice_board").WithSteps(_boardSpeaks).Build();
        var floorArrival = new MapEventBuilder().WithId("cell_1_1").AsArrive().WithSteps(_floorSpeaks).Build();
        var shrine = WithSign("shrine", boardArrival) with { CellEvents = [new CellEvent(new GridCell(1, 1), floorArrival)] };
        var catalog = new ContentCatalogBuilder()
            .WithTiles(TestTiles.Walkable(1))
            .WithObjects(TestExplorationMap.NoticeBoard, TestExplorationMap.Chest)
            .WithMaps(hall, yard, gate, shrine)
            .Build();
        var context = new ExplorationContext(catalog, new Inventory(), world, new FixedSequenceRandom());
        return new ExplorationState(context, leaderId, new MapLocation(mapId, position ?? new Point2D(0.5, 1.5), Facing.Up));
    }

    private static MapLayout WithSign(string mapId, MapEvent readSign) =>
        new MapLayoutBuilder().WithLayer("111", "111").Build() with
        {
            Id = mapId,
            Objects = [new PlacedObject { Uid = "board@0,0", ObjectId = "notice_board", Position = new Point2D(0, 0), Z = MapLayers.PlayerZ, Events = [readSign] }],
        };
}
