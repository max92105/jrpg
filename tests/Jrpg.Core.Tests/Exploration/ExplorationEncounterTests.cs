using Jrpg.Core.Battle;
using Jrpg.Core.Dialogue;
using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Core.Items;
using Jrpg.Core.Maps;
using Jrpg.Core.Randomness;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;
using Jrpg.Core.World;

namespace Jrpg.Core.Tests.Exploration;

/// <summary>
/// "field" is a 6x1 floor with random battles (1 in 2 steps after 1 grace step), a sign cell at (5, 0) and a post at (3, 0) that talks when faced.
/// "house" is a 3x1 floor without battles whose cell (2, 0) warps to the field's (0, 0).
/// </summary>
public sealed class ExplorationEncounterTests
{
    // One cell at 3 tiles a second.
    private static readonly TimeSpan _oneStep = TimeSpan.FromSeconds(1.0 / 3);
    private static readonly MoveIntent _right = MoveIntent.FromAxes(1, 0);

    private static readonly EncounterDefinition _goblins = new() { Id = "goblin_trio", EnemyIds = ["goblin"], BackgroundFile = "Forest.png" };

    [Fact]
    public void Walk_EnterNewCellOnEncounterMap_SetsPendingEncounter()
    {
        var exploration = StartOn("field", new FixedSequenceRandom(0, 0));
        exploration.Walk(_right, _oneStep);

        exploration.Walk(_right, _oneStep);

        Assert.Equal(_goblins, exploration.PendingEncounter);
    }

    [Fact]
    public void Walk_RollMisses_LeavesNoEncounterPending()
    {
        var exploration = StartOn("field", new FixedSequenceRandom(1));
        exploration.Walk(_right, _oneStep);

        exploration.Walk(_right, _oneStep);

        Assert.Null(exploration.PendingEncounter);
    }

    [Fact]
    public void Walk_WithinSameCell_DoesNotRoll()
    {
        var exploration = StartOn("field", new FixedSequenceRandom());
        exploration.Walk(_right, _oneStep);

        exploration.Walk(MoveIntent.FromAxes(0.1, 0), TimeSpan.FromSeconds(0.1));

        Assert.Equal(new GridCell(1, 0), GridCell.Containing(exploration.Location.Position));
        Assert.Null(exploration.PendingEncounter);
    }

    [Fact]
    public void Walk_EnterCellWithTouchEvent_DoesNotRoll()
    {
        // The first step is a grace step and the next two roll misses; the step onto the sign cell (5, 0) starts the sign
        // and draws nothing.
        var exploration = StartOn("field", new FixedSequenceRandom(1, 1), new Point2D(1.5, 0.5));
        for (var step = 0; step < 4; step++)
        {
            exploration.Walk(_right, _oneStep);
        }

        Assert.Equal(new GridCell(5, 0), GridCell.Containing(exploration.Location.Position));
        Assert.True(exploration.IsEventRunning);
        Assert.Null(exploration.PendingEncounter);
    }

    [Fact]
    public void Walk_WhileEncounterPending_Throws()
    {
        var exploration = StartOn("field", new FixedSequenceRandom(0, 0));
        exploration.Walk(_right, _oneStep);
        exploration.Walk(_right, _oneStep);

        Assert.Throws<InvalidOperationException>(() => exploration.Walk(_right, _oneStep));
    }

    [Fact]
    public void TryInteract_WhileEncounterPending_Throws()
    {
        var exploration = StartOn("field", new FixedSequenceRandom(0, 0));
        exploration.Walk(_right, _oneStep);
        exploration.Walk(_right, _oneStep);

        Assert.Throws<InvalidOperationException>(() => exploration.TryInteract());
    }

    [Fact]
    public void FinishEncounter_NextStepsWithinGrace_DoNotRoll()
    {
        var exploration = StartOn("field", new FixedSequenceRandom(0, 0));
        exploration.Walk(_right, _oneStep);
        exploration.Walk(_right, _oneStep);

        exploration.FinishEncounter();
        exploration.Walk(_right, _oneStep);

        Assert.Null(exploration.PendingEncounter);
    }

    [Fact]
    public void FinishEncounter_FacingAnObject_SetsInteractionInFront()
    {
        var exploration = StartOn("field", new FixedSequenceRandom(0, 0));
        exploration.Walk(_right, _oneStep);
        exploration.Walk(_right, _oneStep);
        Assert.Null(exploration.InteractionInFront);

        exploration.FinishEncounter();

        Assert.Equal("post@3,0", exploration.InteractionInFront?.Placed.Uid);
    }

    [Fact]
    public void FinishEncounter_NonePending_Throws()
    {
        var exploration = StartOn("field", new FixedSequenceRandom());

        Assert.Throws<InvalidOperationException>(exploration.FinishEncounter);
    }

    [Fact]
    public void ContinueEvent_WarpToEncounterMap_ResetsSteps()
    {
        // The house step counts, so without the reset on map entry the first field step would be past the grace step and draw.
        var exploration = StartOn("house", new FixedSequenceRandom());
        exploration.Walk(_right, _oneStep);
        exploration.Walk(_right, _oneStep);
        exploration.ContinueEvent();
        Assert.Equal("field", exploration.Location.MapId);

        exploration.Walk(_right, _oneStep);

        Assert.Null(exploration.PendingEncounter);
    }

    private static ExplorationState StartOn(string mapId, IRandom random, Point2D? position = null)
    {
        var field = new MapLayoutBuilder().WithLayer("111111").Build() with
        {
            Id = "field",
            Objects = [new PlacedObject { Uid = "post@3,0", ObjectId = "post", Position = new Point2D(3, 0), Z = MapLayers.PlayerZ, Events = [new MapEventBuilder().WithId("post_0").WithSteps(new DialogueStep(new Speech(["A post."]))).Build()] }],
            CellEvents = [new CellEvent(new GridCell(5, 0), TouchEvent("cell_5_0", new DialogueStep(new Speech(["A sign."]))))],
        };
        var house = new MapLayoutBuilder().WithLayer("111").Build() with
        {
            Id = "house",
            CellEvents = [new CellEvent(new GridCell(2, 0), TouchEvent("cell_2_0", new WarpStep("field", new GridCell(0, 0))))],
        };
        var catalog = new ContentCatalogBuilder()
            .WithObjects(new ObjectDefinition { Id = "post", Width = 1, Height = 1, CollisionPolygon = [], Category = "test", Tiles = [], AbovePlayerZones = [] })
            .WithMaps(field, house)
            .WithRandomEncounters("field", new RandomEncounterTable { OneInSteps = 2, GraceSteps = 1, EncounterIds = ["goblin_trio"] })
            .WithEncounters(_goblins)
            .Build();
        var context = new ExplorationContext(catalog, new Inventory(), new WorldState(), random);
        return new ExplorationState(context, TestParties.LedBy("sarxian"), new MapLocation(mapId, position ?? new Point2D(0.5, 0.5), Facing.Right));
    }

    private static MapEvent TouchEvent(string id, EventStep step) =>
        new MapEventBuilder().WithId(id).AsTouch().WithSteps(step).Build();
}
