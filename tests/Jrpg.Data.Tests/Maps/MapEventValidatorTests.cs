using Jrpg.Core.Dialogue;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Maps;

namespace Jrpg.Data.Tests.Maps;

public sealed class MapEventValidatorTests
{
    private static readonly ObjectDefinition _chestDefinition = new() { Id = "chest", Width = 1, Height = 1, CollisionPolygon = [], Category = "test", Tiles = [], AbovePlayerZones = [] };

    private static readonly MapLayout _cave = new()
    {
        Id = "cave",
        Width = 3,
        Height = 2,
        Layers = [],
        Objects = [],
    };

    private static readonly MapReferences _references = new(
        new MapContentReferences(
        TileIds: new HashSet<int>(),
        Objects: new Dictionary<string, ObjectDefinition> { ["chest"] = _chestDefinition, ["chest_opened"] = _chestDefinition with { Id = "chest_opened" } },
        ItemIds: new HashSet<string> { "potion" },
        EncounterIds: new HashSet<string>()),
        Layouts: new Dictionary<string, MapLayout> { ["cave"] = _cave });

    private static readonly MapEvent _openChest = new()
    {
        Id = "chest@1,1_0",
        Activation = EventActivation.Interact,
        Prompt = "Open",
        IsOnce = true,
        Steps = [new DialogueStep(new Speech(["A potion!"])), new GiveItemStep("potion", 1), new SwapObjectStep("chest_opened")],
    };

    private static readonly MapEvent _warpToCave = new()
    {
        Id = "cell_0_0",
        Activation = EventActivation.Touch,
        Steps = [new WarpStep("cave", new GridCell(2, 1))],
    };

    private static readonly MapLayout _forest = new()
    {
        Id = "forest",
        Width = 2,
        Height = 2,
        Layers = [],
        Objects = [new PlacedObject { Uid = "chest@1,1", ObjectId = "chest", Position = new Point2D(1, 1), Z = MapLayers.PlayerZ, Events = [_openChest] }],
        CellEvents = [new CellEvent(new GridCell(0, 0), _warpToCave)],
    };

    public static TheoryData<MapLayout, string> InvalidLayouts => new()
    {
        { WithChestEvent(_openChest with { Prompt = null }), "event 'chest@1,1_0': an interact event needs a prompt." },
        { WithChestEvent(_openChest with { Steps = [] }), "event 'chest@1,1_0': steps must not be empty." },
        { WithChestEvent(_openChest with { Steps = [new DialogueStep(new Speech([]))] }), "event 'chest@1,1_0': step 0: dialogue needs at least one line." },
        { WithChestEvent(_openChest with { Steps = [new GiveItemStep("potion", 0)] }), "event 'chest@1,1_0': step 0: giveItem quantity must be at least 1 (was 0)." },
        { WithChestEvent(_openChest with { Steps = [new SwapObjectStep("chest_gold")] }), "event 'chest@1,1_0': step 0: swapObject has unknown object id 'chest_gold'." },
        { WithChestEvent(_openChest with { OnlyWhenObjectId = "door" }), "event 'chest@1,1_0': onlyWhenObjectId has unknown object id 'door'." },
        { WithChestEvent(_openChest with { Steps = [new WarpStep("castle", new GridCell(0, 0))] }), "event 'chest@1,1_0': step 0: warp has unknown map id 'castle'." },
        { WithChestEvent(_openChest with { Steps = [null!] }), "event 'chest@1,1_0': steps holds a null entry." },
        { WithChestEvent(_openChest with { Steps = [new DialogueStep(new Speech(["Hi", null!]))] }), "event 'chest@1,1_0': a dialogue's lines hold a null entry." },
        { WithChestEvent(null!), "object 'chest@1,1': events holds a null entry." },
        { _forest with { CellEvents = [null!] }, "cellEvents holds a null entry." },
        { _forest with { CellEvents = [new CellEvent(new GridCell(2, 0), _warpToCave)] }, "cell event 'cell_0_0' at (2, 0): the cell is outside the map." },
    };

    [Fact]
    public void FindProblem_ValidEvents_ReturnsNull()
    {
        var problem = MapEventValidator.FindProblem(_forest, _references);

        Assert.Null(problem);
    }

    [Fact]
    public void FindProblem_GiveItemUnknownItem_NamesTheEventAndStep()
    {
        var layout = WithChestEvent(_openChest with { Steps = [new DialogueStep(new Speech(["Look!"])), new GiveItemStep("megalixir", 1)] });

        var problem = MapEventValidator.FindProblem(layout, _references);

        Assert.Equal("event 'chest@1,1_0': step 1: giveItem has unknown item id 'megalixir'.", problem);
    }

    [Fact]
    public void FindProblem_WarpCellOutsideTargetMap_NamesTheEvent()
    {
        var layout = _forest with { CellEvents = [new CellEvent(new GridCell(0, 0), _warpToCave with { Steps = [new WarpStep("cave", new GridCell(3, 0))] })] };

        var problem = MapEventValidator.FindProblem(layout, _references);

        Assert.Equal("cell event 'cell_0_0' at (0, 0): step 0: warp cell (3, 0) is outside map 'cave'.", problem);
    }

    [Fact]
    public void FindProblem_DuplicateEventId_NamesTheId()
    {
        var layout = _forest with { CellEvents = [new CellEvent(new GridCell(0, 0), _warpToCave with { Id = "chest@1,1_0" })] };

        var problem = MapEventValidator.FindProblem(layout, _references);

        Assert.Equal("event id 'chest@1,1_0' is used more than once.", problem);
    }

    [Fact]
    public void FindProblem_SwapStepInCellEvent_NamesTheCell()
    {
        var layout = _forest with { CellEvents = [new CellEvent(new GridCell(1, 0), _warpToCave with { Steps = [new SwapObjectStep("chest_opened")] })] };

        var problem = MapEventValidator.FindProblem(layout, _references);

        Assert.Equal("cell event 'cell_0_0' at (1, 0): swapObject only works in an object's event.", problem);
    }

    [Fact]
    public void FindProblem_InteractCellEvent_NamesTheCell()
    {
        var layout = _forest with { CellEvents = [new CellEvent(new GridCell(1, 0), _warpToCave with { Activation = EventActivation.Interact, Prompt = "Enter" })] };

        var problem = MapEventValidator.FindProblem(layout, _references);

        Assert.Equal("cell event 'cell_0_0' at (1, 0): a cell event must have touch activation.", problem);
    }

    [Fact]
    public void FindProblem_InteractEventWithoutPrompt_NamesTheEvent()
    {
        var problem = MapEventValidator.FindProblem(WithChestEvent(_openChest with { Prompt = " " }), _references);

        Assert.Equal("event 'chest@1,1_0': an interact event needs a prompt.", problem);
    }

    [Theory]
    [MemberData(nameof(InvalidLayouts))]
    public void FindProblem_InvalidEvent_NamesTheProblem(MapLayout layout, string expectedProblem)
    {
        var problem = MapEventValidator.FindProblem(layout, _references);

        Assert.Equal(expectedProblem, problem);
    }

    private static MapLayout WithChestEvent(MapEvent chestEvent) =>
        _forest with { Objects = [_forest.Objects[0] with { Events = [chestEvent] }] };
}
