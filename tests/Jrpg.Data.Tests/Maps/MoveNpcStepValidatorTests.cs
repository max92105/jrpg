using Jrpg.Core.Dialogue;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Maps;

namespace Jrpg.Data.Tests.Maps;

/// <summary>
/// A <c>moveNpc</c> step's rules (ADR 0044) on a 3 x 3 yard: a guard with a field sprite at (1, 1) and a chest at (0,
/// 0); the guard's talk event holds the step under test.
/// </summary>
public sealed class MoveNpcStepValidatorTests
{
    private static readonly ObjectDefinition _chest = new()
    {
        Id = "chest",
        Width = 1,
        Height = 1,
        CollisionPolygon = [],
        Category = "test",
        Tiles = [],
        AbovePlayerZones = [],
    };

    private static readonly ObjectDefinition _guard = _chest with { Id = "guard", FieldSpriteFile = "guard.png" };

    private static readonly Dictionary<string, ObjectDefinition> _objects = new(StringComparer.Ordinal)
    {
        ["chest"] = _chest,
        ["guard"] = _guard,
    };

    private static readonly MapEvent _talk = new()
    {
        Id = "guard_1_0",
        Activation = EventActivation.Interact,
        Prompt = "Talk",
        Steps = [new DialogueStep(new Speech(["Halt."]))],
    };

    private static readonly MapLayout _yard = new()
    {
        Id = "yard",
        Width = 3,
        Height = 3,
        Layers = [],
        Objects =
        [
            Placed("chest_1", "chest", new Point2D(0, 0)),
            Placed("guard_1", "guard", new Point2D(1, 1)) with { Events = [_talk] },
        ],
    };

    private static readonly MapReferences _references = new(
        new MapContentReferences(
            TileIds: new HashSet<int>(),
            Objects: _objects,
            ItemIds: new HashSet<string>(),
            EncounterIds: new HashSet<string>(),
            MemberIds: new HashSet<string>()),
        Layouts: new Dictionary<string, MapLayout> { ["yard"] = _yard });

    public static TheoryData<MoveNpcStep, string> InvalidMoves => new()
    {
        { new MoveNpcStep("guard_1", []), "moveNpc path must hold 1 to 64 steps (was 0)." },
        { new MoveNpcStep("guard_1", Pacing(65)), "moveNpc path must hold 1 to 64 steps (was 65)." },
        { new MoveNpcStep("guard_2", [Facing.Left]), "moveNpc names 'guard_2', which isn't placed on this map." },
        { new MoveNpcStep("chest_1", [Facing.Right]), "moveNpc names 'chest_1' (chest), but only an NPC walks." },
        { new MoveNpcStep("guard_1", [Facing.Up, Facing.Up]), "moveNpc path step 1 leaves the map at (1, -1)." },
    };

    [Fact]
    public void FindProblem_PathOnTheMap_ReturnsNull()
    {
        var move = new MoveNpcStep("guard_1", [Facing.Left, Facing.Down]);

        Assert.Null(MoveNpcStepValidator.FindProblem(move, _yard, _objects));
    }

    // Objects on the way don't stop a cutscene walk (decision 575): the path may cross the chest's cell.
    [Fact]
    public void FindProblem_PathThroughAnObject_ReturnsNull()
    {
        var move = new MoveNpcStep("guard_1", [Facing.Left, Facing.Up]);

        Assert.Null(MoveNpcStepValidator.FindProblem(move, _yard, _objects));
    }

    // The bounds themselves are accepted: one step, and the longest path pacing left and right.
    [Theory]
    [InlineData(1)]
    [InlineData(MoveNpcStep.MaxSteps)]
    public void FindProblem_PathLengthAtBounds_ReturnsNull(int steps)
    {
        var move = new MoveNpcStep("guard_1", Pacing(steps));

        Assert.Null(MoveNpcStepValidator.FindProblem(move, _yard, _objects));
    }

    [Theory]
    [MemberData(nameof(InvalidMoves))]
    public void FindProblem_InvalidMove_NamesTheProblem(MoveNpcStep move, string expected)
    {
        Assert.Equal(expected, MoveNpcStepValidator.FindProblem(move, _yard, _objects));
    }

    [Fact]
    public void MapEventValidator_MoveOfUnknownUid_NamesTheEventAndStep()
    {
        var layout = WithGuardSteps(new DialogueStep(new Speech(["Halt."])), new MoveNpcStep("guard_2", [Facing.Left]));

        var problem = MapEventValidator.FindProblem(layout, _references);

        Assert.Equal("event 'guard_1_0': step 1: moveNpc names 'guard_2', which isn't placed on this map.", problem);
    }

    // The run keeps the map it started on: an NPC moved after the warp would walk on the map just left.
    [Fact]
    public void MapEventValidator_MoveAfterAWarp_NamesTheStep()
    {
        var layout = WithGuardSteps(new WarpStep("yard", new GridCell(0, 0)), new MoveNpcStep("guard_1", [Facing.Left]));

        var problem = MapEventValidator.FindProblem(layout, _references);

        Assert.Equal("event 'guard_1_0': step 1: moveNpc can't follow a warp: its map is left.", problem);
    }

    [Fact]
    public void MapEventValidator_MoveBeforeAWarp_ReturnsNull()
    {
        var layout = WithGuardSteps(new MoveNpcStep("guard_1", [Facing.Left]), new WarpStep("yard", new GridCell(0, 0)));

        Assert.Null(MapEventValidator.FindProblem(layout, _references));
    }

    // Left then right, so a path of any length stays on the map.
    private static Facing[] Pacing(int steps) =>
        [.. Enumerable.Range(0, steps).Select(index => index % 2 == 0 ? Facing.Left : Facing.Right)];

    private static PlacedObject Placed(string uid, string objectId, Point2D position) =>
        new() { Uid = uid, ObjectId = objectId, Position = position, Z = MapLayers.PlayerZ, Events = [] };

    private static MapLayout WithGuardSteps(params EventStep[] steps) =>
        _yard with { Objects = [_yard.Objects[0], _yard.Objects[1] with { Events = [_talk with { Steps = steps }] }] };
}
